using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using LCrypt.Utility;

namespace LCrypt.Models
{
    /// <summary>
    /// Represents a password entry in the LCrypt Password Manager.
    /// </summary>
    [DataContract(Name = "PasswordEntry", Namespace = "https://github.com/Loris156/LCrypt")]
    public class PasswordEntry : NotifyPropertyChanged, IEquatable<PasswordEntry>, ICloneable, IEditableObject
    {
        private PasswordEntry _copy;

        private PasswordCategory _category;

        private DateTime _created, _lastModified;

        private int _iconId;
        private bool _isFavorite;
        private string _name;

        private string _username, _email, _url, _comment;

        private bool _isSelected;

        /// <summary>
        /// Creates a new password entry with a new Guid and sets Created and LastModified to now.
        /// </summary>
        public PasswordEntry()
        {
            Guid = Guid.NewGuid();
            Created = LastModified = DateTime.Now;
        }

        /// <summary>
        /// Unique identifier of this entry.
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
        /// Icon of this entry. Icons are located in /Resources/PasswordManagerIcons/Entry/.
        /// </summary>
        [DataMember]
        public int IconId
        {
            get => _iconId;
            set => SetAndNotify(ref _iconId, value);
        }

        /// <summary>
        /// True if this entry is in the special favorites category.
        /// </summary>
        [DataMember]
        public bool IsFavorite
        {
            get => _isFavorite;
            set
            {
                SetAndNotify(ref _isFavorite, value);
                LastModified = DateTime.Now;
            }
        }

        /// <summary>
        /// Username of account.
        /// </summary>
        [DataMember]
        public string Username
        {
            get => _username;
            set => SetAndNotify(ref _username, value);
        }

        /// <summary>
        /// EMail of account.
        /// </summary>
        [DataMember]
        public string Email
        {
            get => _email;
            set => SetAndNotify(ref _email, value);
        }

        /// <summary>
        /// URL of service.
        /// </summary>
        [DataMember]
        public string Url
        {
            get => _url;
            set => SetAndNotify(ref _url, value);
        }

        /// <summary>
        /// User-defined comment for entry.
        /// </summary>
        [DataMember]
        public string Comment
        {
            get => _comment;
            set => SetAndNotify(ref _comment, value);
        }

        /// <summary>
        /// Category, that this entry belongs to.
        /// </summary>
        [DataMember]
        public PasswordCategory Category
        {
            get => _category;
            set => SetAndNotify(ref _category, value);
        }

        /// <summary>
        /// Encrypted password of entry.
        /// </summary>
        [DataMember]
        public byte[] Password { get; set; }

        /// <summary>
        /// Creation date of this entry.
        /// </summary>
        [DataMember]
        public DateTime Created
        {
            get => _created;
            set => SetAndNotify(ref _created, value);
        }

        /// <summary>
        /// Last modification of this entry.
        /// </summary>
        [DataMember]
        public DateTime LastModified
        {
            get => _lastModified;
            set => SetAndNotify(ref _lastModified, value);
        }

        /// <summary>
        /// Creates a clone of this instance, but reference types point to a different memory location and changes won't affect original.
        /// Does not set Guid to a new value.
        /// </summary>
        /// <returns>A clone of this PasswordEntry instance.</returns>
        public object Clone()
        {
            return Clone(newGuid: false);
        }

        /// <summary>
        /// Creates a clone of this instance, but reference types point to a different memory location and changes won't affect original.
        /// </summary>
        /// <param name="newGuid">True if Guid should have a new value (for duplication).</param>
        /// <returns>A clone of this PasswordEntry instance.</returns>
        public object Clone(bool newGuid)
        {
            return new PasswordEntry
            {
                Guid = newGuid ? Guid.NewGuid() : Guid,
                Name = Name != null ? string.Copy(Name) : null,
                IconId = IconId,
                IsFavorite = IsFavorite,
                Username = Username != null ? string.Copy(Username) : null,
                Email = Email != null ? string.Copy(Email) : null,
                Url = Url != null ? string.Copy(Url) : null,
                Comment = Comment != null ? string.Copy(Comment) : null,
                Category = Category,
                Password = (byte[])Password?.Clone(),
                Created = Created,
                LastModified = LastModified
            };
        }

        /// <summary>
        /// Returns property "Name".
        /// </summary>
        /// <returns>Name of this entry.</returns>
        public override string ToString()
        {
            return Name;
        }

        public bool Equals(PasswordEntry other)
        {
            if (ReferenceEquals(null, other)) return false;
            return ReferenceEquals(this, other) || Guid.Equals(other.Guid);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == typeof(PasswordEntry) && Equals((PasswordEntry)obj);
        }

        public override int GetHashCode()
        {
            // ReSharper disable once NonReadonlyMemberInGetHashCode
            return Guid.GetHashCode();
        }

        public void BeginEdit()
        {
            _copy = (PasswordEntry)Clone();
        }

        public void EndEdit()
        {
            _copy = null;
        }

        public void CancelEdit()
        {
            Name = _copy.Name;
            IconId = _copy.IconId;
            IsFavorite = IsFavorite;
            Username = _copy.Username;
            Email = _copy.Email;
            Url = _copy.Email;
            Comment = _copy.Comment;
            Category = _copy.Category;
            Password = _copy.Password;
            Created = _copy.Created;
            LastModified = _copy.LastModified;
            _copy = null;
        }
    }
}