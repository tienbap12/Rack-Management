using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Rack.MainInfrastructure.Services
{
    public class TaskInfo
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; }
        public Task Task { get; set; }
        public TaskStatus Status => Task.Status;
        public object Result { get; set; }
        public Exception Exception { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? CompletedAt { get; set; }
    }

    public interface IBackgroundTaskQueue
    {
        Guid QueueTask(string name, Func<CancellationToken, Task<object>> workItem);
        TaskInfo GetTaskInfo(Guid taskId);
        ConcurrentDictionary<Guid, TaskInfo> GetAllTasks(int maxResults = 100);
    }

    public class BackgroundTaskQueue : IBackgroundTaskQueue
    {
        private readonly ConcurrentDictionary<Guid, TaskInfo> _runningTasks = new ConcurrentDictionary<Guid, TaskInfo>();
        private readonly ConcurrentQueue<TaskInfo> _completedTasks = new ConcurrentQueue<TaskInfo>();
        private readonly int _maxCompletedTasks = 1000;

        public Guid QueueTask(string name, Func<CancellationToken, Task<object>> workItem)
        {
            if (workItem == null)
            {
                throw new ArgumentNullException(nameof(workItem));
            }

            var taskInfo = new TaskInfo
            {
                Name = name,
                CreatedAt = DateTime.UtcNow
            };

            _runningTasks.TryAdd(taskInfo.Id, taskInfo);
            return taskInfo.Id;
        }

        public TaskInfo GetTaskInfo(Guid taskId)
        {
            if (_runningTasks.TryGetValue(taskId, out var taskInfo))
            {
                return taskInfo;
            }

            return _completedTasks.FirstOrDefault(t => t.Id == taskId);
        }

        public ConcurrentDictionary<Guid, TaskInfo> GetAllTasks(int maxResults = 100)
        {
            var result = new ConcurrentDictionary<Guid, TaskInfo>();

            // Add running tasks first
            foreach (var task in _runningTasks.Values.Take(maxResults))
            {
                result.TryAdd(task.Id, task);
            }

            // Add completed tasks until we reach the limit
            int remainingCount = maxResults - result.Count;
            if (remainingCount > 0)
            {
                foreach (var task in _completedTasks.Take(remainingCount))
                {
                    result.TryAdd(task.Id, task);
                }
            }

            return result;
        }

        public void MarkTaskCompleted(Guid taskId, object result = null, Exception exception = null)
        {
            if (_runningTasks.TryRemove(taskId, out var taskInfo))
            {
                taskInfo.Result = result;
                taskInfo.Exception = exception;
                taskInfo.CompletedAt = DateTime.UtcNow;

                _completedTasks.Enqueue(taskInfo);

                // Keep the completed tasks queue from growing too large
                while (_completedTasks.Count > _maxCompletedTasks && _completedTasks.TryDequeue(out _))
                {
                }
            }
        }

        public void StartTask(Guid taskId, Task task)
        {
            if (_runningTasks.TryGetValue(taskId, out var taskInfo))
            {
                taskInfo.Task = task;
            }
        }
    }

    public class BackgroundTaskService : BackgroundService
    {
        private readonly IBackgroundTaskQueue _taskQueue;
        private readonly ILogger<BackgroundTaskService> _logger;
        private readonly IServiceProvider _serviceProvider;

        public BackgroundTaskService(
            IBackgroundTaskQueue taskQueue,
            ILogger<BackgroundTaskService> logger,
            IServiceProvider serviceProvider)
        {
            _taskQueue = taskQueue;
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        public async Task<Guid> QueueTaskAsync(string name, Func<CancellationToken, Task<object>> workItem)
        {
            var taskId = _taskQueue.QueueTask(name, workItem);

            // Create a task that will be executed asynchronously
            var task = ExecuteTaskInternal(taskId, workItem, CancellationToken.None);

            // Store the task in the queue for status tracking
            ((BackgroundTaskQueue)_taskQueue).StartTask(taskId, task);

            return taskId;
        }

        private async Task ExecuteTaskInternal(Guid taskId, Func<CancellationToken, Task<object>> workItem, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Starting background task {TaskId} - {TaskName}", taskId, _taskQueue.GetTaskInfo(taskId)?.Name);

                using (var scope = _serviceProvider.CreateScope())
                {
                    // Execute the work item
                    var result = await workItem(cancellationToken);

                    // Mark task as completed
                    ((BackgroundTaskQueue)_taskQueue).MarkTaskCompleted(taskId, result);

                    _logger.LogInformation("Background task {TaskId} completed successfully", taskId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred executing background task {TaskId}", taskId);
                ((BackgroundTaskQueue)_taskQueue).MarkTaskCompleted(taskId, null, ex);
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Background Task Service is running");

            while (!stoppingToken.IsCancellationRequested)
            {
                // Keep the service alive
                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            }
        }
    }
}