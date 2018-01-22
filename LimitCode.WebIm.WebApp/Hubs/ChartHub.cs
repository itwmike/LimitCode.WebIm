﻿using System;
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
        /// 保存 游客的信息集合
        /// </summary>
        private static ConcurrentDictionary<string, VisitorConnectUser> VisitorOnlineUsers = new ConcurrentDictionary<string, VisitorConnectUser>();
        /// <summary>
        /// 保存 客服的信息集合
        /// </summary>
        private static ConcurrentDictionary<string, ServiceConnectUser> ServiceOnlineUsers = new ConcurrentDictionary<string, ServiceConnectUser>();

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
                    var connectType = Context.Headers["connectType"];
                    if (connectType != null && !string.IsNullOrWhiteSpace(connectType)) {
                       await ServiceConnect(ClientUserId.Value);
                    }
                    else {
                       await VisitorUserConnect(ClientUserId.Value);
                    }
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
                var user = new VisitorConnectUser();
                VisitorOnlineUsers.TryRemove(ClientUserId.Value, out user);
                var serviceUser = new ServiceConnectUser();
                ServiceOnlineUsers.TryRemove(ClientUserId.Value, out serviceUser);
            }
            return base.OnDisconnected(stopCalled);
        }
        /// <summary>
        /// 获取所有未分配客服 的用户
        /// </summary>
        /// <returns></returns>
        public IEnumerable<VisitorConnectUser> GetAllNoAllotUser()
        {
            return VisitorOnlineUsers.Where(t => string.IsNullOrWhiteSpace(t.Value.ServiceConnectionId)).Select(t => t.Value);
        }
        /// <summary>
        /// 客服和访客建立连接关系
        /// </summary>
        /// <param name="VisitorConnectionId"></param>
        /// <param name="ServiceConnectId"></param>
        /// <returns></returns>
        public  bool OnConnectVisitorUser(string VisitorConnectionId)
        {
            var result = false;
            var ServiceConnectId = Context.ConnectionId;
            var user = VisitorOnlineUsers.FirstOrDefault(t => t.Value.ClientConnectionId == VisitorConnectionId);
            if ( !string.IsNullOrWhiteSpace(user.Key)) {
                var newUser = user.Value.Clone() as VisitorConnectUser;
                newUser.ServiceConnectionId = ServiceConnectId;
                result= VisitorOnlineUsers.TryUpdate(user.Key, newUser, user.Value);
                //通知其他客服端 该访客已被接入
                var otherService = ServiceOnlineUsers.Where(t => t.Value.ClientConnectionId != ServiceConnectId);
                if (otherService != null && otherService.Count() > 0) {
                      Clients.Clients(otherService.Select(t => t.Value.ClientConnectionId).ToList()).OnConnectVisitorUser(VisitorConnectionId);
                }
            }
            return  result;
        }
        /// <summary>
        /// 客服 链接
        /// </summary>
        /// <param name="user"></param>
        private async Task ServiceConnect(string ClientUserId)
        {
            //客服 链接
            var user = new ServiceConnectUser()
            {
                ClientUserId = ClientUserId,
                ClientConnectionId = Context.ConnectionId,
                ServiceUserName = Context.RequestCookies["ClientUserName"].Value
        };
            ServiceOnlineUsers.TryAdd(user.ClientUserId, user);
            await Clients.Caller.ConnecteResult("连接成功");
        }
        /// <summary>
        /// 访客连接
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        private async Task VisitorUserConnect(string ClientUserId)
        {
            //游客 链接
            var user = new VisitorConnectUser()
            {
                ClientUserId = ClientUserId,
                ClientConnectionId = Context.ConnectionId,
                VisitorUserName="访客1",
            };
            VisitorOnlineUsers.TryAdd(user.ClientUserId, user);
            await Clients.Caller.ConnecteResult("连接成功");
            if (ServiceOnlineUsers.Count > 0) {
                //通知所有客服 端有访客 接入
                Clients.Clients(ServiceOnlineUsers.Select(t => t.Value.ClientConnectionId).ToList()).AddNewsConnectUser(user);
            }
        }

    }
}