using System;
using System.Net;
using System.Text;

namespace XFCollection.Http
{
    class WebClientDemo
    {
        private void Test()
        {
            string uri = "http://coderzh.cnblogs.com";
            WebClient wc = new WebClient();
            Console.WriteLine("Sending an HTTP GET request to " + uri);
            byte[] bResponse = wc.DownloadData(uri);
            string strResponse = Encoding.ASCII.GetString(bResponse);
            Console.WriteLine("HTTP response is: ");
            Console.WriteLine(strResponse);
            
        }
    }
}
