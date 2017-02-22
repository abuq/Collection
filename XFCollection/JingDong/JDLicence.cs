using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using System.Windows.Forms;
using CsvHelper;
using X.CommLib.Ctrl.UserCodes;
using X.GlodEyes.Collectors;
using X.GlodEyes.Collectors.Specialized.JingDong;

namespace XFCollection.JingDong
{
    /// <summary>
    /// 京东营业执照
    /// </summary>
    public class JDLicence : WebRequestCollector<IResut, NormalParameter>
    {

        internal static void Test()
        {
            var parameter = new NormalParameter { Keyword = @"173940" };
            TestHelp<JDLicence>(parameter, 10);

            
        }

        /// <summary>
        /// shopId列表
        /// </summary>
        public List<string> shopIdList;

        /// <summary>
        ///     得到京东营业执照信息
        /// </summary>
        public void GetLicenceInfo()
        {
            

            while (shopIdList.Count > 0)
            {
                try
                {
                    string shopId;

                    lock (shopIdList)
                    {
                        shopId = shopIdList[0];
                        Console.WriteLine(shopId);
                        shopIdList.Remove(shopId);
                    }

                    var parameter = new NormalParameter {Keyword = shopId};
                    var licence = new JDLicence();
                    licence.Init(parameter);

                    licence.MoveNext();
                    var current = licence.Current;

                    var resut = current[0];
                    var value = new Dictionary<string, string>();
                    foreach (var item in resut)
                    {
                        value[item.Key] = $"{item.Value}";
                    }

                    ExportToCSV(value);

                    
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }


            }
        }

        


        /// <summary>
        /// 初始化链接
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        protected override string InitFirstUrl(NormalParameter param)
        {
            var shopId = param.Keyword;
            if (shopId.Equals(string.Empty))
                throw new Exception("传入的ShopId为空，请检查！");
            return $"http://shop.m.jd.com/detail/licenceDetail?shopId={shopId}";
        }

        /// <summary>
        /// 解析当前元素
        /// </summary>
        /// <returns></returns>
        protected override IResut[] ParseCurrentItems()
        {
            

            var licenceDic = GetLicenceDic(HtmlSource);

            var resut = new Resut();
            foreach (var licence in licenceDic)
            {
                resut.Add(licence.Key, licence.Value);
            }
            resut.Add("Url",CurrentUrl);
            return new IResut[] {resut};


           
        }

        /// <summary>
        /// 解析下一个链接
        /// </summary>
        /// <returns></returns>
        protected override string ParseNextUrl()
        {
            return null;
        }

        /// <summary>
        /// 解析当前页数
        /// </summary>
        /// <returns></returns>
        protected override int ParseCurrentPage()
        {
            return 1;
        }

        /// <summary>
        /// 解析总页数
        /// </summary>
        /// <returns></returns>
        protected override int ParseCountPage()
        {
            return 1;
        }

        /// <summary>
        /// 得到执照信息字典
        /// </summary>
        /// <returns></returns>
        private Dictionary<string, string> GetLicenceDic(string htmlString)
        {
            Dictionary<string, string> licenceDic = new Dictionary<string, string>();
            var content = Regex.Match(htmlString, @"(?<=<div class=""licence-bd"">)[\s\S]*?(?=</div>)").Value;
            //var matchs = Regex.Match(content, @"(?<key>[\S]*?)：(?<value>[^<\s]*)");
            var matchs = Regex.Match(content, @" (?<key>\w+)：(?<value>[^<\r\n]+)"); 
            while (matchs.Success)
            {
                licenceDic.Add(matchs.Groups["key"].Value, matchs.Groups["value"].Value);
                matchs = matchs.NextMatch();
            }

            return licenceDic;
        }

        /// <summary>
        ///     导出到CSV格式
        /// </summary>
        private void ExportToCSV(IDictionary<string, string> dic)
        {
            var sw = csvWriter;
            lock (sw)
            {

                foreach (var item in dic)
                {
                    sw.WriteField(item.Value);
                }

                sw.NextRecord();

                /*var count = dic.Count;
                var curCount = 0;
                foreach (var keyValue in dic)
                {
                    /*if (++curCount != count)
                    {
                        sw.Write($"{keyValue.Value}");
                        sw.Write(",");
                    }
                    else
                    {
                        sw.Write($"{keyValue.Value}");
                        sw.WriteLine();
                    }#1#
                }*/
            }
        }

