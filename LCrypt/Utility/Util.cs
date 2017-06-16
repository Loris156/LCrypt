using System.Drawing;

namespace LCrypt.Utility
{
    public class Util
    {
        public static Icon GetIconByFilename(string path)
        {
            return Icon.ExtractAssociatedIcon(path);
        }
    }
}
