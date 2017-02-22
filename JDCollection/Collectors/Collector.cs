namespace X.GlodEyes.Collectors
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Threading;

    using X.GlodEyes.Collectors.Specialized.JingDong;
    using X.GlodEyes.Logs;

    using NUnit.Framework;

    [TestFixture]
    /// <summary>
    ///     采集器的基础基类
    /// </summary>
    public abstract class Collector : Logable, ICollector
    {
        /// <summary>
        ///     为了文件使用
        /// </summary>
        protected readonly Random Random = new Random(Guid.NewGuid().GetHashCode());

        /// <summary>
        ///     Gets or sets the count page.
        /// </summary>
        /// <value>
        ///     The count page.
        /// </value>
        public int CountPage { get; protected set; }

        /// <summary>
        ///     返回当前的采集值
        /// </summary>
        /// <value>
        ///     The current.
        /// </value>
        public IResut[] Current { get; protected set; }

        /// <summary>
        ///     Gets the current page.
        /// </summary>
        /// <value>
        ///     The current page.
        /// </value>
        public int CurrentPage { get; protected set; }

        /// <summary>
        ///     Gets or sets the current URL.
        /// </summary>
        /// <value>
        ///     The current URL.
        /// </value>
        public string CurrentUrl { get; protected set; }

        /// <summary>
        ///     Gets the default move page time span.
        /// </summary>
        /// <value>
        ///     The default move page time span.
        /// </value>
        public virtual double DefaultMovePageTimeSpan => this.Random.NextDouble() * 1d;

        /// <summary>
        ///     Gets or sets the next URL.
        /// </summary>
        /// <value>
        ///     The next URL.
        /// </value>
        public string NextUrl { get; protected set; }

        /// <summary>
        ///     Gets the current.
        /// </summary>
        /// <value>
        ///     The current.
        /// </value>
        object IEnumerator.Current => this.Current;

        /// <summary>
        ///     工作参数
        /// </summary>
        protected IParameter InnerParameter { get; private set; }

        /// <summary>
        ///     Releases unmanaged and - optionally - managed resources.
        /// </summary>
        public virtual void Dispose()
        {
        }

        /// <summary>
        ///     Gets the enumerator.
        /// </summary>
        /// <returns></returns>
        public IEnumerator<IResut[]> GetEnumerator()
        {
            return this;
        }

        /// <summary>
        ///     Gets the status point.
        /// </summary>
        /// <returns></returns>
        public IStatusPoint GetStatusPoint()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///     Setups the specified parameter.
        /// </summary>
        /// <param name="param">The parameter.</param>
        public virtual void Init(IParameter param)
        {
            this.InnerParameter = param;
        }

        /// <summary>
        ///     Moves the next.
        /// </summary>
        /// <returns></returns>
        public abstract bool MoveNext();

        /// <summary>
        ///     Resets this instance.
        /// </summary>
        public virtual void Reset()
        {
            this.Init(this.InnerParameter);
        }

        /// <summary>
        ///     Sets the status point.
        /// </summary>
        /// <param name="point">The point.</param>
        public void SetStatusPoint(IStatusPoint point)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///     Gets the enumerator.
        /// </summary>
        /// <returns></returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        [Test]
        /// <summary>
        ///     使用搜索的关键词进行搜索
        /// </summary>
        /// <typeparam name="T">采集器参数</typeparam>
        /// <param name="parameter">搜索参数.</param>
        /// <param name="pageLimit">搜索项目限制，如果为 0则不限制.</param>
        protected static void TestHelp<T>(IParameter parameter, int pageLimit = 0) where T : class, ICollector
        {
            var page = 0;


            /*using(var collector =  new JdShopListCollector())*/
            using (var collector = Activator.CreateInstance<T>())
            {
                collector.OnLogEvent += (sender, args) => Console.WriteLine(args.Message);

                collector.Init(parameter);

                while (collector.MoveNext())
                {
                    page++;

                    Console.WriteLine($"当前页码：{page} # {collector.CurrentPage} / {collector.CountPage}");
                    Console.WriteLine($"当前链接：{collector.CurrentUrl}");
                    Console.WriteLine($"下一页链接：{collector.NextUrl}");

                    var current = collector.Current;
                    var length = current?.Length;

                    Console.WriteLine();
                    Console.WriteLine($"返回结果 {length}条");

                    if (current != null)
                    {
                        Console.WriteLine(new string('=', 64));
                        Array.ForEach(current, Console.WriteLine);
                    }

                    if (pageLimit > 0 && page >= pageLimit)
                    {
                        Console.WriteLine($"达到最大页数：{page}/{pageLimit}");
                        break;
                    }

                    var sleepTime = collector.DefaultMovePageTimeSpan;
                    if (!(sleepTime > 0)) continue;

                    Console.WriteLine($"暂停 {sleepTime}秒");
                    Thread.Sleep(TimeSpan.FromSeconds(sleepTime));
                }

                Console.WriteLine(@"测试完成");
                Console.Beep();
            }
        }
    }
}