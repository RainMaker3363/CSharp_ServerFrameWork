using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Text;

namespace ServerCore
{
    public class Connector
    {
        Func<Session> _SessionFactory;

        public void Connect(IPEndPoint endPoint, Func<Session> sessionFactory, int connectCount = 1)
        {
            for(int i = 0; i< connectCount; ++i)
            {
                Socket socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                _SessionFactory = sessionFactory;

                SocketAsyncEventArgs args = new SocketAsyncEventArgs();
                args.Completed += OnConnectComplete;
                args.RemoteEndPoint = endPoint;
                args.UserToken = socket;

                RegisterConnect(args);
            }

        }

        void RegisterConnect(SocketAsyncEventArgs args)
        {
            Socket socket = args.UserToken as Socket;
            if (socket == null)
                return;

            bool pending = socket.ConnectAsync(args);
            if (pending == false)
                OnConnectComplete(null, args);
        }

        void OnConnectComplete(object obj, SocketAsyncEventArgs args)
        {
            if(args.SocketError == SocketError.Success)
            {
                Session session = _SessionFactory.Invoke();
                session.Start(args.ConnectSocket);
                session.OnConnected(args.RemoteEndPoint);
            }
            else
            {
                Console.WriteLine($"OnConnectComplete Fail : {args.SocketError}");
            }
        }
    }
}
