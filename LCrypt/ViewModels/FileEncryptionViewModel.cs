using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Input;
using LCrypt.Models;
using LCrypt.Utility;
using Microsoft.Win32;

// ReSharper disable MemberCanBeMadeStatic.Global

namespace LCrypt.ViewModels
{
    public class FileEncryptionViewModel : NotifyPropertyChanged
    {
        private ObservableCollection<FileEncryptionTask> _encryptionTasks;

        public FileEncryptionViewModel()
        {
            EncryptionTasks = new ObservableCollection<FileEncryptionTask>();
        }

        public ICollection<FileEncryptionTask> EncryptionTasks
        {
            get => _encryptionTasks;
            set => SetAndNotify(ref _encryptionTasks, (ObservableCollection<FileEncryptionTask>)value);
        }

        #region Commands

        public ICommand AddTaskCommand
        {
            get
            {
                return new RelayCommand(_ =>
                {
                    var dialog = new OpenFileDialog();
                    if (dialog.ShowDialog().GetValueOrDefault())
                    {
                        EncryptionTasks.Add(new FileEncryptionTask(dialog.FileName));
                    }

                });
            }
        }

        #endregion
    }
}