using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using MySql.Data.MySqlClient;
using Newtonsoft.Json.Linq;

namespace XFCollection.gongshang
{
    class ZhejiangParser
    {
        /// <summary>
        /// Parses this instance.
        /// </summary>
        private void Parse()
        {
            try
            {
                var dbServer = @"211.149.250.122";
                var dbName = @"getemptj";
                var dbUsername = @"asst2";
                var dbPassword = @"fx*#sjTx";

                //向ge_engine_object表插入新记录
                string conn = $"server={dbServer};database={dbName};Uid={dbUsername};Pwd={dbPassword}";
                using (MySqlConnection myConn = new MySqlConnection(conn))
                {

                    myConn.Open();
                    
                    string str = "SELECT Corpid,UniSCID,CompanyName,Contents FROM yuanshi_gongshang_zhejiang_attr_debug";

                    MySqlCommand myComm = new MySqlCommand(str, myConn);

                    //默认30s 设置为0 不限制时间
                    myComm.CommandTimeout = 0;


                    MySqlDataReader myReader = myComm.ExecuteReader();

                    while (myReader.Read())
                    {
                        Console.WriteLine(myReader.GetString(0) + "," + myReader.GetString(1) + "," + myReader.GetString(2));

                        var Corpid = myReader.GetString(0);
                        if (string.IsNullOrEmpty(Corpid))
                            continue;
                        var UniSCID = myReader.GetString(1);
                        var company = myReader.GetString(2);
                        var contents = myReader.GetString(3);

                        var jArray = JArray.Parse(contents);

                        var content = jArray[0]["content"].ToString();
                        var setUpTime = Regex.Match(content, @"(?<=成立日期</th>[^/]*)\d+年\d+月\d+日(?=</td>)").Value;
                        var location = string.Empty;
                        var registeredCapital = string.Empty;
                        
                        if (!string.IsNullOrEmpty(setUpTime))
                        {
                            location = Regex.Match(content, "(?<=住所</th>[^/]*>)[^/]*(?=</td>)").Value;
                            registeredCapital = Regex.Match(content, "(?<=注册资本</th>[^/]*>)[^/]*(?=</td>)").Value;
                        }
                        else
                        {
                            setUpTime = Regex.Match(content, @"(?<=<th>注册日期</th>[^/]*>)\d+年\d+月\d+日(?=</td>)").Value;
                            location = Regex.Match(content, "(?<=<th>经营场所</th>[^/]*>).*?(?=</td>)").Value;
                        }

                        var matches = Regex.Matches(content, @"(?<=<td style=\""padding-left: 5px;\"">)[\S]*?(?=</td>)");
                        foreach (Match match in matches)
                        {
                            var value = match.Value;
                            if (!string.IsNullOrEmpty(value))
                                registeredCapital = $"{registeredCapital}/{value}";
                        }

                        //找到行政处罚信息
                        var punishmentInformatinContent = jArray[4]["content"].ToString();
                        var punishmentInformatin = Regex.Match(punishmentInformatinContent, "(?<=<td.*>).*(?=</td>)").Value;
                        if(punishmentInformatin.Equals("1"))
                            punishmentInformatin = Regex.Match(punishmentInformatinContent, "(?<=<td style=\"padding-left: 5px;\">).*?(?=</td>)").Value;

                        


                        InsertTable(UniSCID, company, setUpTime, location, registeredCapital, punishmentInformatin);


                    }



                }


            }
            catch (Exception)
            {
                //throw new Exception(e.Message);
                throw;
            }
        }

        /// <summary>
        /// Inserts the table.
        /// </summary>
        /// <param name="UniSCID">The uni scid.</param>
        /// <param name="company">The company.</param>
        /// <param name="setUpTime">The set up time.</param>
        /// <param name="location">The location.</param>
        /// <param name="registeredCapital">The registered capital.</param>
        /// <param name="punishmentInformatin">The punishment informatin.</param>
        private void InsertTable(string UniSCID, string company, string setUpTime, string location, string registeredCapital, string punishmentInformatin)
        {
            try
            {
                var dbServer = @"127.0.0.1";
                var dbName = @"zhanxian";
                var dbUsername = @"root";
                var dbPassword = @"root";

                //向ge_engine_object表插入新记录
                string conn = $"server={dbServer};database={dbName};Uid={dbUsername};Pwd={dbPassword}";
                using (MySqlConnection myConn = new MySqlConnection(conn))
                {
                    myConn.Open();
                    string str = "insert into jd(UniSCID,company,setUpTime,location,registeredCapital,punishmentInformatin) values(@UniSCID,@company,@setUpTime,@location,@registeredCapital,@punishmentInformatin)";


                    MySqlCommand myComm = new MySqlCommand(str, myConn);

                    //参数
                    myComm.Parameters.AddWithValue("@UniSCID", UniSCID);
                    myComm.Parameters.AddWithValue("@company", company);
                    myComm.Parameters.AddWithValue("@setUpTime", setUpTime);
                    myComm.Parameters.AddWithValue("@location", location);
                    myComm.Parameters.AddWithValue("@registeredCapital", registeredCapital);
                    myComm.Parameters.AddWithValue("@punishmentInformatin", punishmentInformatin);


                    switch (myComm.ExecuteNonQuery())
                    {
                        case 1:
                            Console.WriteLine(@"插入jd表成功！");
                            break;
                        case 0:
                            throw new Exception(@"更新jd表失败！");
                        default:
                            throw new Exception(@"更新jd表的多条新记录！");
                    }

                }


            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }

        }


    }


}
