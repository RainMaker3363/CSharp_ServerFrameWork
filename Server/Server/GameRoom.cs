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


        public void BroadCast(ClientSession session, string chat)
        {
            S_Chat packet = new S_Chat();
            packet.playerid = session.SessionId;
            packet.chat = $"{chat}, I am {packet.playerid}";

            ArraySegment<byte> segment = packet.Write();

            _pendingList.Add(segment);

        }

        public void Enter(ClientSession session)
        {
            _sessions.Add(session);

            session.Room = this;
        }

        public void Leave(ClientSession session)
        {
            _sessions.Remove(session);
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
