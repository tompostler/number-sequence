using Microsoft.AspNetCore.Mvc.Filters;

namespace number_sequence.Filters
{
    public sealed class Throttle : ActionFilterAttribute
    {
        public Throttle()
        {
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
        }
    }
}
