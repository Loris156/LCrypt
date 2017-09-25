using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Media;
using LCrypt.Utility;
using LCrypt.ViewModels;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using LCrypt.HashAlgorithms;

namespace LCrypt.Models
{
    public class AlgorithmTask : NotifyPropertyChanged
    {
        private string _result;
        private bool _isRunning;
        public CancellationTokenSource CancellationTokenSource { get; set; }

        public string Result
        {
            get => _result;
            set => SetAndNotify(ref _result, value);
        }

        public bool IsRunning
        {
            get => _isRunning;
            set => SetAndNotify(ref _isRunning, value);
        }
    }


    public class FileChecksumTask : NotifyPropertyChanged, IEquatable<FileChecksumTask>
    {
        private readonly Guid _guid;

        private FileInfo _fileInfo;
        private ImageSource _fileIcon;

        private string _verification;
        private bool _verified;

        public FileChecksumTask()
        {
            _guid = Guid.NewGuid();
            Tasks = new Dictionary<string, AlgorithmTask>(7)
            {
                {"MD5", new AlgorithmTask()},
                {"CRC32", new AlgorithmTask()},
                {"SHA-1", new AlgorithmTask()},
                {"SHA-256", new AlgorithmTask()},
                {"SHA-384", new AlgorithmTask()},
                {"SHA-512", new AlgorithmTask()},
                {"Whirlpool", new AlgorithmTask()}
            };
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

        public Dictionary<string, AlgorithmTask> Tasks { get; set; }

        public string Verification
        {
            get => _verification;
            set
            {
                SetAndNotify(ref _verification, value);

                foreach (var task in Tasks)
                {
                    if (task.Value.Result == null) continue;
                    if (task.Value.Result.ToLowerInvariant().Equals(Verification.ToLowerInvariant()))
                    {
                        Verified = true;
                        return;
                    }
                    Verified = false;
                }
            }
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
            return obj.GetType() == GetType() && Equals((FileChecksumTask)obj);
        }

        public override int GetHashCode()
        {
            return _guid.GetHashCode();
        }
    }
}
