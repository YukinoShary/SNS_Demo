using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1
{
    class UdpPacket
    {
        public string Sequence { get; set; }
        public int Amount { get; set; }
        public byte[] Chunk { get; set; }
        public int Count { get; set; }
        public int Length { get; set; }
        public UdpPacket(string sequence, int amount, int count, byte[] chunk, int currentLength)
        {
            Sequence = sequence;
            Amount = amount;
            Chunk = chunk;
            Length = currentLength;
            Count = count; 
        }

    }
}
