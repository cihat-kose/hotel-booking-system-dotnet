namespace HotelBookingSystem.Models;

public class DoubleRoom : Room
{
    public bool HasExtraBed { get; set; }

    public DoubleRoom(string roomNumber, bool hasExtraBed, decimal pricePerNight = 1200m)
        : base(roomNumber, "DoubleRoom", pricePerNight)
    {
        HasExtraBed = hasExtraBed;
    }

    public override string DisplayRoomInfo()
    {
        return $"Room {RoomNumber} - {RoomType}, Price per night: {PricePerNight}, Unoccupied now: {IsAvailable}, Extra bed: {HasExtraBed}";
    }
}
