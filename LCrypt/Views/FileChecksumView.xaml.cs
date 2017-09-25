using System.Windows;
using System.Windows.Controls;
using LCrypt.Utility.Extensions;
using LCrypt.ViewModels;

namespace LCrypt.Views
{
    public partial class FileChecksumView
    {
        public FileChecksumView()
        {
            InitializeComponent();
        }

        private void VerificationTextBox_OnGotFocus(object sender, RoutedEventArgs e)
        {
            var textBox = (TextBox) sender;
            if (!Clipboard.ContainsText()) return;
            if (Clipboard.GetText().IsHex())
                ((FileChecksumViewModel) DataContext).SelectedTask.Verification =
                    textBox.Text = Clipboard.GetText();
        }
    }
}