using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using DotRas;

//参考http://www.cnblogs.com/vingi/articles/4334948.html

namespace XFCollection.ADSL
{
    /// <summary>
    /// ADSLHelper
    /// </summary>
    public class ADSLHelper
    {
        /// <summary>
        /// 创建或更新一个PPPOE连接
        /// </summary>
        /// <param name="updatePPPOEName"></param>
        private void CreateOrUpdatePPPOE(string updatePPPOEName)
        {
            var allUsersPhoneBook = new RasPhoneBook();
            var path = RasPhoneBook.GetPhoneBookPath(RasPhoneBookType.AllUsers);
            allUsersPhoneBook.Open(path);
            //如果该名称的PPPOE已经存在，则更新这个PPPOE服务器地址
            if (allUsersPhoneBook.Entries.Contains(updatePPPOEName))
            {
                allUsersPhoneBook.Entries[updatePPPOEName].PhoneNumber = " ";
                //不管当前PPPOE是否连接，服务器地址的更新总能成功，如果正在连接，则需要PPPOE重启后才能起作用
                allUsersPhoneBook.Entries[updatePPPOEName].Update();
            }
            //创建一个新的PPPOE
            else
            {
                var address = string.Empty;
                var readOnlyCollection = RasDevice.GetDevices();
                var device = RasDevice.GetDevices().First(o => o.DeviceType == RasDeviceType.PPPoE);
                //建立宽带连接
                var entry = RasEntry.CreateBroadbandEntry(updatePPPOEName,device);
                entry.PhoneNumber = " ";
                allUsersPhoneBook.Entries.Add(entry);
            }

        }



        /// <summary>
        /// 创建一个PPPOE连接
        /// </summary>
        /// <param name="updatePPPOEName"></param>
        private void CreatePPPOE(string updatePPPOEName)
        {

            var conns = RasConnection.GetActiveConnections();
            if (conns != null)
            {
                foreach (var conn in conns)
                {
                    conn.HangUp();
                }
            }

            var allUsersPhoneBook = new RasPhoneBook();
            var path = RasPhoneBook.GetPhoneBookPath(RasPhoneBookType.AllUsers);
            allUsersPhoneBook.Open(path);

            //创建一个新的PPPOE
            var address = string.Empty;
            var readOnlyCollection = RasDevice.GetDevices();
            var device = RasDevice.GetDevices().First(o => o.DeviceType == RasDeviceType.PPPoE);
            //建立宽带连接
            var entry = RasEntry.CreateBroadbandEntry(updatePPPOEName, device);
            entry.PhoneNumber = " ";
            if(!allUsersPhoneBook.Entries.Contains(updatePPPOEName))
                allUsersPhoneBook.Entries.Add(entry);
        }

        /// <summary>
        /// Connect
        /// </summary>
        /// <param name="connection"></param>
        private void Connect(string connection)
        {
            try
            {
                CreatePPPOE(connection);
                var dialer = new RasDialer
                {
                    EntryName = connection,
                    PhoneNumber = " ",
                    AllowUseStoredCredentials = true,
                    PhoneBookPath = RasPhoneBook.GetPhoneBookPath(RasPhoneBookType.AllUsers),
                    Credentials = new NetworkCredential("057111414695", "336875"),
                    Timeout = 1000
                };
                var myras = dialer.Dial();
                while (myras.IsInvalid)
                {
                    Thread.Sleep(5000);
                    myras = dialer.Dial();
                }
                if (!myras.IsInvalid)
                {
                    Console.WriteLine($"RasDialer Success! {DateTime.Now}");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"RasDialer error! {DateTime.Now} connection error is :: {ex.ToString()}");
                //Console.WriteLine($"RasDialer error! {DateTime.Now} connection error is :: {ex.ToString()}");
            }
        }

        /// <summary>
        /// AutoADSLConnect
        /// </summary>
        public void AutoADSLConnect()
        {
            Connect("123");
            //休息5s
            Thread.Sleep(5000);
        }

        /// <summary>
        /// Test
        /// </summary>
        public void Test()
        {
            Connect("123");
        }

    }



}
