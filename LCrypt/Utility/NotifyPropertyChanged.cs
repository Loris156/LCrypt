using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace LCrypt.Utility
{
    /// <summary>
    /// Simple functions for dealing with INotifyPropertyChanged.
    /// </summary>
    [DataContract]
    public class NotifyPropertyChanged : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Sets the value of a private field and invokes OnPropertyChanged.
        /// </summary>
        /// <typeparam name="T">Type of private field / property</typeparam>
        /// <param name="field">ref to private field</param>
        /// <param name="value">new value of property</param>
        /// <param name="propertyName">Name of changed property. Can be ommited in most cases.</param>
        /// <example>
        /// How to use SetAndNotify:
        /// <code>
        /// private int _count;
        /// 
        /// public int Count
        /// {
        ///     get => _count;
        ///     set => SetAndNotify(ref _int, value);
        /// }
        /// </code>
        /// </example>
        protected void SetAndNotify<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (ReferenceEquals(field, value)) return;
            if (propertyName == null) return;
            field = value;
            OnPropertyChanged(propertyName);
        }
    }
}
