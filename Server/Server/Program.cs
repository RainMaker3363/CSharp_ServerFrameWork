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

        static void Main(string[] args)
        {
            // 사용할 패킷들을 저장합니다.
            PacketManager.Instance.Register();

            // DNS 서버를 가지고 옵니다.
            string host = Dns.GetHostName();
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipAddr = ipHost.AddressList[0];
            IPEndPoint endpoint = new IPEndPoint(ipAddr, 7777);

            _listener.Init(endpoint, () => { return new ClientSession(); });
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
