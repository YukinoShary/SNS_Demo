using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace WpfApp1
{
    class MessagePush
    {
        private static Dictionary<string,int> messageCounters;         //每个聊天对象的未读消息数
        private static int bufferCount = 0;
        private static string SaveFileDirectory = "d:\\example"+TransmissionData.Getuuid()+"\\fil";
        private static string SavePicDirectory = "d:\\example"+TransmissionData.Getuuid()+"\\pic";
        private static readonly object countersLock=new object ();

        MessagePush()
        {
            Directory.CreateDirectory(SaveFileDirectory);
            Directory.CreateDirectory(SavePicDirectory);
        }

        /// <summary>
        /// 消息推送
        /// </summary>
        /// <param name="type">消息类型</param>
        /// <param name="source">消息来源</param>
        /// <param name="content">消息内容</param>
        /// <param name="target">消息目标</param>
        /// <param name="isgroup">是否为群组聊天</param>
        public static void Push(string type,string source,string content,string target,bool isgroup)
        {
            switch (isgroup)                   
            {
                case false:
                    SingleMessage(type,source,content);
                    break;
                case true:
                    GroupMessage(type,source,content,target);
                    break;
            }
        }

        /// <summary>
        /// 私聊消息处理
        /// </summary>
        /// <param name="type"></param>
        /// <param name="source"></param>
        /// <param name="content"></param>
        private static void SingleMessage(string type,string source,string content)
        {
            lock(countersLock)
            {
                if (MainWindow.mw.messageList.Items.IndexOf(source) == 0)      //若来源客户端的聊天项已存在
                {
                    //对象的未读消息+1
                    messageCounters[source] += 1;
                    MainWindow.mw.ItemCountAdd(source,messageCounters[source]);
                }
                else
                {
                    messageCounters.Add(source, 1);
                    MainWindow.mw.NewItemAdd(source);
                }
            }
            

            //将消息加入页面类对象中的消息队列
            asdasdasd.Receive receive = new asdasdasd.Receive();
            receive.Type = type;

            if (type.Equals("Text"))               //判断消息类型，如果是Text则直接保存入Receive结构中
            {               
                receive.Content = content;
                receive.Source = source;
            }
            else                                   //如果消息不是Text，则将其保存在本地硬盘中，并将其地址存入Receive结构
            {
                receive.Content = SaveToLocal(content, type);
            }

            MainWindow.mw.navigateTableUpdate(receive, source);
        }
        
        /// <summary>
        /// 群组消息处理
        /// </summary>
        /// <param name="type"></param>
        /// <param name="source"></param>
        /// <param name="content"></param>
        /// <param name="target"></param>
        private static void GroupMessage(string type, string source, string content,string target)
        {
            //对象群组的未读消息+1
            lock(countersLock)
            {
                messageCounters[target] += 1;
            }            
            MainWindow.mw.ItemCountAdd(target, messageCounters[target]);

            //新消息内容封装入链表
            asdasdasd.Receive r = new asdasdasd.Receive();
            r.Source = source;

            if (type.Equals("Text"))
            {               
                r.Content = content;
            }
            else
            {
                r.Content = SaveToLocal(content, type);    
            }
            r.Type = type;
            MainWindow.mw.navigateTableUpdate(r, target);
        }
                
        /// <summary>
        /// 将文件缓存在本地，返回值为文件地址
        /// </summary>
        /// <param name="save"></param>
        /// <param name="type"></param>
        private static string SaveToLocal(string save,string type)
        {
            string path = SavePicDirectory + bufferCount + "." + type;
            Interlocked.Increment(ref bufferCount); 
            using (FileStream fileStream = File.Create(path))
            {
                byte[] fb = Encoding.Default.GetBytes(save);
                fileStream.WriteAsync(fb, 0, fb.Length);
                return path;
            }
        }

        /// <summary>
        /// counter计数器新项目
        /// </summary>
        /// <param name="itemName"></param>
        public static void newCounter(string itemName)
        {
            messageCounters.Add(itemName, 0);
        }
    }
}
