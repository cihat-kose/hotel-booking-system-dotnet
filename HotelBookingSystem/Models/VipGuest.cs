namespace HotelBookingSystem.Models;

public class VipGuest : Guest
{
    protected override int MaxActiveBookings => 10;

    public int LoyaltyPoints { get; private set; }

    public VipGuest(string name, string email, int loyaltyPoints = 0)
        : base(name, email)
    {
        LoyaltyPoints = loyaltyPoints;
    }

    public override decimal GetDiscount(decimal basePrice)
    {
        return basePrice * 0.85m;
    }

    internal void AddBookingLoyaltyPoints()
    {
        LoyaltyPoints += 10;
    }
}
