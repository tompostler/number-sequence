﻿using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Threading.Tasks;
using TcpWtf.NumberSequence.Client;
using TcpWtf.NumberSequence.Tool.Extensions;

namespace TcpWtf.NumberSequence.Tool.Commands
{
    internal static class InvoiceCommand
    {
        public static Command Create(Option<Verbosity> verbosityOption)
        {
            Command rootCommand = new("invoice", "Create and manage invoices.");


            Command businessCommand = new("business", "Manage invoice businesses (location that creates invoices).");

            Command businessCreateCommand = new("create", "Create a new invoice business.");
            businessCreateCommand.SetHandler(HandleBusinessCreateAsync, verbosityOption);

            Command businessGetCommand = new("get", "Get and existing invoice business.");
            Argument<long> businessIdArgument = new("id", "The id of the business.");
            businessGetCommand.AddArgument(businessIdArgument);
            businessGetCommand.SetHandler(HandleBusinessGetAsync, businessIdArgument, verbosityOption);

            Command businessListCommand = new("list", "Get and existing invoice business.");
            businessListCommand.SetHandler(HandleBusinessListAsync, verbosityOption);

            businessCommand.AddCommand(businessCreateCommand);
            businessCommand.AddCommand(businessGetCommand);
            businessCommand.AddCommand(businessListCommand);


            Command customerCommand = new("customer", "Manage invoice customers (location that receives invoices).");

            Command customerCreateCommand = new("create", "Create a new invoice customer.");
            customerCreateCommand.SetHandler(HandleCustomerCreateAsync, verbosityOption);

            Command customerGetCommand = new("get", "Get and existing invoice customer.");
            Argument<long> customerIdArgument = new("id", "The id of the customer.");
            customerGetCommand.AddArgument(customerIdArgument);
            customerGetCommand.SetHandler(HandleCustomerGetAsync, customerIdArgument, verbosityOption);

            Command customerListCommand = new("list", "Get and existing invoice customer.");
            customerListCommand.SetHandler(HandleCustomerListAsync, verbosityOption);

            customerCommand.AddCommand(customerCreateCommand);
            customerCommand.AddCommand(customerGetCommand);
            customerCommand.AddCommand(customerListCommand);


            Command lineDefaultCommand = new("line-default", "Manage invoice line defaults (line items that can be used as a reference for adding a line to an invoice).");

            Command lineDefaultCreateCommand = new("create", "Create a new invoice line default.");
            lineDefaultCreateCommand.SetHandler(HandleLineDefaultCreateAsync, verbosityOption);

            Command lineDefaultGetCommand = new("get", "Get and existing invoice line default.");
            Argument<long> lineDefaultIdArgument = new("id", "The id of the line default.");
            lineDefaultGetCommand.AddArgument(lineDefaultIdArgument);
            lineDefaultGetCommand.SetHandler(HandleLineDefaultGetAsync, lineDefaultIdArgument, verbosityOption);

            Command lineDefaultListCommand = new("list", "Get and existing invoice line default.");
            lineDefaultListCommand.SetHandler(HandleLineDefaultListAsync, verbosityOption);

            lineDefaultCommand.AddCommand(lineDefaultCreateCommand);
            lineDefaultCommand.AddCommand(lineDefaultGetCommand);
            lineDefaultCommand.AddCommand(lineDefaultListCommand);


            rootCommand.AddCommand(businessCommand);
            rootCommand.AddCommand(customerCommand);
            rootCommand.AddCommand(lineDefaultCommand);
            return rootCommand;
        }

        #region Businesses

        private static async Task HandleBusinessCreateAsync(Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), TokenProvider.GetAsync, Program.Stamp);

            Contracts.Invoicing.InvoiceBusiness invoiceBusiness = new()
            {
                AccountName = TokenProvider.GetAccount(),
                Name = Input.GetString(nameof(invoiceBusiness.Name)),
                AddressLine1 = Input.GetString(nameof(invoiceBusiness.AddressLine1)),
                AddressLine2 = Input.GetString(nameof(invoiceBusiness.AddressLine2)),
                Contact = Input.GetString(nameof(invoiceBusiness.Contact)),
            };

