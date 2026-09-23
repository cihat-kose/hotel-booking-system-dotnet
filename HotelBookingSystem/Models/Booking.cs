using HotelBookingSystem.Payments;

namespace HotelBookingSystem.Models;

public enum BookingStatus { Booked, CheckedIn, CheckedOut, Cancelled }

/// <summary>A paid reservation created only through Hotel.CreateBooking.</summary>
public class Booking
{
    private static int _bookingCounter;
    private readonly decimal _totalPrice;

    public string BookingId { get; }
    public Room Room { get; }
    public Guest Guest { get; }
    public DateTime CheckInDate { get; }
    public DateTime CheckOutDate { get; }
    public IPayable PaymentMethod { get; }
    public bool IsPaid { get; private set; }
    public BookingStatus Status { get; private set; } = BookingStatus.Booked;
    public bool IsActive => Status is BookingStatus.Booked or BookingStatus.CheckedIn;

    internal Booking(Room room, Guest guest, DateTime checkInDate, DateTime checkOutDate, IPayable paymentMethod)
    {
        ArgumentNullException.ThrowIfNull(room);
        ArgumentNullException.ThrowIfNull(guest);
        ArgumentNullException.ThrowIfNull(paymentMethod);
        ValidateDates(checkInDate, checkOutDate);
        Room = room;
        Guest = guest;
        CheckInDate = checkInDate;
        CheckOutDate = checkOutDate;
        PaymentMethod = paymentMethod;
        _totalPrice = guest.GetDiscount((checkOutDate - checkInDate).Days * room.PricePerNight);
        BookingId = $"BK{Interlocked.Increment(ref _bookingCounter):000}";
    }

    internal static void ValidateDates(DateTime checkIn, DateTime checkOut)
    {
        if (checkIn.TimeOfDay != TimeSpan.Zero || checkOut.TimeOfDay != TimeSpan.Zero)
            throw new ArgumentException("Use whole dates without a time of day.");
        if (checkIn >= checkOut)
            throw new ArgumentException("Check-in date must be earlier than check-out date.");
    }

    /// <summary>The price agreed at creation, unaffected by later room price changes.</summary>
    public decimal CalculateTotalPrice() => _totalPrice;

    public void CheckIn()
    {
        if (Status != BookingStatus.Booked || !IsPaid)
            throw new InvalidOperationException("Only paid, booked reservations can be checked in.");
        Room.CheckIn();
        Status = BookingStatus.CheckedIn;
    }

    public void CheckOut()
    {
        if (Status != BookingStatus.CheckedIn)
            throw new InvalidOperationException("Only checked-in bookings can be checked out.");
        Room.CheckOut();
        Guest.RemoveBooking(this);
        Status = BookingStatus.CheckedOut;
    }

    /// <summary>Closes a reservation, including an active stay. No refund is simulated.</summary>
    public void Cancel()
    {
        if (!IsActive)
            throw new InvalidOperationException("The booking is already completed.");
        if (Status == BookingStatus.CheckedIn)
            Room.CheckOut();
        Guest.RemoveBooking(this);
        Status = BookingStatus.Cancelled;
    }

    internal bool ProcessPayment()
    {
        if (IsPaid) return true;
        IsPaid = PaymentMethod.ProcessPayment(_totalPrice);
        return IsPaid;
    }
}
