using System;
using System.Collections.Generic;
using System.Linq;
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

using Microsoft.AspNet.SignalR.Client;

namespace LimitCode.WebIm.ServiceClient
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += MainWindow_Loaded;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var hubConnect = new HubConnection("http://localhost:50060/");
            var cookie = new System.Net.Cookie("ClientUserId", Guid.NewGuid().ToString(),"/","");
            hubConnect.CookieContainer .Add(cookie) ;
            var chartHub=hubConnect.CreateHubProxy("chartHub");
            chartHub.On<string>("ConnecteResult", (t) => {
                System.Diagnostics.Debug.WriteLine(t);
            });
            hubConnect.Start();
        }
    }
}
