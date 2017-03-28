using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml;
using MySql.Data.MySqlClient;
using X.GlodEyes.Collectors;
using X.GlodEyes.Collectors.Specialized.JingDong;
using XFCollection.HtmlAgilityPack;

namespace XFCollection.TaoBao
{
    /// <summary>
    /// ShopProductCollector
    /// </summary>
    public class ShopProductCollector : WebRequestCollector<IResut, NormalParameter>
    {


        private readonly Http.HttpHelper _httpHelper;
        private string _shopUrl;
        private string _shopName;
        private int _totalPage;
        private int _curPage;
        private readonly Queue<string> _urlQueue;
        private string _cookies;
        private Hashtable _hashTable;

        /// <summary>
        /// DefaultMovePageTimeSpan
        /// </summary>
        public override double DefaultMovePageTimeSpan => 0;

        /// <summary>
        /// 测试
        /// </summary>
        internal static void Test()
        {
            var parameter = new NormalParameter()
            {
                Keyword = @"shop35764087.taobao.com"
            };

            TestHelp<ShopProductCollector>(parameter);
        }



        /// <summary>
        /// Test1
        /// </summary>
        public static void Test1()
        {
            var keyWords = File.ReadAllLines(@"C:\Users\Administrator\Desktop\shopUrl.txt");

            foreach (var keyWord in keyWords)
            {
                Thread.Sleep(60 * 1000);
                Console.WriteLine($"休息60s");
                Console.WriteLine($"keyword id:{keyWord}");

                try
                {
                    var parameter = new NormalParameter() { Keyword = keyWord };
                    TestHelp<ShopProductCollector>(parameter);
                }
                catch (NotSupportedException exception)
                {
                    Console.WriteLine($"error:{exception.Message}");
                }

            }
        }



