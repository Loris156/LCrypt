using LCrypt.Utility;
using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace LCrypt.Models
{
    /// <summary>
    /// A category for sorting entries in the LCrypt Password Manager.
    /// </summary>
    [DataContract(Name = "PasswordCategory", Namespace = "https://github.com/Loris156/LCrypt")]
    public class PasswordCategory : NotifyPropertyChanged, IEquatable<PasswordCategory>, ICloneable, IEditableObject
    {
        private PasswordCategory _copy;

        public readonly Guid AllEntries = new Guid("DC12558F-00D8-4180-BE1F-E072EC04C2FA");
        public readonly Guid Favorites = new Guid("776D9BD0-C41B-4710-885F-68E19075AFCE");

        private string _name;
        private int _iconId;

        /// <summary>
        /// Creates a new PasswordCategory with a new Guid.
        /// </summary>
        public PasswordCategory()
            : this(Guid.NewGuid())
        {
        }

        /// <summary>
        /// Creates a new PasswordCategory with an already known Guid.
        /// </summary>
        /// <param name="guid">Guid for the new PasswordCategory</param>
        public PasswordCategory(Guid guid)
        {
            Guid = guid;
        }

        /// <summary>
        /// Unique identifier of this category.
        /// </summary>
        [DataMember]
        public Guid Guid { get; private set; }

        /// <summary>
        /// Displayed name of this entry.
        /// </summary>
        [DataMember]
        public string Name
        {
            get => _name;
            set => SetAndNotify(ref _name, value);
        }

        /// <summary>
        /// Icon of this category. Icons are located in /Resources/PasswordManagerIcons/Category/.
        /// </summary>
        [DataMember]
        public int IconId
        {
            get => _iconId;
            set => SetAndNotify(ref _iconId, value);
        }

        /// <summary>
        /// Creates a clone of this instance, but reference types point to a different memory location and changes won't affect original.
        /// Does not set Guid to a new value.
        /// </summary>
        /// <returns>A clone of this PasswordCategory instance.</returns>
        public object Clone()
        {
            return Clone(newGuid: false);
        }

        /// <summary>
        /// Creates a clone of this instance, but reference types point to a different memory location and changes won't affect original.
        /// </summary>
        /// <param name="newGuid">True if Guid should have a new value (for duplication).</param>
        /// <returns>A clone of this PasswordCategory instance.</returns>
        public object Clone(bool newGuid)
        {
            return new PasswordCategory
            {
                Guid = newGuid ? Guid.NewGuid() : Guid,
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

        public void BeginEdit()
        {
            _copy = (PasswordCategory) Clone();
        }

        public void EndEdit()
        {
            _copy = null;
        }

        public void CancelEdit()
        {
            this.Name = string.Copy(_copy.Name);
            this.IconId = _copy.IconId;
            _copy = null;
        }
    }
}
