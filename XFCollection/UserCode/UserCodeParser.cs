using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using X.CommLib.Ctrl.UserCodes;

namespace XFCollection.UserCode
{
    /// <summary>
    /// UserCodeParser
    /// </summary>
    public class UserCodeParser : IUserCodeOnlineParser
    {

        /// <summary>
        /// 得到验证码
        /// </summary>
        /// <param name="title"></param>
        /// <param name="desc"></param> 
        /// <param name="userCodeData"></param>
        /// <returns></returns>
        public string GetUserCode(string title, string desc, byte[] userCodeData)
        {
            var userCodeInfo = GetUserCodeEx(title, desc, userCodeData);
            return userCodeInfo.UserCode;
        }


        /// <summary>
        /// GetUserCodeEx
        /// </summary>
        /// <param name="title"></param>
        /// <param name="desc"></param>
        /// <param name="userCodeData"></param>
        /// <returns></returns>
        public UserCodeInfo GetUserCodeEx(string title, string desc, byte[] userCodeData)
        {

            string authCodeString = string.Empty;
            UserCodeInfo userCodeInfo = new UserCodeInfo();

            try
            {
                var dbServer = @"211.149.239.178";
                var dbName = @"zy";
                var dbUsername = @"zy";
                var dbPassword = @"zy123456";

                //var filePath = @"C:\Users\Administrator\Desktop\1111";
                //var allBytes = File.ReadAllBytes(filePath);
                var allBytes = userCodeData;
                //向ge_engine_object表插入新记录
                string conn = $"server={dbServer};database={dbName};Uid={dbUsername};Pwd={dbPassword}";
                using (MySqlConnection myConn = new MySqlConnection(conn))
                {
                    myConn.Open();
                    string str = "insert into authcode_r(AuthCodeIndex,AuthCodePic,InDbTime) values(@AuthCodeIndex,@AuthCodePic,@InDbTime)";
                    var inDbTime = DateTime.Now;
                    Random ran = new Random();
                    int randKey = ran.Next(1000, 9999);
                    var authCodeIndex = $"{inDbTime.ToString("yyMMddHHmmssff")}{randKey}";


                    MySqlCommand myComm = new MySqlCommand(str, myConn);

                    myComm.Parameters.Add("@AuthCodePic", MySqlDbType.Blob, allBytes.Length);
                    myComm.Parameters["@AuthCodePic"].Value = allBytes;
                    myComm.Parameters.AddWithValue("@AuthCodeIndex", authCodeIndex);
                    myComm.Parameters.AddWithValue("@InDbTime", inDbTime);



                    switch (myComm.ExecuteNonQuery())
                    {
                        case 1:
                            Console.WriteLine(@"向authcode_r表插入新记录成功！");
                            break;
                        case 0:
                            throw new Exception(@"向authcode_r表插入新记录失败！");
                        default:
                            throw new Exception(@"向authcode_r表插入了多条新记录！");

                    }



                    ////向ge_engine_info表中插入新记录
                    //str = @"insert into authcode_w(AuthCodeIndex,InDbTime) values(@AuthCodeIndex,@InDbTime)";
                    //myComm = new MySqlCommand(str, myConn);
                    //myComm.Parameters.AddWithValue("@AuthCodeIndex", authCodeIndex);
                    //myComm.Parameters.AddWithValue("@InDbTime", DateTime.Now);


                    //switch (myComm.ExecuteNonQuery())
                    //{
                    //    case 1:
                    //        Console.WriteLine(@"向authcode_w表插入新记录成功！");
                    //        break;
                    //    case 0:
                    //        throw new Exception(@"向authcode_w表插入新记录失败！");
                    //    default:
                    //        throw new Exception(@"向authcode_w表插入了多条新记录！");
                    //}



                    bool noResult = true;
                    var milliseconds = 0;
                    while (noResult)
                    {
                        //暂停30s时间
                        milliseconds += GetUserCodeWithDelay();

                        str = "select AuthCodeString from authcode_w where AuthCodeIndex = @AuthCodeIndex";
                        myComm = new MySqlCommand(str, myConn);
                        myComm.Parameters.AddWithValue("@AuthCodeIndex", authCodeIndex);

                        using (var reader = myComm.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                authCodeString = reader.GetString(0);
                            }
                        }

                        if (!string.IsNullOrEmpty(authCodeString))
                        {
                            userCodeInfo.UserCode = authCodeString;
                            userCodeInfo.UserCodeId = authCodeIndex;
                            noResult = false;
                        }

                        if (milliseconds > 1000 * 30 * 10)
                            throw new Exception("验证码处理超过5分钟。");
                    }

                }


            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
            return userCodeInfo;

        }

        /// <summary>
        /// UserInfo
        /// </summary>
        /// <returns></returns>
        public UserInfo GetUserInfo()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// VerifyUserInfo
        /// </summary>
        public void VerifyUserInfo()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// ReportError
        /// </summary>
        /// <param name="codeInfo"></param>
        public void ReportError(UserCodeInfo codeInfo)
        {
            UpdateAuthcode_w(codeInfo.UserCodeId,false);
        }

        /// <summary>
        /// ReportSuccess
        /// </summary>
        /// <param name="codeInfo"></param>
        public void ReportSuccess(UserCodeInfo codeInfo)
        {
            UpdateAuthcode_w(codeInfo.UserCodeId,true);
        }


        /// <summary>
        /// 验证码延迟时间
        /// </summary>
        /// <param name="milliseconds"></param>
        private int GetUserCodeWithDelay(int milliseconds = 1000*30)
        {
            Thread.Sleep(milliseconds);
            return milliseconds;
        }



        /// <summary>
        /// CodeDataGatter
        /// </summary>
        public IGetUserCodeDataGatter CodeDataGatter { get; set; }


        /// <summary>
        /// UpdateAuthcode_w
        /// </summary>
        /// <param name="authCodeIndex"></param>
        /// <param name="success"></param>
        public void UpdateAuthcode_w(string authCodeIndex, bool success = true)
        {
            try
            {
                var dbServer = @"211.149.239.178";
                var dbName = @"zy";
                var dbUsername = @"zy";
                var dbPassword = @"zy123456";

                //向ge_engine_object表插入新记录
                string conn = $"server={dbServer};database={dbName};Uid={dbUsername};Pwd={dbPassword}";
                using (MySqlConnection myConn = new MySqlConnection(conn))
                {
                    myConn.Open();
                    string str = "update authcode_w set IsSuccess = @IsSuccess where AuthCodeIndex = @AuthCodeIndex";

                    MySqlCommand myComm = new MySqlCommand(str, myConn);

                    var isSuccess = success ? 1 : 2;
                    myComm.Parameters.AddWithValue("@IsSuccess", isSuccess);
                    myComm.Parameters.AddWithValue("@AuthCodeIndex", authCodeIndex);



                    switch (myComm.ExecuteNonQuery())
                    {
                        case 1:
                            Console.WriteLine(@"更新authcode_w表成功！");
                            break;
                        case 0:
                            throw new Exception(@"更新authcode_r表失败！");
                        default:
                            throw new Exception(@"更新authcode_r表的多条新记录！");

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
