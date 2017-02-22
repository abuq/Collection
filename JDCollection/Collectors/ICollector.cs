namespace X.GlodEyes.Collectors
{
    using System.Collections.Generic;

    using X.GlodEyes.Logs;

    /// <summary>
    ///     采集器接口
    /// </summary>
    public interface ICollector : IEnumerable<IResut[]>, IEnumerator<IResut[]>, ILogable
    {
        /// <summary>
        ///     进行初始化
        /// </summary>
        /// <param name="param">The parameter.</param>
        void Init(IParameter param);

        /// <summary>
        ///     页码总数
        /// </summary>
        /// <value>
        ///     The count page.
        /// </value>
        int CountPage { get; }

        /// <summary>
        ///     当前的页码
        /// </summary>
        /// <value>
        ///     The current page.
        /// </value>
        int CurrentPage { get; }

        /// <summary>
        ///     当前链接
        /// </summary>
        /// <value>
        ///     The current URL.
        /// </value>
        string CurrentUrl { get; }

        /// <summary>
        ///     翻页时默认的暂停时间
        /// </summary>
        /// <value>
        ///     The default move page time span.
        /// </value>
        double DefaultMovePageTimeSpan { get; }

        /// <summary>
        ///     下一点的链接
        /// </summary>
        /// <value>
        ///     The next URL.
        /// </value>
        string NextUrl { get; }

        /// <summary>
        ///     返回一个状态点，以后可以通过状态点恢复工作
        /// </summary>
        /// <returns></returns>
        IStatusPoint GetStatusPoint();

        /// <summary>
        ///     设置一个状态点以恢复工作
        /// </summary>
        /// <param name="point">The point.</param>
        void SetStatusPoint(IStatusPoint point);
    }
}