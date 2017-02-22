using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;


namespace test
{
    /// <summary>
    /// QQZoneDemo
    /// </summary>
    class QQZoneDemo
    {


        private void Test()
        {

            //GetValidateCodeByUrl("http://zhixing.court.gov.cn/search/security/jcaptcha.jpg?61");
            

            Image image = Image.FromFile("C:\\Users\\Administrator\\Desktop\\验证码库\\test.jpg");
            Bitmap bitmap = (Bitmap) image;
            GetValidateCodeByImg(bitmap);

            Imgdo(bitmap);
        }



        public void Imgdo(Bitmap bitmap)
        {
            //去色
            Bitmap btp = bitmap;
            Color c = new Color();
            int rr, gg, bb;
            for (int i = 0; i < btp.Width; i++)
            {
                for (int j = 0; j < btp.Height; j++)
                {
                    //取图片当前的像素点
                    c = btp.GetPixel(i, j);
                    rr = c.R; gg = c.G; bb = c.B;
                    //改变颜色
                    if (rr == 102 && gg == 0 && bb == 0)
                    {
                        //重新设置当前的像素点
                        btp.SetPixel(i, j, Color.FromArgb(255, 255, 255, 255));
                    }
                    if (rr == 153 && gg == 0 && bb == 0)
                    {
                        //重新设置当前的像素点
                        btp.SetPixel(i, j, Color.FromArgb(255, 255, 255, 255));
                    }
                    if (rr == 153 && gg == 0 && bb == 51)
                    {
                        //重新设置当前的像素点
                        btp.SetPixel(i, j, Color.FromArgb(255, 255, 255, 255));
                    }
                    if (rr == 153 && gg == 43 && bb == 51)
                    {
                        //重新设置当前的像素点
                        btp.SetPixel(i, j, Color.FromArgb(255, 255, 255, 255));
                    }
                    if (rr == 255 && gg == 255 && bb == 0)
                    {
                        //重新设置当前的像素点
                        btp.SetPixel(i, j, Color.FromArgb(255, 255, 255, 255));
                    }
                    if (rr == 255 && gg == 255 && bb == 51)
                    {
                        //重新设置当前的像素点
                        btp.SetPixel(i, j, Color.FromArgb(255, 255, 255, 255));
                    }
                }
            }
            btp.Save("C:\\Users\\Administrator\\Desktop\\验证码库\\test\\去除相关颜色.png");

           


            //灰度
            Bitmap bmphd = btp;
            for (int i = 0; i < bmphd.Width; i++)
            {
                for (int j = 0; j < bmphd.Height; j++)
                {
                    //取图片当前的像素点
                    var color = bmphd.GetPixel(i, j);

                    var gray = (int)(color.R * 0.001 + color.G * 0.700 + color.B * 0.250);

                    //重新设置当前的像素点
                    bmphd.SetPixel(i, j, Color.FromArgb(gray, gray, gray));
                }
            }
            bmphd.Save("C:\\Users\\Administrator\\Desktop\\验证码库\\test\\灰度.png");
            


            //二值化
            Bitmap erzhi = bmphd;
            Bitmap orcbmp;
            int nn = 3;
            int w = erzhi.Width;
            int h = erzhi.Height;
            BitmapData data = erzhi.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            unsafe
            {
                byte* p = (byte*)data.Scan0;
                byte[,] vSource = new byte[w, h];
                int offset = data.Stride - w * nn;

                for (int y = 0; y < h; y++)
                {
                    for (int x = 0; x < w; x++)
                    {
                        vSource[x, y] = (byte)(((int)p[0] + (int)p[1] + (int)p[2]) / 3);
                        p += nn;
                    }
                    p += offset;
                }
                erzhi.UnlockBits(data);

                Bitmap bmpDest = new Bitmap(w, h, PixelFormat.Format24bppRgb);
                BitmapData dataDest = bmpDest.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
                p = (byte*)dataDest.Scan0;
                offset = dataDest.Stride - w * nn;
                for (int y = 0; y < h; y++)
                {
                    for (int x = 0; x < w; x++)
                    {
                        p[0] = p[1] = p[2] = (int)vSource[x, y] > 161 ? (byte)255 : (byte)0;
                        //p[0] = p[1] = p[2] = (int)GetAverageColor(vSource, x, y, w, h) > 50 ? (byte)255 : (byte)0;
                        p += nn;

                    }
                    p += offset;
                }
                bmpDest.UnlockBits(dataDest);

                orcbmp = bmpDest;
                orcbmp.Save("C:\\Users\\Administrator\\Desktop\\验证码库\\test\\二值化.png");
            }


            var code = GetValidateCodeByImg(orcbmp);
            Console.WriteLine(code);


        }

