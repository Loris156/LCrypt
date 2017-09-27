using System.Text;

namespace LCrypt.TextEncodings
{
    public interface ITextEncoding
    {
        string DisplayName { get; }
        Encoding Create();
    }
}
