using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Dragablz;
using LCrypt.EncryptionAlgorithms;
using LCrypt.HashAlgorithms;
using LCrypt.Models;
using LCrypt.TextEncodings;
using LCrypt.Utility;
using LCrypt.Utility.Extensions;

namespace LCrypt.ViewModels
{
    public class TextEncryptionViewModel : ViewModelBase
    {
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

            var encryptionTask = new TextEncryptionTask();
            encryptionTask.PropertyChanged += EncryptionTask_OnPropertyChanged;

            EncryptionTasks = new ObservableCollection<TextEncryptionTask> { encryptionTask };
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

        public Func<TextEncryptionTask> NewTabFactory => () =>
        {
            var hashingTask = new TextEncryptionTask();
            hashingTask.PropertyChanged += EncryptionTask_OnPropertyChanged;
            return hashingTask;
        };

        public ItemActionCallback TabClosingCallback => e =>
        {
            var task = (TextEncryptionTask)e.DragablzItem.DataContext;
            task.PropertyChanged -= EncryptionTask_OnPropertyChanged;
            task.EncryptionAlgorithmCache?.Dispose();
        };

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

        public ICommand CopyOutputCommand
        {
            get
            {
                return new RelayCommand(_ => { Clipboard.SetText(SelectedTask.Output, TextDataFormat.UnicodeText); },
                    _ => !string.IsNullOrWhiteSpace(SelectedTask?.Output));
            }
        }

        private async void EncryptionTask_OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var task = (TextEncryptionTask)sender;
            if (e.PropertyName.Equals(nameof(task.Output))) return;

            if (e.PropertyName.Equals(nameof(task.TextEncoding)))
                task.TextEncodingCache = task.TextEncoding.Create();

            if (e.PropertyName.Equals(nameof(task.EncryptionAlgorithm)))
            {
                task.EncryptionAlgorithmCache?.Dispose();
                task.EncryptionAlgorithmCache = task.EncryptionAlgorithm.Create();
            }

            if (task.Input == null) return;
            if (task.Password == null) return;

            Debug.Assert(task.EncryptionAlgorithm != null);
            Debug.Assert(task.TextEncoding != null);

            if (task.Encrypt)
            {
                

                var encrypted =
                    await task.EncryptionAlgorithmCache.EncryptStringAsync(task.Input, task.TextEncodingCache);
                task.Output = encrypted.ToHexString();
            }
            else
            {
                var decrypted =
                    await task.EncryptionAlgorithmCache.DecryptStringAsync(task.Input.ToByteArray(), task.TextEncodingCache);
                task.Output = decrypted;
            }
            
            //var hash = task.EncryptionAlgorithmCache.ComputeHash(task.Input, task.TextEncodingCache);
            //task.Output = hash.ToHexString();
        }
    }
}
