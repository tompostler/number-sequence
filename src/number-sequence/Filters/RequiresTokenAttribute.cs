using Microsoft.AspNetCore.Mvc.Filters;

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
            ILogger<RequiresTokenFilter> logger = serviceProvider.GetRequiredService<ILogger<RequiresTokenFilter>>();
            return new RequiresTokenFilter(this.requiredRole, logger);
        }
    }
}
