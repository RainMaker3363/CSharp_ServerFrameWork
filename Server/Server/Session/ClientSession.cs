using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections;
using ServerCore;

namespace Server
{
    class ClientSession : PacketSession
    {
        public int SessionId { get; set; }
        public GameRoom Room { get; set; }

        public override void OnConnected(EndPoint endPoint)
        {
            Console.WriteLine($"OnCennected {endPoint}");

            Program.Room.Push(() =>
            {
                Program.Room.Enter(this);
            });
        }

        public override void OnRecvPacket(ArraySegment<byte> _buffer)
        {
            PacketManager.Instance.OnRecvPacket(this, _buffer);
        }

        public override void OnSend(int numOfByte)
        {
            Console.WriteLine($"Transferred Bytes : {numOfByte}");
        }

        public override void OnDisconnected(EndPoint endPoint)
        {
            SessionManager.Instance.Remove(this);
            if(Room != null)
            {
                GameRoom room = Room;
                room.Push(() =>
                {
                    room.Leave(this);
                });
                Room = null;
            }

            Console.WriteLine($"OnDisconnected {endPoint}");
        }
    }
}
