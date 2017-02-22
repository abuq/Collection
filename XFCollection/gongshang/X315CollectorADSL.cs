using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.SqlServer.Server;
using X.GlodEyes.Collectors;
using X.GlodEyes.Collectors.Specialized.JingDong;
using XFCollection.ADSL;

namespace XFCollection.gongshang
{
    /// <summary>
    /// X315CollectorADSL
    /// </summary>
    class X315CollectorADSL:WebRequestCollector<IResut,X315Parameter>
    {

        /// <summary>
        /// LoginInfo
        /// </summary>
        public static class LoginInfo
        {
            /// <summary>
            /// Cookies
            /// </summary>
            public static string Cookies { get; set; }

            /// <summary>
            /// UserName
            /// </summary>
            public static string UserName { get; set; }

            /// <summary>
            /// PassWord
            /// </summary>
            public static string PassWord { get; set; }

        } 



        private string _id;
        private string _orgId;
        private string _keyWord;
        private readonly Http.HttpHelper _httpHelper = new Http.HttpHelper();


        /// <summary>
        /// Test
        /// </summary>
        internal void Test()
        {
            var parameter = new X315Parameter()
            {
                KeyWord = "临海市南叶家居有限公司",
                UserName = "13157124143",
                PassWord = "m4W9TM@F"
            };

            TestHelp<X315CollectorADSL>(parameter);
        }


        /// <summary>
        /// Test1
        /// </summary>
        internal static void Test2()
        {
            var keyWords = File.ReadAllLines(@"C:\Users\Administrator\Desktop\companyName.txt");

            // 去掉字符串前后的"
            //keyWords = Array.ConvertAll(keyWords, keyWord => keyWord.Trim('"'));

            foreach (var keyWord in keyWords)
            {
                try
                {
                    var parameter = new X315Parameter
                    {
                        KeyWord = keyWord,
                        UserName = "13157124143",
                        PassWord = "m4W9TM@F"
                    };
                    TestHelp<X315CollectorADSL>(parameter);
                }
                catch (NotSupportedException exception)
                {
                    Console.WriteLine($"error: {exception.Message}");
                }
            }
        }

        /// <summary>
        /// InitFirstUrl
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        protected override string InitFirstUrl(X315Parameter param)
        {
            _keyWord = param.KeyWord;

            //Cookie为空 登录
            if (string.IsNullOrEmpty(LoginInfo.Cookies))
            {
                Login(param.UserName, param.PassWord);
            }
            else
            {
                //Cookie不为空 并且账号密码和原来相同 不登录
                if (LoginInfo.UserName == param.UserName && LoginInfo.PassWord == param.PassWord)
                {
                }
                else
                {
                    Login(param.UserName, param.PassWord);
                }
            }


            while (string.IsNullOrEmpty(_id) || string.IsNullOrEmpty(_orgId))
            {
                //得到Id
                _id = GetId(_keyWord);
                //得到orgId
                _orgId = GetOrgId(_id);
            }

            return "http://m.x315.com/";
        }


        /// <summary>
        /// ParseCurrentItems
        /// </summary>
        /// <returns></returns>
        protected override IResut[] ParseCurrentItems()
        {
            var resultList = new List<IResut>();

            var basic = GetBasicInfo(_orgId);
            var manager = GetManagerInfo(_orgId);
            var equity = GetEquityHtml(_orgId);

            IResut resut = new Resut()
            {
                {"keyWord",_keyWord },
                {"basic",basic },
                {"manage",manager},
                {"equity",equity }
            };

            resultList.Add(resut);

            return resultList.ToArray();
        }


        /// <summary>
        /// ParseNextUrl
        /// </summary>
        /// <returns></returns>
        protected override string ParseNextUrl()
        {
            return null;
        }

        /// <summary>
        /// ParseCurrentPage
        /// </summary>
        /// <returns></returns>
        protected override int ParseCurrentPage()
        {
            return 1;
        }

        /// <summary>
        /// ParseCountPage
        /// </summary>
        /// <returns></returns>
        protected override int ParseCountPage()
        {
            return 1;
        }

        /// <summary>
        /// GetBaseInfoHtml
        /// </summary>
        /// <param name="orgId"></param>
        /// <returns></returns>
        private string GetBaseInfoHtml(string orgId)
        {
            return GetHtmlByGet($"http://m.x315.com/orginfo/baseinfo/{orgId}");
        }

        /// <summary>
        /// GetBasicInfo
        /// </summary>
        /// <param name="orgId"></param>
        /// <returns></returns>
        private string GetBasicInfo(string orgId)
        {
            return GetHtmlByGet($"http://m.x315.com/orginfo/info?ajax=ajax&org_id={orgId}&tag=basic");
        }

        /// <summary>
        /// GetManagerInfo
        /// </summary>
        /// <param name="orgId"></param>
        /// <returns></returns>
        private string GetManagerInfo(string orgId)
        {
            var html = GetHtmlByGet($"http://m.x315.com/orginfo/info?ajax=ajax&org_id={orgId}&tag=manager");
            return $"{{{Regex.Match(html, @"""t_cr_org_manager"":\[{.*}\]").Value}}}";
        }

        /// <summary>
        /// GetEquityHtml
        /// </summary>
        /// <param name="orgId"></param>
        /// <returns></returns>
        private string GetEquityHtml(string orgId)
        {
            var html = GetHtmlByGet($"http://m.x315.com/orginfo/info?org_id={orgId}&tag=equity&page=1");
            return $"{{{Regex.Match(html, @"""t_cr_org_equity_capital"":\[{.*}\]").Value}}}";
        }



