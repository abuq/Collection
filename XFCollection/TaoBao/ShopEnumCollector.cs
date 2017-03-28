using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using X.GlodEyes.Collectors;
using X.GlodEyes.Collectors.Specialized.JingDong;
using XFCollection.Http;

namespace XFCollection.TaoBao
{
    /// <summary>
    /// ShopeEnumCollector
    /// </summary>
    public class ShopEnumCollector:WebRequestCollector<IResut,ShopEnumParameter>
    {
        /// <summary>
        /// ShopTypeEnum
        /// </summary>
        private enum ShopTypeEnum
        {
            TaoBao,
            TaoBaoJin,
            TaoBaoHuang,
            TaoBaoZhuan,
            TaoBaoXin,
            Tmall,
            QuanQiuGou,
            Buxian
        };

        private readonly HttpHelper _httpHelper;
        private string _q;
        private ShopTypeEnum _shopType;
        private int _searchPage;
        private string _partUrl;
        private readonly Queue<string> _urlQueue;
        private int _totalPage;
        private int _curPage;
        


        /// <summary>
        /// ShopEnumCollector
        /// </summary>
        public ShopEnumCollector()
        {
            _httpHelper = new HttpHelper();
            _urlQueue = new Queue<string>();
        }

        /// <summary>
        /// Test
        /// </summary>
        internal static void Test()
        {
            var parameter = new ShopEnumParameter()
            {
                KeyWord = "citylife 女包 正品",
                ShopType = "Tmall",
                SearchPage = "5"
            };

            TestHelp<ShopEnumCollector>(parameter);

        }

        /// <summary>
        /// InitFirstUrl
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        protected override string InitFirstUrl(ShopEnumParameter param)
        {
            _shopType = (ShopTypeEnum) Enum.Parse(typeof(ShopTypeEnum), param.ShopType);
            _q = param.KeyWord;
            if(!int.TryParse(param.SearchPage,out _searchPage))
                throw new Exception("搜索值转换为字符串失败。");
            _partUrl = GetFirstUrl(_shopType, _q);
            var html = _httpHelper.GetHtmlByGet(_partUrl);
            InitUrlQueue(html);
            _curPage = 0;
            return _urlQueue.Count == 0 ? null : _urlQueue.Dequeue();
        }

        /// <summary>
        /// ParseCurrentItems
        /// </summary>
        /// <returns></returns>
        protected override IResut[] ParseCurrentItems()
        {
            var jObject = JObject.Parse(GetPageConfig(HtmlSource));
            var dicList = GetInfoDicList(jObject);

            var resultList = new List<IResut>();

            foreach (var dic in dicList)
            {
                IResut resut = new Resut()
                {
                    ["ShopId"] = dic["ShopId"],
                    ["ShopName"] = dic["ShopName"],
                    ["ShopUrl"] = dic["ShopUrl"],
                    ["ShopLogoUrl"] = dic["ShopLogoUrl"],
                    ["MarketName"] = dic["MarketName"],
                    ["ShopRank"] = dic["ShopRank"],
                    ["Location"] = dic["Location"],
                    ["InCountry"] = dic["InCountry"],
                    ["InProvince"] = dic["InProvince"],
                    ["InCity"] = dic["InCity"],
                    ["BossNickName"] = dic["BossNickName"],
                    ["EncryptedUserId"] = dic["EncryptedUserId"],
                    ["MainBiz"] = dic["MainBiz"],
                    ["MainIndustry"] = dic["MainIndustry"],
                    ["SaleCount"] = dic["SaleCount"],
                    ["ProductCount"] = dic["ProductCount"],
                    ["GoodCommentCount"] = dic["GoodCommentCount"],
                    ["GoodCommentRate"] = dic["GoodCommentRate"],
                    ["Comment_MatchDescrip"] = dic["Comment_MatchDescrip"],
                    ["Comment_MatchDescripRate"] = dic["Comment_MatchDescripRate"],
                    ["Comment_ServiceStatue"] = dic["Comment_ServiceStatue"],
                    ["Comment_ServiceStatueRate"] = dic["Comment_ServiceStatueRate"],
                    ["Comment_ShipSpeed"] = dic["Comment_ShipSpeed"],
                    ["Comment_ShipSpeedRate"] = dic["Comment_ShipSpeedRate"],
                    ["Attribute_BuyProtect"] = dic["Attribute_BuyProtect"],
                    ["Attribute_GlobalBuy"] = dic["Attribute_GlobalBuy"],
                    ["Attribute_GoldenSale"] = dic["Attribute_GoldenSale"],
                    ["SearchKeyword"] = dic["SearchKeyword"]
                };

                resultList.Add(resut);

            }

            return resultList.ToArray();
        }

