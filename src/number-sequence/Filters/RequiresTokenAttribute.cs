using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Memory;

namespace number_sequence.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
    public sealed class RequiresTokenAttribute : Attribute, IFilterFactory
    {
        private readonly string requiredRole;

        public RequiresTokenAttribute(string requiredRole = default)
        {
            this.requiredRole = requiredRole;
        }

        public bool IsReusable => true;

        public IFilterMetadata CreateInstance(IServiceProvider serviceProvider)
        {
            IMemoryCache memoryCache = serviceProvider.GetRequiredService<IMemoryCache>();
            ILogger<RequiresTokenFilter> logger = serviceProvider.GetRequiredService<ILogger<RequiresTokenFilter>>();

            return new RequiresTokenFilter(this.requiredRole, memoryCache, logger);
        }
    }
}
