namespace X.GlodEyes.Collectors
{
    using System;
    using System.Collections.Generic;

    using X.CommLib.Net.WebRequestHelper;
    using X.CommLib.Office;

    /// <summary>
    ///     基于 webRequest 的发送工具
    /// </summary>
    public abstract class WebRequestCollector<TResut, TParam> : NormalCollector, 
                                                                IEnumerator<TResut[]>, 
                                                                IEnumerable<TResut[]>
        where TResut : IResut where TParam : IParameter
    {
        /// <summary>
        ///     工作过程中的 Cookies 值
        /// </summary>
        /// <value>
        ///     The cookies.
        /// </value>
        public string Cookies { get; set; }

        /// <summary>
        ///     返回当前的采集值
        /// </summary>
        /// <value>
        ///     The current.
        /// </value>
        public new TResut[] Current { get; protected set; }

        /// <summary>
        ///     是否还有更多的内容
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance has more; otherwise, <c>false</c>.
        /// </value>
        public bool HasMore { get; protected set; }

        /// <summary>
        ///     当前页面的源码
        /// </summary>
        /// <value>
        ///     The HTML source.
        /// </value>
        public string HtmlSource { get; set; }

        /// <summary>
        ///     The inner parameter
        /// </summary>
        public TParam InnerParam { get; private set; }

        /// <summary>
        ///     上一级的链接
        /// </summary>
        /// <value>
        ///     The last URL.
        /// </value>
        public string LastUrl { get; set; }

        /// <summary>
        ///     使用指定的参数进行初始化
        /// </summary>
        /// <param name="param">The parameter.</param>
        public void Init(TParam param)
        {
            this.InnerParam = param;

            this.NextUrl = this.InitFirstUrl(param);

            this.HasMore = true;
        }

        /// <summary>
        ///     Setups the specified parameter.
        /// </summary>
        /// <param name="param">The parameter.</param>
        public override void Init(IParameter param)
        {
            base.Init(param);

            var tParam = Activator.CreateInstance<TParam>();
            tParam.CopyFrom(param);

            this.Init(tParam);
        }

        /// <summary>
        ///     Moves the next.
        /// </summary>
        /// <returns></returns>
        public override bool MoveNext()
        {
            if (!this.HasMore)
            {
                return false;
            }

            this.HtmlSource = this.MoveToNextPage();

            this.NextUrl = this.ParseNextUrl();

            this.CurrentPage = this.ParseCurrentPage();

            this.CountPage = this.ParseCountPage();

            this.Current = this.ParseCurrentItems();

            this.HasMore = this.DetectHasMore();

            this.UpdateResultRankInfo(this.Current, this.CurrentPage);

            return true;
        }

        /// <summary>
        ///     更新排名信息
        /// </summary>
        /// <param name="items">The current.</param>
        /// <param name="page">The current page.</param>
        private void UpdateResultRankInfo(TResut[] items, int page)
        {
            Array.ForEach(items, item => this.SetResultSearchPageIndex(item, page, false));

            for (var i = 0; i < items.Length; i++)
            {
                this.SetResultSearchPageRank(items[i], i + 1, false);
            }
        }

        /// <summary>
        ///     Gets the enumerator.
        /// </summary>
        /// <returns></returns>
        IEnumerator<TResut[]> IEnumerable<TResut[]>.GetEnumerator()
        {
            return this;
        }

        /// <summary>
        ///     检测是否还有更多的内容
        /// </summary>
        /// <returns></returns>
        protected virtual bool DetectHasMore()
        {
            return !StringExtension.IsNullOrWhiteSpace(this.NextUrl);
        }

        /// <summary>
        /// 返回当前主页面的内容
        /// </summary>
        /// <param name="nextUrl">The next URL.</param>
        /// <param name="postData">The post data.</param>
        /// <param name="cookies">The cookies.</param>
        /// <param name="currentUrl">The current URL.</param>
        /// <returns></returns>
        protected virtual string GetMainWebContent(string nextUrl, byte[] postData, ref string cookies, string currentUrl)
        {
            return this.GetWebContent(nextUrl, postData, ref cookies, currentUrl);
        }

        /// <summary>
        ///     返回指定 url 的页面内容
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="postData">The post data.</param>
        /// <param name="cookies">The cookies.</param>
        /// <param name="refere">The refere.</param>
        /// <param name="isAjax">是不是ajax请求.</param>
        /// <returns></returns>
        protected string GetWebContent(
            string url, 
            byte[] postData, 
            ref string cookies, 
            string refere = "", 
            bool isAjax = false)
        {
            var param = WebRequestCtrl.GetWebContentParam.Default;
            param.Refere = refere;
            param.IsAjax = isAjax;

            cookies = cookies ?? string.Empty;
            var webContent = WebRequestCtrl.GetWebContent(url, null, ref cookies, 1, param);

            return webContent;
        }

        /// <summary>
        ///     返回指定 url 的页面内容
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="keepCookies">if set to <c>true</c> [keep cookies].</param>
        /// <returns></returns>
        protected string GetWebContent(string url, bool keepCookies = true)
        {
            var cookies = this.Cookies;
            var webContent = this.GetWebContent(url, null, ref cookies);

            if (keepCookies)
            {
                this.Cookies = cookies;
            }

            return webContent;
        }

        /// <summary>
        ///     为第一页作初始化准备
        /// </summary>
        /// <param name="param">The parameter.</param>
        protected abstract string InitFirstUrl(TParam param);

        /// <summary>
        ///     移动到下一页
        /// </summary>
        /// <returns>System.String.</returns>
        protected virtual string MoveToNextPage()
        {
            var currentUrl = this.CurrentUrl;
            var nextUrl = this.NextUrl;

            if (StringExtension.IsNullOrWhiteSpace(nextUrl))
            {
                throw new NotSupportedException(@"没有指定需要访问的页面链接");
            }

            var cookies = this.Cookies;
            var webContent = this.GetMainWebContent(nextUrl, null, ref cookies, currentUrl);

            this.VerifyWebContent(webContent, cookies);

            this.Cookies = cookies;
            this.LastUrl = currentUrl;
            this.CurrentUrl = this.NextUrl;

            return webContent;
        }

        /// <summary>
        ///     解析出总页码
        /// </summary>
        /// <returns></returns>
        protected virtual int ParseCountPage()
        {
            return -1;
        }

        /// <summary>
        ///     解析出当前值
        /// </summary>
        /// <returns></returns>
        protected abstract TResut[] ParseCurrentItems();

        /// <summary>
        ///     解析出当前页码
        /// </summary>
        /// <returns></returns>
        protected virtual int ParseCurrentPage()
        {
            return -1;
        }

        /// <summary>
        ///     解析出下一页的地址，默认当没有下一页的时候，枚举停止
        /// </summary>
        /// <returns></returns>
        protected abstract string ParseNextUrl();

        /// <summary>
        ///     检测页面内容是否正确
        /// </summary>
        /// <param name="webContent">Content of the web.</param>
        /// <param name="cookies">The cookies.</param>
        protected virtual void VerifyWebContent(string webContent, string cookies)
        {
        }
    }
}