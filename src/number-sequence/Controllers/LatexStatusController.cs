using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using number_sequence.DataAccess;
using number_sequence.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TcpWtf.NumberSequence.Contracts;

namespace number_sequence.Controllers
{
    [ApiController, Route("[controller]"), RequiresToken(AccountRoles.LatexStatus)]
    public sealed class LatexStatusController : ControllerBase
    {
        private readonly IServiceProvider serviceProvider;

        public LatexStatusController(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        [HttpGet]
        public async Task<IActionResult> GetAsync()
        {
            using IServiceScope scope = this.serviceProvider.CreateScope();
            using NsContext nsContext = scope.ServiceProvider.GetRequiredService<NsContext>();

            DateTimeOffset monthAgo = DateTimeOffset.UtcNow.AddDays(-30);

            List<Models.LatexTemplateSpreadsheetRow> latexTemplateSpreadsheetRows = await nsContext.LatexTemplateSpreadsheetRows
                                                                                                    .Where(r => r.CreatedDate > monthAgo)
                                                                                                    .OrderBy(r => r.CreatedDate)
                                                                                                    .Take(10)
                                                                                                    .ToListAsync();
            List<Models.LatexDocument> latexDocuments = await nsContext.LatexDocuments
                                                                        .Where(r => r.CreatedDate > monthAgo)
                                                                        .OrderBy(r => r.CreatedDate)
                                                                        .Take(10)
                                                                        .ToListAsync();
            List<Models.EmailLatexDocument> emailLatexDocuments = await nsContext.EmailLatexDocuments
                                                                                    .Where(r => r.CreatedDate > monthAgo)
                                                                                    .OrderBy(r => r.CreatedDate)
                                                                                    .Take(10)
                                                                                    .ToListAsync();
            LatexStatus latexStatus = new()
            {
                LatexTemplateSpreadsheetRows = latexTemplateSpreadsheetRows.Select(
                    r => new LatexStatus.LatexTemplateSpreadsheetRow
                    {
                        RowId = r.RowId,
                        LatexDocumentId = r.LatexDocumentId,
                        CreatedDate = r.CreatedDate.ToString("u")
                    })
                    .ToList(),
                LatexDocuments = latexDocuments.Select(
                    d => new LatexStatus.LatexDocument
                    {
                        Id = d.Id,
                        CreatedDate = d.CreatedDate.ToString("u"),
                        ProcessedAt = d.ProcessedAt?.ToString("u"),
                        Successful = d.Successful?.ToString()
                    })
                    .ToList(),
                EmailLatexDocuments = emailLatexDocuments.Select(
                    e => new LatexStatus.EmailLatexDocument
                    {
                        Id = e.Id,
                        Subject = e.Subject,
                        AttachmentName = e.AttachmentName,
                        CreatedDate = e.CreatedDate.ToString("u"),
                        ProcessedAt = e.ProcessedAt?.ToString("u")
                    })
                    .ToList()
            };

            return this.Ok(latexStatus);
        }
    }
}
