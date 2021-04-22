using System;
using System.Collections.Generic;
using System.Text;
using ServerCore;

namespace Server
{
    class GameRoom : IJobQueue
    {
        List<ClientSession> _sessions = new List<ClientSession>();
        JobQueue _jobQueue = new JobQueue();
        List<ArraySegment<byte>> _pendingList = new List<ArraySegment<byte>>();


        public void BroadCast(ArraySegment<byte> segment)
        {
            _pendingList.Add(segment);
        }

        public void Enter(ClientSession session)
        {
            // 플레이어 추가

            _sessions.Add(session);
            session.Room = this;

            // 신규 플레이어에게 모든 플레이어 목록을 전송합니다.
            S_PlayerList players = new S_PlayerList();
            foreach(ClientSession s in _sessions)
            {
                players.players.Add(new S_PlayerList.Player()
                {
                    isSelf = (s == session),
                    playerId = s.SessionId,
                    posX = s.PosX,
                    posY = s.PosY,
                    posZ = s.PosZ,
                });
            }
            session.Send(players.Write());


            // 모든 플레이어 목록을 전송합니다.
            S_BroadcastEnterGame enterGame = new S_BroadcastEnterGame();
            enterGame.playerId = session.SessionId;
            enterGame.posX = 0;
            enterGame.posY = 0;
            enterGame.posZ = 0;

            BroadCast(enterGame.Write());
        }

        public void Leave(ClientSession session)
        {
            _sessions.Remove(session);

            S_BroadCastLeaveGame leave = new S_BroadCastLeaveGame();
            leave.playerId = session.SessionId;
            BroadCast(leave.Write());
        }

        public void Move(ClientSession session, C_Move packet)
        {
            // 좌표 바꿔주기
            session.PosX = packet.posX;
            session.PosY = packet.posY;
            session.PosZ = packet.posZ;

            S_BroadCastMove move = new S_BroadCastMove();
            move.playerId = session.SessionId;
            move.posX = session.PosX;
            move.posY = session.PosY;
            move.posZ = session.PosZ;

            BroadCast(move.Write());
        }

        public void Push(Action job)
        {
            _jobQueue.Push(job);
        }

        public void Flush()
        {
            foreach (ClientSession s in _sessions)
            {
                s.Send(_pendingList);
            }

            Console.WriteLine($"Flushed {_pendingList.Count} items");
            _pendingList.Clear();
        }
    }
}
