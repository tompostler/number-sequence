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

            int daysSinceNewest = (int)(DateTime.UtcNow - responseObject.Max(x => x.EventDate).ToDateTime(new(), DateTimeKind.Utc)).TotalDays;
            string title = responseObject.Any()
                ? $"{daysSinceNewest} Day{(daysSinceNewest != 1 ? "s" : string.Empty)} Since {HttpUtility.HtmlEncode(responseObject.First().DaysSince.FriendlyName ?? responseObject.First().DaysSince.Id)}: {responseObject.Count} events"
                : "No Days Since events";

            StringBuilder sb = new();
            _ = sb.AppendLine($$"""
                <!DOCTYPE HTML>
                <html>

                <head>
                  <title>{{title}}</title>
                  <meta http-equiv="refresh" content="33333">
                  <style>
                    table,
                    th,
                    td {
                      padding: 0.3rem;
                      text-align: left;
                      border: 1px solid lightgray;
                      border-collapse: collapse;
                    }
                  </style>
                </head>

                <body>

                  <table>
                    <thead>
                      <tr>
                        <th>Id</th>
                        <th>Event Date</th>
                        <th>Days Ago</th>
                        <th>Description</th>
                      </tr>
                    </thead>
                    <tbody>
                """);

            foreach (DaysSinceEvent daysSinceEvent in responseObject.OrderByDescending(x => x.EventDate).ThenBy(x => x.Id))
            {
                _ = sb.AppendLine("      <tr>");
                _ = sb.AppendLine($"        <td>{daysSinceEvent.Id}</td>");
                _ = sb.AppendLine($"        <td>{daysSinceEvent.EventDate:yyyy-MM-dd}</td>");
                _ = sb.AppendLine($"        <td>{(int)(DateTime.UtcNow - daysSinceEvent.EventDate.ToDateTime(new(), DateTimeKind.Utc)).TotalDays}</td>");
                _ = sb.AppendLine($"        <td>{HttpUtility.HtmlEncode(daysSinceEvent.Description)}</td>");
                _ = sb.AppendLine("      </tr>");
            }

            _ = sb.AppendLine("""
                    </tbody>
                  </table>

                </body>

                </html>
                """);

            await context.HttpContext.Response.WriteAsync(sb.ToString(), selectedEncoding, context.HttpContext.RequestAborted);
        }
    }
}
