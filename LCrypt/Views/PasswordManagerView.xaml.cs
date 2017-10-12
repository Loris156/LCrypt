using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace LCrypt.Views
{
    public partial class PasswordManagerView
    {
        public PasswordManagerView()
        {
            InitializeComponent();
        }

        private void EntryListBox_OnPreviewLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var hitTest = VisualTreeHelper.HitTest(this, e.GetPosition(this));
            if (hitTest.VisualHit.GetType() == typeof(ListBox) || hitTest.VisualHit.GetType() == typeof(ScrollViewer))
                ((ListBox) sender).UnselectAll();
        }

        private void PasswordManagerView_OnLoaded(object sender, RoutedEventArgs e) => Focus();
    }
}
