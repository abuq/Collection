using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace test
{
    class SinaWeiBoDemo
    {

        /// <summary>
        /// WaitLoadingCompleted
        /// </summary>
        /// <returns></returns>
        public static Func<IWebDriver, bool> WaitLoadingCompleted(IWebDriver webDriver)
        {
            return delegate(IWebDriver driver)
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

                return loginName.Displayed&& loginPassword.Displayed&& loginAction.Displayed;

            };
        }


        /// <summary>
        /// LoginSinaWeiBo
        /// </summary>
        private void LoginSinaWeiBo()
        {
            //var url = "http://weibo.com/";
            //var url = "https://shop.m.taobao.com/shop/shop_index.htm?shop_id=131282813";
            var url = "https://shop131282813.m.taobao.com/?shop_id=131282813&user_id=2658592015";
            var options = new ChromeOptions();
            options.AddArgument(
                "--user-agent=Mozilla/5.0 (iPad; CPU OS 6_0 like Mac OS X) AppleWebKit/536.26 (KHTML, like Gecko) Version/6.0 Mobile/10A5355d Safari/8536.25");



            using (var driver = new ChromeDriver(options))
            {
                var navigation = driver.Navigate();
                navigation.GoToUrl(url);

                //driver.Manage().Window.Maximize();

                //等待时间
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
                //直到这个元素出现
                //wait.Until(ExpectedConditions.ElementExists(By.Id("loginName")));
                //wait.Until(ExpectedConditions.ElementExists(By.Id("loginPassword")));
                //wait.Until(ExpectedConditions.ElementExists(By.Id("loginAction")));

                //等待元素全部加载完成
                wait.Until(WaitLoadingCompleted(driver));

                //用户名              
                var loginName = driver.FindElement(By.Id("loginName"));
                loginName.SendKeys("15757135981");
                //密码
                var loginPassword = driver.FindElement(By.Id("loginPassword"));
                loginPassword.SendKeys("huxiaofei");
                //登录按钮
                var loginAction = driver.FindElement(By.Id("loginAction"));
                loginAction.Click();


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


        private void Test()
        {

            
            var url = "https://xiaomi.m.tmall.com/?shop_id=104736810";
            
            var options = new ChromeOptions();
            options.AddArgument(
                "--user-agent=Mozilla/5.0 (iPad; CPU OS 6_0 like Mac OS X) AppleWebKit/536.26 (KHTML, like Gecko) Version/6.0 Mobile/10A5355d Safari/8536.25");


            //options.AddArgument(
            //"--user-agent=Mozilla/5.0 (iPad; CPU OS 6_0 like Mac OS X) AppleWebKit/536.26 (KHTML, like Gecko) Version/6.0 Mobile/10A5355d Safari/8536.25");

            
            using (var driver = new ChromeDriver(options))
            {
                var navigation = driver.Navigate();
                navigation.GoToUrl(url);
            }



        }





    }
}
