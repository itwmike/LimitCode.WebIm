using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using LimitCode.WebIm.WebApp.Models;
using Microsoft.AspNet.SignalR;

namespace LimitCode.WebIm.WebApp.Hubs
{
    /// <summary>
    /// Web Im 集线器
    /// </summary>
    public class ChartHub : Hub
    {
        /// <summary>
        /// 保存客户端用户的信息集合
        /// </summary>
        private static ConcurrentDictionary<string, ConnectUser> _OnlineUsers = new ConcurrentDictionary<string, ConnectUser>();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override async Task OnConnected()
        {
            await base.OnConnected();
            if (!Context.RequestCookies.ContainsKey("ClientUserId")) {
                await Clients.Caller.ConnecteResult("连接失败");
                await base.OnDisconnected(false);
            }
            else
            {
                var ClientUserId = Context.RequestCookies["ClientUserId"];
                if (ClientUserId == null || string.IsNullOrWhiteSpace(ClientUserId.Value))
                {
                    await Clients.Caller.ConnecteResult("连接失败");
                    await base.OnDisconnected(false);
                }
                else
                {
                    _OnlineUsers.TryAdd(ClientUserId.Value, new ConnectUser() { ClientUserId = ClientUserId.Value, ConnectionId = Context.ConnectionId });
                    await Clients.Caller.ConnecteResult("连接成功");
                }
            }
        }
        /// <summary>
        /// 重新链接的时候
        /// </summary>
        /// <returns></returns>
        public override Task OnReconnected()
        {
            return base.OnReconnected();
        }
        /// <summary>
        /// 释放链接的时候
        /// </summary>
        /// <param name="stopCalled"></param>
        /// <returns></returns>
        public override Task OnDisconnected(bool stopCalled)
        {
            if (Context.RequestCookies.ContainsKey("ClientUserId")) {
                var ClientUserId = Context.RequestCookies["ClientUserId"];
                var user = new ConnectUser();
                _OnlineUsers.TryRemove(ClientUserId.Value, out user);
            }
            return base.OnDisconnected(stopCalled);
        }

    }
}