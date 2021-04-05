using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Collections.Generic;

namespace ServerCore
{
    public class Listener
    {
        Socket _ListenSocket;
        Func<Session> _SessionFactory;

        public void Init(IPEndPoint endPoint, Func<Session> sessionFact)
        {
            _ListenSocket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            _SessionFactory += sessionFact;

            _ListenSocket.Bind(endPoint);

            // backlog : 최대 대기수!
            _ListenSocket.Listen(10);

            SocketAsyncEventArgs args = new SocketAsyncEventArgs();
            args.Completed += new EventHandler<SocketAsyncEventArgs>(OnAcceptCompleted);
            RegisterAccept(args);
        }

        void RegisterAccept(SocketAsyncEventArgs args)
        {
            // 잔여 데이터를 우선 초기화  해줍니다.
            args.AcceptSocket = null;

            bool pending = _ListenSocket.AcceptAsync(args);
            // 즉시 완료 되었을 경우
            if (pending == false)
            {
                OnAcceptCompleted(null, args);
            }
        }

        void OnAcceptCompleted(object sender, SocketAsyncEventArgs args)
        {
            if (args.SocketError == SocketError.Success)
            {
                Session session = _SessionFactory.Invoke();
                session.Start(args.AcceptSocket);
                session.OnConnected(args.AcceptSocket.RemoteEndPoint);

                //_onAcceptHandler.Invoke(args.AcceptSocket);
            }
            else
                Console.WriteLine(args.SocketError.ToString());

            // 다시 한번 요청을 진행한다
            RegisterAccept(args);
        }

        public Socket Accept()
        {
            return _ListenSocket.Accept();
        }
    }
}
