using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace test
{
    class ZheJiangGov
    {

        private void Test()
        {
            var url = "http://gsxt.zjaic.gov.cn/client/entsearch/toEntSearch";
            var options = new ChromeOptions();
            options.AddArgument("--user-agent=Mozilla/5.0 (iPad; CPU OS 6_0 like Mac OS X) AppleWebKit/536.26 (KHTML, like Gecko) Version/6.0 Mobile/10A5355d Safari/8536.25");

            using (var driver = new ChromeDriver(options))
            {
                var navigation = driver.Navigate();
                navigation.GoToUrl(url);

                var seBox = driver.FindElementByClassName("se-box");
                seBox.SendKeys("杭州创和松琪贸易有限公司");

                 var popUpSubmit = driver.FindElementById("popup-submit");
                popUpSubmit.Click();


                //*[@id="geetest_1482374133413"]/div[2]/div[2]/div[2]/div[2]
                //0-198

                //var guideTipShow = driver.FindElementByXPath("//div[@class=\"gt_slider\"]//div[@class=\"gt_slider_knob gt_show\"]");
                //var guideTipShowCss = driver.FindElementByCssSelector("div.gt_slider_knob.gt_show");
                var setScroll = "document.getElementsByClassName('gt_slider_knob gt_show')[0].scrollTo=100";
                IJavaScriptExecutor jse = (IJavaScriptExecutor)driver;
                jse.ExecuteScript(setScroll);
                Thread.Sleep(1000);
            }
        }

    }
}
