using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using GE.Data;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using X.CommLib.Ctrl.UserCodes;
using X.CommLib.Net.WebRequestHelper;
using X.GlodEyes.Collectors;
using X.GlodEyes.Collectors.Specialized.JingDong;
using XFCollection.UserCode;

namespace XFCollection.gongshang
{
    /// <summary>
    /// 浙江
    /// </summary>
    public class Zhejiang:WebRequestCollector<IResut,NormalParameter>
    {

        private string _corpid;
        private const string RootUrl = "http://gsxt.zjaic.gov.cn/";
        private string _companyName;
        private string _cookies;
        private string _regNo;
        private string _uniSCID;
        



        /// <summary>
        /// 测试
        /// </summary>
        internal static void Test()
        {
            //一开电气集团有限公司
            var parameter = new NormalParameter {Keyword = "一开电气集团有限公司" };
            TestHelp<Zhejiang>(parameter,1);
        }

        /// <summary>
        /// 定义第一个链接
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        protected override string InitFirstUrl(NormalParameter param)
        {
            _companyName = param.Keyword;
            //_companyName = "一开电气集团有限公司";
            //var parser = CreateUserCodeParser(@"hxf", null);
            VerificationCodeLoop(_companyName);

            return $"http://gsxt.zjaic.gov.cn/appbasicinfo/doReadAppBasicInfo.do?corpid={_corpid}";
        }


        /// <summary>
        /// 解析当前元素
        /// </summary>
        /// <returns></returns>
        protected override IResut[] ParseCurrentItems()
        {


            var resultList = new List<IResut>();
            Resut resut;
            DataTable dt = new DataTable();
            dt.Columns.Add("title", Type.GetType("System.String"));
            dt.Columns.Add("url", Type.GetType("System.String"));
            dt.Columns.Add("content", Type.GetType("System.String"));

            if (!string.IsNullOrEmpty(_corpid))
            {
                //http://blog.csdn.net/zhichao2001/article/details/46681075
                var appBasicInfo = GetAppBasicInfo();
                AddDataTable(ref dt, appBasicInfo);
                var filingInfo = GetFilingInfo();
                AddDataTable(ref dt, filingInfo);
                var dcdyApplyinfoList = GetDcdyApplyinfoList();
                AddDataTable(ref dt, dcdyApplyinfoList);
                var equityAllListFromPv = GetEquityAllListFromPV();
                AddDataTable(ref dt, equityAllListFromPv);
                var punishmentFromPv = GetPunishmentFromPV();
                AddDataTable(ref dt, punishmentFromPv);
                var catalogApplyList = GetCatalogApplyList();
                AddDataTable(ref dt, catalogApplyList);
                var blackListInfo = GetBlackListInfo();
                AddDataTable(ref dt, blackListInfo);
                var pubCheckResultList = GetPubCheckResultList();
                AddDataTable(ref dt, pubCheckResultList);
                var pubReportInfoList = GetPubReportInfoList();
                AddDataTable(ref dt, pubReportInfoList);
                var pubFunded = GetPubFunded();
                AddDataTable(ref dt, pubReportInfoList);
                var pubStock = GetPubStock();
                AddDataTable(ref dt, pubStock);
                var pubLicense = GetPubLicense();
                AddDataTable(ref dt, pubLicense);
                var pubInstantIntellectual = GetPubInstantIntellectual();
                AddDataTable(ref dt, pubInstantIntellectual);
                var pubInstantPunish = GetPubInstantPunish();
                AddDataTable(ref dt, pubInstantPunish);
                var pubOtherLicenceInfo = GetPubOtherLicenceInfo();
                AddDataTable(ref dt, pubOtherLicenceInfo);
                var pubOtherPunishInfo = GetPubOtherPunishInfo();
                AddDataTable(ref dt, pubOtherPunishInfo);
                var frozJusticeInfo = GetFrozJusticeInfo();
                AddDataTable(ref dt, frozJusticeInfo);
                var thawJusticeInfo = GetThawJusticeInfo();
                AddDataTable(ref dt, thawJusticeInfo);

                resut = new Resut
                {
                    ["Corpid"] = _corpid,
                    ["RgeNo"] = _regNo,
                    ["UniSCID"] = _uniSCID,
                    ["CompanyName"] = _companyName,
                    ["Contents"] = JsonConvert.SerializeObject(dt)
                };


            }
            else
            {
                var stringEmpty = string.Empty;
                resut = new Resut
                {
                    ["Corpid"] = stringEmpty,
                    ["RgeNo"] = stringEmpty,
                    ["UniSCID"] = stringEmpty,
                    ["CompanyName"] = _companyName,
                    ["Contents"] = stringEmpty
                };
            }
            
            resultList.Add(resut);
            return resultList.ToArray();

        }


