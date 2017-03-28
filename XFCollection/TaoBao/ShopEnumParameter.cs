using System;
using System.Collections.Generic;
using System.Text;
using X.GlodEyes.Collectors.Specialized.JingDong;

namespace XFCollection.TaoBao
{
    /// <summary>
    /// ShopEnumParameter
    /// </summary>
    public class ShopEnumParameter : NormalParameter
    {
        /// <summary>
        /// 关键词
        /// </summary>
        public string KeyWord
        {
            get
            {
                return this.GetValueCore<string>(string.Empty, "KeyWord");
            }
            set
            {
                this.SetValueCore((object)value, "KeyWord");
            }
        }


        /// <summary>
        /// 店铺类型
        /// </summary>
        public string ShopType
        {
            get
            {
                return this.GetValueCore<string>(string.Empty, "ShopType");
            }
            set
            {
                this.SetValueCore((object)value, "ShopType");
            }
        }

        /// <summary>
        /// 搜索页数
        /// </summary>
        public string SearchPage
        {
            get
            {
                return this.GetValueCore<string>(string.Empty, "SearchPage");
            }
            set
            {
                this.SetValueCore((object)value, "SearchPage");
            }
        }

    }
}
