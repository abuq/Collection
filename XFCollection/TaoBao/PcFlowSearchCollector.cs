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
    /// PcFlowSearchCollector
    /// </summary>
    public class PcFlowSearchCollector : WebRequestCollector<IResut, NormalParameter>
    {

        private Queue<string> _dataUrlQueue;
        private HttpHelper _httpHelper;
        private string _urlPart = "https://s.taobao.com/search?q=";
        private string _q;
        private int _totalPage;
        private int _curPage;

        /// <summary>
        /// 构造函数
        /// </summary>
        public PcFlowSearchCollector()
        {
            _dataUrlQueue = new Queue<string>();
            _httpHelper = new HttpHelper();
        }

        internal static void Test()
        {
            var parameter = new NormalParameter()
            {
                Keyword = @"表"
            };

            TestHelp<PcFlowSearchCollector>(parameter);
        }

        /// <summary>
        /// InitFirstUrl
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        protected override string InitFirstUrl(NormalParameter param)
        {
            _q = param.Keyword;
            var url = $"{_urlPart}{System.Web.HttpUtility.UrlEncode(_q)}";
            _httpHelper.Cookies = $"thw=cn;cna={GetCna()}";
            var html = _httpHelper.GetHtmlByGet(url);
            _totalPage = GetTotalPage(html);
            _curPage = 0;
            InitDataUrlQueue();

            return _dataUrlQueue.Count == 0 ? null : _dataUrlQueue.Dequeue();
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
                    {"searchKeyword",dic["searchKeyword"]},
                    {"productId",dic["productId"]},
                    {"productName",dic["productName"]},
                    {"productPrice",dic["productPrice"]},
                    {"PaymentAcount",dic["PaymentAcount"]},
                    {"productType",dic["productType"]},
                    {"shopName",dic["shopName"]},
                    {"userMemberId",dic["userMemberId"]},
                    {"location",dic["location"]},
                    {"productPosition",dic["productPosition"]},
                    {"positionType",dic["positionType"]},
                    {"pageIndex",dic["pageIndex"]}
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
            return _dataUrlQueue.Count == 0 ? null : _dataUrlQueue.Dequeue();
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
        /// ParseCountPage
        /// </summary>
        /// <returns></returns>
        protected override int ParseCountPage()
        {
            return _totalPage;
        }

        /// <summary>
        /// GetPageConfig
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        private string GetPageConfig(string html)
        {
            return Regex.Match(html, @"(?<=g_page_config = ).*(?=;[\s]*g_srp_loadCss)").Value;
        }

        /// <summary>
        /// GetTotalPage
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        private int GetTotalPage(string html)
        {
            var jObject = JObject.Parse(GetPageConfig(html));
            int totalPage;
            return int.TryParse(jObject["mods"]["pager"]["data"]["totalPage"].ToString(), out totalPage) ? totalPage : -1;
        }

        /// <summary>
        /// InitDataUrlQueue
        /// </summary>
        private void InitDataUrlQueue()
        {
            _totalPage = _totalPage < 3 ? _totalPage : 3;
            for (var i = 0; i < _totalPage; i++)
            {
                _dataUrlQueue.Enqueue($"{_urlPart}{_q}&s={44 * i}");
            }
        }

        /// <summary>
        /// GetInfoDicList
        /// </summary>
        /// <param name="jObject"></param>
        /// <returns></returns>
        private List<Dictionary<string, string>> GetInfoDicList(JObject jObject)
        {
            var dicList = new List<Dictionary<string, string>>();
            var jArray = JArray.Parse(jObject["mods"]["itemlist"]["data"]["auctions"].ToString());
            var productListIndex = 1;
            var p4pProductListIndex = 1;
            foreach (var jToken in jArray)
            {
                var p4p = jToken["p4p"]?.ToString() ?? string.Empty;
                var dic = new Dictionary<string, string>()
                {
                    {"searchKeyword",_q},
                    {"productId",jToken["nid"].ToString()},
                    {"productName",FormatProductName(jToken["title"].ToString())},
                    {"productPrice",FormatProductPrice(jToken["view_price"].ToString())},
                    {"PaymentAcount",Regex.Match(jToken["view_sales"].ToString(),@"\d+").Value},
                    {"productType",jToken["category"].ToString()},
                    {"shopName",jToken["nick"].ToString()},
                    {"userMemberId",jToken["user_id"].ToString()},
                    {"location",jToken["item_loc"].ToString()},
                    {"productPosition",string.IsNullOrEmpty(p4p)?productListIndex++.ToString():p4pProductListIndex++.ToString()},
                    {"positionType",string.IsNullOrEmpty(p4p) ? "ProductList" : "p4pProductList"},
                    {"pageIndex",_curPage.ToString()}
                };

                dicList.Add(dic);
            }


            if (_curPage == 1)
            {
                var customizedconfigUrl = jObject["mods"]["itemlist"]["data"]["customizedconfig"]["url"].ToString();
                customizedconfigUrl = $"https://s.taobao.com{customizedconfigUrl}";
                var html = _httpHelper.GetHtmlByGet(customizedconfigUrl);
                jArray = JArray.Parse(JObject.Parse(html)["API.CustomizedApi"]["itemlist"]["auctions"].ToString());
                foreach (var jToken in jArray)
                {
                    var p4p = jToken["p4p"]?.ToString() ?? string.Empty;
                    var dic = new Dictionary<string, string>()
                    {
                        {"searchKeyword",_q},
                        {"productId",jToken["nid"].ToString()},
                        {"productName",FormatProductName(jToken["title"].ToString())},
                        {"productPrice",FormatProductPrice(jToken["view_price"].ToString())},
                        {"PaymentAcount",Regex.Match(jToken["view_sales"].ToString(),@"\d+").Value},
                        {"productType",jToken["category"].ToString()},
                        {"shopName",jToken["nick"].ToString()},
                        {"userMemberId",jToken["user_id"].ToString()},
                        {"location",jToken["item_loc"].ToString()},
                        {"productPosition",string.IsNullOrEmpty(p4p)?productListIndex++.ToString():p4pProductListIndex++.ToString()},
                        {"positionType",string.IsNullOrEmpty(p4p) ? "ProductList" : "p4pProductList"},
                        {"pageIndex",_curPage.ToString()}
                    };

                    dicList.Add(dic);
                }
            }

            var p4pdata = JToken.Parse(jObject["mods"]["p4p"]["data"]["p4pdata"].ToString());
            var bottomArray = JArray.Parse(p4pdata["bottom"]["data"]["ds1"].ToString());
            var p4pBottomProductListIndex = 1;
            foreach (var jToken in bottomArray)
            {
                var dic = new Dictionary<string, string>()
                {
                    {"searchKeyword",_q},
                    {"productId",jToken["RESOURCEID"].ToString()},
                    {"productName",jToken["TITLE"].ToString()},
                    {"productPrice",FormatProductPrice(jToken["SALEPRICE"].ToString())},
                    {"PaymentAcount",jToken["SELL"].ToString()},
                    {"productType",jToken["GRADE"].ToString()},
                    {"shopName",jToken["SHOPNAME"].ToString()},
                    {"userMemberId",jToken["SELLERID"].ToString()},
                    {"location",jToken["LOCATION"].ToString()},
                    {"productPosition",p4pBottomProductListIndex++.ToString()},
                    {"positionType","p4pBottomProductList"},
                    {"pageIndex",_curPage.ToString()}
                };

                dicList.Add(dic);
            }


            var rightArray = JArray.Parse(p4pdata["right"]["data"]["ds1"].ToString());
            var p4pRightProductListIndex = 1;
            foreach (var jToken in rightArray)
            {
                var dic = new Dictionary<string, string>()
                {
                    {"searchKeyword",_q},
                    {"productId",jToken["RESOURCEID"].ToString()},
                    {"productName",jToken["TITLE"].ToString()},
                    {"productPrice",FormatProductPrice(jToken["SALEPRICE"].ToString())},
                    {"PaymentAcount",jToken["SELL"].ToString()},
                    {"productType",jToken["GRADE"].ToString()},
                    {"shopName",jToken["SHOPNAME"].ToString()},
                    {"userMemberId",jToken["SELLERID"].ToString()},
                    {"location",jToken["LOCATION"].ToString()},
                    {"productPosition",p4pRightProductListIndex++.ToString()},
                    {"positionType","p4pRightProductList"},
                    {"pageIndex",_curPage.ToString()}
                };

                dicList.Add(dic);
            }


            return dicList;
        }

        /// <summary>
        /// FormatProductName
        /// </summary>
        /// <param name="productName"></param>
        /// <returns></returns>
        private string FormatProductName(string productName)
        {
            return Regex.Replace(productName, "</?span.*?>", "");
        }

        /// <summary>
        /// FormatProductPrice
        /// </summary>
        /// <param name="productPrice"></param>
        /// <returns></returns>
        private string FormatProductPrice(string productPrice)
        {
            return string.IsNullOrEmpty(productPrice) ? "0" : productPrice;
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
            _httpHelper.Cookies = $"thw=cn;cna={GetCna()}";
            return HtmlSource = _httpHelper.GetHtmlByGet(nextUrl);
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
                'V', 'W', 'X', 'Y', 'Z'
            };
            for (var i = 0; i < 24; i++)
            {

                cna += array[random.Next(array.Length)];
            }

            return cna;
        }


    }
}
