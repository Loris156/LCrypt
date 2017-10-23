namespace LCrypt.Interfaces
{
    public interface ITaskWithProgress : ITask
    {
        double Progress { get; set; }
    }
}
