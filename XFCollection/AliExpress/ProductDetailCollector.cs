using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.UI;
using X.CommLib.Miscellaneous;
using X.CommLib.Net.Miscellaneous;
using X.CommLib.Net.WebRequestHelper;
using X.GlodEyes.Collectors;
using X.GlodEyes.Collectors.Specialized.JingDong;

namespace XFCollection.AliExpress
{
    /// <summary>
    /// ProductDetailCollector
    /// </summary>
    public class ProductDetailCollector:WebRequestCollector<IResut,NormalParameter>
    {
        /// <summary>
        /// _url
        /// </summary>
        private string _url;

        /// <summary>
        /// _shopId
        /// </summary>
        private string _shopId;

        /// <summary>
        /// _maxPage
        /// </summary>
        private int _maxPage;

        /// <summary>
        /// _curPage
        /// </summary>
        private int _curPage;

        /// <summary>
        /// _cookies
        /// </summary>
        private string _cookies;

        /// <summary>
        /// _searchQueue
        /// </summary>
        private Queue<string> _searchQueue;

        /// <summary>
        /// ProductDetailCollector
        /// </summary>
        public ProductDetailCollector()
        {
            _curPage = 0;
            _searchQueue = new Queue<string>();
        }


        /// <summary>
        /// GetValueByRegex
        /// </summary>
        /// <param name="regex"></param>
        /// <returns></returns>
        private delegate string GetValueByRegex(string regex);
        /// <summary>
        /// GetValueByHtmlAndRegex
        /// </summary>
        /// <param name="html"></param>
        /// <param name="regex"></param>
        /// <returns></returns>
        private delegate string GetValueByHtmlAndRegex(string html,string regex);

        //private string _productId;
        //private string _wishlistUrl;
        //private string _wishlistHtml;


        internal static void Test()
        {
            var parameter = new NormalParameter()
            {
                //Keyword = "https://www.aliexpress.com/store/product/baby-girls-4pcs-sets-longsleeve-cotton-romper-birthday-dress-baby-girls-vestidos-with-pink-stripe-ruffle/1240676_32262374446.html"
                //Keyword = "https://www.aliexpress.com/item/Nuovo-7-Pollice-pin-LCD-Screen-Display-Per-Digma-Optima-7-21-3G-TT7021PG-tablet-pc/32670234250.html?btsid=28f405a9-9912-4f91-884a-410ae9fc855d&ws_ab_test=searchweb0_0%2Csearchweb201602_2_10066_10065_10068_10084_10083_10080_10082_10081_10060_10061_10062_10056_10055_10054_10059_10099_10078_10079_426_10073_10097_10100_10096_10052_10050_10051_424%2Csearchweb201603_8"
                Keyword = "1240676"

            };

            TestHelp<ProductDetailCollector>(parameter);
        }

        ///// <summary>
        ///// GetWebContent
        ///// </summary>
        ///// <param name="url"></param>
        ///// <param name="cookies"></param>
        ///// <returns></returns>
        //private string GetWebContent(string url,ref string cookies)
        //{
        //    MyHttpHelper myHttpHelper = new MyHttpHelper();
        //    HttpItem httpItem = new HttpItem
        //    {
        //        URL = url,
        //        MaximumAutomaticRedirections = 10,
        //        Timeout = 60000,
        //        Allowautoredirect = false,
        //        UserAgent = "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/55.0.2883.87 Safari/537.36"                
        //    };
        //    if (!string.IsNullOrEmpty(Cookies))
        //        httpItem.Cookie = cookies;
        //    HttpResult httpResult = myHttpHelper.GetHtml(httpItem);
        //    if (string.IsNullOrEmpty(cookies))
        //    {
        //        cookies = httpResult.Cookie;
        //        CookieCollection cookie = httpResult.CookieCollection;
        //    }
        //    return httpResult.Html;
        //}

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
            @default.MaxRedirect = 10;
            @default.TimeOut = 60000;
            cookies = cookies ?? string.Empty;
            string html = string.Empty;
            bool isSuccess = false;


            while (!isSuccess)
            {
                
                        try
                        {
                            html = WebRequestCtrl.GetWebContent(nextUrl, postData, ref cookies, 1, @default);
                            
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.ToString());
                            if (e.ToString().Contains("操作超时") || e.ToString().Contains("操作已超时"))
                            {
                                continue;
                            }

                        }

