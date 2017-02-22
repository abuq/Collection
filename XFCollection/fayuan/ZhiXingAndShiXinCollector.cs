using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using X.CommLib.Net.WebRequestHelper;
using X.GlodEyes.Collectors;

namespace XFCollection.fayuan
{
    /// <summary>
    /// ZhiXingAndShiXinCollector
    /// </summary>
    public class ZhiXingAndShiXinCollector:WebRequestCollector<IResut,ZhiXingAndShiXinParameter>
    {

        /// <summary>
        /// UrlType
        /// </summary>
        private enum UrlType
        {
            ZhiXing,
            ShiXin
        };


        private string _zhixingUrl = "http://zhixing.court.gov.cn/search/";
        private string _zhixingCaptchaUrl = "http://zhixing.court.gov.cn/search/security/jcaptcha.jpg";
        private string _zhixingValidateUrl = "http://zhixing.court.gov.cn/search/newsearch";
        //private string _shixinUrl = "http://shixin.court.gov.cn/";
        private string _shixinCaptchaUrl = "http://shixin.court.gov.cn/image.jsp";
        private string _shixinValidateUrl = "http://shixin.court.gov.cn/findd";

        private string _name;
        private string _identifier;
        private string _type;
        private UrlType _urlType;

        private string _failInfo = "失败三次!";

        /// <summary>
        /// Test
        /// </summary>
        public static void Test()
        {
            var parameter = new ZhiXingAndShiXinParameter()
            {
                //Name = "肖小立",
                //Identifier = "440882198707091118"
                Name = "钱多",
                Identifier = "",
                Type= "zhixing"
            };

            //TesseractDemo.Path = @"C:\Users\Administrator\AppData\Local\Temp\JudgementDocListResources\tessdata";
            TestHelp<ZhiXingAndShiXinCollector>(parameter);

        }


        

        /// <summary>
        /// InitFirstUrl
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        protected override string InitFirstUrl(ZhiXingAndShiXinParameter param)
        {
            _name = param.Name;
            _identifier = param.Identifier;
            _type = param.Type;
            _urlType = _type.ToLower().Equals("zhixing") ? UrlType.ZhiXing : UrlType.ShiXin;
            return _zhixingUrl;
        }



