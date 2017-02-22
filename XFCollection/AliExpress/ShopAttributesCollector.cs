using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using X.GlodEyes.Collectors;
using X.GlodEyes.Collectors.Specialized.JingDong;

namespace XFCollection.AliExpress
{

    /// <summary>
    /// ShopAttributesCollector
    /// </summary>
    public class ShopAttributesCollector: WebRequestCollector<IResut,NormalParameter>
    {
        //https://ykloving123.aliexpress.com/store/feedback-score/1240676.html

        private string _url;

        /// <summary>
        /// Test
        /// </summary>
        internal static void Test()
        {
            var parameter = new NormalParameter()
            {
                Keyword = "1491890"
            };

            TestHelp<ShopAttributesCollector>(parameter);
        }

        /// <summary>
        /// InitFirstUrl
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        protected override string InitFirstUrl(NormalParameter param)
        {
            return _url = $"https://it.aliexpress.com/store/feedback-score/{param.Keyword}.html";
        }


        

        /// <summary>
        /// ParseCurrentItems
        /// </summary>
        /// <returns></returns>
        protected override IResut[] ParseCurrentItems()
        {
            var storeNumber = GetStoreNumber(HtmlSource);
            var storeLocation = GetStoreLocation(HtmlSource);
            var storeTime = GetStoreTime(HtmlSource);
            var evaluationDetailHtml = GetEvaluationDetailHtml(HtmlSource);
            var itemList = GetItemList(evaluationDetailHtml);
            var seller = itemList["seller"];
            var positiveFeedbackPastSixMonths = itemList["positiveFeedbackPastSixMonths"];
            var feedbackScore = itemList["feedbackScore"];
            var aliExpressSellerSince = itemList["aliExpressSellerSince"];
            var described = itemList["described"];
            var describedRatings = itemList["describedRatings"];
            var describedPercent = itemList["describedPercent"];
            var communication = itemList["communication"];
            var communicationRatings = itemList["communicationRatings"];
            var communicationPercent = itemList["communicationPercent"];
            var shippingSpeed = itemList["shippingSpeed"];
            var shippingSpeedRatings = itemList["shippingSpeedRatings"];
            var shippingSpeedPercent = itemList["shippingSpeedPercent"];

            var positiveOneMonth = itemList["positiveOneMonth"];
            var positiveThreeMonths = itemList["positiveThreeMonths"];
            var positiveSixMonths = itemList["positiveSixMonths"];
            var positiveOneYear = itemList["positiveOneYear"];
            var positiveOverall = itemList["positiveOverall"];

            var negativeOneMonth = itemList["negativeOneMonth"];
            var negativeThreeMonths = itemList["negativeThreeMonths"];
            var negativeSixMonths = itemList["negativeSixMonths"];
            var negativeOneYear = itemList["negativeOneYear"];
            var negativeOverall = itemList["negativeOverall"];

            var neutralOneMonth = itemList["neutralOneMonth"];
            var neutralThreeMonths = itemList["neutralThreeMonths"];
            var neutralSixMonths = itemList["neutralSixMonths"];
            var neutralOneYear = itemList["neutralOneYear"];
            var neutralOverAll = itemList["neutralOverAll"];

            var positiveFeedbackRateOneMonth = itemList["positiveFeedbackRateOneMonth"];
            var positiveFeedbackRateThreeMonths = itemList["positiveFeedbackRateThreeMonths"];
            var positiveFeedbackRateSixMonths = itemList["positiveFeedbackRateSixMonths"];
            var positiveFeedbackRateOneYear = itemList["positiveFeedbackRateOneYear"];
            var positiveFeedbackRateOverall = itemList["positiveFeedbackRateOverall"];

            var resultList = new List<IResut>();
            IResut resut = new Resut()
            {
                { "storeNumber",storeNumber },
                { "storeLocation",storeLocation},
                { "storeTime",storeTime },
                { "seller",seller },
                {"positiveFeedbackPastSixMonths",positiveFeedbackPastSixMonths },
                {"feedbackScore", feedbackScore},
                { "aliExpressSellerSince",aliExpressSellerSince },
                { "described",described},
                {"describedRatings",describedRatings},
                { "describedPercent",describedPercent},
                { "communication",communication },
                { "communicationRatings",communicationRatings},
                { "communicationPercent",communicationPercent },
                { "shippingSpeed",shippingSpeed },
                {"shippingSpeedRatings",shippingSpeedRatings },
                { "shippingSpeedPercent",shippingSpeedPercent },
                { "positiveOneMonth",positiveOneMonth },
                { "positiveThreeMonths",positiveThreeMonths},
                { "positiveSixMonths",positiveSixMonths },
                { "positiveOneYear",positiveOneYear },
                {"positiveOverall",positiveOverall },
                {"negativeOneMonth", negativeOneMonth},
                { "negativeThreeMonths",negativeThreeMonths },
                { "negativeSixMonths",negativeSixMonths},
                { "negativeOneYear",negativeOneYear},
                { "negativeOverall",negativeOverall },
                { "neutralOneMonth",neutralOneMonth },
                { "neutralThreeMonths",neutralThreeMonths},
                { "neutralSixMonths",neutralSixMonths },
                { "neutralOneYear",neutralOneYear },
                {"neutralOverAll",neutralOverAll },
                {"positiveFeedbackRateOneMonth", positiveFeedbackRateOneMonth},
                { "positiveFeedbackRateThreeMonths",positiveFeedbackRateThreeMonths },
                { "positiveFeedbackRateSixMonths",positiveFeedbackRateSixMonths},
                { "positiveFeedbackRateOneYear",positiveFeedbackRateOneYear},
                { "positiveFeedbackRateOverall",positiveFeedbackRateOverall }
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
        /// ParseNextUrl
        /// </summary>
        /// <returns></returns>
        protected override string ParseNextUrl()
        {
            return null;
        }

        /// <summary>
        /// ParseCurrentPage
        /// </summary>
        /// <returns></returns>
        protected override int ParseCurrentPage()
        {
            return 1;
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
        /// GetStoreNumber
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        private string GetStoreNumber(string html)
        {
            var tempValue = Regex.Match(html, "(?<=<span class=\"store-number\">).*?(?=</span>)").Value;
            return Regex.Match(tempValue,@"\d+").Value;
        }

        /// <summary>
        /// GetStoreLocation
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        private string GetStoreLocation(string html)
        {
            return Regex.Match(html, @"(?<=<span class=""store-location"">)[\s\S]*?(?=</span>)").Value.Trim();
        }

        /// <summary>
        /// GetStoreTime
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        private string GetStoreTime(string html)
        {
            var storeTime = RemoveEm(Regex.Match(html, @"(?<=<span class=""store-time"">)[\s\S]*?(?=</span>)").Value);
            storeTime = storeTime.Replace("This store has been open since ", "");
            return storeTime = DateTime.Parse(storeTime).ToString();
        }

        /// <summary>
        /// GetEvaluationDetailHtml
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        private string GetEvaluationDetailHtml(string html)
        {
            var iframe = Regex.Match(html, @"<iframe id=""detail-displayer""[\s\S]*?</iframe>").Value;
            var evalutionDetailUrl = GetFormatUrl(Regex.Match(iframe, "(?<=src=\").*?(?=\">)").Value);
            var evalutionDetailHtml = base.GetWebContent(evalutionDetailUrl);
            return evalutionDetailHtml;
        }

        /// <summary>
        /// GetFormatUrl
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private string GetFormatUrl(string url)
        {
            return url.Contains("http") ? url : $"https:{url}";
        }

        /// <summary>
        /// RemoveAmp
        /// </summary>
        /// <param name="ampString"></param>
        /// <returns></returns>
        private string RemoveAmp(string ampString)
        {
            return ampString.Replace("amp;", "");
        }

        /// <summary>
        /// RemoveEmAndSapn
        /// </summary>
        /// <param name="emAndSpanString"></param>
        /// <returns></returns>
        private string RemoveEmAndSapn(string emAndSpanString)
        {
            var score = Regex.Match(emAndSpanString, "(?<=<em>).*(?=</em>)").Value;
            var ratingPeople = Regex.Match(emAndSpanString, "(?<=<span>).*?(?=</span>)").Value;
            return $"{score}{ratingPeople}";
        }

        /// <summary>
        /// RemoveEm
        /// </summary>
        /// <param name="emString"></param>
        /// <returns></returns>
        private string RemoveEm(string emString)
        {
            emString = emString.Replace("<em>","");
            emString = emString.Replace("</em>", "");
            return emString;
        }

        /// <summary>
        /// RemoveTagA
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private string RemoveTagA(string value)
        {
            return Regex.Match(value, @"(?<=<td><.*>).*?(?=[\s]*</a>)").Value;
        }

        /// <summary>
        /// RemoveDot
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private string RemoveDot(string value)
        {
            return value.Replace(",", "");
        }

        /// <summary>
        /// GetFormatPercent
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private string GetFormatPercent(string value)
        {
            
            if (value.Contains("Higher"))
            {
                return $"+{Regex.Match(value, @"\d+\.\d+").Value}";
            }
            else if(value.Contains("Lower"))
            {
                return $"-{Regex.Match(value, @"\d+\.\d+").Value}";
            }
            else
            {
                return $"{Regex.Match(value, @"\d+\.\d+").Value}";
            }
        }



        /// <summary>
        /// GetItemList
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        private Dictionary<string,string> GetItemList(string html)
        {
            //html = base.GetWebContent("https://feedback.aliexpress.com/display/evaluationDetail.htm?ownerMemberId=220581614&companyId=230620884&memberType=seller&callType=iframe&iframe_delete=true");
            var dic = new Dictionary<string,string>();
            var list = new List<string>();
            var valueList = Regex.Matches(html, @"<tr>[\s\S]*?</tr>");
            foreach (Match value in valueList)
            {
                list.Add(value.Value);
            }

            if(list.Count!=7)
                throw new Exception("特殊的网页结构！请处理。");

            var seller = RemoveAmp(Regex.Match(list[0], "(?<=<a.*>).*(?=</a>)").Value);
            dic.Add("seller",seller);
            var positiveFeedbackPastSixMonths = Regex.Match(list[1], "(?<=<td><span>).*(?=</span>)").Value;
            dic.Add("positiveFeedbackPastSixMonths", positiveFeedbackPastSixMonths);
            var feedbackScore = RemoveDot(Regex.Match(list[2], "(?<=<td><span>).*(?=</span>)").Value);
            dic.Add("feedbackScore", feedbackScore);
            var aliExpressSellerSince = Regex.Match(list[3], "(?<=<td>).*(?=</td>)").Value;
            aliExpressSellerSince = DateTime.Parse(aliExpressSellerSince).ToString();
            dic.Add("aliExpressSellerSince", aliExpressSellerSince);
            var describedTemp = RemoveEmAndSapn(Regex.Match(list[4], "(?<=<span class=\"dsr-text\">).*(?=</span>)").Value);
            var described = Regex.Match(describedTemp, @"\d+\.\d+(?=\()").Value;
            var describedRatings = Regex.Match(describedTemp, @"(?<=\()\d+").Value;
            dic.Add("described", described);
            dic.Add("describedRatings", describedRatings);
            var describedPercent = GetFormatPercent(Regex.Match(list[4], "(?<=<em class=\"compare-high\">).*(?=</em>)").Value);
            dic.Add("describedPercent", describedPercent);

            var communicationTemp = RemoveEmAndSapn(Regex.Match(list[5], "(?<=<span class=\"dsr-text\">).*(?=</span>)").Value);
            var communication = Regex.Match(communicationTemp, @"\d+\.\d+(?=\()").Value;
            var communicationRatings = Regex.Match(communicationTemp, @"(?<=\()\d+").Value;
            dic.Add("communication", communication);
            dic.Add("communicationRatings", communicationRatings);
            var communicationPercent = GetFormatPercent(Regex.Match(list[5], "(?<=<em class=\"compare-high\">).*(?=</em>)").Value);
            dic.Add("communicationPercent", communicationPercent);
            var shippingSpeedTemp= RemoveEmAndSapn(Regex.Match(list[6], "(?<=<span class=\"dsr-text\">).*(?=</span>)").Value);
            var shippingSpeed = Regex.Match(shippingSpeedTemp, @"\d+\.\d+(?=\()").Value;
            var shippingSpeedRatings = Regex.Match(shippingSpeedTemp, @"(?<=\()\d+").Value;
            dic.Add("shippingSpeed", shippingSpeed);
            dic.Add("shippingSpeedRatings", shippingSpeedRatings);
            var shippingSpeedPercent = GetFormatPercent(Regex.Match(list[6], "(?<=<em class=\"compare-high\">).*(?=</em>)").Value);
            dic.Add("shippingSpeedPercent", shippingSpeedPercent);

            //positive and negative months , one year , overAll

            var blueMatches = Regex.Matches(html, @"<tr class=""blue"">[\s\S]*?</tr>");
            var blueList = new List<string>();
            foreach (Match blue in blueMatches)
            {
                var positiveAndNegativeMatches = Regex.Matches(blue.Value, @"(?<=<td><.*>).*?(?=[\s]*</a>)");
                foreach (Match positiveAndNegative in positiveAndNegativeMatches)
                {
                    blueList.Add(positiveAndNegative.Value);
                }
            }
            
            if(blueList.Count !=10)
                throw new Exception("positiveAndNegative数据格式不正确，请检查！");



            var positiveOneMonth = RemoveDot(blueList[0]);
            var positiveThreeMonths = RemoveDot(blueList[1]);
            var positiveSixMonths = RemoveDot(blueList[2]);
            var positiveOneYear = RemoveDot(blueList[3]);
            var positiveOverall = RemoveDot(blueList[4]);
            dic.Add("positiveOneMonth", positiveOneMonth);
            dic.Add("positiveThreeMonths", positiveThreeMonths);
            dic.Add("positiveSixMonths", positiveSixMonths);
            dic.Add("positiveOneYear", positiveOneYear);
            dic.Add("positiveOverall", positiveOverall);

            var negativeOneMonth = RemoveDot(blueList[5]);
            var negativeThreeMonths = RemoveDot(blueList[6]);
            var negativeSixMonths = RemoveDot(blueList[7]);
            var negativeOneYear = RemoveDot(blueList[8]);
            var negativeOverall = RemoveDot(blueList[9]);
            dic.Add("negativeOneMonth", negativeOneMonth);
            dic.Add("negativeThreeMonths", negativeThreeMonths);
            dic.Add("negativeSixMonths", negativeSixMonths);
            dic.Add("negativeOneYear", negativeOneYear);
            dic.Add("negativeOverall", negativeOverall);


            //neutral one three six months , one year , overAll 
            var blueEven = Regex.Match(html, @"<tr class=""blue even"">[\s\S]*?</tr>").Value;
            var neutralMatches = Regex.Matches(blueEven, @"(?<=<td><.*>).*?(?=[\s]*</a>)");
            var blueEvenList = new List<string>();
            foreach (Match neutral in neutralMatches)
            {
                blueEvenList.Add(neutral.Value);
            }
            if(blueEvenList.Count!=5)
                throw new Exception("Neutral数据格式不正确，请检查！");

            var neutralOneMonth = RemoveDot(blueEvenList[0]);
            var neutralThreeMonths = RemoveDot(blueEvenList[1]);
            var neutralSixMonths = RemoveDot(blueEvenList[2]);
            var neutralOneYear = RemoveDot(blueEvenList[3]);
            var neutralOverAll = RemoveDot(blueEvenList[4]);
            dic.Add("neutralOneMonth", neutralOneMonth);
            dic.Add("neutralThreeMonths", neutralThreeMonths);
            dic.Add("neutralSixMonths", neutralSixMonths);
            dic.Add("neutralOneYear", neutralOneYear);
            dic.Add("neutralOverAll", neutralOverAll);



            //positiveFeedbackRate one three six months , one year , overAll 
            var even = Regex.Match(html, @"<tr class=""even"">[\s\S]*?</tr>").Value;
            var positiveFeedbackRateMatches = Regex.Matches(even, "(?<=<td>).*?(?=</td>)");
            var evenList = new List<string>();
            foreach (Match positiveFeedbackRate in positiveFeedbackRateMatches)
            {
                evenList.Add(positiveFeedbackRate.Value);
            }
            if (evenList.Count != 5)
                throw new Exception("positiveFeedbackRate数据格式不正确，请检查！");

            var positiveFeedbackRateOneMonth = RemoveDot(evenList[0]);
            var positiveFeedbackRateThreeMonths = RemoveDot(evenList[1]);
            var positiveFeedbackRateSixMonths = RemoveDot(evenList[2]);
            var positiveFeedbackRateOneYear = RemoveDot(evenList[3]);
            var positiveFeedbackRateOverall = RemoveDot(evenList[4]);
            dic.Add("positiveFeedbackRateOneMonth", positiveFeedbackRateOneMonth);
            dic.Add("positiveFeedbackRateThreeMonths", positiveFeedbackRateThreeMonths);
            dic.Add("positiveFeedbackRateSixMonths", positiveFeedbackRateSixMonths);
            dic.Add("positiveFeedbackRateOneYear", positiveFeedbackRateOneYear);
            dic.Add("positiveFeedbackRateOverall", positiveFeedbackRateOverall);


            return dic;
        }


    }
}
