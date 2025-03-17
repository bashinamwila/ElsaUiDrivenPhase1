using System;
using Elsa.Extensions;
using Elsa.Workflows;
using Elsa.Workflows.Attributes;
using Elsa.Workflows.Models;
using Elsa.Workflows.UIHints;

namespace UserTask.AddOns
{
    /// <summary>
    /// Suspends workflow execution until the specified signal is received.
    /// </summary>
   public class UserTaskSignal : Activity
    {
        /// <summary>
        /// The name of the signal to wait for.
        /// </summary>
        [Input(Description = "The name of the signal to wait for.")]
        public string Signal { get; set; } = default!;

        /// <summary>
        /// The task name.
        /// </summary>
        [Input(Description = "The task name", Category = "Task")]
        public string TaskName { get; set; } = default!;

        /// <summary>
        /// The title of the task that needs to be executed.
        /// </summary>
        [Input(Description = "The title of the task that needs to be executed.", Category = "Task")]
        public string TaskTitle { get; set; } = default!;

        /// <summary>
        /// The description of the task that needs to be executed.
        /// </summary>
        [Input(
            Description = "The description of the task that needs to be executed.",
            UIHint = InputUIHints.MultiLine,
            Category = "Task"
        )]
        public string TaskDescription { get; set; } = default!;

        /// <summary>
        /// The definition of the data expected to be returned.
        /// </summary>
        [Input(
            Description = "The definition of the data expected to be returned",
            UIHint = InputUIHints.MultiLine,
            Category = "Task"
        )]
        public string UIDefinition { get; set; } = default!;

        /// <summary>
        /// Context data for the usertask.
        /// </summary>
        [Input(
            Description = "Context data for the usertask",
            UIHint = InputUIHints.MultiLine,
            Category = "Task"
        )]
        public object TaskData { get; set; } = default!;

        /// <summary>
        /// The output that was received with the signal.
        /// </summary>
        [Output(Description = "The output that was received with the signal.")]
        public object? Output { get; set; }

        /// <inheritdoc />
        protected override async ValueTask ExecuteAsync(ActivityExecutionContext context)
        {
            // If this activity is the trigger of the workflow, process the input immediately.
            // Otherwise, create a bookmark and wait for the signal.
            if (context.IsTriggerOfWorkflow())
            {
                var input = context.GetWorkflowInput<object>();
                Output = input;
                await context.CompleteActivityAsync();
                return;
            }

            // Create a bookmark for this signal.
            var signalName = Signal.ToLowerInvariant();

            // In Elsa 3, CreateBookmark takes a stimulus object
            var options = new CreateBookmarkArgs
            {
                Stimulus = signalName,
                Callback = OnSignalReceived
            };

            context.CreateBookmark(options);
        }

        private async ValueTask OnSignalReceived(ActivityExecutionContext context)
        {
            // Get the input from the context
            var input = context.GetWorkflowInput<object>();
            Output = input;

            // Complete the activity.
            await context.CompleteActivityAsync();
        }
    }
}