        /// <summary>
        /// ParseNextUrl
        /// </summary>
        /// <returns></returns>
        protected override string ParseNextUrl()
        {
            _curPage++;
            return _urlQueue.Count == 0 ? null : _urlQueue.Dequeue();
        }

        /// <summary>
        /// ParseCountPage
        /// </summary>
        /// <returns></returns>
        protected override int ParseCountPage()
        {
            return _totalPage;
        }

        /// <summary>
        /// ParseCurrentPage
        /// </summary>
        /// <returns></returns>
        protected override int ParseCurrentPage()
        {
            return _curPage;
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
            return HtmlSource = _httpHelper.GetHtmlByGet(nextUrl);
        }

        /// <summary>
        /// GetFirstUrl
        /// </summary>
        /// <param name="shopType"></param>
        /// <param name="q"></param>
        /// <returns></returns>
        private string GetFirstUrl(ShopTypeEnum shopType,string q)
        {
            
            var url = $"https://shopsearch.taobao.com/search?app=shopsearch&q={System.Web.HttpUtility.UrlEncode(q)}";
            var isb = string.Empty;
            var shop_type = string.Empty;
            var ratesum = string.Empty;
            switch (shopType)
            {
                case ShopTypeEnum.TaoBao:
                    isb = "0";
                    break;
                case  ShopTypeEnum.TaoBaoHuang:
                    isb = "0";
                    ratesum = "huang";
                    break;
                case ShopTypeEnum.TaoBaoJin:
                    isb = "0";
                    ratesum = "jin";
                    break;
                case ShopTypeEnum.TaoBaoZhuan:
                    isb = "0";
                    ratesum = "zhuan";
                    break;
                case ShopTypeEnum.TaoBaoXin:
                    isb = "0";
                    ratesum = "xin";
                    break;
                case ShopTypeEnum.Tmall:
                    isb = "1";
                    break;
                case ShopTypeEnum.QuanQiuGou:
                    shop_type = "2";
                    break;
                case ShopTypeEnum.Buxian:
                    break;
            }

            return $"{url}&isb={isb}&shop_type={shop_type}&ratesum={ratesum}";
        }


        /// <summary>
        /// GetPageConfig
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        private string GetPageConfig(string html)
        {
            return Regex.Match(html, "(?<=g_page_config = ).*(?=;)").Value;
        }


        /// <summary>
        /// InitUrlQueue
        /// </summary>
        /// <param name="html"></param>
        private void InitUrlQueue(string html)
        {
            var pageConfig = JObject.Parse(GetPageConfig(html));
            if (!int.TryParse(pageConfig["mods"]?["pager"]?["data"]?["totalPage"]?.ToString()
                              ?? pageConfig["mods"]?["sortbar"]?["data"]?["pager"]?["totalPage"].ToString()
                , out _totalPage))
            {
                //throw new Exception("总页数解析失败。");
                //要求返回 不报错
                SendLog("没有页数");
                return;
            }
            var page = _totalPage >= _searchPage ? _searchPage : _totalPage;

            for (var i = 0; i < page; i++)
            {
                _urlQueue.Enqueue($"{_partUrl}&s={20*i}");
            }
        }

