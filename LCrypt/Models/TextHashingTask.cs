using LCrypt.HashAlgorithms;
using LCrypt.TextEncodings;
using LCrypt.ViewModels;
using System.Security.Cryptography;
using System.Text;
using LCrypt.Utility;

namespace LCrypt.Models
{
    public class TextHashingTask : NotifyPropertyChanged
    {
        private string _input, _output;
        private ITextEncoding _textEncoding;
        private IHashAlgorithm _hashAlgorithm;

        public string Input
        {
            get => _input;
            set => SetAndNotify(ref _input, value);
        }

        public string Output
        {
            get => _output;
            set => SetAndNotify(ref _output, value);
        }

        public ITextEncoding TextEncoding
        {
            get => _textEncoding;
            set => SetAndNotify(ref _textEncoding, value);
        }

        public IHashAlgorithm HashAlgorithm
        {
            get => _hashAlgorithm;
            set => SetAndNotify(ref _hashAlgorithm, value);
        }

        public Encoding TextEncodingCache { get; set; }

        public HashAlgorithm HashAlgorithmCache { get; set; }
    }
}
