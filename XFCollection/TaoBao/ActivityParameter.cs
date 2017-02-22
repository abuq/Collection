using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using X.GlodEyes.Collectors.Specialized.JingDong;

namespace XFCollection.TaoBao
{
    /// <summary>
    /// AcitivtyParameter
    /// </summary>
    public class ActivityParameter : NormalParameter
    {
       /// <summary>
       /// 链接
       /// </summary>
        public string Url
        {
            get
            {
                return this.GetValueCore<string>(string.Empty, "Url");
            }
            set
            {
                this.SetValueCore((object)value, "Url");
            }
        }


    }

}
