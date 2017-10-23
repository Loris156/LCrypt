using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shell;
using System.Windows.Threading;
using LCrypt.Interfaces;

namespace LCrypt.Utility
{
    public class TaskbarProgressManager
    {
        private readonly TaskbarItemInfo _taskbarItem;
        private readonly ConcurrentDictionary<ITaskWithProgress, double> _progresses;
        private readonly ConcurrentBag<ITask> _indeterminateProgresses;

        public TaskbarProgressManager(TaskbarItemInfo item)
        {
            _taskbarItem = item;
            _progresses = new ConcurrentDictionary<ITaskWithProgress, double>();
            _indeterminateProgresses = new ConcurrentBag<ITask>();

            var updateItemTimer = new DispatcherTimer(DispatcherPriority.Normal) {Interval = TimeSpan.FromMilliseconds(100)};
            updateItemTimer.Tick += UpdateItemTimer_OnTick;
            updateItemTimer.Start();
        }

        public void SetProgress(ITaskWithProgress task, double value)
        {
            _progresses[task] = value;
        }

        public void Remove(ITaskWithProgress task)
        {
            _progresses.TryRemove(task, out var ignored);
        }

        public void SetIndeterminate(ITask task)
        {
            _indeterminateProgresses.Add(task);
        }

        public void Remove(ITask task)
        {
            _indeterminateProgresses.TryTake(out var ignored);
        }
    
        private void UpdateItemTimer_OnTick(object sender, EventArgs e)
        {
            if (_indeterminateProgresses.Count > 0)
            {
                _taskbarItem.ProgressState = TaskbarItemProgressState.Indeterminate;
                _taskbarItem.ProgressValue = 0;
            }
            else if (_progresses.Count == 0)
            {
                _taskbarItem.ProgressState = TaskbarItemProgressState.None;
                _taskbarItem.ProgressValue = 0;
            }
            else
            {
                _taskbarItem.ProgressState = TaskbarItemProgressState.Normal;
                _taskbarItem.ProgressValue = _progresses.Values.Average() / 100;
            }
        }
    }
}
