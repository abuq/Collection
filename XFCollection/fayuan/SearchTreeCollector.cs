using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.UI.WebControls;


namespace XFCollection.fayuan
{
    internal class SearchTreeCollector
    {



        //private string caseType = string.Empty;
        //private Dictionary<string,string> rootDic = new Dictionary<string, string>();
        /// <summary>5
        /// 测试
        /// </summary>
        public void Test()
        {
            //GetReasonTree("刑事案件", "一级案由", "刑事案由");
            //GetReasonTree("民事案件", "一级案由", "民事案由");
            //GetReasonTree("行政案件", "一级案由", "行政案由");
            //GetReasonTree("赔偿案件", "一级案由", "赔偿案由");
            //GetReasonTree("执行案件", "一级案由", "执行案由");





            List<string> courtList = new List<string>
            {
              "北京市","天津市","河北省","山西省","内蒙古自治区","辽宁省","吉林省","黑龙江省","上海市","江苏省","浙江省",
              "安徽省","福建省","江西省","山东省","河南省","湖北省","湖南省","广东省","广西壮族自治区","海南省","重庆市",
              "四川省","贵州省","云南省","西藏自治区","陕西省","甘肃省","青海省","宁夏回族自治区","新疆维吾尔自治区","新疆维吾尔自治区高级人民法院生产建设兵团分院"
            };





            //GetCourtTree("民事案件", "法院地域", "浙江省");
            //GetCourtTree1("民事案件", "法院地域", "浙江省");

            foreach (var court in courtList)
            {
                GetCourtTree1("刑事案件", "法院地域", court);
            }
            foreach (var court in courtList)
            {
                GetCourtTree1("民事案件", "法院地域", court);
            }
            foreach (var court in courtList)
            {
                GetCourtTree1("行政案件", "法院地域", court);
            }
            foreach (var court in courtList)
            {
                GetCourtTree1("赔偿案件", "法院地域", court);
            }
            foreach (var court in courtList)
            {
                GetCourtTree1("执行案件", "法院地域", court);
            }


        }

        public void RemoveShopName()
        {
            var contentText = File.ReadAllText(@"C:\Users\Administrator\Desktop\去掉结尾专用店.txt");
            var contents = Regex.Matches(contentText, @"[\S].*(?=\r\n)");
            List<string> removeList = new List<string>{"官方旗舰店", "旗舰店", "专营店", "专营", "体验馆", "专卖店" };
            foreach (Match content in contents)
            {
                var value = content.Value;
                var count = 0;
                foreach (var remove in removeList)
                {

                    

                    var valueTemp = value.Replace(remove, "");

                    
                    if (!valueTemp.Equals(value))
                    {
                        Console.WriteLine(valueTemp);
                        break;
                    }


                    if (++count == 6)
                    {
                        Console.WriteLine(value);
                    }


                }
            }
        }

        /// <summary>
        /// 得到高院数据参数
        /// </summary>
        public void GetHighLevelParam()
        {
            var contentText = File.ReadAllText(@"C:\Users\Administrator\Desktop\法院数据\按地域及法院筛选节点带数量全.txt");
            var contents = Regex.Matches(contentText, @".*(\r\n|$)");
            foreach (Match content in contents)
            {
                var value = content.Value;
                var reason = Regex.Match(value, @"[\S]*案件(?= #)").Value;
                var court = Regex.Match(value, @"(?<=# ).*(?=\()").Value;
                
                
                Console.WriteLine($"insert into renwu_judgementdoclist_daily(reason,court) values(\"{reason}\",\"{court}\");");
                
            }
        }


        /// <summary>
        /// 得到低中高所有法院的参数
        /// </summary>
        public void GetAllParam()
        {
            var contentText = File.ReadAllText(@"C:\Users\Administrator\Desktop\高级法院.txt");
            var contents = Regex.Matches(contentText, @".*\)");
            foreach (Match content in contents)
            {
                var value = content.Value;
                var num = Regex.Match(value, @"(?<=\().*(?=\))").Value;
                var reason = Regex.Match(value, "^.*?(?= )").Value;
                var court = Regex.Match(value, @"(?<=^.*#[\s]+).*(?=\()").Value;
                if (int.Parse(num) > 4000)
                {
                    for (var year = 1996; year < 2017; year++)
                        Console.WriteLine($"insert into targetTable(reason,court,year) values(\"{reason}\",\"{court}\",\"{year}\");");
                }
                else
                {
                    Console.WriteLine($"insert into targetTable(reason,court) values(\"{reason}\",\"{court}\");");
                }
            }
        }



