namespace X.GlodEyes.Collectors.Specialized.JingDong
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Web;

    using Newtonsoft.Json.Linq;

    using X.CommLib.IO;
    using X.CommLib.Net.Miscellaneous;
    using X.CommLib.Net.WebRequestHelper;
    using X.CommLib.Office;
    using X.GlodEyes.Utilities;

    #region 建表sql

    /*
     * 任务数据表
     * CREATE TABLE `x_renwu_dianpu_jingdong` (
                `Keyword` VARCHAR(255) NULL DEFAULT NULL,
                `TaskDataId` BIGINT(20) NOT NULL AUTO_INCREMENT,
                `StationId` VARCHAR(128) NOT NULL DEFAULT '',
                `TaskStatue` INT(1) NOT NULL DEFAULT '0',
                `DispatchTime` DATETIME NULL DEFAULT NULL,
                `TaskPriority` INT(9) NULL DEFAULT '0',
                PRIMARY KEY(`TaskDataId`),
                INDEX `StationId` (`StationId`),
                INDEX `TaskStatue` (`TaskStatue`),
                INDEX `TaskPriority` (`TaskPriority`)
            )
            COLLATE='utf8_general_ci'
            ENGINE=MyISAM;         

        * 结果数据表
        * CREATE TABLE `x_yuanshi_dianpu_jingdong` (
                `ShopUrl` VARCHAR(256) NULL DEFAULT '',
                `SearchKeyword` VARCHAR(256) NULL DEFAULT '',
                `SearchPageRank` VARCHAR(10) NULL DEFAULT '',
                `SearchPageIndex` VARCHAR(10) NULL DEFAULT '',
                `ShopId` VARCHAR(64) NULL DEFAULT '',
                `ShopName` VARCHAR(128) NULL DEFAULT '',
                `ShopBrief` VARCHAR(1024) NULL DEFAULT '',
                `ShopLogoUrl` VARCHAR(256) NULL DEFAULT '',
                `ShopType` VARCHAR(64) NULL DEFAULT '',
                `VenderId` VARCHAR(64) NULL DEFAULT '',
                `VenderType` VARCHAR(64) NULL DEFAULT '',
                `BusinessCategory` VARCHAR(1024) NULL DEFAULT '',
                `MainBrand` VARCHAR(1024) NULL DEFAULT '',
                `VenderTotalScore` VARCHAR(64) NULL DEFAULT '',
                `VenderWareScore` VARCHAR(64) NULL DEFAULT '',
                `VenderServiceScore` VARCHAR(64) NULL DEFAULT '',
                `venderEffectiveScore` VARCHAR(64) NULL DEFAULT '',
                `ShopScore` VARCHAR(64) NULL DEFAULT '',
                `IndustryServiceScore` VARCHAR(64) NULL DEFAULT '',
                `IndustryEffectiveScore` VARCHAR(64) NULL DEFAULT '',
                `IndustryTotalScore` VARCHAR(64) NULL DEFAULT '',                
    
                `InfoUniqueId` BIGINT(20) NOT NULL AUTO_INCREMENT,
                `InfoGatherDate` DATETIME NULL DEFAULT NULL,
                `InfoGatherSourceId` VARCHAR(64) NULL DEFAULT NULL,
                `InfoContentId` VARCHAR(255) NULL DEFAULT NULL,
                `InfoContentUrl` VARCHAR(255) NULL DEFAULT NULL,
                `InfoContentTitle` VARCHAR(255) NULL DEFAULT NULL,
                `InfoAuthor` VARCHAR(255) NULL DEFAULT NULL,
                `InfoSite` VARCHAR(255) NULL DEFAULT NULL,
                `InfoPublishDate` DATETIME NULL DEFAULT NULL,
                `InfoOriginalLabels` VARCHAR(255) NULL DEFAULT NULL,
                `TaskId` VARCHAR(64) NULL DEFAULT NULL,
                `EngineId` VARCHAR(64) NULL DEFAULT NULL,
                `EngineVersion` VARCHAR(36) NULL DEFAULT NULL,
                `DispatchId` VARCHAR(64) NULL DEFAULT NULL,
                `IsTest` TINYINT(1) NULL DEFAULT '0',
                `IsDeleted` TINYINT(1) NULL DEFAULT '0',
                PRIMARY KEY (`InfoUniqueId`)
            )
            COLLATE='utf8_general_ci'
            ENGINE=MyISAM;
        *          
         */
    #endregion

    /// <summary>
    ///     京东列表采集
    ///     一页 5条
    /// </summary>
    public class JdShopListCollector : WebRequestCollector<IResut, NormalParameter>
    {
        /*/// <summary>
        ///     用户数据缓存
        /// </summary>
        private static readonly IDictionary<string, IResut> companyInfoDictionary = new Dictionary<string, IResut>();*/

        /// <summary>
        ///     进行测试
        /// </summary>
        internal void Test()
        {
            IParameter parameter = new Parameter();
            parameter[@"keyword"] = @"平板电视";

            TestHelp<JdShopListCollector>(parameter);
        }

        /// <summary>
        ///  是不是还有更多的内容
        /// </summary>
        private bool hasMore = true;

        /// <summary>
        /// 最的一次获得的店铺json 内容的hash值
        /// </summary>
        private byte[] lastShopJsonHashCode;

        /// <summary>
        ///     检测是否还有更多的内容
        /// </summary>
        /// <returns></returns>
        protected override bool DetectHasMore()
        {
            if (!this.hasMore)
            {
                return false;
            }

            var skus = this.ParseProductDataSkus(this.HtmlSource);
            return skus.Length > 0 && base.DetectHasMore();
        }

        /// <summary>
        ///     为第一页作初始化准备
        /// </summary>
        /// <param name="param">The parameter.</param>
        protected override string InitFirstUrl(NormalParameter param)
        {
            var keyword = param.Keyword;
            if (string.IsNullOrEmpty(keyword))
            {
                throw new ArgumentException(@"没有指定搜索关键词", nameof(param));
            }

            /*return $@"http://search.jd.com/Search?keyword={HttpUtility.UrlEncode(keyword)}&enc=utf-8&vt=3";*/
            // http://search.jd.com/s_new.php?keyword=%E5%B7%A5%E5%85%B7&enc=utf-8&vt=3&page=10

            return $"http://search.jd.com/s_new.php?keyword={HttpUtility.UrlEncode(keyword)}&enc=utf-8&qrst=1&rt=1&stop=1&vt=3&stg=1&sttr=1";
        }

        /// <summary>
        ///     解析出总页码
        /// </summary>
        /// <returns></returns>
        protected override int ParseCountPage()
        {
            var pageToken = this.ParsePageToken();
            var page = JsonHelper.TryReadJobjectValue(pageToken, @"page_count", -1);

            return page * 2; // 因为一页代表后台  json 2个请求，每个 josn 返回5个条目 
        }

        /// <summary>
        ///     解析出当前页码
        /// </summary>
        /// <returns></returns>
        protected override int ParseCurrentPage()
        {
            /*var pageToken = this.ParsePageToken();
            return JsonHelper.TryReadJobjectValue(pageToken, @"page", -1);*/

            var matchResults = Regex.Match(this.HtmlSource, @"LogParm.page=""(?<page>\d+)""", RegexOptions.Multiline);
            var pageGroup = matchResults.Groups[@"page"];
            return pageGroup.Success ? int.Parse(pageGroup.Value) : -1;
        }

        

        /// <summary>
        ///     解析出下一页的地址，默认当没有下一页的时候，枚举停止 
        /// </summary>
        /// <returns></returns>
        protected override string ParseNextUrl()
        {
            string urlPath;
            NameValueCollection collection;

            Url.ParseUrl(this.CurrentUrl, out urlPath, out collection);
            var pageStr = collection[@"page"];
            var page = string.IsNullOrEmpty(pageStr) ? 1 : int.Parse(pageStr);

            /*var nextpage = page + 1;
            collection[@"page"] = $"{nextpage}";*/

            // 尝试从页面中解析出更多的翻页信息，如果解析不出来的话，则从上一页的链接中累加页数
            var matchResults = Regex.Match(
                this.HtmlSource,
                "SEARCH.base_url=\"(?<baseurl>[^\"]+)\"",
                RegexOptions.Multiline);
            var baseUrlGroup = matchResults.Groups["baseurl"];
            var baseUrl = baseUrlGroup.Success ? baseUrlGroup.Value : null;

            var currentPage = this.ParseCurrentPage();
            var pageCount = this.ParseCountPage();

            var nextPage = currentPage > 0 ? currentPage + 1 : page + 1;
            if ((pageCount > 0 && nextPage > pageCount) || nextPage > 200)
            {
                // 如果翻了200页或是下一页码大于总页码
                return null;
            }

            if (!StringExtension.IsNullOrWhiteSpace(baseUrl))
            {
                return $"{urlPath}?{baseUrl}&page={nextPage}&s={page * 5 + 1}&scrolling=y";
            }

            collection[@"scrolling"] = @"y";
            collection[@"page"] = $"{nextPage}";
            collection[@"s"] = $"{page * 5 + 1}";


            return Url.CombinUrl(urlPath, collection);
        }

        /// <summary>
        ///     解析出当前值
        /// </summary>
        /// <returns></returns>
        protected override IResut[] ParseCurrentItems()
        {
            var shopDictionary = this.GetListShopItemInfo();
            if (shopDictionary.Count == 0)
            {
                return new IResut[0];
            }

            /*var shipIds = this.GetSelfSupportShopIds(shopDictionary);
            var companyDictionary = this.GetCompanyInfos(shipIds);

            this.CombinShopInfo(shopDictionary, companyDictionary);*/
            var resuts = new IResut[shopDictionary.Count];
            shopDictionary.Values.CopyTo(resuts, 0);

            this.UpdateResultsKeyNames(resuts);

            return resuts;
        }

        /// <summary>
        ///     将一个 jobject 值转为 result 值
        /// </summary>
        /// <param name="jObject">The j object.</param>
        /// <returns></returns>
        private IResut ConvertToResult(JObject jObject)
        {
            IResut resut = new Resut();

            var properties = jObject.Properties();
            foreach (var property in properties)
            {
                resut[property.Name] = property.Value?.Value<string>() ?? string.Empty;

                // jObject[property].Value<string>();
            }

            return resut;
        }

        /*  /// <summary>
        ///     获取用户数据
        /// </summary>
        /// <param name="shopId">The shop identifier.</param>
        /// <returns></returns>
        private IResut GetCompanyInfo(string shopId)
        {
            IResut resut;
            companyInfoDictionary.TryGetValue(shopId, out resut);
            if (resut != null)
            {
                return resut;
            }

            resut = this.GetCompanyInfoOnline(shopId);
            companyInfoDictionary[shopId] = resut;

            return resut;
        }*/

        /*        /// <summary>
        ///     在线获取用户公司信息
        /// </summary>
        /// <param name="shopId">The shop identifier.</param>
        /// <returns></returns>
        private IResut GetCompanyInfoOnline(string shopId)
        {
            var url = $"http://mall.jd.com/shopLevel-{shopId}.html";
            var cookies = this.Cookies ?? string.Empty;
            var webContent = WebRequestCtrl.GetWebContent(url, null, ref cookies, 2);

            var parser = new CompanyInfoParser();
            return parser.Parse(webContent);
        }*/

        /*    /// <summary>
        ///     获取公司信息
        /// </summary>
        /// <param name="shopIds">The shop ids.</param>
        /// <returns></returns>
        private IDictionary<string, IResut> GetCompanyInfos(string[] shopIds)
        {
            IDictionary<string, IResut> resultDictionary = new Dictionary<string, IResut>();
            foreach (var shopId in shopIds)
            {
                var resut = this.GetCompanyInfo(shopId);
                resultDictionary[shopId] = resut;
            }

            return resultDictionary;
        }*/

        /// <summary>
        ///     获取店铺信息
        /// </summary>
        /// <returns></returns>
        private IDictionary<string, IResut> GetListShopItemInfo()
        {
            var htmlSource = this.HtmlSource;
            IDictionary<string, IResut> resultDictionary = new Dictionary<string, IResut>();

            var shopIds = this.ParseShopIds(htmlSource);
            if (shopIds.Length == 0)
            {
                return resultDictionary;
            }

            // http://search.jd.com/shop_new.php?ids=1000004042,1000000922,1000015485,1000015427,1000010664
            var collection = Url.CreateQueryCollection();
            collection.Add(@"ids", string.Join(",", shopIds));
            var url = Url.CombinUrl(@"http://search.jd.com/shop_new.php", collection);
            var cookies = this.Cookies ?? string.Empty;

            var param = WebRequestCtrl.GetWebContentParam.Default;
            param.Refere = this.CurrentUrl;
            var webContent = WebRequestCtrl.GetWebContent(url, null, ref cookies, 2, param);

            this.TryUpdateHasMoreFlag(webContent);

            var keyword = this.InnerParam.Keyword;
            var page = this.CurrentPage;
            var rank = 0;

            var jArray = JArray.Parse(webContent);

            foreach (var jToken in jArray)
            {
                rank++;

                var shopId = jToken[@"shop_id"]?.Value<string>();
                if (shopId == null)
                {
                    continue;
                }

                var jObject = (JObject)jToken;
                var result = this.ConvertToResult(jObject);
                result[@"ShopUrl"] = $"http://mall.jd.com/index-{shopId}.html";
                result[@"SearchKeyword"] = keyword;
                this.SetResultSearchPageRank(result, rank);
                this.SetResultSearchPageIndex(result, page);
                

                resultDictionary[shopId] = result;
            }

            return resultDictionary;
        }

        /// <summary>
        /// 根据返回内容与上一页内容是否重复来检测是不是还有更多的页码
        /// 可能不太重要
        /// </summary>
        /// <param name="webContent">Content of the web.</param>
        private void TryUpdateHasMoreFlag(string webContent)
        {
            if (StringExtension.IsNullOrWhiteSpace(webContent))
            {
                return;
            }

            var bytes = Encoding.UTF8.GetBytes(webContent);
            var hashcode = HashCodeCreater.GetHashcode(bytes);

            if (this.lastShopJsonHashCode != null)
            {
                var sameCode = HashCodeCreater.IsSameHashCode(hashcode, this.lastShopJsonHashCode);
                if (sameCode)
                {
                    this.SendLog("发现重复采集内容");
                }

                this.hasMore = !sameCode;
            }

            this.lastShopJsonHashCode = hashcode;
        }

        /*        /// <summary>
        ///     返回自营店的店铺编号列表
        /// </summary>
        /// <param name="shopDictionary">The shop dictionary.</param>
        /// <returns></returns>
        private string[] GetSelfSupportShopIds(IDictionary<string, IResut> shopDictionary)
        {
            var shopIdList = new List<string>();

            foreach (var shopItem in shopDictionary)
            {
                var shopId = shopItem.Key;
                if (shopItem.Value.GetStringValue(@"vender_type") == "0")
                {
                    shopIdList.Add(shopId);
                }
            }

            return shopIdList.ToArray();
        }*/

        /// <summary>
        ///     Parses the identifier from xpath.
        /// </summary>
        /// <param name="htmlSource">The HTML source.</param>
        /// <param name="xpath">The xpath.</param>
        /// <returns></returns>
        private string[] ParseIdFromXpath(string htmlSource, string xpath)
        {
            var navigator = HtmlDocumentHelper.CreateNavigator(htmlSource);
            var iterator = navigator.Select(xpath);
            var xItems = HtmlDocumentHelper.CopyNodeToArray(iterator);

            var pids = Array.ConvertAll(xItems, item => item.Value);

            return DictionaryHelper.Distinct(pids);
        }

        /// <summary>
        ///     解析出页码相关的json
        /// </summary>
        /// <returns></returns>
        private JToken ParsePageToken()
        {
            var matchResults = Regex.Match(this.HtmlSource, @"SEARCH\.adv_param=(?<code>\{[^}]+})");
            var codeGroup = matchResults.Groups["code"];

            return codeGroup.Success ? JObject.Parse(codeGroup.Value) : null;
        }

        /// <summary>
        ///     解析出商品的 sku 值
        /// </summary>
        /// <returns></returns>
        private string[] ParseProductDataSkus(string htmlSource)
        {
            return this.ParseIdFromXpath(htmlSource, @"//li[@data-sku]/@data-sku");
        }

        /// <summary>
        ///     从页面中解析出店铺的编号
        /// </summary>
        /// <param name="htmlSource">The HTML source.</param>
        /// <returns></returns>
        private string[] ParseShopIds(string htmlSource)
        {
            return this.ParseIdFromXpath(htmlSource, @"//*[@data-shopid]/@data-shopid");
        }

        /// <summary>
        ///     更新采集结果的 keyname 值
        /// </summary>
        /// <param name="resuts">The resuts.</param>
        private void UpdateResultsKeyNames(IEnumerable<IResut> resuts)
        {
            var keyNames = new Dictionary<string, string>
                               {
                                   { @"shop_id", @"ShopId" },
                                   { @"shop_name", @"ShopName" },
                                   { @"ShopUrl", @"ShopUrl" },
                                   { @"shop_brief", @"ShopBrief" },
                                   { @"shop_logo", @"ShopLogoUrl" },
                                   { @"shop_type", @"ShopType" },
                                   { @"vender_id", @"VenderId" },
                                   { @"vender_type", @"VenderType" },
                                   { @"business_category", @"BusinessCategory" },
                                   { @"main_brand", @"MainBrand" },
                                   { @"shop_score", @"ShopScore" },
                                   { @"vender_total_score", @"VenderTotalScore" },
                                   { @"vender_ware_score", @"VenderWareScore" },
                                   { @"vender_service_score", @"VenderServiceScore" },
                                   { @"vender_effective_score", @"venderEffectiveScore" },
                                   { @"industry_service_score", @"IndustryServiceScore" },
                                   { @"industry_effective_score", @"IndustryEffectiveScore" },
                                   { @"industry_total_score", @"IndustryTotalScore" },
                                   { @"SearchKeyword", @"SearchKeyword" },
                                   { @"SearchPageRank", @"SearchPageRank" },
                                   { @"SearchPageIndex", @"SearchPageIndex" }
                               };

            this.UpdateResultsKeyNames(resuts, keyNames);
        }
    }
}