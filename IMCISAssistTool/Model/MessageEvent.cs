using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;

namespace IMCISAssistTool.Model
{
    /// <summary>
    /// 通讯消息事件
    /// </summary>
    public class MessageEvent<T>
    {
        /// <summary>
        /// 事件类型
        /// </summary>
        public string eventType { get; set; }
        /// <summary>
        /// 打印类型
        /// </summary>
        public string printType { get; set; }
        /// <summary>
        /// 文件类型
        /// </summary>
        public string fileType { get; set; }
        /// <summary>
        /// 打印机名称
        /// </summary>
        public string printerName { get; set; }
        /// <summary>
        /// 打印纸张名称
        /// </summary>
        public string paperName { get; set; }
        /// <summary>
        /// 打印方向
        /// </summary>
        public string direction { get; set; }
        /// <summary>
        /// 图片宽
        /// </summary>
        public int width { get; set; }
        /// <summary>
        /// 图片高
        /// </summary>
        public int height { get; set; }
        /// <summary>
        /// 是否自定义大小
        /// </summary>
        public Boolean customSizeFlag { get; set; }
        
        /// <summary>
        /// 传输数据
        /// </summary>
        public T data { get; set; }
    }
}
