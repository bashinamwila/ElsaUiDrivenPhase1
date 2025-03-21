using Elsa.Persistence;
using Elsa.Persistence.Specifications.WorkflowInstances;
using Elsa.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using UserTask.AddOns.Bookmarks;
using Elsa.Server.Api.ActionFilters;
using Swashbuckle.AspNetCore.Annotations;
using Elsa.Server.Api.Endpoints.Signals;
using UserTask.AddOns.Endpoints.Models;
using UserTask.AddOns.Extensions;
using Microsoft.Extensions.Logging;

namespace UserTask.AddOns.Endpoints
{
    [ApiController]
    [Route("v{apiVersion:apiVersion}/usertask-signals")]
    [Produces("application/json")]
    public class UserTaskSignalController : Controller
    {
        private readonly IUserTaskSignalInvoker invoker;
        private readonly IBookmarkFinder bookmarkFinder;
        private readonly ServerContext serverContext;
        private readonly IWorkflowInstanceStore workflowInstanceStore;
        private readonly ILogger<UserTaskSignalController> logger;


        public UserTaskSignalController(IUserTaskSignalInvoker invoker, IBookmarkFinder bookmarkFinder, IWorkflowInstanceStore workflowInstanceStore, ServerContext serverContext,
            ILogger<UserTaskSignalController> logger)
        {
            this.workflowInstanceStore = workflowInstanceStore;
            this.serverContext = serverContext;
            this.invoker = invoker;
            this.bookmarkFinder = bookmarkFinder;
            this.logger = logger;

        }

        [HttpPost("{signalName}/execute")]
        [ElsaJsonFormatter]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ExecuteSignalResponse))]
        [SwaggerOperation(
            Summary = "Signals all workflows waiting on the specified signal name synchronously.",
            Description = "Signals all workflows waiting on the specified signal name synchronously.",
            OperationId = "UsertaskSignals.Execute",
            Tags = new[] { "UsertaskSignals" })
        ]
        public async Task<IActionResult> Handle(string signalName, ExecuteSignalRequest request,
            CancellationToken cancellationToken = default)
        {
            logger.LogInformation($"Executing {signalName}.... ");
            var collectedWorkflows = await invoker.ExecuteWorkflowsAsync(signalName, request.Input,
                request.WorkflowInstanceId, request.CorrelationId, cancellationToken);
            return Ok(collectedWorkflows.ToList());
        }

        [HttpPost("{signalName}/dispatch")]
        [ElsaJsonFormatter]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ExecuteSignalResponse))]
        [SwaggerOperation(
           Summary = "Signals all workflows waiting on the specified signal name asynchronously.",
           Description = "Signals all workflows waiting on the specified signal name asynchronously.",
           OperationId = "UsertaskSignals.Dispatch",
           Tags = new[] { "UsertaskSignals" })
       ]
        public async Task<IActionResult> HandleDispatch(string signalName, ExecuteSignalRequest request,
           CancellationToken cancellationToken = default)
        {
            var collectedWorkflows = await invoker.DispatchWorkflowsAsync(signalName, request.Input,
                request.WorkflowInstanceId, request.CorrelationId, cancellationToken);
            return Ok(collectedWorkflows.ToList());
        }

        [HttpGet("{signalName}")]
        // [ElsaJsonFormatter]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ExecuteSignalResponse))]
        [SwaggerOperation(
           Summary = "Gets all workflows waiting for a specific signal to be returned",
           Description = "return a list of workflow instances",
           OperationId = "Usertask" +
            "Signals.Query",
           Tags = new[] { "UsertaskSignals" })
       ]
        public async Task<IActionResult> Collect(string signalName,
           CancellationToken cancellationToken = default)
        {
            string normalizedSignal = signalName.ToLowerInvariant();

            var bookmarkFilter = new UserTaskSignalBookmark { Signal = normalizedSignal };
            var bookmarkResults = await bookmarkFinder.FindBookmarksAsync(nameof(UserTaskSignal), new[] { bookmarkFilter });
            var workflowInstanceIds = new WorkflowInstanceIdsSpecification(bookmarkResults.Select(x => x.WorkflowInstanceId).ToList());
            var workflowInstances = await workflowInstanceStore.FindManyAsync(workflowInstanceIds, null, null, cancellationToken);

            var viewmodelResult = workflowInstances.ConvertToViewModels(serverContext);
            normalizedSignal = null;
            return Ok(viewmodelResult.ToList());
        }
    }
}