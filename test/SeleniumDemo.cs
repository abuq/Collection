using System;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.PhantomJS;
using System.Linq;
using System.Runtime.CompilerServices;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Support.UI;

namespace test
{


    //http://www.cnblogs.com/NorthAlan/p/5155915.html
    //[小北De编程手记] : Lesson 01 - Selenium For C# 之 环境搭建
    /// <summary>
    /// SeleniumDemo
    /// </summary>
    public class SeleniumDemo
    {
        /// <summary>
        /// Test
        /// </summary>
        public void Test()
        {
            //var url = @"https://list.taobao.com/itemlist/default.htm?viewIndex=1&commend=all&atype=b&nick=恒源祥风度专卖店&style=list&same_info=1&tid=0&isnew=2&zk=all&_input_charset=utf-8";
            var url = "http://zhixing.court.gov.cn/search/";
            var userAgent = @"Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/51.0.2704.106 Safari/537.36";
            var options = new PhantomJSOptions();
            options.AddAdditionalCapability(@"phantomjs.page.settings.userAgent", userAgent);

            using (var driver = new PhantomJSDriver(options))
            {
                var navigation = driver.Navigate();
                navigation.GoToUrl(url);

                Console.WriteLine($"导航完成");
                Console.WriteLine($"标题:{driver.Title}");
                Console.WriteLine($"url:{driver.Url}");
                Console.WriteLine($"状态:{driver}");

                var desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                var imgPath = Path.Combine(desktop, @"demo.png");

                var screenshot = driver.GetScreenshot();
                screenshot.SaveAsFile(imgPath,ImageFormat.Png);

                var cookies = driver.Manage().Cookies;
                Console.WriteLine($"获取cookies:{cookies}");

                Process.Start(imgPath);

            }

            Console.WriteLine(@"完成");
            Console.ReadKey();


        }

        /// <summary>
        /// 模拟登陆 taobao
        /// </summary>
        public void LoginSina()
        {
            var url = "https://login.taobao.com/member/login.jhtml";
            //var url = "http://zhixing.court.gov.cn/search/";
            var options = new ChromeOptions();
            options.AddArgument("--user-agent=Mozilla/5.0 (iPad; CPU OS 6_0 like Mac OS X) AppleWebKit/536.26 (KHTML, like Gecko) Version/6.0 Mobile/10A5355d Safari/8536.25");



            using (var driver = new ChromeDriver(options))
            {
                var navigation = driver.Navigate();
                navigation.GoToUrl(url);

                //真实环境中元素往往使用复合类名(即多个class用空格分隔)，使用className定位时要注意了，className的参数只能是一个class。
                //报错  Compound class names not permitted
                //var switch_a = driver.FindElementByClassName("rget-pwd J_Quick2Static");

                //driver.Manage().Window.Maximize();


                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));



                var switch_a = driver.FindElementByXPath("//*[@id=\"J_QRCodeLogin\"]//a[@class=\"forget-pwd J_Quick2Static\"]");
                switch_a.Click();

                var userName = driver.FindElementById("TPL_username_1");
                Console.WriteLine(userName.Text);


                //userName.GetAttribute("")

                userName.SendKeys("崔平绿asf");



                var passWord = driver.FindElementById("TPL_password_1");
                passWord.SendKeys("einj915");

                var submitButtom = driver.FindElementById("J_SubmitStatic");

                submitButtom.Click();


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
                    if(i<length)
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
