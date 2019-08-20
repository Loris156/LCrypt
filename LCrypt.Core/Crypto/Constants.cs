using System;
using System.Collections.Generic;
using System.Text;

namespace LCrypt.Core.Crypto
{
    internal static class Constants
    {
        public const int Pbkdf2Iterations = 120000;
        public const int SaltLength = 16;
        public const int FileBufferSize = 131072;

        public const int ReportIntervalMs = 250;

        ///////////////////////////////////////////////////////// L   C    r    y    p    t //
        public static readonly byte[] MagicHeader = new byte[] { 76, 67, 114, 121, 112, 116 };
    }
}
