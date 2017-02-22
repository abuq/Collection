using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using X.GlodEyes.Collectors;
using X.GlodEyes.Collectors.Specialized.JingDong;
using XFCollection.JingDong;

namespace XFCollection.fayuan
{
    /// <summary>
    /// 通过DocId得到
    /// </summary>
    public class JudgementsContentCollector : WebRequestCollector<IResut,NormalParameter>
    {


        //public override double DefaultMovePageTimeSpan => this.Random.Next(1 * 30, 1 * 60);

        /// <summary>
        /// 测试1
        /// </summary>
        public static void Test1()
        {
            var shopIds = File.ReadAllLines(@"C:\Users\Administrator\Desktop\test1.txt");

            /*var shopIds = File.ReadAllLines(@"C:\Users\sinoX\Desktop\errorList.txt");*/
            // 去掉字符串前后的"
            shopIds = Array.ConvertAll(shopIds, shopId => shopId.Trim('"'));

            foreach (var shopId in shopIds)
            {
                Console.WriteLine();
                Console.WriteLine($"shop id: {shopId}");
                try
                {
                    var parameter = new NormalParameter { Keyword = shopId };
                    TestHelp<JudgementsContentCollector>(parameter);
                }
                catch (NotSupportedException exception)
                {
                    Console.WriteLine($"error: {exception.Message}");
                }
            }
        }


        /// <summary>
        ///  测试
        /// </summary>
        internal static void Test()
        {
            var parameter = new NormalParameter {Keyword = @"efea2774-b647-11e3-84e9-5cf3fc0c2c18" };

            TestHelp<JudgementsContentCollector>(parameter);
        }

        /// <summary>
        /// 定义第一个链接
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        protected override string InitFirstUrl(NormalParameter param)
        {
            //var pause = Random.Next(1*10, 1*10);
            //Thread.Sleep(pause * 1000);
            //Console.WriteLine($"暂停{pause}秒");
            return $"http://wenshu.court.gov.cn/CreateContentJS/CreateContentJS.aspx?DocID={param.Keyword}";
        }

        /// <summary>
        /// 得到网页内容
        /// </summary>
        /// <param name="nextUrl"></param>
        /// <param name="postData"></param>
        /// <param name="cookies"></param>
        /// <param name="currentUrl"></param>
        /// <returns></returns>
        protected override string GetMainWebContent(string nextUrl, byte[] postData, ref string cookies, string currentUrl)
        {
            var webContent = base.GetMainWebContent(nextUrl, postData, ref cookies, currentUrl);

            if (!Regex.Match(webContent, "^window.location.href='.*';").Success)
                return webContent;



            Console.WriteLine(@"解析验证码中...................");
            bool isSuccess = false;
            int i = 1;
            while (isSuccess == false)
            {
                Console.WriteLine($"第{i}次解析验证码");
                TesseractDemo tesseract = new TesseractDemo();
                if ((isSuccess = tesseract.HandleValidateCode()) == false)
                    i++;
            }

            return base.GetMainWebContent(nextUrl, postData, ref cookies, currentUrl);
        }

        /// <summary>
        /// 解析当前元素
        /// </summary>    
        /// <returns></returns>
        protected override IResut[] ParseCurrentItems()
        {

            
            var resultList = new List<IResut>();
            var cookies = string.Empty;
            //var htmlString = base.GetMainWebContent(CurrentUrl, null,ref cookies, null);

           
            var judgementsTitle = GetJudgementsTitle(HtmlSource);
            var judgementsPubDate = GetJudgementsPubDate(HtmlSource);
            var judgementsContent = GetJudgementsContent(HtmlSource);
            var judgementsContentHtml = GetJudgementsContentHtml(HtmlSource);

            var resut = new Resut
            {
                ["DocId"] = Regex.Match(CurrentUrl, @"(?<==).*").Value,
                ["Url"] = CurrentUrl,
                ["Title"] = judgementsTitle,
                ["PubDate"] = judgementsPubDate,
                ["Content"] = judgementsContent,
                ["ContentHtml"] = judgementsContentHtml
            };


            resultList.Add(resut);

            

            return resultList.ToArray();
        }



        /// <summary>
        /// 解析下一页
        /// </summary>
        /// <returns></returns>
        protected override string ParseNextUrl()
        {
            return null;
        }

        /// <summary>
        /// 解析页面总数
        /// </summary>
        /// <returns></returns>
        protected override int ParseCountPage()
        {
            return 1;
        }

        /// <summary>
        /// 解析当前页数
        /// </summary>
        /// <returns></returns>
        protected override int ParseCurrentPage()
        {
            return 1;
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
        /// 得到内容HTML格式
        /// </summary>
        /// <param name="htmlString"></param>
        /// <returns></returns>
        public string GetJudgementsContentHtml(string htmlString)
        {
            var match = Regex.Match(htmlString, @"(?<=\\""Html\\"":\\"")[\s\S]*(?=\\""})");
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
            {
                var value = match.Value;
                if (value.Equals(string.Empty))
                    content += System.Environment.NewLine;
                else
                    content += match.Value;
            }
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


    }
}
