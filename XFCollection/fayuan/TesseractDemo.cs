using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using X.CommLib.Logs;


namespace XFCollection.fayuan
{
    /// <summary>
    /// 验证码识别
    /// </summary>
    public class TesseractDemo
    {
        /// <summary>
        /// 测试
        /// </summary>
        public void Test()
        {

            //Console.WriteLine(Environment.CurrentDirectory);
            //var binPath = Regex.Match(Environment.CurrentDirectory, @".*\\bin");

            HandleValidateCode();




            ////http://wenshu.court.gov.cn/Html_Pages/VisitRemind.html
            //Uri uri = new Uri("http://wenshu.court.gov.cn/User/ValidateCode");
            //HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(uri);
            //HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            //Stream resStream = response.GetResponseStream();//得到验证码数据流
            //Bitmap bitmap = new Bitmap(resStream);//初始化Bitmap图片

            //tessnet2.Tesseract ocr = new tessnet2.Tesseract();//声明一个OCR类
            //ocr.SetVariable("tessedit_char_whitelist", "0123456789"); //设置识别变量，当前只能识别数字。
            ////ocr.Init(@"D:\tessdata", "eng", true); //应用当前语言包。注，Tessnet2是支持多国语的。语言包下载链接：http://code.google.com/p/tesseract-ocr/downloads/list
            //List<tessnet2.Word> result = ocr.DoOCR(bitmap, Rectangle.Empty);//执行识别操作
            //string code = result[0].Text;
            //Console.WriteLine(code);

        }

        /// <summary>
        /// 验证码链接
        /// </summary>
        private string validateCodeUrl = "http://wenshu.court.gov.cn/User/ValidateCode";
        /// <summary>
        /// 验证验证码链接
        /// </summary>
        private string checkValidateCodeUrl = "http://wenshu.court.gov.cn/Content/CheckVisitCode";

        /// <summary>
        /// _cookies
        /// </summary>
        private string _cookies;

        /// <summary>
        /// Cookies
        /// </summary>
        public string Cookies
        {
            get { return _cookies;}
            set { _cookies = value; }
        }

        /// <summary>
        /// 语言包路径
        /// </summary>
        public static string Path { set; get; } = @"C:\tessdata";

        /// <summary>
        /// 得到验证码数字通过链接
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public string GetValidateCodeByUrl(string url)
        {
            int code = -1;
            //通过网页链接解析图片
            Uri uri = new Uri(url);
            HttpWebRequest request = WebRequest.Create(uri) as HttpWebRequest;
            request.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/34.0.1847.131 Safari/537.36";
            HttpWebResponse response = request.GetResponse() as HttpWebResponse;
            //var cookieCollection = response.Cookies;
            _cookies = response.Headers["set-cookie"].Replace(",",";");
                
                
                
            Stream stream = response.GetResponseStream();
            Bitmap bitmap = new Bitmap(stream);
            //bitmap.Save(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "test.bmp"));

            //MessageBox.Show("保存图片");

            //使用tessnet2解析图片
            tessnet2.Tesseract ocr = new tessnet2.Tesseract();

            //MessageBox.Show("保存图片1");

            //设置识别变量，当前只能识别数字
            //数字英文大小写字母
            //0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz
            ocr.SetVariable("tessedit_char_whitelist", "0123456789");

            //MessageBox.Show("保存图片2");

            //应用当前语言包。注，Tessnet2是支持多国语的。语言包下载链接：http://code.google.com/p/tesseract-ocr/downloads/list

            //var binPath = Regex.Match(Environment.CurrentDirectory, ".*(?=bin)").Value;
            //ocr.Init($"{binPath}tessdata", "eng", true);

            //MessageBox.Show(Path);

            if (string.IsNullOrEmpty(Path) || !Directory.Exists(Path))
            {
                throw new ApplicationException("解码组件Path为空或者路径不存在");
            }

            ocr.Init(Path, "eng", true);



            //MessageBox.Show("保存图片3");

            //执行识别操作
            List<tessnet2.Word> result = ocr.DoOCR(bitmap, Rectangle.Empty);

            //MessageBox.Show("保存图片4");

            var value = result[0].Text;
            if (int.TryParse(value, out code) == false)
                throw new Exception("验证码不是全为数字，请更改程序！");

            //MessageBox.Show("保存图片5");

            Console.WriteLine($"解析的验证码为{code}");
            
            //MessageBox.Show("保存图片6");

            //MessageBox.Show("保存图片7");

            return value;
        }


        /// <summary>
        /// 验证验证码通过验证码数字
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        private int ValidateCode(string code)
        {
            string htmlString = HtmlHandler.GetHtmlFromPost(checkValidateCodeUrl, Encoding.UTF8, $"ValidateCode={code}");

            int isSuccess = 2;
            if(int.TryParse(htmlString,out isSuccess) == false)
                throw new Exception("验证码判断的规则变了！");

            return isSuccess;

        }
        /// <summary>
        /// 处理验证码
        /// </summary>
        /// <returns></returns>
        public bool HandleValidateCode()
        {
            var code = GetValidateCodeByUrl(validateCodeUrl);
            int isSuccess = ValidateCode(code);
            if (isSuccess == 1)
            {
                Console.WriteLine(@"验证码验证成功！");
                return true;
            }
            else
            {
                Console.WriteLine(@"验证码验证失败！");
                return false;
            }
        }

        



    }

}