        /// <summary>
        /// 得到高院数据
        /// </summary>
        public void GetHighLevel()
        {
            var contentArray = File.ReadAllText(@"C:\Users\Administrator\Desktop\按地域及法院筛选节点带数量全.txt");

            var contents = Regex.Matches(contentArray, @" .*法院地域\(\d+\)[\s\S]*?(?= .*法院地域\(\d+\)|$)");

            foreach (Match content in contents)
            {
                var value = content.Value;
                var result = int.Parse(Regex.Match(value, @"(?<=法院地域\()\d+(?=\))").Value);

                var nums = Regex.Matches(value, @"(?<=中级法院\()\d+(?=\))");
                foreach (Match num in nums)
                {
                    result -= int.Parse(num.Value);
                }

                var highLevel = Regex.Match(content.Value, ".*法院地域").Value;
                highLevel += $@"({result})";

                Console.WriteLine(highLevel);
            }

        }

        /// <summary>
        /// 得到中级法院
        /// </summary>
        public void GetMiddleLevel()
        {
            var contentArray = File.ReadAllText(@"C:\Users\Administrator\Desktop\按地域及法院筛选节点带数量略全.txt");
            
  
            var contents = Regex.Matches(contentArray, @"  .*中级法院\(\d+\)[\s\S]*?(?=  .*中级法院\(\d+\)|$)");

            //Console.WriteLine(contentArray.ToString());


            foreach (Match content in contents)
            {

                var nums =  Regex.Matches(content.Value, @"\d+");

                int result = 0;
                int times = 0;
                foreach (Match num in nums)
                {
                    int value = int.Parse(num.Value);
                    if (++times == 1)
                    {
                        result = value;
                    }
                    else
                    {
                        ++times;
                        result -= value;
                    }
                }

                var middleLevel = Regex.Match(content.Value, ".*中级法院").Value;
                middleLevel += $@"({result})";

                Console.WriteLine(middleLevel);

            }


        }


        /// <summary>
        /// 得到地域搜索树
        /// </summary>
        /// <param name="caseType"></param>
        /// <param name="level"></param>
        /// <param name="key"></param>
        /// <param name="span"></param>
        public void GetCourtTree(string caseType, string level, string key, int span = 0)
        {

            Console.WriteLine($"{new string(' ', span * 2)} {caseType} # {key} # {level}");

            var htmlString = GetHtmlFromPost(@"http://wenshu.court.gov.cn/List/CourtTreeContent", Encoding.UTF8, $"Param=案件类型:{caseType},{level}:{key}&parval={key}");
            if (htmlString.Equals(string.Empty))
                Console.WriteLine($"{caseType} # {key} # {level}(自己查)");
            
            const string shielded = "\"remind\"";
            if (htmlString.Equals(shielded))
            {
                Console.WriteLine(@"解析验证码中...................");
                bool isSuccess = false;
                int i = 1;
                while (isSuccess == false)
                {
                    Console.WriteLine($"第{i}次解析验证码");
                    TesseractDemo tesseract = new TesseractDemo();
                    if ((isSuccess = tesseract.HandleValidateCode()) == false)
                        i++;
                }
                GetCourtTree(caseType, level, key, span);
                return;
            }

            var dics = GetSearchTreeKey(htmlString);
            if (dics.Count == 0)
            {


                var postData = $"Param=案件类型:{caseType},{level}:{key}&Index=1&Page=20&Order=裁判日期&Direction=asc";
                const string url = "http://wenshu.court.gov.cn/List/ListContent";
                var htmlStringContent = GetHtmlFromPost(url, Encoding.UTF8, postData);
                if (htmlStringContent.Equals(shielded))
                {
                    Console.WriteLine(@"解析验证码中...................");
                    bool isSuccess = false;
                    int i = 1;
                    while (isSuccess == false)
                    {
                        Console.WriteLine($"第{i}次解析验证码");
                        TesseractDemo tesseract = new TesseractDemo();
                        if ((isSuccess = tesseract.HandleValidateCode()) == false)
                            i++;
                    }
                    GetCourtTree(caseType, level, key, span);
                    return;
                }

                var num = GetTotalPages(htmlStringContent);
                //Console.WriteLine($"{new string(' ', span * 2)} {caseType} # {key} # {level}({num})");
                return;
            }

            
            foreach (var dic in dics)
            {
                GetCourtTree(caseType, dic.Value, dic.Key, span + 1);
            }
        }