        /// <summary>
        /// 构造函数
        /// </summary>
        public ShopProductCollector()
        {
            _urlQueue = new Queue<string>();
            _httpHelper = new Http.HttpHelper { HttpEncoding = Encoding.Default, MaximumAutomaticRedirections = 50 };
            _hashTable = new Hashtable();
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
        /// InitFirstUrl
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        protected override string InitFirstUrl(NormalParameter param)
        {
            InitUrlQueue(param.Keyword);
            _curPage = 0;
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
            return ++_curPage;
        }



        /// <summary>
        /// ParseCurrentItems
        /// </summary>
        /// <returns></returns>
        protected override IResut[] ParseCurrentItems()
        {
            var resultList = new List<IResut>();

            Newtonsoft.Json.Serialization.Func<string, string> getFormatProductId = productId => Regex.Match(productId, @"(?<=\\"").*(?=\\"")").Value;
            Newtonsoft.Json.Serialization.Func<string, string> getFormatProductName = productName => productName.Trim();
            Newtonsoft.Json.Serialization.Func<string, string> getFormatProductUrl = productUrl => $"https:{Regex.Match(productUrl, @"(?<=\\"").*(?=\\"")").Value}";




            //var html = Regex.Match(HtmlSource, @"<div class=\\""J_TItems\\"">[\s\S]*?<div class=\\""pagination\\"">").Value;

            //var docmentNode = HtmlAgilityPackHelper.GetDocumentNodeByHtml(html);

            //var htmlNodeCollection = docmentNode.SelectNodes(@"//div[@class='\""item4line1\""']//dl") ??
            //             docmentNode.SelectNodes(@"//div[@class='\""item5line1\""']//dl")?? 
            //             docmentNode.SelectNodes(@"//div[@class='\""item30line1\""']//dl");




            //var divNodes = docmentNode.SelectNodes(@"//div");
            //Console.WriteLine(new string('=', 64));
            //foreach (var divNode in divNodes)
            //{
            //    var classValue = divNode.GetAttributeValue(@"class", string.Empty);
            //    Console.WriteLine($"classvalue: {classValue}");
            //}
            //Console.WriteLine(new string('-', 64));

            //用matches和ends-with都提示需要命名空间管理器或 XsltContext。此查询具有前缀、变量或用户定义的函数。还没解决这个问题
            //var htmlNodeCollection = docmentNode.SelectNodes(@"//div[matches(@class,'\""item\d+line1\""')]//dl");
            //var htmlNodeCollection = docmentNode.SelectNodes("//div[starts-with(@class,'\\\"item')]//dl");
            //var htmlNodeCollection = docmentNode.SelectNodes("//div[ends-with(@class,'line1\\\"')]//dl");



            var docmentNode = HtmlAgilityPackHelper.GetDocumentNodeByHtml(HtmlSource);

            var htmlNodeCollection = docmentNode.SelectNodes(@"//div[@class='\""pagination\""']/parent::div/child::div")
                                    ?? docmentNode.SelectNodes(@"//div[@class='\""comboHd\""']/parent::div/child::div");

            //var htmlNodeCollection = docmentNode.SelectNodes(@"//div[contains(@class,'\""item') and contains(@class,'line1\""')]//dl");

            foreach (var htmlNode in htmlNodeCollection)
            {
                var attributes = htmlNode.Attributes["class"].Value;
                //退出 后面的推荐产品不要了
                if (attributes == @"\""pagination\""")
                    break;
                if (attributes == @"\""comboHd\""")
                {
                    //清空队列
                    _urlQueue.Clear();
                    break;
                }
                if (attributes.Contains(@"\""item") && attributes.Contains(@"line1\"""))
                {
                    var htmlNodeDls = htmlNode.SelectNodes(".//dl");
                    foreach (var htmlNodeDl in htmlNodeDls)
                    {
                        var productId = getFormatProductId(htmlNodeDl.Attributes["data-id"].Value);
                        //如果hash表不包含的productId
                        if (!_hashTable.ContainsKey(productId))
                        {
                            //productId加入到hash表中
                            _hashTable.Add(productId, null);
                            var detailNode =
                                htmlNodeDl.SelectSingleNode(@".//dd[@class='\""detail\""']//a[@class='\""item-name']");
                            var productName = getFormatProductName(detailNode.InnerText);
                            var productUrl = getFormatProductUrl(detailNode.Attributes["href"].Value);
                            //Console.WriteLine($"shopId:{productId},shopName:{productName},productUrl:{productUrl}。");

                            var resut = new Resut()
                            {
                                ["productId"] = productId,
                                ["productName"] = productName,
                                ["productUrl"] = productUrl,
                                ["shopId"] = _shopUrl,
                                ["shopName"] = _shopName
                            };

                            resultList.Add(resut);
                        }
                    }

                }
                //ProductId
                //PrdouctName
                //ProductUrl
                //ShopId
                //ShopName
            }

            return resultList.ToArray();

        }





        /// <summary>
        /// ParseNextUrl
        /// </summary>
        /// <returns></returns>
        protected override string ParseNextUrl()
        {
            return _urlQueue.Count == 0 ? null : _urlQueue.Dequeue();
        }




        /// <summary>
        /// InitFirstUrlPart
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private string InitFirstUrlPart(string url)
        {
            if (!url.ToLower().Contains("http"))
                url = $"https://{url}";

            var mainHtml = GetMainWebContent(_shopUrl = url, null, ref _cookies, null);
            _shopName = Regex.Match(mainHtml, @"(?<=<title>)[\s\S]*?(?=</title>)").Value.Trim();
            if (_shopName.Contains("阿里旅行·去啊Alitrip.com"))
                throw new Exception("阿里旅行不支持");
            if (_shopName.Equals("店铺浏览-淘宝网"))
            {
                //throw new Exception("店铺不存在！");
                SendLog("店铺不存在！");
                return string.Empty;
            }
            _shopName = Regex.Match(_shopName, "(?<=-).*(?=-)").Value.Trim();
            //var categoryUrl = $"{url}/category.htm";
            var categoryUrl = $"{url}/search.htm";
            var html = GetMainWebContent(categoryUrl, null, ref _cookies, null);
            var documentNode = HtmlAgilityPackHelper.GetDocumentNodeByHtml(html);
            var listUrl = documentNode.SelectSingleNode("//input[@id='J_ShopAsynSearchURL']").Attributes["value"].Value;
            return $"{url}{listUrl}";
        }


        /// <summary>
        /// InitUrlQueue
        /// </summary>
        /// <param name="url"></param>
        private void InitUrlQueue(string url)
        {
            var firstUrlPart = InitFirstUrlPart(url);
            if (string.IsNullOrEmpty(firstUrlPart))
                return;
            var listHtml = GetListHtml(firstUrlPart);
            InitTotalPage(listHtml);
            if (_totalPage == 0)
                return;
            InitUrlQueue(firstUrlPart, _totalPage);
        }

        /// <summary>
        /// InitUrlQueue
        /// </summary>
        /// <param name="firstUrlPart"></param>
        /// <param name="totalPage"></param>
        private void InitUrlQueue(string firstUrlPart, int totalPage)
        {
            for (var i = 1; i <= totalPage; i++)
            {
                //var curUrl = i == 1 ? firstUrlPart : $"{firstUrlPart}&pageNo={i}";
                var curUrl = $"{firstUrlPart}&pageNo={i}";
                _urlQueue.Enqueue(curUrl);
            }
        }


        /// <summary>
        /// GetListHtml
        /// </summary>
        /// <param name="listUrl"></param>
        /// <returns></returns>
        private string GetListHtml(string listUrl)
        {
            return GetMainWebContent(listUrl, null, ref _cookies, null);
        }



        /// <summary>
        /// InitTotalPage
        /// </summary>
        /// <param name="listHtmlFirst"></param>
        /// <returns></returns>
        private int InitTotalPage(string listHtmlFirst)
        {
            var documentNode = HtmlAgilityPackHelper.GetDocumentNodeByHtml(listHtmlFirst);
            //不转义，双引号里面要两个双引号才表示双引号
            var htmlNode = documentNode.SelectSingleNode(@"//span[@class='\""page-info\""']") ?? documentNode.SelectSingleNode(@"//b[@class='\""ui-page-s-len\""']");
            if (htmlNode == null)
                return 0;
            var text = htmlNode.InnerText;
            var pageNum = Regex.Match(text, @"(?<=/)\d+").Value;
            int pageNumInt;
            return _totalPage = int.TryParse(pageNum, out pageNumInt) ? pageNumInt : 0;

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
            int i = 1;
            while (i <= 5)
            {
                try
                {
                    HtmlSource = _httpHelper.GetHtmlByGet(nextUrl);
                    return HtmlSource;
                }
                catch (Exception e)
                {
                    i++;
                    Console.WriteLine(e.Message);
                    Console.WriteLine($"休息1分钟重试");
                    Thread.Sleep(60 * 1000);
                }
            }
            throw new Exception("尝试超过5次");
        }
    }
}