        /// <summary>
        /// AddDataTable
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="dic"></param>
        private void AddDataTable(ref DataTable dt, Dictionary<string, string> dic)
        {
            var dr = dt.NewRow();
            dr["title"] = dic["title"];
            dr["url"] = dic["url"];
            dr["content"] = dic["content"];
            dt.Rows.Add(dr);
        }

        /// <summary>
        /// 定义下一个链接
        /// </summary>
        /// <returns></returns>
        protected override string ParseNextUrl()
        {
            return null;
        }


        /// <summary>
        /// 得到图片
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private byte[] GetImageBytes(string url)
        {
            //通过网页链接解析图片
            var uri = new Uri(url);
            var request = WebRequest.Create(uri) as HttpWebRequest;
            if (request == null) return null;
            request.Method = "get";
            request.Accept = "image/webp,image/*,*/*;q=0.8";
            //不写这个出不来图片
            request.Referer = "http://gsxt.zjaic.gov.cn/search/doEnGeneralQueryPage.do";
            
            
            request.UserAgent =
                "User-Agent: Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/51.0.2704.106 Safari/537.36";
            
            var response = request.GetResponse() as HttpWebResponse;
            
            var strings = response?.Headers.GetValues("Set-Cookie");
            //全局cookies
            _cookies = string.Join(";", strings ?? new string[0]);

            /*
                  string[] strArray = (string[]) null;
      if ((webResponse != null ? webResponse.Headers : (WebHeaderCollection) null) != null)
        strArray = webResponse.Headers.GetValues("Set-Cookie");
      if (strArray != null)
        return string.Join(";", strArray);
      return string.Empty;
             */

            var stream = response?.GetResponseStream();
            if(stream == null) return null;
            var bitmap = new Bitmap(stream);
            var ms = new MemoryStream();
            bitmap.Save(ms, bitmap.RawFormat);
            var byteImage = new Byte[ms.Length];
            byteImage = ms.ToArray();
            if(byteImage.Length == 0)
                throw new Exception("没有找到图片!");
            return byteImage;

        }


        /// <summary>
        /// 验证验证码
        /// </summary>
        /// <param name="code"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        private bool VerificationCode(string code,string name)
        {
            IDictionary<string,string> dic = new Dictionary<string, string>();
            dic.Add("verifyCode",code);
            dic.Add("name",name);
            var postData = WebRequestCtrl.BuildPostDatas(dic, Encoding.UTF8);
            
            var htmlString = base.GetWebContent("http://gsxt.zjaic.gov.cn/search/doValidatorVerifyCode.do", postData,
                ref _cookies, "http://gsxt.zjaic.gov.cn/search/doEnGeneralQueryPage.do",true);

            var value = Regex.Match(htmlString, "(?<=\"message\":\").*?(?=\")").Value;
            return value.Equals("true");

        }

        /// <summary>
        /// 循环直到验证码通过
        /// </summary>
        /// <param name="name"></param>
        private void VerificationCodeLoop(string name)
        {
            var result = false;
            UserCodeInfo userCodeInfo = null;

            while (result == false)
            {
                var imageBytes = GetImageBytes("http://gsxt.zjaic.gov.cn/common/captcha/doReadKaptcha.do");

                UserCodeParserCreator userCodeParserCreator = new UserCodeParserCreator();
                IUserCodeOnlineParser parser = userCodeParserCreator.CreateUserCodeParser(@"yundaiusercode", null);
                
                userCodeInfo = parser.GetUserCodeEx("", "", imageBytes);
                
                //UserCode.UserCodeParser userCodeParser = new UserCodeParser();
                //var code = userCodeParser.GetUserCode("", "", imageBytes);

                //验证
                result = VerificationCode(userCodeInfo.UserCode, name);

                if (result)
                {
                    //报成功 
                    parser.ReportSuccess(userCodeInfo);
                }
                else
                {
                    //报失败
                    parser.ReportError(userCodeInfo);
                }

               
            }

            

            //GetCorpid
            GetCorpid(userCodeInfo.UserCode,name);

        }

        

