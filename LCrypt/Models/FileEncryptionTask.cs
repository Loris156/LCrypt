using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using LCrypt.Utility;
using LCrypt.ViewModels;

namespace LCrypt.Models
{
    public class FileEncryptionTask : NotifyPropertyChanged, IEquatable<FileEncryptionTask>
    {
        private readonly Guid _guid;
        private FileInfo _fileInfo;

        public FileEncryptionTask(string path)
        {
            _guid = Guid.NewGuid();
            _fileInfo = new FileInfo(path);
            _fileIcon = Util.ExtractFileIcon(FilePath);
        }

        public string FilePath => _fileInfo.FullName;

        public string FileDirectory => _fileInfo.DirectoryName;

        public string FileName => _fileInfo.Name;

        private long FileSize => _fileInfo.Length;

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

        private int _progress;
        public int Progress
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
