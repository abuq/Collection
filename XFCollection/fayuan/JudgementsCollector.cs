using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using X.CommLib.Net.WebRequestHelper;
using X.GlodEyes.Collectors;
using X.GlodEyes.Collectors.Specialized.JingDong;

namespace XFCollection.fayuan
{
    /// <summary>
    /// 法院裁决书信息采集
    /// </summary>
    public class JudgementsCollector : WebRequestCollector<IResut, FaYuanParameter>
    {

        /// <summary>
        /// 测试
        /// </summary>
        internal static void Test()
        {
            var parameter = new FaYuanParameter{ Reason = @"刑事案件",Court = @"北京市",Year = @"2016"};
            TestHelp<JudgementsCollector>(parameter);
        }

        /// <summary>
        /// 定义第一个链接,案件类型:刑事案件,法院地域:北京市,裁判年份:2016
        /// </summary>
        /// <param name="param">案件类型[刑事案件，民事案件，行政案件，赔偿案件，执行案件]</param>
        /// <returns></returns>
        protected override string InitFirstUrl(FaYuanParameter param)
        {
            IDictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("Param", $"案件类型:{param.Reason},法院地域:{param.Court},裁判年份:{param.Year}");
            dic.Add("Page", "20");
            dic.Add("Order", "裁判日期");
            dic.Add("Index", "1");
            dic.Add("Direction", "asc");

            var postData = WebRequestCtrl.BuildPostDatas(dic, Encoding.UTF8);

            var cookies = string.Empty;
            var htmlString = base.GetMainWebContent(HomePage, postData, ref cookies, HomePage);

            //var htmlString = GetHtmlFromPost(HomePage, Encoding.UTF8,$"Param=案件类型:{param.Keyword}&Page=20&Order=法院层级&Index=1&Direction=asc");

            _pageInfo = new PageInfo
            {
                Index = 1,
                CaseType = param.Reason,
                Area = param.Court,
                Year = param.Year,
                Page = 20,
                TotalPages = GetTotalPages(htmlString)
            };
            _pageInfo.TotalIndex = _pageInfo.TotalPages/_pageInfo.Page;

            Console.WriteLine($"总共页数:{_pageInfo.TotalIndex}");


            return HomePage;

        }

        private PageInfo _pageInfo;
        private const string HomePage = @"http://wenshu.court.gov.cn/List/ListContent";

        /// <summary>
        /// 页面信息类
        /// </summary>
        internal class PageInfo
        {
            /// <summary>
            /// 第几页
            /// </summary>
            public int Index { get; set; }

            /// <summary>
            /// 案件类型
            /// </summary>
            public string CaseType { get; set; }

            /// <summary>
            /// 法院地域
            /// </summary>
            public string Area { get; set; }

            /// <summary>
            /// 裁决年份
            /// </summary>
            public string Year { get; set; }

            /// <summary>
            /// 每页几个裁决书
            /// </summary>
            public int Page { get; set; }

            /// <summary>
            /// 总共几个裁决书
            /// </summary>
            public int TotalPages { get; set; }

            /// <summary>
            /// 总共需要翻多少页
            /// </summary>
            public int TotalIndex { get; set; }

        }


        /// <summary>
        /// 找到页面内容
        /// </summary>
        /// <param name="nextUrl"></param>
        /// <param name="postData"></param>
        /// <param name="cookies"></param>
        /// <param name="currentUrl"></param>
        /// <returns></returns>
        protected override string GetMainWebContent(
            string nextUrl,
            byte[] postData,
            ref string cookies,
            string currentUrl)
        {
            IDictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("Param", $"案件类型:{_pageInfo.CaseType},法院地域:{_pageInfo.Area},裁判年份:{_pageInfo.Year}");
            dic.Add("Page", "20");
            dic.Add("Order", "法院层级");
            dic.Add("Index", $"{_pageInfo.Index}");
            dic.Add("Direction", "asc");

            var postDataCur = WebRequestCtrl.BuildPostDatas(dic,Encoding.UTF8);


            return base.GetMainWebContent(HomePage, postDataCur, ref cookies, HomePage);

            //return GetHtmlFromPost(HomePage, Encoding.UTF8, $"Param=案件类型:{_pageInfo.Reason}&Page=20&Order=法院层级&Index={_pageInfo.Index}&Direction=asc");
        }

