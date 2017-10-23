using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using LCrypt.ViewModels;

namespace LCrypt.Models
{
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

        public PasswordEntry()
        {
            Guid = Guid.NewGuid();
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
        public int IconId
        {
            get => _iconId;
            set => SetAndNotify(ref _iconId, value);
        }

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

        [DataMember]
        public string Username
        {
            get => _username;
            set => SetAndNotify(ref _username, value);
        }

        [DataMember]
        public string Email
        {
            get => _email;
            set => SetAndNotify(ref _email, value);
        }

        [DataMember]
        public string Url
        {
            get => _url;
            set => SetAndNotify(ref _url, value);
        }

        [DataMember]
        public string Comment
        {
            get => _comment;
            set => SetAndNotify(ref _comment, value);
        }

        [DataMember]
        public PasswordCategory Category
        {
            get => _category;
            set => SetAndNotify(ref _category, value);
        }

        [DataMember]
        public byte[] Password { get; set; }

        [DataMember]
        public DateTime Created
        {
            get => _created;
            set => SetAndNotify(ref _created, value);
        }

        [DataMember]
        public DateTime LastModified
        {
            get => _lastModified;
            set => SetAndNotify(ref _lastModified, value);
        }

        public bool IsSelected
        {
            get => _isSelected;
            set => SetAndNotify(ref _isSelected, value);
        }

        public object Clone()
        {
            return Clone(newGuid: false);
        }

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
            this.Name = _copy.Name;
            this.IconId = _copy.IconId;
            this.IsFavorite = IsFavorite;
            this.Username = _copy.Username;
            this.Email = _copy.Email;
            this.Url = _copy.Email;
            this.Comment = _copy.Comment;
            this.Category = _copy.Category;
            this.Password = _copy.Password;
            this.Created = _copy.Created;
            this.LastModified = _copy.LastModified;
            _copy = null;
        }
    }
}