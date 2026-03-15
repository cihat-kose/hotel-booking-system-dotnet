namespace HotellBookingSystem.Payments;

public interface IPayable
{
    bool ProcessPayment(decimal amount);
    string GetPaymentInfo();
}
