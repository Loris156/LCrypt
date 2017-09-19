using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Input;
using LCrypt.Models;
using LCrypt.Utility;
using Microsoft.Win32;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using LCrypt.EncryptionAlgorithms;
using MahApps.Metro.Controls.Dialogs;

// ReSharper disable MemberCanBeMadeStatic.Global

namespace LCrypt.ViewModels
{
    public class FileEncryptionViewModel : NotifyPropertyChanged
    {
        private ObservableCollection<FileEncryptionTask> _encryptionTasks;
        private FileEncryptionTask _selectedTask;

        public FileEncryptionViewModel()
        {
            EncryptionAlgorithms = new List<IEncryptionAlgorithm>()
            {
                new Aes256(),
                new Des64(),
                new Tdea192(),
                new Rc2()
            };

            EncryptionTasks = new ObservableCollection<FileEncryptionTask>();
        }

        private IEnumerable<IEncryptionAlgorithm> _encryptionAlgorithms;
        public IEnumerable<IEncryptionAlgorithm> EncryptionAlgorithms
        {
            get => _encryptionAlgorithms;
            set => SetAndNotify(ref _encryptionAlgorithms, value);
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
                }, _ => SelectedTask != null && !SelectedTask.IsRunning);
            }
        }

        public ICommand SetDestinationPathCommand
        {
            get
            {
                return new RelayCommand(t =>
                {
                    Debug.Assert(t != null);
                    var task = (FileEncryptionTask)t;
                    var dialog = new SaveFileDialog();
                    if (dialog.ShowDialog().GetValueOrDefault())
                    {
                        task.DestinationPath = dialog.FileName;
                    }
                });
            }
        }

        public ICommand StartStopEncryptionTask
        {
            get
            {
                return new RelayCommand(async t =>
                {
                    Debug.Assert(t != null);
                    var task = (FileEncryptionTask)t;
                    Debug.Assert(task.Algorithm != null);

                    var fileEncryption = new FileEncryption();
                    var progress = new Progress<long>(p => task.Progress = p);
                    try
                    {
                        if (task.Encrypt)
                            await fileEncryption.EncryptFile(task, progress, task.CancellationToken);
                        else
                            await fileEncryption.DecryptFile(task, progress, task.CancellationToken);
                    }
                    catch (Exception e)
                    {
                        //TODO: Catch specified exceptions
                        Console.WriteLine(e);
                        throw;
                    }

                },
                t =>
                {
                    Debug.Assert(t != null);
                    var task = (FileEncryptionTask)t;

                    return !string.IsNullOrWhiteSpace(task.DestinationPath) && task.FileInfo != null &&
                           task.Password?.Length > 0 && task.Algorithm != null;

                });
            }
        }

        #endregion
    }
}