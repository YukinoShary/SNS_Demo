using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using WpfApp1;

namespace UnitTestProject1
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public async void TestMethod1()
        {
            string sendText = TransmissionData.DataPackaging("Hello World", "All", "Text");
            await UDP_Util.UdpSend(sendText);
            await System.Threading.Tasks.Task.Run(async()=>
            {
                await UDP_Util.UdpSocket();
            }) ;
            
        }
    }
}
