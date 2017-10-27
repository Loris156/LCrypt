namespace LCrypt.Interfaces
{
    /// <summary>
    /// Represents an asynchronous with a known run length. 
    /// </summary>
    public interface ITaskWithProgress : ITask
    {
        /// <summary>
        /// Progress of this task. Must be between 0 and 100.
        /// </summary>
        double Progress { get; set; }
    }
}
