using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using Dragablz;
using LCrypt.Models;
using LCrypt.Utility;
using LCrypt.Utility.Extensions;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.Win32;

namespace LCrypt.ViewModels
{
    public class FileChecksumViewModel : ViewModelBase
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

        public static Func<FileChecksumTask> NewItemFactory => () => new FileChecksumTask();

        public ItemActionCallback TabClosingCallback => async e =>
        {
            var task = (FileChecksumTask)e.DragablzItem.Content;

            if (task.Tasks.Any(t => t.Value.IsRunning))
            {
                e.Cancel();
                await DialogCoordinator.Instance.ShowMessageAsync(this,
                    (string)App.LocalizationDictionary["InvalidOperation"],
                    (string)App.LocalizationDictionary["ThereAreStillTasksRunning"],
                    MessageDialogStyle.Affirmative, new MetroDialogSettings
                    {
                        AffirmativeButtonText = (string)App.LocalizationDictionary["Ok"],
                        CustomResourceDictionary = App.DialogDictionary,
                        SuppressDefaultResources = true
                    });
            }
            else
            {
                foreach (var dictionaryTask in task.Tasks)
                    dictionaryTask.Value.CancellationTokenSource?.Dispose();
            }
        };

        public ICommand OpenFileCommand
        {
            get
            {
                return new RelayCommand(obj =>
                {
                    var dialog = new OpenFileDialog
                    {
                        Filter = (string)App.LocalizationDictionary["AllFiles"] + "|*.*",
                        ValidateNames = true,
                        CheckFileExists = true,
                        CheckPathExists = true
                    };
                    if (!dialog.ShowDialog().GetValueOrDefault()) return;
                    if (dialog.FileName.Equals(SelectedTask.FilePath)) return;

                    UnloadFileCommand.Execute(null);
                    SelectedTask.FileInfo = new FileInfo(dialog.FileName);
                }, _ => SelectedTask == null || SelectedTask.Tasks.All(task => !task.Value.IsRunning));
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
                    var target = (string)t;

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
                    return SelectedTask.Tasks[(string)t].Result != null;
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
                    var task = SelectedTask.Tasks[target];

                    if (task.IsRunning)
                    {
                        task.CancellationTokenSource.Cancel();
                        task.CancellationTokenSource.Dispose();
                        task.IsRunning = false;
                        task.Result = null;
                        App.TaskbarProgressManager.Remove(task);
                    }
                    else
                    {
                        task.CancellationTokenSource = new CancellationTokenSource();
                        task.IsRunning = true;
                        App.TaskbarProgressManager.SetIndeterminate(task);

                        try
                        {
                            using (var algorithm = Util.GetHashAlgorithm(target).Create())
                            {
                                using (var fs = new FileStream(SelectedTask.FilePath, FileMode.Open, FileAccess.Read,
                                    FileShare.Read, FileBufferSize, true))
                                {
                                    var hash = await algorithm.ComputeHashAsync(fs,
                                        task.CancellationTokenSource.Token);
                                    task.Result = hash.ToHexString();

                                    foreach (var dictionaryTask in SelectedTask.Tasks)
                                    {
                                        if (dictionaryTask.Value.Result == null) continue;
                                        if (dictionaryTask.Value.Result.ToLowerInvariant()
                                            .Equals(SelectedTask.Verification?.ToLowerInvariant()))
                                        {
                                            SelectedTask.Verified = true;
                                            return;
                                        }
                                        SelectedTask.Verified = false;
                                    }
                                }
                            }
                        }
                        catch (IOException ex)
                        {
                            await DialogCoordinator.Instance.ShowMessageAsync(this,
                                (string)App.LocalizationDictionary["Error"],
                                string.Format((string)App.LocalizationDictionary["IoException"],
                                    ex.Message), MessageDialogStyle.Affirmative, new MetroDialogSettings
                                    {
                                        AffirmativeButtonText = (string)App.LocalizationDictionary["Continue"],
                                        CustomResourceDictionary = App.DialogDictionary,
                                        SuppressDefaultResources = true
                                    });
                        }
                        catch (OperationCanceledException)
                        {
                            // ignored
                        }
                        catch (Exception ex)
                        {
                            await DialogCoordinator.Instance.ShowMessageAsync(this,
                                (string)App.LocalizationDictionary["Error"],
                                string.Format((string)App.LocalizationDictionary["UnknownException"],
                                    ex.Message), MessageDialogStyle.Affirmative, new MetroDialogSettings
                                    {
                                        AffirmativeButtonText = (string)App.LocalizationDictionary["Continue"],
                                        CustomResourceDictionary = App.DialogDictionary,
                                        SuppressDefaultResources = true
                                    });
                        }
                        finally
                        {
                            task.CancellationTokenSource.Dispose();
                            task.IsRunning = false;
                            App.TaskbarProgressManager.Remove(task);
                            CommandManager.InvalidateRequerySuggested();
                        }
                    }
                }, _ => SelectedTask?.FileInfo != null);
            }
        }

        public ICommand UnloadFileCommand
        {
            get
            {
                return new RelayCommand(_ =>
                {
                    SelectedTask.FileInfo = null;
                    SelectedTask.Tasks.ForEach(task => task.Value.Result = null);
                    SelectedTask.Verification = null;
                    SelectedTask.Verified = false;
                }, _ => SelectedTask?.FileInfo != null && SelectedTask.Tasks.All(task => !task.Value.IsRunning));
            }
        }

        public override bool OnClosing()
        {
            return !ChecksumTasks.Any(t => t.Tasks.Any(at => at.Value.IsRunning));
        }
    }
}
