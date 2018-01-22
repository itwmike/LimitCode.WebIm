using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LimitCode.WebIm.WebApp.Models
{
    /// <summary>
    /// 客服 用户连接
    /// </summary>
    public class ServiceConnectUser: ConnectUser,ICloneable
    {
        /// <summary>
        /// 客服昵称
        /// </summary>
        public string ServiceUserName { get; set; }
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