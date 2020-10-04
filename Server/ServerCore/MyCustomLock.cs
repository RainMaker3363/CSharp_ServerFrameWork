using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace ServerCore
{
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
            int _lockThreadId = (_flag & WRITE_MASK) >> 16;
            if(Thread.CurrentThread.ManagedThreadId == _lockThreadId)
            {
                ++_writeCount;
                return;
            }

            // 아무도 WriteLock or ReadLock을 획득하고 있지 않을 때, 경합해서 소유권을 얻는다
            int desired = (Thread.CurrentThread.ManagedThreadId << 16) & WRITE_MASK;

            while(true)
            {
                for(int i = 0; i<MAX_SPIN_COUNT; ++i)
                {
                    // 원자성 보장을 위해 InterLock으로 체크합니다.
                    // 반환 값은 바꾸기 이전 값!
                    if (Interlocked.CompareExchange(ref _flag, desired, EMPTY_FLAG) == EMPTY_FLAG)
                    {
                        _writeCount = 1;
                        return;
                    }
                }

                Thread.Yield();
            }
        }

        public void WriteUnLock()
        {
            int lockCount = --_writeCount;
            if(lockCount == 0)
                Interlocked.Exchange(ref _flag, EMPTY_FLAG);
        }

        public void ReadLock()
        {
            int _lockThreadId = (_flag & WRITE_MASK) >> 16;
            if (Thread.CurrentThread.ManagedThreadId == _lockThreadId)
            {
                Interlocked.Increment(ref _flag);
                return;
            }

            // 아무도 WriteLock을 획득하지 않았다면 ReadCount를 늘린다.
            while (true)
            {
                for (int i = 0; i < MAX_SPIN_COUNT; ++i)
                {
                    int expected = (_flag & READ_MASK);
                    if (Interlocked.CompareExchange(ref _flag, expected + 1, expected) == expected)
                        return;
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