        /// <summary>
        /// 得到地域搜索树1
        /// </summary>
        /// <param name="caseType"></param>
        /// <param name="level"></param>
        /// <param name="key"></param>
        /// <param name="span"></param>
        public void GetCourtTree1(string caseType, string level, string key, int span = 0)
        {

            //Console.WriteLine($"{new string(' ', span * 2)} {caseType} # {key} # {level}");
            var htmlString = GetHtmlFromPost(@"http://wenshu.court.gov.cn/List/CourtTreeContent", Encoding.UTF8, $"Param=案件类型:{caseType},{level}:{key}&parval={key}");
            if (htmlString.Equals(string.Empty))
                Console.WriteLine($"{caseType} # {key} # {level}(自己查)");

            if (CheckUserCode(htmlString))
            {
                GetCourtTree1(caseType, level, key, span);
                return;
            }
            
            var dics = GetSearchTreeKey(htmlString);

            var postData = $"Param=案件类型:{caseType},{level}:{key}&Index=1&Page=20&Order=裁判日期&Direction=asc";
            const string url = "http://wenshu.court.gov.cn/List/ListContent";
            var htmlStringContent = GetHtmlFromPost(url, Encoding.UTF8, postData);
            if (CheckUserCode(htmlStringContent))
            {
                GetCourtTree1(caseType, level, key, span);
                return;
            }
            
            var num = GetTotalPages(htmlStringContent);
            Console.WriteLine($"{new string(' ', span * 2)} {caseType} # {key} # {level}({num})");

            if (dics.Count == 0)
            {
                return;
            }

            

            foreach (var dic in dics)
            {
                GetCourtTree1(caseType, dic.Value, dic.Key, span + 1);
            }
        }


        public bool CheckUserCode(string htmlStringContent)
        {
            const string shielded = "\"remind\"";

            if (htmlStringContent.Equals(shielded))
            {
                Console.WriteLine(@"解析验证码中...................");
                bool isSuccess = false;
                int i = 1;
                while (isSuccess == false)
                {
                    Console.WriteLine($"第{i}次解析验证码");
                    TesseractDemo tesseract = new TesseractDemo();
                    if ((isSuccess = tesseract.HandleValidateCode()) == false)
                        i++;
                }
                /*GetCourtTree1(caseType, level, key, span);
                return;*/
                return true;
            }

            return false;
        } 
             



        /// <summary>
        /// 得到总共的页数
        /// </summary>
        /// <param name="htmlString">网页内容</param>
        /// <returns></returns>
        public static int GetTotalPages(string htmlString)
        {
            int total;
            if (htmlString.Equals("reload")|| htmlString.Equals("\"reload\"")) return -1;
            try
            {
                var match = Regex.Match(htmlString, @"(?<=\\""Count\\"":\\"")\d*(?=\\"")");
                //Console.WriteLine($"总共页数：{match.Value}页");
                total = int.Parse(match.Value);
            }
            catch (Exception)
            {
                //reload
                throw new Exception(htmlString);
            }
            return total;
        }


