using LCrypt.EncryptionAlgorithms;
using LCrypt.Models;
using LCrypt.Utility;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Windows.Input;

namespace LCrypt.ViewModels
{
    public class FileEncryptionViewModel : ViewModelBase
    {
        private ObservableCollection<FileEncryptionTask> _encryptionTasks;
        private FileEncryptionTask _selectedTask;

        public FileEncryptionViewModel()
        {
            EncryptionAlgorithms = new List<IEncryptionAlgorithm>
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

                    Debug.Assert(task.FilePath != null);
                    Debug.Assert(task.FileName != null);

                    var dialog = new SaveFileDialog
                    {
                        Filter = (string)App.LocalizationDictionary["AllFiles"] + "|*.*",
                        OverwritePrompt = true,
                        ValidateNames = true,
                        AddExtension = true,
                        DefaultExt = Path.GetExtension(task.FileName)
                    };

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

                    if (!task.IsRunning)
                    {
                        task.CancellationTokenSource = new CancellationTokenSource();

                        var fileEncryption = new FileEncryption();
                        var progress = new Progress<double>(p =>
                        {
                            task.Progress = p;
                            App.TaskbarProgressManager.SetProgress(task, p);
                        });

                        task.IsFinished = false;
                        task.IsRunning = true;
                        try
                        {
                            if (task.Encrypt)
                                await fileEncryption.EncryptFileAsync(task, progress, task.CancellationToken);
                            else
                                await fileEncryption.DecryptFileAsync(task, progress, task.CancellationToken);

                            task.IsFinished = true;
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
                        catch (CryptographicException ex)
                        {
                            await DialogCoordinator.Instance.ShowMessageAsync(this,
                                (string)App.LocalizationDictionary["Error"],
                                string.Format((string)App.LocalizationDictionary["CryptographicException"],
                                    ex.Message), MessageDialogStyle.Affirmative, new MetroDialogSettings
                                    {
                                        AffirmativeButtonText = (string)App.LocalizationDictionary["Continue"],
                                        CustomResourceDictionary = App.DialogDictionary,
                                        SuppressDefaultResources = true
                                    });
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
                            task.IsRunning = false;
                            task.Progress = 0;
                            App.TaskbarProgressManager.Remove(task);
                        }
                    }
                    else
                    {
                        var dialogResult = await DialogCoordinator.Instance.ShowMessageAsync(this, (string)App.LocalizationDictionary["EncryptionTask"],
                            string.Format((string)App.LocalizationDictionary["DoYouReallyWantToCancelEncryptionTask"],
                                task.FileName), MessageDialogStyle.AffirmativeAndNegative, new MetroDialogSettings
                                {
                                    AffirmativeButtonText = (string)App.LocalizationDictionary["Yes"],
                                    NegativeButtonText = (string)App.LocalizationDictionary["No"],
                                    CustomResourceDictionary = App.DialogDictionary,
                                    SuppressDefaultResources = true
                                });

                        if (dialogResult != MessageDialogResult.Affirmative) return;
                        task.CancellationTokenSource.Cancel();
                        task.IsRunning = false;
                        task.Progress = 0;
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

        public override bool OnClosing()
        {
            return !EncryptionTasks.Any(t => t.IsRunning);
        }
    }
}