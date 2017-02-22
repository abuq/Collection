using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace test
{
    class EnumerTest
    {
        /// <summary>
        /// IterationSample
        /// </summary>
        public class IterationSample : IEnumerable
        {
            private object[] values;
            private int startingPoint;

            public IterationSample(object[] values, int startingPoint)
            {
                this.values = values;
                this.startingPoint = startingPoint;
            }

            public IEnumerator GetEnumerator()
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// IterationSampleIterator
        /// </summary>
        public class IterationSampleIterator : IEnumerator
        {

            public bool MoveNext()
            {
                throw new NotImplementedException();
            }

            public void Reset()
            {
                throw new NotImplementedException();
            }

            public object Current { get; }
        }


        

    }
}
