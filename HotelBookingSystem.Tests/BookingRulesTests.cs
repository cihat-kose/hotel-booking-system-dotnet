using HotellBookingSystem.Models;
using HotellBookingSystem.Payments;

namespace HotelBookingSystem.Tests;

public class BookingRulesTests
{
    private static readonly DateTime Start = new(2030, 6, 10);
    private readonly Hotel _hotel = new("Test Hotel");
    private readonly Room _room = new SingleRoom("101", true);
    private readonly Guest _guest;

    public BookingRulesTests()
    {
        _guest = new RegularGuest("Demo guest", "guest@example.com");
        _hotel.RegisterRoom(_room);
        _hotel.RegisterGuest(_guest);
    }

    private Booking Book(int start = 0, int end = 3, IPayable? payment = null, Guest? guest = null) =>
        _hotel.CreateBooking((guest ?? _guest).GuestId, _room.RoomNumber,
            Start.AddDays(start), Start.AddDays(end), payment ?? new CardPayment());

    [Fact] // Original SimpleTests scenario 1.
    public void VipDiscountIsFifteenPercent() =>
        Assert.Equal(850m, new VipGuest("Demo", "vip@example.com").GetDiscount(1000m));

    [Fact] // Original SimpleTests scenario 2.
    public void ThreeNightsInSingleRoomCost2400() => Assert.Equal(2400m, Book().CalculateTotalPrice());

    [Theory] // Original scenarios 3 and 5, including containment and exact equality.
    [InlineData(0, 3)]
    [InlineData(1, 2)]
    [InlineData(-1, 4)]
    [InlineData(-1, 1)]
    [InlineData(2, 4)]
    public void OverlappingDatesRejectBookingWithoutCharging(int start, int end)
    {
        Book();
        var payment = new RecordingPayment();
        Assert.Throws<InvalidOperationException>(() => Book(start, end, payment));
        Assert.Empty(_hotel.GetAvailableRooms(Start.AddDays(start), Start.AddDays(end)));
        Assert.Equal(0, payment.Calls);
        Assert.Single(_hotel.BookingHistory);
    }

    [Theory]
    [InlineData(-3, 0)]
    [InlineData(3, 6)]
    public void AdjacentBookingsAreAllowed(int start, int end)
    {
        Book();
        Assert.Contains(_room, _hotel.GetAvailableRooms(Start.AddDays(start), Start.AddDays(end)));
        Book(start, end);
        Assert.Equal(2, _hotel.BookingHistory.Count);
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(1, 0)]
    [InlineData(0.5, 2)]
    [InlineData(0, 2.5)]
    public void InvalidDatesAreRejectedBeforePayment(double start, double end)
    {
        var payment = new RecordingPayment();
        Assert.Throws<ArgumentException>(() => _hotel.CreateBooking(_guest.GuestId, "101",
            Start.AddDays(start), Start.AddDays(end), payment));
        Assert.Throws<ArgumentException>(() => _hotel.GetAvailableRooms(Start.AddDays(start), Start.AddDays(end)));
        Assert.Equal(0, payment.Calls);
        Assert.Empty(_guest.ActiveBookings);
        Assert.Empty(_hotel.BookingHistory);
    }

    [Fact] // Original SimpleTests scenario 4, through the paid booking path.
    public void CheckoutReleasesOccupancyAndActiveBookingButKeepsHistory()
    {
        var booking = Book();
        Assert.True(_room.IsAvailable);
        _hotel.CheckInBooking(booking.BookingId);
        Assert.False(_room.IsAvailable);
        _hotel.CheckOutBooking(booking.BookingId);
        Assert.True(_room.IsAvailable);
        Assert.Equal(BookingStatus.CheckedOut, booking.Status);
        Assert.Empty(_hotel.GetGuestBookings(_guest.GuestId));
        Assert.Same(booking, Assert.Single(_hotel.BookingHistory));
        Assert.Contains(_room, _hotel.GetAvailableRooms(Start, Start.AddDays(3)));
    }

    [Fact]
    public void OccupancyAllowsFutureReservationsButPreventsTwoSimultaneousCheckIns()
    {
        var current = Book();
        current.CheckIn();
        Assert.Contains(_room, _hotel.GetAvailableRooms(Start.AddDays(3), Start.AddDays(6)));
        var future = Book(3, 6);
        Assert.Throws<InvalidOperationException>(() => future.CheckIn());
        Assert.Equal(BookingStatus.Booked, future.Status);
        Assert.False(_room.IsAvailable);
        current.CheckOut();
        future.CheckIn();
        Assert.False(_room.IsAvailable);
        Assert.Equal(BookingStatus.CheckedIn, future.Status);
    }

