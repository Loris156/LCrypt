using System;
using System.Collections.Generic;

namespace LCrypt.Password_Manager
{
    [Serializable]
    public class PasswordStorage
    {
        /// <summary>
        /// Guid of this password storage.
        /// </summary>
        public Guid Guid { get; set; }

        /// <summary>
        /// Name of password storage owner.
        /// </summary>
        public string Owner { get; set; }

        public List<StorageEntry> Entries { get; }

        public string Path { get => _path; set => _path = value; }

        [NonSerialized] private string _path;

        #region Encryption

        public byte[] Key { set => _key = value; }
        [NonSerialized] private byte[] _key;

        public byte[] Iv { set => _iv = value; }
        [NonSerialized] private byte[] _iv;

        #endregion

        /// <summary>
        /// Initializes Guid and Entries.
        /// </summary>
        public PasswordStorage()
        {
            Guid = Guid.NewGuid();
            Entries = new List<StorageEntry>(0);
        }

        public void Save()
        {
        }
    }
}