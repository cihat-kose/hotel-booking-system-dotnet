namespace HotelBookingSystem.Payments;

public interface IPayable
{
    bool ProcessPayment(decimal amount);
    string GetPaymentInformation();
}
