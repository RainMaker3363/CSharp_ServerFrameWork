using System;
using System.Threading;
using System.Threading.Tasks;

namespace ServerCore
{
    #region SpinLock

    class CustomSpinLock
    {
        volatile int _locked = 0;

        public void Acquire()
        {
            while(true)
            {
                //int Original = Interlocked.Exchange(ref _locked, 1);
                //if (Original == 0)
                //    break;
                int expected = 0;
                int desired = 1;

                // CAS Compared and Swap
                int Original = Interlocked.CompareExchange(ref _locked, desired, expected);
                if (Original == 0)
                    break;

                // 쉬다 올게! (컨텐스트 스위칭!)

            }
            
        }

        public void Release()
        {
            _locked = 0;
        }
    }

    #endregion

    #region 커널 Lock

    class Lock
    {
        // bool <- 커널
        AutoResetEvent _available = new AutoResetEvent(true);
        //ManualResetEvent _available = new ManualResetEvent(true);

        public void Acquire()
        {
            // 입장 시도
            _available.WaitOne();
            
            // 문을 닫는다
            //_available.Reset();
        }

        public void Release()
        {
            _available.Set();
        }
    }
    #endregion

    class Program
    {
        static int _num = 0;
        //static Mutex _lock = new Mutex();
        static SpinLock _lock2 = new SpinLock();

        // 상호 베타
        // Monitor
        static object _lock = new object();

        static ReaderWriterLockSlim _lock3 = new ReaderWriterLockSlim();
        static MyCustomLock _lock4 = new MyCustomLock();

        static ThreadLocal<string> _threadName = new ThreadLocal<string>();

        static void Thread_1()
        {
            for(int i = 0; i<10000; ++i)
            {
                _lock4.WriteLock();
                ++_num;
                _lock4.WriteUnLock();
            }
        }

        static void Thread_2()
        {
            for (int i = 0; i < 10000; ++i)
            {
                _lock4.WriteLock();
                --_num;
                _lock4.WriteUnLock();
            }
        }

        static void Main(string[] args)
        {
            Task t1 = new Task(Thread_1);
            Task t2 = new Task(Thread_2);

            t1.Start();
            t2.Start();

            Task.WaitAll(t1, t2);

            Console.WriteLine($"Number : {_num}");
        }
    }
}
