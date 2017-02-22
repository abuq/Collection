using System;
using System.Collections.Generic;
using System.Text;
using X.GlodEyes.Collectors;
using X.GlodEyes.Collectors.Specialized;

namespace XFCollection.fayuan
{
    /// <summary>
    /// 法院参数
    /// </summary>
    public class FaYuanParameter : Parameter
    {

        /// <summary>
        /// 案由筛选
        /// </summary>
        public string Reason
        {
            get { return this.GetValueCore<string>(string.Empty, "Reason"); }
            set { this.SetValueCore((object) value, "Reason"); }
        }

        /// <summary>
        /// 地域及法院
        /// </summary>
        public string Court
        {
            get { return this.GetValueCore<string>(string.Empty, "Court"); }
            set { this.SetValueCore((object) value, "Court"); }
        }

        /// <summary>
        /// 裁判年份
        /// </summary>
        public string Year
        {
            get { return this.GetValueCore<string>(string.Empty, "Year"); }
            set { this.SetValueCore((object) value, "Year"); }
        }


    }
}
