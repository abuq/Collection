using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Newtonsoft.Json.Linq;
using NUnit.Framework.Constraints;
using X.CommLib.Net.WebRequestHelper;
using X.GlodEyes.Collectors;
using X.GlodEyes.Collectors.Specialized.JingDong;
using XFCollection.Http;

namespace XFCollection.TaoBao
{
    /// <summary>
    /// 诚信通采集引擎
    /// </summary>
    public class AlitrustCollector : WebRequestCollector<IResut, NormalParameter>
    {

        private string _targetUid;
        private string _url;
        /// <summary>
        /// 默认30秒
        /// </summary>
        public override double DefaultMovePageTimeSpan { get; } = 30;

        internal static void Test()
        {
            var parameter = new NormalParameter { Keyword = @"http://shop1464825016821.1688.com" };
            //var parameter = new NormalParameter { Keyword = @"https://shop1397148954914.1688.com/" };
            //var parameter = new NormalParameter { Keyword = @"https://shop1423106386435.1688.com" };
            parameter.Add(@"targetUid", "5656");
            TestHelp<AlitrustCollector>(parameter, 1);
        }


        /// <summary>
        /// Test1
        /// </summary>
        /// <param name="args"></param>
        static void Test1(string[] args)
        {
            var keyWords = File.ReadAllLines(@"C:\Users\Administrator\Desktop\shopId.txt");

            /*var shopIds = File.ReadAllLines(@"C:\Users\sinoX\Desktop\errorList.txt");*/
            // 去掉字符串前后的"
            keyWords = Array.ConvertAll(keyWords, keyWord => keyWord.Trim('"'));

            foreach (var keyWord in keyWords)
            {
                Console.WriteLine();
                Console.WriteLine($"keyword id: {keyWord}");
                try
                {
                    var parameter = new NormalParameter { Keyword = $"https://{keyWord}/" };
                    TestHelp<AlitrustCollector>(parameter);
                }
                catch (NotSupportedException exception)
                {
                    Console.WriteLine($"error: {exception.Message}");
                }
            }
        }


        /// <summary>
        /// 定义第一个链接
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        protected override string InitFirstUrl(NormalParameter param)
        {
            _targetUid = param.GetValue(@"targetUid", string.Empty);
            _url = $"{param.Keyword}";
            if (string.IsNullOrEmpty(_url))
                throw new Exception("传入的ShopUrl为空或null,请检查！");
            //if (!_url.Contains("http"))
            //    _url = $"https://{_url}";
            return _url;
        }