        /// <summary>
        /// 解析当前的元素
        /// </summary>
        /// <returns></returns>
        protected IResut[] ParseCurrentItems(string htmlSource)
        {

            // efea2774-b647-11e3-84e9-5cf3fc0c2c18 eff7f53c-b647-11e3-84e9-5cf3fc0c2c18 f096e352-b647-11e3-84e9-5cf3fc0c2c18 f06ab91c-b647-11e3-84e9-5cf3fc0c2c18 f0750746-b647-11e3-84e9-5cf3fc0c2c18 
            // http://wenshu.court.gov.cn/CreateContentJS/CreateContentJS.aspx?DocID=efea2774-b647-11e3-84e9-5cf3fc0c2c18 这里找内容

            var resultList = new List<IResut>();
            var docIds = GetDocId(htmlSource);

            foreach (var docId in docIds)
            {
                var cookies = string.Empty;
                var url = $"http://wenshu.court.gov.cn/CreateContentJS/CreateContentJS.aspx?DocID={docId}";
                //var htmlString = GetHtmlFromGet(url,Encoding.UTF8);
                var htmlString = base.GetWebContent(url);
                var judgementsTitle = GetJudgementsTitle(htmlString);
                var judgementsPubDate = GetJudgementsPubDate(htmlString);
                var judgementContent = GetJudgementsContent(htmlString);

                IResut resut = new Resut();


                resut["DocId"] = docId;
                resut["Url"] = url;
                resut["Reason"] = _pageInfo.CaseType;
                resut["Court"] = _pageInfo.Area;
                resut["Year"] = _pageInfo.Year;
                resut["Title"] = judgementsTitle;
                resut["PubDate"] = judgementsPubDate;
                resut["Content"] = judgementContent;
                

                resultList.Add(resut);

            }
            
            return resultList.ToArray();
        }

        /// <summary>
        /// 解析下一个链接
        /// </summary>
        /// <returns></returns>
        protected override string ParseNextUrl()
        {
            if (_pageInfo.Index < _pageInfo.TotalIndex)
            {

                this._pageInfo.Index++;
                return HomePage;
            }
            else
                return null;

        }

        /// <summary>
        /// 解析当前页面
        /// </summary>
        /// <returns></returns>
        protected override int ParseCurrentPage()
        {

            return _pageInfo.Index-1;
        }

        /// <summary>
        /// 解析网页总页数
        /// </summary>
        /// <returns></returns>
        protected override int ParseCountPage()
        {
            return _pageInfo.TotalIndex;
        }


        /// <summary>
        /// 得到DocId
        /// </summary>
        /// <param name="htmlString">网页内容</param>
        /// <returns></returns>
        public static List<string> GetDocId(string htmlString)
        {
            var docId = new List<string>();
            var matchs = Regex.Matches(htmlString, @"(?<=\\""文书ID\\"":\\"").*?(?=\\"")");
            foreach (Match match in matchs)
            {
                Console.WriteLine($"DocId:{match.Value}");
                docId.Add(match.Value);
            }

            return docId;
        }

        /// <summary>
        /// 得到裁决文书的标题
        /// </summary>
        /// <returns></returns>
        public string GetJudgementsTitle(string htmlString)
        {
            var match = Regex.Match(htmlString, @"(?<=\\""Title\\"":\\"").*?(?=\\"")");
            //Console.WriteLine($"Title:{match.Value}");
            return match.Value;
        }

        /// <summary>
        /// 得到裁判文书的内容
        /// </summary>
        /// <param name="htmlString">网页内容</param>
        /// <returns></returns>
        public string GetJudgementsContent(string htmlString)
        {
            var content = string.Empty;
            var matchs = Regex.Matches(htmlString, "(?<=>).*?(?=<)");
            foreach (Match match in matchs)
                content += match.Value;
            //Console.WriteLine($"Content:{content}");
            return content;

        }


        /// <summary>
        /// 得到裁决文书的发布日期
        /// </summary>
        /// <param name="htmlString">网页内容</param>
        /// <returns></returns>
        public string GetJudgementsPubDate(string htmlString)
        {
            var pubDate = string.Empty;
            var match = Regex.Match(htmlString, @"(?<=\\""PubDate\\"":\\"").*?(?=\\"")");
            //Console.WriteLine($"PubDate:{match.Value}");
            return match.Value;

        }
        /// <summary>
        /// 得到总共的页数
        /// </summary>
        /// <param name="htmlString">网页内容</param>
        /// <returns></returns>
        public static int GetTotalPages(string htmlString)
        {

            var match = Regex.Match(htmlString, @"(?<=\\""Count\\"":\\"")\d*(?=\\"")");
            //Console.WriteLine($"总共页数：{match.Value}页");
            return int.Parse(match.Value);
        }

