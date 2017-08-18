using System;
using System.Runtime.Serialization;

namespace LCrypt.Password_Manager
{
    [DataContract(Name = "StorageEntry", Namespace = "https://www.github.com/Loris156/LCrypt")]
    public class StorageEntry
    {
        /// <summary>
        /// Initialized Guid, IconId, Name, IsFavorite, Username, Email, Created and LastModified.
        /// </summary>
        public StorageEntry()
        {
            Guid = Guid.NewGuid();
            IconId = 0;
            Name = Username = Email = string.Empty;
            IsFavorite = false;
            Created = LastModified = DateTime.Now;
        }

        [DataMember]
        public Guid Guid { get; private set; }

        [DataMember]
        public int IconId { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public bool IsFavorite { get; set; }

        [DataMember]
        public string Username { get; set; }

        [DataMember]
        public string Email { get; set; }

        [DataMember]
        public byte[] Password { get; set; }

        [DataMember]
        public string Comment { get; set; }

        [DataMember]
        public DateTime Created { get; private set; }

        [DataMember]
        public DateTime LastModified { get; set; }
    }
}
