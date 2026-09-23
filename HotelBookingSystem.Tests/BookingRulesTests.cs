using HotelBookingSystem.Models;
using HotelBookingSystem.Payments;

namespace HotelBookingSystem.Tests;

public class BookingRulesTests
{
    private static readonly DateTime _startDate = new(2030, 6, 10);
    private readonly Hotel _hotel = new("Test Hotel");
    private readonly Room _room = new SingleRoom("101", true);
    private readonly Guest _guest;

    public BookingRulesTests()
    {
        _guest = new RegularGuest("Demo guest", "guest@example.com");
        _hotel.RegisterRoom(_room);
        _hotel.RegisterGuest(_guest);
    }

    private Booking CreateTestBooking(int start = 0, int end = 3, IPayable? payment = null, Guest? guest = null) =>
        _hotel.CreateBooking((guest ?? _guest).GuestId, _room.RoomNumber,
            _startDate.AddDays(start), _startDate.AddDays(end), payment ?? new CardPayment());

    [Fact] // Original SimpleTests scenario 1.
    public void GetDiscount_ForVipGuest_ReturnsFifteenPercentDiscount() =>
        Assert.Equal(850m, new VipGuest("Demo", "vip@example.com").GetDiscount(1000m));

    [Fact] // Original SimpleTests scenario 2.
    public void CreateBooking_ForThreeNightSingleRoom_ReturnsExpectedTotalPrice() => Assert.Equal(2400m, CreateTestBooking().CalculateTotalPrice());

    [Theory] // Original scenarios 3 and 5, including containment and exact equality.
    [InlineData(0, 3)]
    [InlineData(1, 2)]
    [InlineData(-1, 4)]
    [InlineData(-1, 1)]
    [InlineData(2, 4)]
    public void CreateBooking_WhenDatesOverlapExistingBooking_ThrowsWithoutCharging(int start, int end)
    {
        CreateTestBooking();
        var payment = new RecordingPayment();
        Assert.Throws<InvalidOperationException>(() => CreateTestBooking(start, end, payment));
        Assert.Empty(_hotel.GetAvailableRooms(_startDate.AddDays(start), _startDate.AddDays(end)));
        Assert.Equal(0, payment.ProcessPaymentCallCount);
        Assert.Single(_hotel.BookingHistory);
    }

    [Theory]
    [InlineData(-3, 0)]
    [InlineData(3, 6)]
    public void CreateBooking_WhenBookingsAreAdjacent_Succeeds(int start, int end)
    {
        CreateTestBooking();
        Assert.Contains(_room, _hotel.GetAvailableRooms(_startDate.AddDays(start), _startDate.AddDays(end)));
        CreateTestBooking(start, end);
        Assert.Equal(2, _hotel.BookingHistory.Count);
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(1, 0)]
    [InlineData(0.5, 2)]
    [InlineData(0, 2.5)]
    public void CreateBooking_WhenDatesAreInvalid_ThrowsBeforePayment(double start, double end)
    {
        var payment = new RecordingPayment();
        Assert.Throws<ArgumentException>(() => _hotel.CreateBooking(_guest.GuestId, "101",
            _startDate.AddDays(start), _startDate.AddDays(end), payment));
        Assert.Throws<ArgumentException>(() => _hotel.GetAvailableRooms(_startDate.AddDays(start), _startDate.AddDays(end)));
        Assert.Equal(0, payment.ProcessPaymentCallCount);
        Assert.Empty(_guest.ActiveBookings);
        Assert.Empty(_hotel.BookingHistory);
    }

    [Fact] // Original SimpleTests scenario 4, through the paid booking path.
    public void CheckoutReleasesOccupancyAndActiveBookingButKeepsHistory()
    {
        var booking = CreateTestBooking();
        Assert.True(_room.IsAvailable);
        _hotel.CheckInBooking(booking.BookingId);
        Assert.False(_room.IsAvailable);
        _hotel.CheckOutBooking(booking.BookingId);
        Assert.True(_room.IsAvailable);
        Assert.Equal(BookingStatus.CheckedOut, booking.Status);
        Assert.Empty(_hotel.GetGuestBookings(_guest.GuestId));
        Assert.Same(booking, Assert.Single(_hotel.BookingHistory));
        Assert.Contains(_room, _hotel.GetAvailableRooms(_startDate, _startDate.AddDays(3)));
    }

