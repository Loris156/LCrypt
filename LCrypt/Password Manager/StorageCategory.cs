using System;
using System.Runtime.Serialization;

namespace LCrypt.Password_Manager
{
    [DataContract(Name ="StorageCategory", Namespace ="https://www.github.com/Loris156/LCrypt")]
    public class StorageCategory : IEquatable<StorageCategory>
    {
        public StorageCategory()
        {
            Guid = Guid.NewGuid();
        }

        [DataMember]
        public Guid Guid { get; private set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public int IconId { get; set; }

        public string Tag { get; set; }

        public bool Equals(StorageCategory other)
        {
            if (ReferenceEquals(null, other)) return false;
            return ReferenceEquals(this, other) || Guid.Equals(other.Guid);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == this.GetType() && Equals((StorageCategory) obj);
        }

        public override int GetHashCode()
        {
            return Guid.GetHashCode();
        }
    }
}
