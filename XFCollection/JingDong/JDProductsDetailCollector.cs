using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using X.CommLib.Net.Miscellaneous;
using X.GlodEyes.Collectors;
using X.GlodEyes.Collectors.Specialized.JingDong;

namespace XFCollection.JingDong
{
    /// <summary>
    /// 京东产品详情收集
    /// </summary>
          
    public class JDProductsDetailCollector : WebRequestCollector<IResut, NormalParameter>
    {
        //334编号

        /// <summary>
        /// 测试
        /// </summary>
        internal static void Test()
        {
            var parameter = new NormalParameter {Keyword = @"10601135831" };
            TestHelp<JDProductsDetailCollector>(parameter, 10);
        }

        /// <summary>
        /// 测试1
        /// </summary>
        internal static void Test1()
        {
            var productsId = File.ReadAllLines(@"C:\Users\Administrator\Desktop\test2.txt");

            /*var shopIds = File.ReadAllLines(@"C:\Users\sinoX\Desktop\errorList.txt");*/
            // 去掉字符串前后的"
            productsId = Array.ConvertAll(productsId, productId => productId.Trim('"'));

            foreach (var productId in productsId)
            {
                Console.WriteLine();
                Console.WriteLine($"productId id: {productId}");
                try
                {
                    var parameter = new NormalParameter { Keyword = productId };
                    TestHelp<JDProductsDetailCollector>(parameter, 1);
                }
                catch (NotSupportedException exception)
                {
                    Console.WriteLine($"error: {exception.Message}");
                }
            }
        }

        /// <summary>
        /// 京东商品暂停时间
        /// </summary>
        public override double DefaultMovePageTimeSpan => 0;

        //产品编号
        private string _productId;
        //另一部分来源链接
        private string _url;
        //另一部分来源网页html
        private string _html;

        

        //http://item.m.jd.com/product/{ShopId}.html

        /// <summary>
        /// 定义第一个链接
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        protected override string InitFirstUrl(NormalParameter param)
        {
            this._productId = param.Keyword;
            this._url = $"http://item.m.jd.com/product/{this._productId}.html";
            this._html = base.GetWebContent(this._url);
            return $"http://item.jd.com/{this._productId}.html";
            
        }   

        /// <summary>
        /// 解析当前元素
        /// </summary>
        /// <returns></returns>
        protected override IResut[] ParseCurrentItems()
        {
            var resultList = new List<IResut>();
            var productName = this.GetProductName(_html);
            if (productName.Equals(string.Empty))
                productName = this.GetSpecialProductName(HtmlSource);
            var selectColor = this.GetSelectColor(HtmlSource);
            var imgSrc = this.GetImgSrc(HtmlSource);
            var warmReminder = this.GetWarmReminder(HtmlSource);
            var productPrice = this.GetProductPrice(_html);
            var whiteBar = this.GetWhiteBar(_html);
            var service = this.GetService(_html);
            var discount = this.GetDiscount(_html);
            var productActivity = this.GetProductActivity(_html);
            var isExist = this.ProductIsExist(_html);
            const string notExist = "该商品已下柜，非常抱歉！";

            if (productName.Equals(string.Empty))
                isExist = "产品不存在!";
            else if (isExist.Equals(notExist))
                isExist = notExist;
            else
                isExist = this.GetIsExist(_html);

            var commentDic = this.GetCommentDic();

            var resut = new Resut
            {
                //产品id
                ["ProductId"] = this._productId,
                //产品名字
                ["ProductName"] = productName,
                //选择颜色
                ["SelectColor"] = selectColor,
                //图片链接
                ["ImgSrc"] = imgSrc,
                //温馨提醒
                ["WarmReminder"] = warmReminder,
                //产品价格
                ["ProductPrice"] = productPrice,
                //白条
                ["WhiteBar"] = whiteBar,
                //服务
                ["Service"] = service,
                //促销
                ["Discount"] = discount,
                //产品活动
                ["ProductActivity"] = productActivity,
                //产品是否有货以及预计送达时间
                ["IsExist"] = isExist, 
                //全部评价
                ["AllCnt"] = commentDic["allCnt"],
                //好评
                ["GoodCnt"] = commentDic["goodCnt"],
                //中评
                ["NormalCnt"] = commentDic["normalCnt"],
                //差评
                ["BadCnt"] = commentDic["badCnt"],
                //有图评价
                ["PictureCnt"] = commentDic["pictureCnt"]
            };

            resultList.Add(resut);
            return resultList.ToArray();
        }

        /// <summary>
        /// 解析下一页
        /// </summary>
        /// <returns></returns>
        protected override string ParseNextUrl()
        {
            return null;
        }

        /// <summary>
        /// 解析页面总数
        /// </summary>
        /// <returns></returns>
        protected override int ParseCountPage()
        {
            return 1;
        }

        /// <summary>
        /// 解析当前页
        /// </summary>
        /// <returns></returns>
        protected override int ParseCurrentPage()
        {
            return 1;
        }

        /// <summary>
        /// 查看产品是否存在
        /// </summary>
        /// <param name="htmlString"></param>
        /// <returns></returns>
        private string ProductIsExist(string htmlString)
        {
            return Regex.Match(htmlString, @"(?<=id=""stockStatus"">[\s]*)[\S]*(?=[\s]*</p>)").Value;
        }

        /// <summary>
        /// 得到产品名字
        /// </summary>
        /// <param name="htmlString"></param>
        /// <returns></returns>
        private string GetProductName(string htmlString)
        {
            return Regex.Match(htmlString, "(?<=<meta name=\"keywords\" CONTENT=\").*(?=\">)").Value;
        }

