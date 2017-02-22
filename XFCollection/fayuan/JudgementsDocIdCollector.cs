using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using GE.Data;
using X.CommLib.Net.WebRequestHelper;
using X.GlodEyes.Collectors;

namespace XFCollection.fayuan
{

    /// <summary>
    /// 得到裁判书DocId类
    /// </summary>
    public class JudgementsDocIdCollector : WebRequestCollector<IResut,FaYuanParameter>
    {

        /// <summary>
        /// 测试
        /// </summary>
        internal static void Test()
        {
            //第一种参数类型
            //var parameter = new FaYuanParameter
            //{
            //    Reason = @"民事案件 # 离婚纠纷 # 四级案由",
            //    Court = @"北京市房山区人民法院 # 基层法院",
            //    Year = @"2016"
            //};


            //var parameter = new FaYuanParameter
            //{
            //    Reason = @"民事案件 # 离婚纠纷 # 四级案由",
            //    Court = @"北京市房山区人民法院 # 基层法院",
            //    Year = @""
            //};


            var parameter = new FaYuanParameter
            {
                Reason = @"民事案件 # 建设工程施工合同纠纷 # 五级案由",
                Court = @"",
                Year = @"2016"
            };

            //var parameter = new FaYuanParameter
            //{
            //    Reason = @"民事案件",
            //    Court = @"北京市房山区人民法院 # 基层法院",
            //    Year = @"2016"
            //};

            //第五种参数
            //var parameter = new FaYuanParameter
            //{
            //    Reason = @"民事案件 # 离婚纠纷 # 四级案由",
            //    Court = @"",
            //    Year = @""
            //};




            TestHelp<JudgementsDocIdCollector>(parameter);
        }

        /// <summary>
        /// 页面信息类
        /// </summary>
        public class PageInfo
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
            /// 案由层级 
            /// </summary>
            public string ReasonLevel { get; set; }

            /// <summary>
            /// 案由名字
            /// </summary>
            public string ReasonKey { get; set; }


            /// <summary>
            /// 法院层级
            /// </summary>
            public string CourtLevel { get; set; }

            /// <summary>
            /// 法院地域
            /// </summary>
            public string CourtKey { get; set; }

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


            /// <summary>
            /// 最大只能翻100页 用于正向和反向
            /// </summary>
            public int MaxIndex { get; } = 100;

            /// <summary>
            /// TotalIndex 和 2*MaxIndex 比较 得出一个小的 为实际需要翻页的页数
            /// </summary>
            public int ActuallyIndex { get; set; }
        }

        /// <summary>
        /// 缺省停顿时间
        /// </summary>
        //public override double DefaultMovePageTimeSpan => this.Random.Next(4*59,4*60);


        public PageInfo _pageInfo;
        /// <summary>
        /// HomePage
        /// </summary>
        public const string HomePage = @"http://wenshu.court.gov.cn/List/ListContent";

