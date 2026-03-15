using HotellBookingSystem.Payments;

namespace HotellBookingSystem.Models;

/// <summary>
/// Simple lifecycle states for a booking.
/// </summary>
public enum BookingStatus
{
    Booked,
    CheckedIn,
    CheckedOut,
    Cancelled
}

/// <summary>
/// Represents one hotel booking.
/// </summary>
public class Booking
{
    private static int _bookingCounter;
    private Room _room = null!;
    private Guest _guest = null!;
    private DateTime _checkInDate;
    private DateTime _checkOutDate;
    private IPayable _paymentMethod = null!;

    public string BookingId { get; }

    /// <summary>
    /// The room connected to the booking.
    /// </summary>
    public Room Room
    {
        get => _room;
        set => _room = value ?? throw new ArgumentNullException(nameof(value));
    }

    /// <summary>
    /// The guest who owns the booking.
    /// </summary>
    public Guest Guest
    {
        get => _guest;
        set => _guest = value ?? throw new ArgumentNullException(nameof(value));
    }

    /// <summary>
    /// Check-in date for the booking.
    /// </summary>
    public DateTime CheckInDate
    {
        get => _checkInDate;
        set
        {
            if (_checkOutDate != default && value >= _checkOutDate)
            {
                throw new ArgumentException("Check-in date must be earlier than check-out date.", nameof(value));
            }

            _checkInDate = value;
        }
    }

    /// <summary>
    /// Check-out date for the booking.
    /// </summary>
    public DateTime CheckOutDate
    {
        get => _checkOutDate;
        set
        {
            if (_checkInDate != default && _checkInDate >= value)
            {
                throw new ArgumentException("Check-in date must be earlier than check-out date.", nameof(value));
            }

            _checkOutDate = value;
        }
    }

    /// <summary>
    /// Payment method used for this booking.
    /// </summary>
    public IPayable PaymentMethod
    {
        get => _paymentMethod;
        set => _paymentMethod = value ?? throw new ArgumentNullException(nameof(value));
    }

    /// <summary>
    /// Indicates whether the booking has been paid.
    /// </summary>
    public bool IsPaid { get; private set; }

    /// <summary>
    /// Current booking status.
    /// </summary>
    public BookingStatus Status { get; private set; } = BookingStatus.Booked;

    /// <summary>
    /// True while the booking still blocks the room period.
    /// </summary>
    public bool IsActive => Status is BookingStatus.Booked or BookingStatus.CheckedIn;

    public Booking(Room room, Guest guest, DateTime checkInDate, DateTime checkOutDate, IPayable paymentMethod)
    {
        _bookingCounter++;
        BookingId = $"BK{_bookingCounter:000}";

        Room = room;
        Guest = guest;
        CheckInDate = checkInDate;
        CheckOutDate = checkOutDate;
        PaymentMethod = paymentMethod;

        Guest.AddBooking(this);
    }

    /// <summary>
    /// Calculates the total price for the stay.
    /// </summary>
    public decimal CalculateTotalPrice()
    {
        var numberOfNights = (CheckOutDate - CheckInDate).Days;
        var basePrice = numberOfNights * Room.PricePerNight;
        return Guest.GetDiscount(basePrice);
    }

    /// <summary>
    /// Checks the guest into the booking.
    /// </summary>
    public void CheckIn()
    {
        if (Status != BookingStatus.Booked)
        {
            throw new InvalidOperationException("Only booked reservations can be checked in.");
        }

        Room.CheckIn();
        Status = BookingStatus.CheckedIn;
    }

    /// <summary>
    /// Checks the guest out and closes the booking.
    /// </summary>
    public void CheckOut()
    {
        if (Status != BookingStatus.CheckedIn)
        {
            throw new InvalidOperationException("Only checked-in bookings can be checked out.");
        }

        Room.CheckOut();
        Guest.RemoveBooking(this);
        Status = BookingStatus.CheckedOut;
    }

    /// <summary>
    /// Cancels the booking before completion.
    /// </summary>
    public void Cancel()
    {
        if (Status is BookingStatus.CheckedOut or BookingStatus.Cancelled)
        {
            throw new InvalidOperationException("The booking is already completed.");
        }

        if (Status == BookingStatus.CheckedIn)
        {
            Room.CheckOut();
        }

        Guest.RemoveBooking(this);
        Status = BookingStatus.Cancelled;
    }

    /// <summary>
    /// Simulates payment processing for the booking.
    /// </summary>
    public bool ProcessPayment()
    {
        var totalPrice = CalculateTotalPrice();
        var paymentSuccessful = PaymentMethod.ProcessPayment(totalPrice);

        if (paymentSuccessful)
        {
            IsPaid = true;

            if (Guest is VipGuest vipGuest)
            {
                vipGuest.AddBookingLoyaltyPoints();
            }
        }

        return paymentSuccessful;
    }
}
