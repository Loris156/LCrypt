using System;
using System.Runtime.Serialization;
using LCrypt.ViewModels;

namespace LCrypt.Models
{
    [DataContract(Name = "PasswordCategory", Namespace = "https://github.com/Loris156/LCrypt")]
    public class PasswordCategory : NotifyPropertyChanged, IEquatable<PasswordCategory>, ICloneable
    {
        public readonly Guid AllEntries = new Guid("DC12558F-00D8-4180-BE1F-E072EC04C2FA");
        public readonly Guid Favorites = new Guid("776D9BD0-C41B-4710-885F-68E19075AFCE");

        private string _name;
        private int _iconId;

        public PasswordCategory()
            : this(Guid.NewGuid())
        {
        }

        public PasswordCategory(Guid guid)
        {
            Guid = guid;
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
        public int IconId
        {
            get => _iconId;
            set => SetAndNotify(ref _iconId, value);
        }

        public object Clone()
        {
            return new PasswordCategory
            {
                Guid = Guid,
                Name = Name != null ? string.Copy(Name) : null,
                IconId = IconId
            };
        }

        public bool Equals(PasswordCategory other)
        {
            if (ReferenceEquals(null, other)) return false;
            return ReferenceEquals(this, other) || Guid.Equals(other.Guid);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == typeof(PasswordCategory) && Equals((PasswordCategory) obj);
        }

        public override int GetHashCode()
        {
            // ReSharper disable once NonReadonlyMemberInGetHashCode
            return Guid.GetHashCode();
        }
    }
}