        /// <summary>
        /// ParseCurrentItems
        /// </summary>
        /// <returns></returns>
        protected override IResut[] ParseCurrentItems()
        {

            var resultList = new List<IResut>();
            var ListDic = GetAllResultListDic();
            var checkTime = System.DateTime.Now;

            foreach (var dic in ListDic)
            {
                IResut resut = new Resut();

                resut["Name"] = _name;
                resut["Identifier"] = _identifier;
                resut["UserName"] = dic["UserName"];
                resut["CaseTime"] = dic["CaseTime"];
                resut["CaseId"] = dic["CaseId"];
                resut["Id"] = dic["Id"];
                resut["Type"] = dic["Type"];
                resut["State"] = dic["State"];
                resut["View"] = string.Empty;
                resut["CheckTime"] = checkTime;
                

                resultList.Add(resut);
            }

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
        /// 
        /// </summary>
        /// <param name="nextUrl"></param>
        /// <param name="postData"></param>
        /// <param name="cookies"></param>
        /// <param name="currentUrl"></param>
        /// <returns></returns>
        protected override string GetMainWebContent(string nextUrl, byte[] postData, ref string cookies, string currentUrl)
        {
            Uri uri = new Uri(nextUrl);
            WebRequestCtrl.WebRequestCtrlSetting setting = new WebRequestCtrl.WebRequestCtrlSetting(uri, postData, true, cookies, currentUrl, 5 * 60000);

            using (var args = WebRequestCtrl.GetResponse(setting))
            {
                var exception = args.Error;
                if (exception != null)
                {
                    throw exception;
                }
                
                return args.GetWebContentString();
            }

        }


        /// <summary>
        /// GetHtmlLoopUntilSuccess
        /// </summary>
        /// <param name="urlType"></param>
        /// <returns></returns>
        private string GetHtmlLoopUntilSuccess(UrlType urlType)
        {
            var success = false;
            var html = string.Empty;
            while (!success)
            {
                html = GetHtmlByUrl(urlType);
                //设置这个值 为失败三次的信息
                if (html.Equals(_failInfo))
                {
                    return _failInfo;
                }
                var title = Regex.Match(html, "(?<=<title>).*(?=</title>)").Value;
                if (!title.Equals("验证码出现错误，请重新输入！"))
                    success = true;
            }
            return html;
        }

        /// <summary>
        /// GetHtmlByUrl
        /// </summary>
        /// <returns></returns>
        private string GetHtmlByUrl(UrlType urlType)
        {
            TesseractDemo tesseractDemo = new TesseractDemo();
            string cookies;
            string code = string.Empty;
            
            var dic = new Dictionary<string, string>();
            string html = string.Empty;
            if (urlType == UrlType.ZhiXing)
            {
                code = tesseractDemo.GetValidateCodeByUrl(_zhixingCaptchaUrl);
                cookies = tesseractDemo.Cookies;
                //这部分cookie要去掉才能显示正确的内容
                cookies = cookies.Replace(";Path=/search", "");
                dic.Add("selectCourtId", "1");
                dic.Add("selectCourtArrange", "1");
                dic.Add("searchCourtName", "全国法院（包含地方各级法院）");
                dic.Add("pname", _name);
                dic.Add("cardNum", _identifier);
                dic.Add("j_captcha", code.ToString());
                var postData = WebRequestCtrl.BuildPostDatas(dic, Encoding.UTF8);
                html = base.GetWebContent(_zhixingValidateUrl, postData, ref cookies);
            }
            else
            {
                var time = DateTime.Now;
                _shixinCaptchaUrl = $"{_shixinCaptchaUrl}?date={time}";



                code = tesseractDemo.GetValidateCodeByUrl(_shixinCaptchaUrl);
                cookies = tesseractDemo.Cookies;
                var times = 1;
                while (string.IsNullOrEmpty(cookies)||string.IsNullOrEmpty(code))
                {
                    times++;
                    if (times > 3)
                    {
                        return _failInfo;
                        //throw new Exception("三次请求都没有得到cookies或者解析验证码出错!");
                    }
                    code = tesseractDemo.GetValidateCodeByUrl(_shixinCaptchaUrl);
                    cookies = tesseractDemo.Cookies;
                }
                cookies = cookies.Replace("; Path=/", "");
                //cookies = tesseractDemo.Cookies;
                dic.Add("pProvince", "0");
                dic.Add("pName", _name);
                dic.Add("pCode", code.ToString());
                dic.Add("pCardNum", _identifier);
                var postData = WebRequestCtrl.BuildPostDatas(dic, Encoding.UTF8);


                html = GetHtmlFaildReturnEmpty(_shixinValidateUrl, postData, cookies);
                times = 1;
                while (string.IsNullOrEmpty(html))
                {
                    times++;
                    if (times > 3)
                    {
                        return _failInfo;
                        //throw new Exception("三次请求都没有得到网页内容!");
                    }
                    html = GetHtmlFaildReturnEmpty(_shixinValidateUrl, postData, cookies);
                }
            }

            return html;
        }


        /// <summary>
        /// 正文内容异常返回空字符串
        /// </summary>
        /// <param name="url"></param>
        /// <param name="postData"></param>
        /// <param name="cookies"></param>
        /// <returns></returns>
        private string GetHtmlFaildReturnEmpty(string url,byte[] postData, string cookies)
        {
            string html;
            try
            {
                html = base.GetWebContent(_shixinValidateUrl, postData, ref cookies);
            }
            catch (Exception)
            {
                return string.Empty;
            }

            return html;
        }

        /// <summary>
        /// GetResultListDic
        /// </summary>
        /// <param name="html"></param>
        /// <param name="urlType"></param>
        /// <returns></returns>
        private List<Dictionary<string, string>> GetResultListDic(string html,UrlType urlType)
        {
            var listDic = new List<Dictionary<string, string>>();
            //var type = urlType == UrlType.ZhiXing ? "ZhiXing" : "ShiXin";
            var trs = Regex.Matches(html, @"<tr style.*>[\s\S]*?</tr>");

            if (trs.Count == 0)
            {
                var dic = new Dictionary<string, string>
                {
                    {"UserName", ""},
                    {"CaseTime", ""},
                    {"CaseId", ""},
                    {"Id", ""},
                    {"Type", _type},
                    {"State","success" }
                };

                listDic.Add(dic);
            }
            else
            {
                foreach (Match tr in trs)
                {
                    var trValue = tr.Value;
                    var values = Regex.Matches(trValue, @"(?<=<td.*"">)[\S]*(?=</td>)");
                    var list = (from Match value in values select value.Value).ToList();
                    if (list.Count != 4)
                        throw new Exception("解析td格式与标准格式不符！");
                    var dic = new Dictionary<string, string>
                    {
                        {"UserName",Regex.Match(list[1],".*(?=<)").Value },
                        {"CaseTime", list[2]},
                        {"CaseId", list[3]},
                        {"Id", Regex.Match(trValue, @"(?<=id="")\d+(?="")").Value},
                        {"Type", _type},
                        {"State","success" }
                    };
                    listDic.Add(dic);
                }
            }

            return listDic;
        }



        /// <summary>
        /// UpdateResultRankInfo
        /// </summary>
        /// <param name="items"></param>
        /// <param name="page"></param>
        protected override void UpdateResultRankInfo(IResut[] items, int page)
        {
            base.UpdateResultRankInfo(items, page);

            foreach (var item in items)
            {
                item.Remove("SearchPageIndex");
                item.Remove("SearchPageRank");
            }


        }

        /// <summary>
        /// GetAllResultListDic
        /// </summary>
        /// <returns></returns>
        private List<Dictionary<string, string>> GetAllResultListDic()
        {
            //var htmlZhiXing = GetHtmlLoopUntilSuccess(UrlType.ZhiXing);
            //var listDicZhiXing = GetResultListDic(htmlZhiXing,UrlType.ZhiXing);
            //var htmlShiXin = GetHtmlLoopUntilSuccess(UrlType.ShiXin);
            //var listDicShiXin = GetResultListDic(htmlShiXin, UrlType.ShiXin);
            //var listDicAll = listDicZhiXing.ToList();
            //listDicAll.AddRange(listDicShiXin);


            var listDicAll = new List<Dictionary<string, string>>();
            var html = GetHtmlLoopUntilSuccess(_urlType);
            if (html.Equals(_failInfo))
            {

                var dic = new Dictionary<string, string>
                {
                    {"UserName","" },
                    {"CaseTime", ""},
                    {"CaseId", ""},
                    {"Id", ""},
                    {"Type", _type},
                    {"State","fail" }
                };
                listDicAll.Add(dic);
            }
            else
            {
                listDicAll = GetResultListDic(html, _urlType);
            }

            return listDicAll;
        }

    }
}
