namespace HotelBookingSystem.Models;

public class SingleRoom : Room
{
    public bool HasDesk { get; set; }

    public SingleRoom(string roomNumber, bool hasDesk, decimal pricePerNight = 800m)
        : base(roomNumber, "SingleRoom", pricePerNight)
    {
        HasDesk = hasDesk;
    }

    public override string DisplayRoomInfo()
    {
        return $"Room {RoomNumber} - {RoomType}, Price per night: {PricePerNight}, Unoccupied now: {IsAvailable}, Desk: {HasDesk}";
    }
}
