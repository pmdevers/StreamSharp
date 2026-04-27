using Microsoft.Extensions.DependencyInjection;
using StreamSharp.Core.Storage;

namespace StreamSharp.Core;

public static class AddCoreExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddCore()
        {
            services.AddScoped<LibraryRepository>();
            services.AddScoped<LibraryItemRepository>();
            return services;
        }
    }
}
