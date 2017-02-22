using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Text.RegularExpressions;
using GE.Data;
using Newtonsoft.Json.Linq;
using X.GlodEyes.Collectors;
using X.GlodEyes.Collectors.Specialized.JingDong;

namespace XFCollection.TaoBao 
{
    /// <summary>
    /// 活动数据采集（阿里站内部分）
    /// </summary>
    public class ActivityCollectors : WebRequestCollector<IResut, ActivityParameter>
    {
        //delegate string GetFormatDelegate(string url);

        /// <summary>
        /// 活动类型枚举
        /// </summary>
        private enum AcvivityEnum
        {
            Brand,
            JuLiangFan,
            MingPin,
            Life,
            Trip
        }


        private string _type;
        /// <summary>
        /// The _count page
        /// </summary>
        private int _countPage;
        /// <summary>
        /// The _acvivity enum
        /// </summary>
        private AcvivityEnum _acvivityEnum;
        /// <summary>
        /// The _activity list
        /// </summary>
        private List<string> _activityList;
        /// <summary>
        /// The _activity URL list
        /// </summary>
        private List<string> _activityUrlList;
        /// <summary>
        /// The _data URL queue
        /// </summary>
        private Queue<string> _dataUrlQueue;

        /// <summary>
        /// 测试
        /// </summary>
        internal static void Test()
        {
            var parameter = new ActivityParameter()
            {
                Url = @"https://ju.taobao.com/tg/brand.htm#美容护肤"
            };



            TestHelp<ActivityCollectors>(parameter);    
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        public ActivityCollectors()
        {
            _activityList = new List<string>();
            _activityUrlList = new List<string>();
            _dataUrlQueue = new Queue<string>();
        }

        /// <summary>
        /// GetActivityList step1 
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private void GetActivityList(string url)
        {
            switch (_acvivityEnum)
            {
                case AcvivityEnum.Brand:
                    var html = base.GetWebContent(url);
                    //当前的和即将上线的
                    var regexString = $@"(?<=data-ajax="").*?(?="">[\s\S]*{_type}</div>)";
                    var regexStringSoon = $@"(?<=data-ajax="")[\S]*?(?="">[^/]*{_type}</span>)";
                    var value = Regex.Match(html, regexString).Value;
                    var valueSoon = Regex.Match(html, regexStringSoon).Value;
                    if(!string.IsNullOrEmpty(value))
                        _activityList.Add(FillUrl(value));
                    if(!string.IsNullOrEmpty(valueSoon))
                        _activityList.Add(FillUrl(valueSoon));
                    break;
                case AcvivityEnum.MingPin:
                    html = base.GetWebContent(url);
                    //当前的和即将上线的
                    regexString = @"{\""id\"":.*?\""},";
                    var matches = Regex.Matches(html, regexString);
                    foreach (Match match in matches)
                    {
                        value = match.Value;
                        if (value.Contains(_type))
                        {
                            var realUrl = Regex.Match(value, @"(?<=\""url\"":\"").*?(?=\"")").Value;
                            _activityList.Add(FillUrl(realUrl));
                            break;
                        }
                    }
                    //regexString = @"{\""price\"":.*?}}";
                    //regexString = @"{\""merit\"".*?[^""{]}}";
                    break;
                case AcvivityEnum.JuLiangFan:
                case AcvivityEnum.Life:
                case AcvivityEnum.Trip:
                    _activityList.Add(url);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(_acvivityEnum), _acvivityEnum, null);
            }

        }

        /// <summary>
        /// GetActivityUrlListLoop step2 loop
        /// </summary>
        private void GetActivityUrlListLoop()
        {
            
            foreach (var activity in _activityList)
            {

                GetActivityUrlList(activity);
            }
        }


