using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using X.GlodEyes.Collectors;
using X.GlodEyes.Collectors.Specialized.JingDong;
using XFCollection.phantomjs;

namespace XFCollection.TaoBao
{
    /// <summary>
    /// PromotionCollector
    /// </summary>
    public class PromotionCollector : WebRequestCollector<IResut, NormalParameter>
    {

        private Queue<string> _dataUrlQueue;
        private string _nick;

        private static string _cookies;
        /// <summary>
        /// DefaultMovePageTimeSpan
        /// </summary>
        public override double DefaultMovePageTimeSpan => 60;

        //l=AmRk0HdXlWiBuvM4W0Z4qZ6BNHlW/Yhn;isg=Ag0NWFMkadoHVs32g9oUIQekFinGH0G8dD3Qo0-SSaQTRi34FzpRjFtUYmDf;cna=liq9EMiE1zwCAXPOyzO+sm7p;_tb_token_=e44754563e83e;t=11c47ac2d1010a2052743213511ad21b;cookie2=16da497e63db695664f9d0d81fc7abfd;v=0

        /// <summary>
        /// 测试
        /// </summary>
        internal static void Test()
        {
            var parameter = new NormalParameter()
            {
                //Keyword = @"恒源祥风度专卖店"
                Keyword = @"jackjones官方旗舰"
            };




            TestHelp<PromotionCollector>(parameter);
        }

