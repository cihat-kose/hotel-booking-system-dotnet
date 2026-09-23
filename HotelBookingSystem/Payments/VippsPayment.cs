namespace HotelBookingSystem.Payments;

/// <summary>Demo only: no credentials, network calls, or money movement.</summary>
public sealed class VippsPayment(bool simulateSuccess = true) : IPayable
{
    public bool ProcessPayment(decimal amount) => amount > 0 && simulateSuccess;
    public string GetPaymentInformation() => "Vipps payment simulation (no real payment)";
}
