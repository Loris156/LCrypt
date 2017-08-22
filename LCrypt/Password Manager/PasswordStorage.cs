using LCrypt.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Xml;

namespace LCrypt.Password_Manager
{
    [DataContract(Name = "PasswordStorage", Namespace = "https://www.github.com/Loris156/LCrypt")]
    public class PasswordStorage : IDisposable
    {
        /// <summary>
        /// Initialzed Guid, Name, Entries and Created.
        /// </summary>
        public PasswordStorage()
        {
            Guid = Guid.NewGuid();
            Entries = new List<StorageEntry>();
            Categories = new List<StorageCategory>();
            Created = LastModified = LastOpened = DateTime.Now;
        }

        [DataMember]
        public Guid Guid { get; private set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public List<StorageEntry> Entries { get; set; }

        [DataMember]
        public List<StorageCategory> Categories { get; set; }

        [DataMember]
        public DateTime Created { get; private set; }

        [DataMember]
        public DateTime LastModified { get; set; }

        [DataMember]
        public DateTime LastOpened { get; set; }

        public string Path { get; set; }

        public byte[] Salt { get; set; }

        public AesManaged Aes { get; set; }

        public async Task SaveAsync()
        {
            using (var fileStream = new FileStream(Path, FileMode.Create, FileAccess.Write, FileShare.None, 4096,
                useAsync: true))
            {
                await fileStream.WriteAsync(Salt, 0, Salt.Length);
                await fileStream.WriteAsync(Aes.IV, 0, Aes.BlockSize / 8);

                byte[] serialized;
                using (var xmlStream = new MemoryStream())
                {
                    using (var xmlWriter = XmlDictionaryWriter.CreateBinaryWriter(xmlStream))
                    {
                        var serializer = new DataContractSerializer(typeof(PasswordStorage));
                        serializer.WriteObject(xmlWriter, this);
                    }
                    serialized = xmlStream.ToArray();
                }

                using (var transform = Aes.CreateEncryptor())
                {
                    using (var cryptoStream = new CryptoStream(fileStream, transform, CryptoStreamMode.Write))
                    {
                        await cryptoStream.WriteAsync(serialized, 0, serialized.Length);
                    }
                }
            }
        }

        public void Dispose()
        {
            Aes?.Dispose();
        }

        public override string ToString()
        {
            return $"Storage {Name} with GUID {Guid} created on {Created:F}";
        }
    }
}
