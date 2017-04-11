using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using X.CommLib.Net.WebRequestHelper;
using X.GlodEyes.Collectors;
using X.GlodEyes.Collectors.Specialized.JingDong;

namespace XFCollection.TaoBao
{
    /// <summary>
    ///
    /// </summary>
    public class JiYouJiaCollector : WebRequestCollector<IResut,NormalParameter>
    {

        private string _targetUid;
        //private readonly HttpHelper _httpHelper = new HttpHelper();


        /// <summary>
        /// 测试
        /// </summary>
        internal static void Test()
        {
            //var parameter = new NormalParameter { Keyword = @"https://qiansifang8.1688.com/" };
            var parameter = new NormalParameter { Keyword = @"https://shop113769528.taobao.com/" };
            //var parameter = new NormalParameter { Keyword = @"https://btjiaju.jiyoujia.com/shop/view_shop.htm" };
            //var parameter = new NormalParameter { Keyword = @"https://88421950.taobao.com" };
            parameter.Add(@"targetUid", "5656");



            //var parameter = new NormalParameter { Keyword = @"http://shop73144325.taobao.com"};

            TestHelp<JiYouJiaCollector>(parameter, 1);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="nextUrl"></param>
        /// <param name="postData"></param>
        /// <param name="cookies"></param>
        /// <param name="currentUrl"></param>
        /// <returns></returns>
        protected override string GetMainWebContent(string nextUrl, byte[] postData, ref string cookies, string currentUrl)
        {

            //return _httpHelper.GetHtmlByGet(nextUrl);

            WebRequestCtrl.GetWebContentParam @default = WebRequestCtrl.GetWebContentParam.Default;
            @default.Refere = currentUrl;
            @default.MaxRedirect = 20;
            cookies = cookies ?? string.Empty;
            return WebRequestCtrl.GetWebContent(nextUrl, postData, ref cookies, 1, @default);
        }

        /// <summary>
        /// 定义第一个链接
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        protected override string InitFirstUrl(NormalParameter param)
        {
            _targetUid = param.GetValue(@"targetUid", string.Empty);
            var shopUrl = param.Keyword;
            if (shopUrl.Equals(""))
                throw new Exception("传入的ShopUrl为空,请检查！");
            return shopUrl;
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
        /// 解析总共页数
        /// </summary>
        /// <returns></returns>
        protected override int ParseCountPage()
        {
            return 1;
        }
        /// <summary>
        /// 
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
        /// 解析当前元素
        /// </summary>
        /// <returns></returns>
        protected override IResut[] ParseCurrentItems()
        {
            var stringEmpty = string.Empty;
            var errorNotice = stringEmpty;
            var bossNickName = stringEmpty;
            var mainIndustry = stringEmpty;
            var Comment_MatchDescrip = stringEmpty;
            var Comment_MatchDescripRate = stringEmpty;
            var Comment_ServiceStatue = stringEmpty;
            var Comment_ServiceStatueRate = stringEmpty;
            var Comment_ShipSpeed = stringEmpty;
            var Comment_ShipSpeedRate = stringEmpty;
            var marginCharge = stringEmpty;
            var shopRank = stringEmpty;
            var location = stringEmpty;
            var saleCount = stringEmpty;
            var productCount = stringEmpty;
            var goodCommentRate = stringEmpty;
            var mainBiz = stringEmpty;
            var intDefault = 0;
            var dateDefault = DateTime.Parse("1990-01-01 00:00:00");

            var resultList = new List<IResut>();
            var shopId = GetShopId(HtmlSource);
            var userId = GetUserId(HtmlSource);
            var shopName = GetShopName(HtmlSource);
            var shopType = GetShopType(HtmlSource);
            if (shopName.Equals(stringEmpty))
            {

                errorNotice = GetErrorNotice(HtmlSource);
                if (errorNotice.Equals(stringEmpty))
                    errorNotice = "不支持的店铺类型";
            }
            else
            {
                marginCharge = GetMarginCharge(HtmlSource);
                //把shopname编码成url中能够识别的编码 不然在url里#这些特殊字符会出错
                var shopNameEncoding = System.Web.HttpUtility.UrlEncode(shopName);
                var url = $"https://shopsearch.taobao.com/search?app=shopsearch&q={shopNameEncoding}";
                var htmlString = base.GetWebContent(url);
                //httpHelper.Cookies = "thw=cn;";
                //var htmlString = _httpHelper.GetHtmlByGet(url);
                //用userId匹配符合的那段 用shopId也可以 
                var tempToken = GetContentJsonStringByUserId(htmlString, userId);
                //var tempTokenString = tempToken.ToString();

                if (tempToken != null)
                {
                    var tempString = tempToken.ToString();
                    bossNickName = tempToken["nick"].ToString();
                    mainIndustry = GetMainIndustry(tempString);
                    Comment_MatchDescrip = GetComment_MatchDescrip(tempString);
                    Comment_MatchDescripRate = GetComment_MatchDescripRate(tempString);
                    Comment_ServiceStatue = GetComment_ServiceStatue(tempString);
                    Comment_ServiceStatueRate = GetComment_ServiceStatueRate(tempString);
                    Comment_ShipSpeed = GetComment_ShipSpeed(tempString);
                    Comment_ShipSpeedRate = GetComment_ShipSpeedRate(tempString);
                    shopRank = tempToken["shopIcon"]["iconClass"].ToString();
                    location = tempToken["provcity"].ToString();
                    saleCount = tempToken["totalsold"].ToString();
                    productCount = tempToken["procnt"].ToString();
                    goodCommentRate = tempToken["goodratePercent"]?.ToString().Replace("%", "");
                    mainBiz = tempToken["mainAuction"].ToString();

                }
                else
                {
                    errorNotice = "店铺存在但搜索不到";
                }
            }





            //System.Func<string, string> GetIntDefault = key => { return string.IsNullOrEmpty(key) ? "0" : key; };


            Resut resut = new Resut
            {
                //店铺ID
                ["ShopId"] = shopId,
                //店铺名
                ["ShopName"] = shopName,
                //店铺名2
                ["ShopName2"] = shopName,
                //旺旺号
                ["BossName"] = bossNickName,
                //旺旺号的昵称
                ["BossNickName"] = bossNickName,
                //公司名称
                ["CompanyName"] = stringEmpty,
                //开店时间
                ["ShopStartDate"] = dateDefault,
                //ShpAgeNum
                ["ShpAgeNum"] = intDefault,
                //采集入口参数
                ["ShopUrl"] = GetUrlFormat(CurrentUrl),
                //好评数
                ["GoodCommentCount"] = intDefault,
                //主营行业
                ["MainIndustry"] = mainIndustry,
                //描述相符
                ["Comment_MatchDescrip"] = GetIntDefault(Comment_MatchDescrip),
                //描述相符率
                ["Comment_MatchDescripRate"] = GetIntDefault(Comment_MatchDescripRate),
                //服务态度
                ["Comment_ServiceStatue"] = GetIntDefault(Comment_ServiceStatue),
                //服务态度率
                ["Comment_ServiceStatueRate"] = GetIntDefault(Comment_ServiceStatueRate),
                //物流服务
                ["Comment_ShipSpeed"] = GetIntDefault(Comment_ShipSpeed),
                //物流服务率
                ["Comment_ShipSpeedRate"] = GetIntDefault(Comment_ShipSpeedRate),
                //保证金
                ["MarginCharge"] = GetIntDefault(marginCharge),
                //店铺等级
                ["ShopRank"] = shopRank,
                //所在位置
                ["Location"] = location,
                //销售数量
                ["SaleCount"] = GetIntDefault(saleCount),
                //产品数量
                ["ProductCount"] = GetIntDefault(productCount),
                //好评率
                ["GoodCommentRate"] = GetIntDefault(goodCommentRate),
                //主营产品
                ["MainBiz"] = mainBiz,
                //店铺类型
                ["DayMonitor"] = shopType,
                ["Loaned"] = intDefault,
                ["targetuid"] = _targetUid,
                //当前店铺状态        
                ["Error_Notice"] = errorNotice
            };

            resultList.Add(resut);
            return resultList.ToArray();
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
        /// 得到链接格式化
        /// </summary>
        /// <returns></returns>
        private string GetUrlFormat(string url)
        {
            var tempurl = Regex.Match(url, "(?<=http[s]?://)[^/]*").Value;
            if (tempurl.Equals(string.Empty))
                return url;
            else
                return tempurl;

        }

        /// <summary>
        /// 得到错误标记
        /// </summary>
        /// <param name="htmlString"></param>
        /// <returns></returns>
        private string GetErrorNotice(string htmlString)
        {
            return Regex.Match(htmlString, @"(?<=<div class=""error-notice-hd"">[\s]*)[\S]*(?=[\s]*</div>)").Value;
        }


        /// <summary>
        /// GetShopType
        /// </summary>
        /// <param name="htmlString"></param>
        /// <returns></returns>
        private string GetShopType(string htmlString)
        {
            //var shopType = "UnKnown";
            var title = Regex.Match(htmlString, @"(?<=<title>)[\s\S]*(?=</title>)").ToString();
            if (title.Contains("天猫Tmall.com"))
            {
                return "4";
            }
            else if (title.Contains("天猫国际"))
            {
                return "8";
            }
            //淘宝 淘宝企业 极有家
            else if (title.Contains("淘宝网"))
            {
                var htmlNode = HtmlAgilityPack.HtmlAgilityPackHelper.GetDocumentNodeByHtml(htmlString);

                //极有家
                var src = htmlNode.SelectSingleNode("//div[@class=\"logos\"]/a/img")?.Attributes["src"]?.Value;
                if (src == "//gdp.alicdn.com/bao/uploaded/i2/TB1ekNeKFXXXXcXXFXXwu0bFXXX")
                {
                    //极有家
                    return "5";
                }
                else if (src == "//img.alicdn.com/tps/TB1M0QCNpXXXXaWXXXXXXXXXXXX-150-45.png")
                {
                    //亲宝贝
                    return "6";
                }

                //淘宝企业
                var taoBaoQiYe = htmlNode.SelectSingleNode("//div[@class=\"shop-type\"]/a")?.Attributes["class"]?.Value;
                if (taoBaoQiYe == "shop-type-icon-enterprise")
                {
                    //淘宝企业
                    return "3";
                }

                //普通淘宝
                return "2";
            }
            else if (title.Contains("阿里旅行·去啊Alitrip.com"))
            {
                //飞猪
                return "7";
            }
            //未知
            else
            {
                return "99";
            }

            //
            //gdp.alicdn.com/bao/uploaded/i2/TB1ekNeKFXXXXcXXFXXwu0bFXXX

        }

        /// <summary>
        /// shopId
        /// </summary>
        /// <param name="htmlString"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        private JToken GetContentJsonStringByUserId(string htmlString, string userId)
        {

            var jsonString = Regex.Match(htmlString, "(?<=g_page_config = ){.*?\"map\":{}}(?=;)").Value;
            var jObject = JObject.Parse(jsonString);
            var jToken = jObject["mods"]?["shoplist"]?["data"]?["shopItems"];
            if (jToken == null)
                throw new Exception("json解析失败，jToken为空。");
            var jArray = JArray.Parse(jToken.ToString());
            foreach (var token in jArray)
            {
                var value = token["uid"].ToString();
                if (value.Equals(userId))
                {
                    return token;
                }
            }
            return null;
        }

        /// <summary>
        /// 得到店铺Id
        /// </summary>
        /// <param name="htmlString"></param>
        /// <returns></returns>
        private string GetShopId(string htmlString)
        {
            return Regex.Match(htmlString, "(?<=shopId=).*?(?=;)").Value;
        }

        /// <summary>
        /// 得到userId
        /// </summary>
        /// <param name="htmlString"></param>
        /// <returns></returns>
        private string GetUserId(string htmlString)
        {
            return Regex.Match(htmlString, "(?<=userId=).*?(?=\")").Value;
        }

        /// <summary>
        /// 得到店铺名字
        /// </summary>
        /// <param name="htmlString"></param>
        /// <returns></returns>
        private string GetShopName(string htmlString)
        {
            //var shopName = Regex.Match(htmlString, "(?<=<span class=\"shop-name-title\".*>).*(?=</span>)").Value;
            //////淘宝普通店铺 有些shop-name被截断
            ////if (string.IsNullOrEmpty(shopName))
            ////{
            ////    shopName = Regex.Match(htmlString, "<a class=\"shop-name\" href=.*?\"><span>.*</span></a>").Value;
            ////    shopName = Regex.Match(shopName, "(?<=<span>).*?(?=</span>)").Value;
            ////}
            ////淘宝普通店铺 用title标签里的
            //if (string.IsNullOrEmpty(shopName))
            //{
            //    shopName = Regex.Match(htmlString, "<title>.*?</title>").Value;
            //    shopName = Regex.Match(shopName, "(?<=<title>.*-).*(?=-.*</title>)").Value;
            //}


            var title = Regex.Match(htmlString, @"(?<=<title>)[\s\S]*(?=</title>)").ToString();
            var shopName = Regex.Match(title, "(?<=-).*(?=-)").Value;
            //特殊情况 没有左边的-
            if (string.IsNullOrEmpty(shopName))
                shopName = Regex.Match(title, ".*(?=-)").ToString().Trim();
            return shopName;
        }

        /// <summary>
        /// 得到卖家名字
        /// </summary>
        /// <param name="htmlString"></param>
        /// <returns></returns>
        private string GetBossNickName(string htmlString)
        {
            return Regex.Match(htmlString, "(?<=\"nick\":\").*?(?=\",\"provcity\")").Value;
        }

        /// <summary>
        /// 得到主营行业
        /// </summary>
        /// <param name="htmlString"></param>
        /// <returns></returns>
        private string GetMainIndustry(string htmlString)
        {
            return Regex.Match(htmlString, @"(?<=\\""ind\\"":[\s]*\\"").*?(?=\\"")").Value;
        }

        /// <summary>
        /// 得到描述相符
        /// </summary>
        /// <param name="htmlString"></param>
        /// <returns></returns>
        private string GetComment_MatchDescrip(string htmlString)
        {
            return Regex.Match(htmlString, @"(?<=\\""mas\\"":[\s]*\\"").*?(?=\\"")").Value;
        }

        /// <summary>
        /// 得到描述相符率
        /// </summary>
        /// <param name="htmlString"></param>
        /// <returns></returns>
        private string GetComment_MatchDescripRate(string htmlString)
        {
            return Regex.Match(htmlString, @"(?<=\\""mg\\"":[\s]*\\"").*?(?=%\\"")").Value;
        }



        /// <summary>
        /// 得到服务态度
        /// </summary>
        /// <param name="htmlString"></param>
        /// <returns></returns>
        private string GetComment_ServiceStatue(string htmlString)
        {
            return Regex.Match(htmlString, @"(?<=\\""sas\\"":[\s]*\\"").*?(?=\\"")").Value;
        }

        /// <summary>
        /// 得到服务态度率
        /// </summary>
        /// <param name="htmlString"></param>
        /// <returns></returns>
        private string GetComment_ServiceStatueRate(string htmlString)
        {
            return Regex.Match(htmlString, @"(?<=\\""sg\\"":[\s]*\\"").*?(?=%\\"")").Value;
        }

        /// <summary>
        /// 得到物流服务
        /// </summary>
        /// <param name="htmlString"></param>
        /// <returns></returns>
        private string GetComment_ShipSpeed(string htmlString)
        {
            return Regex.Match(htmlString, @"(?<=\\""cas\\"":[\s]*\\"").*?(?=\\"")").Value;
        }

        /// <summary>
        /// 得到物流服务率
        /// </summary>
        /// <param name="htmlString"></param>
        /// <returns></returns>
        private string GetComment_ShipSpeedRate(string htmlString)
        {
            return Regex.Match(htmlString, @"(?<=\\""cg\\"":[\s]*\\"").*?(?=%\\"")").Value;
        }


        /// <summary>
        /// 得到保证金
        /// </summary>
        /// <param name="htmlString"></param>
        /// <returns></returns>
        private string GetMarginCharge(string htmlString)
        {
            return Regex.Match(htmlString, "(?<=<span class=\"tb-seller-bail\"><i></i>).*(?=元</span>)").Value;
        }

        /// <summary>
        /// 得到店铺等级
        /// </summary>
        /// <param name="htmlString"></param>
        /// <returns></returns>
        private string GetShopRank(string htmlString)
        {
            return Regex.Match(htmlString, "(?<=\"iconClass\":[\\s]*\").*?(?=\")").Value;
        }

        /// <summary>
        /// 得到所在地
        /// </summary>
        /// <param name="htmlString"></param>
        /// <returns></returns>
        private string GetLocation(string htmlString)
        {
            return Regex.Match(htmlString, "(?<=\"provcity\":[\\s]*\").*?(?=\")").Value;
        }

        /// <summary>
        /// 得到销售数量
        /// </summary>
        /// <param name="htmlString"></param>
        /// <returns></returns>
        private string GetSaleCount(string htmlString)
        {
            return Regex.Match(htmlString, @"(?<=totalsold"":[\s]*)[\S]*?(?=,)").Value;
        }


        /// <summary>
        /// 得到产品数量
        /// </summary>
        /// <param name="htmlString"></param>
        /// <returns></returns>
        private string GetProductCount(string htmlString)
        {
            return Regex.Match(htmlString, @"(?<=""procnt"":[\s*])[\S]*?(?=,)").Value;
        }

        /// <summary>
        /// 得到好评率
        /// </summary>
        /// <param name="htmlString"></param>
        /// <returns></returns>
        private string GetGoodCommentRate(string htmlString)
        {
            return Regex.Match(htmlString, "(?<=\"goodratePercent\":[\\s]*\").*?(?=%\")").Value;
        }

        /// <summary>
        /// 得到主营产品
        /// </summary>
        /// <param name="htmlString"></param>
        /// <returns></returns>
        private string GetMainBiz(string htmlString)
        {
            return Regex.Match(htmlString, "(?<=\"mainAuction\":[\\s]*\").*?(?=\")").Value;
        }


    }
}
