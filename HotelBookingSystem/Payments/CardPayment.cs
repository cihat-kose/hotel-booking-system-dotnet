namespace HotellBookingSystem.Payments;

/// <summary>Demo only: no credentials, network calls, or money movement.</summary>
public sealed class CardPayment(bool simulateSuccess = true) : IPayable
{
    public bool ProcessPayment(decimal amount) => amount > 0 && simulateSuccess;
    public string GetPaymentInfo() => "Card DEMO simulation (no real payment)";
}