        /// <summary>
        /// Test1
        /// </summary>
        public static void Test1()
        {
            var keyWords = File.ReadAllLines(@"C:\Users\Administrator\Desktop\bossNickName.txt");

            keyWords = Array.ConvertAll(keyWords, keyWord => keyWord.Trim('"'));

            foreach (var keyWord in keyWords)
            {
                Console.WriteLine($"keyword id:{keyWord}");

                try
                {
                    var parameter = new NormalParameter() { Keyword = keyWord };
                    TestHelp<PromotionCollector>(parameter);
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
        public PromotionCollector()
        {
            _dataUrlQueue = new Queue<string>();
        }



        /// <summary>
        /// InitDataUrlQueue
        /// </summary>
        /// <param name="_nick"></param>
        private void InitDataUrlQueue(string _nick)
        {
            var tid = 0;
            var s = 95;
            var module = "page";
            var data_value= 0;
            var data_key = "s";
            //var data_action = "";
            var _input_charset = "utf-8";
            var json = "on";
            var atype = "b";
            var cat = 0;
            var nick = _nick;
            //var nick = "jackjones官方旗舰";
            var style = "list";
            var _as = 0;
            var viewIndex = 1;
            var same_info = 1;
            var zk = "all";
            var isnew = 2;
            var commend = "all";
            var pSize = 95;
            var dateTime = new DateTime();
            var start = new DateTime(1970, 1, 1, 0, 0, 0, dateTime.Kind);
            var t = Convert.ToInt64((DateTime.Now - start).TotalSeconds);
            var _ksTS = $"{t}_{Random.Next(10, 99)}";

            var curUrl =
                $"https://list.taobao.com/itemlist/default.htm?_input_charset={_input_charset}&json={json}&atype={atype}&cat={cat}&" +
                $"nick={nick}&style={style}&as={_as}&viewIndex={viewIndex}&same_info={same_info}&zk={zk}&isnew={isnew}&commend={commend}&pSize={pSize}&_ksTS={_ksTS}";
            _dataUrlQueue.Enqueue(curUrl);

            if (string.IsNullOrEmpty(_cookies))
            {
                //通过phantomjs得到cookies
                _cookies =
                    PhantomjsDemo.GetCookies(
                        $"https://list.taobao.com/itemlist/default.htm?viewIndex=1&commend=all&atype=b&nick={nick}&style=list&same_info=1&tid=0&isnew=2&zk=all&_input_charset=utf-8");
            }

            //get请求 加上cookies就能得到网页内容了
            var html = base.GetWebContent(curUrl,null,ref _cookies,"");
            if(Regex.IsMatch(html, "^{\"rgv587_flag\":\"sm\","))
                throw new Exception("被屏蔽了！");
            //MyHttpHelper httpHelper = new MyHttpHelper();
            //HttpItem httpItem = new HttpItem();
            //httpItem.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8";
            //httpItem.ContentType = "text/html";
            //httpItem.UserAgent = "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/51.0.2704.106 Safari/537.36";
            //httpItem.URL = curUrl;
            ////是否允许跳转
            //httpItem.Allowautoredirect = true;
            //var httpResult = httpHelper.GetHtml(httpItem);
            //html = httpResult.Html;



            var totalNumberString = Regex.Match(html, @"(?<=""totalNumber"":"")\d+(?="")").Value;
            if (String.IsNullOrEmpty(totalNumberString.ToString()))
                return;
            var totalNumber = Int32.Parse(totalNumberString);
            if (totalNumber == 0)
                return;

           
            var totalPage = totalNumber%pSize==0? totalNumber/ pSize: totalNumber / pSize+1;

            

            for (var i = 0; i < totalPage-1; i++)
            {
                data_value += pSize;
                t = Convert.ToInt64((DateTime.Now - start).TotalSeconds);
                _ksTS = $"{t}_{Random.Next(10, 999)}";
                curUrl = $"https://list.taobao.com/itemlist/default.htm?_input_charset={_input_charset}&json={json}&cat={cat}&viewIndex={viewIndex}&as={_as}&commend={commend}&" +
                         $"atype={atype}&s={s}&nick={nick}&style={style}&same_info={same_info}&tid={tid}&isnew={isnew}&zk={zk}&pSize={pSize}&data-key={data_key}&data-value={data_value}&data-action&module={module}&_ksTS={_ksTS}";
                _dataUrlQueue.Enqueue(curUrl);
            }


            CountPage = totalPage;
            CurrentPage = 0;

        }



        /// <summary>
        /// 为第一页作初始化准备
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        protected override string InitFirstUrl(NormalParameter param)
        {
            _nick = param.Keyword;
            InitDataUrlQueue(_nick);

            return _dataUrlQueue.Count != 0 ? _dataUrlQueue.Dequeue() : null;

        }

        /// <summary>
        /// ParseCurrentItems
        /// </summary>
        /// <returns></returns>
        protected override IResut[] ParseCurrentItems()
        {
            var resultList = new List<IResut>();
            var itemList = GetItemList();

            foreach (var item in itemList)
            {
                var dic = GetInformationByJToken(item);


                IResut resut = new Resut()
                {
                    {"imageUrl", dic["imageUrl"]},
                    {"title", dic["title"]},
                    {"price", dic["price"]},
                    {"curPrice",dic["curPrice"]},
                    {"vipPrice", dic["vipPrice"]},
                    {"tradeNum", dic["tradeNum"]},
                    {"nick", _nick},
                    {"sellerId", dic["sellerId"]},
                    {"itemId", dic["itemId"]},
                    {"loc", dic["loc"]},
                    {"storeLink", dic["storeLink"]},
                    {"href", dic["href"]},
                    {"commend", dic["commend"]},
                    {"commendHref", dic["commendHref"]}
                };

                resultList.Add(resut);

            }

            return resultList.ToArray();
        }

        /// <summary>
        /// GetInformationByJToken
        /// </summary>
        /// <param name="jToken"></param>
        /// <returns></returns>
        private Dictionary<string, string> GetInformationByJToken(JToken jToken)
        {

            var imageUrl = jToken["image"].ToString();
            var title = jToken["title"].ToString();
            var price = jToken["price"].ToString();
            var curPrice = jToken["currentPrice"].ToString();
            var vipPrice = jToken["vipPrice"].ToString();
            var tradeNum = jToken["tradeNum"].ToString();
            var nick = jToken["nick"].ToString();
            var sellerId = jToken["sellerId"].ToString();
            var itemId = jToken["itemId"].ToString();
            var loc = jToken["loc"].ToString();
            var storeLink = jToken["storeLink"].ToString();
            var href = jToken["href"].ToString();
            var commend = jToken["commend"].ToString();
            var commendHref = jToken["commendHref"].ToString();



            var dic = new Dictionary<string, string>()
            {
                {"imageUrl",imageUrl },
                {"title",title },
                {"price",price },
                {"curPrice",curPrice },
                {"vipPrice",vipPrice },
                {"tradeNum",tradeNum },
                {"nick", nick},
                {"sellerId",sellerId },
                {"itemId",itemId },
                {"loc",loc },
                {"storeLink", storeLink},
                {"href",href },
                {"commend",commend },
                {"commendHref",commendHref }
            };


            return dic;
        }


        /// <summary>
        /// GetItemList
        /// </summary>
        /// <returns></returns>
        private List<JToken> GetItemList()
        {
            //var itemList = new List<string>();
            var html = base.GetWebContent(CurrentUrl, null, ref _cookies, "");
            if (Regex.IsMatch(html, "^{\"rgv587_flag\":\"sm\","))
                throw new Exception("被屏蔽了！");
            return ParseListJson(html);
        }

        void Test13143()
        {
            var filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), @"json.json");
            var allText = File.ReadAllText(filePath);
            ParseListJson(allText);
        }

        /// <summary>
        /// ParseListJson
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        private List<JToken> ParseListJson(string html)
        {
            var jObject = JObject.Parse(html);
            JToken jToken;
            
           jToken = jObject["itemList"];
          
            //“System.InvalidOperationException”的异常
            //var value1 = jToken.Children<JToken>();
            //var value2 = jToken.Values<JToken>();
            
            //Console.WriteLine(jToken.ToString());

            if (jToken == null||String.IsNullOrEmpty(jToken.ToString()))
                return new List<JToken>();
            else
            {
                return new List<JToken>(jToken.Values<JToken>());
            }

            //return jToken == null||jToken.ToString() == "{}" ? new List<JToken>() : new List<JToken>(jToken.Values<JToken>());
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