    [Theory]
    [InlineData(false, 3)]
    [InlineData(true, 10)]
    public void BookingLimitIsCheckedBeforePaymentAndCancellationRestoresCapacity(bool vip, int limit)
    {
        Guest guest = vip ? new VipGuest("VIP", "vip@example.com") : _guest;
        if (vip) _hotel.RegisterGuest(guest);
        for (var i = 0; i < limit; i++) Book(i, i + 1, guest: guest);
        var payment = new RecordingPayment();
        Assert.Throws<InvalidOperationException>(() => Book(limit, limit + 1, payment, guest));
        Assert.Equal(0, payment.Calls);
        Assert.Equal(limit, guest.ActiveBookings.Count);
        _hotel.CancelBooking(guest.ActiveBookings[0].BookingId);
        Book(limit, limit + 1, payment, guest);
        Assert.Equal(1, payment.Calls);
        Assert.Equal(limit, guest.ActiveBookings.Count);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void PaymentFailureLeavesNoBookingsOccupancyOrLoyaltySideEffects(bool throws)
    {
        var vip = new VipGuest("VIP", "vip@example.com");
        _hotel.RegisterGuest(vip);
        var payment = new RecordingPayment { Success = false, Throw = throws };
        var error = Assert.Throws<InvalidOperationException>(() => Book(payment: payment, guest: vip));
        Assert.DoesNotContain("private provider detail", error.ToString());
        Assert.Empty(vip.ActiveBookings);
        Assert.Empty(_hotel.BookingHistory);
        Assert.True(_room.IsAvailable);
        Assert.Equal(0, vip.LoyaltyPoints);
        Assert.Contains(_room, _hotel.GetAvailableRooms(Start, Start.AddDays(3)));
        var retry = Book(guest: vip);
        Assert.True(retry.IsPaid);
        Assert.Equal(10, vip.LoyaltyPoints);
    }

    [Fact]
    public void VipBookingChargesDiscountedPriceAndAwardsPointsOnce()
    {
        var vip = new VipGuest("VIP", "vip@example.com");
        _hotel.RegisterGuest(vip);
        var payment = new RecordingPayment();
        var booking = Book(payment: payment, guest: vip);
        Assert.Equal(2040m, payment.Amount);
        Assert.Equal(2040m, booking.CalculateTotalPrice());
        booking.CheckIn();
        booking.CheckOut();
        Assert.Equal(1, payment.Calls);
        Assert.Equal(10, vip.LoyaltyPoints);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void CancellationReleasesBookingAndKeepsPaidHistoryWithoutRefund(bool checkedIn)
    {
        var booking = Book();
        if (checkedIn) booking.CheckIn();
        _hotel.CancelBooking(booking.BookingId);
        Assert.Equal(BookingStatus.Cancelled, booking.Status);
        Assert.True(booking.IsPaid);
        Assert.False(booking.IsActive);
        Assert.True(_room.IsAvailable);
        Assert.Empty(_guest.ActiveBookings);
        Assert.Single(_hotel.BookingHistory);
        Book();
    }

    [Fact]
    public void CancellingFutureBookingDoesNotReleaseCurrentOccupant()
    {
        var current = Book();
        current.CheckIn();
        Book(3, 6).Cancel();
        Assert.False(_room.IsAvailable);
        Assert.Equal(BookingStatus.CheckedIn, current.Status);
    }

    [Fact]
    public void CheckoutBeforeCheckInAndRepeatedCheckInAreRejected()
    {
        var booking = Book();
        Assert.Throws<InvalidOperationException>(() => booking.CheckOut());
        Assert.Equal(BookingStatus.Booked, booking.Status);
        booking.CheckIn();
        Assert.Throws<InvalidOperationException>(() => booking.CheckIn());
        Assert.Equal(BookingStatus.CheckedIn, booking.Status);
        Assert.False(_room.IsAvailable);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void CompletedBookingsRejectAllFurtherTransitions(bool cancel)
    {
        var booking = Book();
        if (cancel) booking.Cancel();
        else { booking.CheckIn(); booking.CheckOut(); }
        var status = booking.Status;
        Assert.Throws<InvalidOperationException>(() => booking.CheckIn());
        Assert.Throws<InvalidOperationException>(() => booking.CheckOut());
        Assert.Throws<InvalidOperationException>(() => booking.Cancel());
        Assert.Equal(status, booking.Status);
        Assert.True(_room.IsAvailable);
    }

    [Fact]
    public void PaidPriceRemainsStableWhenRoomRateChanges()
    {
        var booking = Book();
        _room.PricePerNight = 1500m;
        Assert.Equal(2400m, booking.CalculateTotalPrice());
        Assert.Equal(4500m, Book(3, 6).CalculateTotalPrice());
    }

    [Theory]
    [InlineData("Room")]
    [InlineData("Guest")]
    [InlineData("CheckInDate")]
    [InlineData("CheckOutDate")]
    [InlineData("PaymentMethod")]
    public void ReservationIdentityAndTermsCannotBeReassigned(string property) =>
        Assert.Null(typeof(Booking).GetProperty(property)!.SetMethod);

    [Fact]
    public void CollectionsCannotBeCastBackToMutableLists()
    {
        var booking = Book();
        Assert.Throws<NotSupportedException>(() => ((IList<Booking>)_guest.ActiveBookings).Clear());
        Assert.Throws<NotSupportedException>(() => ((IList<Booking>)_hotel.BookingHistory).Clear());
        Assert.Throws<NotSupportedException>(() => ((IList<Room>)_hotel.Rooms).Clear());
        Assert.Throws<NotSupportedException>(() => ((IList<Guest>)_hotel.Guests).Clear());
        Assert.Same(booking, Assert.Single(_guest.ActiveBookings));
    }

    [Fact]
    public void UnknownGuestRoomAndNullPaymentAreRejected()
    {
        var payment = new RecordingPayment();
        Assert.Throws<InvalidOperationException>(() => _hotel.CreateBooking("missing", "101", Start, Start.AddDays(1), payment));
        Assert.Throws<InvalidOperationException>(() => _hotel.CreateBooking(_guest.GuestId, "missing", Start, Start.AddDays(1), payment));
        Assert.Throws<ArgumentNullException>(() => _hotel.CreateBooking(_guest.GuestId, "101", Start, Start.AddDays(1), null!));
        Assert.Equal(0, payment.Calls);
        Assert.Empty(_hotel.BookingHistory);
    }

    [Fact]
    public void DuplicateRoomAndGuestEmailAreRejectedCaseInsensitively()
    {
        Assert.Throws<InvalidOperationException>(() => _hotel.RegisterRoom(new SingleRoom("101", false)));
        Assert.Throws<InvalidOperationException>(() => _hotel.RegisterGuest(new RegularGuest("Other", "GUEST@example.com")));
        Assert.False(typeof(Room).GetProperty("RoomNumber")!.SetMethod!.IsPublic);
        Assert.False(typeof(Guest).GetProperty("Email")!.SetMethod!.IsPublic);
    }

    [Theory]
    [InlineData("", "demo@example.com")]
    [InlineData("Demo", "invalid")]
    public void InvalidGuestDataIsRejected(string name, string email) =>
        Assert.Throws<ArgumentException>(() => new RegularGuest(name, email));

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void NonPositiveRoomPriceIsRejected(int price) =>
        Assert.Throws<ArgumentException>(() => new SingleRoom("102", true, price));

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void DemoPaymentsNeedNoSensitiveInputAndSupportDecline(bool vipps)
    {
        IPayable success = vipps ? new VippsPayment() : new CardPayment();
        IPayable decline = vipps ? new VippsPayment(false) : new CardPayment(false);
        Assert.True(success.ProcessPayment(100));
        Assert.False(decline.ProcessPayment(100));
        Assert.False(success.ProcessPayment(0));
        Assert.Contains("DEMO simulation", success.GetPaymentInfo());
        Assert.DoesNotMatch(@"\d{4,}", success.GetPaymentInfo());
    }

    private sealed class RecordingPayment : IPayable
    {
        public bool Success { get; init; } = true;
        public bool Throw { get; init; }
        public int Calls { get; private set; }
        public decimal Amount { get; private set; }
        public bool ProcessPayment(decimal amount)
        {
            Calls++;
            Amount = amount;
            if (Throw) throw new Exception("private provider detail");
            return Success;
        }
        public string GetPaymentInfo() => "Test simulation";
    }
}
