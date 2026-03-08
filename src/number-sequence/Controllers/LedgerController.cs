using Microsoft.AspNetCore.Mvc;
using number_sequence.Filters;
using number_sequence.Utilities;
using TcpWtf.NumberSequence.Contracts;

namespace number_sequence.Controllers
{
    [ApiController, Route("[controller]"), RequiresToken(AccountRoles.Ledger)]
    public sealed partial class LedgerController : ControllerBase
    {
        private readonly IServiceProvider serviceProvider;
        private readonly Sentinals sentinals;
        private readonly ILogger<LedgerController> logger;

        public LedgerController(
            IServiceProvider serviceProvider,
            Sentinals sentinals,
            ILogger<LedgerController> logger)
        {
            this.serviceProvider = serviceProvider;
            this.sentinals = sentinals;
            this.logger = logger;
        }
    }
}
