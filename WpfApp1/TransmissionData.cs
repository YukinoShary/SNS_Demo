using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace WpfApp1
{
    public class TransmissionData
    {
        private static string uuid = "";

        /// <summary>
        /// 初始化程序的uuid
        /// </summary>
        private static void BuildUuid()
        {
            Guid g = Guid.NewGuid();
            uuid = g.ToString();
        }

        /// <summary>
        /// 封装文本内容以及信息并序列化为JSON
        /// </summary>
        /// <param name="content">文本内容</param>
        /// <param name="target">目标uuid</param>
        /// <returns></returns>
        public static string DataPackaging(string content,string target,string contentType)
        {

            Dictionary<string, string> package = new Dictionary<string, string>();
            package.Add("Source", Getuuid());
            package.Add("ContentType", contentType);
            package.Add("Content", content);
            package.Add("Target", target);
            string serializeText = JsonConvert.SerializeObject(package);          //序列化字典类型的数据集合为JSON
            return serializeText;
        }

        /// <summary>
        /// 解包并用MessagePush推送消息
        /// </summary>
        /// <param name="Text"></param>
        /// <returns></returns>
        public static bool DataUnpackaging(string Text)
        {
            Dictionary<string,string> Unpackaging = JsonConvert.DeserializeObject<Dictionary<string, string>>(Text);
            //若包的目标uuid或者源uuid与本客户端对应则推送消息(本客户端为接收端或发送端时都应显示)
            if (Getuuid().Equals(Unpackaging["Target"])||Getuuid().Equals(Unpackaging["Source"]))
            {
                MessagePush.Push(Unpackaging["ContentType"], Unpackaging["Source"], Unpackaging["Content"], Unpackaging["Target"], false);
                return true;
            }
            //若包的目标群组名存在于本客户端的聊天列表则推送消息
            else if(MainWindow.mw.CheckExist(Unpackaging["Target"]))
            {
                MessagePush.Push(Unpackaging["ContentType"], Unpackaging["Source"], Unpackaging["Content"], Unpackaging["Target"], true);
                return true;
            }
            else
                return false;
        }

        /// <summary>
        /// 获取本程序的uuid
        /// </summary>
        /// <returns></returns>
        public static string Getuuid()
        {
            if (uuid == "")
            {
                BuildUuid();
            }
            return uuid;
        }


    }
}
