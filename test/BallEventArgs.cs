using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace test
{
    /// <summary>
    /// 
    /// </summary>
    public class BallEventArgs : EventArgs
    {

        //public event EventHandler ballInPlay;

        delegate Boolean moreOrlessDelegate(int item);

        /// <summary>
        /// Test
        /// </summary>
        public void Test()
        {
            var array = new List<int>() {1,2,3,4,5,6};
            moreOrlessDelegate d1 = new moreOrlessDelegate(More);
            moreOrlessDelegate d2 = new moreOrlessDelegate(Less);

            Print(array, d1);
            Print(array, d2);

            Console.WriteLine("OK");

        }

        static void Print(List<int> arr, moreOrlessDelegate dl)
        {
            foreach (var item in arr)
            {
                if (dl(item))
                {
                    Console.WriteLine(item);
                }
            }
        }

        /// <summary>
        /// More
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool More(int item)
        {
            return item < 3;
        }


        /// <summary>
        /// Less
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Less(int item)
        {
            return item > 1;
        }
    }
}
