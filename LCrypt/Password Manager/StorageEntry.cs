using System;
using System.Runtime.Serialization;

namespace LCrypt.Password_Manager
{
    [DataContract(Name = "StorageEntry", Namespace = "https://www.github.com/Loris156/LCrypt")]
    public class StorageEntry : IEquatable<StorageEntry>
    {
        /// <summary>
        /// Initialized Guid, IconId, Name, IsFavorite, Username, Email, Created and LastModified.
        /// </summary>
        public StorageEntry()
        {
            Guid = Guid.NewGuid();
            IconId = 0;
            Name = Username = Email = Comment = string.Empty;
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
        public StorageCategory Category { get; set; }

        [DataMember]
        public string Comment { get; set; }

        [DataMember]
        public DateTime Created { get; private set; }

        [DataMember]
        public DateTime LastModified { get; set; }

        public bool Equals(StorageEntry other)
        {
            if (ReferenceEquals(null, other)) return false;
            return ReferenceEquals(this, other) || Guid.Equals(other.Guid);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((StorageEntry) obj);
        }

        public override int GetHashCode()
        {
            return Guid.GetHashCode();
        }
    }
}
