namespace X.GlodEyes.Collectors
{
    using System.Collections.Generic;

    using X.CommLib.Office;

    /// <summary>
    ///     通用处理器
    /// </summary>
    public abstract class NormalCollector : Collector
    {
        /// <summary>
        ///     设置该条目在页码
        /// </summary>
        /// <param name="result">The result.</param>
        /// <param name="page">The page.</param>
        /// <param name="overwrite">是不是强制写入</param>
        protected void SetResultSearchPageIndex(IResut result, int page, bool overwrite = true)
        {
            if (!overwrite && result.ContainsKey(@"SearchPageIndex"))
            {
                return;
            }

            result[@"SearchPageIndex"] = page;
        }

        /// <summary>
        ///     设置该条目在当前页面中的排名
        /// </summary>
        /// <param name="result">The result.</param>
        /// <param name="rank">The rank.</param>
        /// <param name="overwrite">是不是强制写入</param>
        protected void SetResultSearchPageRank(IResut result, int rank, bool overwrite = true)
        {
            if (!overwrite && result.ContainsKey(@"SearchPageRank"))
            {
                return;
            }

            result[@"SearchPageRank"] = rank;
        }

        /// <summary>
        ///     更新采集结果中的 keyname 值
        ///     结果集中与 keyNames 中 Key 对应的属性名将被改为 Value 中指定的值，如果 Value 设置为 null，则保留，如果 Key 不存在，则删除该属性
        /// </summary>
        /// <param name="resuts">The resuts.</param>
        /// <param name="keyNames">The key names.</param>
        protected void UpdateResultsKeyNames(IEnumerable<IResut> resuts, IDictionary<string, string> keyNames)
        {
            foreach (var resut in resuts)
            {
                UpdateResultKeyName(resut, keyNames);
            }
        }

        /// <summary>
        ///     按照 KEYNAMES 重命名 result，并删除不存在的key
        /// </summary>
        /// <param name="resut">The resut.</param>
        /// <param name="keyNames">The key names.</param>
        private static void UpdateResultKeyName(IDic resut, IDictionary<string, string> keyNames)
        {
            var keys = resut.KeysToArray();
            var removeList = new List<string>();

            foreach (var key in keys)
            {
                string newKey;

                if (keyNames.TryGetValue(key, out newKey))
                {
                    if (StringExtension.IsNullOrWhiteSpace(newKey))
                    {
                        // 未定义的key认为是一致的。
                        newKey = key;
                    }

                    if (StringExtension.Same(newKey, key, true))
                    {
                        // 如果新旧key一致，直接跳过
                        continue;
                    }

                    resut[newKey] = resut[key];
                }

                removeList.Add(key);
            }

            removeList.ForEach(key => resut.Remove(key));
        }
    }
}