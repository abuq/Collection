using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using Newtonsoft.Json.Linq;
using X.GlodEyes.Collectors;
using X.GlodEyes.Collectors.Specialized.JingDong;

namespace XFCollection.TaoBao
{
    /// <summary>
    /// Class TejiaCollectors.
    /// </summary>
    public class TejiaCollectors: WebRequestCollector<IResut, ActivityParameter>
    {
        private enum AcvivityEnum
        {
            TeJia,
            TeJiaTen,
            TejiaJinRiBaoKuan,
            TejiaTeHuiTunNew,
            TaoJinBi
        }


        /// <summary>
        /// The _count page
        /// </summary>
        private int _countPage;
        /// <summary>
        /// The _acvivity enum
        /// </summary>
        private AcvivityEnum _acvivityEnum;
        /// <summary>
        /// The _data URL queue
        /// </summary>
        private Queue<string> _dataUrlQueue;
        private string _homePage;

        /// <summary>
        /// 测试
        /// </summary>
        internal static void Test()
        {
            var parameter = new ActivityParameter()
            {
                Url = @"http://tejia.taobao.com"
            };


            TestHelp<TejiaCollectors>(parameter);
        }




        /// <summary>
        /// Initializes a new instance of the <see cref="TejiaCollectors"/> class.
        /// </summary>
        public TejiaCollectors()
        {
            _dataUrlQueue = new Queue<string>();
        }

        /// <summary>
        /// 为第一页作初始化准备
        /// </summary>
        /// <param name="param">The parameter.</param>
        /// <returns>System.String.</returns>
        /// <exception cref="System.Exception">url链接错误。</exception>
        protected override string InitFirstUrl(ActivityParameter param)
        {
            var url = param.Url;
            if (url.Equals("http://tejia.taobao.com"))
                _acvivityEnum = AcvivityEnum.TeJia;
            else if (url.Equals("http://tejia.taobao.com/ten.htm"))
                _acvivityEnum = AcvivityEnum.TeJiaTen;
            else if (url.Equals("http://tejia.taobao.com/jinribaokuan.htm"))
                _acvivityEnum = AcvivityEnum.TejiaJinRiBaoKuan;
            else if (url.Equals("http://www.taobao.com/markets/tejia/tehuitunnew"))
                _acvivityEnum = AcvivityEnum.TejiaTeHuiTunNew;
            else if(url.Equals("https://taojinbi.taobao.com"))
                _acvivityEnum = AcvivityEnum.TaoJinBi;
            else
                throw new Exception("url链接错误。");

            if (_acvivityEnum.ToString().Equals("TeJia")
                || _acvivityEnum.ToString().Equals("TeJiaTen")
                || _acvivityEnum.ToString().Equals("TejiaJinRiBaoKuan"))
            {
                _homePage = "http://tejia.taobao.com";
            }
            else if(_acvivityEnum.ToString().Equals("TejiaTeHuiTunNew"))
            {
                _homePage = "http://www.taobao.com/markets/tejia/tehuitunnew";
            }
            else if (_acvivityEnum.ToString().Equals("TaoJinBi"))
            {
                _homePage = "https://taojinbi.taobao.com";
            }

            GetDataUrlQueue(_homePage);

            return _dataUrlQueue.Count != 0 ? _dataUrlQueue.Dequeue() : null;

        }


        /// <summary>
        /// GetDataUrlQueue step 3
        /// </summary>
        private void GetDataUrlQueue(string url)
        {
            var html = base.GetWebContent(url);

            InitDataUrlQueue(html);
            
            _countPage = _dataUrlQueue.Count;
        }


