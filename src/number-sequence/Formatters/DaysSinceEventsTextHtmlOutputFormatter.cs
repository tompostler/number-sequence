using Microsoft.AspNetCore.Mvc.Formatters;
using System.Text;
using System.Web;
using TcpWtf.NumberSequence.Contracts;

namespace number_sequence.Formatters
{
    public sealed class DaysSinceEventsTextHtmlOutputFormatter : TextOutputFormatter
    {
        public DaysSinceEventsTextHtmlOutputFormatter()
        {
            this.SupportedMediaTypes.Add("text/html");
            this.SupportedEncodings.Add(Encoding.UTF8);
            this.SupportedEncodings.Add(Encoding.Unicode);
        }

        protected override bool CanWriteType(Type type) => type == typeof(IList<DaysSinceEvent>) || type == typeof(List<DaysSinceEvent>);

        public override async Task WriteResponseBodyAsync(OutputFormatterWriteContext context, Encoding selectedEncoding)
        {
            var responseObject = context.Object as IList<DaysSinceEvent>;

            StringBuilder sb = new();
            _ = sb.AppendLine("<!DOCTYPE HTML>");
            _ = sb.AppendLine("<html>");
            _ = sb.AppendLine("<body>");
            _ = sb.AppendLine("");

            _ = sb.AppendLine("<table>");
            _ = sb.AppendLine("  <tr>");
            _ = sb.AppendLine("    <th>Id</th>");
            _ = sb.AppendLine("    <th>Event Date</th>");
            _ = sb.AppendLine("    <th>Days Ago</th>");
            _ = sb.AppendLine("    <th>Description</th>");
            _ = sb.AppendLine("  </tr>");
            foreach (DaysSinceEvent daysSinceEvent in responseObject.OrderByDescending(x => x.EventDate).ThenBy(x => x.Id))
            {
                _ = sb.AppendLine("  <tr>");
                _ = sb.AppendLine($"    <td>{daysSinceEvent.Id}</td>");
                _ = sb.AppendLine($"    <td>{daysSinceEvent.EventDate:yyyy-MM-dd}</td>");
                _ = sb.AppendLine($"    <td>{DateTime.UtcNow.Subtract(daysSinceEvent.EventDate.ToDateTime(new(), DateTimeKind.Utc)).TotalDays:0}</td>");
                _ = sb.AppendLine($"    <td>{HttpUtility.HtmlEncode(daysSinceEvent.Description)}</td>");
                _ = sb.AppendLine("  </tr>");
            }
            _ = sb.AppendLine("</table>");

            _ = sb.AppendLine("");
            _ = sb.AppendLine("</body>");
            _ = sb.AppendLine("</html>");

            await context.HttpContext.Response.WriteAsync(sb.ToString(), selectedEncoding, context.HttpContext.RequestAborted);
        }
    }
}
