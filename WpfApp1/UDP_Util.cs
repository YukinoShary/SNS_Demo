using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WpfApp1
{
    public class UDP_Util
    {
        //http://www.cnblogs.com/gaochundong/archive/2013/04/14/csharp_udp_datagram_splitter.html
        //https://docs.microsoft.com/zh-cn/dotnet/api/system.net.sockets.socketasynceventargs?view=netframework-4.7.2
        //https://blog.csdn.net/Andrewniu/article/details/72469023

        private static IPAddress ip = IPAddress.Parse("127.0.0.1");
        private static IPEndPoint point = new IPEndPoint(ip,18888);
        private static int receiveBufferSize = 1470;   // buffer size to use for each socket I/O operation 
        private static Socket receiveSocket;
        private static SocketAsyncEventArgs readWriteEventArg;
        private static BufferManager bufferManager;
        private static SocketAsyncEventArgsPool readWritePool;
        private static int totalBytesRead;
        private static Dictionary<string,List<UdpPacket>> receivePackets;

        private static int numConnections = 5;  // the maximum number of connections the sample is designed to handle simultaneously 
        private static Semaphore maxNumberAcceptedClients;




        /// <summary>
        /// 发送Udp数据包
        /// </summary>
        /// <param name="result">发送的内容</param>
        public static async Task UdpSend(string result)
        {
            try
            {
                UdpClient udpclient = new UdpClient();
                Socket sc = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                sc.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, 1);
                sc.Bind(point);
                udpclient.Client = sc;
                byte[] sendBytes = Encoding.UTF8.GetBytes(result);
                ICollection<UdpPacket>udpPackets = UdpSplit(sendBytes, 1470);
                foreach(var udpPacket in udpPackets)
                {
                    byte[] ready2Send = Serialize(udpPacket);
                    //Encoding.UTF8.GetBytes(BitConverter.ToString(ready2Send));
                    string debug = BitConverter.ToString(ready2Send);
                    udpclient.SendAsync(ready2Send, ready2Send.Length, point);
                }
                
                udpclient.Close();
            }
            catch(Exception e)
            {
                throw e;
            }
        }

        /// <summary>
        /// 监听事件初始化以及线程池分配
        /// </summary>
        private static void Init()
        {
            // Allocates one large byte buffer which all I/O operations use a piece of.  This gaurds 
            // against memory fragmentation
            bufferManager.InitBuffer();
           
            for (int i = 0; i < numConnections; i++)
            {
                //Pre-allocate a set of reusable SocketAsyncEventArgs
                readWriteEventArg = new SocketAsyncEventArgs();
                readWriteEventArg.UserToken = new AsyncUserToken();
                readWriteEventArg.RemoteEndPoint = point;
                readWriteEventArg.Completed += new EventHandler<SocketAsyncEventArgs>(IO_Completed);
                
                // assign a byte buffer from the buffer pool to the SocketAsyncEventArg object
                bufferManager.SetBuffer(readWriteEventArg);

                // add SocketAsyncEventArg to the pool
                readWritePool.Push(readWriteEventArg);
            }
        }

        /// <summary>
        /// 初始化Socket
        /// </summary>
        /// <returns></returns>
        public static async Task UdpSocket()
        {
            totalBytesRead = 0;
            maxNumberAcceptedClients = new Semaphore(numConnections, numConnections);
            bufferManager = new BufferManager(receiveBufferSize * numConnections, receiveBufferSize);
            readWritePool = new SocketAsyncEventArgsPool(numConnections);

            receiveSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            receiveSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, 1);
            receiveSocket.Bind(point);           
            //receiveSocket.Listen(numConnections);

            Init();
            StartReceive();
        }


        private static void StartReceive()
        {
            if (!receiveSocket.ReceiveFromAsync(readWriteEventArg))
                ProcessReceive(readWriteEventArg);
        }

        /*private static void ProcessAccept(SocketAsyncEventArgs e) UDP不需要建立连接
        {
            Interlocked.Increment(ref numConnectedSockets);         
            SocketAsyncEventArgs readEventArgs = readWritePool.Pop();
            ((AsyncUserToken)readEventArgs.UserToken).Socket = e.AcceptSocket;

            try
            {
                // As soon as the client is connected, post a receive to the connection
                bool willRaiseEvent = e.AcceptSocket.ReceiveAsync(readEventArgs);
                if (!willRaiseEvent)
                {
                    ProcessReceive(readEventArgs);
                }
            }
            catch(Exception exc)
            {
                throw (exc);
            }

            // Accept the next connection request .Release the resource
            StartAccept(e);

        }*/  

    // This method is called whenever a receive or send operation is completed on a socket 
    //
    // <param name="e">SocketAsyncEventArg associated with the completed receive operation</param>
    private static void IO_Completed(object sender, SocketAsyncEventArgs e)
    {
        // determine which type of operation just completed and call the associated handler
        switch (e.LastOperation)
        {
            case SocketAsyncOperation.ReceiveFrom:
                ProcessReceive(e);
                break;
            default:
                throw new ArgumentException("The last operation completed on the socket was not a receive or send");
        }       

    }

        /// <summary>
        /// 接收Udp包
        /// </summary>
        /// <param name="e"></param>
        private static void ProcessReceive(SocketAsyncEventArgs e)
        {
            // check if the remote host closed th e connection  
            AsyncUserToken token = (AsyncUserToken)e.UserToken;
            if (e.BytesTransferred>0 && e.SocketError == SocketError.Success)
            {
                try
                {
                    byte[] receive = new byte[receiveBufferSize];                                    //以太网单个数据帧最大为1472
                    Interlocked.Add(ref totalBytesRead, e.BytesTransferred);
                    e.SetBuffer(receive, 0, receive.Length); 
                    if(e.Count!=0)
                    {
                        PacketHandle(receive);
                    }                   
                }
                catch (Exception ex)
                {
                    Debug.Write(ex);
                }
            }
            StartReceive();
        }

        /// <summary>
        /// UdpPacket处理
        /// </summary>
        /// <param name="receiveByte"></param>
        private static void PacketHandle(byte[] receiveByte)
        {
            /*if (receiveByte.Length != Marshal.SizeOf(typeof(UdpPacket)))
            {
                throw new ArgumentException("receiveByte参数与UdpPacket参数字节长度不一致");
            }

            //将byte[]类数据还原为UdpPacket类
            IntPtr bufferHandler = Marshal.AllocHGlobal(receiveByte.Length);                    
            for (int index = 0; index < receiveByte.Length; index++)
            {
                Marshal.WriteByte(bufferHandler, index, receiveByte[index]);
            }
            UdpPacket Packet = (UdpPacket)Marshal.PtrToStructure<UdpPacket>(bufferHandler);
            Marshal.FreeHGlobal(bufferHandler);*/

            string s = Encoding.UTF8.GetString(receiveByte);
            UdpPacket Packet = Deserialize(Encoding.Default.GetBytes(s));
            //判断Udp数据文件是否完整
            if(Packet.Amount == 1)
            {
                TransmissionData.DataUnpackaging(Packet.Chunk.ToString());                          //当只有一个包时直接返回
            }
            else
            {
                if(receivePackets.ContainsKey(Packet.Sequence))
                {
                    receivePackets.Add(Packet.Sequence, new List<UdpPacket>());            //当接收到数据包集合的第一个包时建立新的链表      
                }
                receivePackets[Packet.Sequence].Add(Packet);
                if (receivePackets[Packet.Sequence][1].Amount==receivePackets[Packet.Sequence].Count)   //数据包集合传输完成
                {
                    receivePackets[Packet.Sequence].Sort();
                    string result = "";
                    foreach(var rs in receivePackets[Packet.Sequence])
                    {
                        result += rs.Chunk.ToString();
                    }
                    TransmissionData.DataUnpackaging(result);
                } 
            }
        }


        /// <summary>
        /// 分割Udp数据包
        /// </summary>
        /// <param name="datagram"></param>
        /// <param name="chunkLength"></param>
        /// <returns></returns>
        private static ICollection<UdpPacket> UdpSplit(byte[] datagram,int chunkLength)
        {
            string sequence = Guid.NewGuid().ToString();
            List<UdpPacket> packets = new List<UdpPacket>();
            int chunks = datagram.Length / chunkLength;                           //数据包数量
            int remainder = datagram.Length % chunkLength;
            //remainder为余下的数据，若不为0，则多加一个数据包
            if (remainder > 0)                                                    
                chunks++;
            //循环分包
            for (int i = 0; i < chunks - 1; i++)
            {
                byte[] chunk = new byte[chunkLength];                             //初始化单数据包
                Buffer.BlockCopy(datagram, i * chunkLength, chunk, 0, chunkLength);
                packets.Add(new UdpPacket(sequence, chunks, i, chunk, chunkLength));
            }
            if(remainder > 0)
            {
                int length = remainder;
                byte[] chunk = new byte[length];
                Buffer.BlockCopy(datagram, chunkLength * (--chunks), chunk, 0, length);
                packets.Add(new UdpPacket(sequence, chunks, chunks, chunk, length));
            }
            return packets;
        }

        private static byte[] Serialize(UdpPacket p)
        {
            byte[] buff;
            using (MemoryStream ms = new MemoryStream())
            {
                IFormatter formatter = new BinaryFormatter();
                formatter.Serialize(ms, p);
                buff = ms.ToArray();
                return buff;
            }
        }

        private static UdpPacket Deserialize(byte[] b)
        {
            UdpPacket p;
            using (MemoryStream ms = new MemoryStream(b))
            {
                IFormatter formatter = new BinaryFormatter();
                ms.Position = 0;
                p = (UdpPacket)formatter.Deserialize(ms);
            }
            return p;
        }
    }
}
