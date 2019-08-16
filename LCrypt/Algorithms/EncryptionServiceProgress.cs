using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LCrypt.Algorithms
{
    public class EncryptionServiceProgress
    {
        public long ProcessedBytes { get; set; }

        public double MibPerSecond { get; set; } 
    }
}
