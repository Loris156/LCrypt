using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using LCrypt.Models;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using Dragablz;
using LCrypt.Utility;
using LCrypt.HashAlgorithms;
using LCrypt.Utility.Extensions;
using Microsoft.Win32;

namespace LCrypt.ViewModels
{
    public class FileChecksumViewModel : NotifyPropertyChanged
    {
        private const int FileBufferSize = 131072; // 128 KiB

        private ObservableCollection<FileChecksumTask> _checksumTasks;
        private FileChecksumTask _selectedTask;

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

        public FileChecksumTask SelectedTask
        {
            get => _selectedTask;
            set => SetAndNotify(ref _selectedTask, value);
        }

        public Func<FileChecksumTask> NewItemFactory => () => new FileChecksumTask();

        public ICommand OpenFileCommand
        {
            get
            {
                return new RelayCommand(async obj =>
                {
                    var dialog = new OpenFileDialog
                    {
                        Filter = (string)App.LocalizationDictionary["AllFiles"] + "|*.*",
                        ValidateNames = true,
                        CheckFileExists = true,
                        CheckPathExists = true                       
                    };
                    if(dialog.ShowDialog().GetValueOrDefault())
                    {
                        SelectedTask.FileInfo = new FileInfo(dialog.FileName);
                    }
                },
                obj =>
                {
                    if (SelectedTask == null) return true;
                    foreach (var task in SelectedTask.Tasks)
                    {
                        if (task.Value.IsRunning)
                            return false;
                    }
                    return true;
                });
            }
        }

        public ICommand CopyHashCommand
        {
            get
            {
                return new RelayCommand(t =>
                {
                    Debug.Assert(t != null);
                    Debug.Assert(t is string);
                    var target = (string) t;

                    if (!SelectedTask.Tasks.ContainsKey(target)) return;
                    if (SelectedTask.Tasks[(string)t].Result != null)
                        Clipboard.SetText(SelectedTask.Tasks[(string)t].Result);
                },
                t =>
                {
                    Debug.Assert(t != null);
                    Debug.Assert(t is string);
                    var target = (string)t;

                    if (SelectedTask == null || !SelectedTask.Tasks.ContainsKey(target)) return false;
                    return SelectedTask.Tasks[(string) t].Result != null;
                });
            }
        }

        public ICommand StartComputationCommand
        {
            get
            {
                return new RelayCommand(async t =>
                {
                    Debug.Assert(t != null);
                    Debug.Assert(t is string);
                    var target = (string)t;

                    if (SelectedTask.Tasks[target].IsRunning)
                    {
                        SelectedTask.Tasks[target].CancellationTokenSource.Cancel();
                        SelectedTask.Tasks[target].IsRunning = false;
                        SelectedTask.Tasks[target].Result = null;
                    }
                    else
                    {
                        SelectedTask.Tasks[target].CancellationTokenSource = new CancellationTokenSource();
                        SelectedTask.Tasks[target].IsRunning = true;

                        try
                        {
                            using (var algorithm = Util.GetHashAlgorithm(target).Create())
                            {
                                using (var fs = new FileStream(SelectedTask.FilePath, FileMode.Open, FileAccess.Read,
                                    FileShare.Read, bufferSize: FileBufferSize, useAsync: true))
                                {
                                    var hash = await algorithm.ComputeHashAsync(fs,
                                        SelectedTask.Tasks[target].CancellationTokenSource.Token).ConfigureAwait(false);

                                    SelectedTask.Tasks[target].Result = hash.ToHexString();

                                    foreach (var task in SelectedTask.Tasks)
                                    {
                                        if (task.Value.Result == null) continue;
                                        if (task.Value.Result.ToLowerInvariant().Equals(SelectedTask.Verification?.ToLowerInvariant()))
                                        {
                                            SelectedTask.Verified = true;
                                            return;
                                        }
                                        SelectedTask.Verified = false;
                                    }

                                }
                            }
                        }
                        catch (Exception e)
                        {
                            throw;
                        }
                        finally
                        {
                            SelectedTask.Tasks[target].IsRunning = false;
                        }
                    }
                });
            }
        }
    }
}
