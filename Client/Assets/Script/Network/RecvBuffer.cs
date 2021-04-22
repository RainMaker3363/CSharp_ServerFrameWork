using System;
using System.Collections.Generic;
using System.Text;

namespace ServerCore
{
    public class RecvBuffer
    {
        ArraySegment<byte> _buffer;
        int _ReadPos;
        int _WritePos;

        public RecvBuffer(int bufferSize)
        {
            _buffer = new ArraySegment<byte>(new byte[bufferSize], 0, bufferSize);
        }

        public int DataSize { get { return _WritePos - _ReadPos; } }
        public int FreeSize { get { return _buffer.Count - _WritePos; } }

        public ArraySegment<byte> ReadSegment
        {
            get
            {
                return new ArraySegment<byte>(_buffer.Array, _buffer.Offset + _ReadPos, DataSize);
            }
        }

        public ArraySegment<byte> WriteSegment
        {
            get
            {
                return new ArraySegment<byte>(_buffer.Array, _buffer.Offset + _WritePos, FreeSize);
            }
        }

        public void Clean()
        {
            int dataSize = DataSize;
            if(dataSize <= 0)
            {
                // 남은 데이터가 없으면 커서만 옮겨줌!
                _ReadPos = _WritePos = 0;
            }
            else
            {
                // 남은 데이터가 있으면 시작 위치로 복사!
                Array.Copy(_buffer.Array, _buffer.Offset + _ReadPos, _buffer.Array, _buffer.Offset, dataSize);
                _ReadPos = 0;
                _WritePos = dataSize;
            }
        }

        public bool OnRead(int numofBytes)
        {
            if (numofBytes > DataSize)
                return false;

            _ReadPos += numofBytes;

            return true;
        }

        public bool OnWrite(int numofBytes)
        {
            if (numofBytes > FreeSize)
                return false;

            _WritePos += numofBytes;

            return true;
        }

    }
}
