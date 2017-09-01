using System;
using System.Globalization;
using System.Windows.Input;

namespace LCrypt
{
    public class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Predicate<object> _canExecute;

        public RelayCommand(Action<object> execute, Predicate<object> canExecute, KeyGesture keyGesture = null)
        {
            _execute = execute;
            _canExecute = canExecute;
            Gesture = keyGesture;
        }

        public RelayCommand(Action<object> execute, KeyGesture keyGesture = null)
            : this(execute, _ => true, keyGesture)
        {
        }

        public KeyGesture Gesture { get; }

        public Key Key => Gesture?.Key ?? Key.None;

        public ModifierKeys Modifiers => Gesture?.Modifiers ?? ModifierKeys.None;

        public string GestureText => Gesture?.GetDisplayStringForCulture(CultureInfo.CurrentCulture);

        public bool CanExecute(object parameter)
        {
            return _canExecute(parameter);
        }

        public void Execute(object parameter)
        {
            _execute(parameter);
        }

        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
    }
}