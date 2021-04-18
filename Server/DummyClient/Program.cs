using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using ServerCore;

namespace DummyClient
{
  
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
            connector.Connect(endpoint,
                () => { return SessionManager.Instance.Generate(); },
                500);

            while(true)
            {

                try
                {
                    SessionManager.Instance.SendForEach();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }

                Thread.Sleep(250);
            }

        }
    }
}
