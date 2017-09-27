using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using Dragablz;
using LCrypt.HashAlgorithms;
using LCrypt.Models;
using LCrypt.TextEncodings;
using LCrypt.Utility;
using LCrypt.Utility.Extensions;

namespace LCrypt.ViewModels
{
    public class TextHashingViewModel : NotifyPropertyChanged
    {
        private IEnumerable<IHashAlgorithm> _hashAlgorithms;

        private ObservableCollection<TextHashingTask> _hashingTasks;
        private TextHashingTask _selectedTask;
        private IEnumerable<ITextEncoding> _textEncodings;

        public TextHashingViewModel()
        {
            TextEncodings = new List<ITextEncoding>(5)
            {
                new Ascii(),
                new Utf8(),
                new Utf16(),
                new Utf32(),
                new BigEndianUtf16()
            };

            HashAlgorithms = new List<IHashAlgorithm>(7)
            {
                new Md5(),
                new Crc32(),
                new Sha1(),
                new Sha256(),
                new Sha384(),
                new Sha512(),
                new Whirlpool()
            };

            var hashingTask = new TextHashingTask();
            hashingTask.PropertyChanged += HashingTask_PropertyChanged;

            HashingTasks = new ObservableCollection<TextHashingTask> { hashingTask };
        }

        public IEnumerable<ITextEncoding> TextEncodings
        {
            get => _textEncodings;
            set => SetAndNotify(ref _textEncodings, value);
        }

        public IEnumerable<IHashAlgorithm> HashAlgorithms
        {
            get => _hashAlgorithms;
            set => SetAndNotify(ref _hashAlgorithms, value);
        }

        public ICollection<TextHashingTask> HashingTasks
        {
            get => _hashingTasks;
            set => SetAndNotify(ref _hashingTasks, (ObservableCollection<TextHashingTask>)value);
        }

        public TextHashingTask SelectedTask
        {
            get => _selectedTask;
            set => SetAndNotify(ref _selectedTask, value);
        }

        public Func<TextHashingTask> NewTabFactory => () =>
        {
            var hashingTask = new TextHashingTask();
            hashingTask.PropertyChanged += HashingTask_PropertyChanged;
            return hashingTask;
        };

        public ItemActionCallback TabClosingCallback => e =>
        {
            var task = (TextHashingTask)e.DragablzItem.DataContext;
            task.PropertyChanged -= HashingTask_PropertyChanged;
            task.HashAlgorithmCache?.Dispose();
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

        private void HashingTask_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var task = (TextHashingTask)sender;
            if (e.PropertyName.Equals(nameof(task.Output))) return;

            if (e.PropertyName.Equals(nameof(task.TextEncoding)))
                task.TextEncodingCache = task.TextEncoding.Create();

            if (e.PropertyName.Equals(nameof(task.HashAlgorithm)))
            {
                task.HashAlgorithmCache?.Dispose();
                task.HashAlgorithmCache = task.HashAlgorithm.Create();
            }

            if (task.Input == null) return;

            Debug.Assert(task.HashAlgorithm != null);
            Debug.Assert(task.TextEncoding != null);

            var hash = task.HashAlgorithmCache.ComputeHash(task.Input, task.TextEncodingCache);
            task.Output = hash.ToHexString();
        }
    }
}
