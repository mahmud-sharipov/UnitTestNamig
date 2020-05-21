using Eagle.Framework.Server.Entity;
using Eagle.Framework.Server.Plugin;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace Sales.Payment
{
    using Eagle.Core;
    using Eagle.Framework.Common.Data;
    using Eagle.Framework.Server.UICommunications;
    using Model.Entities;
    using System;
    using System.Collections;
    using System.Collections.Generic;

#if DEBUG
    [TestClass]
#endif
    public class UnitTestNamingExample : BusinessLogicBase<SalesInvoice>
    {
        #region Access

        #region example 1
        [AccessCompute(SalesInvoice.pn.cmdProcess, new string[] { SalesInvoice.pn.BillingCustomer, SalesInvoice.pn.Balance, SalesInvoice.pn.IsChargeTemporaryAllowed }, AllowedProperties = new string[] { PaymentType.pn.Name },
            UserSummary = "Cash customer can't have outstanding invoices. But it can have credit invoices and also can apply money on account")]
        public void SalesInvoice_accesscmdProcess_ChargeIsNotAllowed_Compute(SalesInvoice entity, AccessComputeEventArgs e)
        {
            var customer = entity.BillingCustomer;
            if (customer != null)
            {
                bool isChargeAllowed = customer.AllowedPaymentTypes.Any(pt => pt.Name == "Charge");
                if (!isChargeAllowed && entity.Balance > 0 && !entity.IsChargeTemporaryAllowed)
                    e.NewValue = new AccessValue(BLPropertyAccess.HiddenAndNotApplicable, "charge is not allowed");
            }
        }

#if DEBUG
        [TestMethod]
        public void SalesInvoicecmdProcessAccessCompute_BillingCustomerIsNull_Applicable()
        {
            var salesInvoice = new SalesInvoice(TestContext);
            Assert.IsFalse(IsHiddenAndNotApplicable(salesInvoice.accesscmdProcess));
        }

        [TestMethod]
        public void SalesInvoicecmdProcessAccessCompute_ChargeIsAllowedForCustomer_Applicable()
        {
            var customer = PaymentHelper.GetCustomer("LGBUI");
            var salesInvoice = PaymentHelper.CreateInvoiceWithDetail(customer, PaymentHelper.DefaultProduct, 1, 10).Invoice;
            Assert.IsFalse(IsHiddenAndNotApplicable(salesInvoice.accesscmdProcess));
        }

        [TestMethod]
        public void SalesInvoicecmdProcessAccessCompute_ChargeIsNotAllowedForCustomer_HiddenAndNotApplicable()
        {
            var cashCustomer = PaymentHelper.GetCustomer("LGBUI");
            PaymentHelper.DisallowCharge(cashCustomer);
            var salesInvoice = PaymentHelper.CreateInvoiceWithDetail(cashCustomer, PaymentHelper.DefaultProduct, 1, 10).Invoice;
            Assert.IsTrue(IsHiddenAndNotApplicable(salesInvoice.accesscmdProcess));
        }

        [TestMethod]
        public void SalesInvoicecmdProcessAccessCompute_ChargeIsNotAllowedAndBalanceIsZero_Applicable()
        {
            var cashCustomer = PaymentHelper.GetCustomer("LGBUI");
            PaymentHelper.DisallowCharge(cashCustomer);
            var salesInvoice = PaymentHelper.CreateInvoiceWithDetail(cashCustomer, PaymentHelper.DefaultProduct, 1, 10).Invoice;
            var payment = PaymentHelper.CreatePayment(cashCustomer, salesInvoice.Balance, salesInvoice).Payment;
            payment.cmdPay.TryExecute();
            Assert.IsFalse(IsHiddenAndNotApplicable(salesInvoice.accesscmdProcess));
        }

        [TestMethod]
        public void SalesInvoicecmdProcessAccessCompute_ChargeIsNotAllowedAndBalanceIsNegative_Applicable()
        {
            var cashCustomer = PaymentHelper.GetCustomer("LGBUI");
            PaymentHelper.DisallowCharge(cashCustomer);
            var salesInvoice = PaymentHelper.CreateInvoiceWithDetail(cashCustomer, PaymentHelper.DefaultProduct, 1, 10).Invoice;
            var payment = PaymentHelper.CreatePayment(cashCustomer, salesInvoice.Balance + 10, salesInvoice).Payment;
            payment.cmdPay.TryExecute();
            Assert.IsFalse(IsHiddenAndNotApplicable(salesInvoice.accesscmdProcess));
        }

        [TestMethod]
        public void SalesInvoicecmdProcessAccessCompute_ChargeIsTemporaryAllowed_Applicable()
        {
            var cashCustomer = PaymentHelper.GetCustomer("LGBUI");
            PaymentHelper.DisallowCharge(cashCustomer);
            var salesInvoice = PaymentHelper.CreateInvoiceWithDetail(cashCustomer, PaymentHelper.DefaultProduct, 1, 10).Invoice;
            salesInvoice.IsChargeTemporaryAllowed = true;
            Assert.IsFalse(IsHiddenAndNotApplicable(salesInvoice.accesscmdProcess));
        }

        bool IsHiddenAndNotApplicable(PropertyAccess access) => !access.ClientReadable && !access.ServerWritable;
#endif
        #endregion

        #region example 2
        [AccessCompute(BankAccount.pn.accessNextCheckNumber, new string[] { BankAccount.pn.Type }, UserSummary = "Next check number should be show if account type is 'Checking'")]
        public void BankAccount_accessNextCheckNumber_Compute(BankAccount entity, AccessComputeEventArgs e)
        {
            if (entity.Type != BankAccountType.Checking)
                e.NewValue = new AccessValue(BLPropertyAccess.Hidden, "account type is not 'Checking'");
        }

#if DEBUG
        [TestMethod]
        public void BankAccountNextCheckNumberAccessCompute_AccountTypeIsNotChecking_Hidden()
        {
            var bankAccount = new BankAccount(TestContext) { Type = BankAccountType.CreditCard };
            Assert.IsFalse(bankAccount.accessNextCheckNumber.ClientReadable, "NextCheckNumber should be hide because account type is not 'Checking'");
        }

        [TestMethod]
        public void BankAccountNextCheckNumberAccessCompute_AccountTypeChecking_Writable()
        {
            var bankAccount = new BankAccount(TestContext) { Type = BankAccountType.Checking };
            Assert.IsTrue(bankAccount.accessNextCheckNumber.ClientWritable, "NextCheckNumber should be writable because account type is 'Checking'");
        }
#endif
        #endregion

        #endregion

        #region Compute

        #region example 1
        [Compute(Customer.pn.AllPaymentTypes, new string[] { Customer.pn.IsAnonymousCustomer },
            AllowedProperties = new string[] { PaymentType.pn.Name, CustomPaymentType.pn.IsActive, CustomPaymentType.pn.IsAvailableForIncomingPayments, CustomPaymentType.pn.IsAvailableForOutgoingPayments },
            UserSummary = "Charge, PayFromAccount and inactive payment types are not allowed for anonymous customer")]
        public void Customer_AllPaymentTypes_Compute(Customer entity, ComputeEventArgs<IEnumerable<PaymentType>> e)
        {
            IEnumerable<PaymentType> paymentTypes = e.Context.GetEntities<PaymentType>();
            if (entity.IsAnonymousCustomer)
                e.NewValue = paymentTypes.Where(pt => pt.Name != "Charge" && pt.Name != "Pay from account");
            else
                e.NewValue = paymentTypes;
        }

#if DEBUG
        [TestMethod]
        public void CustomerAllPaymentTypesCompute_IsAnonymousCustomer_ChargeAndPayFromAccountNotAvailable()
        {
            var customer = new Customer(TestContext);
            var allPaymentTypesCount = TestContext.GetEntities<PaymentType>().Count();
            Assert.IsTrue(customer.AllPaymentTypes.All(pt => pt.Name != "Charge" && pt.Name != "Pay from account"));
            Assert.AreEqual(allPaymentTypesCount - 2, customer.AllPaymentTypes.Count);
        }

        [TestMethod]
        public void CustomerAllPaymentTypesCompute_IsNotAnonymousCustomer_AllPaymentTypesAreAvailable()
        {
            var customer = new Customer(TestContext);
            var allPaymentTypesCount = TestContext.GetEntities<PaymentType>().Count();
            customer.IsAnonymousCustomer = false;
            Assert.IsTrue(customer.AllPaymentTypes.Any(pt => pt.Name == "Charge"));
            Assert.IsTrue(customer.AllPaymentTypes.Any(pt => pt.Name == "Pay from account"));
        }
#endif
        #endregion

        #endregion

        #region Executing

        #region example 1
        [MessageTemplate(0, SeverityLevel.Error,
              "This document cannot be paid or processed",
              "This customer has no payment types allowed.",
              "Please, open {0} and allow a payment type.")]
        [CommandExecuting(SalesInvoice.pn.cmdProcessAndOrPay, UserSummary = "Show message if customer doesn't have any allowed payment type")]
        public void SalesInvoice_cmdProcessAndOrPay_Executing(SalesInvoice entity, ExecutingEventArgs e)
        {
            var customer = entity.BillingCustomer;
            if (!customer.AllowedPaymentTypeLinks.Any())
                entity.cmdProcessAndOrPay.Message(e, 0, new[] { PaymentHelper.CreateHyperlinkToCustomerAllowedPaymentTypeList(entity.BillingCustomer) });
        }

#if DEBUG
        [TestMethod]
        public void SalesInvoicecmdProcessAndOrPayExecuting_ThereIsNotAnyAllowedType_DisplayMessage()
        {
            var message = new MessageId(this, nameof(SalesInvoice_cmdProcessAndOrPay_Executing));
            var salesInvoice = PaymentHelper.CreateInvoiceWithDetail(PaymentHelper.GetCustomer("LGBUI"), PaymentHelper.DefaultProduct, 1, 12).Invoice;
            salesInvoice.BillingCustomer.AllowedPaymentTypeLinks.Clear();
            salesInvoice.cmdProcessAndOrPay.Execute();
            Assert.IsTrue(message.Any(TestContext.Messages));
        }

        [TestMethod]
        public void SalesInvoicecmdProcessAndOrPayExecuting_WhenThereAreSomeAllowedType_NoMessage()
        {
            var message = new MessageId(this, nameof(SalesInvoice_cmdProcessAndOrPay_Executing));
            var customer = TestContext.GetEntity<Customer>("AMERET");
            var invoice = PaymentHelper.CreateInvoiceWithDetail(PaymentHelper.GetCustomer("AMERET"), PaymentHelper.DefaultProduct, 1, 12).Invoice;
            invoice.cmdProcessAndOrPay.Execute();
            Assert.IsFalse(message.Any(TestContext.Messages));
        }
#endif
        #endregion

        #endregion

        #region Executed

        #region example 1
        [CommandExecuted(SalesInvoice.pn.cmdProcess, UserSummary = "Show warning message if there is no real payment type")]
        [MessageTemplate(SeverityLevel.Warning, "Customer doesn't have any real payment method",
            "Customer doesn't have any real payment method you may not be able to pay this invoice", "Allow any real payment method for this customer")]
        public void SalesInvoice_cmdProcess_Executed(SalesInvoice entity, ExecutedEventArgs e)
        {
            if (!entity.BillingCustomer.AllowedPaymentTypes.Any(type => type.IsRealPaymentType))
                entity.Message(e);
        }

#if DEBUG
        [TestMethod]
        public void SalesInvoicecmdProcessExecuted_NoAllowedPaymentType_DisplayMessage()
        {
            var warningMessage = new MessageId(typeof(SalesInvoiceBL), nameof(SalesInvoiceBL.SalesInvoice_cmdProcess_Executed));
            var customer = PaymentHelper.GetCustomer("ZURMIK");
            var salesInvoice = PaymentHelper.CreateInvoiceWithDetail(customer, PaymentHelper.DefaultProduct, 10, 10).Invoice;
            customer.AllowedPaymentTypes.Clear();
            salesInvoice.cmdProcess.Execute();
            Assert.IsTrue(warningMessage.Any(TestContext.Messages));
        }

        [TestMethod]
        public void SalesInvoicecmdProcessExecuted_OnlyAllowedNoneRealPaymentType_DisplayMessage()
        {
            var warningMessage = new MessageId(typeof(SalesInvoiceBL), nameof(SalesInvoiceBL.SalesInvoice_cmdProcess_Executed));
            var customer = PaymentHelper.GetCustomer("ZURMIK");
            var salesInvoice = PaymentHelper.CreateInvoiceWithDetail(customer, PaymentHelper.DefaultProduct, 10, 10).Invoice;
            var charge = customer.AllowedPaymentTypes.Single(pt => pt.Name == "Charge");
            customer.AllowedPaymentTypes.Clear();
            customer.AllowedPaymentTypes.Add(charge);
            salesInvoice.cmdProcess.Execute();
            Assert.IsTrue(warningMessage.Any(TestContext.Messages));
        }

        [TestMethod]
        public void SalesInvoicecmdProcessExecuted_HasAllowedRealPaymentType_NoMessage()
        {
            var warningMessage = new MessageId(typeof(SalesInvoiceBL), nameof(SalesInvoiceBL.SalesInvoice_cmdProcess_Executed));
            var salesInvoice = PaymentHelper.CreateInvoiceWithDetail(PaymentHelper.GetCustomer("AMERET"), PaymentHelper.DefaultProduct, 10, 10).Invoice;
            salesInvoice.cmdProcess.Execute();
            Assert.IsFalse(warningMessage.Any(TestContext.Messages));
        }
#endif
        #endregion

        #endregion

        #region FilterEntities

        #region example 1
        [FilterEntities(DataTypes.OpenSalesDoc, UserSummary = "Selects only sales documents that is not closed")]
        public void OpenSalesDoc_FilterEntities(IQueryable<SalesDoc> query, FilterEntitiesEventArgs<SalesDoc> e)
        {
            e.Query = query.Where(sd => sd.ClosedEvent == null);
        }

#if DEBUG
        [TestMethod]
        public void SalesDoc_FilterEntities_OpenSalesDoc()
        {
            var testList = TestContext.DataProvider.GetEntities<SalesDoc>(DataTypes.OpenSalesDoc);
            Assert.IsTrue(testList.All(sd => sd.ClosedEvent == null), "There should not be any closed invoice");
        }
#endif
        #endregion

        #endregion

        #region CollectionChanged

        #region example 1
        [CollectionChanged(CustomerPaymentsView.pn.SelectedInvoices, UserSummary = "This subscriber will set default amount to payment link")]
        public void CustomerPaymentsView_SelectedInvoices_Changed(CustomerPaymentsView entity, EagleCollectionChangedEventArgs e)
        {
            var changedDoc = (e.ChangedItem as SalesDoc);

            if (e.Action == EagleCollectionChangedAction.Added)
                AddTemporaryPaymentLink(changedDoc, entity.CustomerPayment);
            else if (e.Action == EagleCollectionChangedAction.Removed)
                RemoveTemporaryLink(changedDoc);
        }

        void AddTemporaryPaymentLink(SalesDoc document, CustomerPayment payment)
        {
            var link = new SalesDocPaymentLink(payment) { Document = document };
            link.PaymentAmount = document.Balance;
            document.TemporaryPaymentLink = link;
        }

        void RemoveTemporaryLink(SalesDoc document)
        {
            var temporaryLink = document?.TemporaryPaymentLink;
            if (temporaryLink != null)
            {
                temporaryLink.cmdDelete.Execute();
                document.TemporaryPaymentLink = null;
            }
        }

#if DEBUG
        [TestMethod]
        //RemoveTemporaryLink_TempoprtyLinkIsNull_Null
        //RemoveTemporaryLink_TempoprtyLinkIsNull_NoAction
        public void RemoveTemporaryLink_TempoprtyLinkIsNull_NoError()
        {
            var doc = new Test_SalesDoc(TestContext);
            RemoveTemporaryLink(doc);
            Assert.IsFalse(TestContext.Messages.Any());
        }

        public void RemoveTemporaryLink_TempoprtyLinkIsNotNull_RemoveTeporaryLink()
        {
            var payment = new CustomerPayment(TestContext);
            var link = new SalesDocPaymentLink(payment);
            var doc = new Test_SalesDoc(TestContext);
            doc.TemporaryPaymentLink = link;
            RemoveTemporaryLink(doc);
            Assert.IsNull(doc.TemporaryPaymentLink);
            Assert.IsTrue(link.MarkedAsDeleted);
        }

        public void AddTemporaryPaymentLink_AddLinkToDocument()
        {
            var payment = new CustomerPayment(TestContext);
            var doc = new Test_SalesDoc(TestContext);
            AddTemporaryPaymentLink(doc, payment);
            Assert.IsNotNull(doc.TemporaryPaymentLink);
            Assert.AreEqual(payment, doc.TemporaryPaymentLink.Payment);
            Assert.AreEqual(doc.Balance, doc.TemporaryPaymentLink.PaymentAmount);
        }

        [TestMethod]
        public void CustomerPaymentsViewSelectedInvoicesChanged_Added_AddTemporaryLink()
        {
            var customer = TestContext.GetEntity<Customer>("AMERET");
            var paymentView = TestContext.GetEntities<CustomerPaymentsView>().Single();
            paymentView.Customer = customer;
            var invoice = paymentView.OutstandingInvoices.Single(pi => pi.DocNumber == "12344321");
            paymentView.SelectedInvoices.Add(invoice);
            Assert.IsNotNull(invoice.TemporaryPaymentLink);
        }

        [TestMethod]
        public void CustomerPaymentsViewSelectedInvoicesChanged_Removed_RemoveTemporaryLink()
        {
            var customer = TestContext.GetEntity<Customer>("AMERET");
            var paymentView = TestContext.GetEntities<CustomerPaymentsView>().Single();
            paymentView.Customer = customer;
            var invoice = paymentView.OutstandingInvoices.Single(pi => pi.DocNumber == "12344321");
            paymentView.SelectedInvoices.Add(invoice);
            paymentView.SelectedInvoices.RemoveItem(invoice);
            Assert.IsNull(invoice.TemporaryPaymentLink);
        }
#endif
        #endregion

        #endregion
    }
}