        /// <summary>
        /// GetValidateCodeByImg
        /// </summary>
        /// <returns></returns>
        private string GetValidateCodeByImg(Bitmap bitmap)
        {
            var code = string.Empty;
            try
            { 
                //使用tessnet2解析图片
                tessnet2.Tesseract ocr = new tessnet2.Tesseract();
                //设置识别变量，当前只能识别数字
                //数字英文大小写字母
                //0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz
                ocr.SetVariable("tessedit_char_whitelist", "1234567890ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz");
                //应用当前语言包。注，Tessnet2是支持多国语的。语言包下载链接：http://code.google.com/p/tesseract-ocr/downloads/list

                var binPath = Regex.Match(Environment.CurrentDirectory, ".*(?=bin)").Value;

                //ocr.Init($"{binPath}tessdata", "eng", true);
                ocr.Init($"{binPath}tessdata", "eng", false);
                //ocr.Init($"{binPath}tessdata", "eng", true);

                //执行识别操作
                List<tessnet2.Word> result = ocr.DoOCR(bitmap, Rectangle.Empty);
                

                code = result[0].Text;

                Console.WriteLine($"解析的验证码为{code}");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return code;
        }

        /// <summary>
        /// GetValidateCodeByUrl
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private string GetValidateCodeByUrl(string url)
        {
            string code = string.Empty;
            try
            {
                //通过网页链接解析图片
                Uri uri = new Uri(url);
                HttpWebRequest request = WebRequest.Create(uri) as HttpWebRequest;
                HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                Stream stream = response.GetResponseStream();
                Bitmap bitmap = new Bitmap(stream);

                //使用tessnet2解析图片
                tessnet2.Tesseract ocr = new tessnet2.Tesseract();
                //设置识别变量，当前只能识别数字
                //数字英文大小写字母
                //0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz
                ocr.SetVariable("tessedit_char_whitelist", "1234567890ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz");
                //应用当前语言包。注，Tessnet2是支持多国语的。语言包下载链接：http://code.google.com/p/tesseract-ocr/downloads/list

                var binPath = Regex.Match(Environment.CurrentDirectory, ".*(?=bin)").Value;
                //ocr.Init($"{binPath}tessdata", "eng", true);
                //ocr.Init("C:\\tessdata", "eng", true);
                ocr.Init("C:\\tessdata", "eng", false);
                //执行识别操作
                List<tessnet2.Word> result = ocr.DoOCR(bitmap, Rectangle.Empty);

                code = result[0].Text;
                
                Console.WriteLine($"解析的验证码为{code}");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return code;
        }

        /// <summary>
        /// WaitLoadingCompleted
        /// </summary>
        /// <returns></returns>
        public static Func<IWebDriver, bool> WaitLoadingCompleted(IWebDriver webDriver)
        {
            return delegate (IWebDriver driver)
            {
                IWebElement loginName = null;
                IWebElement loginPassword = null;
                IWebElement loginAction = null;
                try
                {
                    loginName = driver.FindElement(By.Id("loginName"));
                    loginPassword = driver.FindElement(By.Id("loginPassword"));
                    loginAction = driver.FindElement(By.Id("loginAction"));

                }
                catch (NoSuchElementException)
                {
                    return false;
                }

                return loginName.Displayed && loginPassword.Displayed && loginAction.Displayed;

            };
        }


        /// <summary>
        /// LoginQQZone
        /// </summary>
        private void LoginQQZone()
        {
            var url = "http://qzone.qq.com/";
            var options = new ChromeOptions();
            options.AddArgument(
                "--user-agent=Mozilla/5.0 (iPad; CPU OS 6_0 like Mac OS X) AppleWebKit/536.26 (KHTML, like Gecko) Version/6.0 Mobile/10A5355d Safari/8536.25");



            using (var driver = new ChromeDriver(options))
            {
                var navigation = driver.Navigate();
                navigation.GoToUrl(url);
                //切换到iframe里
                driver.SwitchTo().Frame("login_frame");

                //driver.Manage().Window.Maximize();
                //
                //等待时间
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
                //等待加载完成
                //wait.Until(ExpectedConditions.ElementExists(By.XPath("//*[@id=\"switcher_plogin\"]")));

                //点击账号密码登录
                //login_frame

                var switcher_plogin = driver.FindElement(By.XPath("//*[@id=\"switcher_plogin\"]"));
                switcher_plogin.Click();

                var userName = driver.FindElement(By.Id("u"));
                userName.SendKeys("2465926548");
                var passWord = driver.FindElement(By.Id("p"));
                passWord.SendKeys("hu13515785161");
                var login_button = driver.FindElement(By.Id("login_button"));
                login_button.Click();

                //driver.SwitchTo().DefaultContent();
                //没有iframe id 所以移到第一个子frame
                //qq空间的验证码破不了
                //driver.SwitchTo().Frame(0);

                //var capImg = driver.FindElement(By.Id("capImg"));
                //var imageSrc = capImg.GetAttribute("src");

                //var code = GetValidateCodeByUrl(imageSrc);

                //var capAns = driver.FindElement(By.Id("capAns"));
                //capAns.SendKeys(code);

                //var submit = driver.FindElement(By.Id("submit"));
                //submit.Click();



                Console.WriteLine($"导航完成");
                Console.WriteLine($"标题:{driver.Title}");
                Console.WriteLine($"url:{driver.Url}");
                Console.WriteLine($"状态:{driver}");

                var desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                var imgPath = Path.Combine(desktop, @"demo.png");

                var screenshot = driver.GetScreenshot();
                screenshot.SaveAsFile(imgPath, ImageFormat.Png);

                var cookies = driver.Manage().Cookies.AllCookies;

                var cookieString = string.Empty;
                var length = cookies.Count;
                var i = 1;
                foreach (var cookie in cookies)
                {
                    if (i < length)
                        cookieString += $"{cookie.Name}={cookie.Value};";
                    else
                    {
                        cookieString += $"{cookie.Name}={cookie.Value}";
                    }

                    i += 1;
                }

                Console.WriteLine($"获取cookies:{cookieString}");



                Process.Start(imgPath);
            }
        }





    }
}

