using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using GE.Engine;
using X.CommLib.Net.WebRequestHelper;
using X.GlodEyes.Collectors;
using X.GlodEyes.Collectors.Specialized.JingDong;

namespace XFCollection.AliExpress
{
    /// <summary>
    /// 
    /// </summary>
    public class ProductTransactionData:WebRequestCollector<IResut,NormalParameter>
    {

        //private string _url;
        private string _cookies;
        private int _curPage;
        private int _totalPage;
        private Queue<string> _postDataQueue;


        /// <summary>
        /// ProductTransactionData
        /// </summary>
        public ProductTransactionData()
        {
            _postDataQueue = new Queue<string>();
        }

        internal static void Test()
        {
            var parameter = new NormalParameter()
            {
                Keyword = "1240676"
            };

            TestHelp<ProductTransactionData>(parameter);
        }


        /// <summary>
        /// InitFirstUrl
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        protected override string InitFirstUrl(NormalParameter param)
        {
            //1240676
            var url = $"https://it.aliexpress.com/store/feedback-score/{param.Keyword}.html";

            var html = base.GetWebContent(url);
            var theSrc = GetFormatUrl(Regex.Match(html, "(?<=thesrc=\").*(?=\")").Value);
            var cookies1 = string.Empty;
            var feedbackHtml = base.GetMainWebContent(theSrc, null, ref cookies1, null);

            var cookies2 = string.Empty;
            base.GetMainWebContent("https://dmtracking2.alibaba.com/", null, ref cookies2, null);

            _totalPage = GetTotalPageByHtml(feedbackHtml);
            _curPage = 0;
            _cookies = $"{cookies1};{cookies2};cna={GetCna()}";

            var dic = GetPostDataDicByHtml(feedbackHtml);
            InitPostDataQueue(dic);
            
            //dic["page"] = "1";
            //var postDataCur = WebRequestCtrl.BuildPostDatas(dic, Encoding.UTF8);
            //var postHtml = GetMainWebContent("https://feedback.aliexpress.com/display/evaluationList.htm", postDataCur,
            //    ref _cookies, null);

            return _postDataQueue.Count == 0 ? null : _postDataQueue.Dequeue();
        }

        /// <summary>
        /// GetMainWebContent
        /// </summary>
        /// <param name="nextUrl"></param>
        /// <param name="postData"></param>
        /// <param name="cookies"></param>
        /// <param name="currentUrl"></param>
        /// <returns></returns>
        protected override string GetMainWebContent(string nextUrl, byte[] postData, ref string cookies,string currentUrl)
        {
            
            return base.GetWebContent("https://feedback.aliexpress.com/display/evaluationList.htm", Encoding.Default.GetBytes(nextUrl), ref _cookies, null);
        }