                        isSuccess = true;
                        

                    }

            return html;
        }




        /// <summary>
        /// Test1
        /// </summary>
        static void Test1()
        {
            var keyWords = File.ReadAllLines(@"C:\Users\Administrator\Desktop\someUrl.txt");

            /*var shopIds = File.ReadAllLines(@"C:\Users\sinoX\Desktop\errorList.txt");*/
            // 去掉字符串前后的"
            keyWords = Array.ConvertAll(keyWords, keyWord => keyWord.Trim('"'));

            foreach (var keyWord in keyWords)
            {
                Console.WriteLine();
                Console.WriteLine($"keyword id: {keyWord}");

                var url = !keyWord.ToLower().Contains("http") ? $"https:{keyWord}" : keyWord;
                url = url.Replace("amp;", "");
                try
                {
                    var parameter = new NormalParameter { Keyword = url };
                    TestHelp<ProductDetailCollector>(parameter);
                }
                catch (NotSupportedException exception)
                {
                    Console.WriteLine($"error: {exception.Message}");
                }
            }
        }

        /// <summary>
        /// InitFirstUrl
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        protected override string InitFirstUrl(NormalParameter param)
        {
            Console.WriteLine(Cookies);
            _shopId = param.Keyword;
            _url = $"https://it.aliexpress.com/store/{_shopId}";
            var cookies1 = string.Empty;
            GetMainWebContent($"https://it.aliexpress.com/store/all-wholesale-products/{_shopId}.html", null,ref cookies1, null);
            //GetWebContent($"https://it.aliexpress.com/store/all-wholesale-products/{_shopId}.html", ref cookies1);
            var cookies2 = string.Empty;
            GetMainWebContent("https://dmtracking2.alibaba.com/",null,ref cookies2, null);
            //GetWebContent("https://dmtracking2.alibaba.com/", ref cookies2);
            //cna=j5++EFDIBHICAXPCq3/i11W+ 24位
            _cookies = $"{cookies1};ali_beacon_id={cookies2};cna={GetCna()}";
            
            InitTotalPage($"https://it.aliexpress.com/store/{_shopId}/search/1.html", _cookies);
            InitSearchQueue();  
            return _searchQueue.Count==0?null:_searchQueue.Dequeue();    
        }

        /// <summary>
        /// ParseCurrentItems
        /// </summary>
        /// <returns></returns>
        protected override IResut[] ParseCurrentItems()
        {
            var urlSet = GetUrlQueueByUrl(CurrentUrl);
            var resultList = new List<IResut>();
            foreach (var url in urlSet)
            {
                //var url1 = "https://www.aliexpress.com/store/product/baby-girls-4pcs-sets-longsleeve-cotton-romper-birthday-dress-baby-girls-vestidos-with-pink-stripe-ruffle/1240676_32262374446.html";
                //var html = GetMainWebContent(url1, null, ref _cookies, null);
                var html = GetMainWebContent(url.Key.ToString(), null,ref _cookies, null);
                //var html = GetWebContent(url.Key.ToString(), ref _cookies);
                //GetValueByRegex getValueByRegex = new GetValueByRegex(GetResultByRegex);
                GetValueByHtmlAndRegex getValueByHtmlAndRegex = new GetValueByHtmlAndRegex(GetResultByHtmlAndRegex);
                var title = getValueByHtmlAndRegex(html, "(?<=<title>).*(?=</title>)").Replace("Aliexpress.com : ","");
                var percentNum = getValueByHtmlAndRegex(html,"(?<=<span class=\"percent-num\">).*?(?=</span>)");
                var ratingsNum = GetInt(getValueByHtmlAndRegex(html,"(?<=<span class=\"rantings-num\">).*?(?=</span>)"));
                var orderNum = GetInt(getValueByHtmlAndRegex(html,"(?<=<span class=\"order-num\" id=\"j-order-num\">).*?(?=</span>)"));
                var discountRage = GetInt(getValueByHtmlAndRegex(html,"(?<=<span class=\"p-discount-rate\">).*?(?=</span>)"));
                var actMinPrice = getValueByHtmlAndRegex(html,"(?<=actMinPrice=\").*?(?=\";)");
                var actMaxPrice = getValueByHtmlAndRegex(html,"(?<=actMaxPrice=\").*?(?=\";)");
                var minPrice = getValueByHtmlAndRegex(html,"(?<=minPrice=\").*?(?=\";)");
                var maxPrice = getValueByHtmlAndRegex(html,"(?<=maxPrice=\").*?(?=\";)");
                var mobileDiscountPrice = GetDouble(getValueByHtmlAndRegex(html,"(?<=mobileDiscountPrice=\").*?(?=\";)"));
                var productId = getValueByHtmlAndRegex(html,"(?<=productId=\").*?(?=\";)");
                var totalAvailQuantity = getValueByHtmlAndRegex(html,@"(?<=totalAvailQuantity=)\d+(?=;)");

                string collectNum = string.Empty;
                if (!string.IsNullOrEmpty(productId))
                {
                    var wishlistUrl = $"https://us.ae.aliexpress.com/wishlist/wishlist_item_count.htm?itemid={productId}";
                    string wishlistHtml =string.Empty;
                    bool isSuccess = false;
                    while (!isSuccess)
                    {
                        try
                        {
                            wishlistHtml = GetMainWebContent(wishlistUrl, null, ref _cookies, null);
                        }
                        catch (Exception e)
                        {

                            if(e.ToString().Contains("操作超时") || e.ToString().Contains("操作已超时"))
                            {
                                continue;
                            }
                        }

                        isSuccess = true;
                    }
                    
                    
                    collectNum = getValueByHtmlAndRegex(wishlistHtml, @"(?<=""num"":)\d+(?=})");
                }

                var eventTimeLeft = Regex.Match(html, "(?<=class=\"p-eventtime-left\").*?(?=</span>)").Value;
                if (eventTimeLeft.Contains("data-hour") || eventTimeLeft.Contains("data-minute") ||
                    eventTimeLeft.Contains("data-second"))
                {
                    var hour = Regex.Match(eventTimeLeft, @"(?<=data-hour="")\d+(?="")").Value;
                    if (hour.Length == 1)
                        hour = $"0{hour}";
                    var minute = Regex.Match(eventTimeLeft, @"(?<=data-minute="")\d+(?="")").Value;
                    if (minute.Length == 1)
                        minute = $"0{minute}";
                    var second = Regex.Match(eventTimeLeft, @"(?<=data-second="")\d+(?="")").Value;
                    if (second.Length == 1)
                        second = $"0{second}";

                    eventTimeLeft = $"{hour}:{minute}:{second}";
                }

                //var dic = new Dictionary<string, string>()
                //{

                //    {"Url",url.Key.ToString() },
                //    { "Title",title},
                //    { "PercentNum",percentNum },
                //    { "RatingsNum",ratingsNum},
                //    { "OrderNum",orderNum },
                //    { "DiscountRage",discountRage },
                //    {"EventTimeLeft",eventTimeLeft },
                //    {"ActMinPrice", actMinPrice},
                //    { "ActMaxPrice",actMaxPrice },
                //    { "MinPrice",minPrice},
                //    { "MaxPrice",maxPrice},
                //    { "MobileDiscountPrice",mobileDiscountPrice },
                //    { "ProductId",productId },
                //    { "TotalAvailQuantity",totalAvailQuantity },
                //    { "CollectNum",collectNum }
                //};

                //DataBaseHelper.MysqlHelper mysqlHelper = new MysqlHelper();
                //mysqlHelper.InsertTable(dic,"AliExpress");



                IResut resut = new Resut()
                {
                    {"Url",url.Key.ToString() },
                    { "Title",title},
                    { "PercentNum",percentNum },
                    { "RatingsNum",ratingsNum},
                    { "OrderNum",orderNum },
                    { "DiscountRage",discountRage },
                    {"EventTimeLeft",eventTimeLeft },
                    {"ActMinPrice", actMinPrice},
                    { "ActMaxPrice",actMaxPrice },
                    { "MinPrice",minPrice},
                    { "MaxPrice",maxPrice},
                    { "MobileDiscountPrice",mobileDiscountPrice },
                    { "ProductId",productId },
                    { "TotalAvailQuantity",totalAvailQuantity },
                    { "CollectNum",collectNum }
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
            return _searchQueue.Count == 0 ? null : _searchQueue.Dequeue();
        }

        /// <summary>
        /// ParseCountPage
        /// </summary>
        /// <returns></returns>
        protected override int ParseCountPage()
        {
            return _maxPage;
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
        /// GetResultByRegex
        /// </summary>
        /// <param name="regex"></param>
        /// <returns></returns>
        private string GetResultByRegex(string regex)
        {
            return Regex.Match(HtmlSource, regex).Value;
        }

        /// <summary>
        /// GetResultByHtmlAndRegex
        /// </summary>
        /// <param name="html"></param>
        /// <param name="regex"></param>
        /// <returns></returns>
        private string GetResultByHtmlAndRegex(string html, string regex)
        {
            return Regex.Match(html, regex).Value;
        }


        /// <summary>
        /// InitTotalPage 需要cookie
        /// </summary>
        /// <param name="url"></param>
        /// <param name="cookies"></param>
        private void InitTotalPage(string url,string cookies)
        {
            
            //url = "https://it.aliexpress.com/store/301635/search/1.html";
            //var html = base.GetWebContent(url);
            
            var html = GetMainWebContent(url, null, ref _cookies, null);
            //var html = GetWebContent(url,ref _cookies);
            var tempString = Regex.Match(html, @"<div class=""ui-pagination-navi util-left"">[\s\S]*?</div>").Value;
            var maxPage = 0;
            if (string.IsNullOrEmpty(tempString))
            {
                _maxPage = 0;
            }
            else
            {
                var tempList = Regex.Matches(tempString, "<a.*?</a>");
                
                foreach (Match temp in tempList)
                {
                    var value = temp.Value;
                    var pageList = Regex.Matches(value, @"(?<=>)\d+(?=</a>)");
                    foreach (var page in pageList)
                    {
                        var pageInt = int.Parse(page.ToString());
                        maxPage = maxPage < pageInt ? pageInt : maxPage;
                    }

                }

            }
            _maxPage = maxPage;
        }

        /// <summary>
        /// InitSearchQueue
        /// </summary>
        private void InitSearchQueue()
        {
            for (var i = 1; i <= _maxPage; i++)
            {
                _searchQueue.Enqueue($"https://it.aliexpress.com/store/{_shopId}/search/{i}.html");
            }
        }



        /// <summary>
        /// GetUrlQueueByUrl
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private HashSet<string> GetUrlQueueByUrl(string url)
        {
            var html = GetMainWebContent(url, null, ref _cookies, null);
            //var html = GetWebContent(url,ref _cookies);
            var htmlTemp = Regex.Match(html, @"<div class=""ui-box-body"">[\s\S]*?<div id=""pagination-bottom""").Value;
            var urlMatchCollection = Regex.Matches(htmlTemp, "(?<=href=\").*store/product.*?(?=\")");
            var urlSet = new HashSet<string>();
            foreach (Match urlMatch in urlMatchCollection)
            {
                var value = GetFormatUrl(urlMatch.Value);
                
                if (!urlSet.ContainsKey(value))
                {
                    urlSet.Add(value);
                }
            }
            return urlSet;

        }

        /// <summary>
        /// GetFormatUrl
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private string GetFormatUrl(string url)
        {
            if (!url.ToLower().Contains("http"))
            {
                return $"https:{url}";
            }
            else
            {
                return url;
            }
        }



        /// <summary>
        /// GetInt
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private string GetInt(string value)
        {
            return Regex.Match(value, @"\d+").Value;
        }

        /// <summary>
        /// GetDouble
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private string GetDouble(string value)
        {
            return Regex.Match(value, @"\d+\.\d+").Value;
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
            char[] array = new[]
            {
                'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u',
                'v', 'w', 'x', 'y', 'z',
                'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U',
                'V', 'W', 'X', 'Y', 'Z',
                '/', '+'
            };
            for (int i = 0; i < 24; i++)
            {
                
                cna += array[random.Next(array.Length)];
            }

            return cna;
        }


        private void Test222()
        {
            _shopId = "1240676";
            var cookies1 = string.Empty;
            //GetMainWebContent($"https://it.aliexpress.com/store/all-wholesale-products/{_shopId}.html", null,ref cookies1, null);
            GetHtmlFromGet($"https://it.aliexpress.com/store/all-wholesale-products/{_shopId}.html",Encoding.UTF8);
            var cookies2 = string.Empty;
            //GetMainWebContent("https://dmtracking2.alibaba.com/",null,ref cookies2, null);
            GetHtmlFromGet("https://dmtracking2.alibaba.com/", Encoding.UTF8);
        }

        /// <summary>
        /// 通过GET方式获取页面的方法
        /// </summary>
        /// <param name="urlString">请求的URL</param>
        /// <param name="encoding">页面编码</param>
        /// <returns></returns>
        private static string GetHtmlFromGet(string urlString, Encoding encoding)
        {
            //定义局部变量
            HttpWebRequest httpWebRequest = null;
            HttpWebResponse httpWebResponse = null;
            Stream stream = null;
            StreamReader streamReader = null;
            var htmlString = string.Empty;


            //请求页面
            try
            {
                
                httpWebRequest = WebRequest.Create(urlString) as HttpWebRequest;
                

            }
            //处理异常
            catch (Exception ex)
            {
                throw new Exception("建立页面请求时发生错误！", ex);
            }
            httpWebRequest.MaximumAutomaticRedirections = 10;
            httpWebRequest.UserAgent =
                "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/55.0.2883.87 Safari/537.36";

            //获取服务器的返回信息
            try
            {
                httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                stream = httpWebResponse.GetResponseStream();
            }
            //处理异常
            catch (Exception ex)
            {
                throw new Exception("接受服务器返回页面时发生错误！", ex);
            }
            streamReader = new StreamReader(stream, encoding);
            //读取返回页面
            try
            {
                htmlString = streamReader.ReadToEnd();
            }
            //处理异常
            catch (Exception ex)
            {
                throw new Exception("读取页面数据时发生错误！", ex);
            }

            var cookies = httpWebResponse.Headers["set-cookie"];
            
            foreach (Cookie cookie in httpWebResponse.Cookies)
            {
                Console.WriteLine(cookie);
            }

            //释放资源返回结果
            streamReader.Close();
            stream.Close();

            return htmlString;

        }

        /// <summary>
        /// 提供通过POST方法获取页面的方法
        /// </summary>
        /// <param name="urlString">请求的URL</param>
        /// <param name="encoding">页面使用的编码</param>
        /// <param name="postDataString">POST数据</param>
        /// <returns></returns>
        private static string GetHtmlFromPost(string urlString, Encoding encoding, string postDataString)
        {
            //定义局部变量
            CookieContainer cookieContainer = new CookieContainer();
            HttpWebRequest httpWebRequest = null;
            HttpWebResponse httpWebResponse = null;
            Stream inputStream = null;
            Stream outputStream = null;
            StreamReader streamReader = null;
            string htmlString = string.Empty;
            //转换POST数据
            byte[] postDataByte = encoding.GetBytes(postDataString);
            //建立页面请求
            try
            {
                httpWebRequest = WebRequest.Create(urlString) as HttpWebRequest;
            }
            //处理异常
            catch (Exception ex)
            {
                throw new Exception("建立页面请求时发生错误！", ex);
            }
            //指定请求处理方式
            httpWebRequest.Method = "POST";
            httpWebRequest.KeepAlive = false;
            httpWebRequest.ContentType = "application/x-www-form-urlencoded";
            httpWebRequest.CookieContainer = cookieContainer;
            httpWebRequest.ContentLength = postDataByte.Length;
            //向服务器传送数据
            try
            {
                inputStream = httpWebRequest.GetRequestStream();
                inputStream.Write(postDataByte, 0, postDataByte.Length);
            }
            //处理异常
            catch (Exception ex)
            {
                throw new Exception("发送POST数据时发生错误！", ex);
            }
            finally
            {
                inputStream.Close();
            }
            //接受服务器返回信息
            try
            {
                httpWebResponse = httpWebRequest.GetResponse() as HttpWebResponse;
                outputStream = httpWebResponse.GetResponseStream();
                streamReader = new StreamReader(outputStream, encoding);
                htmlString = streamReader.ReadToEnd();
            }
            //处理异常
            catch (Exception ex)
            {
                throw new Exception("接受服务器返回页面时发生错误！", ex);
            }
            finally
            {
                streamReader.Close();
            }
            foreach (Cookie cookie in httpWebResponse.Cookies)
            {
                cookieContainer.Add(cookie);
            }
            return htmlString;

        }
    }
}
