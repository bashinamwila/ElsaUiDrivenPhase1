using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Elsa.Workflows.Runtime;
using Elsa.Workflows.Runtime.Results;

namespace UserTask.AddOns.Bookmarks
{
    public interface IUserTaskSignalInvoker
    {
        Task<IEnumerable<WorkflowExecutionResult>> ExecuteWorkflowsAsync(
            string signal,
            object? input = null,
            string? workflowInstanceId = null,
            string? correlationId = null,
            CancellationToken cancellationToken = default);

        Task<IEnumerable<WorkflowExecutionResult>> DispatchWorkflowsAsync(
            string signal,
            object? input = null,
            string? workflowInstanceId = null,
            string? correlationId = null,
            CancellationToken cancellationToken = default);
    }
}