using System;
using System.Collections.Generic;
using System.Text;
using X.GlodEyes.Collectors;

namespace XFCollection.fayuan
{
    /// <summary>
    /// ZhiXingAndShiXinParameter
    /// </summary>
    public class ZhiXingAndShiXinParameter:Parameter
    {
        /// <summary>
        /// Name
        /// </summary>
        public string Name
        {
            get { return this.GetValueCore<string>(string.Empty, "Name"); }
            set { this.SetValueCore((object)value,"Name");}
        }

        /// <summary>
        /// Identifier
        /// </summary>
        public string Identifier
        {

            get { return this.GetValueCore<string>(string.Empty, "Identifier"); }
            set { this.SetValueCore((object)value, "Identifier");}
        }

        /// <summary>
        /// Type
        /// </summary>
        public string Type
        {
            get { return this.GetValueCore<string>(string.Empty, "Type"); }
            set { this.SetValueCore((object)value, "Type"); }
        }

    }
}
