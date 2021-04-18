using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Collections.Generic;
using System.Collections;
using ServerCore;

namespace DummyClient
{

    class ServerSession : PacketSession
    {
        public override void OnConnected(EndPoint endPoint)
        {
            Console.WriteLine($"OnCennected {endPoint}");
        }

        public override void OnRecvPacket(ArraySegment<byte> _buffer)
        {
            //string recvData = Encoding.UTF8.GetString(_buffer.Array, _buffer.Offset, _buffer.Count);
            //Console.WriteLine($"[From Server] {recvData}");

            //return _buffer.Count;

            PacketManager.Instance.OnRecvPacket(this, _buffer);
        }

        public override void OnSend(int numOfByte)
        {
            //Console.WriteLine($"Transferred Bytes : {numOfByte}");
        }

        public override void OnDisconnected(EndPoint endPoint)
        {
            Console.WriteLine($"OnDisconnected {endPoint}");
        }

    }
}
