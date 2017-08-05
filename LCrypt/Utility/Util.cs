using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;

namespace LCrypt.Utility
{
    public static class Util
    {
        public static Icon GetIconByFilename(string path)
        {
            return Icon.ExtractAssociatedIcon(path);
        }

        //Modified code from .NET Reference source
        public static async Task<byte[]> ReadAllBytesAsync(string path)
        {
            byte[] bytes;
            using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true))
            {
                var index = 0;
                var fileLength = fs.Length;
                if (fileLength > Int32.MaxValue)
                    throw new IOException("File is larger than 2GB.");
                var count = (int)fileLength;
                bytes = new byte[count];
                while (count > 0)
                {
                    var n = await fs.ReadAsync(bytes, index, count);
                    if (n == 0)
                        throw new EndOfStreamException("Read beyond 2GB.");
                    index += n;
                    count -= n;
                }
            }
            return bytes;
        }
    }
}
