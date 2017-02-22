using System;
using System.Collections.Generic;
using System.Text;
using X.GlodEyes.Collectors;

namespace XFCollection.gongshang
{
    /// <summary>
    /// X315Parameter
    /// </summary>
    public class X315Parameter :Parameter
    {
        /// <summary>
        /// KeyWord
        /// </summary>
        public string KeyWord
        {
            get { return this.GetValueCore<string>(string.Empty, "KeyWord"); }
            set { this.SetValueCore((object)value,"KeyWord");}
        }

        /// <summary>
        /// UserName
        /// </summary>
        public string UserName
        {
            get { return this.GetValueCore<string>(string.Empty, "UserName"); }
            set { this.SetValueCore((object)value, "UserName"); }
        }

        /// <summary>
        /// PassWord
        /// </summary>
        public string PassWord
        {
            get { return this.GetValueCore<string>(string.Empty, "PassWord"); }
            set { this.SetValueCore((object)value, "PassWord"); }
        }


    }
}
