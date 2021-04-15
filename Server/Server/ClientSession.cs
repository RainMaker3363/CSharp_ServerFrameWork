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
        public override void OnConnected(EndPoint endPoint)
        {
            Console.WriteLine($"OnCennected {endPoint}");

            //Packet packet = new Packet() { size = 100, packetid = 10 };

            //ArraySegment<byte> openSegment = SendBufferHelper.Open(4096);
            //byte[] Buffer = BitConverter.GetBytes(packet.size); //Encoding.UTF8.GetBytes("Welcome to MMORPG Server !");
            //byte[] Buffer2 = BitConverter.GetBytes(packet.packetid);//Encoding.UTF8.GetBytes("Hello Server !");

            //Array.Copy(Buffer, 0, openSegment.Array, openSegment.Offset, Buffer.Length);
            //Array.Copy(Buffer2, 0, openSegment.Array, openSegment.Offset + Buffer.Length, Buffer2.Length);

            //ArraySegment<byte> sendBuff = SendBufferHelper.Close(Buffer.Length + Buffer2.Length);

            //Send(sendBuff);

            Thread.Sleep(5000);

            Disconnect();
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
            Console.WriteLine($"OnDisconnected {endPoint}");
        }
    }
}
