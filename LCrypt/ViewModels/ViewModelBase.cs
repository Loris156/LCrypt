using LCrypt.Utility;

namespace LCrypt.ViewModels
{
    /// <summary>
    /// Abstract base class for all LCrypt ViewModels
    /// </summary>
    public abstract class ViewModelBase : NotifyPropertyChanged
    {
        /// <summary>
        /// Gets called when the user requests a window close
        /// </summary>
        /// <returns>True if the ViewModel allows closing, false if not.</returns>
        public virtual bool OnClosing() => true;
    }
}
