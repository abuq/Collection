using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using Microsoft.SqlServer.Server;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace XFCollection.Json
{
    /// <summary>
    /// JsonHelper
    /// </summary>
    public class JsonHelper
    {

        /// <summary>
        /// Test
        /// </summary>
        public void Test()
        { 
            string jsonText = "[{'a':'aaa','b':'bbb','c':'ccc'},{'a':'aa','b':'bb','c':'cc'}]";
            JArray mJobject = JArray.Parse(jsonText);
            foreach (var ss in mJobject)
            {
                Console.WriteLine(ss["a"]);
                
                Console.WriteLine(ss["b"]);
                
            }

            Console.WriteLine(mJobject);
        }

    }
}
