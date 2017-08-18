using LCrypt.Utility;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace LCrypt.Password_Manager
{
    [DataContract(Name = "PasswordStorage", Namespace = "https://www.github.com/Loris156/LCrypt")]
    public class PasswordStorage
    {
        /// <summary>
        /// Initialzed Guid, OwnerName, Entries and Created.
        /// </summary>
        public PasswordStorage()
        {
            Guid = Guid.NewGuid();
            OwnerName = Environment.UserName.UppercaseFirst();
            Entries = new List<StorageEntry>();
            Created = LastModified = LastOpened = DateTime.Now;
        }

        [DataMember]
        public Guid Guid { get; private set; }

        [DataMember]
        public string OwnerName { get; set; }

        [DataMember]
        public List<StorageEntry> Entries { get; set; }

        [DataMember]
        public DateTime Created { get; private set; }

        [DataMember]
        public DateTime LastModified { get; set; }

        [DataMember]
        public DateTime LastOpened { get; set; }
    }
}