        /// <summary>
        /// GetInfoDicList
        /// </summary>
        /// <param name="jObject"></param>
        /// <returns></returns>
        private List<Dictionary<string, string>> GetInfoDicList(JObject jObject)
        {
            var shopItems = jObject["mods"]?["shoplist"]?["data"]?["shopItems"]?.ToString();
            if (shopItems == null)
                return new List<Dictionary<string, string>>();
            var jArray = JArray.Parse(shopItems);
            var dicList = new List<Dictionary<string,string>>();

            foreach (var jToken in jArray)
            {
                var dsrInfoToken = jToken["dsrInfo"]?["dsrStr"]?.ToString();
                JObject dsrInfo = null;
                if (dsrInfoToken!=null)
                    dsrInfo = JObject.Parse(jToken["dsrInfo"]?["dsrStr"]?.ToString());

                var iconsArray = JArray.Parse(jToken["icons"]?.ToString());
                var dic = new Dictionary<string,string>()
                {
                    ["ShopId"] = jToken["shopUrl"]?.ToString().Replace("/",""),
                    ["ShopName"] = jToken["title"]?.ToString(),
                    ["ShopUrl"] = jToken["shopUrl"]?.ToString(),
                    ["ShopLogoUrl"] = jToken["picUrl"]?.ToString(),
                    ["MarketName"] = iconsArray.Count==0 ? string.Empty: iconsArray[0]?["title"]?.ToString(),
                    ["ShopRank"] = jToken["shopIcon"]?["iconClass"]?.ToString(),
                    ["Location"] = jToken["provcity"]?.ToString(),
                    ["InCountry"] = string.Empty,
                    ["InProvince"] = string.Empty,
                    ["InCity"] = string.Empty,
                    ["BossNickName"] = jToken["nick"]?.ToString(),
                    ["EncryptedUserId"] = dsrInfo==null?string.Empty:dsrInfo["encryptedUserId"]?.ToString(),
                    ["MainBiz"] = jToken["mainAuction"]?.ToString(),
                    ["MainIndustry"] = dsrInfo == null ? string.Empty : dsrInfo["ind"]?.ToString(),
                    ["SaleCount"] = jToken["totalsold"]?.ToString(),
                    ["ProductCount"] = jToken["procnt"]?.ToString(),
                    ["GoodCommentCount"] = dsrInfo == null ? string.Empty : dsrInfo["srn"]?.ToString(),
                    ["GoodCommentRate"] = RemovePerSign(dsrInfo == null ? string.Empty : dsrInfo["sgr"]?.ToString()),
                    ["Comment_MatchDescrip"] = dsrInfo == null ? string.Empty : dsrInfo["mas"]?.ToString(),
                    ["Comment_MatchDescripRate"] = RemovePerSign(dsrInfo == null ? string.Empty : dsrInfo["mg"]?.ToString()),
                    ["Comment_ServiceStatue"] = dsrInfo == null ? string.Empty : dsrInfo["sas"]?.ToString(),
                    ["Comment_ServiceStatueRate"] = RemovePerSign(dsrInfo == null ? string.Empty : dsrInfo["sg"]?.ToString()),
                    ["Comment_ShipSpeed"] = dsrInfo == null ? string.Empty : dsrInfo["cas"]?.ToString(),
                    ["Comment_ShipSpeedRate"] = RemovePerSign(dsrInfo == null ? string.Empty : dsrInfo["cg"]?.ToString()),
                    ["Attribute_BuyProtect"] = string.Empty,
                    ["Attribute_GlobalBuy"] = string.Empty,
                    ["Attribute_GoldenSale"] = string.Empty,
                    ["SearchKeyword"] = _q
                };
                dicList.Add(dic);
            }
                      
            return dicList;
        }


        /// <summary>
        /// RemovePerSign
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private string RemovePerSign(string value)
        {
            return value.Replace("%", "");
        }

        /// <summary>
        /// UpdateResultRankInfo
        /// </summary>
        /// <param name="items"></param>
        /// <param name="page"></param>
        protected override void UpdateResultRankInfo(IResut[] items, int page)
        {
            base.UpdateResultRankInfo(items, page);
            foreach (var item in items)
            {
                item.Remove("SearchPageIndex");
                item.Remove("SearchPageRank");
            }
        }
    }
}
