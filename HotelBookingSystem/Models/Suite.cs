namespace HotelBookingSystem.Models;

public class Suite : Room
{
    public bool HasJacuzzi { get; set; }
    public bool HasLounge { get; set; }

    public Suite(string roomNumber, bool hasJacuzzi, bool hasLounge, decimal pricePerNight = 3500m)
        : base(roomNumber, "Suite", pricePerNight)
    {
        HasJacuzzi = hasJacuzzi;
        HasLounge = hasLounge;
    }

    public override string DisplayRoomInfo()
    {
        return $"Room {RoomNumber} - {RoomType}, Price per night: {PricePerNight}, Unoccupied now: {IsAvailable}, Jacuzzi: {HasJacuzzi}, Lounge: {HasLounge}";
    }
}
