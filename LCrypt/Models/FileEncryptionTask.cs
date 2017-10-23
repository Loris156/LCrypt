using LCrypt.Utility;
using LCrypt.ViewModels;
using System;
using System.IO;
using System.Security;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using LCrypt.EncryptionAlgorithms;
using LCrypt.Interfaces;

namespace LCrypt.Models
{
    public class FileEncryptionTask : NotifyPropertyChanged, IEquatable<FileEncryptionTask>, ITaskWithProgress
    {
        private readonly Guid _guid;
        private FileInfo _fileInfo;

        public FileEncryptionTask(string path)
        {
            _guid = Guid.NewGuid();
            _fileInfo = new FileInfo(path);
            _fileIcon = Util.ExtractFileIcon(FilePath);
        }

        public FileInfo FileInfo
        {
            get => _fileInfo;
            set => SetAndNotify(ref _fileInfo, value);
        }

        public string FilePath => _fileInfo.FullName;

        public string FileDirectory => _fileInfo.DirectoryName;

        public string FileName => _fileInfo.Name;

        public long FileSize => _fileInfo.Length;

        public string FileSizeString
        {
            get
            {
                double adaptedSize;
                string unit;
                if (FileSize < 1024)
                {
                    adaptedSize = FileSize;
                    unit = "Byte";
                }
                else if (FileSize < 1048576)
                {
                    adaptedSize = FileSize / 1024d;
                    unit = "KiB";
                }
                else if (FileSize < 1073741824)
                {
                    adaptedSize = FileSize / 1048576d;
                    unit = "MiB";
                }
                else
                {
                    adaptedSize = FileSize / 1073741824d;
                    unit = "GiB";
                }

                adaptedSize = Math.Round(adaptedSize, 2);
                return $"{adaptedSize} {unit}";
            }
        }

        private ImageSource _fileIcon;
        public ImageSource FileIcon
        {
            get => _fileIcon;
            set => SetAndNotify(ref _fileIcon, value);
        }

        private string _destinationPath;
        public string DestinationPath
        {
            get => _destinationPath;
            set => SetAndNotify(ref _destinationPath, value);
        }

        private bool _running;
        public bool IsRunning
        {
            get => _running;
            set => SetAndNotify(ref _running, value);
        }

        private double _progress;
        public double Progress
        {
            get => _progress;
            set => SetAndNotify(ref _progress, value);
        }

        private SecureString _password;
        public SecureString Password
        {
            get => _password;
            set => SetAndNotify(ref _password, value);
        }

        private bool _encrypt = true;
        public bool Encrypt
        {
            get => _encrypt;
            set => SetAndNotify(ref _encrypt, value);
        }

        private IEncryptionAlgorithm _algorithm;
        public IEncryptionAlgorithm Algorithm
        {
            get => _algorithm; 
            set => SetAndNotify(ref _algorithm, value);
        }

        private CancellationTokenSource _cancellationTokenSource;
        public CancellationTokenSource CancellationTokenSource
        {
            get => _cancellationTokenSource;
            set => SetAndNotify(ref _cancellationTokenSource, value);
        }

        public CancellationToken CancellationToken => CancellationTokenSource.Token;

        private Task _task;
        public Task Task
        {
            get => _task;
            set => SetAndNotify(ref _task, value);
        }

        private bool _finished;
        public bool IsFinished
        {
            get => _finished;
            set => SetAndNotify(ref _finished, value);
        }


        public override string ToString()
        {
            return FileName;
        }

        public bool Equals(FileEncryptionTask other)
        {
            if (ReferenceEquals(null, other)) return false;
            return ReferenceEquals(this, other) || _guid.Equals(other._guid);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == this.GetType() && Equals((FileEncryptionTask)obj);
        }

        public override int GetHashCode()
        {
            return _guid.GetHashCode();
        }
    }
}
