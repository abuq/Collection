namespace X.GlodEyes.Collectors
{
    using System.Collections.Generic;

    /// <summary>
    ///     一个字典类，作为参数的基类
    /// </summary>
    public interface IDic : IDictionary<string, object>
    {
        /// <summary>
        ///     从指定的对象中复制值
        /// </summary>
        /// <param name="dic">The parameter.</param>
        /// <param name="overwrite">if set to <c>true</c> [overwrite].</param>
        void CopyFrom(IDic dic, bool overwrite = true);

        /// <summary>
        ///     将值复制到指定的对象中
        /// </summary>
        /// <param name="dic">The dic.</param>
        /// <param name="overwrite">if set to <c>true</c> [overwrite].</param>
        void CopyTo(IDic dic, bool overwrite = true);

        /// <summary>
        ///     返回一个指定key的值
        ///     给定参数化类型 T 的一个变量 t，只有当 T 为引用类型时，语句 t = null 才有效；
        ///     只有当 T 为数值类型而不是结构时，语句 t = 0 才能正常使用。
        ///     解决方案是使用 default 关键字，此关键字对于引用类型会返回 null，对于数值类型会返回零。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="keyName">The keyname.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns></returns>
        T GetValue<T>(string keyName, T defaultValue = default(T));
        

        /// <summary>
        /// 返回字符串格式的值
        /// </summary>
        /// <param name="keyName">Name of the key.</param>
        /// <returns></returns>
        string GetStringValue(string keyName);

        /// <summary>
        ///     设置值
        /// </summary>
        /// <param name="keyName">Name of the key.</param>
        /// <param name="value">The value.</param>
        void SetValue(string keyName, object value);

        /// <summary>
        /// 将 key 输出为数组
        /// </summary>
        /// <returns></returns>
        string[] KeysToArray();
    }
}