        /// <summary>
        /// GetCorpid
        /// </summary>
        /// <param name="code"></param>
        /// <param name="name"></param>
        private void GetCorpid(string code,string name)
        {
            IDictionary<string,string> dic = new Dictionary<string, string>();
            dic.Add("clickType","1");
            dic.Add("verifyCode",code);
            dic.Add("name",name);

            var postData = WebRequestCtrl.BuildPostDatas(dic, Encoding.UTF8);

            var htmlString = base.GetWebContent("http://gsxt.zjaic.gov.cn/search/doGetAppSearchResult.do", postData,
                ref _cookies, "http://gsxt.zjaic.gov.cn/search/doEnGeneralQueryPage.do", false);

             
             _corpid = Regex.Match(htmlString, @"(?<=href=""/appbasicinfo/doViewAppBasicInfoByLog\.do\?corpid=).*(?="")").Value;

        }

        /// <summary>
        /// 工商公示信息-登记信息
        /// </summary>
        /// <returns></returns>
        private Dictionary<string,string> GetAppBasicInfo()
        {
            var title = "工商公示信息-登记信息";
            var url = $"http://gsxt.zjaic.gov.cn/appbasicinfo/doReadAppBasicInfo.do?corpid={_corpid}";
            var refer = $"http://gsxt.zjaic.gov.cn/appbasicinfo/doViewAppBasicInfo.do?corpid={_corpid}";
            var htmlString = base.GetWebContent(url,null,ref _cookies,refer);
            _regNo = Regex.Match(htmlString, @"(?<=注册号：)[\S]*").Value;
            _uniSCID =
                Regex.Match(htmlString, @"(?<=统一社会信用代码/注册号</th>[\s]*<td width=""30%"">[\s]*)[\S]*?(?=[\s]*</td>)").Value;
            return new Dictionary<string, string>
            {
                ["title"] = title,
                ["url"] = url,
                ["content"] = htmlString

            };
        }

        /// <summary>
        /// 工商公示信息-备案信息
        /// </summary>
        /// <returns></returns>
        private Dictionary<string, string> GetFilingInfo()
        {
            var title = "工商公示信息-备案信息";
            var url = $"http://gsxt.zjaic.gov.cn/filinginfo/doViewFilingInfo.do?corpid={_corpid}";
            var htmlString = base.GetWebContent(url, null, ref _cookies);

            return new Dictionary<string, string>
            {
                ["title"] = title,
                ["url"] = url,
                ["content"] = htmlString

            };
        }

        /// <summary>
        /// 工商公示信息-动产抵押登记信息
        /// </summary>
        /// <returns></returns>
        private Dictionary<string,string> GetDcdyApplyinfoList()
        {
            var title = "工商公示信息-动产抵押登记信息";
            var url = $"http://gsxt.zjaic.gov.cn/dcdyapplyinfo/doReadDcdyApplyinfoList.do?regNo={_regNo}&uniSCID={_uniSCID}";
            var htmlString = base.GetWebContent(url,null,ref _cookies);
            return new Dictionary<string, string>
            {
                ["title"] = title,
                ["url"] = url,
                ["content"] = htmlString

            };
        }


        /// <summary>
        /// 工商公示信息-股权出质登记信息
        /// </summary>
        /// <returns></returns>
        private Dictionary<string,string> GetEquityAllListFromPV()
        {
            var title = "工商公示信息-股权出质登记信息";
            var url = $"http://gsxt.zjaic.gov.cn/equityall/doReadEquityAllListFromPV.do?corpid={_corpid}";
            var htmlString = base.GetWebContent(url,null,ref _cookies);
            return new Dictionary<string, string>
            {
                ["title"] = title,
                ["url"] = url,
                ["content"] = htmlString

            };
        }

        /// <summary>
        /// 工商公示信息-行政处罚信息
        /// </summary>
        /// <returns></returns>
        private Dictionary<string,string> GetPunishmentFromPV()
        {
            var title = "工商公示信息-行政处罚信息";
            var url = $"http://gsxt.zjaic.gov.cn/punishment/doViewPunishmentFromPV.do?corpid={_corpid}";
            var htmlString = base.GetWebContent(url,null,ref _cookies);

            return new Dictionary<string, string>
            {
                ["title"] = title,
                ["url"] = url,
                ["content"] = htmlString

            };
        }

