using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using mshtml;
using Newtonsoft.Json.Linq;
using X.GlodEyes.Collectors;
using X.GlodEyes.Collectors.Specialized.JingDong;
using System.IO;

namespace XFCollection.TaoBao
{

    /// <summary>
    /// MSearchCollector
    /// </summary>
    public class MSearchCollector:WebRequestCollector<IResut, NormalParameter>
    {

        private string _q;
        private string _homePage = "https://s.m.taobao.com/h5?event_submit_do_new_search_auction=1&_input_charset=utf-8&topSearch=1&atype=b&searchfrom=1&action=home:redirect_app_action&from=1&sst=1&n=20&buying=buyitnow&q=";
        private Queue<string> _dataUrlQueue;
        private int _maxPage;

        ///// <summary>
        ///// Main
        ///// </summary>
        ///// <param name="args"></param>
        //static void Main(string[] args)
        //{
        //    var keyWords = File.ReadAllLines(@"C:\Users\Administrator\Desktop\key.txt");

        //    /*var shopIds = File.ReadAllLines(@"C:\Users\sinoX\Desktop\errorList.txt");*/
        //    // 去掉字符串前后的"
        //    keyWords = Array.ConvertAll(keyWords, keyWord => keyWord.Trim('"'));

        //    foreach (var keyWord in keyWords)
        //    {
        //        Console.WriteLine();
        //        Console.WriteLine($"keyword id: {keyWord}");
        //        try
        //        {
        //            var parameter = new NormalParameter { Keyword = keyWord };
        //            TestHelp<MSearchCollector>(parameter);
        //        }
        //        catch (NotSupportedException exception)
        //        {
        //            Console.WriteLine($"error: {exception.Message}");
        //        }
        //    }
        //}

        /// <summary>
        /// Test1
        /// </summary>
        public static void Test1()
        {
            var keyWords = File.ReadAllLines(@"C:\Users\Administrator\Desktop\key.txt");

            /*var shopIds = File.ReadAllLines(@"C:\Users\sinoX\Desktop\errorList.txt");*/
            // 去掉字符串前后的"
            keyWords = Array.ConvertAll(keyWords, keyWord => keyWord.Trim('"'));

            foreach (var keyWord in keyWords)
            {
                Console.WriteLine();
                Console.WriteLine($"keyword id: {keyWord}");
                try
                {
                    var parameter = new NormalParameter { Keyword = keyWord };
                    TestHelp<MSearchCollector>(parameter);
                }
                catch (NotSupportedException exception)
                {
                    Console.WriteLine($"error: {exception.Message}");
                }
            }
        }
        /// <summary>
        /// 测试
        /// </summary>
        internal static void Test()
        {
            var parameter = new NormalParameter()
            {
                //Keyword = @"鱼具椅"
                //Keyword = @"13岁大童女装夏装"
                Keyword = @"冲击钻 家用"
                //Keyword =  "巅峰"

            };

            //parameter["MaxPage"] = 3;


            TestHelp<MSearchCollector>(parameter);
        }




