namespace X.GlodEyes.Collectors.Specialized.JingDong
{
    /// <summary>
    /// 通用采集参数
    /// </summary>
    public class NormalParameter : Parameter
    {
        /// <summary>
        /// 搜索关键词
        /// </summary>
        /// <value>
        /// The keyword.
        /// </value>
        public string Keyword
        {
            get
            {
                return this.GetValueCore(string.Empty);
            }
            set
            {
                this.SetValueCore(value);
            }
        }
    }
}