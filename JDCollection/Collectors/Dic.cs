namespace X.GlodEyes.Collectors
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Text;

    using X.CommLib.Miscellaneous;

    /// <summary>
    ///     字典基类
    /// </summary>
    public class Dic : Dictionary<string, object>, IDic
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="Dic" /> class.
        /// </summary>
        public Dic()
            : base(StringComparer.OrdinalIgnoreCase)
        {
        }

        /// <summary>
        ///     从指定的对象中复制值
        /// </summary>
        /// <param name="dic">The parameter.</param>
        /// <param name="overwrite">if set to <c>true</c> [overwrite].</param>
        public void CopyFrom(IDic dic, bool overwrite = true)
        {
            foreach (var item in dic)
            {
                if (overwrite)
                {
                    this[item.Key] = item.Value;
                }
                else if (!this.ContainsKey(item.Key))
                {
                    this[item.Key] = item.Value;
                }
            }
        }

        /// <summary>
        ///     将值复制到指定的对象中
        /// </summary>
        /// <param name="dic">The dic.</param>
        /// <param name="overwrite">if set to <c>true</c> [overwrite].</param>
        public void CopyTo(IDic dic, bool overwrite = true)
        {
            dic.CopyFrom(this, overwrite);
        }

        /// <summary>
        ///     返回字符串格式的值
        /// </summary>
        /// <param name="keyName">Name of the key.</param>
        /// <returns></returns>
        public string GetStringValue(string keyName)
        {
            return this.GetValue(keyName, (string)null);
        }

        /// <summary>
        ///     返回指定 key 的值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="keyName">Name of the key.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">@无效的关键值</exception>
        public T GetValue<T>(string keyName, T defaultValue = default(T))
        {
            if (keyName == null)
            {
                throw new ArgumentNullException(nameof(keyName), @"无效的关键值");
            }

            object value;
            return !this.TryGetValue(keyName, out value)
                       ? defaultValue
                       : ObjectDetailOutput.Convert(value, defaultValue);
        }

        /// <summary>
        ///     返回一个字符串格式的指定值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="keyName">Name of the key.</param>
        /// <returns></returns>
        public T GetValueCore<T>(T defaultValue, [CallerMemberName] string keyName = null)
        {
            return this.GetValue(keyName, defaultValue);
        }

        /// <summary>
        ///     将 key 输出为数组
        /// </summary>
        /// <returns></returns>
        public string[] KeysToArray()
        {
            var keyCollection = this.Keys;
            var keys = new string[keyCollection.Count];

            keyCollection.CopyTo(keys, 0);

            return keys;
        }

        /// <summary>
        ///     设置值
        /// </summary>
        /// <param name="keyName">Name of the key.</param>
        /// <param name="value">The value.</param>
        public void SetValue(string keyName, object value)
        {
            if (keyName == null)
            {
                throw new ArgumentNullException(nameof(keyName), @"无效的关键值");
            }

            this[keyName] = value;
        }

        /// <summary>
        ///     设置值
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="keyName">Name of the key.</param>
        public void SetValueCore(object value, [CallerMemberName] string keyName = null)
        {
            this.SetValue(keyName, value);
        }

        /// <summary>
        ///     Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        ///     A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            var builder = new StringBuilder();
            foreach (var key in this.Keys)
            {
                builder.AppendLine($"{key} # {this[key]}");
            }

            return builder.ToString();
        }
    }
}