using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using GE.Data;
using X.GlodEyes.Collectors;
using X.GlodEyes.Collectors.Specialized.JingDong;


namespace XFCollection.JingDong
{

    /// <summary>
    /// 京东产品详情收集
    /// </summary>
    public class JDShopDetailCollector : WebRequestCollector<IResut, NormalParameter>
    {

        /// <summary>
        /// 测试
        /// </summary>
        internal static void Test()
        {
            var parameter = new NormalParameter {Keyword = @"49600" };
            TestHelp<JDShopDetailCollector>(parameter, 10);
        }

        /// <summary>
        /// 测试1
        /// </summary>
        public static void Test1()
        {
            var shopIds = File.ReadAllLines(@"C:\Users\Administrator\Desktop\test.txt");

            /*var shopIds = File.ReadAllLines(@"C:\Users\sinoX\Desktop\errorList.txt");*/
            // 去掉字符串前后的"
            shopIds = Array.ConvertAll(shopIds, shopId => shopId.Trim('"'));

            foreach (var shopId in shopIds)
            {
                Console.WriteLine();
                Console.WriteLine($"shop id: {shopId}");
                try
                {
                    var parameter = new NormalParameter { Keyword = shopId };
                    TestHelp<JDShopDetailCollector>(parameter, 1);
                }
                catch (NotSupportedException exception)
                {
                    Console.WriteLine($"error: {exception.Message}");
                }
            }
        }

        private string _shopId;

        /// <summary>
        /// 初始化链接
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        protected override string InitFirstUrl(NormalParameter param)
        {
            _shopId = param.Keyword;
            if(_shopId.Equals(""))
                throw new Exception("传入的ShopId为空，请检查！");
            return $"http://mall.jd.com/shopLevel-{param.Keyword}.html";
        }


