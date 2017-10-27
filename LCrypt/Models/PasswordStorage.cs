using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Threading.Tasks;
using LCrypt.Utility;
using LCrypt.ViewModels;
using LCrypt.Utility.Extensions;

namespace LCrypt.Models
{
    [DataContract(Name = "PasswordStorage", Namespace = "https://github.com/Loris156/LCrypt")]
    public class PasswordStorage : NotifyPropertyChanged, IDisposable
    {
        private bool _disposed;
        private string _name;
        
        private bool _showToolbar;

        public PasswordStorage()
        {
            Guid = Guid.NewGuid();
            Entries = new List<PasswordEntry>();
            Categories = new List<PasswordCategory>();
            Created = LastModified = DateTime.Now;
        }

        [DataMember]
        public Guid Guid { get; private set; }

        [DataMember]
        public string Name
        {
            get => _name;
            set => SetAndNotify(ref _name, value);
        }

        [DataMember]
        public List<PasswordEntry> Entries { get; private set; }

        [DataMember]
        public List<PasswordCategory> Categories { get; private set; }

        [DataMember]
        public DateTime Created { get; set; }

        [DataMember]
        public DateTime LastModified { get; set; }

        [DataMember]
        public DateTime LastOpened { get; set; }

        [DataMember]
        public bool ShowToolbar
        {
            get => _showToolbar;
            set => SetAndNotify(ref _showToolbar, value);
        }

        public byte[] Salt { get; set; }

        public AesManaged Aes { get; set; }

        public static async Task<PasswordStorage> CreateDefaultStorageAsync(AesManaged aes)
        {
            var storage = new PasswordStorage { Aes = aes };

            storage.Entries.Add(new PasswordEntry
            {
                Name = (string)App.LocalizationDictionary["ExampleEntryName"],
                Username = (string)App.LocalizationDictionary["ExampleEntryUsername"],
                Email = (string)App.LocalizationDictionary["ExampleEntryEmail"],
                Url = (string)App.LocalizationDictionary["ExampleEntryUrl"],
                Comment = (string)App.LocalizationDictionary["ExampleEntryComment"],
                Password = await aes.EncryptStringAsync("LCrypt Password Manager")
            });
            storage.Entries.Add(new PasswordEntry
            {
                Name = (string)App.LocalizationDictionary["ExampleEntryName"],
                Username = (string)App.LocalizationDictionary["ExampleEntryUsername"],
                Email = (string)App.LocalizationDictionary["ExampleEntryEmail"],
                Url = (string)App.LocalizationDictionary["ExampleEntryUrl"],
                Comment = (string)App.LocalizationDictionary["ExampleEntryComment"],
                Password = await aes.EncryptStringAsync("LCrypt Password Manager")
            });

            storage.Categories.Add(new PasswordCategory
            {
                IconId = 2,
                Name = "Netzwerk"
            });
            storage.Categories.Add(new PasswordCategory
            {
                IconId = 5,
                Name = "Tresore"
            });

            return storage;
        }

        public void Dispose()
        {
            if (_disposed) return;

            Aes?.Dispose();

            GC.SuppressFinalize(this);
            _disposed = true;
        }

        ~PasswordStorage()
        {
            Dispose();
        }
    }
}