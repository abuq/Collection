using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using Ionic.Zip;

namespace XFCollection.fayuan
{
    /// <summary>
    ///     资源类
    /// </summary>
    public class AssemblyResource
    {
        #region Public Methods and Operators

        /// <summary>从程序集中载入资源</summary>
        /// <param name="path">资源的全路径，一般为嵌入资源.‘程序集名称.文件路径.文件名组成’</param>
        /// <param name="uriKind">
        ///     路径为相对路径还是绝对路径
        ///     如果是相对路径，则在路径前加入assembly的命名空间
        ///     如果使用都是，则会两者都进行尝试
        /// </param>
        /// <param name="assembly">
        ///     资源所在的程序集，一般使用GetExecutingAssembly获取.
        ///     如果该参数为null则使用调用LoadAssemblyStream者(GetCallingAssembly())所在的程序集
        /// </param>
        /// <returns></returns>
        public static Stream LoadAssemblyStream(string path, UriKind uriKind = UriKind.Relative,
            Assembly assembly = null)
        {
            assembly = assembly ?? Assembly.GetCallingAssembly();

            switch (uriKind)
            {
                case UriKind.Absolute:
                    return assembly.GetManifestResourceStream(path);

                case UriKind.Relative:

                    // 相对路径则转为绝对路径
                    var fullPath = string.Format("{0}.{1}", assembly.GetName().Name, path);
                    return LoadAssemblyStream(fullPath, UriKind.Absolute, assembly);

                default:

                    // 先尝试相对路径，再使用绝对路径
                    var stream = LoadAssemblyStream(path, UriKind.Relative, assembly);
                    return stream ?? LoadAssemblyStream(path, UriKind.Absolute, assembly);
            }
        }

        /// <summary>
        ///     传入资源所在程序集中的一个对象类型及资源文件相对于该对象的路径来返回资源流
        ///     路径使用.来分隔
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="fileName">Name of the file.</param>
        /// <returns></returns>
        public static Stream LoadAssemblyStream(Type type, string fileName)
        {
            var spaceName = string.Empty;

            var typeNames = type.FullName.Split('.');
            if (typeNames.Length > 0)
            {
                var names = new string[typeNames.Length - 1];
                Array.Copy(typeNames, names, names.Length);
                spaceName = string.Join(".", names);
            }

            var path = string.IsNullOrEmpty(spaceName) ? fileName : $"{spaceName}.{fileName}";
            return LoadAssemblyStream(path, UriKind.Absolute, type.Assembly);
        }

        /// <summary>
        ///     从载入的资源中载入字符内容
        ///     如果资源不存在返回null
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="encoding">文件的编码，默认为utf8.</param>
        /// <returns></returns>
        public static string LoadString(Type type, string fileName, Encoding encoding = null)
        {
            using (var stream = LoadAssemblyStream(type, fileName))
            {
                if (stream == null)
                {
                    return null;
                }

                using (var reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }


        /// <summary>
        ///     得到资源文件
        /// </summary>
        /// <returns></returns>
        public static void GetFile(string filePath = @"C:\")
        {
#if DEBUG
            MessageBox.Show(@"TEST");
#endif
            try
            {

 
            var fileName = @"testdata.zip";
            var filePathAndName = Path.Combine(filePath, fileName); // $@"{filePath}{fileName}";


            //如果路径不存在 则创建
            if (!Directory.Exists(filePath))
            {
                var directoryInfo = new DirectoryInfo(filePath);
                directoryInfo.Create();
            }


            using (
                var stream = LoadAssemblyStream(@"Resources.tessdata.zip",
                    UriKind.RelativeOrAbsolute))
            {
                Debug.WriteLine("资源" + (stream == null ? "为空" : "不为空"));


                using (var fs = new FileStream(filePathAndName, FileMode.Create))
                {
                    var bytes = new byte[stream.Length];
                    var numBytesRead = 0;
                    var numBytesToRead = (int) stream.Length;
                    stream.Position = 0;
                    while (numBytesToRead > 0)
                    {
                        var n = stream.Read(bytes, numBytesRead, Math.Min(numBytesToRead, int.MaxValue));
                        if (n <= 0)
                        {
                            break;
                        }
                        fs.Write(bytes, numBytesRead, n);
                        numBytesRead += n;
                        numBytesToRead -= n;
                    }
                    fs.Close();
                }
            }



            //解压  
            using (var zip = new ZipFile(filePathAndName))
            {
                zip.ExtractAll(filePath,
                    ExtractExistingFileAction.OverwriteSilently);
            }
            //设置验证码语言路径
            /*TesseractDemo.Path = Regex.Match(filePathAndName, ".*(?=.zip)").Value;*/
            TesseractDemo.Path = Path.Combine(filePath, @"tessdata");

            }
            catch (Exception e)
            {
                throw new Exception($"初始化验证码组件异常,{e.Message}", e );
            }
        }

        #endregion

        #region Methods

        /// <summary>
        ///     内部测试
        /// </summary>
        private static void Test()
        {
            var stream = LoadAssemblyStream(@"Resources.tessdata.zip",
                UriKind.RelativeOrAbsolute);
            Debug.WriteLine("资源" + (stream == null ? "为空" : "不为空"));

            //Assembly.GetCallingAssembly().GetManifestResourceStream("XFCollection.Resources.tessdata.zip");
            //Debug.WriteLine("资源" + (stream == null ? "为空" : "不为空"));


            var path = @"D:\test\tessdata.zip";
            using (var fs = new FileStream(path, FileMode.Create))
            {
                var bytes = new byte[stream.Length];
                var numBytesRead = 0;
                var numBytesToRead = (int) stream.Length;
                stream.Position = 0;
                while (numBytesToRead > 0)
                {
                    var n = stream.Read(bytes, numBytesRead, Math.Min(numBytesToRead, int.MaxValue));
                    if (n <= 0)
                    {
                        break;
                    }
                    fs.Write(bytes, numBytesRead, n);
                    numBytesRead += n;
                    numBytesToRead -= n;
                }
                fs.Close();
            }


            stream.Close();


            //解压  
            using (var zip = new ZipFile(@"D:\test\tessdata.zip"))
            {
                zip.ExtractAll(@"D:\test", ExtractExistingFileAction.OverwriteSilently);
            }


            //if (stream != null) stream.Dispose();
            //stream = AssemblyResource.LoadAssemblyStream("Spiders.AliBaBaCn.AliBaBaAreaInfos.AliBaBaArea.zip");
            //Debug.WriteLine("资源" + (stream == null ? "为空" : "不为空"));
            //if (stream != null) stream.Dispose();
            //stream = AssemblyResource.LoadAssemblyStream(typeof(AliBaBaAreaInfo), "AliBaBaArea.zip");
            //Debug.WriteLine("资源" + (stream == null ? "为空" : "不为空"));
            //if (stream != null) stream.Dispose();
        }


        /// <summary>
        ///     测试1
        /// </summary>
        public void test1()
        {
            GetFile();
            //GetFile(@"D:\test\");         
        }

        #endregion
    }
}
