using System.Windows.Controls;
using System.Windows.Input;

namespace LCrypt.Views
{
    public partial class FileEncryptionView
    {
        public FileEncryptionView()
        {
            InitializeComponent();
        }

        private void ListBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ((ListBox)sender).SelectedItem = null;
        }
    }
}