        /// <summary>
        /// 工商公示信息-经营异常信息
        /// </summary>
        /// <returns></returns>
        private Dictionary<string,string> GetCatalogApplyList()
        {
            var title = "工商公示信息-经营异常信息";
            var url = $"http://gsxt.zjaic.gov.cn/catalogapply/doReadCatalogApplyList.do?corpid={_corpid}";
            var htmlString = base.GetWebContent(url, null,ref _cookies);

            return new Dictionary<string, string>
            {
                ["title"] = title,
                ["url"] = url,
                ["content"] = htmlString

            };
        }

        /// <summary>
        /// 工商公示信息-严重违法信息
        /// </summary>
        /// <returns></returns>
        private Dictionary<string,string> GetBlackListInfo()
        {
            var title = "工商公示信息-严重违法信息";
            var url = $"http://gsxt.zjaic.gov.cn/blacklist/doViewBlackListInfo.do?corpid={_corpid}";
            var htmlString = base.GetWebContent(url,null,ref _cookies);

            return new Dictionary<string, string>
            {
                ["title"] = title,
                ["url"] = url,
                ["content"] = htmlString

            };
        }


        /// <summary>
        /// 工商公示信息-抽查检测信息
        /// </summary>
        /// <returns></returns>
        private Dictionary<string,string> GetPubCheckResultList()
        {
            var title = "工商公示信息-抽查检测信息";
            var url = $"http://gsxt.zjaic.gov.cn/pubcheckresult/doViewPubCheckResultList.do?corpid={_corpid}";
            var htmlString = base.GetWebContent(url,null,ref _cookies);

            return new Dictionary<string, string>
            {
                ["title"] = title,
                ["url"] = url,
                ["content"] = htmlString

            };
        }

        /// <summary>
        /// 企业公示信息-企业年报
        /// </summary>
        /// <returns></returns>
        private Dictionary<string,string> GetPubReportInfoList()
        {
            var title = "企业公示信息-企业年报";
            var url = $"http://gsxt.zjaic.gov.cn/pubreportinfo/doReadPubReportInfoList.do?corpid={_corpid}&appConEntTypeCatg=11";
            var htmlString = base.GetWebContent(url,null,ref _cookies);

            return new Dictionary<string, string>
            {
                ["title"] = title,
                ["url"] = url,
                ["content"] = htmlString

            };
        }


        /// <summary>
        /// 企业公示信息-股东及出资信息
        /// </summary>
        /// <returns></returns>
        private Dictionary<string,string> GetPubFunded()
        {
            var title = "企业公示信息-股东及出资信息";
            var url = $"http://gsxt.zjaic.gov.cn/pubfunded/doReadPubFunded.do?pubFunded.corpid={_corpid}";
            var htmlString = base.GetWebContent(url, null,ref _cookies);
            return new Dictionary<string, string>
            {
                ["title"] = title,
                ["url"] = url,
                ["content"] = htmlString

            };
        }

        /// <summary>
        /// 企业公示信息-股权变更信息
        /// </summary>
        /// <returns></returns>
        private Dictionary<string,string> GetPubStock()
        {
            var title = "企业公示信息-股权变更信息";
            var url = $"http://gsxt.zjaic.gov.cn/pubinstantstock/doReadPubStock.do?pubInstantStock.corpid={_corpid}";
            var htmlString = base.GetWebContent(url,null,ref _cookies);
            return new Dictionary<string, string>
            {
                ["title"] = title,
                ["url"] = url,
                ["content"] = htmlString

            };

        }

        /// <summary>
        /// 企业公示信息-行政许可信息
        /// </summary>
        /// <returns></returns>
        private Dictionary<string, string> GetPubLicense()
        {
            var title = "企业公示信息-行政许可信息";
            var url = $"http://gsxt.zjaic.gov.cn/publicense/doReadPubLicense.do?pubLicense.corpid={_corpid}";
            var htmlString = base.GetWebContent(url, null,ref _cookies);

            return new Dictionary<string, string>
            {
                ["title"] = title,
                ["url"] = url,
                ["content"] = htmlString

            };
        }