        /// <summary>
        /// Initializes the data URL queue.
        /// </summary>
        /// <param name="html">The HTML.</param>
        private void InitDataUrlQueue(string html)
        {

            if (_acvivityEnum.ToString().Equals("TeJia"))
            {
                var curRow = 0;
                var totalPage = 1;
                for (int i = 0; i < totalPage; i++)
                {
                    var appId = Regex.Match(html, "(?<=data-app-id=\").*?(?=\")").Value;
                    var blockId = Regex.Match(html, @"(?<=data-cat-id="").*?(?=""[\s\S]*全部商品</em>)").Value;
                    DateTime dateTime = new DateTime();
                    var start = new DateTime(1970, 1, 1, 0, 0, 0, dateTime.Kind);
                    var t = Convert.ToInt64((DateTime.Now - start).TotalSeconds);
                    var _input_charset = "utf-8";
                    var sort = "";
                    var pageSize = 100;
                    var bucketId = 1;
                    var extQuery = "";
                    var thirdQuery = "";
                    var viewId = System.Guid.NewGuid().ToString();
                    var requestId = System.Guid.NewGuid().ToString();
                    var _ksTS = $"{t}_{Random.Next(1000, 9999)}";
                    var startRow = curRow;
                    var url = $@"http://zhi.taobao.com/json/fantomasItems.htm?t={t}&_input_charset={_input_charset}&sort={sort}&appId={appId}&blockId={blockId}&pageSize={pageSize}&bucketId={bucketId}&startRow={startRow}&extQuery={extQuery}&thirdQuery={thirdQuery}&viewId={viewId}&requestId={requestId}&ksTS={_ksTS}";
                    _dataUrlQueue.Enqueue(url);

                    if (curRow == 0)
                    {
                        var json = base.GetWebContent(url);
                        var totalItem = int.Parse(Regex.Match(json, "(?<=totalItem\": \").*?(?=\")").Value);
                        totalPage = totalItem % pageSize == 0 ? totalItem / pageSize : totalItem / pageSize + 1;
                    }

                    curRow += pageSize;

                }
            }
            //和TeJia只有pageSize不一样
            else if (_acvivityEnum.ToString().Equals("TeJiaTen"))
            {
                var curRow = 0;
                var totalPage = 1;
                for (int i = 0; i < totalPage; i++)
                {
                    var appId = Regex.Match(html, @"(?<=data-app-id="").*?(?=""[\s\S]*alt=""更多10元包邮"")").Value;
                    var blockId = Regex.Match(html, @"(?<=data-block-id="").*?(?=""[\s\S]*alt=""更多10元包邮"")").Value;
                    DateTime dateTime = new DateTime();
                    var start = new DateTime(1970, 1, 1, 0, 0, 0, dateTime.Kind);
                    var t = Convert.ToInt64((DateTime.Now - start).TotalSeconds);
                    var _input_charset = "utf-8";
                    var sort = "";
                    var pageSize = 102;
                    var bucketId = 1;
                    var extQuery = "";
                    var thirdQuery = "";
                    var viewId = System.Guid.NewGuid().ToString();
                    var requestId = System.Guid.NewGuid().ToString();
                    var _ksTS = $"{t}_{Random.Next(1000, 9999)}";
                    var startRow = curRow;
                    var url = $@"http://zhi.taobao.com/json/fantomasItems.htm?t={t}&_input_charset={_input_charset}&sort={sort}&appId={appId}&blockId={blockId}&pageSize={pageSize}&bucketId={bucketId}&startRow={startRow}&extQuery={extQuery}&thirdQuery={thirdQuery}&viewId={viewId}&requestId={requestId}&ksTS={_ksTS}";
                    _dataUrlQueue.Enqueue(url);

                    if (curRow == 0)
                    {
                        var json = base.GetWebContent(url);
                        var totalItem = int.Parse(Regex.Match(json, "(?<=totalItem\": \").*?(?=\")").Value);
                        totalPage = totalItem % pageSize == 0 ? totalItem / pageSize : totalItem / pageSize + 1;
                    }

                    curRow += pageSize;

                }
            }
            else if (_acvivityEnum.ToString().Equals("TejiaJinRiBaoKuan"))
            {
                
                var appId = Regex.Match(html, "(?<=data-app-id=\").*?(?=\"[^(包邮)]*alt=\"更多今日爆款\")").Value;
                var blockId = Regex.Match(html, "(?<=data-block-id=\").*?(?=\"[^(包邮)]*alt=\"更多今日爆款\")").Value;
                DateTime dateTime = new DateTime();
                var start = new DateTime(1970, 1, 1, 0, 0, 0, dateTime.Kind);
                var t = Convert.ToInt64((DateTime.Now - start).TotalSeconds);
                var _input_charset = "utf-8";
                var sort = "";
                var pageSize = 100;
                var extQuery = "";
                var thirdQuery = "";
                var viewId = System.Guid.NewGuid().ToString();
                var requestId = System.Guid.NewGuid().ToString();
                var combine = 8;
                var flowId = 25;
                var topIds = "";
                var _ksTS = $"{t}_{Random.Next(10, 99)}";
                var startRow = 0;
                var url = $@"http://zhi.taobao.com/json/fantomasItems.htm?t={t}&_input_charset={_input_charset}&sort={sort}&appId={appId}&blockId={blockId}&pageSize={pageSize}&startRow={startRow}&extQuery={extQuery}&thirdQuery={thirdQuery}&viewId={viewId}&requestId={requestId}&combine={combine}&flowId={flowId}&topIds={topIds}&ksTS={_ksTS}";
                _dataUrlQueue.Enqueue(url);

            }else if (_acvivityEnum.ToString().Equals("TejiaTeHuiTunNew"))
            {
                var matches = Regex.Matches(html, @"(?<=quot;:)\d{7}");
                var list = new List<string>();
                foreach (Match match in matches)
                {
                    var value = match.Value;
                    if(!list.Contains(value))
                        list.Add(value);
                }

                var tce_sid = string.Empty;
                var isFirstTime = true;
                foreach (var value in list)
                {
                    tce_sid = isFirstTime == true ? $"{value}" : $"{tce_sid},{value}";
                    if (isFirstTime == true)
                        isFirstTime = false;
                }

                var count = ",,,,,";
                var env = "online,online,online,online,online,online";
                var requestId =
                    $"{System.Guid.NewGuid().ToString()},{System.Guid.NewGuid().ToString()},{System.Guid.NewGuid().ToString()}," +
                    $"{System.Guid.NewGuid().ToString()},{System.Guid.NewGuid().ToString()},{System.Guid.NewGuid().ToString()}";
                var tab = ",,,,,";
                var tce_vid = "1,1,1,1,1,1";
                var tid = ",,,,,";
                var topic = ",,,,,";
                var viewId = $"{System.Guid.NewGuid().ToString()},{System.Guid.NewGuid().ToString()},{System.Guid.NewGuid().ToString()}," +
                             $"{System.Guid.NewGuid().ToString()},{System.Guid.NewGuid().ToString()},{System.Guid.NewGuid().ToString()}";

                var url =
                    $@"https://tce.taobao.com/api/mget.htm?tce_sid={tce_sid}&tce_vid={tce_vid}&tid={tid}&tab={tab}&topic={topic}&count={count}&env={env}&viewId={viewId}&requestId={requestId}";
                _dataUrlQueue.Enqueue(url);

            }
            else if(_acvivityEnum.ToString().Equals("TaoJinBi"))
            {
                var curRow = 0;
                var totalPage = 1;
                var curPage = 1;

                for (int i = 0; i < totalPage; i++)
                {
                    var appId = 10;
                    var blockId = 1001;
                    var buckId = 1;
                    var pageSize = 18;
                    var viewId = System.Guid.NewGuid();
                    DateTime dateTime = new DateTime();
                    var start = new DateTime(1970, 1, 1, 0, 0, 0, dateTime.Kind);
                    var t = Convert.ToInt64((DateTime.Now - start).TotalSeconds);
                    var _ksTS = $"{t}_{Random.Next(10, 99)}";
                    var startRow = curRow;
                    var page = curPage;

                    var url = $"https://zhi.taobao.com/json/fantomasItems.htm?appId={appId}&blockId={blockId}&bucketId={buckId}&page={page}&startRow={startRow}&pageSize={pageSize}&viewId={viewId}&_ksTS={_ksTS}";
                    _dataUrlQueue.Enqueue(url);

                    if (curRow == 0)
                    {
                        var json = base.GetWebContent(url);
                        var totalItem = int.Parse(Regex.Match(json, "(?<=totalItem\": \").*?(?=\")").Value);
                        totalPage = totalItem % pageSize == 0 ? totalItem / pageSize : totalItem / pageSize + 1;
                    }


                    curPage ++;
                    curRow += pageSize;
                    

                }

            }
        }


