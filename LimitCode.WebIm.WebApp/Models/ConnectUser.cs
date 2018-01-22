using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LimitCode.WebIm.WebApp.Models
{
    /// <summary>
    /// 用户连接 对象
    /// </summary>
    public abstract class ConnectUser
    {
        /// <summary>
        /// 客户端唯一编号，一个客户只会分配一个
        /// </summary>
        public string ClientUserId { get; set; }
        /// <summary>
        /// 客户端链接Id， 每次连接都会由Hub 随机分配
        /// </summary>
        public string ClientConnectionId { get; set; }
        
    }
}