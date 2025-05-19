using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Rack.Application.Commons.DTOs;
using Rack.Domain.Commons.Primitives;
using Rack.Domain.Enum;
using Rack.MainInfrastructure.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Rack.API.Controllers.V1
{
    [ApiController]
    [Route("api/v1/tasks")]
    public class TasksController : ControllerBase
    {
        private readonly IBackgroundTaskQueue _taskQueue;
        private readonly BackgroundTaskService _backgroundTaskService;
        private readonly ILogger<TasksController> _logger;

        public TasksController(
            IBackgroundTaskQueue taskQueue,
            BackgroundTaskService backgroundTaskService,
            ILogger<TasksController> logger)
        {
            _taskQueue = taskQueue;
            _backgroundTaskService = backgroundTaskService;
            _logger = logger;
        }

        [HttpGet]
        public ActionResult<ApiResponseBaseDto> GetAllTasks([FromQuery] int limit = 50)
        {
            try
            {
                var tasks = _taskQueue.GetAllTasks(limit);
                var taskDtos = tasks.Values.Select(t => new
                {
                    Id = t.Id,
                    Name = t.Name,
                    Status = t.Status.ToString(),
                    CreatedAt = t.CreatedAt,
                    CompletedAt = t.CompletedAt,
                    HasError = t.Exception != null,
                    ErrorMessage = t.Exception?.Message
                });

                return new SuccessApiResponseDto<object>(
                    "Lấy danh sách tác vụ thành công",
                    HttpStatusCodeEnum.OK,
                    taskDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách tác vụ");
                return new FailureApiResponseDto(
                    "Lỗi hệ thống khi lấy danh sách tác vụ",
                    HttpStatusCodeEnum.InternalServerError,
                    new ErrorDto(HttpStatusCodeEnum.InternalServerError, ex.Message, ErrorType.InternalServerError));
            }
        }

        [HttpGet("{taskId}")]
        public ActionResult<ApiResponseBaseDto> GetTaskById(Guid taskId)
        {
            try
            {
                var taskInfo = _taskQueue.GetTaskInfo(taskId);
                if (taskInfo == null)
                {
                    return new FailureApiResponseDto(
                        "Không tìm thấy tác vụ",
                        HttpStatusCodeEnum.NotFound,
                        new ErrorDto(HttpStatusCodeEnum.NotFound, $"Tác vụ với ID {taskId} không tồn tại", ErrorType.Failure));
                }

                var taskDto = new
                {
                    Id = taskInfo.Id,
                    Name = taskInfo.Name,
                    Status = taskInfo.Status.ToString(),
                    CreatedAt = taskInfo.CreatedAt,
                    CompletedAt = taskInfo.CompletedAt,
                    HasError = taskInfo.Exception != null,
                    ErrorMessage = taskInfo.Exception?.Message,
                    Result = taskInfo.Result
                };

                return new SuccessApiResponseDto<object>(
                    "Lấy thông tin tác vụ thành công",
                    HttpStatusCodeEnum.OK,
                    taskDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy thông tin tác vụ {TaskId}", taskId);
                return new FailureApiResponseDto(
                    "Lỗi hệ thống khi lấy thông tin tác vụ",
                    HttpStatusCodeEnum.InternalServerError,
                    new ErrorDto(HttpStatusCodeEnum.InternalServerError, ex.Message, ErrorType.InternalServerError));
            }
        }

        // Example of starting a long-running task - this is just a demo endpoint
        [HttpPost("demo-long-task")]
        public async Task<ActionResult<ApiResponseBaseDto>> StartDemoLongTask(
            [FromQuery] int durationSeconds = 30,
            CancellationToken cancellationToken = default)
        {
            try
            {
                // Queue a demo task that just waits for the specified number of seconds
                var taskId = await _backgroundTaskService.QueueTaskAsync(
                    $"Demo long task ({durationSeconds}s)",
                    async (ct) =>
                    {
                        await Task.Delay(TimeSpan.FromSeconds(durationSeconds), ct);
                        return new { Message = $"Task completed after {durationSeconds} seconds" };
                    });

                return new SuccessApiResponseDto<object>(
                    "Tác vụ dài hạn đã được khởi tạo",
                    HttpStatusCodeEnum.Accepted,
                    new { TaskId = taskId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi khởi tạo tác vụ dài hạn");
                return new FailureApiResponseDto(
                    "Lỗi hệ thống khi khởi tạo tác vụ dài hạn",
                    HttpStatusCodeEnum.InternalServerError,
                    new ErrorDto(HttpStatusCodeEnum.InternalServerError, ex.Message, ErrorType.InternalServerError));
            }
        }
    }
}