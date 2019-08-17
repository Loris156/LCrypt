using System;
using System.Collections.Generic;
using System.Text;

namespace LCrypt.Core.Crypto
{
    public class FileHeaderV1
    {
        public int Pbkdf2Iterations { get; set; }

        public byte[] Salt { get; set; }

        public byte[] Iv { get; set; }
    }
}
