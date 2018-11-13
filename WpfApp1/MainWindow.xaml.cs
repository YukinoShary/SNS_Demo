using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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

namespace WpfApp1
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public static MainWindow mw;
        private Dictionary<string, List<asdasdasd.Receive>> navigateTable = new Dictionary<string, List<asdasdasd.Receive>>();              //存储每个聊天页面的内容
        private Dictionary<string,ListViewItem> listContent = new Dictionary<string, ListViewItem>();                  //联系人列表数据
        private Thickness thickness = new Thickness(85, 5, 5, 5);

        public MainWindow()
        { 
            InitializeComponent();
            mw=this;
            TransmissionData.Getuuid();

            navigateTable.Add("All", new List<asdasdasd.Receive>());
            listContent.Add("All", CreatItem("All", 0));

            Task.Run(async () =>
            {
                await UDP_Util.UdpSocket();
            });
            HelloWord();
        }

        /// <summary>
        /// 页面跳转
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void messageList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //事件触发导航至聊天页同时传递聊天内容
            ListViewItem selected = (ListViewItem)messageList.SelectedItem;
            asdasdasd page = new asdasdasd(selected.Name, navigateTable[selected.Name]);
            main.Navigate(page); 
        }


        private async void HelloWord()
        {
            string result = TransmissionData.DataPackaging("Hello World", "All", "Text");
            await UDP_Util.UdpSend(result);
        }

        /// <summary>
        /// 新建一个listviewitem
        /// </summary>
        /// <param name="item"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        private ListViewItem CreatItem(string item, int count)
        {
            //设置网格布局
            Grid grid = new Grid();
            ColumnDefinition cd1 = new ColumnDefinition();
            ColumnDefinition cd2 = new ColumnDefinition();
            grid.ColumnDefinitions.Add(cd1);
            grid.ColumnDefinitions.Add(cd2);
            cd1.Width = new GridLength(160);
            cd2.Width = new GridLength(30);
            //设置控件
            TextBlock name = new TextBlock();
            name.Name = item + "name";
            name.Text = item;
            name.FontSize = 18;
            TextBlock counter = new TextBlock();
            counter.Name = item + "Count";
            counter.Margin = thickness;
            counter.Width = 20;
            counter.FontSize = 16;
            counter.Text = count.ToString();
            counter.Margin = new Thickness(0, 5, 5, 5);

            name.SetValue(Grid.ColumnProperty, 0);
            counter.SetValue(Grid.ColumnProperty, 1);
            grid.Children.Add(name);
            grid.Children.Add(counter);
            ListViewItem i = new ListViewItem();
            i.Name = item;
            i.Content = grid;
            return i;
        }

        private void listUpdate()
        {
            messageList.Items.Clear();
            foreach(KeyValuePair<string,ListViewItem> v in listContent)
            {
                messageList.Items.Add(v.Value);
            }
        }

        /// <summary>
        /// 指定聊天项消息+1(重写item)
        /// </summary>
        /// <param name="item">指定聊天项</param>
        /// <param name="count"></param>
        public void ItemCountAdd(string item, int count)
        {
            mw.Dispatcher.Invoke(() =>
            {
                listContent[item] = CreatItem(item, count);
                listUpdate();
            }); 
        }

        /// <summary>
        /// 创建新的聊天项
        /// </summary>
        /// <param name="item">为私聊对象uuid或者群聊群组名</param>
        public void NewItemAdd(string item)
        {
            mw.Dispatcher.Invoke(() =>
            {
                listContent.Add(item, CreatItem(item,1));
                listUpdate();
                List<asdasdasd.Receive> l = new List<asdasdasd.Receive>();
                navigateTable.Add(item, l);
            });
        }

        /// <summary>
        /// 建立新聊天按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NewChat_Click(object sender, RoutedEventArgs e)
        {
            AddNew an = new AddNew();
            an.Show();
        }

        /// <summary>
        /// 在主线程同步页面导航表的数据
        /// </summary>
        /// <param name="receive"></param>
        /// <param name="source"></param>
        public void navigateTableUpdate(asdasdasd.Receive receive,string source)
        {
            mw.Dispatcher.Invoke(() => 
            {
                if(navigateTable.ContainsKey(source))
                {                    
                    navigateTable[source].Add(receive);
                }
                else
                {
                    List<asdasdasd.Receive> li = new List<asdasdasd.Receive>();
                    li.Add(receive);
                    navigateTable.Add(source, li);
                }               
            });
        }

        /// <summary>
        /// 获取聊天页面的消息数
        /// </summary>
        /// <returns></returns>
        public int getNavigateTableCount(string page)
        {
            return navigateTable[page].Count;
        }

        /// <summary>
        /// 获取单个聊天页面内容
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        public List<asdasdasd.Receive> getPageContent(string page)
        {
            return navigateTable[page];
        }

        /// <summary>
        /// 确认自己有该群组的聊天项
        /// </summary>
        /// <param name="GroupName">群组名</param>
        /// <returns></returns>
        public bool CheckExist(string GroupName)
        {
            if (messageList.FindName(GroupName) != null)
                return true;
            else
                return false;
        }

    }
}
 