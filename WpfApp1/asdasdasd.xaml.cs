using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace WpfApp1
{
    /// <summary>
    /// asdasdasd.xaml 的交互逻辑
    /// </summary>
    public partial class asdasdasd : Page
    {
        private DispatcherTimer timer;
        private List<Receive> rl;                    //消息队列
        public struct Receive
        {
            public string Type { get; set; }
            public string Content { get; set; }
            public string Source { get; set; }
        }

        public asdasdasd(string id,List<Receive>r)
        {
            InitializeComponent();
            ID.Text = id;
            rl = r;
            ShowContent();

            //消息刷新设置为两秒一次
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(2);
            timer.Tick += Flash;
            //timer.Start();
        }

        private void ShowContent()
        {
            Thickness t = new Thickness(10, 10, 0, 0);
            StackPanel sp;
            if (rl.Count == 0)
                return;
            for (int i=0; i<=rl.Count-1; i++)
            {
                sp = new StackPanel();
                sp.Margin = t;
                //当前消息为列表第一项或上一条消息不是同一个人发出时标出发送者
                if (i==0||!rl[--i].Source.Equals(rl[i].Source))
                {
                    if(rl[i].Type.Equals("Text"))                         //文字输出处理
                    {
                        TextBlock tb = new TextBlock();
                        tb.Text = rl[i].Content;
                        sp.Children.Add(tb); 
                    }
                    else if(rl[i].Type.Equals("Picture"))                 //图片输出处理
                    {
                        BitmapImage bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.UriSource = new Uri(rl[i].Content);
                        bitmap.EndInit();
                        Image image = new Image();
                        image.Source = bitmap;
                        sp.Children.Add(image);
                    }
                    else 
                    {
                        TextBlock textBlock=new TextBlock();
                        textBlock.Text="文件已保存:\n"+rl[i].Content;
                        sp.Children.Add(textBlock);
                    }
                    show.Items.Add(sp);
                }
                else
                {
                    if (rl[i].Type.Equals("Text"))                        //文字输出处理
                    {
                        TextBlock tb = new TextBlock();
                        tb.Text = rl[i].Content;
                        sp.Children.Add(tb); 
                    }
                    else if (rl[i].Type.Equals("Picture"))                //图片输出处理
                    {
                        Image image = new Image();
                        BitmapImage bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.UriSource = new Uri(rl[i].Content);
                        bitmap.EndInit();
                        image.Source = bitmap;
                        sp.Children.Add(image);
                    }
                    else 
                    {
                        TextBlock textBlock=new TextBlock();

                        textBlock.Text="文件已保存:\n"+rl[i].Content;
                        sp.Children.Add(textBlock);
                    }
                }
            }
        }

        /// <summary>
        /// 当前聊天页面消息刷新
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Flash(object sender, object e)
        {
            int minus = MainWindow.mw.getNavigateTableCount(ID.Text) - rl.Count;          //计算后台消息推送数与实际页面消息显示数量差值
            if(minus!=0)
            {
                rl = MainWindow.mw.getPageContent(ID.Text);                
                ShowContent();
            }
        }

        /// <summary>
        /// 开启新线程进行信息封装以及异步发送消息
        /// </summary>
        /// <param name="type">消息类型</param>
        /// <param name="content">内容</param>
        private void SendMessage(string type,string content)
        {
            if("Text".Equals(type))
            {
                Task.Run(async() =>
                {
                    string s = TransmissionData.DataPackaging(content, ID.Text, "Text");
                    await UDP_Util.UdpSend(s);
                });
            }
        }

        private void send_Click(object sender, RoutedEventArgs e)
        {
            SendMessage("Text",input.Text);
        }

        /// <summary>
        /// 文件/图片拖拽发送
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void input_DragEnter(object sender, DragEventArgs e)
        {
            string[] Value = (string[])e.Data.GetData("FileName");
            if(new FileInfo(Value[0]).Extension.ToLower().Equals(".jpg")
                ||new FileInfo(Value[0]).Extension.ToLower().Equals(".png"))
            {
                MessageBoxResult result = MessageBox.Show("选\"是\"以图片形式发送否则以文件形式发送","发送",MessageBoxButton.YesNoCancel,MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                    SendMessage("Picture", sender.ToString());
                else if (result == MessageBoxResult.No)
                    SendMessage("File", sender.ToString());
            }
            else
            {
                SendMessage("File", sender.ToString());
            }
        }

    }
}
