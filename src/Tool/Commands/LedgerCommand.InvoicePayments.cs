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
    }
}
