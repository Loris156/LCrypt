using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using LCrypt.Algorithms;
using LCrypt.Enumerations;
using LCrypt.Utility;
using MahApps.Metro;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.Win32;
using System.IO;
using System.Media;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using HashAlgorithm = LCrypt.Enumerations.HashAlgorithm;
using Localization = LCrypt.Properties.Localization;
using Path = System.IO.Path;
using System.Windows.Shapes;
using Encoding = LCrypt.Enumerations.Encoding;
using Timer = System.Timers.Timer;

namespace LCrypt
{
    public partial class MainWindow
    {
        private string _selectedFile = string.Empty;
        private FileInfo _selectedFileInfo;
        private string _lengthInMiB = string.Empty;

        private FileInfo _selectedChecksumFile;

        public MainWindow()
        {
            InitializeComponent();
            CoBLanguage.SelectedIndex = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName.Equals("de") ? 0 : 1;
            CoBLanguage.SelectionChanged += CoBLanguage_SelectionChanged;

            CoBAppTheme.SelectedIndex = ThemeManager.DetectAppStyle().Item1.Name.Equals("BaseDark") ? 1 : 0;
            CoBAppTheme.SelectionChanged += CoBAppStyle_OnSelectionChanged;

            RbBase64.Checked += RbHashOutput_CheckedChanged;
            RbHexadecimal.Checked += RbHashOutput_CheckedChanged;

            ChBHexadecimalHyphens.Checked += RbHashOutput_CheckedChanged;
            ChBHexadecimalHyphens.Unchecked += RbHashOutput_CheckedChanged;

            RbTextEncryptHexadecimal.Checked += RbTextEncryptOutputFormat;
            RbTextEncryptBase64.Checked += RbTextEncryptOutputFormat;

            RbTextDecryptHexadecimal.Checked += RbTextDecryptInputFormat;
            RbTextDecryptBase64.Checked += RbTextDecryptInputFormat;

            TblVersion.Text = Assembly.GetExecutingAssembly().GetName().Name + " Version " + Assembly.GetExecutingAssembly().GetName().Version.RemoveTrailingZeros();
        }

        private void BtSettings_OnClick(object sender, RoutedEventArgs e)
        {
            SettingsFlyout.IsOpen = !SettingsFlyout.IsOpen;
            TcSettings.SelectedIndex = 0;
            Separator.Visibility = Visibility.Collapsed;
            BtRestart.Visibility = Visibility.Collapsed;
        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Process.Start(e.Uri.AbsoluteUri);
            e.Handled = true;
        }

        private void CoBLanguage_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (((ComboBox)sender).SelectedIndex)
            {
                case 0: //German
                    App.Settings.Language = "de-DE";
                    break;
                case 1: //English
                    App.Settings.Language = "en-US";
                    break;
            }

            Separator.Visibility = Visibility.Visible;
            BtRestart.Visibility = Visibility.Visible;
        }

        private void CoBAppStyle_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            AppTheme theme;
            switch (((ComboBox)sender).SelectedIndex)
            {
                case 0:
                    theme = ThemeManager.GetAppTheme("BaseLight");
                    break;
                case 1:
                    theme = ThemeManager.GetAppTheme("BaseDark");
                    break;
                default:
                    theme = ThemeManager.GetAppTheme("BaseLight");
                    break;
            }

