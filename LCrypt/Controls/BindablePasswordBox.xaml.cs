using System.Security;
using System.Windows;
using System.Windows.Controls;

namespace LCrypt.Controls
{
    public partial class BindablePasswordBox
    {
        public static readonly DependencyProperty SecurePasswordProperty;

        static BindablePasswordBox()
        {
            SecurePasswordProperty = DependencyProperty.Register("SecurePassword", typeof(SecureString),
                typeof(BindablePasswordBox),
                new FrameworkPropertyMetadata(default(SecureString),
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
        }

        public BindablePasswordBox()
        {
            InitializeComponent();
        }

        public SecureString SecurePassword
        {
            get => (SecureString)GetValue(SecurePasswordProperty);
            set => SetValue(SecurePasswordProperty, value);
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            SecurePassword = ((PasswordBox)sender).SecurePassword;
        }

        private void UserControl_GotFocus(object sender, RoutedEventArgs e)
        {
            PasswordBox.Focus();
        }
    }
}
