namespace HotellBookingSystem.Payments;

public class VippsPayment : IPayable
{
    private string _phoneNumber = string.Empty;

    public string PhoneNumber
    {
        get => _phoneNumber;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("Phone number cannot be null or empty.", nameof(value));
            }

            _phoneNumber = value;
        }
    }

    public VippsPayment(string phoneNumber)
    {
        PhoneNumber = phoneNumber;
    }

    public bool ProcessPayment(decimal amount)
    {
        return true;
    }

    public string GetPaymentInfo()
    {
        return $"Vipps payment registered for phone number {PhoneNumber}";
    }
}