        /// <summary>
        /// GetId
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        private string GetId(string param)
        {
            var postDataString = $"k={param}&z=100000&sort_field=&sort_direction=&currentPage=1&found_time=&capital_range=&date={GetLinuxTimeStamp()}&num=7";
            var html = GetHtmlByPost("http://m.x315.com/search/org/selectedorg?ajax=ajax", postDataString);
            var id = Regex.Match(html, "(?<=\"id\":\").*?(?=\")").Value;
            return id;
        }

        /// <summary>
        /// GetOrgId
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private string GetOrgId(string id)
        {
            
            var html = GetHtmlByGet($"http://m.x315.com/orginfo/tobaseinfo/{id}");
            var orgId = Regex.Match(html, "(?<=<input id=\"org_id\".*?value=\").*?(?=\"/>)").Value;
            
            //if(string.IsNullOrEmpty(orgId))
            //    throw new Exception("orgId为空");
            return orgId;
        }

        /// <summary>
        /// Login
        /// </summary>
        /// <param name="userNmae"></param>
        /// <param name="passWord"></param>
        private void Login(string userNmae,string passWord)
        {
            //获得linux时间戳
            var t = GetLinuxTimeStamp();
            //先验证手机号是否注册
            GetHtmlByPost("http://m.x315.com/login/check_phone", $"p={userNmae}&date={t}");
            //登录
            var html = GetHtmlByGet($"http://m.x315.com/login/login?un={userNmae}&up={passWord}");
            var msg = Regex.Match(html, "(?<=\"msg\":\").*(?=\"})").Value;
            if(!msg.Contains("登录成功"))
                throw new Exception("登录失败！");
            //记录登录信息
            LoginInfo.Cookies = _httpHelper.Cookies;
            LoginInfo.UserName = userNmae;
            LoginInfo.PassWord = passWord;
            
        }

        /// <summary>
        /// GetLinuxTimeStamp
        /// </summary>
        /// <returns></returns>
        private long GetLinuxTimeStamp()
        {
            //linux时间戳 从1970年1月1日 0:0:0到现在的秒数
            var dateTime = new DateTime();
            var start = new DateTime(1970, 1, 1, 0, 0, 0, dateTime.Kind);
            var t = Convert.ToInt64((DateTime.Now - start).TotalSeconds);
            return t;
        }

        /// <summary>
        /// GetHtmlByGet
        /// </summary>
        /// <param name="url"></param>
        private string GetHtmlByGet(string url)
        {
            var html = string.Empty;
            
            while (string.IsNullOrEmpty(html))
            {
                try
                {
                    html = _httpHelper.GetHtmlByGet(url);
                }
                catch (Exception e)
                {
                    //如果是500错误 则自动拨号
                    if (e.Message.Contains("500"))
                    {
                        Console.WriteLine(e.Message);
                        var adslHelper = new ADSLHelper();
                        adslHelper.AutoADSLConnect();
                    }
                    else if (e.Message.Contains("404"))
                    {
                        //这里id为空会一直出问题
                        break;
                    }
                    //否则 抛出异常
                    else
                    {
                        throw;
                    }
                }

                TimeOutHandler(html, url);
            }
            return html;
        }


        /// <summary>
        /// TimeOutHandler
        /// </summary>
        /// <param name="html"></param>
        /// <param name="url"></param>
        private void TimeOutHandler(string html,string url)
        {
            var errInfo = Regex.Match(html, @"<div class=""f_h25 f_g1 f_w_b"">[\s\S]*?</div>").Value;
            while (errInfo.Contains("太厉害了! 您的查询速度已经超过"))
            {
                var adslHelper = new ADSLHelper();
                adslHelper.AutoADSLConnect();
                html = _httpHelper.GetHtmlByGet(url);
                errInfo = Regex.Match(html, @"<div class=""f_h25 f_g1 f_w_b"">[\s\S]*?</div>").Value;
            }
        }

        /// <summary>
        /// GetHtmlByPost
        /// </summary>
        /// <param name="url"></param>
        /// <param name="postDataString"></param>
        /// <returns></returns>
        private string GetHtmlByPost(string url,string postDataString)
        {
            var html = string.Empty;
            while (string.IsNullOrEmpty(html))
            {
                try
                {
                    html = _httpHelper.GetHtmlByPost(url, postDataString);
                }
                catch (Exception e)
                {
                    //如果是500错误 则自动拨号
                    if (e.Message.Contains("500"))
                    {
                        Console.WriteLine(e.Message);
                        var adslHelper = new ADSLHelper();
                        adslHelper.AutoADSLConnect();
                    }
                    //否则 抛出异常
                    else
                    {
                        throw;
                    }
                }
            }
            
            return html;
        }

        /// <summary>
        /// Test1
        /// </summary>
        private void Test1()
        {

            Login("13157124143", "m4W9TM@F");
            _id = GetId("深圳市腾讯计算机系统有限公司");
            _orgId = GetOrgId(_id);
            var html = GetBaseInfoHtml(_orgId);
            Console.WriteLine(html);
            html = GetBasicInfo(_orgId);
            html = GetManagerInfo(_orgId);
            html = GetEquityHtml(_orgId);
        }
    }
}