        /// <summary>
        /// Gets the information by json.
        /// </summary>
        /// <param name="json">The json.</param>
        /// <returns>Dictionary&lt;System.String, System.String&gt;.</returns>
        private Dictionary<string, string> GetInformationByJson(string json)
        {
            //去掉\&quot;
            json = HttpUtility.HtmlDecode(json);
            var jObject = JObject.Parse(json);
            var empty = string.Empty;
            //编号
            var itemId = empty;
            //标题
            var title = empty;
            //保留价格
            var reservePrice = empty;
            //打折价格
            var discountPrice = empty;
            //折扣
            var discount = empty;
            //目前销售量
            var currentSellOut = empty;
            //数量
            var quantity = empty;
            //当前数量
            var currentQuantity = empty;
            //活动开始时间
            var activityStartTime = empty;
            //活动结束时间
            var activityEndTime = empty;
            //店铺编号
            var shopId = empty;
            //店铺名称
            var shopName = empty;

            if (_acvivityEnum.ToString().Equals("TeJia")
                || _acvivityEnum.ToString().Equals("TeJiaTen")
                || _acvivityEnum.ToString().Equals("TejiaJinRiBaoKuan")
                || _acvivityEnum.ToString().Equals("TaoJinBi"))
            {
                //编号
                itemId = jObject["itemId"].ToString();
                //标题
                title = jObject["title"].ToString();
                //保留价格
                reservePrice = jObject["reservePrice"].ToString();
                //打折价格
                discountPrice = jObject["discountPrice"].ToString();
                //折扣
                discount = jObject["discount"].ToString();
                //目前销售量
                currentSellOut = jObject["currentSellOut"].ToString();
                //数量
                quantity = jObject["quantity"].ToString();
                //当前数量
                currentQuantity = jObject["currentQuantity"].ToString();
                //活动开始时间
                activityStartTime = jObject["activityStartTime"].ToString();
                //活动结束时间
                activityEndTime = jObject["activityEndTime"].ToString();
                //店铺编号
                shopId = jObject["shopId"].ToString();
                //店铺名称
                shopName = jObject["shopName"].ToString();



            }
            else if (_acvivityEnum.ToString().Equals("TejiaTeHuiTunNew"))
            {

                //编号
                itemId = jObject["item_numiid"].ToString();
                //标题
                title = jObject["item_title"].ToString();
                //保留价格
                reservePrice = jObject["item_price"].ToString();
                //打折价格
                discountPrice = jObject["item_current_price"].ToString();
                //目前销售量
                currentSellOut = jObject["item_trade_num"].ToString();


            }

            var dic = new Dictionary<string, string>
            {
                {"itemId", itemId},
                {"title", title},
                {"reservePrice", reservePrice},
                {"discountPrice", discountPrice},
                {"discount", discount},
                {"currentSellOut", currentSellOut},
                {"quantity", quantity},
                {"currentQuantity", currentQuantity},
                {"activityStartTime", activityStartTime},
                {"activityEndTime", activityEndTime},
                {"shopId", shopId},
                {"shopName", shopName}
            };
            

            return dic;
        }