        ///// <summary>  
        ///// 导出报表为Csv  
        ///// </summary>  
        ///// <param name="dt">DataTable</param>  
        ///// <param name="strFilePath">物理路径</param>  
        ///// <param name="tableheader">表头</param>  
        ///// <param name="columname">字段标题,逗号分隔</param>  
        //public static string dt2csv(DataTable dt, string strFilePath, string tableheader, string columname)
        //{
        //    try
        //    {
        //        string strBufferLine = "";
        //        StreamWriter strmWriterObj = new StreamWriter(strFilePath, false, System.Text.Encoding.UTF8);
        //        //strmWriterObj.WriteLine(tableheader);  
        //        strmWriterObj.WriteLine(columname);
        //        for (int i = 0; i < dt.Rows.Count; i++)
        //        {
        //            strBufferLine = "";
        //            for (int j = 0; j < dt.Columns.Count; j++)
        //            {
        //                if (j > 0)
        //                    strBufferLine += ",";
        //                strBufferLine += dt.Rows[i][j].ToString();
        //            }
        //            strmWriterObj.WriteLine(strBufferLine);
        //        }
        //        strmWriterObj.Close();
        //        return "备份成功";
        //    }
        //    catch (Exception ex)
        //    {
        //        return "备份失败 " + ex.ToString();
        //    }
        //}

        /// <summary>
        /// run多少个线程
        /// </summary>
        /// <param name="threadNum"></param>
        public void Run(int threadNum = 10)
        {

            OpenFile();

            try
            {
                Thread[] threads = new Thread[threadNum];
                for (int i = 0; i < threadNum; i++)
                {
                    Thread t = new Thread(new ThreadStart(GetLicenceInfo));
                    t.IsBackground = true;
                    t.Start();
                    threads[i] = t;
                }

                Array.ForEach(threads, thread => thread.Join());
            }
            finally
            {
                CloseFile();
            }

        }

        /// <summary>
        /// 打开文件
        /// </summary>
        private void OpenFile()
        {
            var path = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            var name = "Lience.csv";
            path = Path.Combine(path, name);
            var append = File.Exists(path);
            /*var mode = FileMode.Create;
            if (File.Exists(path))
                mode = FileMode.Append;*/

            TextWriter writer = new StreamWriter(path, append, Encoding.Default );
            /*var fileStream = new FileStream(path, mode, FileAccess.Write);*/
            /*this.streamWriter = new StreamWriter(fileStream, Encoding.Default);*/
            this.csvWriter = new CsvWriter(writer);

           /* if (!append)
            {
                csvWriter.WriteField("qkq");
                csvWriter.WriteField("afsf");
                csvWriter.WriteField("aasf");
                csvWriter.NextRecord();
            }*/
        }

        /// <summary>
        /// 关闭文件
        /// </summary>
        private void CloseFile()
        {   
            csvWriter?.Dispose();
        }


        private CsvWriter csvWriter;
    }


   /// <summary>
   /// 测试
   /// </summary>
   public class Test
    {
        ///// <summary>
        ///// 多线程运行
        ///// </summary>
        //public static void Main(string[] args)
        //{
        //    var shopIds = File.ReadAllLines(@"C:\Users\Administrator\Desktop\last.txt");
        //    // 去掉字符串前后的"
        //    //shopIds = Array.ConvertAll(shopIds, shopId => shopId.Trim('"'));
        //    JDLicence jdLicence = new JDLicence();
        //    jdLicence.shopIdList = shopIds.ToList();

        //    jdLicence.Run(100);







        //    //while (true)
        //    //{
        //    //    bool isAlive = false;

        //    //    for (int i = 0; i < threadNum; i++)
        //    //    {
        //    //        if (threads[i].IsAlive)
        //    //        {
        //    //            isAlive = true;
        //    //            break;
        //    //        }
        //    //    }

        //    //    if (!isAlive)
        //    //    {
        //    //        break;
        //    //    }

        //    //}


        //    //while (true)
        //    //{
        //    //    if (Array.TrueForAll(threads, thread => !thread.IsAlive))
        //    //    {
        //    //        break;
        //    //    }

        //    //    Thread.Sleep(200);
        //    //}








        //}
    }
}

