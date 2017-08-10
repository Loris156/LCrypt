using System;

namespace LCrypt.Password_Manager
{
    [Serializable]
    public class StorageEntry
    {
        /// <summary>
        /// Guid of this entry.
        /// </summary>
        public Guid Guid { get; set; }
        
        /// <summary>
        /// Icon of this entry.
        /// Converted to ImageSource with <see cref="Int32ToPasswordImageSource"/>.
        /// </summary>
        public int IconId { get; set; }

        /// <summary>
        /// Name of this entry.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// True if entry is in favorites.
        /// </summary>
        public bool IsFavorite { get; set; }

        /// <summary>
        /// Username of user.
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Email of user.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Encrypted password of user.
        /// </summary>
        public byte[] Password { get; set; }

        /// <summary>
        /// User defined comment.
        /// </summary>
        public string Comment { get; set; }

        /// <summary>
        /// Creation date/time of this entry.
        /// </summary>
        public DateTime Created { get; set; }

        /// <summary>
        /// Last modification time of this entry.
        /// </summary>
        public DateTime LastModified { get; set; }

        /// <summary>
        /// Initializes Guid, Created and LastModified.
        /// </summary>
        public StorageEntry()
        {
            Guid = Guid.NewGuid();
            Created = LastModified = DateTime.Now;
        }
    }
}