        /// <summary>
        /// 企业公示信息-知识产权出质登记信息
        /// </summary>
        /// <returns></returns>
        private Dictionary<string,string> GetPubInstantIntellectual()
        {
            var title = "企业公示信息-知识产权出质登记信息";
            var url = $"http://gsxt.zjaic.gov.cn/pubinstantintellectual/doReadPubInstantIntellectual.do?pubInstantIntellectual.corpid={_corpid}";
            var htmlString = base.GetWebContent(url,null,ref _cookies);

            return new Dictionary<string, string>
            {
                ["title"] = title,
                ["url"] = url,
                ["content"] = htmlString

            };
        }

        /// <summary>
        /// 企业公示信息-行政处罚信息
        /// </summary>
        /// <returns></returns>
        private Dictionary<string,string> GetPubInstantPunish()
        {
            var title = "企业公示信息-行政处罚信息";
            var url = $"http://gsxt.zjaic.gov.cn/pubinstantpunish/doReadPubInstantPunish.do?pubInstantPunish.corpid={_corpid}";
            var htmlString = base.GetWebContent(url,null,ref _cookies);
            return new Dictionary<string, string>
            {
                ["title"] = title,
                ["url"] = url,
                ["content"] = htmlString

            };
        }

        /// <summary>
        /// 其他部门公示信息-行政许可及变更信息
        /// </summary>
        /// <returns></returns>
        private Dictionary<string,string> GetPubOtherLicenceInfo()
        {
            var title = "其他部门公示信息-行政许可及变更信息";
            var url = $"http://gsxt.zjaic.gov.cn/pubotherlicence/readPubOtherLicenceInfo.do?corpid={_corpid}";
            var htmlString = base.GetWebContent(url,null,ref _cookies);

            return new Dictionary<string, string>
            {
                ["title"] = title,
                ["url"] = url,
                ["content"] = htmlString

            };
        }

        /// <summary>
        /// 其他部分公示信息-行政处罚信息
        /// </summary>
        /// <returns></returns>
        private Dictionary<string,string> GetPubOtherPunishInfo()
        {
            var title = "其他部分公示信息-行政处罚信息";
            var url = $"http://gsxt.zjaic.gov.cn/pubotherpunish/readPubOtherPunishInfo.do?corpid={_corpid}";
            var htmlString = base.GetWebContent(url,null,ref _cookies);
            return new Dictionary<string, string>
            {
                ["title"] = title,
                ["url"] = url,
                ["content"] = htmlString

            };
        }


        /// <summary>
        /// 司法协助公示信息-股权冻结信息
        /// </summary>
        /// <returns></returns>
        private Dictionary<string,string> GetFrozJusticeInfo()
        {
            var title = "司法协助公示信息-股权冻结信息";
            var url = $"http://gsxt.zjaic.gov.cn/pubjusticeinfo/doReadFrozJusticeInfo.do?corpid={_corpid}&justiceInfoType=1&justiceAuditResult=1";
            var htmlString = base.GetWebContent(url, null,ref _cookies);

            return new Dictionary<string, string>
            {
                ["title"] = title,
                ["url"] = url,
                ["content"] = htmlString

            };
        }

        /// <summary>
        /// 司法协助公示信息-股东强制变更信息
        /// </summary>
        /// <returns></returns>
        private Dictionary<string,string> GetThawJusticeInfo()
        {
            var title = "司法协助公示信息-股东强制变更信息";
            var url = $"http://gsxt.zjaic.gov.cn/pubjusticeinfo/doReadThawJusticeInfo.do?corpid={_corpid}&justiceInfoType=2&justiceAuditResult=1";
            var htmlString = base.GetWebContent(url,null,ref _cookies);

            return new Dictionary<string, string>
            {
                ["title"] = title,
                ["url"] = url,
                ["content"] = htmlString

            };
        }




    }






    /// <summary>
    /// 验证码解析接口
    /// </summary>
    public class UserCodeParserCreator : IUserCodeParserCreator
    {
        /// <summary>
        /// IUserCodeOnlineParser
        /// </summary>
        /// <param name="parserName"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public IUserCodeOnlineParser CreateUserCodeParser(string parserName, IDictionary<string, string> param)
        {
            if (parserName == "yundaiusercode")
                return new UserCodeParser();
            throw new NotImplementedException();
        }
    }





}
