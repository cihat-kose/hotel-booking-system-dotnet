namespace HotellBookingSystem.Models;

public class Suite : Room
{
    public bool HasJacuzzi { get; set; }
    public bool HasLounge { get; set; }

    public Suite(string roomNumber, bool hasJacuzzi, bool hasLounge, decimal pricePerNight = 3500m)
        : base(roomNumber, "Suite", pricePerNight, 4)
    {
        HasJacuzzi = hasJacuzzi;
        HasLounge = hasLounge;
    }

    public override string DisplayRoomInfo()
    {
        return $"Room {RoomNumber} - {RoomType}, Price: {PricePerNight}, Unoccupied now: {IsAvailable}, Max guests: {MaxGuests}, Jacuzzi: {HasJacuzzi}, Lounge: {HasLounge}";
    }
}
