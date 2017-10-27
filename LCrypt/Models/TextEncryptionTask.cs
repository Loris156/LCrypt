using System.ComponentModel;
using System.Security;
using LCrypt.EncryptionAlgorithms;
using LCrypt.TextEncodings;
using LCrypt.ViewModels;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using LCrypt.Interfaces;
using LCrypt.Utility;
using LCrypt.Utility.Extensions;

namespace LCrypt.Models
{
    public class TextEncryptionTask : NotifyPropertyChanged, IDataErrorInfo, ITask
    {
        private string _input, _output;
        private ITextEncoding _textEncoding;
        private IEncryptionAlgorithm _hashAlgorithm;
        private SecureString _password;
        private bool _encrypt;

        private bool _isRunning;

        public TextEncryptionTask()
        {
            Encrypt = true;
        }

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

        public IEncryptionAlgorithm EncryptionAlgorithm
        {
            get => _hashAlgorithm;
            set => SetAndNotify(ref _hashAlgorithm, value);
        }

        public SecureString Password
        {
            get => _password;
            set => SetAndNotify(ref _password, value);
        }

        public bool Encrypt
        {
            get => _encrypt;
            set
            {
                SetAndNotify(ref _encrypt, value);
                OnPropertyChanged(nameof(Input));
            } 
        }

        public bool IsRunning
        {
            get => _isRunning;
            set => SetAndNotify(ref _isRunning, value);
        }

        public CancellationTokenSource CancellationTokenSource { get; set; }

        public CancellationToken CancellationToken => CancellationTokenSource.Token;

        public string this[string columnName]
        {
            get
            {
                switch (columnName)
                {
                    case nameof(Input):
                        if (!Encrypt && !Input.IsHex())
                        {
                            Output = null;
                            return (string)App.LocalizationDictionary["InputIsNotHexadecimal"];
                        }
                        break;
                    default:
                        return null;
                }

                return null;
            }
        }

        public string Error => null;
    }
}
