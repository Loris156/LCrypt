using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Windows.Media;
using LCrypt.Interfaces;
using LCrypt.Utility;
using LCrypt.ViewModels;

namespace LCrypt.Models
{
    public class AlgorithmTask : NotifyPropertyChanged, ITask
    {
        private bool _isRunning;
        private string _result;
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

    public class FileChecksumTask : NotifyPropertyChanged
    {
        private ImageSource _fileIcon;
        private FileInfo _fileInfo;

        private string _verification;
        private bool _verified;

        public FileChecksumTask()
        {
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

                if (FileInfo != null)
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

        public string FileDirectory => _fileInfo?.DirectoryName;

        private long? FileSize => _fileInfo?.Length;

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
    }
}
