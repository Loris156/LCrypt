using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Controls;
using LCrypt.Enumerations;
using LCrypt.Properties;
using MahApps.Metro.Controls;

namespace LCrypt.Algorithms
{
    public static class Checksum
    {
        public static async Task CalculateHash(string path, TextBox output, ProgressRing indicator, Button copyButton, HashAlgorithm algorithm)
        {
            await indicator.Dispatcher.InvokeAsync(() =>
            {
                indicator.IsActive = true;
            });
            var checksum = string.Empty;
            try
            {
                using (var fileStream = new FileStream(path, FileMode.Open,
                    FileAccess.Read, FileShare.Read, 16777216))
                {
                    using (var hash = algorithm.GetAlgorithm())
                    {
                        checksum = BitConverter.ToString(hash.ComputeHash(fileStream)).Replace("-", string.Empty)
                            .ToLower();
                    }
                }

                await copyButton.Dispatcher.InvokeAsync(() => copyButton.IsEnabled = true);
            }
            catch (Exception e)
            {
                checksum = $"{Localization.Error} {e.Message}";
            }
            finally
            {
                await output.Dispatcher.InvokeAsync(() =>
                {
                    output.Text = checksum;
                });

                await indicator.Dispatcher.InvokeAsync(() =>
                {
                    indicator.IsActive = false;
                });
            }
        }
    }
}