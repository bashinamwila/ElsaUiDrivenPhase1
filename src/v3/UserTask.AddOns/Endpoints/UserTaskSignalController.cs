using Elsa.Workflows.Management;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UserTask.AddOns.Bookmarks;
using UserTask.AddOns.Endpoints.Models;
using UserTask.AddOns.Extensions;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using Elsa.Workflows.Management.Filters;
using Elsa.Workflows.Runtime;
using Elsa.Workflows.Runtime.Filters;

namespace UserTask.AddOns.Endpoints
{
    [ApiController]
    [Route("api/usertask-signals")]
    public class UserTaskSignalController : ControllerBase
    {
        private readonly IUserTaskSignalInvoker _invoker;
        private readonly ServerContext _serverContext;
        private readonly IWorkflowInstanceStore _workflowInstanceStore;
        private readonly IBookmarkStore _bookmarkStore;

        public UserTaskSignalController(
            IUserTaskSignalInvoker invoker,
            ServerContext serverContext,
            IWorkflowInstanceStore workflowInstanceStore,
            IBookmarkStore bookmarkStore)
        {
            _invoker = invoker;
            _serverContext = serverContext;
            _workflowInstanceStore = workflowInstanceStore;
            _bookmarkStore = bookmarkStore;
        }

        public class ExecuteSignalRequest
        {
            public object? Input { get; set; }
            public string? WorkflowInstanceId { get; set; }
            public string? CorrelationId { get; set; }
        }

        [HttpPost("{signalName}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> HandleExecute(string signalName, ExecuteSignalRequest request,
            CancellationToken cancellationToken = default)
        {
            var results = await _invoker.ExecuteWorkflowsAsync(signalName, request.Input,
                request.WorkflowInstanceId, request.CorrelationId, cancellationToken);
            return Ok(results.ToList());
        }

        [HttpPost("{signalName}/dispatch")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> HandleDispatch(string signalName, ExecuteSignalRequest request,
            CancellationToken cancellationToken = default)
        {
            var results = await _invoker.DispatchWorkflowsAsync(signalName, request.Input,
                request.WorkflowInstanceId, request.CorrelationId, cancellationToken);
            return Ok(results.ToList());
        }

        [HttpGet("{signalName}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<UsertaskViewModel>))]
        public async Task<IActionResult> GetWorkflowsWaitingForSignal(string signalName, CancellationToken cancellationToken = default)
        {
            string normalizedSignal = signalName.ToLowerInvariant();

            // Find bookmarks for the UserTaskSignal with the specified signal name
            var bookmarkFilter = new BookmarkFilter
            {
                ActivityTypeName = nameof(UserTaskSignal),
                // Use the hash of the signal name to filter bookmarks
                Hash = normalizedSignal
            };

            // Get all bookmarks for UserTaskSignal activities with the specified signal
            var bookmarks = await _bookmarkStore.FindManyAsync(bookmarkFilter, cancellationToken);

            if (!bookmarks.Any())
                return Ok(new List<UsertaskViewModel>());

            // Get the workflow instance IDs from the bookmarks
            var workflowInstanceIds = bookmarks.Select(b => b.WorkflowInstanceId).Distinct().ToList();

            // Create a filter to get the workflow instances by their IDs
            var instanceFilter = new WorkflowInstanceFilter
            {
                Ids = workflowInstanceIds
            };

            // Get the workflow instances
            var workflowInstances = await _workflowInstanceStore.FindManyAsync(instanceFilter, cancellationToken);

            // Convert to view models
            var viewmodelResult = workflowInstances.ConvertToViewModels(_serverContext);
            return Ok(viewmodelResult.ToList());
        }
    }
}