        /// <summary>
        /// 解析当前元素
        /// </summary>
        /// <returns></returns>
        protected override IResut[] ParseCurrentItems()
        {

            var resultList = new List<IResut>();
            var shopIsExist = GetShopIsExist(HtmlSource);
            Resut resut;

            if (shopIsExist.Equals("0"))
            {
                var stringEmpty = string.Empty;
                resut = new Resut
                {
                    //店铺ID
                    ["ShopId"] = _shopId,
                    //品牌简介链接
                    ["BrandProfile"] = stringEmpty,
                    //品牌
                    ["ShopIsExist"] = shopIsExist,
                    //公司名称
                    ["ShopName"] = stringEmpty,
                    //所在地
                    ["Location"] = stringEmpty,
                    //电话
                    ["Phone"] = stringEmpty,
                    //违章次数
                    ["IllegalRecord"] = stringEmpty,
                    //店铺综合评分
                    ["Comment_General"] = stringEmpty,
                    //店铺综合评分比率
                    ["Comment_GeneralRate"] = stringEmpty,
                    //店铺质量满意度
                    ["Comment_MatchDescrip"] = stringEmpty,
                    //店铺质量满意度比率
                    ["Comment_MatchDescripRate"] = stringEmpty,
                    //服务态度满意度
                    ["Comment_ServiceStatue"] = stringEmpty,
                    //服务态度满意度比率
                    ["Comment_ServiceStatueRate"] = stringEmpty,
                    //物流速度满意度
                    ["Comment_ShipSpeed"] = stringEmpty,
                    //物流速度满意度比率
                    ["Comment_ShipSpeedRate"] = stringEmpty,
                    //商品描述满意度
                    ["Comment_ProductDescrip"] = stringEmpty,
                    //商品描述满意度比率
                    ["Comment_ProductDescripRate"] = stringEmpty,
                    //退换货处理满意度
                    ["Comment_ReturnGoods"] = stringEmpty,
                    //退换货处理满意度比率
                    ["Comment_ReturnGoodsRate"] = stringEmpty,
                    //售后处理时长
                    ["Servece_AfterSales"] = stringEmpty,
                    //售后处理时长比率
                    ["Servece_AfterSalesRate"] = stringEmpty,
                    //交易纠纷率
                    ["Service_TradeDispute"] = stringEmpty,
                    //交易纠纷率比率
                    ["Service_TradeDisputeRate"] = stringEmpty,
                    //退换货返修率
                    ["Service_ReturnRepair"] = stringEmpty,
                    //退换货返修率比率
                    ["Service_ReturnRepairRate"] = stringEmpty,
                    //关注人数
                    ["FollowNumber"] = stringEmpty,
                    //全部商品
                    ["ProductsNum"] = stringEmpty,
                    //上新
                    ["NewProducts"] = stringEmpty,
                    //促销
                    ["PromotionNum"] = stringEmpty,
                    //开店时间
                    ["OpenTime"] = stringEmpty
                };

            }
            else
            {

                var brandProfile = GetBrandProfile(HtmlSource);
                var shopName = GetShopName(HtmlSource);
                var location = GetLocation(HtmlSource);
                var phone = GetPhone(HtmlSource);
                var illegalRecord = GetIllegalRecord(HtmlSource);
                var shopComment = GetShopComment(HtmlSource);
                var shopService = GetShopService(HtmlSource);
                var url = $"http://shop.m.jd.com/detail/detail?shopId={_shopId}";
                var html = base.GetWebContent(url);
                var followNumber = GetFollowNumber(html);
                var totalNumDic = GetTotalNumDic(html);
                var openTime = GetOpenTime(html);

                resut = new Resut
                {
                    //店铺ID
                    ["ShopId"] = _shopId,
                    //品牌简介链接
                    ["BrandProfile"] = brandProfile,
                    //品牌
                    ["ShopIsExist"] = shopIsExist,
                    //公司名称
                    ["ShopName"] = shopName,
                    //所在地
                    ["Location"] = location,
                    //电话
                    ["Phone"] = phone,
                    //违章次数
                    ["IllegalRecord"] = illegalRecord,
                    //店铺综合评分
                    ["Comment_General"] = shopComment["Comment_General"],
                    //店铺综合评分比率
                    ["Comment_GeneralRate"] = shopComment["Comment_GeneralRate"],
                    //店铺质量满意度
                    ["Comment_MatchDescrip"] = shopComment["Comment_MatchDescrip"],
                    //店铺质量满意度比率
                    ["Comment_MatchDescripRate"] = shopComment["Comment_MatchDescripRate"],
                    //服务态度满意度
                    ["Comment_ServiceStatue"] = shopComment["Comment_ServiceStatue"],
                    //服务态度满意度比率
                    ["Comment_ServiceStatueRate"] = shopComment["Comment_ServiceStatueRate"],
                    //物流速度满意度
                    ["Comment_ShipSpeed"] = shopComment["Comment_ShipSpeed"],
                    //物流速度满意度比率
                    ["Comment_ShipSpeedRate"] = shopComment["Comment_ShipSpeedRate"],
                    //商品描述满意度
                    ["Comment_ProductDescrip"] = shopComment["Comment_ProductDescrip"],
                    //商品描述满意度比率
                    ["Comment_ProductDescripRate"] = shopComment["Comment_ProductDescripRate"],
                    //退换货处理满意度
                    ["Comment_ReturnGoods"] = shopComment["Comment_ReturnGoods"],
                    //退换货处理满意度比率
                    ["Comment_ReturnGoodsRate"] = shopComment["Comment_ReturnGoodsRate"],
                    //售后处理时长
                    ["Servece_AfterSales"] = shopService["Servece_AfterSales"],
                    //售后处理时长比率
                    ["Servece_AfterSalesRate"] = shopService["Servece_AfterSalesRate"],
                    //交易纠纷率
                    ["Service_TradeDispute"] = shopService["Service_TradeDispute"],
                    //交易纠纷率比率
                    ["Service_TradeDisputeRate"] = shopService["Service_TradeDisputeRate"],
                    //退换货返修率
                    ["Service_ReturnRepair"] = shopService["Service_ReturnRepair"],
                    //退换货返修率比率
                    ["Service_ReturnRepairRate"] = shopService["Service_ReturnRepairRate"],
                    //关注人数
                    ["FollowNumber"] = followNumber,
                    //全部商品
                    ["ProductsNum"] = totalNumDic["ProductsNum"],
                    //上新
                    ["NewProducts"] = totalNumDic["NewProductsNum"],
                    //促销
                    ["PromotionNum"] = totalNumDic["PromotionNum"],
                    //开店时间
                    ["OpenTime"] = openTime
                };
            }
            resultList.Add(resut);
            return resultList.ToArray();
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
        /// 得到关注人数
        /// </summary>
        /// <param name="htmlString"></param>
        /// <returns></returns>
        public string GetFollowNumber(string htmlString)
        {
            return Regex.Match(htmlString, "(?<=<em class=\"follow-number\">).*(?=人关注</em>)").Value;
        }

        /// <summary>
        /// 得到全部商品，上新，促销数
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, string> GetTotalNumDic(string htmlString)
        {
            IList<string> key = new List<string>
            {
                "ProductsNum",
                "NewProductsNum",
                "PromotionNum"
            };

            var matchs = Regex.Matches(htmlString, "(?<=<span class=\"total-num\">).*(?=</span>)");

            IList<string> value = (from Match match in matchs select match.Value).ToList();

            if(value.Count!=key.Count)
                throw new Exception("商品信息不匹配！");


            Dictionary<string, string> totalNumDic = new Dictionary<string, string>();

            for (int i = 0; i < key.Count; i++)
                totalNumDic.Add(key[i],value[i]);

            return totalNumDic;

        }

        /// <summary>
        /// 得到开店时间
        /// </summary>
        /// <param name="htmlString"></param>
        /// <returns></returns>
        public string GetOpenTime(string htmlString)
        {
            return Regex.Match(htmlString, @"(?<=<span class=""cell-title"">开店时间</span>[\s]*<span class=""cell cell-desc"">).*(?=</span>)").Value;
        }




        /// <summary>
        /// 得到品牌简介链接
        /// </summary>
        /// <param name="htmlString">网页源码</param>
        /// <returns></returns>
        public string GetBrandProfile(string htmlString)
        {
            return Regex.Match(htmlString, @"(?<=class=""j-shop-logo"">[\s]*<img src="")[\S]*(?=""[\s]*alt)").Value;
        }
        /// <summary>
        /// 得到品牌
        /// </summary>
        /// <param name="htmlString"></param>
        /// <returns></returns>
        public string GetShopIsExist(string htmlString)
        {
            if (Regex.Match(htmlString, "url=//www.jd.com/error.aspx").Success)
                return "0";
            else 
                return "1";
        }
        

        /// <summary>
        /// 得到公司名称
        /// </summary>
        /// <param name="htmlString">网页源码</param>
        /// <returns></returns>
        public string GetShopName(string htmlString)
        {
            return Regex.Match(htmlString, @"(?<=class=""j-shop-name"">[\s]*)[\S]*?(?=[\s]*</p>)").Value;
            //return Regex.Match(htmlString, @"(?<=公司名称：</span>[\s]*<span class=""value"">)[\S]*(?=</span>)").Value;
        }

        /// <summary>
        /// 得到所在地
        /// </summary>
        /// <param name="htmlString">网页源码</param>
        /// <returns></returns>
        public string GetLocation(string htmlString)
        {
            return Regex.Match(htmlString, @"(?<=所在地：</span>[\s]* <span class=""value"">).*(?=</span>)").Value.Trim();
        }

        /// <summary>
        /// 得到电话号码
        /// </summary>
        /// <param name="htmlStirng"></param>
        /// <returns></returns>
        public string GetPhone(string htmlStirng)
        {
            return Regex.Match(htmlStirng, @"(?<=class=""phone"">[\s]*<i></i>)[\S]*(?=[\s])").Value;
        }

        /// <summary>
        /// 得到违章记录
        /// </summary>
        /// <param name="htmlString"></param>
        /// <returns></returns>
        public string GetIllegalRecord(string htmlString)
        {
            return Regex.Match(htmlString, @"(?<=a href=""javascript:void\(0\);"">).*(?=</a>)").Value;
        }


        /// <summary>
        /// 得到90天内平台监控店铺服务，包括售后处理时长 交易纠纷率 退换货返修率
        /// </summary>
        /// <param name="htmlString"></param>
        /// <returns></returns>
        public IDictionary<string, string> GetShopService(string htmlString)
        {
            IDictionary<string,string> dic = new Dictionary<string, string>();

            //先解析出优于还是劣于
            var matchs = Regex.Matches(htmlString, @"(?<=<span class=""result"">[\s]*)[\S]*?(?=[\s]*</span>)");
            IList<string> listCompare = (from Match match in matchs select match.Value).ToList();

            //前缀名字
            IList<string> listName = new List<string>
            {
                "Servece_AfterSales",
                "Servece_AfterSalesRate",
                "Service_TradeDispute",
                "Service_TradeDisputeRate",
                "Service_ReturnRepair",
                "Service_ReturnRepairRate"
            };


            //解析出评价 
            matchs = Regex.Matches(htmlString, @"(?<=<span class=""f16 value("" k=""[\S]* )?"">)[\S]*(?=</span>)");
            IList<string> listComment = (from Match match in matchs select match.Value).ToList();

            //组成dic
            for (var i = 0; i < listComment.Count; i++)
            {
                var comment = listComment[i];
                var name = listName[i];

                if ((i + 1)%2 == 0)
                {
                    var compare = listCompare[i/2];
                    if (compare.Equals("优于"))
                        dic.Add(name, (comment.Equals("h")||comment.Equals("%"))? $"{comment}":$"+{comment}");
                    else if(compare.Equals("劣于"))
                        dic.Add(name, (comment.Equals("h") || comment.Equals("%"))?$"{comment}":$"-{comment}");
                    else
                        dic.Add(name, $"{comment}");
                }
                else
                    dic.Add(name, comment);
            }

            return dic;
        }



        /// <summary>
        /// 180天内店铺动态评分
        /// </summary>
        /// <param name="htmlString">网页源码</param>
        /// <returns></returns>
        public IDictionary<string, string> GetShopComment(string htmlString)
        {
//#if DEBUG
//            MessageBox.Show("test");
//#endif
            IDictionary<string, string> dic = new Dictionary<string, string>();


            //list key
            IList<string> listKey = new List<string>
            {
                "Comment_General",
                "Comment_GeneralRate",
                "Comment_MatchDescrip",
                "Comment_MatchDescripRate",
                "Comment_ServiceStatue",
                "Comment_ServiceStatueRate",
                "Comment_ShipSpeed",
                "Comment_ShipSpeedRate",
                "Comment_ProductDescrip",
                "Comment_ProductDescripRate",
                "Comment_ReturnGoods",
                "Comment_ReturnGoodsRate"
            };


            //评分百分比 包括综合评分
            var matchs = Regex.Matches(htmlString, "(?<=class=\"percent\">).*(?=</span>)");
            IList<string> listRate = (from Match match in matchs select match.Value).ToList();


            //评分比较颜色 绿色- 红色+
            IList<string> listCompare = new List<string>();
            //综合评分颜色
            var matchGeneral = Regex.Match(htmlString, @"(?<=class=""total-score-view[\s]*)[\S]*(?=[\s]*"">)");
            listCompare.Add(matchGeneral.Value);
            matchs = Regex.Matches(htmlString, @"(?<=分</span>[\s]*<div class="").*(?="">)");
            foreach (Match match in matchs)
            {
                listCompare.Add(match.Value);
            }


            //评分分数
            IList<string> listScore = new List<string>();
            //综合评分分数
            matchGeneral = Regex.Match(htmlString, @"(?<=class=""total-score-num"">[\s]*<span>).*(?=</span>)");
            listScore.Add(matchGeneral.Value);
            matchs = Regex.Matches(htmlString, "(?<=score-180\">).*(?=</span>)");
            foreach (Match match in matchs)
            {
                listScore.Add(match.Value);
            }


            //listRate    listScore  listCompare
            //23.48 %     9.40 分      green
            //12.78 %     9.47 分      red green
            //26.90 %     9.38 分      green
            //37.13 %     9.31 分      green
            //7.11 %      9.50 分      green
            //            -
            //
            //listCompare计数
            var curIndexCompare = 0;
            //listRate计数
            var curIndexRate = 0;
            for (var i = 0; i < listKey.Count; i++)
            {
                //偶数索引
                if ((i + 1)%2 == 0)
                {

                    //第一种情况 分数为- 
                    if (listScore[(i+1)/2-1].Equals("-"))
                    {
                        dic.Add(listKey[i], "暂无评分");
                    }
                    //第二种情况 全部为空字符串
                    else if (listScore[(i + 1)/2 - 1].Equals(string.Empty))
                    {
                        dic.Add(listKey[i], listRate[curIndexRate]);
                        curIndexRate++;
                    }
                    //第三种情况 正常
                    else
                    {
                        //这里只能放在这个位置，如果放在外面会数组越界
                        var color = listCompare[curIndexCompare];
                        var key = listKey[i];
                        
                        if (color.Equals("red"))
                        {
                            var value = listRate[curIndexRate];
                            dic.Add(key, value.Equals("%")?$"{value}":$"+{value}");
                            curIndexRate++;
                        }
                        //灰色 后面比率 暂无评分
                        else if (color.Equals("gray"))
                        {
                            dic.Add(key, "暂无评分");
                        }
                        else
                        {
                            var value = listRate[curIndexRate];
                            dic.Add(key,value.Equals("%")?$"{value}":$"-{value}");
                            curIndexRate++;
                        }
                        curIndexCompare++;
                    }

                }
                //奇数索引
                else
                {
                    dic.Add(listKey[i], listScore[(i+1)/2]);
                }
            }


            return dic;
        }
    }
}
