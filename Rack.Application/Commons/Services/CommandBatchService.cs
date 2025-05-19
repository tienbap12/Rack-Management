using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Rack.Application.Commons.Services
{
    /// <summary>
    /// Service for efficiently handling batches of commands
    /// </summary>
    public class CommandBatchService
    {
        private readonly IMediator _mediator;
        private readonly ILogger<CommandBatchService> _logger;

        public CommandBatchService(IMediator mediator, ILogger<CommandBatchService> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// Process a batch of commands with parallelism but controlled concurrency
        /// </summary>
        public async Task<IEnumerable<TResponse>> ProcessBatchAsync<TCommand, TResponse>(
            IEnumerable<TCommand> commands,
            int maxConcurrency = 10,
            CancellationToken cancellationToken = default)
            where TCommand : IRequest<TResponse>
        {
            var commandArray = commands.ToArray();
            var results = new ConcurrentBag<TResponse>();
            var exceptions = new ConcurrentBag<Exception>();

            // Use SemaphoreSlim to control concurrency
            using var semaphore = new SemaphoreSlim(maxConcurrency);
            var tasks = new List<Task>();

            foreach (var command in commandArray)
            {
                await semaphore.WaitAsync(cancellationToken);

                tasks.Add(Task.Run(async () =>
                {
                    try
                    {
                        var result = await _mediator.Send(command, cancellationToken);
                        results.Add(result);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing command batch item of type {CommandType}", typeof(TCommand).Name);
                        exceptions.Add(ex);
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                }, cancellationToken));
            }

            // Wait for all tasks to complete
            await Task.WhenAll(tasks);

            if (exceptions.Any())
            {
                _logger.LogWarning("Completed batch processing with {ExceptionCount} errors out of {CommandCount} commands",
                    exceptions.Count, commandArray.Length);

                if (exceptions.Count == commandArray.Length)
                {
                    // All commands failed
                    throw new AggregateException("All commands in batch failed", exceptions);
                }
            }

            return results;
        }

        /// <summary>
        /// Process commands in order, stopping on first failure
        /// </summary>
        public async Task<IEnumerable<TResponse>> ProcessOrderedBatchAsync<TCommand, TResponse>(
            IEnumerable<TCommand> commands,
            bool stopOnFailure = true,
            CancellationToken cancellationToken = default)
            where TCommand : IRequest<TResponse>
        {
            var commandArray = commands.ToArray();
            var results = new List<TResponse>();

            foreach (var command in commandArray)
            {
                try
                {
                    var result = await _mediator.Send(command, cancellationToken);
                    results.Add(result);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing ordered command batch item of type {CommandType}", typeof(TCommand).Name);

                    if (stopOnFailure)
                    {
                        throw;
                    }
                }
            }

            return results;
        }
    }
}