        /// <summary>
        /// 解析当前的数据
        /// </summary>
        /// <returns></returns>
        protected override IResut[] ParseCurrentItems()
        {
            return this.ParseCurrentItems(this.HtmlSource);
        }



        ///// <summary>
        ///// 通过GET方式获取页面的方法
        ///// </summary>
        ///// <param name="urlString">请求的URL</param>
        ///// <param name="encoding">页面编码</param>
        ///// <returns></returns>
        //public static string GetHtmlFromGet(string urlString, Encoding encoding)
        //{
        //    //定义局部变量
        //    HttpWebRequest httpWebRequest = null;
        //    HttpWebResponse httpWebResponse = null;
        //    Stream stream = null;
        //    StreamReader streamReader = null;
        //    var htmlString = string.Empty;

        //    //请求页面
        //    try
        //    {
        //        httpWebRequest = WebRequest.Create(urlString) as HttpWebRequest;
        //    }
        //    //处理异常
        //    catch (Exception ex)
        //    {
        //        throw new Exception("建立页面请求时发生错误！", ex);
        //    }
        //    httpWebRequest.UserAgent = "Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 5.1; .NET CLR 2.0.50727; Maxthon 2.0)";
        //    //获取服务器的返回信息
        //    try
        //    {
        //        httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
        //        stream = httpWebResponse.GetResponseStream();
        //    }
        //    //处理异常
        //    catch (Exception ex)
        //    {
        //        throw new Exception("接受服务器返回页面时发生错误！", ex);
        //    }
        //    streamReader = new StreamReader(stream, encoding);
        //    //读取返回页面
        //    try
        //    {
        //        htmlString = streamReader.ReadToEnd();
        //    }
        //    //处理异常
        //    catch (Exception ex)
        //    {
        //        throw new Exception("读取页面数据时发生错误！", ex);
        //    }

        //    //释放资源返回结果
        //    streamReader.Close();
        //    stream.Close();

        //    return htmlString;

        //}

        ///// <summary>
        ///// 提供通过POST方法获取页面的方法
        ///// </summary>
        ///// <param name="urlString">请求的URL</param>
        ///// <param name="encoding">页面使用的编码</param>
        ///// <param name="postDataString">POST数据</param>
        ///// <returns></returns>
        //public static string GetHtmlFromPost(string urlString, Encoding encoding, string postDataString)
        //{
        //    //定义局部变量
        //    CookieContainer cookieContainer = new CookieContainer();
        //    HttpWebRequest httpWebRequest = null;
        //    HttpWebResponse httpWebResponse = null;
        //    Stream inputStream = null;
        //    Stream outputStream = null;
        //    StreamReader streamReader = null;
        //    string htmlString = string.Empty;
        //    //转换POST数据
        //    byte[] postDataByte = encoding.GetBytes(postDataString);
        //    //建立页面请求
        //    try
        //    {
        //        httpWebRequest = WebRequest.Create(urlString) as HttpWebRequest;
        //    }
        //    //处理异常
        //    catch (Exception ex)
        //    {
        //        throw new Exception("建立页面请求时发生错误！", ex);
        //    }
        //    //指定请求处理方式
        //    httpWebRequest.Method = "POST";
        //    httpWebRequest.KeepAlive = false;
        //    httpWebRequest.ContentType = "application/x-www-form-urlencoded";
        //    httpWebRequest.CookieContainer = cookieContainer;
        //    httpWebRequest.ContentLength = postDataByte.Length;
        //    //向服务器传送数据
        //    try
        //    {
        //        inputStream = httpWebRequest.GetRequestStream();
        //        inputStream.Write(postDataByte, 0, postDataByte.Length);
        //    }
        //    //处理异常
        //    catch (Exception ex)
        //    {
        //        throw new Exception("发送POST数据时发生错误！", ex);
        //    }
        //    finally
        //    {
        //        inputStream.Close();
        //    }
        //    //接受服务器返回信息
        //    try
        //    {
        //        httpWebResponse = httpWebRequest.GetResponse() as HttpWebResponse;
        //        outputStream = httpWebResponse.GetResponseStream();
        //        streamReader = new StreamReader(outputStream, encoding);
        //        htmlString = streamReader.ReadToEnd();
        //    }
        //    //处理异常
        //    catch (Exception ex)
        //    {
        //        throw new Exception("接受服务器返回页面时发生错误！", ex);
        //    }
        //    finally
        //    {
        //        streamReader.Close();
        //    }
        //    foreach (Cookie cookie in httpWebResponse.Cookies)
        //    {
        //        cookieContainer.Add(cookie);
        //    }
        //    return htmlString;

        //}


    }
}
