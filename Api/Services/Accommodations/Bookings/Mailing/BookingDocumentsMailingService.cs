using CSharpFunctionalExtensions;
using HappyTravel.DataFormatters;
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
using HappyTravel.Money.Models;
using System.Linq;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.Services.Agents;

namespace HappyTravel.Edo.Api.Services.Accommodations.Bookings.Mailing
{
    public class BookingDocumentsMailingService : IBookingDocumentsMailingService
    {
        public BookingDocumentsMailingService(IBookingDocumentsService documentsService,
            INotificationService notificationsService, IAgentContextService agentContext)
        {
            _documentsService = documentsService;
            _notificationsService = notificationsService;
            _agentContext = agentContext;
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
                        PropertyOwnerConfirmationCode = voucher.PropertyOwnerConfirmationCode,
                        RoomConfirmationCodes = string.Join("; ", voucher.RoomDetails.Select(r => r.SupplierRoomReferenceCode)),
                        RoomDetails = voucher.RoomDetails,
                        CheckInDate = DateTimeFormatters.ToDateString(voucher.CheckInDate),
                        CheckOutDate = DateTimeFormatters.ToDateString(voucher.CheckOutDate),
                        MainPassengerName = voucher.MainPassengerName,
                        BannerUrl = voucher.BannerUrl,
                        LogoUrl = voucher.LogoUrl,
                        SpecialValues = voucher.SpecialValues?.ToDictionary(s => s.Key, s => s.Value)
                    };

                    return await _notificationsService.Send(agent: agent,
                        messageData: voucherData,
                        notificationType: NotificationTypes.BookingVoucher,
                        email: email);
                });
        }


        public Task<Result> SendInvoice(Booking booking, string email, bool sendCopyToAdmins, SlimAgentContext agent)
        {
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
                        NetPrice = (data.TotalPrice != data.NetPrice) ? FormatPrice(data.NetPrice) : null,
                        Commission = (data.TotalPrice != data.NetPrice) ? FormatPrice(data.TotalPrice - data.NetPrice) : null,
                        CurrencyCode = EnumFormatters.FromDescription(data.TotalPrice.Currency),
                        ReferenceCode = data.ReferenceCode,
                        ClientReferenceCode = data.ClientReferenceCode,
                        SupplierReferenceCode = data.SupplierReferenceCode,
                        SellerDetails = data.SellerDetails,
                        PayDueDate = DateTimeFormatters.ToDateString(data.PayDueDate),
                        CheckInDate = DateTimeFormatters.ToDateString(data.CheckInDate),
                        CheckOutDate = DateTimeFormatters.ToDateString(data.CheckOutDate),
                        PaymentStatus = EnumFormatters.FromDescription(data.PaymentStatus),
                        DeadlineDate = DateTimeFormatters.ToDateString(data.DeadlineDate)
                    };

                    var errors = string.Empty;
                    var (_, isAgentNotificationFailure, agentNotificationError) = await _notificationsService.Send(agent: agent,
                        messageData: invoiceData,
                        notificationType: NotificationTypes.BookingInvoice,
                        email: email);
                    if (isAgentNotificationFailure)
                        errors = agentNotificationError;

                    if (sendCopyToAdmins)
                    {
                        var (_, isAdminsNotificationFailure, adminsNotificationError) = await _notificationsService.Send(messageData: invoiceData,
                            notificationType: NotificationTypes.BookingInvoice);
                        if (isAdminsNotificationFailure)
                            errors = string.Join(", ", errors, adminsNotificationError);
                    }

                    if (errors != string.Empty)
                        return Result.Failure(errors);

                    return Result.Success();
                });
        }


        public async Task<Result> SendReceiptToCustomer((DocumentRegistrationInfo RegistrationInfo, PaymentReceipt Data) receipt, string email)
        {
            var (registrationInfo, paymentReceipt) = receipt;
            var agent = await _agentContext.GetAgent();

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

            return await _notificationsService.Send(apiCaller: agent.ToApiCaller(),
                        messageData: payload,
                        notificationType: NotificationTypes.SuccessfulPaymentReceipt,
                        email: email);
        }


        public Task<Result> SendPaymentRefundNotification(PaymentRefundMail payload, string email, SlimAgentContext agentContext)
            => _notificationsService.Send(agent: agentContext,
            messageData: payload,
            notificationType: NotificationTypes.PaymentRefund,
            email: email);


        private static string FormatPrice(MoneyAmount moneyAmount)
            => MoneyFormatter.ToCurrencyString(moneyAmount.Amount, moneyAmount.Currency);


        private readonly IBookingDocumentsService _documentsService;
        private readonly INotificationService _notificationsService;
        private readonly IAgentContextService _agentContext;
    }
}