        /// <summary>
        /// 解析下一个链接
        /// </summary>
        /// <returns></returns>
        protected override string ParseNextUrl()
        {
            return null;
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
        /// 解析总页数
        /// </summary>
        /// <returns></returns>
        protected override int ParseCountPage()
        {
            return 1;
        }

        /// <summary>
        /// GetMainWebContent
        /// </summary>
        /// <param name="nextUrl"></param>
        /// <param name="postData"></param>
        /// <param name="cookies"></param>
        /// <param name="currentUrl"></param>
        /// <returns></returns>
        protected override string GetMainWebContent(string nextUrl, byte[] postData, ref string cookies, string currentUrl)
        {
            WebRequestCtrl.GetWebContentParam @default = WebRequestCtrl.GetWebContentParam.Default;
            @default.Refere = currentUrl;
            @default.MaxRedirect = 20;
            cookies = cookies ?? string.Empty;
            return WebRequestCtrl.GetWebContent(nextUrl, postData, ref cookies, 1, @default);
        }



        /// <summary>
        /// 得到链接格式化
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private string GetUrlFormat(string url)
        {
            var tempurl = Regex.Match(url, "(?<=http[s]?://)[^/]*").Value;
            return tempurl.Equals(string.Empty) ? url : tempurl;
        }


        /// <summary>
        /// 解析当前项目
        /// </summary>
        /// <returns></returns>
        protected override IResut[] ParseCurrentItems()
        {
            var resultList = new List<IResut>();

            var shopId = GetShopId(HtmlSource);
            var shopName = GetShopName(HtmlSource);
            if (shopName.Equals("1688.com,阿里巴巴打造的全球最大的采购批发平台"))
            {
                SendLog("发现被屏蔽，暂停30s");
                Thread.Sleep(30000);
                throw new Exception("被屏蔽了!");
            }
            var shopName2 = GetShopName2(HtmlSource);
            var bossNickName = GetBossName(HtmlSource);
            var shpAgeNum = GetShpAgeNum(HtmlSource);
            shpAgeNum = GetIntDefault(shpAgeNum);
            var mainBiz = GetMainBiz(HtmlSource);
            var location = GetLocation(HtmlSource);
            var productCount = GetProductCount(HtmlSource);
            var shopRank = GetShopRank(HtmlSource);
            //var dicComment = GetComment(HtmlSource);

            IDictionary<string, string> dicComment = !shopName.Equals("旺铺关闭页面-未达到") ? GetComment() : new Dictionary<string, string>();


            var stringEmpty = string.Empty;
            var intDefault = 0;
            var dateDefault = DateTime.Parse("1990-01-01 00:00:00");
            //if (int.Parse(shpAgeNum) != 0)
            //{
            //    dateDefault = DateTime.Now.AddYears(-int.Parse(shpAgeNum));
            //}

            //旺铺关闭页面-未达到
            var errorNotice = stringEmpty;
            if (shopName.Equals("违规下架"))
                errorNotice = "违规下架";
            else if (shopName.Equals("旺铺关闭页面-未达到"))
                errorNotice = "旺铺关闭页面-未达到";


            var resut = new Resut
            {
                //店铺ID
                ["ShopId"] = shopId,
                //店铺名
                ["ShopName"] = shopName,
                //店铺名2
                ["ShopName2"] = shopName2,
                //旺旺号
                ["BossName"] = bossNickName,
                //旺旺号的昵称
                ["BossNickName"] = bossNickName,
                //公司名称
                ["CompanyName"] = shopName,
                //开店时间
                ["ShopStartDate"] = dateDefault,
                //ShpAgeNum
                ["ShpAgeNum"] = shpAgeNum,
                //采集入口参数
                ["ShopUrl"] = GetUrlFormat(CurrentUrl),
                //好评数
                ["GoodCommentCount"] = intDefault,
                //主营行业
                ["MainIndustry"] = stringEmpty,
                //描述相符
                ["Comment_MatchDescrip"] = intDefault,
                //描述相符率
                ["Comment_MatchDescripRate"] = dicComment.ContainsKey("Comment_MatchDescripRate") ? GetIntDefault(dicComment["Comment_MatchDescripRate"]) : "0",
                //服务态度
                ["Comment_ServiceStatue"] = intDefault,
                //服务态度率
                ["Comment_ServiceStatueRate"] = dicComment.ContainsKey("Comment_ServiceStatueRate") ? GetIntDefault(dicComment["Comment_ServiceStatueRate"]) : "0",
                //物流服务
                ["Comment_ShipSpeed"] = intDefault,
                //物流服务率
                ["Comment_ShipSpeedRate"] = dicComment.ContainsKey("Comment_ShipSpeedRate") ? GetIntDefault(dicComment["Comment_ShipSpeedRate"]) : "0",
                //保证金
                ["MarginCharge"] = intDefault,
                //店铺等级
                ["ShopRank"] = shopRank,
                //所在位置
                ["Location"] = location,
                //销售数量
                ["SaleCount"] = intDefault,
                //产品数量
                ["ProductCount"] = GetIntDefault(productCount),
                //好评率
                ["GoodCommentRate"] = intDefault,
                //主营产品
                ["MainBiz"] = mainBiz,
                ["DayMonitor"] = intDefault,
                ["Loaned"] = intDefault,
                ["targetuid"] = stringEmpty,
                //当前店铺状态
                ["Error_Notice"] = errorNotice
            };

            resultList.Add(resut);
            return resultList.ToArray();


        }

        /// <summary>
        /// moveNext
        /// </summary>
        /// <returns></returns>
        public override bool MoveNext()
        {
            var result = base.MoveNext();

            var current = Current;
            if (current != null)
                Array.ForEach(current, item =>
                {
                    item.Remove(@"SearchPageIndex");
                    item.Remove(@"SearchPageRank");
                });

            return result;
        }

        /// <summary>
        /// 得到缺省的值
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string GetIntDefault(string key)
        {
            return string.IsNullOrEmpty(key) ? "0" : key;
        }

        /// <summary>
        /// 得到ShopId
        /// </summary>
        /// <param name="htmlString"></param>
        /// <returns></returns>
        private string GetShopId(string htmlString)
        {
            return Regex.Match(htmlString, "(?<=<meta name=\"mobile-agent\" content=\"format=html5;url= //m.1688.com/winport/).*(?=.html\">)").Value;
        }

        /// <summary>
        /// 得到店铺名称
        /// </summary>
        /// <param name="htmlString"></param>
        /// <returns></returns>
        private string GetShopName(string htmlString)
        {
            return Regex.Match(htmlString, "(?<=<title>).*(?=</title>)").Value;
        }


        private string GetShopName2(string htmlString)
        {
            string result = Regex.Match(htmlString, "(?<=&quot;)实力商家供应商信息(?=&quot;)").Value;
            return result.Equals("实力商家供应商信息") ? "实力商家供应商信息" : "供应商信息";
        }

        /// <summary>
        /// 得到BossName
        /// </summary>
        /// <param name="htmlString"></param>
        /// <returns></returns>
        private string GetBossName(string htmlString)
        {
            return Regex.Match(htmlString, "(?<={name:\").*(?=\"})").Value;
        }

        /// <summary>
        /// 得到shpAgeNum
        /// </summary>
        /// <param name="htmlString"></param>
        /// <returns></returns>
        private string GetShpAgeNum(string htmlString)
        {
            var year = Regex.Match(htmlString, "(?<=<span class=\"tp-year\"[ ]?>).*(?=</span>)").Value;
            int result;
            if (!int.TryParse(year, out result))
                year = Regex.Match(year, @"(?<=>)\d+(?=<)").Value;
            return year;
        }

        /// <summary>
        /// 得到主营内容
        /// </summary>
        /// <param name="htmlString"></param>
        /// <returns></returns>
        private string GetMainBiz(string htmlString)
        {
            return Regex.Match(htmlString, "(?<=meta name=\"keywords\" content=\".*[,，]).*(?=\" />)").Value;
        }


        /// <summary>
        /// 得到居住地
        /// </summary>
        /// <param name="htmlString"></param>
        /// <returns></returns>
        private string GetLocation(string htmlString)
        {
            return Regex.Match(htmlString, "(?<=<span class=\"disc\">).*(?=</span>)").Value;
        }

        /// <summary>
        /// 得到产品数
        /// </summary>
        /// <param name="htmlString"></param>
        /// <returns></returns>
        private string GetProductCount(string htmlString)
        {
            var productCount = Regex.Match(htmlString, "(?<=data-tracelog=\"wp_widget_supplierinfo_wplist\">).*(?=条</a>)").Value;
            if (string.IsNullOrEmpty(productCount))
            {
                var cookies = $"cna={GetCna()}";
                var html = GetMainWebContent($"{_url}/page/offerlist.htm", null, ref cookies, null);
                productCount = Regex.Match(html, @"(?<=<em class=""offer-count"">)\d+(?=</em>)").Value;
                if (string.IsNullOrEmpty(productCount))
                {
                    var navigator = X.CommLib.Net.Miscellaneous.HtmlDocumentHelper.CreateNavigator(html);
                    var items = navigator.Select(@"//div[@class='wp-offerlist-windows']//div[@class='image']");
                    return $"{items.Count}";



                    ////不满一页 看图片数
                    //var imageCount = Regex.Matches(html, "<div class=\"image\">").Count;
                    //var imageWrapCount = Regex.Matches(html, @"<div class=""image-wrap"">[\s\S]*?<div class=""image"">").Count;
                    //var count = imageCount - imageWrapCount;
                    //productCount = count >= 0 ? count.ToString() : "-1";
                }
            }

            return productCount;
        }

        /// <summary>
        /// 得到评论
        /// </summary>
        /// <param name="htmlString"></param>
        /// <returns></returns>
        private Dictionary<string, string> GetComment(string htmlString)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();


            var Comment_MatchDescripRate = Regex.Match(htmlString, "<span class=\"product-description-value.*?\">.*?</span>").Value;
            var Comment_MatchDescripRateNum = GetFormatRate(Comment_MatchDescripRate).Trim();
            dic.Add("Comment_MatchDescripRate", Comment_MatchDescripRate.Contains("lower") ? $"-{Comment_MatchDescripRateNum}" : $"+{Comment_MatchDescripRateNum}");


            var Comment_ServiceStatueRate = Regex.Match(htmlString, "<span class=\"shopper-response-value.*?\">.*?</span>").Value;
            var Comment_ServiceStatueRateNum = GetFormatRate(Comment_ServiceStatueRate).Trim();
            dic.Add("Comment_ServiceStatueRate", Comment_ServiceStatueRate.Contains("lower") ? $"-{Comment_ServiceStatueRateNum}" : $"+{Comment_ServiceStatueRateNum}");

            var Comment_ShipSpeedRate = Regex.Match(htmlString, "<span class=\"sell-product-value.*?\">.*?</span>").Value;
            var Comment_ShipSpeedRateNum = GetFormatRate(Comment_ShipSpeedRate);

            dic.Add("Comment_ShipSpeedRate", Comment_ShipSpeedRate.Contains("lower") ? $"-{Comment_ShipSpeedRateNum}" : $"+{Comment_ShipSpeedRateNum}");

            return dic;
        }

