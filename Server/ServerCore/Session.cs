using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ServerCore
{
    public abstract class PacketSession : Session
    {
        public static readonly int HeaderSize = 2;

        public sealed override int OnRecv(ArraySegment<byte> _buffer)
        {
            int processLen = 0;
            int packetCount = 0;

            while(true)
            {
                // 최초 2바이트는 패킷 사이즈다
                if (_buffer.Count < HeaderSize)
                    break;

                // 패킷이 완전체로 도착했는지 확인
                ushort dataSize = BitConverter.ToUInt16(_buffer.Array, _buffer.Offset);
                if(_buffer.Count < dataSize)
                    break;

                // 여기가지 왔으면 패킷 조립
                OnRecvPacket(new ArraySegment<byte>(_buffer.Array, _buffer.Offset, dataSize));
                packetCount++;

                processLen += dataSize;
                _buffer = new ArraySegment<byte>(_buffer.Array, _buffer.Offset + dataSize, _buffer.Count - dataSize);
            }

            if (packetCount > 1)
                Console.WriteLine($"패킷 모아 보내기 : {packetCount}");

            return processLen;
        }

        public abstract void OnRecvPacket(ArraySegment<byte> _buffer);
    }

    public abstract class Session
    {
        Socket _socket;
        int _disconnect = 0;

        // Recv 버퍼
        RecvBuffer _recvBuffer = new RecvBuffer(65535);

        object _lock = new object();
        Queue<ArraySegment<byte>> _sendQueue = new Queue<ArraySegment<byte>>();

        // Send 부하를 줄이기 위해 미리 만들어둡니다.
        List<ArraySegment<byte>> pendingList = new List<ArraySegment<byte>>();

        SocketAsyncEventArgs sendArgs = new SocketAsyncEventArgs();
        SocketAsyncEventArgs recvArgs = new SocketAsyncEventArgs();

        public abstract void OnConnected(EndPoint endPoint);
        public abstract int OnRecv(ArraySegment<byte> _buffer);
        public abstract void OnSend(int numOfByte);
        public abstract void OnDisconnected(EndPoint endPoint);

        void Clear()
        {
            lock(_lock)
            {
                _sendQueue.Clear();
                pendingList.Clear();
            }
        }


        public void Start(Socket socket)
        {
            _socket = socket;

            recvArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnRecvCompleted);
            sendArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnSendCompleted);

            RegisterRecv();
        }

        public void Send(List<ArraySegment<byte>> sendBuffList)
        {
            if (sendBuffList.Count <= 0)
                return;

            lock (_lock)
            {
                foreach(ArraySegment<byte> sendBuff in sendBuffList)
                {
                    _sendQueue.Enqueue(sendBuff);
                }
                
                if (pendingList.Count <= 0)
                {
                    RegisterSend();
                }
            }
        }

        public void Send(ArraySegment<byte> sendBuff)
        {
            lock(_lock)
            {
                _sendQueue.Enqueue(sendBuff);
                if (pendingList.Count <= 0)
                {
                    RegisterSend();
                }
            }
        }

        public void Disconnect()
        {
            if (Interlocked.Exchange(ref _disconnect, 1) == 1)
                return;

            OnDisconnected(_socket.RemoteEndPoint);

            _socket.Shutdown(SocketShutdown.Both);
            _socket.Close();

            Clear();
        }

        #region 네트워크 통신

        void RegisterSend()
        {
            if (_disconnect == 1)
                return;

            pendingList.Clear();
            while (_sendQueue.Count > 0)
            {
                ArraySegment<byte> buffer = _sendQueue.Dequeue();
                //sendArgs.SetBuffer(buffer, 0, buffer.Length);

                // 패킷을 리스트에 모아서 한번에 보내줍니다!
                pendingList.Add(buffer);
            }

            sendArgs.BufferList = pendingList;

            try
            {
                bool pending = _socket.SendAsync(sendArgs);
                if (pending == false)
                    OnSendCompleted(null, sendArgs);
            }
            catch(Exception e)
            {
                Console.WriteLine($"RegisterSend Failed : {e.Message}");
            }
        }

        void OnSendCompleted(object sender, SocketAsyncEventArgs args)
        {
            lock(_lock)
            {
                if (args.BytesTransferred > 0 &&
                    args.SocketError == SocketError.Success)
                {
                    try
                    {
                        sendArgs.BufferList = null;
                        pendingList.Clear();

                        // Send 결과물을 반영해줍니다!
                        OnSend(sendArgs.BytesTransferred);

                        if(_sendQueue.Count > 0)
                        {
                            RegisterSend();
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"OnSendCompleted Failed {e}");
                    }
                }
                else
                {
                    Disconnect();
                }
            }

        }



        void RegisterRecv()
        {
            if (_disconnect == 1)
                return;

            _recvBuffer.Clean();
            ArraySegment<byte> segment = _recvBuffer.WriteSegment;

            recvArgs.SetBuffer(segment.Array, segment.Offset, segment.Count);

            try
            {
                bool pending = _socket.ReceiveAsync(recvArgs);
                if (pending == false)
                {
                    OnRecvCompleted(null, recvArgs);
                }
            }
            catch(Exception e)
            {
                Console.WriteLine($"RegisterRecv Failed {e}");
            }

        }

        void OnRecvCompleted(object sender, SocketAsyncEventArgs args)
        {
            if(args.BytesTransferred >0 &&
                args.SocketError == SocketError.Success)
            {
                try
                {
                    // Write 커서 이동
                    if(_recvBuffer.OnWrite(args.BytesTransferred) == false)
                    {
                        Disconnect();
                        return;
                    }

                    // Receive 결과를 반영해줍니다.
                    // 인터페이스를 위함!

                    // 컨텐츠 쪽으로 데이터를 넘겨주고 얼마나 처리했는지 받는다
                    int processLen = OnRecv(_recvBuffer.ReadSegment);
                    if(processLen < 0 || _recvBuffer.DataSize < processLen)
                    {
                        Disconnect();
                        return;
                    }


                    // Read커서 이동
                    if(_recvBuffer.OnRead(processLen) == false)
                    {
                        Disconnect();
                        return;
                    }

                    RegisterRecv();
                }
                catch(Exception e)
                {
                    Console.WriteLine($"OnRecvCompleted Failed {e}");
                }
                
            }
            else
            {
                // Disconnect
                //Console.WriteLine("Disconnect!");
                Disconnect();
            }

        }

        #endregion
    }
}
