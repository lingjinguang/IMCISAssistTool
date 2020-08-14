using System;
using WebSocketSharp;
using WebSocketSharp.Server;
using IMCISAssistTool.Model;
using IMCISAssistTool.Utils;
using IMCISAssistTool.Common;

namespace IMCISAssistTool.Services
{
    class GetPrintInfoService : WebSocketBehavior
    {
        private static GetPrintInfoService _instance { get; set; }
        /// <summary>
        /// 获得实例
        /// </summary>
        public static GetPrintInfoService Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new GetPrintInfoService();
                return _instance;
            }
            set
            {
                _instance = value;
            }
        }

        public GetPrintInfoService()
        {

        }
        /// <summary>
        /// 客户端发送send请求会被触发，接收请求数据
        /// </summary>
        protected override void OnMessage(MessageEventArgs e)
        {
            String result = "数据获取成功！";
            try
            {
                MessageEvent<String> data = JsonHelper.DeserializeJsonToObject<MessageEvent<String>>(e.Data);
                switch (Convert.ToString(data.fileType).ToUpper())
                {
                    case "PRINTERNAMES":
                        result = PrintUtils.GetPrinterNames();
                        break;
                    case "PAGESIZES":
                        result = PrintUtils.GetPageSizesByName(Convert.ToString(data.data));
                        break;
                    default:
                        result = "参数异常:fileType字段只能为getPrinterName、getPageSizes";
                        break;
                }
            }
            catch (Exception ex)
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
