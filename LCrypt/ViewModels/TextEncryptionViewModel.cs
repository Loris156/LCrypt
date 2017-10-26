using LCrypt.EncryptionAlgorithms;
using LCrypt.Models;
using LCrypt.TextEncodings;
using LCrypt.Utility;
using LCrypt.Utility.Extensions;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Input;

namespace LCrypt.ViewModels
{
    public class TextEncryptionViewModel : ViewModelBase
    {
        private const int SaltSize = 32;
        private const int Iterations = 50000;

        private IEnumerable<IEncryptionAlgorithm> _encryptionAlgorithms;

        private ObservableCollection<TextEncryptionTask> _encryptionTasks;
        private TextEncryptionTask _selectedTask;
        private IEnumerable<ITextEncoding> _textEncodings;

        public TextEncryptionViewModel()
        {
            TextEncodings = new List<ITextEncoding>(5)
            {
                new Ascii(),
                new Utf8(),
                new Utf16(),
                new Utf32(),
                new BigEndianUtf16()
            };

            EncryptionAlgorithms = new List<IEncryptionAlgorithm>(4)
            {
                new Aes256(),
                new Des64(),
                new Tdea192(),
                new Rc2()
            };

            EncryptionTasks = new ObservableCollection<TextEncryptionTask> { new TextEncryptionTask() };
        }

        public IEnumerable<ITextEncoding> TextEncodings
        {
            get => _textEncodings;
            set => SetAndNotify(ref _textEncodings, value);
        }

        public IEnumerable<IEncryptionAlgorithm> EncryptionAlgorithms
        {
            get => _encryptionAlgorithms;
            set => SetAndNotify(ref _encryptionAlgorithms, value);
        }

        public ICollection<TextEncryptionTask> EncryptionTasks
        {
            get => _encryptionTasks;
            set => SetAndNotify(ref _encryptionTasks, (ObservableCollection<TextEncryptionTask>)value);
        }

        public TextEncryptionTask SelectedTask
        {
            get => _selectedTask;
            set => SetAndNotify(ref _selectedTask, value);
        }

        public Func<TextEncryptionTask> NewTabFactory => () => new TextEncryptionTask();

        public ICommand PasteInputCommand
        {
            get { return new RelayCommand(_ => { SelectedTask.Input = Clipboard.GetText(); }); }
        }

        public ICommand CopyInputCommand
        {
            get
            {
                return new RelayCommand(_ => { Clipboard.SetText(SelectedTask.Input, TextDataFormat.UnicodeText); },
                    _ => !string.IsNullOrWhiteSpace(SelectedTask?.Input));
            }
        }

        public ICommand StartStopEncryptionCommand
        {
            get
            {
                return new RelayCommand(async t =>
                {
                    Debug.Assert(t is TextEncryptionTask);
                    var task = (TextEncryptionTask)t;

                    if (!task.IsRunning)
                    {
                        task.CancellationTokenSource = new CancellationTokenSource();
                        task.IsRunning = true;
                        App.TaskbarProgressManager.SetIndeterminate(task);

                        try
                        {
                            var encoding = task.TextEncoding.Create();

                            using (var algorithm = task.EncryptionAlgorithm.Create())
                            {
                                if (task.Encrypt) // Encryption
                                {
                                    algorithm.IV = Util.GenerateStrongRandomBytes(algorithm.BlockSize / 8);

                                    var salt = Util.GenerateStrongRandomBytes(SaltSize);
                                    algorithm.Key = await SelectedTask.Password.DeriveKeyAsync(salt, Iterations,
                                        algorithm.KeySize / 8,
                                        task.CancellationToken);

                                    using (var encryptionResult = new MemoryStream())
                                    {
                                        await encryptionResult.WriteAsync(salt, 0, SaltSize);
                                        await encryptionResult.WriteAsync(algorithm.IV, 0, algorithm.BlockSize / 8);

                                        var encryptedString = await algorithm.EncryptStringAsync(task.Input, encoding, task.CancellationToken);
                                        await encryptionResult.WriteAsync(encryptedString, 0, encryptedString.Length);

                                        task.Output = encryptionResult.ToArray().ToHexString();
                                    }
                                }
                                else // Decryption
                                {
                                    var encryptedBytes = task.Input.ToByteArray();
                                    using (var encryptedStream = new MemoryStream(encryptedBytes))
                                    {

                                        byte[] salt = new byte[SaltSize], iv = new byte[algorithm.BlockSize / 8];

                                        await encryptedStream.ReadAsync(salt, 0, SaltSize);
                                        await encryptedStream.ReadAsync(iv, 0, algorithm.BlockSize / 8);

                                        algorithm.IV = iv;

                                        var encryptedString =
                                            new byte[encryptedStream.Length - SaltSize - algorithm.BlockSize / 8];
                                        await encryptedStream.ReadAsync(encryptedString, 0, encryptedString.Length);

                                        algorithm.Key = await task.Password.DeriveKeyAsync(salt, Iterations,
                                            algorithm.KeySize / 8, task.CancellationToken);

                                        task.Output = await algorithm.DecryptStringAsync(encryptedString, encoding, task.CancellationToken);
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            await DialogCoordinator.Instance.ShowMessageAsync(this,
                                (string) App.LocalizationDictionary["Error"],
                                string.Format((string) App.LocalizationDictionary["TextEncryptionError"], ex.Message),
                                MessageDialogStyle.Affirmative, new MetroDialogSettings
                                {
                                    AffirmativeButtonText = (string) App.LocalizationDictionary["Ok"],
                                    CustomResourceDictionary = App.DialogDictionary,
                                    SuppressDefaultResources = true
                                });
                        }
                        finally
                        {
                            task.IsRunning = false;
                            App.TaskbarProgressManager.Remove(task);
                            CommandManager.InvalidateRequerySuggested();
                        }
                    }
                    else
                    {
                        if (await DialogCoordinator.Instance.ShowMessageAsync(this,
                                (string)App.LocalizationDictionary["Warning"],
                                (string)App.LocalizationDictionary["TextEncryptionReallyCancel"],
                                MessageDialogStyle.AffirmativeAndNegative, new MetroDialogSettings
                                {
                                    AffirmativeButtonText = (string)App.LocalizationDictionary["Yes"],
                                    NegativeButtonText = (string)App.LocalizationDictionary["No"],
                                    CustomResourceDictionary = App.DialogDictionary,
                                    SuppressDefaultResources = true
                                }) == MessageDialogResult.Affirmative)
                            task.CancellationTokenSource.Cancel();
                    }
                },
                t =>
                {
                    Debug.Assert(t is TextEncryptionTask);
                    var task = (TextEncryptionTask)t;
                    return task.Password?.Length > 0 && !string.IsNullOrEmpty(task.Input);
                });
            }
        }

        public ICommand CopyOutputCommand
        {
            get
            {
                return new RelayCommand(_ => { Clipboard.SetText(SelectedTask.Output, TextDataFormat.UnicodeText); },
                    _ => !string.IsNullOrWhiteSpace(SelectedTask?.Output));
            }
        }

        public override bool OnClosing()
        {
            return EncryptionTasks.All(t => !t.IsRunning);
        }
    }
}
