using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ServerCore;

namespace Server
{


    class Program
    {
        static Listener _listener = new Listener();
        public static GameRoom Room = new GameRoom();

        static void FlushRoom()
        {
            Room.Push(() =>
            {
                Room.Flush();
            });
            JobTimer.Instance.Push(FlushRoom, 250);
        }

        static void Main(string[] args)
        {
            // DNS 서버를 가지고 옵니다.
            string host = Dns.GetHostName();
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipAddr = ipHost.AddressList[0];
            IPEndPoint endpoint = new IPEndPoint(ipAddr, 7777);

            _listener.Init(endpoint, () => { return SessionManager.Instance.Generate(); });
            Console.WriteLine($"Listening...");


            FlushRoom();

            try
            {
                while (true)
                {
                    JobTimer.Instance.Flush();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
