using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LimitCode.WebIm.ServiceClient.Modules
{
    /// <summary>
    /// 访客 连接 对象
    /// </summary>
    public class VisitorConnectUser
    {
        /// <summary>
        /// 客户端 名称，一个访问客户只会分配一个
        /// </summary>
        public string VisitorUserName { get; set; }
        /// <summary>
        /// 客户端链接Id， 每次连接都会有Hub 随机分配
        /// </summary>
        public string ClientConnectionId { get; set; }
        /// <summary>
        /// 消息数量
        /// </summary>
        public int MsgCount { get; set; }
    }
}