        /// <summary>
        /// GetComment
        /// </summary>
        /// <returns></returns>
        private IDictionary<string, string> GetComment()
        {

            var dic = new Dictionary<string, string>();
            var url = CurrentUrl.EndsWith("/") ? $"{CurrentUrl}event/app/winport_bsr/getBsrData.htm" : $"{CurrentUrl}/event/app/winport_bsr/getBsrData.htm";
            //var url = "https://shop1397148954914.1688.com/event/app/winport_bsr/getBsrData.htm";
            var httpHelper = new HttpHelper();
            var html = httpHelper.GetHtmlByPost(url, $"site_id=winport&page_type=index");
            var jArray = JArray.Parse(JObject.Parse(html)["result"]["bsrDataList"].ToString());

            var Comment_MatchDescripRate = jArray[0]?["compareLineRate"]?.ToString().Replace("%", string.Empty);
            if (Comment_MatchDescripRate == "-1")
            {
                Comment_MatchDescripRate = string.Empty;
            }
            else if (Comment_MatchDescripRate == "0")
            {
                Comment_MatchDescripRate = "0";
            }
            else
            {
                var compareTag = int.Parse(jArray[0]?["compareTag"]?.ToString() ?? "0");
                Comment_MatchDescripRate = compareTag == -1
                    ? $"-{Comment_MatchDescripRate}"
                    : $"+{Comment_MatchDescripRate}";
            }


            var Comment_ServiceStatueRate = jArray[1]?["compareLineRate"]?.ToString().Replace("%", string.Empty);
            if (Comment_ServiceStatueRate == "-1")
            {
                Comment_ServiceStatueRate = string.Empty;
            }
            else if (Comment_ServiceStatueRate == "0")
            {
                Comment_ServiceStatueRate = "0";
            }
            else
            {
                var compareTag = int.Parse(jArray[1]?["compareTag"]?.ToString() ?? "0");
                Comment_ServiceStatueRate = compareTag == -1 ? $"-{Comment_ServiceStatueRate}" : $"+{Comment_ServiceStatueRate}";
            }

            var Comment_ShipSpeedRate = jArray[2]?["compareLineRate"]?.ToString().Replace("%", string.Empty);
            if (Comment_ShipSpeedRate == "-1")
            {
                Comment_ShipSpeedRate = string.Empty;
            }
            else if (Comment_ShipSpeedRate == "0")
            {
                Comment_ShipSpeedRate = "0";
            }
            else
            {
                var compareTag = int.Parse(jArray[2]?["compareTag"]?.ToString() ?? "0");
                Comment_ShipSpeedRate = compareTag == -1 ? $"-{Comment_ShipSpeedRate}" : $"+{Comment_ShipSpeedRate}";
            }



            dic.Add("Comment_MatchDescripRate", Comment_MatchDescripRate);
            dic.Add("Comment_ServiceStatueRate", Comment_ServiceStatueRate);
            dic.Add("Comment_ShipSpeedRate", Comment_ShipSpeedRate);

            return dic;
        }