        /// <summary>
        /// GetItemList step 4 一串json的列表
        /// </summary>
        /// <param name="url"></param>
        private List<string> GetItemList(string url)
        {

            //url = "https://ju.taobao.com//json/brand/ajaxGetBrandFloorV2.json?actSignId=20548753&floorIndex=3&includeforecast=true&stype=ids&reverse=down";
            var itemList = new List<string>();
            var html = base.GetWebContent(url);
            string regexString;

            switch (_acvivityEnum)
            {
                case AcvivityEnum.TeJia:
                case AcvivityEnum.TeJiaTen:
                case AcvivityEnum.TejiaJinRiBaoKuan:
                case AcvivityEnum.TaoJinBi:
                    regexString = @"{[\s]*""itemId"":""[\s\S]*?}";
                    break;
                case AcvivityEnum.TejiaTeHuiTunNew:
                    regexString = @"{[\s]*""sys_tce_result_sequence"":""[\s\S]*?}";
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException(nameof(_acvivityEnum), _acvivityEnum, null);
            }

            var matches = Regex.Matches(html, regexString);
            foreach (Match match in matches)
            {
                var value = match.Value;
                itemList.Add(value);
                Console.WriteLine(value);
            }

            return itemList;

        }


        /// <summary>
            /// 解析出当前值
            /// </summary>
        /// <returns>IResut[].</returns>
        /// <exception cref="System.NotImplementedException"></exception>
        protected override IResut[] ParseCurrentItems()
        {
            var resultList = new List<IResut>();
            var itemList = GetItemList(CurrentUrl);
            var activityType = _acvivityEnum.GetType();



            foreach (var item in itemList)
            {

                var dic = GetInformationByJson(item);



                IResut resut = new Resut()
                {
                    {"itemId", dic["itemId"]},
                    {"title", dic["title"]},
                    {"reservePrice", dic["reservePrice"]},
                    {"discountPrice", dic["discountPrice"]},
                    {"discount", dic["discount"]},
                    {"currentSellOut", dic["currentSellOut"]},
                    {"quantity",dic["quantity"] },
                    {"currentQuantity", dic["currentQuantity"]},
                    {"activityStartTime", dic["activityStartTime"]},
                    {"activityEndTime", dic["activityEndTime"]},
                    {"shopId", dic["shopId"]},
                    {"shopName", dic["shopName"]},
                    { "type" , _acvivityEnum.ToString()}
                };



                resultList.Add(resut);
            }

            return resultList.ToArray();
        }

        /// <summary>
        /// 解析出下一页的地址，默认当没有下一页的时候，枚举停止
        /// </summary>
        /// <returns>System.String.</returns>
        protected override string ParseNextUrl()
        {
            return _dataUrlQueue.Count != 0 ? _dataUrlQueue.Dequeue() : null;
        }


        /// <summary>
        /// ParseCountPage
        /// </summary>
        /// <returns></returns>
        protected override int ParseCountPage()
        {
            return _countPage;
        }


        /// <summary>
        /// ParseCurrentPage
        /// </summary>
        /// <returns></returns>
        protected override int ParseCurrentPage()
        {
            return _countPage - (_dataUrlQueue.Count + 1);
        }
    }
}
