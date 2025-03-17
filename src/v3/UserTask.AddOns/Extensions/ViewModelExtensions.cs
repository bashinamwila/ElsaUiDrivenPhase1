using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Elsa.Workflows.Management.Entities;
using Elsa.Workflows.Runtime.Entities;
using Elsa.Workflows.Runtime;
using UserTask.AddOns.Endpoints.Models;

namespace UserTask.AddOns.Extensions
{
    public static class ViewModelExtensions
    {
        internal static List<UsertaskViewModel> ConvertToViewModels(this IEnumerable<WorkflowInstance> source, ServerContext serverContext)
        {
            var result = new List<UsertaskViewModel>();
            foreach (var instance in source)
            {
                var viewModels = instance.ConvertToViewModels(serverContext);
                result.AddRange(viewModels);
            }
            return result;
        }

        internal static List<UsertaskViewModel> ConvertToViewModels(this WorkflowInstance instance, ServerContext serverContext)
        {
            var result = new List<UsertaskViewModel>();

            // Access workflow state to get activity data
            if (instance.WorkflowState == null)
                return result;

            // Parse the workflow state to access activity data
            var workflowStateJson = JsonSerializer.Serialize(instance.WorkflowState);
            var workflowState = JsonSerializer.Deserialize<JsonElement>(workflowStateJson);

            // Check if we have activity data
            if (!workflowState.TryGetProperty("ActivityData", out var activityDataElement))
                return result;

            // We need to get bookmarks from the bookmark store or from the workflow state
            // For now, let's extract blocking activities from the workflow state
            if (workflowState.TryGetProperty("BlockingActivities", out var blockingActivitiesElement))
            {
                foreach (var blockingActivity in blockingActivitiesElement.EnumerateArray())
                {
                    if (!blockingActivity.TryGetProperty("ActivityType", out var activityTypeElement) ||
                        activityTypeElement.GetString() != nameof(UserTaskSignal))
                        continue;

                    if (!blockingActivity.TryGetProperty("ActivityId", out var activityIdElement))
                        continue;

                    var activityId = activityIdElement.GetString();
                    if (string.IsNullOrEmpty(activityId))
                        continue;

                    // Try to get activity data for this activity
                    if (!activityDataElement.TryGetProperty(activityId, out var activityState))
                        continue;

                    var viewModel = new UsertaskViewModel
                    {
                        WorkflowInstanceId = instance.Id,
                        Signal = GetPropertyValue(activityState, "Signal"),
                        TaskName = GetPropertyValue(activityState, "TaskName"),
                        TaskTitle = GetPropertyValue(activityState, "TaskTitle"),
                        TaskDescription = GetPropertyValue(activityState, "TaskDescription"),
                        TaskData = GetPropertyValue(activityState, "TaskData"),
                        UIDefinition = GetPropertyValue(activityState, "UIDefinition"),
                        EngineId = serverContext.EngineId
                    };

                    result.Add(viewModel);
                }
            }

            return result;
        }

        private static string GetPropertyValue(JsonElement element, string propertyName)
        {
            if (element.TryGetProperty(propertyName, out var property))
            {
                if (property.ValueKind == JsonValueKind.String)
                    return property.GetString() ?? string.Empty;

                return property.ToString();
            }

            return string.Empty;
        }
    }
}