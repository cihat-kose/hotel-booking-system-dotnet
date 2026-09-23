namespace HotellBookingSystem.Models;

/// <summary>
/// Base class for all room types in the hotel.
/// </summary>
public abstract class Room
{
    private string _roomNumber = string.Empty;
    private decimal _pricePerNight;

    /// <summary>
    /// Unique room number for the room.
    /// </summary>
    public string RoomNumber
    {
        get => _roomNumber;
        private set
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("Room number cannot be empty.", nameof(value));
            }

            _roomNumber = value;
        }
    }

    public string RoomType { get; protected set; }

    /// <summary>
    /// Price per night in NOK.
    /// </summary>
    public decimal PricePerNight
    {
        get => _pricePerNight;
        set
        {
            if (value <= 0)
            {
                throw new ArgumentException("Price per night must be positive.", nameof(value));
            }

            _pricePerNight = value;
        }
    }

    /// <summary>
    /// Indicates whether the room is currently occupied.
    /// </summary>
    public bool IsAvailable { get; private set; } = true;

    public int MaxGuests { get; protected set; }

    protected Room(string roomNumber, string roomType, decimal pricePerNight, int maxGuests)
    {
        RoomNumber = roomNumber;
        RoomType = roomType;
        PricePerNight = pricePerNight;
        MaxGuests = maxGuests;
    }

    /// <summary>
    /// Marks the room as occupied.
    /// </summary>
    internal void CheckIn()
    {
        if (!IsAvailable)
            throw new InvalidOperationException("Room is currently occupied. Check out the current stay first.");
        IsAvailable = false;
    }

    /// <summary>
    /// Marks the room as available again.
    /// </summary>
    internal void CheckOut()
    {
        IsAvailable = true;
    }

    /// <summary>
    /// Returns room-specific display information.
    /// </summary>
    public abstract string DisplayRoomInfo();
}
