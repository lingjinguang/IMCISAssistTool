using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace IMCISAssistTool.Utils
{
    class HttpUtils
    {
        /// <summary>
        /// 判断一个字符串是否为url
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsUrl(string str)
        {
            str = str.ToLower();
            string Url = @"^(http|https)://";
            return Regex.IsMatch(str, Url);
        }

        #region 
        //网络pdf文件保存到本地
        public static string SaveRemoteFile(string saveLoadFile, string pdfFile)
        {
            var saveFile = Path.Combine(saveLoadFile,Guid.NewGuid().ToString("D") + ".pdf");
            Uri downUri = new Uri(pdfFile);
            //建立一个web请求，返回HttpWebRequest对象
            HttpWebRequest hwr = (HttpWebRequest)WebRequest.Create(downUri);
            //流对象使用完后自动关闭
            using (Stream stream = hwr.GetResponse().GetResponseStream())
            {
                //文件流，流信息读到文件流中，读完关闭
                using (FileStream fs = File.Create(saveFile))
                {
                    //建立字节组，并设置它的大小是多少字节
                    byte[] bytes = new byte[102400];
                    int n = 1;
                    while (n > 0)
                    {
                        //一次从流中读多少字节，并把值赋给N，当读完后，N为０,并退出循环
                        n = stream.Read(bytes, 0, 10240);
                        fs.Write(bytes, 0, n);　//将指定字节的流信息写入文件流中
                    }
                }
            }
            return saveFile;
        }
        public static MemoryStream StreamToMemoryStream(Stream instream)
        {
            MemoryStream outstream = new MemoryStream();
            const int bufferLen = 4096;
            byte[] buffer = new byte[bufferLen];
            int count = 0;
            while ((count = instream.Read(buffer, 0, bufferLen)) > 0)
            {
                outstream.Write(buffer, 0, count);
            }
            return outstream;
        }

        #endregion 
    }
}
