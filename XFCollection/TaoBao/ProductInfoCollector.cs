using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.UI.WebControls;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;
using X.CommLib.Net.WebRequestHelper;
using X.GlodEyes.Collectors;
using X.GlodEyes.Collectors.Specialized.JingDong;
using static System.DateTime;

namespace XFCollection.TaoBao
{
    /// <summary>
    /// ProductInfoCollector
    /// </summary>
    public class ProductInfoCollector:WebRequestCollector<IResut,NormalParameter>
    {

        /// <summary>
        /// DefaultMovePageTimeSpan
        /// </summary>
        public override double DefaultMovePageTimeSpan => 0;
        private string _productId;
        private string _shopId;

        /// <summary>
        /// 链接 http://hws.m.taobao.com/cache/wdetail/5.0/?id=521240802773
        /// 主要的json页面
        /// </summary>
        private string _jsonUrl = "http://hws.m.taobao.com/cache/wdetail/5.0/?id=";

        ///// <summary>
        ///// 链接 https://detail.tmall.com/item.htm?id=521240802773
        ///// 显示界面
        ///// </summary>
        //private string _tmall_displayUrl = "https://detail.tmall.com/item.htm?id=";


        ///// <summary>
        ///// 链接 https://rate.tmall.com/listTagClouds.htm?itemId=521240802773
        ///// 标签云信息 "tag":"质量好" "count":36
        ///// </summary>
        //private string _tmall_listTagCloudsUrl = "https://rate.tmall.com/listTagClouds.htm?itemId=";


        ///// <summary>
        ///// 链接 https://rate.tmall.com/list_detail_rate.htm?itemId=521240802773+spuId=274378617+sellerId=2579937287
        ///// 图片评价数picNum 追评数used 
        ///// </summary>
        //private string _tmall_listDetailRateUrl = "https://rate.tmall.com/list_detail_rate.htm?";



        /// <summary>
        /// 链接 https://dsr-rate.tmall.com/list_dsr_info.htm?itemId=521240802773+spuId=274378617+sellerId=2579937287
        /// 天猫 这里采gradeAvg 与描述相符 和 rateTotal 累计评价
        /// </summary>
        private string _tmall_list_dsr_infoUrl = "https://dsr-rate.tmall.com/list_dsr_info.htm?";

        ///// <summary>
        ///// 链接 https://count.taobao.com/counter3?_ksTS=1479712001880_231+callback=jsonp236+keys=SM_368_dsr-2579937287,ICCP_1_521240802773
        ///// 这里取收藏数 favcount
        ///// </summary>
        //private string _tmall_counter3Url;

        //private string _tmall_displayHtml;
        //private string _tmall_listTagCloudsHtml;  
        //private string _tmall_listDetailRateHtml;
        private string _tmall_list_dsr_infoHtml;
        //private string _tmall_counter3Html;

        ////https://item.taobao.com/item.htm?id=525224771372
        //private string _taobao_displayUrl = "https://item.taobao.com/item.htm?id=";

        //https://rate.taobao.com/detailCommon.htm?userNumId=786957650&auctionNumId=525224771372&siteID=1&spuId=0
        private string _taobao_detailCommonUrl;
        ////收藏数 favcount
        ////https://count.taobao.com/counter3?_ksTS=1479712297019_85&callback=jsonp86&inc=ICVT_7_43403445631&sign=66976978452822e10fb577ba76869cd6e0a33&keys=DFX_200_1_43403445631,ICVT_7_43403445631,ICCP_1_43403445631,SCCP_2_57628545
        //private string _taobao_counter3Url; 



        //private string _taobao_displayHtml;
        private string _taobao_detailCommonHtml;
        //private string _taobao_counter3Html;

        /// <summary>
        /// Test
        /// </summary>
        internal static void Test()
        {
            var parameter = new NormalParameter()
            {
                //Keyword = @"521240802773" //天猫
                //Keyword = @"521288700194"  //淘宝
                Keyword = @"520643556727"
            };

            parameter["shopId"] = "";

            TestHelp<ProductInfoCollector>(parameter);
        }


        /// <summary>
        /// Main
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
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
                    var parameter = new NormalParameter { Keyword = keyWord };
                    TestHelp<ProductInfoCollector>(parameter);
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