        /// <summary>
        /// 得到案由搜索树
        /// </summary>
        /// <param name="caseType"></param>
        /// <param name="level"></param>
        /// <param name="key"></param>
        /// <param name="span"></param>
        public void GetReasonTree(string caseType,string level,string key, int span = 0)
        {
            //Console.WriteLine($"{new string(' ', span * 2)} {caseType} # {level} # {key}");

            var htmlString = GetHtmlFromPost(@"http://wenshu.court.gov.cn/List/ReasonTreeContent", Encoding.UTF8,$"Param=案件类型:{caseType},{level}:{key}&parval={key}");
            if(htmlString.Equals(string.Empty))
                Console.WriteLine($"{caseType} # {key} # {level}(自己查)");
            const string shielded = "\"remind\"";
            if (htmlString.Equals(shielded))
            {
                Console.WriteLine(@"解析验证码中...................");
                bool isSuccess = false;
                int i = 1;
                while (isSuccess == false)
                {
                    Console.WriteLine($"第{i}次解析验证码");
                    TesseractDemo tesseract = new TesseractDemo();
                    if ((isSuccess = tesseract.HandleValidateCode()) == false)
                        i++;
                }
                GetReasonTree(caseType, level, key, span);
                return;
            }

            var dics = GetSearchTreeKey(htmlString);
            if (dics.Count == 0)
            {
                var postData = $"Param=案件类型:{caseType},{level}:{key}&Index=1&Page=20&Order=裁判日期&Direction=asc";
                const string url = "http://wenshu.court.gov.cn/List/ListContent";
                var htmlStringContent = GetHtmlFromPost(url, Encoding.UTF8, postData);
                if (htmlStringContent.Equals(shielded))
                {
                    Console.WriteLine(@"解析验证码中...................");
                    bool isSuccess = false;
                    int i = 1;
                    while (isSuccess == false)
                    {
                        Console.WriteLine($"第{i}次解析验证码");
                        TesseractDemo tesseract = new TesseractDemo();
                        if ((isSuccess = tesseract.HandleValidateCode()) == false)
                            i++;
                    }
                    GetReasonTree(caseType, level,key,span);
                    return;
                }
                
                var num = GetTotalPages(htmlStringContent);
                Console.WriteLine($"{caseType} # {key} # {level}({num})");
                return;
            }

            foreach (var dic in dics)
            {
                 if (dic.Key.Equals("人事争议")&&dic.Value.Equals("四级案由"))
                 {
                    Console.WriteLine($"{caseType} # {dic.Key} # {dic.Value}(手动查)");
                 }
                 else
                      GetReasonTree(caseType, dic.Value, dic.Key, span + 1);
            }
        }


        //得到搜索树key
        public Dictionary<string,string> GetSearchTreeKey(string htmlString)
        {
            var matchs = Regex.Matches(htmlString, @"(?<=\\""Key\\"":\\"").*?(?=\\"")");

            var isFirst = true;
            var value = string.Empty;
            var dic = new Dictionary<string, string>();
            foreach (Match match in matchs)
            {
                if (isFirst)
                {
                    value = match.Value;
                    if (value.Equals("\"\""))
                    {
                        dic.Add(value,value);
                        return dic;
                    }
                    isFirst = false;

                }
                else
                {
                    var key = match.Value;
                    if (key.Equals(""))
                        continue;
                    else
                        dic.Add(key,value);
                }
            }
            return dic;
        }




        /// <summary>
        /// 提供通过POST方法获取页面的方法
        /// </summary>
        /// <param name="urlString">请求的URL</param>
        /// <param name="encoding">页面使用的编码</param>
        /// <param name="postDataString">POST数据</param>
        /// <returns></returns>
        public string GetHtmlFromPost(string urlString, Encoding encoding, string postDataString)
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
            catch (Exception)
            {
                return GetHtmlFromPost(urlString, encoding, postDataString);
                //throw new Exception("发送POST数据时发生错误！", ex);
            }

            //接受服务器返回信息
            try
            {
                httpWebResponse = httpWebRequest.GetResponse() as HttpWebResponse;
                outputStream = httpWebResponse.GetResponseStream();
                streamReader = new StreamReader(outputStream, encoding);
                if(streamReader.Equals(null)) return GetHtmlFromPost(urlString, encoding, postDataString);
                htmlString = streamReader.ReadToEnd();
                if (htmlString.Equals("reload") || htmlString.Equals("\"reload\"")) return GetHtmlFromPost(urlString, encoding, postDataString);
            }
            //处理异常
            catch (Exception)
            {
                return GetHtmlFromPost(urlString, encoding, postDataString);

            }

            foreach (Cookie cookie in httpWebResponse.Cookies)
            {
                cookieContainer.Add(cookie);
            }

            inputStream.Close();
            outputStream.Close();
            streamReader.Close();

            return htmlString;

        }

    }
}
