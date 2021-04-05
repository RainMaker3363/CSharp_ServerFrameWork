using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace ServerCore
{
    #region SpinLock

    class cSpinLock
    {
        volatile int _locked = 0;

        public void Acquire()
        {
            while (true)
            {
                //int original = Interlocked.Exchange(ref _locked, 1);
                //if (original == 0)
                //    break;

                // CAS (Compare-And-Swap)
                int expected = 0;
                int desired = 1;
                int original = Interlocked.CompareExchange(ref _locked, desired, expected);
                if (original == expected)
                    break;

                // 다른 스레드에게 스케쥴링을 양보한다!
                Thread.Yield();
            }

        }

        public void Release()
        {
            _locked = 0;
        }
    }

    #endregion

    #region Event

    class CLock
    {
        AutoResetEvent _available = new AutoResetEvent(true);

        public void Acquire()
        {
            _available.WaitOne();   // 입장 시도!
        }

        public void Release()
        {
            _available.Set();
        }
    }

    #endregion

    // 재귀적 락을 허용할지 (Yes)
    // 스핀락 정책 (5000번 -> Yield)
    class MyCustomLock
    {
        const int EMPTY_FLAG = 0x00000000;
        const int WRITE_MASK = 0x7FFF0000;
        const int READ_MASK = 0x0000FFFF;
        const int MAX_SPIN_COUNT = 5000;

        // [unused(1)] [WriteThreadID(15)] [ReadCount(16)]
        int _flag = EMPTY_FLAG;
        int _writeCount = 0;

        public void WriteLock()
        {
            // 동일 스레드가 WriteLock을 이미 획득하고 있는지 확인
            int lockThread = ((_flag & WRITE_MASK) >> 16);
            if(Thread.CurrentThread.ManagedThreadId == lockThread)
            {
                _writeCount++;
                return; 
            }

            int desired = ((Thread.CurrentThread.ManagedThreadId << 16) & WRITE_MASK);

            for(int i = 0; i < MAX_SPIN_COUNT; ++i)
            {
                // 시도를 성공하면 return
                if(Interlocked.CompareExchange(ref _flag, desired, EMPTY_FLAG) == EMPTY_FLAG)
                {
                    _writeCount = 1;
                    return;
                }
            }

            Thread.Yield();
        }

        public void WriteUnLock()
        {
            int lockCount = --_writeCount;
            if(lockCount <= 0)
                Interlocked.Exchange(ref _flag, EMPTY_FLAG);
        }

        public void ReadLock()
        {
            // 동일 스레드가 WriteLock을 이미 획득하고 있는지 확인
            int lockThread = ((_flag & WRITE_MASK) >> 16);
            if (Thread.CurrentThread.ManagedThreadId == lockThread)
            {
                Interlocked.Increment(ref _flag);
                return;
            }

            // 아무도 WriteLock을 획득하고 있지 않으면, ReadCount를 1 늘린다.
            while (true)
            {
                for (int i = 0; i < MAX_SPIN_COUNT; ++i)
                {
                    int expected = (_flag & READ_MASK);
                    if (Interlocked.CompareExchange(ref _flag, expected + 1, expected) == expected)
                    {
                        return;
                    }
                }

                Thread.Yield();
            }
        }

        public void ReadUnLock()
        {
            Interlocked.Decrement(ref _flag);
        }
    }
}
