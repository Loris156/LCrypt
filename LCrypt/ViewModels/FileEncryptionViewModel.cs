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
        private FileEncryptionTask _selectedTask;

        public FileEncryptionViewModel()
        {
            EncryptionTasks = new ObservableCollection<FileEncryptionTask>();
        }

        public ICollection<FileEncryptionTask> EncryptionTasks
        {
            get => _encryptionTasks;
            set => SetAndNotify(ref _encryptionTasks, (ObservableCollection<FileEncryptionTask>)value);
        }
        
        public FileEncryptionTask SelectedTask
        {
            get => _selectedTask;
            set => SetAndNotify(ref _selectedTask, value);
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

        public ICommand DeleteSelectedTaskCommand
        {
            get
            {
                return new RelayCommand(_ =>
                {
                    EncryptionTasks.Remove(SelectedTask);
                }, _ =>
                {
                    return SelectedTask != null;
                });
            }
        }

        public ICommand SetDestinationPathCommand
        {
            get
            {
                return new RelayCommand(t =>
                {
                    var task = (FileEncryptionTask)t;
                    var dialog = new SaveFileDialog();
                    if (dialog.ShowDialog().GetValueOrDefault())
                    {
                        task.DestinationPath = dialog.FileName;
                    }
                });
            }
        }

        #endregion
    }
}