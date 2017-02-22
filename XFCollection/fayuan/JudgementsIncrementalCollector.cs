using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using X.CommLib.Net.WebRequestHelper;
using X.GlodEyes.Collectors;
using XFCollection.fayuan;

namespace XFCollection.bin.fayuan
{
    /// <summary>
    /// 增量
    /// </summary>
    public class JudgementsIncrementalCollector:JudgementsDocIdCollector
    {



        private readonly DateTime _date = DateTime.Now.AddMonths(-1);
        private int _done = 0;


        /// <summary>
        /// 测试
        /// </summary>
        internal static void Test1()
        {
            //第一种参数类型
            var parameter = new FaYuanParameter
            {
                Reason = @"民事案件 # 离婚纠纷 # 四级案由",
                Court = @"北京市房山区人民法院 # 基层法院",
                Year = @"2016"
            };




            //var parameter = new FaYuanParameter
            //{
            //    Reason = @"民事案件",
            //    Court = @"邢台市桥西区人民法院 # 基层法院",
            //    Year = @""
            //};



            //第五种参数
            //var parameter = new FaYuanParameter
            //{
            //    Reason = @"民事案件 # 离婚纠纷 # 四级案由",
            //    Court = @"",
            //    Year = @""
            //};




            TestHelp<JudgementsIncrementalCollector>(parameter);
        }






        /// <summary>
        /// 定义第一个链接,案件类型:刑事案件,法院地域:北京市,裁判年份:2016
        /// </summary>
        /// <param name="param">参数</param>
        /// <returns></returns>
        protected override string InitFirstUrl(FaYuanParameter param)
        {
            var paramDic = GetParam(param);

            //案件类型:民事案件,四级案由:遗嘱继承纠纷,中级法院:北京市第一中级人民法院,裁判年份:2016
            IDictionary<string, string> dic = new Dictionary<string, string>();
            var paramString = $"案件类型:{paramDic["CaseType"]}";
            if (!string.IsNullOrEmpty(paramDic["ReasonLevel"]) && !string.IsNullOrEmpty(paramDic["ReasonKey"]))
                paramString = $"{paramString},{paramDic["ReasonLevel"]}:{paramDic["ReasonKey"]}";
            if (!string.IsNullOrEmpty(paramDic["CourtLevel"]) && !string.IsNullOrEmpty(paramDic["CourtKey"]))
                paramString = $"{paramString},{paramDic["CourtLevel"]}:{paramDic["CourtKey"]}";
            if (!string.IsNullOrEmpty(paramDic["Year"]))
                paramString = $"{paramString},裁判年份:{paramDic["Year"]}";
            if (paramDic["CourtLevel"].Equals("法院地域"))
                paramString = $"{paramString},法院层级:高级法院";

            dic.Add("Param", paramString);
            dic.Add("Page", "20");
            dic.Add("Order", "裁判日期");
            dic.Add("Index", "1");
            dic.Add("Direction", "desc");

            var postData = WebRequestCtrl.BuildPostDatas(dic, Encoding.UTF8);

            var cookies = string.Empty;
            var htmlString = base.GetContent(HomePage, postData, ref cookies, HomePage);

            //处理验证码
            const string shielded = "\"remind\"";
            if (htmlString.Equals(shielded))
            {
                //循环处理验证码，直到验证通过
                this.LoopHandleValidateCode();
                //处理完了重新来
                htmlString = base.GetContent(HomePage, postData, ref cookies, HomePage);
            }

            //设置参数
            _pageInfo = new PageInfo
            {
                Index = 1,
                CaseType = paramDic["CaseType"],
                ReasonLevel = paramDic["ReasonLevel"],
                ReasonKey = paramDic["ReasonKey"],
                CourtLevel = paramDic["CourtLevel"],
                CourtKey = paramDic["CourtKey"],
                Year = paramDic["Year"],
                Page = 20,
                TotalPages = GetTotalPages(htmlString),
            };
            _pageInfo.TotalIndex = (_pageInfo.TotalPages / _pageInfo.Page) + (_pageInfo.TotalPages % _pageInfo.Page == 0 ? 0 : 1);
            _pageInfo.ActuallyIndex = _pageInfo.TotalIndex > 2 * _pageInfo.MaxIndex ? 2 * _pageInfo.MaxIndex : _pageInfo.TotalIndex;


            return HomePage;
        }

