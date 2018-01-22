using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LimitCode.WebIm.WebApp.Models
{
    /// <summary>
    /// 游客 访客 连接对象
    /// </summary>
    public class VisitorConnectUser: ConnectUser ,ICloneable
    {
        /// <summary>
        /// 随机分配的一个客户昵称
        /// </summary>
        public string VisitorUserName { get; set; }
        /// <summary>
        /// 分配的客服链接 id
        /// </summary>
        public string ServiceConnectionId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}