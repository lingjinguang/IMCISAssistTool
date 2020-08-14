using System;
using System.IO;
using System.Net;
using System.Linq;
using System.Drawing;
using WebSocketSharp;
using WebSocketSharp.Server;
using IMCISAssistTool.Model;
using IMCISAssistTool.Utils;
using IMCISAssistTool.Common;

namespace IMCISAssistTool.Services
{
    public class PrinterService : WebSocketBehavior
    {
        private static PrinterService _instance { get; set; }
        /// <summary>
        /// 获得实例
        /// </summary>
        public static PrinterService Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new PrinterService();
                return _instance;
            }
            set
            {
                _instance = value;
            }
        }
       
        public PrinterService()
        {

        }

        /// <summary>
        /// 客户端发送send请求会被触发，接收请求数据
        /// </summary>
        protected override void OnMessage(MessageEventArgs e)
        {
            String result = "打印成功！";
            try
            {
                MessageEvent<String> data = JsonHelper.DeserializeJsonToObject<MessageEvent<String>>(e.Data);
                string eventType = Convert.ToString(data.eventType).ToUpper();
                string printType = Convert.ToString(data.printType).ToUpper();
                string direction = Convert.ToString(data.direction);
                string printerName = Convert.ToString(data.printerName);
                string paperName = Convert.ToString(data.paperName);
                string folderName = Guid.NewGuid().ToString();  //pdf转图片所存放的文件夹名称
                string tempDir = Path.Combine(Directory.GetCurrentDirectory(), "tempImg");  //生成图片临时存放文件夹
                string tempImageDir = Path.Combine(tempDir, folderName);
                //string targetPath = Path.Combine(tempDir, fileName + ".jpg");
                int width = data.width;
                int height = data.height;
                Boolean customSizeFlag = data.customSizeFlag;
                WebRequest webreq = null;
                WebResponse webres = null;
                Boolean isUrl = HttpUtils.IsUrl(Convert.ToString(data.data));
                string fileType = Convert.ToString(data.fileType).ToUpper();
                #region
                if (File.Exists(Convert.ToString(data.data)))   //fileType由于IMCIS没有对应变量，data是路径文件名时取值为其后缀，否则当printType时“BASE64”也是JPG，否则就是HTML打印
                {
                    fileType = Path.GetExtension(Convert.ToString(data.data)).Substring(1).ToUpper();
                }
                else if (isUrl) //网络文件，通过ContentType获取
                {
                    webreq = WebRequest.Create(Convert.ToString(data.data));
                    webres = webreq.GetResponse();
                    string[] contentType = Convert.ToString(webres.ContentType).Split('/');
                    fileType = contentType.Length > 1 ? contentType[1].ToUpper() : fileType;
                }
                else
                {
                    fileType = printType == "BASE64" ? "JPG" : "HTML";
                }
                #endregion
                switch (fileType)
                {
                    case "JPG":
                    case "JPEG":
                    case "PNG":
                    case "RAW":
                    case "BMP":
                        if (isUrl)
                        {
                            using (Stream stream = webres.GetResponseStream())
                            {
                                using (Image image = Image.FromStream(stream))
                                {
                                    result = PrintUtils.PrintImage(image, eventType, direction, printerName, paperName);
                                }
                            }
                        }
                        else
                        {
                            result = printType == "FILEPATH" ?
                               PrintUtils.PreView(Convert.ToString(data.data), eventType, direction, printerName, paperName)
                               : PrintUtils.PrintImage(Convert.ToString(data.data), eventType, printType, direction, printerName, paperName);
                        }
                        break;
                    case "TXT":
                        PrintUtils.PrintTxt(Convert.ToString(data.data), printType);
                        break;
                    case "PDF":
                        string fileName = Path.GetFileNameWithoutExtension(data.data);  //通过完整路径取得pdf文件名称作为jpg的文件名
                        if (isUrl)
                        {
                            using (Stream stream = webres.GetResponseStream())
                            {
                                MemoryStream ms = HttpUtils.StreamToMemoryStream(stream);
                                Pdf2JpgUtils.Pdf2Jpg(ms, Path.Combine(tempDir, fileName + ".jpg"), null, folderName);
                            }
                        }
                        else
                        {
                            Pdf2JpgUtils.Pdf2Jpg(Convert.ToString(data.data), Path.Combine(tempDir, fileName + ".jpg"), null, folderName);
                        }

                        if (eventType == "PRINT")
                        {
                            var files = Directory.GetFiles(tempImageDir).OrderBy(f => Convert.ToInt32(Path.GetFileNameWithoutExtension(f)));
                            foreach (var file in files)
                            {
                                result = PrintUtils.PrintImage(file,eventType, printType, direction, printerName, paperName);
                            }
                            try
                            {
                                //删除文件夹
                                Directory.Delete(tempImageDir, true);
                            }
                            catch (Exception ex)
                            {
                                result = string.Format("删除临时目录出错：{0};其他错误信息：{1}", tempImageDir, ex);
                            }
                        }
                        else if (eventType == "PREVIEW")
                        {
                            result = PrintUtils.PreView(tempImageDir, eventType, direction, printerName, paperName);
                        }
                        break;
                    case "HTML":
                        string tempPdf = Path.Combine(Directory.GetCurrentDirectory(), "tempPdf");
                        string tempPdfDir = Path.Combine(tempPdf, folderName);
                        Directory.CreateDirectory(tempPdfDir);
                        string tempPdfPath = Path.Combine(tempPdfDir, "Html2Pdf.pdf");
                        Html2PdfUtils.Html2Pdf(Convert.ToString(data.data).Trim(), tempPdfPath, paperName, direction);
                        Pdf2JpgUtils.Pdf2Jpg(tempPdfPath, Path.Combine(tempDir, "Html2Pdf.jpg"), null, folderName);
                        if (eventType == "PRINT")
                        {
                            var files = Directory.GetFiles(tempImageDir).OrderBy(f => Convert.ToInt32(Path.GetFileNameWithoutExtension(f)));
                            foreach (var file in files)
                            {
                                result = PrintUtils.PrintImage(file, eventType, printType, direction, printerName, paperName);
                            }
                            try
                            {
                                //删除临时文件夹
                                Directory.Delete(tempImageDir, true);
                                Directory.Delete(tempPdfDir, true);
                            }
                            catch (Exception ex)
                            {
                                result = string.Format("删除临时目录出错：{0};其他错误信息：{1}", tempPdfDir, ex);
                            }
                        }
                        else if (eventType == "PREVIEW")
                        {
                            result = PrintUtils.PreView(tempImageDir, eventType, direction, printerName, paperName);
                        }
                        break;
                    #region pdf不转图片直接打印
                    /*
                    case "PDF":
                        string fileName = Path.GetFileNameWithoutExtension(data.data);  //通过完整路径取得pdf文件名称作为jpg的文件名
                        if (eventType == "PRINT")
                        {
                            result = PrintUtils.PrintPdf(Convert.ToString(data.data), eventType, direction, printerName, paperName);
                        }
                        else if (eventType == "PREVIEW")
                        {
                            result = PrintUtils.PreViewPdf(Convert.ToString(data.data), eventType, direction, printerName, paperName);
                        }
                        break;
                    case "HTML":
                        string tempPdf = Path.Combine(Directory.GetCurrentDirectory(), "tempPdf");
                        string tempPdfDir = Path.Combine(tempPdf, folderName);
                        Directory.CreateDirectory(tempPdfDir);
                        string tempPdfPath = Path.Combine(tempPdfDir, "Html2Pdf.pdf");
                        Html2PdfUtils.Html2Pdf(Convert.ToString(data.data).Trim(), tempPdfPath, paperName, direction);
                        if (eventType == "PRINT")
                        {
                            result = PrintUtils.PrintPdf(tempPdfPath, eventType, direction, printerName, paperName);
                        }
                        else if (eventType == "PREVIEW")
                        {
                            result = PrintUtils.PreViewPdf(tempPdfPath, eventType, direction, printerName, paperName);
                        }
                        try
                        {
                            //删除临时文件夹
                            Directory.Delete(tempPdfDir, true);
                        }
                        catch (Exception ex)
                        {
                            result = string.Format("删除临时目录出错：{0};其他错误信息：{1}", tempPdfDir, ex);
                        }
                        break;
                         * */
                    #endregion
                    default:
                        result = "参数异常:fileType字段只能为jpg、pdf、html、getPrinterName、getPageSizes";
                        break;
                }
            }
            catch(Exception ex)
            {
                result = "打印出错【" + ex.Message + "】";
            }
            Send(result);
        }

        protected override void OnOpen()
        {
            //Instance();
        }
    }
}