        /// <summary>
        /// 解析当前元素
        /// </summary>
        /// <param name="htmlSource"></param>
        /// <returns></returns>
        protected override IResut[] ParseCurrentItems(string htmlSource)
        {


            var resultList = new List<IResut>();
            var docIds = GetDocId(htmlSource);
            var pubDate = GetOneDate(htmlSource);
            Console.WriteLine(pubDate);
            if (DateTime.Compare(_date, DateTime.Parse(pubDate)) < 0)
            {
                foreach (var docId in docIds)
                {
                    IResut resut = new Resut();




                    resut["DocId"] = docId;
                    resut["CaseType"] = base._pageInfo.CaseType;
                    resut["ReasonLevel"] = base._pageInfo.ReasonLevel;
                    resut["ReasonKey"] = base._pageInfo.ReasonKey;
                    resut["CourtLevel"] = base._pageInfo.CourtLevel;
                    resut["CourtKey"] = base._pageInfo.CourtKey;
                    resut["Year"] = base._pageInfo.Year;

                    resultList.Add(resut);
                }
            }
            else
            {
                _done = 1;
            }

        

            return resultList.ToArray();

            
        }



        /// <summary>
        /// GetMainWebContent
        /// 
        /// </summary>
        /// <param name="nextUrl"></param>
        /// <param name="postData"></param>
        /// <param name="cookies"></param>
        /// <param name="currentUrl"></param>
        /// <returns></returns>
        protected override string GetMainWebContent(string nextUrl, byte[] postData, ref string cookies, string currentUrl)
        {

            IDictionary<string, string> dic = new Dictionary<string, string>();

            var paramString = $"案件类型:{_pageInfo.CaseType}";
            if (!string.IsNullOrEmpty(_pageInfo.ReasonLevel) && !string.IsNullOrEmpty(_pageInfo.ReasonKey))
                paramString = $"{paramString},{_pageInfo.ReasonLevel}:{_pageInfo.ReasonKey}";
            if (!string.IsNullOrEmpty(_pageInfo.CourtLevel) && !string.IsNullOrEmpty(_pageInfo.CourtKey))
                paramString = $"{paramString},{_pageInfo.CourtLevel}:{_pageInfo.CourtKey}";
            if (!string.IsNullOrEmpty(_pageInfo.Year))
                paramString = $"{paramString},裁判年份:{_pageInfo.Year}";
            if (_pageInfo.CourtLevel.Equals("法院地域"))
                paramString = $"{paramString},法院层级:高级法院";

            dic.Add("Param", paramString);
            dic.Add("Page", "20");
            dic.Add("Order", "裁判日期");
            dic.Add("Index", $"{_pageInfo.Index}");
            dic.Add("Direction", "desc");

            
            var postDataCur = WebRequestCtrl.BuildPostDatas(dic, Encoding.UTF8);

            var htmlString = base.GetContent(HomePage, postDataCur, ref cookies, HomePage);


            const string shielded = "\"remind\"";
            if (!htmlString.Equals(shielded)) return htmlString;
            //等于指定字符串，则处理验证码
            this.LoopHandleValidateCode();

            return base.GetContent(HomePage, postDataCur, ref cookies, HomePage);
        }

        /// <summary>
        /// 解析下一个链接
        /// </summary>
        /// <returns></returns>
        protected override string ParseNextUrl()
        {
            if (_done==0)
            {
                _pageInfo.Index++;
                return HomePage;
            }
            else
            {
                _pageInfo.Index++;
                return null;
            }


        }



        /// <summary>
        /// 得到第一个日期
        /// </summary>
        /// <param name="htmlString">网页内容</param>
        /// <returns></returns>
        public static string GetOneDate(string htmlString)
        {
            return Regex.Match(htmlString, @"(?<=\\""裁判日期\\"":\\"").*?(?=\\"")").Value;
        }





    }
}
