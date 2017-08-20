using LCrypt.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Cryptography;

namespace LCrypt.Password_Manager
{
    [DataContract(Name = "PasswordStorage", Namespace = "https://www.github.com/Loris156/LCrypt")]
    public class PasswordStorage
    {
        /// <summary>
        /// Initialzed Guid, Name, Entries and Created.
        /// </summary>
        public PasswordStorage()
        {
            Guid = Guid.NewGuid();
            Name = Environment.UserName.UppercaseFirst();
            Entries = new List<StorageEntry>();
            Categories = new List<StorageCategory>();
            Created = LastModified = LastOpened = DateTime.Now;
        }

        [DataMember]
        public Guid Guid { get; private set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public List<StorageEntry> Entries { get; set; }

        public List<StorageCategory> Categories { get; set; }

        [DataMember]
        public DateTime Created { get; private set; }

        [DataMember]
        public DateTime LastModified { get; set; }

        [DataMember]
        public DateTime LastOpened { get; set; }

        public string Path { get; set; }

        public AesManaged Aes { get; set; }
    }
}
