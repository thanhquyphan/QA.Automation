using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;

namespace TQP.QA.Automation.UI.Core
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection RegisterPageDrivers(this IServiceCollection services, Type marker)
        {
            var assembly = marker.Assembly;

            var toRegister = assembly.GetTypes()
                .Where(x => !x.IsInterface && !x.IsAbstract && !x.IsEnum && x.IsClass && x.IsSubclassOf(typeof(PageBase)));

            foreach (var concreteType in toRegister)
            {
                services.AddTransient(concreteType);
            }

            return services;
        }
    }
}
