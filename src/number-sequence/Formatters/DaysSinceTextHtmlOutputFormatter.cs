using Microsoft.AspNetCore.Mvc.Formatters;
using System.Text;
using System.Web;
using TcpWtf.NumberSequence.Contracts;

namespace number_sequence.Formatters
{
    public sealed class DaysSinceTextHtmlOutputFormatter : TextOutputFormatter
    {
        public DaysSinceTextHtmlOutputFormatter()
        {
            this.SupportedMediaTypes.Add("text/html");
            this.SupportedEncodings.Add(Encoding.UTF8);
            this.SupportedEncodings.Add(Encoding.Unicode);
        }

        protected override bool CanWriteType(Type type) => type == typeof(DaysSince);

        public override async Task WriteResponseBodyAsync(OutputFormatterWriteContext context, Encoding selectedEncoding)
        {
            var responseObject = context.Object as DaysSince;

            StringBuilder sb = new();
            _ = sb.AppendLine("<!DOCTYPE HTML>");
            _ = sb.AppendLine("<html>");
            _ = sb.AppendLine("");
            _ = sb.AppendLine("<head>");
            _ = sb.AppendLine("<meta name=viewport content=\"width=device-width, initial-scale=1\">");
            _ = sb.AppendLine("</head>");
            _ = sb.AppendLine("");
            _ = sb.AppendLine("<body>");
            _ = sb.AppendLine("");


            _ = sb.AppendLine("<svg width=\"400\" height=\"400\" xmlns=\"http://www.w3.org/2000/svg\">");
            
            // Background
            _ = sb.AppendLine("  <rect width=\"100%\" height=\"100%\" fill=\"yellow\" />");

            // Outer edge stripes
            _ = sb.AppendLine("  <defs>");
            _ = sb.AppendLine("    <pattern id=\"stripes\" patternUnits=\"userSpaceOnUse\" width=\"50\" height=\"20\" patternTransform=\"rotate(45)\">");
            _ = sb.AppendLine("      <rect width=\"25\" height=\"20\" fill=\"black\" />");
            _ = sb.AppendLine("    </pattern>");
            _ = sb.AppendLine("    <mask id=\"innerMask\">");
            _ = sb.AppendLine("      <rect x=\"0\" y=\"0\" width=\"40\" height=\"400\" fill=\"white\" />");
            _ = sb.AppendLine("      <rect x=\"0\" y=\"0\" width=\"400\" height=\"40\" fill=\"white\" />");
            _ = sb.AppendLine("      <rect x=\"360\" y=\"0\" width=\"40\" height=\"400\" fill=\"white\" />");
            _ = sb.AppendLine("      <rect x=\"0\" y=\"360\" width=\"400\" height=\"40\" fill=\"white\" />");
            _ = sb.AppendLine("    </mask>");
            _ = sb.AppendLine("  </defs>");
            _ = sb.AppendLine("  <rect width=\"100%\" height=\"100%\" fill=\"url(#stripes)\" mask=\"url(#innerMask)\" />");

            // White rectangle
            _ = sb.AppendLine("  <rect x=\"50\" y=\"50\" width=\"300\" height=\"100\" fill=\"white\" stroke=\"black\" stroke-width=\"3\" />");
            
            // Days counter
            _ = sb.Append("  <text x=\"200\" y=\"130\" font-family=\"Arial\" font-size=\"80\" font-weight=\"bold\" fill=\"blue\" text-anchor=\"middle\">");
            _ = sb.Append((int)(DateTime.UtcNow - responseObject.LastOccurrence.ToDateTime(new TimeOnly(), DateTimeKind.Utc)).TotalDays);
            _ = sb.AppendLine("</text>");

            // Days since
            _ = sb.AppendLine("  <text x=\"200\" y=\"200\" font-family=\"Arial\" font-size=\"40\" font-weight=\"bold\" fill=\"black\" text-anchor=\"middle\">DAYS SINCE</text>");

            // Insert the 4 different value rows as appropriate.
            if (responseObject.ValueLine4 != null)
            {
                _ = sb.Append("<text x=\"200\" y=\"250\" font-family=\"Arial\" font-size=\"30\" fill=\"black\" text-anchor=\"middle\">");
                _ = sb.Append(HttpUtility.HtmlEncode(responseObject.ValueLine1));
                _ = sb.AppendLine("</text>");

                _ = sb.Append("<text x=\"200\" y=\"280\" font-family=\"Arial\" font-size=\"30\" fill=\"black\" text-anchor=\"middle\">");
                _ = sb.Append(HttpUtility.HtmlEncode(responseObject.ValueLine2));
                _ = sb.AppendLine("</text>");

                _ = sb.Append("<text x=\"200\" y=\"310\" font-family=\"Arial\" font-size=\"30\" fill=\"black\" text-anchor=\"middle\">");
                _ = sb.Append(HttpUtility.HtmlEncode(responseObject.ValueLine3));
                _ = sb.AppendLine("</text>");

                _ = sb.Append("<text x=\"200\" y=\"340\" font-family=\"Arial\" font-size=\"30\" fill=\"black\" text-anchor=\"middle\">");
                _ = sb.Append(HttpUtility.HtmlEncode(responseObject.ValueLine4));
                _ = sb.AppendLine("</text>");
            }
            else if (responseObject.ValueLine3 != null)
            {
                _ = sb.Append("<text x=\"200\" y=\"260\" font-family=\"Arial\" font-size=\"30\" fill=\"black\" text-anchor=\"middle\">");
                _ = sb.Append(HttpUtility.HtmlEncode(responseObject.ValueLine1));
                _ = sb.AppendLine("</text>");

                _ = sb.Append("<text x=\"200\" y=\"290\" font-family=\"Arial\" font-size=\"30\" fill=\"black\" text-anchor=\"middle\">");
                _ = sb.Append(HttpUtility.HtmlEncode(responseObject.ValueLine2));
                _ = sb.AppendLine("</text>");

                _ = sb.Append("<text x=\"200\" y=\"320\" font-family=\"Arial\" font-size=\"30\" fill=\"black\" text-anchor=\"middle\">");
                _ = sb.Append(HttpUtility.HtmlEncode(responseObject.ValueLine3));
                _ = sb.AppendLine("</text>");
            }
            else if (responseObject.ValueLine2 != null)
            {
                _ = sb.Append("<text x=\"200\" y=\"270\" font-family=\"Arial\" font-size=\"30\" fill=\"black\" text-anchor=\"middle\">");
                _ = sb.Append(HttpUtility.HtmlEncode(responseObject.ValueLine1));
                _ = sb.AppendLine("</text>");

                _ = sb.Append("<text x=\"200\" y=\"300\" font-family=\"Arial\" font-size=\"30\" fill=\"black\" text-anchor=\"middle\">");
                _ = sb.Append(HttpUtility.HtmlEncode(responseObject.ValueLine2));
                _ = sb.AppendLine("</text>");
            }
            else
            {
                _ = sb.Append("<text x=\"200\" y=\"280\" font-family=\"Arial\" font-size=\"30\" fill=\"black\" text-anchor=\"middle\">");
                _ = sb.Append(HttpUtility.HtmlEncode(responseObject.ValueLine1));
                _ = sb.AppendLine("</text>");
            }

            _ = sb.AppendLine("");
            _ = sb.AppendLine("</body>");
            _ = sb.AppendLine("");
            _ = sb.AppendLine("</html>");

            await context.HttpContext.Response.WriteAsync(sb.ToString(), selectedEncoding, context.HttpContext.RequestAborted);
        }
    }
}
