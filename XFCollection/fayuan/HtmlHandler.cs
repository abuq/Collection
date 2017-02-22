using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace XFCollection.fayuan
{
    /// <summary>
    /// html处理
    /// </summary>
    public static class HtmlHandler
    {
        /// <summary>
        /// 通过GET方式获取页面的方法
        /// </summary>
        /// <param name="urlString">请求的URL</param>
        /// <param name="encoding">页面编码</param>
        /// <returns></returns>
        public static string GetHtmlFromGet(string urlString, Encoding encoding)
        {
            //定义局部变量
            HttpWebRequest httpWebRequest = null;
            HttpWebResponse httpWebResponse = null;
            Stream stream = null;
            StreamReader streamReader = null;
            var htmlString = string.Empty;

            //请求页面
            try
            {
                httpWebRequest = WebRequest.Create(urlString) as HttpWebRequest;
            }
            //处理异常
            catch (Exception ex)
            {
                throw new Exception("建立页面请求时发生错误！", ex);
            }
            httpWebRequest.UserAgent = "Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 5.1; .NET CLR 2.0.50727; Maxthon 2.0)";
            //获取服务器的返回信息
            try
            {
                httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                stream = httpWebResponse.GetResponseStream();
            }
            //处理异常
            catch (Exception ex)
            {
                throw new Exception("接受服务器返回页面时发生错误！", ex);
            }
            streamReader = new StreamReader(stream, encoding);
            //读取返回页面
            try
            {
                htmlString = streamReader.ReadToEnd();
            }
            //处理异常
            catch (Exception ex)
            {
                throw new Exception("读取页面数据时发生错误！", ex);
            }

            //释放资源返回结果
            streamReader.Close();
            stream.Close();

            return htmlString;

        }

        /// <summary>
        /// 提供通过POST方法获取页面的方法
        /// </summary>
        /// <param name="urlString">请求的URL</param>
        /// <param name="encoding">页面使用的编码</param>
        /// <param name="postDataString">POST数据</param>
        /// <returns></returns>
        public static string GetHtmlFromPost(string urlString, Encoding encoding, string postDataString)
        {
            //定义局部变量
            CookieContainer cookieContainer = new CookieContainer();
            HttpWebRequest httpWebRequest = null;
            HttpWebResponse httpWebResponse = null;
            Stream inputStream = null;
            Stream outputStream = null;
            StreamReader streamReader = null;
            string htmlString = string.Empty;
            //转换POST数据
            byte[] postDataByte = encoding.GetBytes(postDataString);
            //建立页面请求
            try
            {
                httpWebRequest = WebRequest.Create(urlString) as HttpWebRequest;
            }
            //处理异常
            catch (Exception ex)
            {
                throw new Exception("建立页面请求时发生错误！", ex);
            }
            //指定请求处理方式
            httpWebRequest.Method = "POST";
            httpWebRequest.KeepAlive = false;
            httpWebRequest.ContentType = "application/x-www-form-urlencoded";
            httpWebRequest.CookieContainer = cookieContainer;
            httpWebRequest.ContentLength = postDataByte.Length;
            //向服务器传送数据
            try
            {
                inputStream = httpWebRequest.GetRequestStream();
                inputStream.Write(postDataByte, 0, postDataByte.Length);
            }
            //处理异常
            catch (Exception ex)
            {
                throw new Exception("发送POST数据时发生错误！", ex);
            }
            finally
            {
                inputStream.Close();
            }
            //接受服务器返回信息
            try
            {
                httpWebResponse = httpWebRequest.GetResponse() as HttpWebResponse;
                outputStream = httpWebResponse.GetResponseStream();
                streamReader = new StreamReader(outputStream, encoding);
                htmlString = streamReader.ReadToEnd();
            }
            //处理异常
            catch (Exception ex)
            {
                throw new Exception("接受服务器返回页面时发生错误！", ex);
            }
            finally
            {
                streamReader.Close();
            }
            foreach (Cookie cookie in httpWebResponse.Cookies)
            {
                cookieContainer.Add(cookie);
            }
            return htmlString;

        }
    }
}