        /// <summary>
        /// 定义第一个链接,案件类型:刑事案件,法院地域:北京市,裁判年份:2016
        /// </summary>
        /// <param name="param">参数</param>
        /// <returns></returns>
        protected override string InitFirstUrl(FaYuanParameter param)
        {
            var paramDic = GetParam(param);

            //案件类型:民事案件,四级案由:遗嘱继承纠纷,中级法院:北京市第一中级人民法院,裁判年份:2016
            IDictionary<string,string> dic = new Dictionary<string, string>();
            var paramString = $"案件类型:{paramDic["CaseType"]}";
            if (!string.IsNullOrEmpty(paramDic["ReasonLevel"]) && !string.IsNullOrEmpty(paramDic["ReasonKey"]))
                paramString = $"{paramString},{paramDic["ReasonLevel"]}:{paramDic["ReasonKey"]}";
            if (!string.IsNullOrEmpty(paramDic["CourtLevel"]) && !string.IsNullOrEmpty(paramDic["CourtKey"]))
                paramString = $"{paramString},{paramDic["CourtLevel"]}:{paramDic["CourtKey"]}";
            if(!string.IsNullOrEmpty(paramDic["Year"]))
                paramString = $"{paramString},裁判年份:{paramDic["Year"]}";
            if(paramDic["CourtLevel"].Equals("法院地域"))
                paramString = $"{paramString},法院层级:高级法院";

            dic.Add("Param",paramString);
            dic.Add("Page","20");
            dic.Add("Order", "裁判日期");
            dic.Add("Index","1");
            dic.Add("Direction","asc");
       
            var postData = WebRequestCtrl.BuildPostDatas(dic, Encoding.UTF8);

            var cookies = string.Empty;
            var htmlString = base.GetMainWebContent(HomePage, postData, ref cookies, HomePage);

            //处理验证码
            const string shielded = "\"remind\"";
            if (htmlString.Equals(shielded))
            {
                //循环处理验证码，直到验证通过
                this.LoopHandleValidateCode();
                //处理完了重新来
                htmlString = base.GetMainWebContent(HomePage, postData, ref cookies, HomePage);
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
            _pageInfo.TotalIndex = (_pageInfo.TotalPages/_pageInfo.Page)+(_pageInfo.TotalPages % _pageInfo.Page==0?0:1);
            _pageInfo.ActuallyIndex = _pageInfo.TotalIndex > 2 * _pageInfo.MaxIndex ? 2 * _pageInfo.MaxIndex : _pageInfo.TotalIndex;


            return HomePage;
        }

        /// <summary>
        /// 解析得到参数 
        /// Reason解析出一个参数(后面两个字符设为空字符)或者解析出三个参数
        /// Court解析出两个参数
        /// Year可能为空字符或者为null(设为空字符)
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        protected Dictionary<string, string> GetParam(FaYuanParameter param)
        {
            var dic = new Dictionary<string, string>();
            var stringEmpty = string.Empty;

            var num = 0;
            var matchs = Regex.Matches(param.Reason, @"(?<=#[\s]|^)[\S]*(?=[\s]|$)");  
            foreach (Match match in matchs)
            {
                num++;
                var value = match.Value;
                switch (num)
                {
                    case 1:
                        dic.Add("CaseType", value); 
                        break;
                    case 2:
                        dic.Add("ReasonKey", value);
                        break;
                    case 3:
                        dic.Add("ReasonLevel", value);
                        break;
                    default:
                        throw new Exception("输入的Reason参数解析出来的参数过多！");

                }
            }
            //如果为1 说明后面两个参数为空
            if (num == 1)
            {
                dic.Add("ReasonKey", stringEmpty);
                dic.Add("ReasonLevel", stringEmpty);
            }


            //如果为空字符串或者为null
            if (string.IsNullOrEmpty(param.Court))
            {
                dic.Add("CourtKey", stringEmpty);
                dic.Add("CourtLevel", stringEmpty);
            }
            else
            {
                num = 0; 
                matchs = Regex.Matches(param.Court, @"(?<=#[\s]|^)[\S]*(?=[\s]|$)");
                foreach (Match match in matchs)
                {
                    num++;
                    var value = match.Value;
                    switch (num)
                    {
                        case 1:
                            dic.Add("CourtKey", value);
                            break;
                        case 2:
                            dic.Add("CourtLevel", value);
                            break;
                        default:
                            throw new Exception("输入的Court参数解析出来的参数过多！");
                    }
                }
            }


            //为空字符或者null
            dic.Add("Year", string.IsNullOrEmpty(param.Year) ? stringEmpty : param.Year);


            return dic;
        }

        /// <summary>
        /// 解析当前元素
        /// </summary>
        /// <returns></returns>
        protected override IResut[] ParseCurrentItems()
        {
            return ParseCurrentItems(HtmlSource);
        }

      

        /// <summary>
        /// 解析当前元素
        /// </summary>
        /// <param name="htmlSource"></param>
        /// <returns></returns>
        protected virtual IResut[] ParseCurrentItems(string htmlSource)
        {
            var resultList = new List<IResut>();
            var docIds = GetDocId(htmlSource);

            foreach (var docId in docIds )
            {
                IResut resut = new Resut();

                resut["DocId"] = docId;
                resut["CaseType"] = _pageInfo.CaseType;
                resut["ReasonLevel"] = _pageInfo.ReasonLevel;
                resut["ReasonKey"] = _pageInfo.ReasonKey;
                resut["CourtLevel"] = _pageInfo.CourtLevel;
                resut["CourtKey"] = _pageInfo.CourtKey;                
                resut["Year"] = _pageInfo.Year;

                resultList.Add(resut);
            }

            return resultList.ToArray();
        }

        /// <summary>
        /// 找到页面内容
        /// </summary>
        /// <param name="nextUrl"></param>
        /// <param name="postData"></param>
        /// <param name="cookies"></param>
        /// <param name="currentUrl"></param>
        /// <returns></returns>
        protected override string GetMainWebContent(string nextUrl, byte[] postData, ref string cookies, string currentUrl)
        {

            IDictionary<string,string> dic = new Dictionary<string, string>();

            var paramString = $"案件类型:{_pageInfo.CaseType}";
            if (!string.IsNullOrEmpty(_pageInfo.ReasonLevel) && !string.IsNullOrEmpty(_pageInfo.ReasonKey))
                paramString = $"{paramString},{_pageInfo.ReasonLevel}:{_pageInfo.ReasonKey}";
            if (!string.IsNullOrEmpty(_pageInfo.CourtLevel) && !string.IsNullOrEmpty(_pageInfo.CourtKey))
                paramString = $"{paramString},{_pageInfo.CourtLevel}:{_pageInfo.CourtKey}";
            if(!string.IsNullOrEmpty(_pageInfo.Year))
                paramString = $"{paramString},裁判年份:{_pageInfo.Year}";
            if (_pageInfo.CourtLevel.Equals("法院地域"))
                paramString = $"{paramString},法院层级:高级法院";

            dic.Add("Param", paramString);
            dic.Add("Page", "20");
            dic.Add("Order", "裁判日期");

            if (_pageInfo.Index <= _pageInfo.MaxIndex)
            {
                dic.Add("Index", $"{_pageInfo.Index}");
                dic.Add("Direction", "asc");
            }
            else
            {
                dic.Add("Index", $"{_pageInfo.Index - _pageInfo.MaxIndex}");
                dic.Add("Direction", "desc");
                
            }

            var postDataCur = WebRequestCtrl.BuildPostDatas(dic, Encoding.UTF8);

            var htmlString = base.GetMainWebContent(HomePage, postDataCur, ref cookies, HomePage);


            const string shielded = "\"remind\"";
            if (!htmlString.Equals(shielded)) return htmlString;
            //等于指定字符串，则处理验证码
            this.LoopHandleValidateCode();

            return base.GetMainWebContent(HomePage, postDataCur, ref cookies, HomePage);
        }

        /// <summary>
        /// GetContent 用于子类
        /// </summary>
        /// <param name="nextUrl"></param>
        /// <param name="postData"></param>
        /// <param name="cookies"></param>
        /// <param name="currentUrl"></param>
        /// <returns></returns>
        public string GetContent(string nextUrl, byte[] postData, ref string cookies, string currentUrl)
        {
            return base.GetMainWebContent(HomePage, postData, ref cookies, HomePage);
        }

        

        /// <summary>
        /// 解析下一页
        /// </summary>
        /// <returns></returns>
        protected override string ParseNextUrl()
        {


            if (_pageInfo.Index < _pageInfo.ActuallyIndex)
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
        /// 解析当前页数
        /// </summary>
        /// <returns></returns>
        protected override int ParseCurrentPage()
        {
            //return _pageInfo.Index - 1;
            return _pageInfo.Index - 1 > _pageInfo.MaxIndex ? _pageInfo.MaxIndex - _pageInfo.Index + 1: _pageInfo.Index -1;
        }

        /// <summary>
        /// 解析总页数
        /// </summary>
        /// <returns></returns>
        protected override int ParseCountPage()
        {
            return _pageInfo.TotalIndex;
        }

        /// <summary>
        /// 得到总共的页数
        /// </summary>
        /// <param name="htmlString">网页内容</param>
        /// <returns></returns>
        public static int GetTotalPages(string htmlString)
        {
            try
            {
                var match = Regex.Match(htmlString, @"(?<=\\""Count\\"":\\"")\d*(?=\\"")");
                //Console.WriteLine($"总共页数：{match.Value}页");
                int total;
                if (int.TryParse(match.Value, out total))
                    return total;
                else
                    return -1;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
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
                //Console.WriteLine($"DocId:{match.Value}");
                docId.Add(match.Value);
            }

            return docId;
        }


        /// <summary>
        /// 处理验证码
        /// </summary>
        public void LoopHandleValidateCode()
        {
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
        }

    }
}
