using System;
using System.Collections.Generic;
using System.Globalization;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings.Payments;
using HappyTravel.Edo.Data.Bookings;
using HappyTravel.Money.Enums;
using HappyTravel.Money.Models;
using Xunit;
using Booking = HappyTravel.Edo.Data.Bookings.Booking;

namespace HappyTravel.Edo.UnitTests.Tests.Extensions.BookingExtensionsTests
{
    public class MoneyRefundCalculation : IDisposable
    {
        [Fact]
        public void Everything_unknown_should_not_have_penalty()
        {
            _booking.Rooms = new List<BookedRoom>
            {
                MakeBookedRoom(
                    deadline: new Deadline(null, null, null, true),
                    price: new MoneyAmount(100m, Currencies.USD))
            };
            var forDate = new DateTime(2020, 1, 1);

            var cancellationPenalty = BookingCancellationPenaltyCalculator.Calculate(_booking, forDate);

            Assert.Equal(0, cancellationPenalty.Amount);
        }


        [Fact]
        public void No_policies_before_deadline_should_not_have_penalty()
        {
            _booking.Rooms = new List<BookedRoom>
            {
                MakeBookedRoom(
                    deadline: new Deadline(new DateTime(2020, 1, 2), null, null, true),
                    price: new MoneyAmount(100m, Currencies.USD))
            };
            var forDate = new DateTime(2020, 1, 1);

            var cancellationPenalty = BookingCancellationPenaltyCalculator.Calculate(_booking, forDate);

            Assert.Equal(0, cancellationPenalty.Amount);
        }

        
        [Fact]
        public void No_policies_after_deadline_should_have_100_percent_penalty()
        {
            _booking.Rooms = new List<BookedRoom>
            {
                MakeBookedRoom(
                    deadline: new Deadline(new DateTime(2020, 1, 1), null, null, true),
                    price: new MoneyAmount(100m, Currencies.USD))
            };
            var forDate = new DateTime(2020, 1, 2);

            var cancellationPenalty = BookingCancellationPenaltyCalculator.Calculate(_booking, forDate);

            Assert.Equal(100, cancellationPenalty.Amount);
        }

        
        [Fact]
        public void Existing_policies_should_ignore_deadline_date()
        {
            _booking.Rooms = new List<BookedRoom>
            {
                MakeBookedRoom(
                    deadline: new Deadline(
                        new DateTime(2020, 1, 1),
                        new List<CancellationPolicy>
                        {
                            new CancellationPolicy(fromDate: new DateTime(2020, 1, 3), 0.3d, null)
                        }, null, true),
                    price: new MoneyAmount(100m, Currencies.USD))
            };
            var forDate = new DateTime(2020, 1, 2);

            var cancellationPenalty = BookingCancellationPenaltyCalculator.Calculate(_booking, forDate);

            Assert.Equal(0, cancellationPenalty.Amount);
        }


        [Fact]
        public void Penalty_amount_should_be_ceiled()
        {
            _booking.Rooms = new List<BookedRoom>
            {
                MakeBookedRoom(
                    deadline: new Deadline(
                        new DateTime(2020, 2, 1),
                        new List<CancellationPolicy>
                        {
                            new(fromDate: new DateTime(2020, 2, 1), 50.53533d, null)
                        }, null, true),
                    price: new MoneyAmount(100m, Currencies.USD))
            };
            var forDate = new DateTime(2020, 2, 2);

            var cancellationPenalty = BookingCancellationPenaltyCalculator.Calculate(_booking, forDate);

            Assert.Equal(50.54m, cancellationPenalty.Amount);
        }
        

        [Theory]
        [InlineData(2, 0)]
        [InlineData(3, 30)]
        [InlineData(4, 30)]
        [InlineData(5, 60)]
        [InlineData(6, 60)]
        [InlineData(7, 60)]
        [InlineData(8, 80)]
        [InlineData(9, 80)]
        [InlineData(10, 100)]
        [InlineData(11, 100)]
        public void Existing_policies_should_show_refund_according_date(int day, int expectedPenalty)
        {
            _booking.Rooms = new List<BookedRoom>
            {
                MakeBookedRoom(
                    deadline: new Deadline(
                        new DateTime(2020, 1, 3),
                        new List<CancellationPolicy>
                        {
                            new CancellationPolicy(fromDate: new DateTime(2020, 1, 3), 30d, null),
                            new CancellationPolicy(fromDate: new DateTime(2020, 1, 5), 60d, null),
                            new CancellationPolicy(fromDate: new DateTime(2020, 1, 8), 80d, null),
                            new CancellationPolicy(fromDate: new DateTime(2020, 1, 10), 100d, null),
                        }, null, true),
                    price: new MoneyAmount(100m, Currencies.USD))
            };
            var forDate = new DateTime(2020, 1, day);

            var cancellationPenalty = BookingCancellationPenaltyCalculator.Calculate(_booking, forDate);

            Assert.Equal(expectedPenalty, cancellationPenalty.Amount);
        }
        
        
        [Theory]
        [InlineData("2019/01/01")]
        [InlineData("2021/01/01")]
        public void Advance_purchase_rate_rooms_should_refund_nothing_ignoring_policies(string deadline)
        {
            var deadlineDate = DateTime.ParseExact(deadline, "yyyy/MM/dd", CultureInfo.InvariantCulture);
            _booking.Rooms = new List<BookedRoom>
            {
                MakeBookedRoom(
                    deadline: new Deadline(
                        deadlineDate,
                        new List<CancellationPolicy>
                        {
                            new(fromDate: deadlineDate, 72.00, null)
                        }, null, true),
                    price: new MoneyAmount(100m, Currencies.USD),
                    isAdvancePurchaseRate: true)
            };
            var forDate = new DateTime(2020, 2, 2);

            var cancellationPenalty = BookingCancellationPenaltyCalculator.Calculate(_booking, forDate);

            Assert.Equal(100m, cancellationPenalty.Amount);
        }


        private BookedRoom MakeBookedRoom(Deadline deadline, MoneyAmount price, List<CancellationPolicy>? policies = null, bool isAdvancePurchaseRate = false) =>
            new BookedRoom(default, default, price, default, default, default, default, new List<KeyValuePair<string, string>>(), deadline, new List<Passenger>(), isAdvancePurchaseRate, default);


        private readonly Booking _booking = new Booking{Currency = Currencies.USD};

        public void Dispose()
        {
            
        }
    }
}
