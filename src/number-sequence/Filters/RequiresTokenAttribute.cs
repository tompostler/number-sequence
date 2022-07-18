using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;

namespace number_sequence.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
    public sealed class RequiresTokenAttribute : Attribute, IFilterFactory
    {
        public bool IsReusable => true;

        public IFilterMetadata CreateInstance(IServiceProvider serviceProvider)
        {
            IMemoryCache memoryCache = serviceProvider.GetRequiredService<IMemoryCache>();
            ILogger<RequiresTokenFilter> logger = serviceProvider.GetRequiredService<ILogger<RequiresTokenFilter>>();

            return new RequiresTokenFilter(memoryCache, logger);
        }
    }
}