            _shopId = param.GetStringValue("shopId");
            _productId = param.Keyword; 
            if(string.IsNullOrEmpty(_productId))
                throw new Exception("传入的参数为空或null");
            return CurrentUrl = $"{_jsonUrl}{_productId}";
           
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
            //多次跳转，最大重定向次数需要设置大一点
            @default.MaxRedirect = 20;
            cookies = cookies ?? string.Empty;
            return WebRequestCtrl.GetWebContent(nextUrl, postData, ref cookies, 1, @default);
        }



        /// <summary>
        /// ParseCurrentItems
        /// </summary>
        /// <returns></returns>
        protected override IResut[] ParseCurrentItems()
        {



            var stringEmpty = string.Empty;


            var cookies = stringEmpty;

            var ItemTypeName = Regex.Match(HtmlSource, "(?<=\"itemTypeName\":\").*?(?=\")").Value;

            var CollectionNumber = Regex.Match(HtmlSource, @"(?<=""favcount"":"")\d+(?="")").Value;

            var Starts = Regex.Match(HtmlSource, "(?<=\"starts\":\").*?(?=\")").Value;


            JObject jObjectSellPoint = new JObject();
            var SellPoint = stringEmpty;

            

            var UserId = stringEmpty;


            //旧的需要访问很多链接
            //if (ItemTypeName.ToLower().Equals("tmall"))
            //{
            //    _tmall_displayHtml = GetMainWebContent($"{_tmall_displayUrl}{_productId}", null, ref cookies, "");

            //    //CollectionNumber
            //    var getUrl = Regex.Match(_tmall_displayHtml, "(?<=\"apiBeans\":\").*?(?=\")").Value;
            //    if (!getUrl.Equals(stringEmpty))
            //    {
            //        var callback = $"jsonp{Random.Next(100, 999)}";
            //        var dateTime = new DateTime();
            //        var start = new DateTime(1970, 1, 1, 0, 0, 0, dateTime.Kind);
            //        var t = Convert.ToInt64((DateTime.Now - start).TotalSeconds);
            //        var _ksTS = $"{t}_{Random.Next(100, 999)}";
            //        _tmall_counter3Url = $"https:{getUrl}&callback={callback}&_ksTS={_ksTS}";
            //        _tmall_counter3Html = GetMainWebContent(_tmall_counter3Url, null, ref cookies, "");
            //        CollectionNumber = Regex.Match(_tmall_counter3Html, $@"(?<=ICCP_1_{_productId}"":)\d+").Value;
            //    }
            //    else
            //    {
            //        CollectionNumber = stringEmpty;
            //    }


            //    var spuId = Regex.Match(_tmall_displayHtml, @"(?<=""spuId"":"")\d+(?="")").Value;
            //    var sellerId = Regex.Match(_tmall_displayHtml, @"(?<=""sellerId"":)\d+(?=,)").Value;

            //    _tmall_listTagCloudsHtml = GetMainWebContent($"{_tmall_listTagCloudsUrl}{_productId}", null, ref cookies,
            //        "");
            //    var tagClouds = Regex.Match(_tmall_listTagCloudsHtml, "(?<=\"tagClouds\":).*(?=})").Value;
            //    JArray jArray = JArray.Parse(tagClouds);
            //    var dic = jArray.ToDictionary(jToken => jToken["tag"].ToString(), jToken => jToken["count"].ToString());


            //    _tmall_listDetailRateHtml =
            //        GetMainWebContent($"{_tmall_listDetailRateUrl}itemId={_productId}&spuId={spuId}&sellerId={sellerId}",
            //            null, ref cookies, "");

            //    var PicNum = Regex.Match(_tmall_listDetailRateHtml, @"(?<=""picNum"":)\d+(?=,)").Value;
            //    var Userd = Regex.Match(_tmall_listDetailRateHtml, @"(?<=""used"":)\d+").Value;

            //    _tmall_list_dsr_infoHtml =
            //        GetMainWebContent($"{_tmall_list_dsr_infoUrl}itemId={_productId}&spuId={spuId}&sellerId={sellerId}",
            //            null, ref cookies, "");
            //    var GradeAvg = Regex.Match(_tmall_list_dsr_infoHtml, "(?<=\"gradeAvg\":).*?(?=,)").Value;
            //    var RateTotal = Regex.Match(_tmall_list_dsr_infoHtml, "(?<=\"rateTotal\":).*?(?=,)").Value;


            //    jObjectSellPoint.Add(new JProperty("ItemTypeName", ItemTypeName));
            //    jObjectSellPoint.Add(new JProperty("GradeAvg", GradeAvg));
            //    jObjectSellPoint.Add(new JProperty("RateTotal", RateTotal));
            //    jObjectSellPoint.Add(new JProperty("Userd", Userd));
            //    jObjectSellPoint.Add(new JProperty("PicNum", PicNum));
            //    foreach (var keyValue in dic)
            //    {
            //        jObjectSellPoint.Add(new JProperty(keyValue.Key, keyValue.Value));
            //    }

            //    UserId = Regex.Match(HtmlSource, @"(?<=userId=)\d+(?="")").Value;

            //}
            //else
            //{
            //    ItemTypeName = "taobao";
            //    _taobao_displayHtml = GetMainWebContent($"{_taobao_displayUrl}{_productId}", null, ref cookies, "");

            //    //CollectionNumber
            //    var getUrl = Regex.Match(_taobao_displayHtml, @"(?<=counterApi[\s]*:[\s]*').*?(?=')").Value;
            //    if (!getUrl.Equals(stringEmpty))
            //    {
            //        var callback = $"jsonp{Random.Next(100, 999)}";
            //        var dateTime = new DateTime();
            //        var start = new DateTime(1970, 1, 1, 0, 0, 0, dateTime.Kind);
            //        var t = Convert.ToInt64((DateTime.Now - start).TotalSeconds);
            //        var _ksTS = $"{t}_{Random.Next(100, 999)}";

            //        //_taobao_counter3Url = $"https:{getUrl}&callback={callback}&_ksTS={_ksTS}";
            //        //_taobao_counter3Html = GetMainWebContent(_taobao_counter3Url, null, ref cookies, "");
            //        CollectionNumber = Regex.Match(_taobao_counter3Html, $@"(?<=ICCP_1_{_productId}"":)\d+").Value;

            //        //下面这个可以单独判断 不过讲道理 前面的找不到 这里也找不到了(谁和你讲道理)
            //        _taobao_detailCommonUrl = $"https:{Regex.Match(_taobao_displayHtml, "(?<=data-commonApi = \").*?(?=\")").Value.Replace("&amp;", "&")}";
            //        _taobao_detailCommonHtml = GetMainWebContent(_taobao_detailCommonUrl, null, ref cookies, "");
            //        var Correspond = Regex.Match(_taobao_detailCommonHtml, "(?<=\"correspond\":\").*?(?=\")").Value;
            //        var Total = Regex.Match(_taobao_detailCommonHtml, @"(?<=""totalFull"":)\d+").Value;
            //        var GoodFull = Regex.Match(_taobao_detailCommonHtml, @"(?<=""goodFull"":)\d+").Value;
            //        var Additional = Regex.Match(_taobao_detailCommonHtml, @"(?<=""additional"":)\d+").Value;
            //        var Normal = Regex.Match(_taobao_detailCommonHtml, @"(?<=""normal"":)\d+").Value;
            //        var Pic = Regex.Match(_taobao_detailCommonHtml, @"(?<=""pic"":)\d+").Value;
            //        var Bad = Regex.Match(_taobao_detailCommonHtml, @"(?<=""bad"":)\d+").Value;

            //        jObjectSellPoint.Add(new JProperty("ItemTypeName", ItemTypeName));
            //        jObjectSellPoint.Add(new JProperty("Total", Total));
            //        jObjectSellPoint.Add(new JProperty("GoodFull", GoodFull));
            //        jObjectSellPoint.Add(new JProperty("Normal", Normal));
            //        jObjectSellPoint.Add(new JProperty("Bad", Bad));
            //        jObjectSellPoint.Add(new JProperty("Additional", Additional));
            //        jObjectSellPoint.Add(new JProperty("Pic", Pic));

            //    }
            //    else
            //    {
            //        CollectionNumber = stringEmpty;
            //    }





            //    UserId = Regex.Match(HtmlSource, @"(?<=""userNumId"":"")\d+(?="")").Value;


            //}

            //tmall
            if (ItemTypeName.ToLower().Equals("tmall"))
            {

                var sellerId = Regex.Match(HtmlSource, @"(?<=""sellerId"":"")\d+(?="")").Value;
                _tmall_list_dsr_infoHtml =
                    GetMainWebContent($"{_tmall_list_dsr_infoUrl}itemId={_productId}&sellerId={sellerId}",
                        null, ref cookies, "");
                var GradeAvg = Regex.Match(_tmall_list_dsr_infoHtml, "(?<=\"gradeAvg\":).*?(?=,)").Value;
                var RateTotal = Regex.Match(_tmall_list_dsr_infoHtml, "(?<=\"rateTotal\":).*?(?=,)").Value;


                jObjectSellPoint.Add(new JProperty("ItemTypeName", ItemTypeName));
                jObjectSellPoint.Add(new JProperty("GradeAvg", GradeAvg));
                jObjectSellPoint.Add(new JProperty("RateTotal", RateTotal));
                

            }
            //taobao
            else
            {
                ItemTypeName = "taobao";
                //@"(?<=sellerId=)\d+"
                var userNumId = Regex.Match(HtmlSource, @"(?<=""SELLER_ID"":"")\d+(?="")").Value;
                _taobao_detailCommonUrl = $"https://rate.taobao.com/detailCommon.htm?userNumId={userNumId}&auctionNumId={_productId}";
                _taobao_detailCommonHtml = GetMainWebContent(_taobao_detailCommonUrl, null, ref cookies, "");
                var Correspond = Regex.Match(_taobao_detailCommonHtml, "(?<=\"correspond\":\").*?(?=\")").Value;
                var Total = Regex.Match(_taobao_detailCommonHtml, @"(?<=""totalFull"":)\d+").Value;
                var GoodFull = Regex.Match(_taobao_detailCommonHtml, @"(?<=""goodFull"":)\d+").Value;
                var Additional = Regex.Match(_taobao_detailCommonHtml, @"(?<=""additional"":)\d+").Value;
                var Normal = Regex.Match(_taobao_detailCommonHtml, @"(?<=""normal"":)\d+").Value;
                var Pic = Regex.Match(_taobao_detailCommonHtml, @"(?<=""pic"":)\d+").Value;
                var Bad = Regex.Match(_taobao_detailCommonHtml, @"(?<=""bad"":)\d+").Value;

                jObjectSellPoint.Add(new JProperty("ItemTypeName", ItemTypeName));
                jObjectSellPoint.Add(new JProperty("Total", Total));
                jObjectSellPoint.Add(new JProperty("GoodFull", GoodFull));
                jObjectSellPoint.Add(new JProperty("Normal", Normal));
                jObjectSellPoint.Add(new JProperty("Bad", Bad));
                jObjectSellPoint.Add(new JProperty("Additional", Additional));
                jObjectSellPoint.Add(new JProperty("Pic", Pic));

                UserId = Regex.Match(HtmlSource, @"(?<=""userNumId"":"")\d+(?="")").Value;
            }


            jObjectSellPoint.Add("Starts", Starts);




            


            //var ProductId = Regex.Match(HtmlSource, @"(?<=""itemId"":"")\d+(?="")").Value;
            var ProductName = Regex.Match(HtmlSource, "(?<=,\"title\":\").*?(?=\")").Value;
            var ProductStateText = Regex.Match(HtmlSource, "(?<=\"ret\":).*?(?=,)").Value;
            var ProductImageUrl = Regex.Match(HtmlSource, @"(?<=""picsPath"":\["").*?(?="")").Value;
            if (ProductImageUrl.Equals(string.Empty))
                ProductImageUrl = Regex.Match(HtmlSource, "(?<=\"imgUrl\":\").*?(?=\")").Value;


            //活动数据
            if (!string.IsNullOrEmpty(ProductStateText)&&!ProductStateText.Equals("[\"ERRCODE_QUERY_DETAIL_FAIL::宝贝不存在\"]"))
            {

                var typeValue = JObject.Parse(HtmlSource)["data"]["apiStack"];
                //里面是个array[1]
                var typeValueString = JArray.Parse(typeValue.ToString())[0]["value"].ToString();
                typeValueString = Regex.Match(typeValueString, @"(?<=""priceUnits"":\[).*?(?=\])").Value;
                var names = Regex.Matches(typeValueString, "(?<=\"name\":\").*?(?=\")");

                //打折秒杀 限时打折
                var DaZheMiaoSha = 0;
                //淘金币 淘金币价 淘金币
                var TaoJinBi = 0;
                //聚划算 
                var JuHuaSuan = 0;
                //天天特价
                var TianTianTeJia = 0;

                if (ProductName.Contains("天天特价"))
                    TianTianTeJia = 1;


                foreach (Match name in names)
                {
                    var value = name.Value;
                    if (value.Equals("限时打折"))
                    {
                        DaZheMiaoSha = 1;
                    }
                    else if (value.Equals("聚划算"))
                    {
                        JuHuaSuan = 1;
                    }
                    else if (value.Equals("天天特价"))
                    {
                        TianTianTeJia = 1;
                    }
                    else if (value.Contains("淘金币"))
                    {
                        TaoJinBi = 1;
                    }
                }
                jObjectSellPoint.Add("DaZheMiaoSha", DaZheMiaoSha);
                jObjectSellPoint.Add("TaoJinBi", TaoJinBi);
                jObjectSellPoint.Add("JuHuaSuan", JuHuaSuan);
                jObjectSellPoint.Add("TianTianTeJia", TianTianTeJia);
            }


            var MianYunFei = 0;
            if (Regex.Match(HtmlSource, @"(?<=\\""subInfos\\"":).*?(?=})").Value.Contains("免运费"))
                MianYunFei = 1;
            jObjectSellPoint.Add("MianYunFei", MianYunFei);

            SellPoint = jObjectSellPoint.ToString();


            



            var ProductLocation = Regex.Match(HtmlSource, "(?<=\"location\":\").*?(?=\")").Value;
            var ShopId = Regex.Match(HtmlSource, @"(?<=""shopId"":"")\d+(?="")").Value;
            var RangePriceMatches = Regex.Matches(HtmlSource, @"(?<=\\""rangePrice\\"":\\"").*?(?=\\"")");
            var PromotionPriceString = stringEmpty;
            var ReservePriceString = stringEmpty;

            if (RangePriceMatches.Count >=2)
            {
                PromotionPriceString = RangePriceMatches[0].Value;
                ReservePriceString = RangePriceMatches[1].Value;
            }

            double? PromotionPrice;
            double? ReservePrice;
            double? ProductPrice;
            double? ProductPriceMax;

            if (PromotionPriceString.Contains("-"))
            {
                ProductPrice = StringToDouble(Regex.Match(PromotionPriceString, ".*(?=-)").Value);
                ProductPriceMax = StringToDouble(Regex.Match(PromotionPriceString, "(?<=-).*").Value);
                PromotionPrice = ProductPrice;
            }
            else
            {
                ProductPrice = ProductPriceMax = PromotionPrice = StringToDouble(PromotionPriceString);
            }

            int? ProductQuantity = StringToInt(Regex.Match(HtmlSource, @"(?<=\\""quantity\\"":\\"")\d+(?=\\"")").Value);

            ReservePrice = StringToDouble(ReservePriceString.Contains("-") ? Regex.Match(ReservePriceString, ".*(?=-)").Value : ReservePriceString);


            int? SellCountMonthly = StringToInt(Regex.Match(HtmlSource, @"(?<=\\""totalSoldQuantity\\"":\\"")\d+(?=\\"")").Value);
            int? TotalCommentCount = StringToInt(Regex.Match(HtmlSource, @"(?<=""rateCounts"":"")\d+(?="")").Value);

            var PromotionType = stringEmpty;
            DateTime? PromotionStartTime = null;
            DateTime? PromotionEndTime = null;
            var CategoryId = Regex.Match(HtmlSource, @"(?<=""categoryId"":"")\d+(?="")").Value;
            var RootCatId = stringEmpty;
            var BrandId = Regex.Match(HtmlSource, @"(?<=""brandId"":"")\d+(?="")").Value;
            var Brand = stringEmpty;
            //var UserId = Regex.Match(HtmlSource, @"(?<=userId=)\d+(?="")").Value;
            var SpuId = stringEmpty;
            var EncryptUserId = stringEmpty;
            var BossNickName = stringEmpty;
            var FanCount = Regex.Match(HtmlSource, @"(?<=""fansCount"":"")\d+(?="")").Value;
            var CreditLevel = Regex.Match(HtmlSource, @"(?<=""creditLevel"":"")\d+(?="")").Value;
            //var ProductDescription = HtmlSource;
            var ProductDescription = stringEmpty;

            jObjectSellPoint.Add(new JProperty("CollectionNumber", CollectionNumber));
            jObjectSellPoint.Add(new JProperty("ShopId",ShopId));
            jObjectSellPoint.Add(new JProperty("FanCount",FanCount));
            jObjectSellPoint.Add(new JProperty("CreditLevel", CreditLevel));

            SellPoint = jObjectSellPoint.ToString();



            if (ProductPrice==null)
            {
                var priceAll = Regex.Matches(HtmlSource, @"(?<=\\""price\\"":\\"").*?(?=\\"")");
                foreach (Match price in priceAll)
                {
                    var value = price.Value;
                    double? valuePrice;
                    valuePrice = StringToDouble(value.Contains("-") ? Regex.Match(PromotionPriceString, ".*(?=-)").Value : value);

                    if (valuePrice != null)
                    {
                        ProductPrice = ProductPriceMax = ReservePrice = PromotionPrice = valuePrice;
                        break;
                    }
                }

                //double? priceAll = StringToDouble(Regex.Match(HtmlSource, @"(?<=\\""price\\"":\\"").*?(?=\\"")").Value);

                //ProductPrice = ProductPriceMax = ReservePrice = PromotionPrice = priceAll;
            }


            //ProductId itemId
            //ProductName title
            //ProductStateText ret
            //SellPoint evaluateInfo
            //ProductImageUrl imgUrl
            //ProductDescription 
            //ProductLocation location
            //ShopId shopId
            //ProductPrice rangePrice
            //ProductPriceMax rangePrice
            //ProductQuantity quantity
            //ReservePrice 价格中的小值
            //SellCountMonthly totalSoldQuantity
            //TotalCommentCount rateCounts
            //PromotionPrice 同productPrice
            //PromotionType 空
            //PromotionStartTime 空
            //PromotionEndTime 空
            //CategoryId categoryId
            //RootCatId 空
            //BrandId brandId
            //Brand 空
            //UserId userId
            //SpuId 空
            //EncryptUserId 空
            //BossNickName 空
            //FanCount fansCount
            //CreditLevel creditLevel
            //Content json内容

            var resultList = new List<IResut>();

            IResut resut = new Resut()
            {
                {"ProductId", _productId},
                {"ProductName",ProductName},
                {"ProductStateText",ProductStateText },
                {"SellPoint",SellPoint },
                {"ProductImageUrl",ProductImageUrl },
                {"ProductDescription",ProductDescription },
                {"ProductLocation", ProductLocation},
                {"ShopId",_shopId },
                {"ProductPrice",ProductPrice },
                {"ProductPriceMax",ProductPriceMax },
                {"ProductQuantity",ProductQuantity },
                {"ReservePrice",ReservePrice },
                {"SellCountMonthly",SellCountMonthly },
                {"TotalCommentCount",TotalCommentCount },
                {"PromotionPrice",PromotionPrice },
                {"PromotionType",PromotionType },
                {"PromotionStartTime",PromotionStartTime},
                {"PromotionEndTime", PromotionEndTime},
                {"CategoryId",CategoryId },
                {"RootCatId" ,RootCatId},
                {"BrandId",BrandId},
                {"Brand" ,Brand},
                {"UserId",UserId },
                {"SpuId",SpuId },
                {"EncryptUserId",EncryptUserId },
                {"BossNickName",BossNickName },
                //{"ShopId" ,ShopId},
                //{"FanCount" ,FanCount},
                //{"CreditLevel" ,CreditLevel},
                //{"Content" ,Content}

            };

            resultList.Add(resut);

            return resultList.ToArray();
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
        /// StringToDouble
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private double? StringToDouble(string text)
        {
            double value;
            if (double.TryParse(text, out value))
            {
                return value;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// StringToInt
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private int? StringToInt(string text)
        {
            int value;
            if (int.TryParse(text, out value))
            {
                return value;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// ParseNextUrl
        /// </summary>
        /// <returns></returns>
        protected override string ParseNextUrl()
        {
            return null;
        }

        /// <summary>
        /// ParseCountPage
        /// </summary>
        /// <returns></returns>
        protected override int ParseCountPage()
        {
            return 1;
        }

        /// <summary>
        /// ParseCurrentPage
        /// </summary>
        /// <returns></returns>
        protected override int ParseCurrentPage()
        {
            return 1;
        }
    }
}