        private string GetSpecialProductName(string htmlString)
        {
            return Regex.Match(htmlString, @"(?<=<h1>[\s]*)[^\s].*[^\s](?=[\s]*</h1>)").Value;
        }

        /// <summary>
        /// 选择颜色
        /// </summary>
        /// <param name="htmlString"></param>
        /// <returns></returns>
        private string GetSelectColor(string htmlString)
        {
            var selectColor = string.Empty;
            var matchs = Regex.Matches(htmlString, @"(?<=<div class=""item"">[\s\S]*<i>).+(?=</i>)");
            var count = 1;
            foreach (Match match in matchs)
            {
                var value = match.Value;
                selectColor = count++ ==1 ? $"{value}" : $"{selectColor},{value}";
            }

            return selectColor;
        }

        /// <summary>
        /// 得到图片链接
        /// </summary>
        /// <param name="htmlString"></param>
        /// <returns></returns>
        private string GetImgSrc(string htmlString)
        {
            var imgSrc = string.Empty;
            var matchs = Regex.Matches(htmlString, @"(?<=<img[\s\S]*alt='[^>]*src=')[\S]*(?=' data-url=)");
            var count = 1;
            foreach (Match match in matchs)
            {
                var value = match.Value;
                imgSrc = count++ == 1 ? $"{value}" : $"{imgSrc},{value}";
            }

            return imgSrc;
        }

        /// <summary>
        /// 得到温馨提醒
        /// </summary>
        /// <param name="htmlString"></param>
        /// <returns></returns>
        private string GetWarmReminder(string htmlString)
        {
            var warmReminder = Regex.Match(htmlString, @"(?<=tips: ).*?(?=,[\s]+)").Value;
            if(warmReminder.Equals(string.Empty))
                return warmReminder;

            var matchs = Regex.Matches(warmReminder, "(?<=\"tip\":\").*?(?=\"})");
            var count = 1;
            foreach (Match match in matchs)
            {
                var value = match.Value;
                warmReminder = count++ == 1 ? $"{value}" : $"{warmReminder},{value}";
            }
            return warmReminder;
        }

        /// <summary>
        /// 得到产品价格
        /// </summary>
        /// <param name="htmlString"></param>
        /// <returns></returns>
        private string GetProductPrice(string htmlString)
        {

            var bigPrice = Regex.Match(htmlString,"(?<=<span class=\"big-price\">).*?(?=</span>)").Value;
            var smallPrice = Regex.Match(htmlString, "(?<=<span class=\"small-price\">).*?(?=</span>)").Value;
            var price = $"{bigPrice}{smallPrice}";
            return price;
        }

        /// <summary>
        /// 得到白条
        /// </summary>
        /// <param name="htmlString"></param>
        /// <returns></returns>
        private string GetWhiteBar(string htmlString)
        {
            return Regex.Match(htmlString, "(?<=div class=\"prod-whitebar-txt\">).*(?=</div>)").Value;
        }

        /// <summary>
        /// 得到服务
        /// </summary>
        /// <returns></returns>
        private string GetService(string htmlString)
        {
            return Regex.Match(htmlString, "(?<=<p class=\"provider base-txt\">).*(?=</p>)").Value;
        }

        /// <summary>
        /// 得到促销
        /// </summary>
        /// <returns></returns>
        private string GetDiscount(string htmlString)
        {
            var discount = string.Empty;
            var count = 1;
            var matchs = Regex.Matches(htmlString, "(?<=class=\"promotion-item-text\" >).*?(?=</span>)");

            foreach (Match match in matchs)
            {
                var value = match.Value;
                value = value.Replace(@"<i>", "");
                value = value.Replace(@"</i>", "");

                discount = count++ == 1 ? value : $"{discount} {value}";
            }

            return discount;
        }


        /// <summary>
        /// 得到产品活动
        /// </summary>
        /// <returns></returns>
        private string GetProductActivity(string htmlString)
        {
            var productActivity = Regex.Match(htmlString, "(?<=<div class=\"prod-act\">).*?(?=</div>)").Value;
            if(productActivity.Equals(string.Empty))
                return productActivity;

            var matchs = Regex.Matches(productActivity, "(?<=^|>).*?(?=<[/]{0,1}a|$)");

            var count = 1;
            foreach (Match match in matchs)
            {
                var value = match.Value;
                productActivity = count++ == 1 ? value : $"{productActivity}{value}";
            }
            return productActivity;
        }


        

        /// <summary>
        /// 产品是否有货以及预计送达时间
        /// </summary>
        private string GetIsExist(string htmlString)
        {
            return Regex.Match(htmlString, "(?<=<span class=\"isExist\">).*(?=</span>)").Value;
        }


        /// <summary>
        /// 得到评论字典
        /// </summary>
        /// <returns></returns>
        private Dictionary<string, string> GetCommentDic()
        {

            var htmlUrl = $"http://item.m.jd.com/ware/getDetailCommentList.json?wareId={this._productId}";
            var htmlString = base.GetWebContent(htmlUrl);
            var dic = new Dictionary<string, string>
            {
                {"allCnt", Regex.Match(htmlString, "(?<=\"allCnt\":\").*?(?=\")").Value},
                {"goodCnt", Regex.Match(htmlString, "(?<=\"goodCnt\":\").*?(?=\")").Value},
                {"normalCnt", Regex.Match(htmlString, "(?<=\"normalCnt\":\").*?(?=\")").Value},
                {"badCnt", Regex.Match(htmlString, "(?<=\"badCnt\":\").*?(?=\")").Value},
                {"pictureCnt", Regex.Match(htmlString, "(?<=\"pictureCnt\":\").*?(?=\")").Value}
            };


            return dic;
        }


    }
}
