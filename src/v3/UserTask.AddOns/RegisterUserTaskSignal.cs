using Elsa.Extensions;
using Elsa.Workflows.Activities;
using Microsoft.Extensions.DependencyInjection;
using UserTask.AddOns.Bookmarks;
using UserTask.AddOns.Endpoints.Models;

namespace UserTask.AddOns
{
    public static class RegisterUserTaskSignal
    {
        public static IServiceCollection AddUserTaskSignalActivities(this IServiceCollection services, string engineId)
        {
            services.AddScoped<IUserTaskSignalInvoker, UserTaskSignalInvoker>();
            services.AddSingleton(opt => new ServerContext(engineId));
            services.AddElsa(elsa =>
            {
                elsa.AddActivity<UserTaskSignal>();
            });
            // Register the activity and bookmark provider
           
            

            return services;
        }
    }
}