        private string GetFormatRate(string rate)
        {
            var formatRate = Regex.Match(rate, "(?<=>)[^%]*(?=%?<)").Value.Trim();
            double rateInt;
            return double.TryParse(formatRate, out rateInt) ? formatRate : "0";

        }


        /// <summary>
        /// 得到店铺等级
        /// </summary>
        /// <returns></returns>
        public string GetShopRank(string htmlString)
        {

            var matchs = Regex.Matches(htmlString, "(?<=<img src=\").*(?=\" alt=\"供应等级\")");
            int cur = 0;
            string imgSrc = String.Empty;
            foreach (Match match in matchs)
            {
                imgSrc = match.Value;
                cur++;
            }
            if (imgSrc.Contains("2422944_1490276829.png"))
                return $"sale_{cur}_diamond";
            else if (imgSrc.Contains("2421892_1490276829.png"))
                return $"sale_{cur}_star";
            else if (imgSrc.Contains("2423877_1490276829.png"))
                return $"sale_{cur}_royal";
            else
                return $"{imgSrc}+{cur}";
        }


        /// <summary>
        /// GetCna
        /// </summary>
        /// <returns></returns>
        private string GetCna()
        {
            var cna = string.Empty;
            var random = new Random();
            //abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ
            var array = new[]
            {
                'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u',
                'v', 'w', 'x', 'y', 'z',
                'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U',
                'V', 'W', 'X', 'Y', 'Z',
                '/', '+'
            };
            for (var i = 0; i < 24; i++)
            {

                cna += array[random.Next(array.Length)];
            }

            return cna;
        }


    }
}
