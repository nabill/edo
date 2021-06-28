using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure.Options;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.Mailing;
using HappyTravel.Edo.Api.Models.Payments;
using HappyTravel.Edo.Api.Models.Users;
using HappyTravel.Edo.Api.NotificationCenter.Services;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings.Documents;
using HappyTravel.Edo.Data.Bookings;
using HappyTravel.Edo.Data.Documents;
using HappyTravel.Edo.Notifications.Enums;
using HappyTravel.EdoContracts.Accommodations.Enums;
using HappyTravel.DataFormatters;
using HappyTravel.Money.Models;
using Microsoft.Extensions.Options;

namespace HappyTravel.Edo.Api.Services.Accommodations.Bookings.Mailing
{
    public class BookingDocumentsMailingService : IBookingDocumentsMailingService
    {
        public BookingDocumentsMailingService(IBookingDocumentsService documentsService,
            INotificationService notificationsService,
            IOptions<BookingMailingOptions> options)
        {
            _documentsService = documentsService;
            _notificationsService = notificationsService;
            _options = options.Value;
        }
        
        
        public Task<Result> SendVoucher(Booking booking, string email, string languageCode, SlimAgentContext agent)
        {
            return _documentsService.GenerateVoucher(booking, languageCode)
                .Bind(async voucher =>
                {
                    var voucherData = new VoucherData
                    {
                        Accommodation = voucher.Accommodation,
                        AgentName = voucher.AgentName,
                        BookingId = voucher.BookingId,
                        DeadlineDate = DateTimeFormatters.ToDateString(voucher.DeadlineDate),
                        NightCount = voucher.NightCount,
                        ReferenceCode = voucher.ReferenceCode,
                        SupplierReferenceCode = voucher.SupplierReferenceCode,
                        RoomDetails = voucher.RoomDetails,
                        CheckInDate = DateTimeFormatters.ToDateString(voucher.CheckInDate),
                        CheckOutDate = DateTimeFormatters.ToDateString(voucher.CheckOutDate),
                        MainPassengerName = voucher.MainPassengerName,
                        BannerUrl = voucher.BannerUrl,
                        LogoUrl = voucher.LogoUrl
                    };

                    return await _notificationsService.Send(agent: agent, 
                        messageData: voucherData, 
                        notificationType: NotificationTypes.BookingVoucher, 
                        email: email, 
                        templateId: _options.VoucherTemplateId);
                });
        }


        public Task<Result> SendInvoice(Booking booking, string email, bool sendCopyToAdmins, SlimAgentContext agent)
        {
            // TODO: hardcoded to be removed with UEDA-20
            var addresses = new List<string> {email};
            if (sendCopyToAdmins)
                addresses.AddRange(_options.CcNotificationAddresses);
            
            return _documentsService.GetActualInvoice(booking)
                .Bind(async invoice =>
                {
                    var (registrationInfo, data) = invoice;
                    var invoiceData = new InvoiceData
                    {
                        Number = registrationInfo.Number,
                        BuyerDetails = data.BuyerDetails,
                        InvoiceDate = DateTimeFormatters.ToDateString(registrationInfo.Date),
                        InvoiceItems = data.InvoiceItems
                            .Select(i => new InvoiceData.InvoiceItem
                            {
                                Number = i.Number,
                                Price = FormatPrice(i.Price),
                                Total = FormatPrice(i.Total),
                                AccommodationName = i.AccommodationName,
                                RoomDescription = i.RoomDescription,
                                RoomType = EnumFormatters.FromDescription<RoomTypes>(i.RoomType),
                                DeadlineDate = DateTimeFormatters.ToDateString(i.DeadlineDate),
                                MainPassengerName = PersonNameFormatters.ToMaskedName(i.MainPassengerFirstName, i.MainPassengerLastName)
                            })
                            .ToList(),
                        TotalPrice = FormatPrice(data.TotalPrice),
                        CurrencyCode = EnumFormatters.FromDescription(data.TotalPrice.Currency),
                        ReferenceCode = data.ReferenceCode,
                        SupplierReferenceCode = data.SupplierReferenceCode,
                        SellerDetails = data.SellerDetails,
                        PayDueDate = DateTimeFormatters.ToDateString(data.PayDueDate),
                        CheckInDate = DateTimeFormatters.ToDateString(data.CheckInDate),
                        CheckOutDate = DateTimeFormatters.ToDateString(data.CheckOutDate),
                        PaymentStatus = EnumFormatters.FromDescription(data.PaymentStatus),
                        DeadlineDate = DateTimeFormatters.ToDateString(data.DeadlineDate)
                    };

                    return await _notificationsService.Send(agent: agent,
                        messageData: invoiceData,
                        notificationType: NotificationTypes.BookingInvoice,
                        emails: addresses,
                        templateId: _options.InvoiceTemplateId);
                });
        }
        
        
        public async Task<Result> SendReceiptToCustomer((DocumentRegistrationInfo RegistrationInfo, PaymentReceipt Data) receipt, string email, ApiCaller apiCaller)
        {
            var (registrationInfo, paymentReceipt) = receipt;

            var payload = new PaymentReceiptData
            {
                Date = DateTimeFormatters.ToDateString(registrationInfo.Date),
                Number = registrationInfo.Number,
                CustomerName = paymentReceipt.CustomerName,
                Amount = MoneyFormatter.ToCurrencyString(paymentReceipt.Amount, paymentReceipt.Currency),
                Method = EnumFormatters.FromDescription(paymentReceipt.Method),
                InvoiceNumber = paymentReceipt.InvoiceInfo.Number,
                InvoiceDate = DateTimeFormatters.ToDateString(paymentReceipt.InvoiceInfo.Date),
                ReferenceCode = paymentReceipt.ReferenceCode,
                AccommodationName = paymentReceipt.AccommodationName,
                RoomDetails = paymentReceipt.ReceiptItems.Select(r => new PaymentReceiptData.RoomDetail
                {
                    DeadlineDate = DateTimeFormatters.ToDateString(r.DeadlineDate),
                    RoomType = r.RoomType
                }).ToList(),
                CheckInDate = DateTimeFormatters.ToDateString(paymentReceipt.CheckInDate),
                CheckOutDate = DateTimeFormatters.ToDateString(paymentReceipt.CheckOutDate),
                BuyerInformation = new PaymentReceiptData.Buyer
                {
                    Address = paymentReceipt.BuyerDetails.Address,
                    ContactPhone = paymentReceipt.BuyerDetails.ContactPhone,
                    Email = paymentReceipt.BuyerDetails.Email,
                    Name = paymentReceipt.BuyerDetails.Name
                },
                DeadlineDate = DateTimeFormatters.ToDateString(paymentReceipt.DeadlineDate)
            };

            return await _notificationsService.Send(apiCaller: apiCaller,
                        messageData: payload,
                        notificationType: NotificationTypes.SuccessfulPaymentReceipt,
                        email: email,
                        templateId: _options.BookingReceiptTemplateId);
        }
        
        
        private static string FormatPrice(MoneyAmount moneyAmount) 
            => MoneyFormatter.ToCurrencyString(moneyAmount.Amount, moneyAmount.Currency);

        
        private readonly IBookingDocumentsService _documentsService;
        private readonly INotificationService _notificationsService;
        private readonly BookingMailingOptions _options;
    }
}