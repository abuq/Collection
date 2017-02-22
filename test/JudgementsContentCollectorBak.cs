using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;




///// <summary>
///// http://www.cnblogs.com/tonywang711/archive/2010/08/12/1798269.html
///// </summary>

namespace test
{
    internal class JudgementsContentCollectorBak
    {

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
        /// 测试
        /// </summary>
        public void test()
        {
            string htmlString = string.Empty;
            List<string> docId = new List<string>();
            int totalPages = 0;
            string judgementsTitle = string.Empty;
            string judgementsPubDate = string.Empty;
            string judgementContent = string.Empty;

            //htmlString = GetHtmlFromGet("http://wenshu.court.gov.cn/CreateContentJS/CreateContentJS.aspx?DocID=44e6c335-c4c4-4233-8d0c-cfce1b250cc1", Encoding.UTF8);
            //GetHtmlFromGet("http://wenshu.court.gov.cn/Index", Encoding.UTF8);
            //Param=案件类型:刑事案件&Page=5&Order=法院层级&Index=3&Direction=asc 这里找DocID

            htmlString = GetHtmlFromPost("http://wenshu.court.gov.cn/List/ListContent", Encoding.UTF8,
            "Param=案件类型:刑事案件&Page=5&Order=法院层级&Index=3&Direction=asc");
            totalPages = GetTotalPages(htmlString);

            docId.Clear();
            docId = GetDocId(htmlString);


            // efea2774-b647-11e3-84e9-5cf3fc0c2c18 eff7f53c-b647-11e3-84e9-5cf3fc0c2c18 f096e352-b647-11e3-84e9-5cf3fc0c2c18 f06ab91c-b647-11e3-84e9-5cf3fc0c2c18 f0750746-b647-11e3-84e9-5cf3fc0c2c18 
            // http://wenshu.court.gov.cn/CreateContentJS/CreateContentJS.aspx?DocID=efea2774-b647-11e3-84e9-5cf3fc0c2c18 这里找内容
            for (int i = 0; i < docId.Count; i++)
            {
                htmlString = GetHtmlFromGet($"http://wenshu.court.gov.cn/CreateContentJS/CreateContentJS.aspx?DocID={docId[i]}", Encoding.UTF8);
                judgementsTitle = GetJudgementsTitle(htmlString);
                judgementsPubDate = GetJudgementsPubDate(htmlString);
                judgementContent = GetJudgementsContent(htmlString);
            }

       
            
        }


        /// <summary>
        /// 通过GET方式获取页面的方法
        /// </summary>
        /// <param name="urlString">请求的URL</param>
        /// <param name="encoding">页面编码</param>
        /// <returns></returns>
        public static string GetHtmlFromGet(string urlString, Encoding encoding)
        {
            //定义局部变量
            HttpWebRequest httpWebRequest = null;
            HttpWebResponse httpWebResponse = null;
            Stream stream = null;
            var htmlString = string.Empty;

            //请求页面
            httpWebRequest = WebRequest.Create(urlString) as HttpWebRequest;
            //设置代理
            httpWebRequest.UserAgent =
                "Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 5.1; .NET CLR 2.0.50727; Maxthon 2.0)";
            //获取服务器的返回信息
            httpWebResponse = httpWebRequest.GetResponse() as HttpWebResponse;
            stream = httpWebResponse.GetResponseStream();
            var streamReader = new StreamReader(stream, encoding);
            htmlString = streamReader.ReadToEnd();
            streamReader.Close();
            stream.Close();


            //Console.WriteLine(htmlString);

            return htmlString;

        }

        /// <summary>
        /// 提供通过POST方法获取页面的方法
        /// </summary>
        /// <param name="urlString">请求的URL</param>
        /// <param name="encoding">页面使用的编码</param>
        /// <param name="postDataString">POST数据</param>
        /// <returns></returns>
        public static string GetHtmlFromPost(string urlString, Encoding encoding, string postDataString)
        {
            //定义局部变量
            CookieContainer cookieContainer = new CookieContainer();
            HttpWebRequest httpWebRequest = null;
            HttpWebResponse httpWebResponse = null;
            Stream inputStream = null;
            Stream outputStream = null;
            StreamReader streamReader = null;
            string htmlString = string.Empty;
            //转化POST数据
            byte[] postDataByte = encoding.GetBytes(postDataString);

            //建立页面请求
            httpWebRequest = WebRequest.Create(urlString) as HttpWebRequest;

            //指定请求处理方式
            httpWebRequest.Method = "POST";
            httpWebRequest.KeepAlive = true;
            httpWebRequest.ContentType = "application/x-www-form-urlencoded";
            httpWebRequest.CookieContainer = cookieContainer;
            httpWebRequest.ContentLength = postDataByte.Length;

            inputStream = httpWebRequest.GetRequestStream();
            inputStream.Write(postDataByte, 0, postDataByte.Length);

            inputStream.Close();

            //接受服务器
            httpWebResponse = httpWebRequest.GetResponse() as HttpWebResponse;
            outputStream = httpWebResponse.GetResponseStream();
            streamReader = new StreamReader(outputStream, encoding);
            htmlString = streamReader.ReadToEnd();

            streamReader.Close();

            foreach (Cookie cookie in httpWebResponse.Cookies)
            {
                cookieContainer.Add(cookie);
            }

            return htmlString;
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
            Console.WriteLine($"Title:{match.Value}");
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
            Console.WriteLine($"Content:{content}");
            return content;

        }
        /// <summary>
        /// 得到裁决文书的发布日期
        /// </summary>
        /// <param name="htmlString"></param>
        /// <returns></returns>
        public string GetJudgementsPubDate(string htmlString)
        {
            var pubDate = string.Empty;
            var match = Regex.Match(htmlString, @"(?<=\\""PubDate\\"":\\"").*?(?=\\"")");
            Console.WriteLine($"PubDate:{match.Value}");
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
            Console.WriteLine($"总共页数：{match.Value}页");
            return int.Parse(match.Value);
        }



    }
}
