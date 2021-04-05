using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using ServerCore;

namespace DummyClient
{
    class Packet
    {
        public ushort size;
        public ushort packetid;
    }

    class GameSession : Session
    {
        public override void OnConnected(EndPoint endPoint)
        {
            Console.WriteLine($"OnCennected {endPoint}");

            Packet packet = new Packet() { size = 4, packetid = 7 };

            // 보낸다
            for (int i = 0; i < 5; ++i)
            {    

                ArraySegment<byte> openSegment = SendBufferHelper.Open(4096);
                byte[] Buffer = BitConverter.GetBytes(packet.size); //Encoding.UTF8.GetBytes("Welcome to MMORPG Server !");
                byte[] Buffer2 = BitConverter.GetBytes(packet.packetid);//Encoding.UTF8.GetBytes("Hello Server !");

                Array.Copy(Buffer, 0, openSegment.Array, openSegment.Offset, Buffer.Length);
                Array.Copy(Buffer2, 0, openSegment.Array, openSegment.Offset + Buffer.Length, Buffer2.Length);

                ArraySegment<byte> sendBuff = SendBufferHelper.Close(packet.size);

                Send(sendBuff);
            }
        }

        public override int OnRecv(ArraySegment<byte> _buffer)
        {
            string recvData = Encoding.UTF8.GetString(_buffer.Array, _buffer.Offset, _buffer.Count);
            Console.WriteLine($"[From Server] {recvData}");

            return _buffer.Count;
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

    class Program
    {
        static void Main(string[] args)
        {
            // DNS 서버를 가지고 옵니다.
            string host = Dns.GetHostName();
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipAddr = ipHost.AddressList[0];
            IPEndPoint endpoint = new IPEndPoint(ipAddr, 7777);

            Connector connector = new Connector();
            connector.Connect(endpoint, () => { return new GameSession(); });

            while(true)
            {

                try
                {

                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }

                Thread.Sleep(1000);
            }

        }
    }
}
