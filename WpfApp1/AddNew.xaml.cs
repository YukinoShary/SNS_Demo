using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace WpfApp1
{
    /// <summary>
    /// AddNew.xaml 的交互逻辑
    /// </summary>
    public partial class AddNew : Window
    {
        public AddNew()
        {
            InitializeComponent();
        }

        private void confirm_Click(object sender, RoutedEventArgs e)
        {
            if (MainWindow.mw.CheckExist(ChatName.Text))
                throw new Exception("the item is exist");
            else
            {
                if (AddContact.IsChecked == true && ChatName != null)
                {
                    MessagePush.newCounter(ChatName.Text);
                    MainWindow.mw.NewItemAdd(ChatName.Text);
                }
                else if(AddGroup.IsChecked == true && ChatName != null)
                {
                    MessagePush.newCounter(ChatName.Text);
                    MainWindow.mw.NewItemAdd(ChatName.Text);
                }
            }
            this.Close();
        }

        private void cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
