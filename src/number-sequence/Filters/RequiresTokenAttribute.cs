using Microsoft.AspNetCore.Mvc.Filters;

namespace number_sequence.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
    public sealed class RequiresTokenAttribute : Attribute, IFilterFactory
    {
        private readonly string[] requiredRoles;

        /// <summary>
        /// If multiple roles are given, the account needs only one of them (OR, not AND) to pass.
        /// </summary>
        public RequiresTokenAttribute(params string[] requiredRoles)
        {
            this.requiredRoles = requiredRoles;
        }

        public bool IsReusable => true;

        public IFilterMetadata CreateInstance(IServiceProvider serviceProvider)
        {
            ILogger<RequiresTokenFilter> logger = serviceProvider.GetRequiredService<ILogger<RequiresTokenFilter>>();
            return new RequiresTokenFilter(this.requiredRoles, logger);
        }
    }
}
