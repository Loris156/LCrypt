using LCrypt.Utility.Extensions;
using MaterialDesignThemes.Wpf;
using System;
using System.Diagnostics;
using System.Security;
using System.Windows;
using System.Windows.Input;

namespace LCrypt.Controls
{
    public partial class PeekPasswordBox
    {
        public static readonly DependencyProperty SecurePasswordProperty;

        static PeekPasswordBox()
        {
            SecurePasswordProperty = DependencyProperty.Register("SecurePassword", typeof(SecureString),
                typeof(PeekPasswordBox),
                new FrameworkPropertyMetadata(default(SecureString),
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, SecureStringProperty_OnPropertyChanged));
        }

        private static void SecureStringProperty_OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Debug.Assert(d is PeekPasswordBox);
            var passwordBox = (PeekPasswordBox)d;
            passwordBox.PasswordBox.PasswordChanged -= passwordBox.PasswordBox_PasswordChanged;

            if (e.NewValue == null)
            {
                passwordBox.PasswordBox.Clear();
                passwordBox.ShowPasswordButton.IsEnabled = false;
                passwordBox.PasswordBox.PasswordChanged += passwordBox.PasswordBox_PasswordChanged;
            }
            else
                throw new InvalidOperationException("Cannot set new SecureString.");           
        }

        public PeekPasswordBox()
        {
            InitializeComponent();
        }

        public SecureString SecurePassword
        {
            get => (SecureString)GetValue(SecurePasswordProperty);
            set => SetValue(SecurePasswordProperty, value);
        }

        private void ShowPasswordButton_OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            PasswordBox.Visibility = Visibility.Hidden;

            TextBox.Text = SecurePassword.ToInsecureString();
            TextBox.Visibility = Visibility.Visible;
            EyeIcon.Kind = PackIconKind.EyeOff;
        }

        private void ShowPasswordButton_OnPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            PasswordBox.Visibility = Visibility.Visible;

            TextBox.Text = string.Empty;
            TextBox.Visibility = Visibility.Hidden;
            EyeIcon.Kind = PackIconKind.Eye;

            GC.Collect();
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            SecurePassword = PasswordBox.SecurePassword;
            ShowPasswordButton.IsEnabled = SecurePassword?.Length > 0;
        }
    }
}
