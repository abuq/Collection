namespace X.GlodEyes.Collectors.Specialized.JingDong
{
    using System;
    using System.IO;
    using System.Xml.XPath;

    using X.CommLib.Net.Miscellaneous;

    /// <summary>
    /// 店铺信息解析器
    /// </summary>
    internal class CompanyInfoParser
    {
        /// <summary>
        ///     Tests this instance.
        /// </summary>
        private static void Test()
        {
            var filePath = @"C:\Users\sinoX\Desktop\高级信息.txt";
            var webContent = File.ReadAllText(filePath);
            var parser = new CompanyInfoParser();
            var resut = parser.Parse(webContent);

            Console.WriteLine(resut);
            /*webContent = Regex.Replace(webContent, "<[^>]+>", "");*/
        }

        /// <summary>
        /// 开始解析
        /// </summary>
        /// <param name="webContent">Content of the web.</param>
        /// <returns></returns>
        public IResut Parse(string webContent)
        {
            IResut resut = new Resut();
            var navigator = HtmlDocumentHelper.CreateNavigator(webContent);

            ParseShopScoreResult(resut, navigator);

            throw new NotImplementedException();
        }


        /// <summary>
        /// 180 天店铺评分
        /// </summary>
        /// <param name="resut">The resut.</param>
        /// <param name="navigator">The navigator.</param>
        private void ParseShopScoreResult(IResut resut, XPathNavigator navigator)
        {
            throw new NotImplementedException();

            /*var shopScoreNode = navigator.SelectSingleNode(@"(//div[@class='j-score'])[1]");
            if (shopScoreNode != null)
            {
                var iterator = shopScoreNode.Select(@"./div");
                var nodes = HtmlDocumentHelper.CopyNodeToArray(iterator);
                Array.ForEach(nodes,
                    node =>

                    {
                        var infos = node.Value.Split('\r', '\n');
                        infos = Array.ConvertAll(infos, info => info.Trim());
                        infos = Array.FindAll(infos, info => info.Length > 0);

                        var indexOf = infos[0].IndexOf("：");
                        var key = infos[indexOf]

                        Console.WriteLine(new string('=', 32));
                        Console.WriteLine(string.Join(" # ", infos));
                        Console.WriteLine(new string('-', 32));
                    });
            }*/
        }
    }
}