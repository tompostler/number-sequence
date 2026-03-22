using TcpWtf.NumberSequence.Client;
using Unlimitedinf.Utilities;
using Unlimitedinf.Utilities.Extensions;

namespace TcpWtf.NumberSequence.Tool.Commands
{
    internal static partial class LedgerCommand
    {
        private static async Task HandlePaymentAddAsync(long invoiceId, bool raw, Stamp stamp, Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), TokenProvider.GetAsync, stamp);

            Contracts.Ledger.Invoice invoice = await client.Ledger.GetInvoiceAsync(invoiceId);
            PrintSingleInvoice(invoice, raw);

            Contracts.Ledger.InvoicePayment payment = new()
            {
                AccountName = TokenProvider.GetAccount(),
                Amount = Input.GetDecimal(nameof(payment.Amount)),
                PaymentDate = Input.GetDateOnly(nameof(payment.PaymentDate)),
                Details = Input.GetString(nameof(payment.Details)),
            };

            invoice = await client.Ledger.CreateInvoicePaymentAsync(invoiceId, payment);
            PrintSingleInvoice(invoice, raw);
        }

        private static async Task HandlePaymentEditAsync(long invoiceId, long paymentId, bool raw, Stamp stamp, Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), TokenProvider.GetAsync, stamp);

            Contracts.Ledger.Invoice invoice = await client.Ledger.GetInvoiceAsync(invoiceId);
            Contracts.Ledger.InvoicePayment payment = invoice.Payments.Single(x => x.Id == paymentId);

            payment.PaymentDate = Input.GetDateOnly(nameof(payment.PaymentDate), payment.PaymentDate);
            payment.Details = Input.GetString(nameof(payment.Details), payment.Details);

            invoice = await client.Ledger.UpdateInvoicePaymentAsync(invoiceId, paymentId, payment);
            PrintSingleInvoice(invoice, raw);
        }

        private static async Task HandlePaymentRemoveAsync(long invoiceId, long paymentId, bool raw, Stamp stamp, Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), TokenProvider.GetAsync, stamp);

            Contracts.Ledger.Invoice invoice = await client.Ledger.DeleteInvoicePaymentAsync(invoiceId, paymentId);
            PrintSingleInvoice(invoice, raw);
        }

        private static async Task HandlePaymentAddBulkAsync(bool raw, Stamp stamp, Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), TokenProvider.GetAsync, stamp);

            DateOnly paymentDate = Input.GetDateOnly(nameof(Contracts.Ledger.InvoicePayment.PaymentDate));
            string details = Input.GetString(nameof(Contracts.Ledger.InvoicePayment.Details));
            decimal amount = Input.GetDecimal(nameof(Contracts.Ledger.InvoicePayment.Amount));

            List<Contracts.Ledger.Invoice> invoices = [];
            while (true)
            {
                long invoiceId = Input.GetLong("InvoiceId (0 to finish)", canDefault: true, defaultVal: 0);
                if (invoiceId == 0)
                {
                    break;
                }

                Contracts.Ledger.Invoice invoice = await client.Ledger.GetInvoiceAsync(invoiceId);
                if (invoice.PaidDate.HasValue)
                {
                    Console.WriteLine($"  Skipping invoice {invoice.Id}: already paid on {invoice.PaidDate:o}.");
                    continue;
                }
                invoices.Add(invoice);
                Console.WriteLine($"  Added invoice {invoice.Id} \"{invoice.Title}\": balance ${invoice.Balance:N2}");
            }

            if (invoices.Count == 0)
            {
                Console.WriteLine("No invoices selected.");
                return;
            }

            decimal remaining = amount;
            for (int i = 0; i < invoices.Count; i++)
            {
                Contracts.Ledger.Invoice invoice = invoices[i];
                if (remaining <= 0)
                {
                    break;
                }

                string suffix = $" (split {i + 1}/{invoices.Count}, from ${amount:N2})";
                string fullDetails = details.Length + suffix.Length > 128
                    ? details[..(128 - suffix.Length)] + suffix
                    : details + suffix;
                Contracts.Ledger.InvoicePayment payment = new()
                {
                    AccountName = TokenProvider.GetAccount(),
                    Amount = Math.Min(invoice.Balance, remaining),
                    PaymentDate = paymentDate,
                    Details = fullDetails,
                };
                remaining -= payment.Amount;
                Contracts.Ledger.Invoice updatedInvoice = await client.Ledger.CreateInvoicePaymentAsync(invoice.Id, payment);
                PrintSingleInvoice(updatedInvoice, raw);
            }

            if (remaining > 0)
            {
                Console.WriteLine($"Warning: ${remaining:N2} of the entered amount was not applied (insufficient invoice balances). Create an additional payment, or supply a refund.");
            }
        }
    }
}
