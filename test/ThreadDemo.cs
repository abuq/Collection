using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace test
{
    class ThreadDemo
    {

        private static Int32 Sum(Int32 n)
        {
            Int32 sum = 0;
            for (; n > 0; n--)
            {
                checked
                {
                    sum += n;
                }
            }
            return sum;
        }

        public void Test()
        {
            Task<Int32> t = new Task<int>(n=> Sum((Int32) n),100);

            t.Start();

            t.Wait();

            Console.Write("This Sum is:"+t.Result);
        }



        //static void Main(string[] args)
        //{


        //    Thread[] threads = new Thread[10];
        //    for (int i = 0; i < 10; i++)
        //    {
        //        Thread t = new Thread(Run);

        //        t.Start(i);
        //        //t.Join();
        //        threads[i] = t;
        //    }

        //    for (int i = 0; i < 10; i++)
        //    {
        //        threads[i].Join();
        //    }
        //}

        static object obj = new object();

        static int count = 0;

        static void Run(object o)
        {
            //Thread.Sleep(10);

            Console.WriteLine("当前数字count+i:{0}+{1}",count, o);
            count = count + int.Parse(o.ToString());
        }

    }
}
