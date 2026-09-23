namespace HotellBookingSystem.Models;

public class SingleRoom : Room
{
    public bool HasDesk { get; set; }

    public SingleRoom(string roomNumber, bool hasDesk, decimal pricePerNight = 800m)
        : base(roomNumber, "SingleRoom", pricePerNight, 1)
    {
        HasDesk = hasDesk;
    }

    public override string DisplayRoomInfo()
    {
        return $"Room {RoomNumber} - {RoomType}, Price: {PricePerNight}, Unoccupied now: {IsAvailable}, Max guests: {MaxGuests}, Desk: {HasDesk}";
    }
}
