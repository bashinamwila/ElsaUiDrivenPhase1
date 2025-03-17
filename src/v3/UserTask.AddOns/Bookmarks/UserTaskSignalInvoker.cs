using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Elsa.Workflows.Runtime;
using Elsa.Workflows.Runtime.Contracts;
using Elsa.Workflows.Management.Contracts;
using Elsa.Workflows.Models;
using Elsa.Workflows.Runtime.Requests;
using Elsa.Workflows.Runtime.Results;
using Elsa.Workflows;
using Elsa.Workflows.Runtime.Stimuli;
using Elsa.Workflows.Runtime.Messages;

namespace UserTask.AddOns.Bookmarks
{
    public class UserTaskSignalInvoker : IUserTaskSignalInvoker
    {
        private readonly IStimulusSender _stimulusSender;
        private readonly IWorkflowDispatcher _workflowDispatcher;

        public UserTaskSignalInvoker(IStimulusSender stimulusSender, IWorkflowDispatcher workflowDispatcher)
        {
            _stimulusSender = stimulusSender;
            _workflowDispatcher = workflowDispatcher;
        }

        public async Task<IEnumerable<WorkflowExecutionResult>> ExecuteWorkflowsAsync(
            string signal,
            object? input = null,
            string? workflowInstanceId = null,
            string? correlationId = null,
            CancellationToken cancellationToken = default)
        {
            string normalizedSignal = signal.ToLowerInvariant();

            // Create a bookmark payload
            var bookmarkPayload = new Dictionary<string, object>
            {
                ["Signal"] = normalizedSignal
            };

            // Create input dictionary if input is not already a dictionary
            IDictionary<string, object> inputDict;
            if (input is IDictionary<string, object> dict)
            {
                inputDict = dict;
            }
            else
            {
                inputDict = new Dictionary<string, object>
                {
                    ["Input"] = input ?? new object()
                };
            }

            // Create metadata
            var metadata = new StimulusMetadata
            {
                Input = inputDict,
                CorrelationId = correlationId,
                WorkflowInstanceId = workflowInstanceId
            };

            // Trigger workflows synchronously
            var result = await _stimulusSender.SendAsync(
                nameof(UserTaskSignal),
                bookmarkPayload,
                metadata,
                cancellationToken);

            // Convert RunWorkflowInstanceResponse to WorkflowExecutionResult
            return result.WorkflowInstanceResponses.Select(response => new WorkflowExecutionResult(
                response.WorkflowInstanceId,
                response.Status,
                response.SubStatus,
                response.Bookmarks.ToList(),
                response.Incidents.ToList(),
                null
            )).ToList();
        }

        public async Task<IEnumerable<WorkflowExecutionResult>> DispatchWorkflowsAsync(
            string signal,
            object? input = null,
            string? workflowInstanceId = null,
            string? correlationId = null,
            CancellationToken cancellationToken = default)
        {
            string normalizedSignal = signal.ToLowerInvariant();

            // Create a bookmark payload
            var bookmarkPayload = new Dictionary<string, object>
            {
                ["Signal"] = normalizedSignal
            };

            // Create input dictionary if input is not already a dictionary
            IDictionary<string, object> inputDict;
            if (input is IDictionary<string, object> dict)
            {
                inputDict = dict;
            }
            else
            {
                inputDict = new Dictionary<string, object>
                {
                    ["Input"] = input ?? new object()
                };
            }

            // Create dispatch request
            var dispatchRequest = new DispatchTriggerWorkflowsRequest(
                nameof(UserTaskSignal),
                bookmarkPayload)
            {
                Input = inputDict,
                CorrelationId = correlationId,
                WorkflowInstanceId = workflowInstanceId
            };

            // For dispatch (async), we use the dispatcher
            var dispatchResult = await _workflowDispatcher.DispatchAsync(
                dispatchRequest,
                cancellationToken);

            // Since dispatch is asynchronous, we don't have execution results yet
            // We'll create placeholder results
            var results = new List<WorkflowExecutionResult>();

            // In Elsa 3, the dispatch result doesn't contain workflow instance IDs
            // We'll just return an empty list since we can't track the dispatched workflows
            return results;
        }
    }
}