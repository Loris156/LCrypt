using System;
using System.IO;
using System.Windows.Media;
using LCrypt.Utility;
using LCrypt.ViewModels;
using System.Diagnostics;

namespace LCrypt.Models
{
    public class FileChecksumTask : NotifyPropertyChanged, IEquatable<FileChecksumTask>
    {
        private readonly Guid _guid;

        private FileInfo _fileInfo;
        private ImageSource _fileIcon;

        private string _md5Result, _crc32Result, _sha1Result, _sha256Result, _sha384Result, _sha512Result, _whirlpoolResult;
        private string _verification;
        private bool _verified;

        public FileChecksumTask()
        {
            _guid = Guid.NewGuid();
            FileInfo = new FileInfo(@"C:\users\lole\desktop\contacts.xml");
            Verified = true;
        }

        
        public FileInfo FileInfo
        {
            get => _fileInfo;
            set
            {
                SetAndNotify(ref _fileInfo, value);

                FileIcon = Util.ExtractFileIcon(FileInfo.FullName);

                OnPropertyChanged(nameof(FileName));
                OnPropertyChanged(nameof(FilePath));
                OnPropertyChanged(nameof(FileDirectory));
                OnPropertyChanged(nameof(FileSize));
                OnPropertyChanged(nameof(FileSizeString));
                OnPropertyChanged(nameof(FileIcon));
            }
        }

        public string FileName => _fileInfo?.Name;

        public string FilePath => _fileInfo?.FullName;

        public string FileDirectory => _fileInfo.DirectoryName;

        public long? FileSize => _fileInfo?.Length;

        public string FileSizeString
        {
            get
            {
                if (!FileSize.HasValue) return null;

                double adaptedSize;
                string unit;
                if (FileSize < 1024)
                {
                    adaptedSize = FileSize.Value;
                    unit = "Byte";
                }
                else if (FileSize < 1048576)
                {
                    adaptedSize = FileSize.Value / 1024d;
                    unit = "KiB";
                }
                else if (FileSize < 1073741824)
                {
                    adaptedSize = FileSize.Value / 1048576d;
                    unit = "MiB";
                }
                else
                {
                    adaptedSize = FileSize.Value / 1073741824d;
                    unit = "GiB";
                }

                adaptedSize = Math.Round(adaptedSize, 2);
                return $"{adaptedSize} {unit}";
            }
        }

        public ImageSource FileIcon
        {
            get => _fileIcon;
            set => SetAndNotify(ref _fileIcon, value);
        }

        public string Md5Result
        {
            get => _md5Result;
            set => SetAndNotify(ref _md5Result, value);
        }

        public string Crc32Result
        {
            get => _crc32Result;
            set => SetAndNotify(ref _crc32Result, value);
        }

        public string Sha1Result
        {
            get => _sha1Result;
            set => SetAndNotify(ref _sha1Result, value);
        }

        public string Sha256Result
        {
            get => _sha256Result;
            set => SetAndNotify(ref _sha256Result, value);
        }

        public string Sha384Result
        {
            get => _sha384Result;
            set => SetAndNotify(ref _sha384Result, value);
        }

        public string Sha512Result
        {
            get => _sha512Result;
            set => SetAndNotify(ref _sha512Result, value);
        }

        public string WhirlpoolResult
        {
            get => _whirlpoolResult;
            set => SetAndNotify(ref _whirlpoolResult, value);
        }

        public string Verification
        {
            get => _verification;
            set => SetAndNotify(ref _verification, value); // TODO
        }

        public bool Verified
        {
            get => _verified;
            set => SetAndNotify(ref _verified, value);
        }

        public override string ToString()
        {
            return FileName;
        }

        public bool Equals(FileChecksumTask other)
        {
            if (ReferenceEquals(null, other)) return false;
            return ReferenceEquals(this, other) || _guid.Equals(other._guid);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((FileChecksumTask) obj);
        }

        public override int GetHashCode()
        {
            return _guid.GetHashCode();
        }
    }
}