    [Fact]
    public void OccupancyAllowsFutureBookingsButPreventsTwoSimultaneousCheckIns()
    {
        var current = CreateTestBooking();
        current.CheckIn();
        Assert.Contains(_room, _hotel.GetAvailableRooms(_startDate.AddDays(3), _startDate.AddDays(6)));
        var future = CreateTestBooking(3, 6);
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
    public void CreateBooking_WhenGuestLimitIsReached_RejectsBeforePaymentAndCancellationRestoresCapacity(bool vip, int limit)
    {
        Guest guest = vip ? new VipGuest("VIP", "vip@example.com") : _guest;
        if (vip) _hotel.RegisterGuest(guest);
        for (var i = 0; i < limit; i++) CreateTestBooking(i, i + 1, guest: guest);
        var payment = new RecordingPayment();
        Assert.Throws<InvalidOperationException>(() => CreateTestBooking(limit, limit + 1, payment, guest));
        Assert.Equal(0, payment.ProcessPaymentCallCount);
        Assert.Equal(limit, guest.ActiveBookings.Count);
        _hotel.CancelBooking(guest.ActiveBookings[0].BookingId);
        CreateTestBooking(limit, limit + 1, payment, guest);
        Assert.Equal(1, payment.ProcessPaymentCallCount);
        Assert.Equal(limit, guest.ActiveBookings.Count);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void CreateBooking_WhenPaymentFails_LeavesNoBookingOccupancyOrLoyaltySideEffects(bool throws)
    {
        var vip = new VipGuest("VIP", "vip@example.com");
        _hotel.RegisterGuest(vip);
        var payment = new RecordingPayment { ShouldSucceed = false, ShouldThrow = throws };
        var error = Assert.Throws<InvalidOperationException>(() => CreateTestBooking(payment: payment, guest: vip));
        Assert.DoesNotContain("private provider detail", error.ToString());
        Assert.Empty(vip.ActiveBookings);
        Assert.Empty(_hotel.BookingHistory);
        Assert.True(_room.IsAvailable);
        Assert.Equal(0, vip.LoyaltyPoints);
        Assert.Contains(_room, _hotel.GetAvailableRooms(_startDate, _startDate.AddDays(3)));
        var retry = CreateTestBooking(guest: vip);
        Assert.True(retry.IsPaid);
        Assert.Equal(10, vip.LoyaltyPoints);
    }

    [Fact]
    public void CreateBooking_ForVipGuest_ChargesDiscountedPriceAndAwardsPointsOnce()
    {
        var vip = new VipGuest("VIP", "vip@example.com");
        _hotel.RegisterGuest(vip);
        var payment = new RecordingPayment();
        var booking = CreateTestBooking(payment: payment, guest: vip);
        Assert.Equal(2040m, payment.LastProcessedAmount);
        Assert.Equal(2040m, booking.CalculateTotalPrice());
        booking.CheckIn();
        booking.CheckOut();
        Assert.Equal(1, payment.ProcessPaymentCallCount);
        Assert.Equal(10, vip.LoyaltyPoints);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Cancel_WhenBookingIsActive_ReleasesBookingAndKeepsPaidHistoryWithoutRefund(bool checkedIn)
    {
        var booking = CreateTestBooking();
        if (checkedIn) booking.CheckIn();
        _hotel.CancelBooking(booking.BookingId);
        Assert.Equal(BookingStatus.Cancelled, booking.Status);
        Assert.True(booking.IsPaid);
        Assert.False(booking.IsActive);
        Assert.True(_room.IsAvailable);
        Assert.Empty(_guest.ActiveBookings);
        Assert.Single(_hotel.BookingHistory);
        CreateTestBooking();
    }

    [Fact]
    public void CancellingFutureBookingDoesNotReleaseCurrentOccupant()
    {
        var current = CreateTestBooking();
        current.CheckIn();
        CreateTestBooking(3, 6).Cancel();
        Assert.False(_room.IsAvailable);
        Assert.Equal(BookingStatus.CheckedIn, current.Status);
    }

    [Fact]
    public void CheckoutBeforeCheckInAndRepeatedCheckInAreRejected()
    {
        var booking = CreateTestBooking();
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
    public void BookingLifecycle_WhenBookingIsCompleted_RejectsFurtherTransitions(bool cancel)
    {
        var booking = CreateTestBooking();
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
    public void CreateBooking_WhenRoomRateChanges_PreservesAgreedBookingPrice()
    {
        var booking = CreateTestBooking();
        _room.PricePerNight = 1500m;
        Assert.Equal(2400m, booking.CalculateTotalPrice());
        Assert.Equal(4500m, CreateTestBooking(3, 6).CalculateTotalPrice());
    }

    [Theory]
    [InlineData("Room")]
    [InlineData("Guest")]
    [InlineData("CheckInDate")]
    [InlineData("CheckOutDate")]
    [InlineData("PaymentMethod")]
    public void BookingIdentityAndTerms_WhenReassigned_AreRejected(string property) =>
        Assert.Null(typeof(Booking).GetProperty(property)!.SetMethod);

    [Fact]
    public void CollectionsCannotBeCastBackToMutableLists()
    {
        var booking = CreateTestBooking();
        Assert.Throws<NotSupportedException>(() => ((IList<Booking>)_guest.ActiveBookings).Clear());
        Assert.Throws<NotSupportedException>(() => ((IList<Booking>)_hotel.BookingHistory).Clear());
        Assert.Throws<NotSupportedException>(() => ((IList<Room>)_hotel.Rooms).Clear());
        Assert.Throws<NotSupportedException>(() => ((IList<Guest>)_hotel.Guests).Clear());
        Assert.Same(booking, Assert.Single(_guest.ActiveBookings));
    }

    [Fact]
    public void CreateBooking_WhenGuestRoomOrPaymentIsInvalid_Throws()
    {
        var payment = new RecordingPayment();
        Assert.Throws<InvalidOperationException>(() => _hotel.CreateBooking("missing", "101", _startDate, _startDate.AddDays(1), payment));
        Assert.Throws<InvalidOperationException>(() => _hotel.CreateBooking(_guest.GuestId, "missing", _startDate, _startDate.AddDays(1), payment));
        Assert.Throws<ArgumentNullException>(() => _hotel.CreateBooking(_guest.GuestId, "101", _startDate, _startDate.AddDays(1), null!));
        Assert.Equal(0, payment.ProcessPaymentCallCount);
        Assert.Empty(_hotel.BookingHistory);
    }

    [Fact]
    public void RegisterEntities_WhenRoomOrGuestEmailDuplicates_RejectsCaseInsensitiveDuplicate()
    {
        Assert.Throws<InvalidOperationException>(() => _hotel.RegisterRoom(new SingleRoom("101", false)));
        Assert.Throws<InvalidOperationException>(() => _hotel.RegisterGuest(new RegularGuest("Other", "GUEST@example.com")));
        Assert.False(typeof(Room).GetProperty("RoomNumber")!.SetMethod!.IsPublic);
        Assert.False(typeof(Guest).GetProperty("Email")!.SetMethod!.IsPublic);
    }

    [Theory]
    [InlineData("", "demo@example.com")]
    [InlineData("Demo", "invalid")]
    public void CreateGuest_WhenNameOrEmailIsInvalid_Throws(string name, string email) =>
        Assert.Throws<ArgumentException>(() => new RegularGuest(name, email));

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void CreateRoom_WhenPriceIsNotPositive_Throws(int price) =>
        Assert.Throws<ArgumentException>(() => new SingleRoom("102", true, price));

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void PaymentSimulation_WhenConfigured_RequiresNoSensitiveInputAndSupportsDecline(bool vipps)
    {
        IPayable success = vipps ? new VippsPayment() : new CardPayment();
        IPayable decline = vipps ? new VippsPayment(false) : new CardPayment(false);
        Assert.True(success.ProcessPayment(100));
        Assert.False(decline.ProcessPayment(100));
        Assert.False(success.ProcessPayment(0));
        Assert.Contains("payment simulation", success.GetPaymentInformation());
        Assert.DoesNotMatch(@"\d{4,}", success.GetPaymentInformation());
    }

    private sealed class RecordingPayment : IPayable
    {
        public bool ShouldSucceed { get; init; } = true;
        public bool ShouldThrow { get; init; }
        public int ProcessPaymentCallCount { get; private set; }
        public decimal LastProcessedAmount { get; private set; }
        public bool ProcessPayment(decimal amount)
        {
            ProcessPaymentCallCount++;
            LastProcessedAmount = amount;
            if (ShouldThrow) throw new Exception("private provider error detail");
            return ShouldSucceed;
        }
        public string GetPaymentInformation() => "Test payment simulation";
    }
}