        /// <summary>
        /// GetActivityUrlList step2
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private void GetActivityUrlList(string url)
        {
            Predicate<string> predicate = myUrl => myUrl.Contains("act_sign_id") && myUrl.Contains("seller_id");


            switch (_acvivityEnum)
            {
                case AcvivityEnum.Brand:
                case AcvivityEnum.MingPin:
                    Console.WriteLine(url);
                    var html = base.GetWebContent(url); 
                    var matches = Regex.Matches(html, @"(?<=activityUrl\"":\"").*?(?=\"")");
                    foreach (Match match in matches)
                    {
                        var value = FillUrl(match.Value);
                        if(predicate(value))
                            _activityUrlList.Add(value);
                    }
                    break;
                case AcvivityEnum.JuLiangFan:          
                case AcvivityEnum.Life:
                case AcvivityEnum.Trip:
                    _activityUrlList.Add(url);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(_acvivityEnum), _acvivityEnum, null);
            }
        }


        /// <summary>
        /// GetDataUrlQueueLoop step 3 loop
        /// </summary>
        private void GetDataUrlQueueLoop()
        {
            foreach (var activityUrl in _activityUrlList)
            {
                GetDataUrlQueue(activityUrl);
            }
        }

        /// <summary>
        /// GetDataUrlQueue step 3
        /// </summary>
        private void GetDataUrlQueue(string url)
        {
            var html = base.GetWebContent(url);
            string regexString;
            switch (_acvivityEnum)
            {
                case AcvivityEnum.Brand:
                case AcvivityEnum.MingPin:
                    regexString = @"(?<=data-url=\"").*?(?=\"")";
                    break;
                case AcvivityEnum.JuLiangFan:
                case AcvivityEnum.Life:
                case AcvivityEnum.Trip:
                    regexString = @"(?<=dataUrl\"":\"").*?(?=\"")";
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(_acvivityEnum), _acvivityEnum, null);
            }

            InitDataUrlQueue(html, regexString);

            
            _countPage = _dataUrlQueue.Count;
        }






        /// <summary>
        /// Initializes the data URL queue.
        /// </summary>
        /// <param name="html">The HTML.</param>
        /// <param name="regexString">The regex string.</param>
        private void InitDataUrlQueue(string html, string regexString)
        {
            var matches = Regex.Matches(html, regexString);
            foreach (Match match in matches)
            {
                var value = match.Value;
                _dataUrlQueue.Enqueue(FillUrl(FormatUrl(value)));
                Console.WriteLine(FillUrl(FormatUrl(value)));
            }
        }


        static void TestJsonFile()
        {
            var desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            var filePath = Path.Combine(desktop, @"ajaxGetBrandFloorV2.json");

            var allText = File.ReadAllText(filePath);
            var jObject = JObject.Parse(allText);
            foreach (JToken item in jObject[@"itemList"].Children())
            {
        
                var itemId = item[@"baseinfo"][@"itemId"];
                Console.WriteLine(itemId);
                var title = item[@"name"][@"title"];
                Console.WriteLine(title);

            }
        }

        /// <summary>
        /// GetInformationByJson
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        private Dictionary<string, string> GetInformationByJson(string json)
        {


            var jObject = JObject.Parse(json);
            //标题
            var title = jObject["name"]["title"];
            //描述
            var description = jObject["merit"]["desc"];
            //编号
            var itemId = jObject["baseinfo"]["itemId"];
            //开抢时间
            var ostimeText = jObject["baseinfo"]["ostimeText"];
            //剩余时间
            var leftTime = jObject["baseinfo"]["leftTime"];
            //总共库存
            var totalStock = jObject["baseinfo"]["totalStock"];
            //销售总额
            var soldAmount = jObject["baseinfo"]["soldAmount"];
            //链接
            var itemUrl = jObject["baseinfo"]["itemUrl"];
            //实际价格
            var actPrice = jObject["price"]["actPrice"];
            //原始价格
            var origPrice = jObject["price"]["origPrice"];
            //折扣
            var discount = jObject["price"]["discount"];

            var dic = new Dictionary<string, string>
            {
                {"title", title.ToString()},
                {"description", description.ToString()},
                {"itemId", itemId.ToString()},
                {"ostimeText", ostimeText.ToString()},
                {"leftTime", leftTime.ToString()},
                {"totalStock", totalStock.ToString()},
                {"soldAmount",soldAmount.ToString() },
                {"itemUrl", itemUrl.ToString()},
                {"actPrice", actPrice.ToString()},
                {"origPrice", origPrice.ToString()},
                {"discount", discount.ToString()}
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
                case AcvivityEnum.Brand:
                case AcvivityEnum.MingPin:
                    regexString = @"{\""baseinfo\"".*?soldCount.*?}}";
                    break;
                case AcvivityEnum.JuLiangFan:
                case AcvivityEnum.Life:
                case AcvivityEnum.Trip:
                    regexString = @"{\""baseinfo\"".*?brandLogo\"":\""\""}}";
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
        /// InitFirstUrl
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        protected override string InitFirstUrl(ActivityParameter param)
        {

            var url = param.Url;

            if (url.Contains("#"))
            {
                _type = Regex.Match(url, "(?<=#).*").Value;
                url = Regex.Match(url, ".*(?=#)").Value;
            }

            //var activityType = Regex.Match(url, "(?<=tg/).*?(?=.htm)").Value;
            //activityType = string.IsNullOrEmpty(activityType)? Regex.Match(url, "(?<=jusp/.*/).*(?=.tp)").Value: activityType;
            //// str1大于str2 1  str1等于str 0  str1小于str2 -1
            //if (string.Compare(activityType,"Brand",StringComparison.OrdinalIgnoreCase)== 0)
            //    _acvivityEnum = AcvivityEnum.Brand;
            //else if(string.Compare(activityType, "JuLiangFan", StringComparison.OrdinalIgnoreCase) == 0)
            //    _acvivityEnum = AcvivityEnum.JuLiangFan;
            //else if(string.Compare(activityType, "MingPin",StringComparison.OrdinalIgnoreCase) == 0)
            //    _acvivityEnum = AcvivityEnum.MingPin;
            //else if(string.Compare(activityType, "Life",StringComparison.OrdinalIgnoreCase) == 0)
            //    _acvivityEnum = AcvivityEnum.Life;
            //else if(string.Compare(activityType, "Trip", StringComparison.OrdinalIgnoreCase) ==0 )
            //    _acvivityEnum = AcvivityEnum.Trip;
            //else
            //    throw new Exception("ActivityType参数类型错误！");

            if(url.Equals("https://ju.taobao.com/tg/brand.htm"))
                _acvivityEnum = AcvivityEnum.Brand;
            else if (url.Equals("https://ju.taobao.com/jusp/other/juliangfan/tp.htm"))
                _acvivityEnum = AcvivityEnum.JuLiangFan;
            else if(url.Equals("https://ju.taobao.com/jusp/other/mingpin/tp.htm"))
                _acvivityEnum = AcvivityEnum.MingPin;
            else if(url.Equals("https://ju.taobao.com/jusp/shh/life/tp.htm"))
                _acvivityEnum = AcvivityEnum.Life;
            else if(url.Equals("https://ju.taobao.com/jusp/shh/trip/tp.htm"))
                _acvivityEnum = AcvivityEnum.Trip;
            else
                throw new Exception("url链接错误。");


            


            GetActivityList(url);
            GetActivityUrlListLoop();
            GetDataUrlQueueLoop();
            return url;
        }



        /// <summary>
        /// ParseCurrentItems
        /// </summary>
        /// <returns></returns>
        protected override IResut[] ParseCurrentItems()
        {
            var resultList  = new List<IResut>();
            var itemList = GetItemList(CurrentUrl);
            var activityType = _acvivityEnum.GetType();
            
            

            foreach (var item in itemList)
            {

                var dic = GetInformationByJson(item);


                IResut resut = new Resut()
                {
                    { "title",dic["title"]},
                    { "description", dic["description"]},
                    { "itemId", dic["itemId"]},
                    { "ostimeText", dic["ostimeText"]},
                    { "leftTime", dic["leftTime"]},
                    { "totalStock", dic["totalStock"]},
                    { "soldAmount",dic["soldAmount"]},
                    { "itemUrl", dic["itemUrl"]},
                    { "actPrice", dic["actPrice"]},
                    { "origPrice", dic["origPrice"]},
                    { "discount", dic["discount"]},
                    { "type" , _acvivityEnum.ToString()}
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
            
            return _dataUrlQueue.Count!=0 ? _dataUrlQueue.Dequeue() : null;
        }


        /// <summary>
        /// ParseCountPage
        /// </summary>
        /// <returns></returns>
        protected override int ParseCountPage()
        {
            return _countPage+1;
        }

        /// <summary>
        /// ParseCurrentPage
        /// </summary>
        /// <returns></returns>
        protected override int ParseCurrentPage()
        {
            return _countPage - _dataUrlQueue.Count;
        }

        /// <summary>
        /// FormatUrl
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private string FormatUrl(string url)
        {
            return url.Replace("amp;", "");
        }

        /// <summary>
        /// FillUrl
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private string FillUrl(string url)
        {
            if (url.Contains("http") || url.Contains("https"))
                return url;
            return $"https:{url}";
        }

        /// <summary>
        /// GetExparams
        /// </summary>
        /// <returns></returns>
        public string GetExparams(string url)
        {
            return Regex.Match(url, "(?<=\"exparams\",\").*?(?=\")").Value;
        }





        /// <summary>
        /// Linq Test
        /// </summary>
        private void LinqTest()
        {
            //List<string> urlList = new List<string>()
            //{
            //    "//ju.taobao.com//json/brand/ajaxGetBrandFloorV2.json?actSignId=20814522&amp;floorIndex=2&amp;includeforecast=true&amp;stype=samount&amp;reverse=down",
            //    "//ju.taobao.com/tg/brand_items.htm?act_sign_id=21193226&amp;seller_id=1985598030",
            //    "//ju.taobao.com/tg/brand_items.htm?act_sign_id=21215404&amp;seller_id=1985843490",
            //    "//ju.taobao.com/tg/brand_items.htm?act_sign_id=21193274&amp;seller_id=2142698337"
            //};

            //string url = "//ju.taobao.com//json/brand/ajaxGetBrandFloorV2.json?actSignId=20814522&amp;floorIndex=2&amp;includeforecast=true&amp;stype=samount&amp;reverse=down";

            //GetFormatDelegate getFormatDelegate = new GetFormatDelegate(GetFormatUrl);
            //var formatUrl = getFormatDelegate(url);


            //Predicate<string> predicate = new Predicate<string>(delegate(string myUrl)
            //{
            //    return myUrl.Contains("amp;");
            //});


            //Console.WriteLine(predicate(url));

            //Predicate<string> predicate1 = myUrl => myUrl.Contains("amp;");

            //Console.WriteLine(predicate1(url));

            //Action<string> action = new Action<string>(delegate(string myUrl)
            //{
            //    Console.WriteLine(myUrl.Replace("amp;",""));
            //    return;
            //});

            //action(url);

            //Action<string> action1 = myUrl =>
            //{
            //    Console.WriteLine(myUrl.Replace("amp;", ""));
            //};

            //action1(url);

            //System.Func<string,string> func = new System.Func<string,string>(delegate(string myUrl)
            //{
            //    return myUrl.Replace("amp;", url);
            //});

            //func(url);

            //Func<string, string> func1 = myUrl =>
            //{
            //    return myUrl.Replace("amp;", url);
            //};


            //List<int> arr = new List<int>() {1,2,3,4,5,6,7,8,9};
            //var result = arr.Where(num => num > 3).Sum();

            //Console.WriteLine(result);

            //result = (from num in arr where num > 3 select num).Sum();
            //Console.WriteLine(result);


            //Action<string> action = FormalUrl;
            //action(url);
        }

        ///// <summary>
        ///// GetFormatUrl
        ///// </summary>
        ///// <param name="url"></param>
        ///// <returns></returns>
        //private string GetFormatUrl(string url)
        //{
        //    return url.Replace("amp;", "");
        //}

        ///// <summary>
        ///// IsFormatUrl
        ///// </summary>
        ///// <param name="url"></param>
        ///// <returns></returns>
        //private bool IsFormatUrl(string url)
        //{
        //    return url.Contains("amp;");
        //}

        ///// <summary>
        ///// FormalUrl
        ///// </summary>
        ///// <param name="url"></param>
        //private void FormalUrl(string url)
        //{
        //    return;
        //}
    }
}
