using HotelBookingSystem.Payments;

namespace HotelBookingSystem.Models;

/// <summary>
/// Main class that manages rooms, guests, and bookings.
/// </summary>
public class Hotel
{
    private string _name = string.Empty;
    private readonly List<Room> _rooms = new();
    private readonly List<Guest> _guests = new();
    private readonly List<Booking> _bookingHistory = new();

    /// <summary>
    /// Hotel name.
    /// </summary>
    public string Name
    {
        get => _name;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("Hotel name cannot be null or empty.", nameof(value));
            }

            _name = value;
        }
    }

    /// <summary>
    /// All rooms registered in the hotel.
    /// </summary>
    public IReadOnlyList<Room> Rooms => _rooms.AsReadOnly();

    /// <summary>
    /// All registered guests.
    /// </summary>
    public IReadOnlyList<Guest> Guests => _guests.AsReadOnly();

    /// <summary>
    /// Log of all bookings in the system.
    /// </summary>
    public IReadOnlyList<Booking> BookingHistory => _bookingHistory.AsReadOnly();

    public Hotel(string name)
    {
        Name = name;
    }

    /// <summary>
    /// Adds a room if the room number is unique.
    /// </summary>
    public void RegisterRoom(Room room)
    {
        if (room is null)
        {
            throw new ArgumentNullException(nameof(room));
        }

        var duplicateRoomExists = _rooms.Any(existingRoom =>
            string.Equals(existingRoom.RoomNumber, room.RoomNumber, StringComparison.OrdinalIgnoreCase));

        if (duplicateRoomExists)
        {
            throw new InvalidOperationException("A room with the same room number already exists.");
        }

        _rooms.Add(room);
    }

    /// <summary>
    /// Registers a new guest if the email is unique.
    /// </summary>
    public void RegisterGuest(Guest guest)
    {
        if (guest is null)
        {
            throw new ArgumentNullException(nameof(guest));
        }

        var duplicateEmailExists = _guests.Any(existingGuest =>
            string.Equals(existingGuest.Email, guest.Email, StringComparison.OrdinalIgnoreCase));

        if (duplicateEmailExists)
        {
            throw new InvalidOperationException("A guest with the same email address is already registered.");
        }

        _guests.Add(guest);
    }

    /// <summary>
    /// Returns rooms that are free for the requested date range.
    /// </summary>
    public List<Room> GetAvailableRooms(DateTime checkIn, DateTime checkOut)
    {
        Booking.ValidateDates(checkIn, checkOut);

        return _rooms
            .Where(room => !_bookingHistory.Any(booking =>
                booking.Room.RoomNumber == room.RoomNumber &&
                booking.IsActive &&
                DatesOverlap(checkIn, checkOut, booking.CheckInDate, booking.CheckOutDate)))
            .ToList();
    }

    /// <summary>
    /// Creates and pays for a booking without checking the guest in.
    /// </summary>
    public Booking CreateBooking(string guestId, string roomNumber, DateTime checkIn, DateTime checkOut, IPayable payment)
    {
        if (string.IsNullOrWhiteSpace(guestId))
        {
            throw new ArgumentException("Guest ID cannot be null or empty.", nameof(guestId));
        }

        if (string.IsNullOrWhiteSpace(roomNumber))
        {
            throw new ArgumentException("Room number cannot be null or empty.", nameof(roomNumber));
        }

        Booking.ValidateDates(checkIn, checkOut);

        if (payment is null)
        {
            throw new ArgumentNullException(nameof(payment));
        }

        var guest = _guests.FirstOrDefault(g => g.GuestId == guestId);
        if (guest is null)
        {
            throw new InvalidOperationException("Guest not found.");
        }

        var room = _rooms.FirstOrDefault(r => r.RoomNumber == roomNumber);
        if (room is null)
        {
            throw new InvalidOperationException("Room not found.");
        }

        var roomIsAvailable = !_bookingHistory.Any(existingBooking =>
            existingBooking.Room.RoomNumber == room.RoomNumber &&
            existingBooking.IsActive &&
            DatesOverlap(checkIn, checkOut, existingBooking.CheckInDate, existingBooking.CheckOutDate));

        if (!roomIsAvailable)
        {
            throw new InvalidOperationException("Room is not available in the selected period.");
        }

        guest.EnsureBookingCapacity();
        var booking = new Booking(room, guest, checkIn, checkOut, payment);
        bool paymentSuccessful;
        try
        {
            paymentSuccessful = booking.ProcessPayment();
        }
        catch (Exception)
        {
            // A payment provider's exception may contain private data. Do not expose it.
            throw new InvalidOperationException("Payment simulation failed. No booking was created.");
        }
        if (!paymentSuccessful)
            throw new InvalidOperationException("Payment simulation declined. No booking was created.");

        guest.AddBooking(booking);
        if (guest is VipGuest vipGuest)
            vipGuest.AddBookingLoyaltyPoints();
        _bookingHistory.Add(booking);
        return booking;
    }

    /// <summary>
    /// Checks in a booked reservation.
    /// </summary>
    public void CheckInBooking(string bookingId)
    {
        if (string.IsNullOrWhiteSpace(bookingId))
        {
            throw new ArgumentException("Booking ID cannot be null or empty.", nameof(bookingId));
        }

        var booking = _bookingHistory.FirstOrDefault(b => b.BookingId == bookingId);
        if (booking is null)
        {
            throw new InvalidOperationException("Booking not found.");
        }

        booking.CheckIn();
    }

    /// <summary>
    /// Checks out a checked-in reservation.
    /// </summary>
    public void CheckOutBooking(string bookingId)
    {
        if (string.IsNullOrWhiteSpace(bookingId))
        {
            throw new ArgumentException("Booking ID cannot be null or empty.", nameof(bookingId));
        }

        var booking = _bookingHistory.FirstOrDefault(b => b.BookingId == bookingId);
        if (booking is null)
        {
            throw new InvalidOperationException("Booking not found.");
        }

        booking.CheckOut();
    }

    /// <summary>
    /// Cancels a booking and keeps it in the history log.
    /// </summary>
    public void CancelBooking(string bookingId)
    {
        if (string.IsNullOrWhiteSpace(bookingId))
        {
            throw new ArgumentException("Booking ID cannot be null or empty.", nameof(bookingId));
        }

        var booking = BookingHistory.FirstOrDefault(b => b.BookingId == bookingId);
        if (booking is null)
        {
            throw new InvalidOperationException("Booking not found.");
        }

        booking.Cancel();
    }

    /// <summary>
    /// Returns active bookings for a guest.
    /// </summary>
    public List<Booking> GetGuestBookings(string guestId)
    {
        if (string.IsNullOrWhiteSpace(guestId))
        {
            throw new ArgumentException("Guest ID cannot be null or empty.", nameof(guestId));
        }

        var guest = _guests.FirstOrDefault(g => g.GuestId == guestId);
        if (guest is null)
        {
            throw new InvalidOperationException("Guest not found.");
        }

        return guest.ActiveBookings.ToList();
    }

    private static bool DatesOverlap(DateTime requestedCheckIn, DateTime requestedCheckOut, DateTime existingCheckIn, DateTime existingCheckOut)
    {
        return requestedCheckIn < existingCheckOut && existingCheckIn < requestedCheckOut;
    }
}
