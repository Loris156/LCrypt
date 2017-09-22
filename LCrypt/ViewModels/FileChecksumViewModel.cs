using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using LCrypt.Models;
using System.Collections.ObjectModel;
using Dragablz;

namespace LCrypt.ViewModels
{
    public class FileChecksumViewModel : NotifyPropertyChanged
    {
        private ObservableCollection<FileChecksumTask> _checksumTasks;

        public FileChecksumViewModel()
        {
            ChecksumTasks = new ObservableCollection<FileChecksumTask>
            {
                new FileChecksumTask()
            };
        }

        public ICollection<FileChecksumTask> ChecksumTasks
        {
            get => _checksumTasks;
            set => SetAndNotify(ref _checksumTasks, (ObservableCollection<FileChecksumTask>)value);
        }

        public Func<FileChecksumTask> NewItemFactory => () => new FileChecksumTask();
    }
}
