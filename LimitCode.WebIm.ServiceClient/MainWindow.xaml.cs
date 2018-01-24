using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using LimitCode.WebIm.ServiceClient.Modules;
using Microsoft.AspNet.SignalR.Client;

namespace LimitCode.WebIm.ServiceClient
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        /// <summary>
        /// 我的客户集合
        /// </summary>
        private ObservableCollection<VisitorConnectUser> MyConnectUserList;
        /// <summary>
        /// 未分配客户集合
        /// </summary>
        private ObservableCollection<VisitorConnectUser> NoAllotConnectUserList;


        /// <summary>
        /// 
        /// </summary>
        private HubConnection hubConnect;
        /// <summary>
        /// 
        /// </summary>
        private IHubProxy chartHubProxy;
        /// <summary>
        /// 
        /// </summary>
        private string _StatusMsg = "欢迎使用";
        /// <summary>
        /// 
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        private void NotifyPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        /// <summary>
        /// 
        /// </summary>
        public string StatusMsg {
            get => _StatusMsg;
            set  {
                _StatusMsg = value;
                NotifyPropertyChanged("StatusMsg");
            } 
        }
        /// <summary>
        /// 
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            MyConnectUserList = new ObservableCollection<VisitorConnectUser>();
            NoAllotConnectUserList = new ObservableCollection<VisitorConnectUser>();
            MyConnectUser_ListBox.ItemsSource = MyConnectUserList;
            NoAllotUser_ListBox.ItemsSource = NoAllotConnectUserList;
            bt_SendReply.Click += Bt_SendReply_Click;
            Loaded += MainWindow_Loaded;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Bt_SendReply_Click(object sender, RoutedEventArgs e)
        {
            //获取当前连接的访客
            var selectUser = MyConnectUser_ListBox.SelectedItem as VisitorConnectUser;
            var txt =  System.Windows.Markup.XamlWriter.Save( rtb_ReplyContent .Document);
            if (selectUser == null) {
                StatusMsg = "请选择一个访客来回复。";
                return;
            }
            var result = chartHubProxy.Invoke<bool>("ReplayVisitorUser", selectUser.ClientConnectionId, txt).Result;
            if (result) {
                //添加到对话 集合
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            hubConnect = new HubConnection("http://webim.limitcode.com/");
            var cookieContainer = new CookieContainer();
            var userIdCookie = new Cookie("ClientUserId", Guid.NewGuid().ToString(), "/", "webim.limitcode.com");
            cookieContainer.Add(userIdCookie);
            var userNameCookie = new Cookie("ClientUserName", System.Web.HttpUtility.UrlEncode("小芳"), "/", "webim.limitcode.com");
            cookieContainer.Add(userNameCookie);
            hubConnect.CookieContainer = cookieContainer;
            hubConnect.Headers.Add("connectType", "service");//标识为客户端链接
            chartHubProxy = hubConnect.CreateHubProxy("chartHub");
            //连接成功的回调
            chartHubProxy.On<string>("ConnecteResult", async (t) =>
            {
                StatusMsg = t;
                if (t != "连接成功") return;
                //获取 未分配的用户集合
                var result = await chartHubProxy.Invoke<IEnumerable<VisitorConnectUser>>("GetAllNoAllotUser");
                await this.Dispatcher.InvokeAsync(() =>
                 {
                     foreach (var item in result)
                     {
                         NoAllotConnectUserList.Add(item);
                     }
                 });
            });
            //服务端通知客服端 有新的未分配的访客接入
            chartHubProxy.On<VisitorConnectUser>("AddNewsConnectUser", (t) =>
            {
                this.Dispatcher.InvokeAsync(() =>
                {
                    StatusMsg = string.Format("有新访客【{0}】接入。", t.VisitorUserName);
                    NoAllotConnectUserList.Add(t);
                });
            });
            //服务端通知客服端 访客被客服接入
            chartHubProxy.On<string>("OnConnectVisitorUser", (t) =>
            {
                this.Dispatcher.InvokeAsync(() =>
                {
                    if (NoAllotConnectUserList.Count(k => k.ClientConnectionId == t) > 0)
                    {
                        //从未分配访客中移除该连接
                        NoAllotConnectUserList.Remove(NoAllotConnectUserList.FirstOrDefault(k => k.ClientConnectionId == t));
                    }
                });
            });
            //访客断开连接 
            chartHubProxy.On<string>("VisitorUserConnectClose", (t) =>
            {
                this.Dispatcher.InvokeAsync(() =>
                {
                    if (NoAllotConnectUserList.Count(k => t == k.ClientConnectionId) > 0)
                    {
                        //从未分配访客中移除该连接
                        NoAllotConnectUserList.Remove(NoAllotConnectUserList.FirstOrDefault(k => k.ClientConnectionId == t));
                    }
                    if (MyConnectUserList.Count(k => t == k.ClientConnectionId) > 0)
                    {
                        //从未分配访客中移除该连接
                        MyConnectUserList.Remove(MyConnectUserList.FirstOrDefault(k => k.ClientConnectionId == t));
                    }
                });
            });
            //接收访客的消息
            chartHubProxy.On<string,string>("ReceiveVisitorUserMsg", (VisitorConnectionId, msg) =>
            {
                // 如果 当前正在和访客聊天中，则将消息输出到 聊天记录窗口 否则 更新改访客的未处理消息个数
               var selectUser= MyConnectUser_ListBox.SelectedItem as VisitorConnectUser;
                if (selectUser == null || selectUser.ClientConnectionId != VisitorConnectionId)
                {
                    MyConnectUserList.SingleOrDefault(t => t.ClientConnectionId == VisitorConnectionId).MsgCount += 1;
                }
                else {
                    System.Diagnostics.Debug.WriteLine(msg);
                }
            });
            hubConnect.Start();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MyConnectUser_ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectUser = MyConnectUser_ListBox.SelectedItem as VisitorConnectUser;
            if (selectUser == null) return;
            //获取聊天记录


        }
        /// <summary>
        /// 未分配访客单击事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NoAllotUser_ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectUser = NoAllotUser_ListBox.SelectedItem as VisitorConnectUser;
            if (selectUser == null|| NoAllotUser_ListBox.SelectedIndex<0) return;
            var result= chartHubProxy.Invoke<bool>("OnConnectVisitorUser", new string[] { selectUser.ClientConnectionId } ).Result;
             this.Dispatcher.InvokeAsync(() =>
            {
                if (result)
                {
                    //分配成功
                    MyConnectUserList.Add(new VisitorConnectUser() { ClientConnectionId = selectUser.ClientConnectionId, VisitorUserName = selectUser.VisitorUserName });
                    StatusMsg = string.Format("访客【{0}】连接成功。", selectUser.VisitorUserName);
                }
                else
                {
                    //分配失败
                    StatusMsg = string.Format("访客【{0}】连接失败。", selectUser.VisitorUserName);
                }
                if (NoAllotConnectUserList.Count(k => selectUser.ClientConnectionId == k.ClientConnectionId) > 0)
                {
                    //从未分配访客中移除该连接
                    NoAllotConnectUserList.Remove(NoAllotConnectUserList.FirstOrDefault(k => k.ClientConnectionId == selectUser.ClientConnectionId));
                }
            });
        }
    }
}
