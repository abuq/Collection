using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using HtmlAgilityPack;

namespace XFCollection.HtmlAgilityPack
{
    /// <summary>
    /// HtmlAgilityPackHelper
    /// </summary>
    public static class HtmlAgilityPackHelper
    {

        /// <summary>
        /// GetDocumentNodeByHtml
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        public static HtmlNode GetDocumentNodeByHtml(string html)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            return doc.DocumentNode;
        }




        /// <summary>
        /// GetDocumentNode
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        public static HtmlNode GetDocumentNode(string html)
        {
            var doc = new HtmlDocument();
            doc.Load(html);
            return doc.DocumentNode;
        }

  

    }
}