        /// <summary>
        /// 构造函数
        /// </summary>
        public MSearchCollector()
        {
            _dataUrlQueue = new Queue<string>();
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
        /// InitDataUrlQueue
        /// </summary>
        /// <param name="url"></param>
        private void InitDataUrlQueue(string url)
        {


            var html = base.GetWebContent(url);

            var event_submit_do_new_search_auction = 1;
            var _input_charset = "utf-8";
            var topSearch = 1;
            var atype = "b";
            var searchfrom = 1;
            var action = "home:redirect_app_action";
            var from = 1;
            var q = _q;
            var sst = 1;
            var n = 20;
            var buying = "buyitnow";
            
            var abtest = Regex.Match(html, @"(?<=abtest:')\d+(?=')").Value;
            var wlsort = Regex.Match(html, @"(?<=wlsort:')\d+(?=')").Value;
            var m = Regex.Match(html, @"(?<=m:')\w+(?=')").Value;

            //传入null或者空 得到总页数
            if (_maxPage == -1)
            {
                var curUrl =
                    $"https://s.m.taobao.com/search?event_submit_do_new_search_auction={event_submit_do_new_search_auction}&_input_charset={_input_charset}&topSearch={topSearch}&atype={atype}&searchfrom={searchfrom}&action={action}&from={from}&q={q}&sst={sst}&n={n}&buying={buying}&m={m}&abtest={abtest}&wlsort={wlsort}&page=1";
                var curHtml = base.GetWebContent(curUrl);
                try
                {
                    //违禁词
                    _maxPage = int.Parse(Regex.Match(curHtml, @"(?<={""totalPage"":"")\d+(?="")").Value);
                }
                catch (Exception e)
                {
                    var searchQuery = Regex.Match(curHtml, "(?<=search query:).*?(?=\")").Value;
                    if(!string.IsNullOrEmpty(searchQuery))
                        throw new Exception(searchQuery);
                    else
                        throw new Exception(e.Message);
                }
            }

            var i = 1;
            while (i <= _maxPage)
            {
                var page = i;

                var curUrl = 
                    $"https://s.m.taobao.com/search?event_submit_do_new_search_auction={event_submit_do_new_search_auction}&_input_charset={_input_charset}&topSearch={topSearch}&atype={atype}&searchfrom={searchfrom}&action={action}&from={from}&q={q}&sst={sst}&n={n}&buying={buying}&m={m}&abtest={abtest}&wlsort={wlsort}&page={i}";


                // 第一次的时候先查 看总页数 比5小的取解析出来的页数 不然取5页
                if (i == 1)
                {
                    
                    var curHtml = base.GetWebContent(curUrl);
                    var totalPage = int.Parse(Regex.Match(curHtml, @"(?<={""totalPage"":"")\d+(?="")").Value);
                    CurrentPage = 0;
                    CountPage = _maxPage = _maxPage > totalPage ? totalPage : _maxPage;
                }

                

                _dataUrlQueue.Enqueue(curUrl);

                i++;
            }


            


        }


        /// <summary>
        /// 为第一页作初始化准备
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        protected override string InitFirstUrl(NormalParameter param)
        {
            _q = param.Keyword;
            var url = $"{_homePage}{param.Keyword}";
            _maxPage = param.GetValue("MaxPage", -1);

//            if (!int.TryParse(param["MaxPage"].ToString(), out _maxPage))
//            {
//                _maxPage = -1;
//            }

            InitDataUrlQueue(url);


            return _dataUrlQueue.Count != 0 ? _dataUrlQueue.Dequeue() : null;
        }


        /// <summary>
        /// ParseCurrentItems
        /// </summary>
        /// <returns></returns>
        protected override IResut[] ParseCurrentItems()
        {
            var resultList = new List<IResut>();
            var itemList = GetItemList(CurrentUrl);

            var stringEmpty = string.Empty;
            var SearchKeyword = _q;
            var ShopName = stringEmpty;
            var UserMemberId = stringEmpty;


            var index = 1;
            foreach (var item in itemList)
            {
                var dic = GetInformationByJToken(item);


                IResut resut = new Resut()
                {
                    {"SearchKeyword", SearchKeyword},
                    {"ProductId", dic["ProductId"] },
                    {"PositionType",dic["PositionType"] },
                    {"PageIndex",CurrentPage },
                    {"ProductPosition",index.ToString() }

                };

                //IResut resut = new Resut()
                //{
                //    {"SearchKeyword", SearchKeyword},
                //    {"ProductId", dic["ProductId"] },
                //    {"ProductName", dic["ProductName"]},
                //    {"ProductPrice", dic["ProductPrice"]},
                //    {"PaymentAcount", dic["PaymentAcount"]},
                //    {"ProductType", dic["ProductType"]},
                //    {"ShopName", ShopName},
                //    {"UserMemberId",UserMemberId },
                //    {"Location",dic["Location"]},
                //    {"PositionType",dic["PositionType"] },
                //    {"PageIndex",CurrentPage },
                //    {"ProductPosition",index.ToString() }

                //};

                resultList.Add(resut);
                index ++;
            }


            return resultList.ToArray();

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
            //Keyword = @"鱼具椅" 解决报错未处理的控制字符错误
            return base.GetMainWebContent(nextUrl, postData, ref cookies, null);
        }


        /// <summary>
        /// GetItemList
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private List<JToken> GetItemList(string url)
        {
            var itemList = new List<string>();

            if (!HtmlSource.Trim().StartsWith("{"))
            {
                return new List<JToken>();
            }

            var jObject = JObject.Parse(HtmlSource);
            var jToken = jObject["listItem"];
            //var jArray = JArray.Parse(jObject["listItem"].ToString());
            if (jToken == null)
            {
                return new List<JToken>();
            }

            return new List<JToken>(jToken.Values<JToken>()); 
        }


        /// <summary>
        /// GetInformationByJToken
        /// </summary>
        /// <param name="jToken"></param>
        /// <returns></returns>
        private Dictionary<string, string> GetInformationByJToken(JToken jToken)
        {


            var ProductId = jToken["item_id"].ToString();
            var PositionType = jToken["isP4p"].ToString().Equals("false") ? "ProductList" : "p4pProductList";

            var dic = new Dictionary<string, string>
            {
                { "ProductId",ProductId },
                { "PositionType",PositionType}
            };


            //var ProductId = jToken["item_id"].ToString();
            //var ProductName = jToken["name"].ToString();
            //var ProductPrice = jToken["price"].ToString();
            //var PaymentAccount = jToken["sold"].ToString();
            ////框架里为int类型 需要适应框架的数据类型
            //PaymentAccount = PaymentAccount == string.Empty ? "0" : PaymentAccount;
            //var ProductType = jToken["url"].ToString().Contains("tmall") ? "Tmall" : "Taobao";
            //var Location = jToken["location"].ToString();
            //var PositionType = jToken["isP4p"].ToString().Equals("false") ? "ProductList" : "p4pProductList";

            //var dic = new Dictionary<string,string>
            //{
            //    { "ProductId",ProductId },
            //    { "ProductName",ProductName },
            //    { "ProductPrice", ProductPrice },
            //    { "PaymentAcount",PaymentAccount },
            //    { "ProductType",ProductType },
            //    { "Location",Location },
            //    { "PositionType",PositionType}
            //};

            return dic;

        }

        /// <summary>
        /// ParseNextUrl
        /// </summary>
        /// <returns></returns>
        protected override string ParseNextUrl()
        {
            return _dataUrlQueue.Count != 0 ? _dataUrlQueue.Dequeue() : null;
        }


        /// <summary>
        /// ParseCurrentPage
        /// </summary>
        /// <returns></returns>
        protected override int ParseCurrentPage()
        {
            CurrentPage = CurrentPage + 1;
            return CurrentPage;
        }


        /// <summary>
        /// ParseCountPage
        /// </summary>
        /// <returns></returns>
        protected override int ParseCountPage()
        {
            return CountPage;
        }



    }
}
