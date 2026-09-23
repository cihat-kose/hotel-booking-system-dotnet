namespace HotelBookingSystem.Models;

public class RegularGuest : Guest
{
    protected override int MaxActiveBookings => 3;

    public RegularGuest(string name, string email)
        : base(name, email)
    {
    }

    public override decimal GetDiscount(decimal basePrice)
    {
        return basePrice;
    }
}
