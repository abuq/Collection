using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.XPath;
using SHDocVw;
using X.CommLib.Net.Miscellaneous;
using X.CommLib.Office;
using X.GlodEyes.Collectors;
using X.GlodEyes.Collectors.Specialized.JingDong;

namespace XFCollection.JingDong
{


    /// <summary>
    /// 京东店铺商品搜索
    /// </summary>
    public class JDShopProductsCollecor : WebRequestCollector<IResut, NormalParameter>
    {

        /// <summary>
        /// 没有定义的一个定义值
        /// </summary>
        private const string NotInitId = "NotInit";

        /// <summary>
        /// 店铺地址
        /// </summary>
        private string ShopUrl { get; set; }    

        /// <summary>
        /// 类别 Normal Special 处理一些特殊的情况
        /// </summary>
        private string Category { get; set; } 
       

        /// <summary>
        /// 仅测试一家店铺
        /// </summary>
        internal static void TestSimplyRun()
        {
            //var parameter = new NormalParameter { Keyword = @"192477" };
            var parameter = new NormalParameter { Keyword = @"1000072775" };
            //TestHelp<JDShopProductsCollecor>(parameter,3);
            TestHelp<JDShopProductsCollecor>(parameter);
        }

        /// <summary>
        ///     Tests this instance.
        /// </summary>
        public static void Test()
        {
            var shopIds = File.ReadAllLines(@"C:\Users\Administrator\Desktop\test3.txt");

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
                    TestHelp<JDShopProductsCollecor>(parameter, 10);
                }
                catch (NotSupportedException exception)
                {
                    Console.WriteLine($"error: {exception.Message}");
                }
            }
        }


        /// <summary>
        /// 移动到下一个链接
        /// </summary>
        /// <returns></returns>
        public override bool MoveNext()
        {
            var result = base.MoveNext();
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="nextUrl"></param>
        /// <param name="postData"></param>
        /// <param name="cookies"></param>
        /// <param name="currentUrl"></param>
        /// <returns></returns>
        protected override string GetMainWebContent
        (string nextUrl,
         byte[] postData,
         ref string cookies,
         string currentUrl)
        {
            if (this.Category.Equals("Special"))
            {
                return this.GetWebContent(nextUrl);
            }


            var webContent = this.GetWebContent(nextUrl, postData, ref cookies, currentUrl, true);

            return this.ParseModuleText(webContent);
        }

        /// <summary>
        /// 为第一页做初始化准备
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        protected override string InitFirstUrl(NormalParameter param)
        {
            var shopId = param.Keyword;

            if (StringExtension.IsNullOrWhiteSpace(shopId))
            {
                throw new ArgumentException(@"没能指定店铺编号");
            }

            this.ShopUrl = $"http://mall.jd.com/index-{shopId}.html";

            return NotInitId;
             
        }

        /// <summary>
        /// 移动到下一页
        /// </summary>
        /// <returns></returns>
        protected override string MoveToNextPage()
        {
            return this.NextUrl == NotInitId ? this.MoveToFirstPage() : base.MoveToNextPage();
        }


        /// <summary>
        /// 解析下一个链接
        /// </summary>
        /// <returns></returns>
        protected override string ParseNextUrl()
        {
            if (this.IsEmptyPage())
            {
                return null;
            }

            var currentUrl = this.CurrentUrl;

            if (StringExtension.IsNullOrWhiteSpace(currentUrl))
            {
                return null;
            }

            var nextpageUrl = HtmlDocumentHelper.GetValueByXPath(this.HtmlSource,@"//a[text()='下一页']/@href");
            if (StringExtension.IsNullOrWhiteSpace(nextpageUrl))
            {
                return null;
            }


            if (this.Category.Equals("Special"))
            {
                return $"http:{nextpageUrl}";
            }


            string baseUrl;
            NameValueCollection collection;
            Url.ParseUrl(currentUrl, out baseUrl, out collection);

            var pageNo = int.Parse(collection[@"pageNo"]);
            collection[@"pageNo"] = $"{pageNo + 1}";

            var nextUrl = Url.CombinUrl(baseUrl,collection);

            return nextUrl;

        }


        /// <summary>
        /// 搜索布局参数，是基本的一个，但不知道是哪一个，所以全部返回
        /// </summary>
        /// <param name="webContent"></param>
        /// <returns></returns>
        private IDictionary<string, string>[] LoadRenderStructures(string webContent)
        {
            var results = new List<IDictionary<string, string>>();

            var matchResultses = Regex.Matches(webContent,"<[^>]+\"m_render_structure loading\"[^>]+>");
            foreach (Match matchResultse in matchResultses)
            {
                IDictionary<string, string> result = new Dictionary<string,string>();
                var matchResults = Regex.Match(matchResultse.Value,@"(?<key>\w+)=""(?<val>[^""]+)""");
                while (matchResults.Success)
                {
                    result[matchResults.Groups["key"].Value] = matchResults.Groups["val"].Value;

                    matchResults = matchResults.NextMatch();
                }

                results.Add(result);
            }

            return results.ToArray();
             
        }


        private string BuildAjaxSearchUrl(string webContent, IDictionary<string, string> renderStructure)
        {
            var matchResults = Regex.Match(webContent,"(?<=var params = ){[^}]+}");
            if (!matchResults.Success)
            {
                throw new NotSupportedException("无法从页面中解析出搜索参数");
            }

            var jObject = JObject.Parse(matchResults.Value);
            var navigator = HtmlDocumentHelper.CreateNavigator(webContent);



            //System.Func<string, string> readJsonFunc = key => JsonHelper.TryReadJobjectValue(jObject, key,(string)null);

            //System.Func<string, string> readHtmlFunc = key =>
            //{
            //    string value;
            //    renderStructure.TryGetValue(key,out value);
            //    return value;
            //};

            //System.Func<string, string> readInputFunc = key =>
            //HtmlDocumentHelper.GetNodeValue(navigator,$@"//input[@id='{key}']/@value");

            var collection = Url.CreateQueryCollection();



            collection[@"appId"] = ReadJsonFunc(jObject,"appId");
            collection[@"orderBy"] = "5";
            collection[@"pageNo"] = "1";
            collection[@"direction"] = "1";
            collection[@"categoryId"] = ReadJsonFunc(jObject,@"categoryId");
            collection[@"pageSize"] = @"24";
            collection[@"pagePrototypeId"] = ReadJsonFunc(jObject,@"pagePrototypeId");
            collection[@"pageInstanceId"] = ReadHtmlFunc(renderStructure,@"m_render_pageInstance_id");
            collection[@"moduleInstanceId"] = ReadHtmlFunc(renderStructure,"m_render_instance_id");
            collection[@"prototypeId"] = ReadHtmlFunc(renderStructure,@"m_render_prototype_id");
            collection[@"templateId"] = ReadHtmlFunc(renderStructure,@"m_render_template_id");
            collection[@"layoutInstanceId"] = ReadHtmlFunc(renderStructure,@"m_render_layout_instance_id");
            collection[@"origin"] = ReadHtmlFunc(renderStructure,@"m_render_origin");
            collection[@"shopId"] = ReadInputFunc(navigator,@"shop_id");
            collection[@"verderId"] = ReadInputFunc(navigator,@"vender_id");

            collection[@"_"] = $"{JsCodeHelper.GetDateTime()}";

            var baseUrl = renderStructure[@"m_render_is_search"] == "true"
                            ? @"http://module-jshop.jd.com/module/getModuleHtml.html"
                            : @"http://mall.jd.com/view/getModuleHtml.html";

            return Url.CombinUrl(baseUrl,collection);
        }


        /// <summary>
        /// ReadJsonFunc
        /// </summary>
        /// <param name="jObject"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public string ReadJsonFunc(JObject jObject,string key)
        {
            return JsonHelper.TryReadJobjectValue(jObject, key, (string) null);
        }

        /// <summary>
        /// ReadHtmlFunc
        /// </summary>
        /// <param name="renderStructure"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public string ReadHtmlFunc(IDictionary<string, string> renderStructure,string key)
        {
         

            string value;
            renderStructure.TryGetValue(key, out value);
            return value;
        }

        /// <summary>
        /// ReadInputFunc
        /// </summary>
        /// <param name="navigator"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public string ReadInputFunc(XPathNavigator navigator,string key)
        {
            return HtmlDocumentHelper.GetNodeValue(navigator, $@"//input[@id='{key}']/@value");
        }

        /// <summary>
        /// 移动到第一页
        /// </summary>
        /// <returns></returns>
        private string MoveToFirstPage()
        {
            var webContent = this.GetSearchPageContent(this.ShopUrl);

            var renderStructures = this.LoadRenderStructures(webContent);
            var contents = new List<KeyValuePair<string, string>>();

            foreach (var renderStructure in renderStructures)
            {
                var ajaxSearchUrl = this.BuildAjaxSearchUrl(webContent,renderStructure);
                var searchContent = this.GetWebContent(ajaxSearchUrl);
                var content = this.ParseModuleText(searchContent);

                contents.Add(new KeyValuePair<string, string>(ajaxSearchUrl,searchContent));

                if (!this.isSearchWebContent(content))
                {
                    continue;
                }

                this.HtmlSource = content;
                this.CurrentUrl = ajaxSearchUrl;
                this.Category = "Normal";
           

                return content;

            }


            foreach (var contentItem in contents)
            {
                var content = this.ParseModuleText(contentItem.Value);
                if (!this.GuessIsSearchWebContent(content))
                {
                    continue;
                }

                this.HtmlSource = content;
                this.CurrentUrl = contentItem.Key;
                this.Category = "Normal";

                return content;
            }

            //在这里加
            webContent = GetSpecialSearchPageContent(this.ShopUrl);
            this.HtmlSource = webContent;
            this.Category = "Special";

            return webContent;

            throw new NotSupportedException("没有解析出有效的搜索页面");
        }

        /// <summary>
        /// 返回搜索页面的内容
        /// </summary>
        /// <param name="shopUrl"></param>
        /// <returns></returns>
        private string GetSearchPageContent(string shopUrl)
        {
           
            var webContent = this.GetWebContent(shopUrl);
            // 取出<input type="hidden" value="504028" id="pageInstance_appId"/>中的value
            var pageAppId = HtmlDocumentHelper.GetNodeValue(webContent, @"//input[@id='pageInstance_appId']/@value");


            /*
             // view_search-店铺页面编号-0-排序类型-排序方向-每页条数-页码.html
                排序类型： 5 销量 4 价格 3 收藏  2 时间 
                每页条数： 最大 24 
                排序方向： 1 从大到时小  0 从小到大
                页码： 从 1 开始 
                查找pageInstance_appId找到value的值
                http://mall.jd.com//view_search-337310-0-5-0-24-5.html
             */


            var searchUrl = $"http://mall.jd.com/view_search-{pageAppId}-0-5-0-24-1.html";



            return this.GetWebContent(searchUrl);

        }

        /// <summary>
        /// 处理特例
        /// </summary>
        /// <param name="shopUrl"></param>
        /// <returns></returns>
        private string GetSpecialSearchPageContent(string shopUrl)
        {
            var webContent = this.GetWebContent(shopUrl);

            var pageAppId = HtmlDocumentHelper.GetNodeValue(webContent, @"//input[@id='pageInstance_appId']/@value");
            var vender_id = HtmlDocumentHelper.GetNodeValue(webContent, @"//input[@id='vender_id']/@value");
            var shop_id = HtmlDocumentHelper.GetNodeValue(webContent, @"//input[@id='shop_id']/@value");

            var searchUrl = $"http://mall.jd.com/advance_search-{pageAppId}-{vender_id}-{shop_id}-5-0-0-1-1-24.html";
            this.CurrentUrl = searchUrl;

            return this.GetWebContent(searchUrl);
        }


        /// <summary>
        /// 推荐是不是商品页面
        /// </summary>
        /// <param name="webContent"></param>
        /// <returns></returns>
        private bool GuessIsSearchWebContent(string webContent)
        {
            if (string.IsNullOrEmpty(webContent))
            {
                return false;
            }

            var navigator = HtmlDocumentHelper.CreateNavigator(webContent);
            var iterator = navigator.Select(@"//a");

            var itemCount = 0;

            foreach (XPathNavigator item in iterator)
            {
                var href = item.GetAttribute(@"href", string.Empty);

                if (href == null)
                {
                    continue;
                }

                if (Regex.IsMatch(href,@"^//item\.jd\.com/\d+\.html",RegexOptions.IgnoreCase))
                {
                    itemCount++;
                }

                if (itemCount >= 8)
                {
                    return true;
                }

            }

            return false;
        }

        /// <summary>
        /// 解析是否为空页
        /// </summary>
        /// <returns></returns>
        private bool IsEmptyPage()
        {
            return StringExtension.IsNullOrWhiteSpace(this.HtmlSource);
        }

        /// <summary>
        /// 是否是搜索页面
        /// </summary>
        /// <param name="searchContent"></param>
        /// <returns></returns>
        private bool isSearchWebContent(string searchContent)
        {
            var page = this.ParseAmountPage(searchContent);
            return page != -1;
        }


        /// <summary>
        /// 解析出总共的页数
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        private int ParseAmountPage(string content)
        {
            if (content == null)
            {
                return -1;
            }

            var navigator = HtmlDocumentHelper.CreateNavigator(content);
            var node = navigator.SelectSingleNode(@"//div[@class='jPage']/em");


            if (node == null)
            {
                return -1;
            }

            var matchResults = Regex.Match(node.Value, @"(?<=共)\d+(?=条记录)");
            return matchResults.Success ? int.Parse(matchResults.Value) : -1;

        }

        /// <summary>
        /// 解析当前页数
        /// </summary>
        /// <returns></returns>
        protected override int ParseCurrentPage()
        {
            var currentUrl = this.CurrentUrl;
            if (string.IsNullOrEmpty(currentUrl))
            {
                return -1;
            }

            if (this.Category.Equals("Special"))
            {
                int result;
                if (int.TryParse(Regex.Match(currentUrl, @"(?<=1-)\d+(?=-24.html)").Value, out result))
                    return result;
                else
                    return -1;
            }

            var collection = Url.ParseQueryString(currentUrl);

            int page;
            return int.TryParse(collection[@"pageNo"],out page) ? page : -1;

        }

        /// <summary>
        ///     解析出总页码
        /// </summary>
        /// <returns></returns>
        protected override int ParseCountPage()
        {
            return this.ParseAmountPage(this.HtmlSource);
        }

        /// <summary>
        /// 解析当前页的所有产品信息
        /// </summary>
        /// <returns></returns>
        protected override IResut[] ParseCurrentItems()
        {
            return this.ParseCurrentItems(this.HtmlSource);
        }

        /// <summary>
        /// 解析当前页的所有产品信息
        /// </summary>
        /// <param name="htmlSource"></param>
        /// <param name="listOnly"></param>
        /// <returns></returns>
        private IResut[] ParseCurrentItems(string htmlSource, bool listOnly = false)
        {
            const string SkuIdKey = "ProductSku";
            var resultList = new List<IResut>();

            // 返回xpath查询器
            var navigator = HtmlDocumentHelper.CreateNavigator(htmlSource);
            var iterator = navigator.Select(@"//ul/li");

            foreach (XPathNavigator item in iterator)
            {
                var title = HtmlDocumentHelper.GetNodeValue(item, ".//div[@class='jDesc']/a/text()");
                var href = HtmlDocumentHelper.GetNodeValue(item,".//div[@class='jDesc']//@href");
                if (string.IsNullOrEmpty(title))
                {
                    title = HtmlDocumentHelper.GetNodeValue(item, ".//div[@class='jTitle']/a/text()");
                    href = HtmlDocumentHelper.GetNodeValue(item, ".//div[@class='jTitle']//@href");
                }





                //HtmlDocumentHelper.GetNodeValue(item,".//div[@class='jPic']//@original")

                var imgSrc = HtmlDocumentHelper.GetNodeValue(item, ".//div[@class='jPic']//@original");
                if(imgSrc.Equals(string.Empty))
                    imgSrc = HtmlDocumentHelper.GetNodeValue(htmlSource, ".//div[@class='jPic']//@src");

                var skuMatchResults = Regex.Match(href,@"(?<=/)\d+(?=.html)");
                var sku = skuMatchResults.Success ? skuMatchResults.Value : string.Empty;

                if (string.IsNullOrEmpty(sku))
                {
                    continue;
                }

                // 评价数据
                var comments = ParseComments(item);

                IResut resut = new Resut();

                resut[SkuIdKey] = sku;
                resut["ShopUrl"] = this.ShopUrl;
                resut["ProductName"] = title;
                resut["ProductUrl"] = href;
                resut["ProductImage"] = imgSrc;
                resut["ProductComments"] = comments;
                resultList.Add(resut);
            }

            if (!listOnly)
            {
                this.UpdateResultsPrices(resultList,SkuIdKey);
            }

            return resultList.ToArray();
        }

        /// <summary>
        ///解析评论人数以及评价
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private static string ParseComments(XPathNavigator item)
        {
            var matchResults = Regex.Match(item.Value, @"\d+(?= *人评价)");
            if (matchResults.Success)
            {
                return matchResults.Value;
            }

            // 这里还没有找到例子
            var commentsNode = HtmlDocumentHelper.GetNodeValue(item, ".//span[@class='evaluate']");
            matchResults = Regex.Match(commentsNode, @"(?<=\()\d+(?=\))");
            if (matchResults.Success)
            {
                return matchResults.Value;
            }

            return "-1";
        }



        /// <summary>
        /// 解析json中的内容
        /// </summary>
        /// <param name="webContent"></param>
        /// <returns></returns>
        private string ParseModuleText(string webContent)
        {
            var matchResults = Regex.Match(webContent,@"(?<=^\w+\(){.*}(?=\))");
            webContent = matchResults.Success ? matchResults.Value : webContent;

            var jObject = JObject.Parse(webContent);
            var jToken = jObject[@"moduleText"];
            if (jToken == null)
            {
                return "本店装修中";
                //throw new NotFiniteNumberException($"没有从结果中解析出商品内容:{this.Current}");
            }

            return jToken.Value<string>();


        }

        /// <summary>
        /// 更新价格
        /// </summary>
        /// <param name="resultList"></param>
        /// <param name="skuIdKey"></param>
        private void UpdateResultsPrices(List<IResut> resultList,string skuIdKey)
        {
            if (resultList.Count == 0)
            {
                return;
            }

            IDictionary<string, IResut> resultDictionary = new Dictionary<string, IResut>();
            resultList.ForEach(result => resultDictionary[$"J_{result[skuIdKey]}"] = result);

            var skuids = new string[resultDictionary.Count];
            resultDictionary.Keys.CopyTo(skuids,0);

            var collection = Url.CreateQueryCollection();
            collection[@"skuids"] = string.Join(",",skuids);
            collection[@"_"] = $"{JsCodeHelper.GetDateTime()}";

            const string BaseUrl = @"http://p.3.cn/prices/mgets";
            var url = Url.CombinUrl(BaseUrl,collection);

            var webContent = this.GetWebContent(url);

            var jArray = JArray.Parse(webContent);
            foreach (var jToken in jArray)
            {
                var skuid = JsonHelper.ReadJobjectValue<string>(jToken, @"id");
                var price = JsonHelper.ReadJobjectValue<string>(jToken, @"p");
                var mprice = JsonHelper.ReadJobjectValue<string>(jToken, @"m");

                IResut result;
                if (!resultDictionary.TryGetValue(skuid, out result)) continue;

                result[@"ProductPrice"] = price;
                result[@"ProductMPrice"] = mprice;

            }
        }


    }
}