        /// <summary>
        /// ParseCurrentItems
        /// </summary>
        /// <returns></returns>
        protected override IResut[] ParseCurrentItems()
        {
            var resultList = new List<IResut>();
            //var html = base.GetWebContent("https://feedback.aliexpress.com/display/evaluationList.htm",Encoding.UTF8.GetBytes(CurrentUrl),ref _cookies,null);
            var nameVipLevelList = GetNameVipLevelList(HtmlSource);
            var productNameList = GetProductNameList(HtmlSource);
            var totalPriceList = GetTotalPriceList(HtmlSource);
            var feedBackDateList = GetFeedBackDateList(HtmlSource);
            var feedBackContentList = GetFeedBackContentList(HtmlSource);
            var starMList = GetStarMList(HtmlSource);

            var length = nameVipLevelList.Count;
            for (var i = 0; i < length; i++)
            {
                IResut resut = new Resut()
                {
                    ["nameVipLevel"] = nameVipLevelList[i],
                    ["productName"] = productNameList[i],
                    ["totalPrice"] = totalPriceList[i],
                    ["feedBackDate"] = feedBackDateList[i],
                    ["feedBackContent"] = feedBackContentList[i],
                    ["starM"] = starMList[i]
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
            return _postDataQueue.Count == 0 ? null : _postDataQueue.Dequeue(); ;
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
        /// GetFormatUrl
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private string GetFormatUrl(string url)
        {
            if (!url.ToLower().Contains("http"))
            {
                url = $"https:{url}";
            }
            return url;
        }


        /// <summary>
        /// InitPostDataQueue
        /// </summary>
        /// <param name="infoDic"></param>
        private void InitPostDataQueue(Dictionary<string,string> infoDic)
        {
            for (var i = 0; i < _totalPage; i++)
            {
                infoDic["page"] = $"{i} + 1";
                var postDataCur = WebRequestCtrl.BuildPostDatas(infoDic, Encoding.UTF8);
                _postDataQueue.Enqueue(Encoding.Default.GetString(postDataCur));
            }    
        }




        /// <summary>
        /// GetNameVipLevelList
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        private List<string> GetNameVipLevelList(string html)
        {
            var nameVipLevelCollection = Regex.Matches(html, @"(?<=<span class=""name vip-level"">.*)[^>]*\.(?=</a>|[\s]*<b)");
            //if(nameVipLevelCollection.Count!=10)
            //    throw new Exception("nameVipLevel个数不为10。");
            var nameVipLevelList = (from Match nameVipLevel in nameVipLevelCollection select nameVipLevel.Value).ToList();
            return nameVipLevelList;
        }

        /// <summary>
        /// GetProductNameList
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        private List<string> GetProductNameList(string html)
        {
            var productNameCollection = Regex.Matches(html, "(?<=<span class=\"product-name\">.*?>).*?(?=</a>)");
            //if(productNameCollection.Count!=10)
            //    throw new Exception("productName的个数不为10。");
            var productNameList = (from Match productName in productNameCollection select productName.Value).ToList();
            return productNameList;
        }

        /// <summary>
        /// GetFeedBackDateList
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        private List<string> GetFeedBackDateList(string html)
        {
            var feedBackDateCollection = Regex.Matches(html, "(?<=<div class=\"feedback-date\">).*(?=</div>)");
            //if(feedBackDateCollection.Count!=10)
            //    throw new Exception("feedBackDate的个数不为10。");
            var feedBackDateList = (from Match feedBackDate in feedBackDateCollection select feedBackDate.Value).ToList();
            return feedBackDateList;
        }


        /// <summary>
        /// GetFeedBackContentList
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        private List<string> GetFeedBackContentList(string html)
        {
            var feedBackContentCollection = Regex.Matches(html, @"(?<=<span class=""c"">)[\s\S]*?(?=</span>)");
            //if (feedBackContentCollection.Count != 10)
            //    throw new Exception("feedBackContent的个数不为10。");
            var feedBackContentList = (from Match feedBackContent in feedBackContentCollection select feedBackContent.Value).ToList();
            return feedBackContentList;
        }

        /// <summary>
        /// GetTotalPriceList
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        private List<string> GetTotalPriceList(string html)
        {
            var totalPriceCollection = Regex.Matches(html, "(?<=<div class=\"total-price\">[^<]*<span>).*?(?=<.span>)");
            //if(totalPriceCollection.Count!=10)
            //    throw new Exception("totalPrice的个数不为10。");
            var totalPriceList = (from Match totalPrice in totalPriceCollection select totalPrice.Value).ToList();
            return totalPriceList;
        }

        /// <summary>
        /// GetStarMList
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        private List<string> GetStarMList(string html)
        {
            var starMCollection = Regex.Matches(html, @"(?<=<div class=""star star-m""><span style=""width:)\d+%(?=;"">)");
            //if(starMCollection.Count != 10)
            //    throw new Exception("starM的个数不为10。");
            var starMList = (from Match starM in starMCollection select starM.Value).ToList();
            return starMList;
        }

        /// <summary>
        /// GetTotalPageByHtml
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        private int GetTotalPageByHtml(string html)
        {
            var pageCollection = Regex.Matches(html, @"(?<=<a class=""list_goto_page"".*>)\d+(?=</a>)");
            var maxPage = 0;
            foreach (Match page in pageCollection)
            {
                var curPage = int.Parse(page.Value);
                maxPage = curPage > maxPage ? curPage : maxPage;
            }

            return maxPage;
        }

        /// <summary>
        /// GetPostDataDicByHtml
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        private Dictionary<string,string> GetPostDataDicByHtml(string html)
        {
            var form = Regex.Match(html, @"<form id[\s\S]*</form>").Value;
            var valueCollection = Regex.Matches(form, "(?<=value=[\"']).*?(?=['\"])");
            var valueList = (from Match value in valueCollection select value.Value).ToList();

            if(valueList.Count!=11)
                throw new Exception("Post参数解析不正确！");
            var dic = new Dictionary<string,string>()
            {
                ["ownerMemberId"] = valueList[0],
                ["companyId"] = valueList[1],
                ["memberType"] = valueList[2],
                ["evalType"] = valueList[3],
                ["month"] = valueList[4],
                ["refreshPage"] = valueList[5],
                ["page"] = valueList[6],
                ["dynamicTab"] = valueList[7],
                ["evaSortValue"] = valueList[8],
                ["_csrf_token"] = valueList[9],
                ["callType"] = valueList[10]
            };

            return dic;
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
            for (var i = 0; i < 24; i++)
            {

                cna += array[random.Next(array.Length)];
            }

            return cna;
        }



        

    }
}