            ThemeManager.ChangeAppStyle(Application.Current, ThemeManager.DetectAppStyle().Item2, theme);
            App.Settings.Theme = theme.Name;
        }

        private async void BtRestart_OnClick(object sender, RoutedEventArgs e)
        {
            if (await this.ShowMessageAsync(Localization.ApplyChanges, Localization.ReallyRestart,
                    MessageDialogStyle.AffirmativeAndNegative, new MetroDialogSettings
                    {
                        AffirmativeButtonText = Localization.Yes,
                        NegativeButtonText = Localization.No,
                        DefaultButtonFocus = MessageDialogResult.Negative,
                        AnimateShow = true,
                        AnimateHide = false
                    }) == MessageDialogResult.Affirmative)
            {
                App.Restart();
            }
        }

        private void ColorSelection_OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            var rect = sender as Rectangle;
            var newColor = rect?.Uid;
            ThemeManager.ChangeAppStyle(Application.Current, ThemeManager.GetAccent(newColor),
                ThemeManager.DetectAppStyle().Item1);
            App.Settings.Accent = newColor;
        }

        private void ColorSelection_OnMouseEnter(object sender, MouseEventArgs e)
        {
            ((Rectangle)sender).StrokeThickness = 2;
        }

        private void ColorSelection_OnMouseLeave(object sender, MouseEventArgs e)
        {
            ((Rectangle)sender).StrokeThickness = 0;
        }

        private void BtChooseOutputDirectory_OnClick(object sender, RoutedEventArgs e)
        {
            var saveFileDialog = new SaveFileDialog
            {
                AddExtension = true,
                CheckPathExists = true,
                OverwritePrompt = true,
                Title = Localization.FileDestination,
                ValidateNames = true,
                Filter = $"{Localization.AllFiles}(*.*)|*.*",
                InitialDirectory = !string.IsNullOrWhiteSpace(TbFileDestination.Text) &&
                                   TbFileDestination.Text.Length < 248
                    ? Path.GetDirectoryName(TbFileDestination.Text)
                    : Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
            };
            try
            {
                if (saveFileDialog.ShowDialog() == true)
                {
                    TbFileDestination.Text = saveFileDialog.FileName;
                }
            }
            catch (Exception)
            {
                saveFileDialog.InitialDirectory = null;
                if (saveFileDialog.ShowDialog() == true)
                {
                    TbFileDestination.Text = saveFileDialog.FileName;
                }
            }

            CheckFields();
        }

        private void BtChooseFile_OnClick(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                CheckPathExists = true,
                CheckFileExists = true,
                Filter = $"{Localization.AllFiles}(*.*)|*.*",
                ValidateNames = true
            };

            if (openFileDialog.ShowDialog() != true) return;
            if (!File.Exists(openFileDialog.FileName)) return;

            ImportFile(openFileDialog.FileName);
        }

        private void BtChooseFile_DragEnter(object sender, DragEventArgs e)
        {
            e.Effects = e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.Copy : DragDropEffects.None;
        }

        private void BtChooseFile_Drop(object sender, DragEventArgs e)
        {
            var files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files != null)
            {
                if (files.Length == 1)
                {
                    if (File.Exists(files[0]))
                    {
                        ImportFile(files[0]);
                    }
                    else
                    {
                        this.ShowMessageAsync(Localization.DragAndDrop,
                            Localization.DragAndDropInvalidFile, MessageDialogStyle.Affirmative,
                            new MetroDialogSettings
                            {
                                AffirmativeButtonText = "OK",
                                AnimateShow = true,
                                AnimateHide = false
                            });
                    }
                }
                else
                {
                    this.ShowMessageAsync(Localization.DragAndDrop,
                        Localization.DragAndDropSingleFile, MessageDialogStyle.Affirmative,
                        new MetroDialogSettings
                        {
                            AffirmativeButtonText = "OK",
                            AnimateShow = true,
                            AnimateHide = false
                        });
                }
            }
            else
            {
                this.ShowMessageAsync(Localization.DragAndDrop,
                    Localization.DragAndDropError, MessageDialogStyle.Affirmative,
                    new MetroDialogSettings
                    {
                        AffirmativeButtonText = "OK",
                        AnimateShow = true,
                        AnimateHide = false
                    });
            }
        }

        private void ImportFile(string path)
        {
            _selectedFile = path;
            _selectedFileInfo = new FileInfo(_selectedFile);

            var length = _selectedFileInfo.Length;
            double adaptedLength;
            string unit;

            if (length < 1024)
            {
                adaptedLength = length;
                unit = "B";
            }
            else if (length < 1048576)
            {
                adaptedLength = length / 1024d;
                unit = "KiB";
            }
            else if (length < 1073741824)
            {
                adaptedLength = length / 1048576d;
                unit = "MiB";
            }
            else
            {
                adaptedLength = length / 1073741824d;
                unit = "GiB";
            }

            adaptedLength = Math.Round(adaptedLength, 2);
            _lengthInMiB = Math.Round(length / 1048576d, 2).ToString(CultureInfo.InvariantCulture);

            TblSelectedFile.Text =
                $"{Path.GetFileName(_selectedFile)} ({adaptedLength} {unit})";
            ImgSelectedFile.Source = Util.GetIconByFilename(_selectedFile).ToImageSource();

            TbFileDestination.Text =
                $"{Path.GetDirectoryName(_selectedFile)}\\{Path.GetFileNameWithoutExtension(_selectedFile)}_{(RbEncrypt.IsChecked == true ? "Encrypted" : "Decrypted")}{Path.GetExtension(_selectedFile)}";

            CheckFields();
        }

        private async void BtStart_OnClick(object sender, RoutedEventArgs e)
        {
            if (_selectedFile == null || _selectedFileInfo == null || CoBAlgorithm.SelectedItem == null ||
                string.IsNullOrWhiteSpace(TbFileDestination.Text)) return;

            BtChooseFile.IsEnabled = false;
            BtChecksumChooseFile.AllowDrop = false;
            RbEncrypt.IsEnabled = false;
            RbDecrypt.IsEnabled = false;
            TbFileDestination.IsEnabled = false;
            CoBAlgorithm.IsEnabled = false;
            PbPassword.IsEnabled = false;
            RbThisAccount.IsEnabled = false;
            RbThisComputer.IsEnabled = false;
            BtStart.IsEnabled = false;

            PrBFileEncryption.Value = 0;
            PrBFileEncryption.Maximum = _selectedFileInfo.Length;
            TblSpeed.Text = "0 MiB/s";
            TblProgress.Text = $"0 / {_lengthInMiB} MiB";

            try
            {
                var encrypt = RbEncrypt.IsChecked == true;
                string successMessage;

                if (CoBAlgorithm.Text == "Windows Data Protection (DPAPI)")
                {
                    PrBFileEncryption.IsIndeterminate = true;

                    var dpApi = new DpApi(_selectedFileInfo, TbFileDestination.Text,
                        RbThisAccount.IsChecked == true
                            ? DataProtectionScope.CurrentUser
                            : DataProtectionScope.LocalMachine);

                    if (encrypt)
                    {
                        await dpApi.Encrypt();
                        successMessage = Localization.SuccessfullyEncrypted;
                    }
                    else
                    {
                        await dpApi.Decrypt();
                        successMessage = Localization.SuccessfullyDecrypted;
                    }
                }
                else
                {
                    using (var encryptionService = new EncryptionService(
                        ((Algorithm)CoBAlgorithm.SelectedIndex).GetAlgorithm(),
                        _selectedFileInfo,
                        TbFileDestination.Text,
                        PbPassword.Password,
                        new Progress<EncryptionServiceProgress>(progress =>
                    {
                        TblSpeed.Text = $"{progress.MibPerSecond} MiB/s";

                        var progressMiB = Math.Round(progress.ProcessedBytes / (1024d * 1024d), 2);
                        TblProgress.Text = $"{progressMiB} / {_lengthInMiB} MiB";
                        PrBFileEncryption.Value = progress.ProcessedBytes;
                    })))
                    {
                        if (encrypt)
                        {
                            await encryptionService.EncryptAsync();
                            successMessage = Localization.SuccessfullyEncrypted;
                        }
                        else
                        {
                            await encryptionService.DecryptAsync();
                            successMessage = Localization.SuccessfullyDecrypted;
                        }
                    }
                }

                await this.ShowMessageAsync("LCrypt",
                    string.Format(successMessage, _selectedFileInfo.Name, Path.GetFileName(TbFileDestination.Text), CoBAlgorithm.Text),
                    MessageDialogStyle.Affirmative, new MetroDialogSettings
                    {
                        AffirmativeButtonText = "OK",
                        AnimateShow = true,
                        AnimateHide = false
                    });
            }
            catch (Exception)
            {
                await this.ShowMessageAsync("LCrypt",
                    string.Format(Localization.EncryptionDecryptionFailed, _selectedFileInfo.Name),
                    MessageDialogStyle.Affirmative, new MetroDialogSettings
                    {
                        AffirmativeButtonText = "OK",
                        AnimateShow = true,
                        AnimateHide = false
                    });
            }
            finally
            {
                BtChooseFile.IsEnabled = true;
                BtChecksumChooseFile.AllowDrop = false;
                RbEncrypt.IsEnabled = true;
                RbDecrypt.IsEnabled = true;
                TbFileDestination.IsEnabled = true;
                CoBAlgorithm.IsEnabled = true;
                PbPassword.IsEnabled = true;
                RbThisAccount.IsEnabled = true;
                RbThisComputer.IsEnabled = true;
                BtStart.IsEnabled = true;

                PrBFileEncryption.Maximum = 1;
                PrBFileEncryption.Value = 0;
                PrBFileEncryption.IsIndeterminate = false;
                TblSpeed.Text = "- MiB/s";
                TblProgress.Text = "- / - MiB";
            }
        }

        private void CheckFields()
        {
            if (CoBAlgorithm.SelectedIndex == 4)
            {
                SpDpApiSettings.Visibility = Visibility.Visible;
                GrFileEncryptionPassword.Visibility = Visibility.Collapsed;
                PbPassword.Clear();

                if (_selectedFile != null &&
                    _selectedFileInfo != null &&
                    CoBAlgorithm.SelectedItem != null &&
                    !string.IsNullOrWhiteSpace(TbFileDestination.Text)
                )
                    BtStart.IsEnabled = true;
                else
                    BtStart.IsEnabled = false;

                return;
            }

            SpDpApiSettings.Visibility = Visibility.Collapsed;
            GrFileEncryptionPassword.Visibility = Visibility.Visible;

            if (_selectedFile != null &&
                _selectedFileInfo != null &&
                !string.IsNullOrWhiteSpace(PbPassword.Password) &&
                CoBAlgorithm.SelectedItem != null &&
                !string.IsNullOrWhiteSpace(TbFileDestination.Text)
            )
                BtStart.IsEnabled = true;
            else
                BtStart.IsEnabled = false;
        }

        private void PbPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            CheckFields();
        }

        private void CoBAlgorithm_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CheckFields();
        }

        private void CalculateHash()
        {
            if (CoBHashAlgorithm.SelectedItem == null || CoBHashEncoding.SelectedItem == null) return;
            var algorithm = (HashAlgorithm)CoBHashAlgorithm.SelectedIndex;
            var encoding = (Encoding)CoBHashEncoding.SelectedIndex;

            var inputBytes = encoding.GetBytesFromText(TbHashInput.Text);
            byte[] computedHash;
            using (var algorithmProvider = algorithm.GetAlgorithm())
            {
                computedHash = algorithmProvider.ComputeHash(inputBytes);
            }

            if (RbBase64.IsChecked == true)
            {
                TbHashOutput.Text = Convert.ToBase64String(computedHash);
            }
            else if (RbHexadecimal.IsChecked == true)
            {
                var hex = BitConverter.ToString(computedHash);
                if (ChBHexadecimalHyphens.IsChecked == false)
                    hex = hex.Replace("-", string.Empty);
                TbHashOutput.Text = hex.ToLower();
            }
        }

        private void TbHashInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            CalculateHash();
        }

        private void CoBHashAlgorithm_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CalculateHash();
        }

        private void CoBHashEncoding_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CalculateHash();
        }

        private void RbHashOutput_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (ReferenceEquals(sender, RbHexadecimal))
            {
                ChBHexadecimalHyphens.IsEnabled = true;
            }
            else if (ReferenceEquals(sender, RbBase64))
            {
                ChBHexadecimalHyphens.IsChecked = false;
                ChBHexadecimalHyphens.IsEnabled = false;
            }
            CalculateHash();
        }

        private void EncryptText()
        {
            if (CoBTextEncryptAlgorithm.SelectedItem == null || CoBTextEncryptEncoding.SelectedItem == null ||
                string.IsNullOrEmpty(PbTextEncrypt.Password)) return;
            var algorithm = (Algorithm)CoBTextEncryptAlgorithm.SelectedIndex;
            var encoding = (Encoding)CoBTextEncryptEncoding.SelectedIndex;

            var salt = new byte[8];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(salt);
            }

            using (var encryptionAlgorithm = algorithm.GetAlgorithm())
            {
                using (var password = new Rfc2898DeriveBytes(PbTextEncrypt.Password, salt))
                {
                    encryptionAlgorithm.Key = password.GetBytes(encryptionAlgorithm.KeySize / 8);
                    encryptionAlgorithm.IV = password.GetBytes(encryptionAlgorithm.BlockSize / 8);
                }

                var textBytes = encoding.GetBytesFromText(TbTextEncryptInput.Text);
                using (var memoryStream = new MemoryStream())
                {
                    memoryStream.Write(salt, 0, 8);
                    memoryStream.Write(encryptionAlgorithm.IV, 0, encryptionAlgorithm.BlockSize / 8);
                    using (var transform = encryptionAlgorithm.CreateEncryptor())
                    {
                        using (var cryptoStream = new CryptoStream(memoryStream, transform, CryptoStreamMode.Write))
                        {
                            cryptoStream.Write(textBytes, 0, textBytes.Length);
                        }
                    }

                    if (RbTextEncryptBase64.IsChecked == true)
                    {
                        TbTextEncryptOutput.Text = Convert.ToBase64String(memoryStream.ToArray());
                    }
                    else if (RbTextEncryptHexadecimal.IsChecked == true)
                    {
                        TbTextEncryptOutput.Text = BitConverter.ToString(memoryStream.ToArray())
                            .Replace("-", string.Empty).ToLower();
                    }
                }
            }
        }

        private void TbTextEncryptInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            EncryptText();
        }

        private void CoBTextEncrypt_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            EncryptText();
        }

        private void PbTextEncrypt_PasswordChanged(object sender, RoutedEventArgs e)
        {
            EncryptText();
        }

        private void RbTextEncryptOutputFormat(object sender, RoutedEventArgs e)
        {
            EncryptText();
        }

        private void DecryptText()
        {
            if (CoBTextDecryptAlgorithm.SelectedItem == null || CoBTextDecryptEncoding.SelectedItem == null ||
                string.IsNullOrEmpty(PbTextDecrypt.Password)) return;
            try
            {
                byte[] input;
                if (RbTextDecryptHexadecimal.IsChecked == true)
                {
                    var length = TbTextDecryptInput.Text.Length;
                    input = new byte[length / 2];
                    for (var i = 0; i < length; i += 2)
                    {
                        input[i / 2] = Convert.ToByte(TbTextDecryptInput.Text.Substring(i, 2), 16);
                    }
                }
                else if (RbTextDecryptBase64.IsChecked == true)
                {
                    input = Convert.FromBase64String(TbTextDecryptInput.Text);
                }
                else
                    return;

                using (var inputStream = new MemoryStream(input, false))
                {
                    var salt = new byte[8];
                    inputStream.Read(salt, 0, 8);

                    using (var algorithm = ((Algorithm)CoBTextDecryptAlgorithm.SelectedIndex).GetAlgorithm())
                    {
                        using (var password = new Rfc2898DeriveBytes(PbTextDecrypt.Password, salt))
                        {
                            algorithm.Key = password.GetBytes(algorithm.KeySize / 8);
                        }

                        var iv = new byte[algorithm.BlockSize / 8];
                        inputStream.Read(iv, 0, algorithm.BlockSize / 8);
                        algorithm.IV = iv;

                        var encryptedData = new byte[inputStream.Length - (8 + algorithm.BlockSize / 8)];
                        inputStream.Read(encryptedData, 0, (int)inputStream.Length - (8 + algorithm.BlockSize / 8));

                        using (var outputStream = new MemoryStream())
                        {
                            using (var transform = algorithm.CreateDecryptor())
                            {
                                using (var cryptoStream =
                                    new CryptoStream(outputStream, transform, CryptoStreamMode.Write))
                                {
                                    cryptoStream.Write(encryptedData, 0, encryptedData.Length);
                                }
                            }

                            TbTextDecryptOutput.Text = ((Encoding)CoBTextDecryptEncoding.SelectedIndex).GetEncoding()
                                .GetString(outputStream.ToArray());
                        }
                    }
                }
            }
            catch (Exception)
            {
                TbTextDecryptOutput.Clear();
            }
        }

        private void TbTextDecryptInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            DecryptText();
        }

        private void CoBTextDecrypt_SelectionChanged(object sender, RoutedEventArgs e)
        {
            DecryptText();
        }

        private void PbTextDecrypt_PasswordChanged(object sender, RoutedEventArgs e)
        {
            DecryptText();
        }

        private void RbTextDecryptInputFormat(object sender, RoutedEventArgs e)
        {
            DecryptText();
        }

        private async void ImportChecksumFile(string path)
        {
            _selectedChecksumFile = new FileInfo(path);
            BtChecksumChooseFile.IsEnabled = false;
            BtChecksumChooseFile.AllowDrop = false;
            BtChecksumExport.IsEnabled = false;

            TbChecksumMd5.Clear();
            TbChecksumSha1.Clear();
            TbChecksumSha256.Clear();
            TbChecksumSha384.Clear();
            TbChecksumSha512.Clear();

            BtCopyMd5.IsEnabled = false;
            BtCopySha1.IsEnabled = false;
            BtCopySha256.IsEnabled = false;
            BtCopySha384.IsEnabled = false;
            BtCopySha512.IsEnabled = false;

            var length = _selectedChecksumFile.Length;
            double adaptedLength;
            string unit;

            if (length < 1024)
            {
                adaptedLength = length;
                unit = "B";
            }
            else if (length < 1048576)
            {
                adaptedLength = length / 1024d;
                unit = "KiB";
            }
            else if (length < 1073741824)
            {
                adaptedLength = length / 1048576d;
                unit = "MiB";
            }
            else
            {
                adaptedLength = length / 1073741824d;
                unit = "GiB";
            }

            adaptedLength = Math.Round(adaptedLength, 2);
            _lengthInMiB = Math.Round(length / 1048576d, 2).ToString(CultureInfo.InvariantCulture);

            TblChecksumSelectedFile.Text =
                $"{Path.GetFileName(_selectedChecksumFile.FullName)} ({adaptedLength} {unit})";
            ImgChecksumSelectedFile.Source = Util.GetIconByFilename(_selectedChecksumFile.FullName).ToImageSource();

            await Task.WhenAll(
                Task.Run(async () => await Checksum.CalculateHash(_selectedChecksumFile.FullName, TbChecksumMd5,
                    PrMd5, BtCopyMd5, HashAlgorithm.Md5)),
                Task.Run(async () => await Checksum.CalculateHash(_selectedChecksumFile.FullName, TbChecksumSha1,
                    PrSha1, BtCopySha1, HashAlgorithm.Sha1)),
                Task.Run(async () => await Checksum.CalculateHash(_selectedChecksumFile.FullName, TbChecksumSha256,
                    PrSha256, BtCopySha256, HashAlgorithm.Sha256)),
                Task.Run(async () => await Checksum.CalculateHash(_selectedChecksumFile.FullName, TbChecksumSha384,
                    PrSha384, BtCopySha384, HashAlgorithm.Sha384)),
                Task.Run(async () => await Checksum.CalculateHash(_selectedChecksumFile.FullName, TbChecksumSha512,
                    PrSha512, BtCopySha512, HashAlgorithm.Sha512)));

            BtChecksumChooseFile.IsEnabled = true;
            BtChecksumChooseFile.AllowDrop = true;
            BtChecksumExport.IsEnabled = true;
        }

        private void BtCopyChecksum_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedChecksumFile == null) return;

            var button = sender as Button;
            if (button == null) return;
            switch (button.Name)
            {
                case "BtCopyMd5":
                    Clipboard.SetText(TbChecksumMd5.Text);
                    break;
                case "BtCopySha1":
                    Clipboard.SetText(TbChecksumSha1.Text);
                    break;
                case "BtCopySha256":
                    Clipboard.SetText(TbChecksumSha256.Text);
                    break;
                case "BtCopySha384":
                    Clipboard.SetText(TbChecksumSha384.Text);
                    break;
                case "BtCopySha512":
                    Clipboard.SetText(TbChecksumSha512.Text);
                    break;
                default:
                    Clipboard.SetText("Unknown checksum!");
                    break;
            }
        }

        private void BtChecksumChooseFile_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                CheckPathExists = true,
                CheckFileExists = true,
                Filter = $"{Localization.AllFiles}(*.*)|*.*",
                ValidateNames = true
            };

            if (openFileDialog.ShowDialog() != true) return;
            if (!File.Exists(openFileDialog.FileName)) return;

            ImportChecksumFile(openFileDialog.FileName);
        }

        private void BtChecksumChooseFile_DragEnter(object sender, DragEventArgs e)
        {
            e.Effects = e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.Copy : DragDropEffects.None;
        }

        private void BtChecksumChooseFile_Drop(object sender, DragEventArgs e)
        {
            var files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files != null)
            {
                if (files.Length == 1)
                {
                    if (File.Exists(files[0]))
                    {
                        ImportChecksumFile(files[0]);
                    }
                    else
                    {
                        this.ShowMessageAsync(Localization.DragAndDrop,
                            Localization.DragAndDropInvalidFile, MessageDialogStyle.Affirmative,
                            new MetroDialogSettings
                            {
                                AffirmativeButtonText = "OK",
                                AnimateShow = true,
                                AnimateHide = false
                            });
                    }
                }
                else
                {
                    this.ShowMessageAsync(Localization.DragAndDrop,
                        Localization.DragAndDropSingleFile, MessageDialogStyle.Affirmative,
                        new MetroDialogSettings
                        {
                            AffirmativeButtonText = "OK",
                            AnimateShow = true,
                            AnimateHide = false
                        });
                }
            }
            else
            {
                this.ShowMessageAsync(Localization.DragAndDrop,
                    Localization.DragAndDropError, MessageDialogStyle.Affirmative,
                    new MetroDialogSettings
                    {
                        AffirmativeButtonText = "OK",
                        AnimateShow = true,
                        AnimateHide = false
                    });
            }
        }

        private async void BtChecksumExport_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedChecksumFile == null) return;

            var saveFileDialog = new SaveFileDialog
            {
                AddExtension = true,
                CheckPathExists = true,
                DefaultExt = ".txt",
                Filter = $"Text|*.txt|{Localization.AllFiles}(*.*)|*.*",
                ValidateNames = true,
                OverwritePrompt = true,
                FileName = _selectedChecksumFile.Name + " - LCrypt Checksums"
            };

            if (saveFileDialog.ShowDialog() != true)
                return;

            try
            {
                using (var stream = new StreamWriter(saveFileDialog.FileName))
                {
                    await stream.WriteLineAsync(
                        $"LCrypt {Assembly.GetExecutingAssembly().GetName().Version} generated Hash Export");
                    await stream.WriteLineAsync($"Executed on: {DateTime.Now.ToString(CultureInfo.CurrentCulture)}");
                    await stream.WriteLineAsync($"by {System.Security.Principal.WindowsIdentity.GetCurrent().Name}");
                    await stream.WriteLineAsync("");
                    await stream.WriteLineAsync($"Generated for file: {_selectedChecksumFile.FullName}");
                    await stream.WriteLineAsync("");
                    await stream.WriteLineAsync($"MD5: {TbChecksumMd5.Text}");
                    await stream.WriteLineAsync($"SHA-1: {TbChecksumSha1.Text}");
                    await stream.WriteLineAsync($"SHA-256: {TbChecksumSha256.Text}");
                    await stream.WriteLineAsync($"SHA-384: {TbChecksumSha384.Text}");
                    await stream.WriteLineAsync($"SHA-512: {TbChecksumSha512.Text}");
                }

                await this.ShowMessageAsync("Export", Localization.SuccessfullyExported,
                    MessageDialogStyle.Affirmative, new MetroDialogSettings
                    {
                        AffirmativeButtonText = "OK",
                        AnimateShow = true,
                        AnimateHide = false
                    });
            }
            catch (Exception ex)
            {
                await this.ShowMessageAsync("Export", string.Format(Localization.ErrorWhileExporting, ex.Message),
                    MessageDialogStyle.Affirmative, new MetroDialogSettings
                    {
                        AffirmativeButtonText = "OK",
                        AnimateShow = true,
                        AnimateHide = false
                    });
            }
        }

        private void BtGeneratePasswords_Click(object sender, RoutedEventArgs e)
        {
            var contentBuilder = new StringBuilder();

            if (CbPasswordConsonants.IsChecked == true)
            {
                switch (CoBPasswordLetters.SelectedIndex)
                {
                    case 0:
                        contentBuilder.Append("qQwWrRtTzZpPsSdDfFgGhHjJkKlLyYxXcCvVbBnNmM");
                        break;
                    case 1:
                        contentBuilder.Append("QWRTZPSDFGHJKLYXCVBNM");
                        break;
                    case 2:
                        contentBuilder.Append("qwrtzpsdfghjklyxcvbnm");
                        break;
                }
            }

            if (CbPasswordVocals.IsChecked == true)
            {
                switch (CoBPasswordLetters.SelectedIndex)
                {
                    case 0:
                        contentBuilder.Append("aAeEiIoOuU");
                        break;
                    case 1:
                        contentBuilder.Append("AEIOU");
                        break;
                    case 2:
                        contentBuilder.Append("aeiou");
                        break;
                }
            }

            if (CbPasswordNumbers.IsChecked == true)
                contentBuilder.Append("0123456789");
            if (CbPasswordSymbols.IsChecked == true)
                contentBuilder.Append(@"!§$@%&/()=?\}][{<>|,;.:-_#+*~^");

            var passwordList = new List<string>(0);
            if (!NudPasswordLength.Value.HasValue)
                return;
            var length = NudPasswordLength.Value;

            if (contentBuilder.Length < 1)
                return;
            var content = contentBuilder.ToString();
            using (var random = new RNGCryptoServiceProvider())
            {
                for (var pw = 0; pw < NudPasswordCount.Value; ++pw)
                {
                    var password = new StringBuilder();
                    var pwLength = length;
                    while (0 < pwLength--)
                        password.Append(content[random.GetNextInt32(content.Length)]);
                    passwordList.Add(password.ToString());
                }
            }

            foreach (var password in passwordList)
            {
                LbPasswords.Items.Add(password);
            }
        }

        private void BtClearPasswords_Click(object sender, RoutedEventArgs e)
        {
            LbPasswords.Items.Clear();
        }

        private void BtCopyPassword_Click(object sender, RoutedEventArgs e)
        {
            if (LbPasswords.SelectedItem == null) return;
            var timer = new Timer(TimeSpan.FromSeconds(10).TotalMilliseconds);
            timer.Start();
            timer.Elapsed += (o, ev) =>
            {
                SystemSounds.Asterisk.Play();
                var resetThread = new Thread(Clipboard.Clear); // Clipboard must be cleared from a different thread.
                resetThread.SetApartmentState(ApartmentState.STA);
                resetThread.Start();

                timer.Stop();
                timer.Dispose();
            };
            Clipboard.SetText((string)LbPasswords.SelectedItem);
        }

        private void BtCopyAllPasswords_Click(object sender, RoutedEventArgs e)
        {
            if (LbPasswords.Items.Count < 1) return;

            var passwordBuilder = new StringBuilder();
            foreach (var password in LbPasswords.Items)
            {
                passwordBuilder.AppendLine(password as string);
            }

            var timer = new Timer(TimeSpan.FromSeconds(10).TotalMilliseconds);
            timer.Start();
            timer.Elapsed += (o, ev) =>
            {
                SystemSounds.Asterisk.Play();
                var resetThread = new Thread(Clipboard.Clear); // Clipboard must be cleared from a different thread.
                resetThread.SetApartmentState(ApartmentState.STA);
                resetThread.Start();

                timer.Stop();
                timer.Dispose();
            };
            Clipboard.SetText(passwordBuilder.ToString());
        }

        private Timer _clearPasswordTimer;

        private void PbPasswordCheck_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (_clearPasswordTimer == null)
            {
                _clearPasswordTimer = new Timer(TimeSpan.FromSeconds(5).TotalMilliseconds);
                _clearPasswordTimer.Elapsed += (o, ev) =>
                {
                    PbPasswordCheck.Dispatcher.Invoke(() => PbPasswordCheck.Clear());
                };
                _clearPasswordTimer.AutoReset = false;
            }
            _clearPasswordTimer.Stop();
            _clearPasswordTimer.Start();

            TbPasswordLength.Text = PbPasswordCheck.Password.Length.ToString();

            int letters = 0, numbers = 0, symbols = 0;
            foreach (var character in PbPasswordCheck.Password)
            {
                if (char.IsLetter(character))
                {
                    letters++;
                    continue;
                }
                if (char.IsDigit(character))
                {
                    numbers++;
                    continue;
                }

                if (!Regex.Match(character.ToString(), @"[!,§,@,$,%,&,/,(,),=,?,},\],\[,{,<,>,|,,,;,.,:,-,_,#,+,*,~,^]",
                    RegexOptions.ECMAScript).Success) continue;
                symbols++;
            }

            TbPasswordLetters.Text = letters.ToString();
            TbPasswordNumbers.Text = numbers.ToString();
            TbPasswordSymbols.Text = symbols.ToString();

            var strength = PasswordCheck.GetStrength(PbPasswordCheck.Password);

            switch (strength)
            {
                case PasswordStrength.Blank:
                    TblPasswordStrength.Text = "-";
                    PrBPasswordStrength.Value = 0;
                    break;
                case PasswordStrength.VeryWeak:
                    TblPasswordStrength.Text = Localization.VeryWeak;
                    PrBPasswordStrength.Value = 20;
                    PrBPasswordStrength.Foreground = new SolidColorBrush(Color.FromRgb(224, 11, 11));
                    break;
                case PasswordStrength.Weak:
                    TblPasswordStrength.Text = Localization.Weak;
                    PrBPasswordStrength.Value = 40;
                    PrBPasswordStrength.Foreground = new SolidColorBrush(Color.FromRgb(169, 113, 10));
                    break;
                case PasswordStrength.Medium:
                    TblPasswordStrength.Text = Localization.Medium;
                    PrBPasswordStrength.Value = 60;
                    PrBPasswordStrength.Foreground = new SolidColorBrush(Color.FromRgb(8, 111, 158));
                    break;
                case PasswordStrength.Strong:
                    TblPasswordStrength.Text = Localization.Strong;
                    PrBPasswordStrength.Value = 80;
                    PrBPasswordStrength.Foreground = new SolidColorBrush(Color.FromRgb(11, 68, 7));
                    break;
                case PasswordStrength.VeryStrong:
                    TblPasswordStrength.Text = Localization.VeryStrong;
                    PrBPasswordStrength.Value = 100;
                    PrBPasswordStrength.Foreground = new SolidColorBrush(Color.FromRgb(41, 235, 26));
                    break;
                default:
                    TblPasswordStrength.Text = "-";
                    PrBPasswordStrength.Value = 0;
                    break;
            }
            var commonWord = PasswordCheck.IsCommonPassword(PbPasswordCheck.Password);
            if (commonWord != null)
            {
                TblPasswordBadWord.Visibility = Visibility.Visible;
                TblPasswordHover.Visibility = Visibility.Visible;
                TblPasswordHover.ToolTip = commonWord;
            }
            else
            {
                TblPasswordBadWord.Visibility = Visibility.Hidden;
                TblPasswordHover.Visibility = Visibility.Hidden;
            }
        }
    }
}
