using Microsoft.Extensions.DependencyInjection;

namespace StreamSharp.Core;

public static class AddCoreExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddCore()
        {
            return services;
        }
    }
}
