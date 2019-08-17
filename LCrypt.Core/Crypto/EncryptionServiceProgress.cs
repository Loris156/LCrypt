using System;
using System.Collections.Generic;
using System.Text;

namespace LCrypt.Core.Crypto
{
    public class EncryptionServiceProgress
    {
        public long ProcessedBytes { get; set; }

        public double BytesPerSecond { get; set; }
    }
}
