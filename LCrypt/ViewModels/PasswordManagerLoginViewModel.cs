using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Xml;
using LCrypt.Models;
using LCrypt.Utility;
using LCrypt.Utility.Extensions;
using MahApps.Metro.Controls.Dialogs;

namespace LCrypt.ViewModels
{
    public class PasswordManagerLoginViewModel : ViewModelBase
    {
        private bool _isBusy;
        private bool _isLogin, _isRegister;

        private SecureString _password, _repeatedPassword;

        private string _storageName;

        public PasswordManagerLoginViewModel()
        {
            if (File.Exists(Path.Combine(App.MyDocuments, "LCrypt", PasswordManagerViewModel.WalletFileName)))
                IsLogin = true;
            else
                IsRegister = true;
        }

        public bool IsBusy
        {
            get => _isBusy;
            set => SetAndNotify(ref _isBusy, value);
        }

        public bool IsLogin
        {
            get => _isLogin;
            set => SetAndNotify(ref _isLogin, value);
        }

        public bool IsRegister
        {
            get => _isRegister;
            set => SetAndNotify(ref _isRegister, value);
        }

        public SecureString Password
        {
            get => _password;
            set => SetAndNotify(ref _password, value);
        }

        public SecureString RepeatedPassword
        {
            get => _repeatedPassword;
            set => SetAndNotify(ref _repeatedPassword, value);
        }

        public string StorageName
        {
            get => _storageName;
            set => SetAndNotify(ref _storageName, value);
        }

        public event EventHandler<PasswordStorage> LoggedInSuccessfully;

        public ICommand LoginCommand
        {
            get
            {
                return new RelayCommand(async _ =>
                {
                    IsBusy = true;

                    try
                    {

                        using (var fs =
                            new FileStream(
                                Path.Combine(App.MyDocuments, "LCrypt", PasswordManagerViewModel.WalletFileName),
                                FileMode.Open, FileAccess.Read, FileShare.None, PasswordManagerViewModel.FileBufferSize,
                                useAsync: true))
                        {
                            var aes = new AesManaged();

                            var salt = new byte[PasswordManagerViewModel.SaltLength];
                            var iv = new byte[aes.BlockSize / 8];

                            await fs.ReadAsync(salt, 0, PasswordManagerViewModel.SaltLength);
                            await fs.ReadAsync(iv, 0, aes.BlockSize / 8);

                            aes.IV = iv;

                            var key = await Password.DeriveKeyAsync(salt, PasswordManagerViewModel.Pbkdf2Iterations,
                                aes.KeySize / 8, CancellationToken.None);
                            aes.Key = key;

                            using (var transform = aes.CreateDecryptor())
                            {
                                using (var cryptoStream = new CryptoStream(fs, transform, CryptoStreamMode.Read))
                                {
                                    using (var xmlReader =
                                        XmlDictionaryReader.CreateBinaryReader(cryptoStream,
                                            XmlDictionaryReaderQuotas.Max))
                                    {
                                        var deserializer = new DataContractSerializer(typeof(PasswordStorage));
                                        var storage = (PasswordStorage)deserializer.ReadObject(xmlReader);
                                        storage.Aes = aes;
                                        storage.Salt = salt;
                                        storage.LastOpened = DateTime.Now;

                                        LoggedInSuccessfully?.Invoke(this, storage);
                                    }
                                }
                            }
                        }
                    }
                    catch (IOException ex)
                    {
                    }
                    catch (CryptographicException ex)
                    {

                    }
                    catch (Exception ex)
                    {
                        // TODO
                    }
                    finally
                    {
                        IsBusy = false;
                    }

                }, _ => Password?.Length > 0);
            }
        }

        public ICommand RegisterCommand
        {
            get
            {
                return new RelayCommand(async _ =>
                {
                    IsBusy = true;

                    try
                    {
                        if (!Password.IsEqual(RepeatedPassword))
                        {
                            IsBusy = false;

                            await DialogCoordinator.Instance.ShowMessageAsync(this,
                                (string)App.LocalizationDictionary["PasswordManager"],
                                (string)App.LocalizationDictionary["PasswordsDontMatch"],
                                MessageDialogStyle.Affirmative,
                                new MetroDialogSettings
                                {
                                    AffirmativeButtonText = (string)App.LocalizationDictionary["Retry"],
                                    CustomResourceDictionary = App.DialogDictionary,
                                    SuppressDefaultResources = true
                                });
                            return;
                        }

                        var salt = Util.GenerateStrongRandomBytes(PasswordManagerViewModel.SaltLength);

                        using (var aes = new AesManaged())
                        {
                            aes.Key = await Password.DeriveKeyAsync(salt, PasswordManagerViewModel.Pbkdf2Iterations,
                                aes.KeySize / 8,
                                CancellationToken.None);

                            aes.IV = Util.GenerateStrongRandomBytes(aes.BlockSize / 8);

                            var storage = await PasswordStorage.CreateDefaultStorageAsync(aes);

                            using (var fs =
                                new FileStream(
                                    Path.Combine(App.MyDocuments, "LCrypt", PasswordManagerViewModel.WalletFileName),
                                    FileMode.Create, FileAccess.Write, FileShare.None,
                                    PasswordManagerViewModel.FileBufferSize, useAsync: true))
                            {
                                await fs.WriteAsync(salt, 0, PasswordManagerViewModel.SaltLength);
                                await fs.WriteAsync(aes.IV, 0, aes.BlockSize / 8);

                                using (var transform = aes.CreateEncryptor())
                                {
                                    using (var cryptoStream = new CryptoStream(fs, transform, CryptoStreamMode.Write))
                                    {
                                        using (var xmlWriter = XmlDictionaryWriter.CreateBinaryWriter(cryptoStream))
                                        {
                                            var serializer = new DataContractSerializer(typeof(PasswordStorage));
                                            serializer.WriteObject(xmlWriter, storage);
                                        }
                                    }
                                }
                            }

                            Password.Dispose();
                            Password = null;
                            RepeatedPassword.Dispose();
                            RepeatedPassword = null;

                            IsRegister = false;
                            IsLogin = true;
                        }
                    }
                    finally
                    {
                        IsBusy = false;
                    }
                }, _ => !string.IsNullOrWhiteSpace(StorageName) && Password?.Length > 0 &&
                        RepeatedPassword?.Length > 0);
            }
        }
    }
}