            invoiceBusiness = await client.Invoice.CreateBusinessAsync(invoiceBusiness);
            Console.WriteLine(invoiceBusiness.ToJsonString());
        }

        private static async Task HandleBusinessGetAsync(long id, Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), TokenProvider.GetAsync, Program.Stamp);
            Contracts.Invoicing.InvoiceBusiness invoiceBusiness = await client.Invoice.GetBusinessAsync(id);
            Console.WriteLine(invoiceBusiness.ToJsonString());
        }

        private static async Task HandleBusinessListAsync(Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), TokenProvider.GetAsync, Program.Stamp);
            List<Contracts.Invoicing.InvoiceBusiness> invoiceBusinesses = await client.Invoice.GetBusinessesAsync();

            Console.WriteLine();
            Output.WriteTable(
                invoiceBusinesses,
                nameof(Contracts.Invoicing.InvoiceBusiness.Id),
                nameof(Contracts.Invoicing.InvoiceBusiness.Name),
                nameof(Contracts.Invoicing.InvoiceBusiness.AddressLine1),
                nameof(Contracts.Invoicing.InvoiceBusiness.AddressLine2),
                nameof(Contracts.Invoicing.InvoiceBusiness.Contact),
                nameof(Contracts.Invoicing.InvoiceBusiness.CreatedDate));
        }

        #endregion // Businesses

        #region Customers

        private static async Task HandleCustomerCreateAsync(Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), TokenProvider.GetAsync, Program.Stamp);

            Contracts.Invoicing.InvoiceCustomer invoiceCustomer = new()
            {
                AccountName = TokenProvider.GetAccount(),
                Name = Input.GetString(nameof(invoiceCustomer.Name)),
                AddressLine1 = Input.GetString(nameof(invoiceCustomer.AddressLine1)),
                AddressLine2 = Input.GetString(nameof(invoiceCustomer.AddressLine2)),
                Contact = Input.GetString(nameof(invoiceCustomer.Contact)),
            };

            invoiceCustomer = await client.Invoice.CreateCustomerAsync(invoiceCustomer);
            Console.WriteLine(invoiceCustomer.ToJsonString());
        }

        private static async Task HandleCustomerGetAsync(long id, Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), TokenProvider.GetAsync, Program.Stamp);
            Contracts.Invoicing.InvoiceCustomer invoiceCustomer = await client.Invoice.GetCustomerAsync(id);
            Console.WriteLine(invoiceCustomer.ToJsonString());
        }

        private static async Task HandleCustomerListAsync(Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), TokenProvider.GetAsync, Program.Stamp);
            List<Contracts.Invoicing.InvoiceCustomer> invoiceCustomers = await client.Invoice.GetCustomersAsync();

            Console.WriteLine();
            Output.WriteTable(
                invoiceCustomers,
                nameof(Contracts.Invoicing.InvoiceCustomer.Id),
                nameof(Contracts.Invoicing.InvoiceCustomer.Name),
                nameof(Contracts.Invoicing.InvoiceCustomer.AddressLine1),
                nameof(Contracts.Invoicing.InvoiceCustomer.AddressLine2),
                nameof(Contracts.Invoicing.InvoiceCustomer.Contact),
                nameof(Contracts.Invoicing.InvoiceCustomer.CreatedDate));
        }

        #endregion // Customers

        #region Line Defaults

        private static async Task HandleLineDefaultCreateAsync(Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), TokenProvider.GetAsync, Program.Stamp);

            Contracts.Invoicing.InvoiceLineDefault invoiceLineDefault = new()
            {
                AccountName = TokenProvider.GetAccount(),
                Title = Input.GetString(nameof(invoiceLineDefault.Title)),
                Description = Input.GetString(nameof(invoiceLineDefault.Description)),
                Quantity = Input.GetDecimal(nameof(invoiceLineDefault.Quantity), defaultVal: 1),
                Unit = Input.GetString(nameof(invoiceLineDefault.Unit)),
                Price = Input.GetDecimal(nameof(invoiceLineDefault.Price)),
            };

            invoiceLineDefault = await client.Invoice.CreateLineDefaultAsync(invoiceLineDefault);
            Console.WriteLine(invoiceLineDefault.ToJsonString());
        }

        private static async Task HandleLineDefaultGetAsync(long id, Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), TokenProvider.GetAsync, Program.Stamp);
            Contracts.Invoicing.InvoiceLineDefault invoiceLineDefault = await client.Invoice.GetLineDefaultAsync(id);
            Console.WriteLine(invoiceLineDefault.ToJsonString());
        }

        private static async Task HandleLineDefaultListAsync(Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), TokenProvider.GetAsync, Program.Stamp);
            List<Contracts.Invoicing.InvoiceLineDefault> invoiceLineDefaults = await client.Invoice.GetLineDefaultsAsync();

            Console.WriteLine();
            Output.WriteTable(
                invoiceLineDefaults,
                nameof(Contracts.Invoicing.InvoiceLineDefault.Id),
                nameof(Contracts.Invoicing.InvoiceLineDefault.Title),
                nameof(Contracts.Invoicing.InvoiceLineDefault.Description),
                nameof(Contracts.Invoicing.InvoiceLineDefault.Quantity),
                nameof(Contracts.Invoicing.InvoiceLineDefault.Unit),
                nameof(Contracts.Invoicing.InvoiceLineDefault.Price),
                nameof(Contracts.Invoicing.InvoiceLineDefault.CreatedDate));
        }

        #endregion // Line Defaults
    }
}
