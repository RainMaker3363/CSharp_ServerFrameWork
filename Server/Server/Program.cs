using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ServerCore;

namespace Server
{
    class Packet
    {
        public ushort size;
        public ushort packetid;
    }

    class GameSession : PacketSession
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
            ushort size = BitConverter.ToUInt16(_buffer.Array, _buffer.Offset);
            ushort id = BitConverter.ToUInt16(_buffer.Array, _buffer.Offset + sizeof(ushort));

            Console.WriteLine($"Recv Packet ID : {id}, Size {size}");
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
        static Listener _listener = new Listener();

        static void Main(string[] args)
        {
            // DNS 서버를 가지고 옵니다.
            string host = Dns.GetHostName();
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipAddr = ipHost.AddressList[0];
            IPEndPoint endpoint = new IPEndPoint(ipAddr, 7777);

            _listener.Init(endpoint, () => { return new GameSession(); });
            Console.WriteLine($"Listening...");

            try
            {
                while (true)
                {

                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
