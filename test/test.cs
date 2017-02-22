using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace test
{
    internal class test
    {
        /// <summary>
        /// Test
        /// </summary>
        public void Test()
        {

            bool exist = false;
            var processes = Process.GetProcesses();
            foreach (Process item in processes)
            {
                if (item.ProcessName.ToLower() == "gov")//判断gov是否运行
                {
                    exist = true;
                }
            }
            if (!exist)
            {
                var desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                var path = Path.Combine(desktop,"gov","gov.exe");
                Process.Start(path);//运行程序
            }


        }
    }
} 
