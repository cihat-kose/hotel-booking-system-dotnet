namespace HotellBookingSystem.Models;

public class DoubleRoom : Room
{
    public bool HasExtraBed { get; set; }

    public DoubleRoom(string roomNumber, bool hasExtraBed, decimal pricePerNight = 1200m)
        : base(roomNumber, "DoubleRoom", pricePerNight, 2)
    {
        HasExtraBed = hasExtraBed;
    }

    public override string DisplayRoomInfo()
    {
        return $"Room {RoomNumber} - {RoomType}, Price: {PricePerNight}, Unoccupied now: {IsAvailable}, Max guests: {MaxGuests}, Extra bed: {HasExtraBed}";
    }
}
