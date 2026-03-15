namespace HotellBookingSystem.Payments;

public class CardPayment : IPayable
{
    private string _cardNumber = string.Empty;
    private string _cardType = string.Empty;

    public string CardNumber
    {
        get => _cardNumber;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("Card number cannot be null or empty.", nameof(value));
            }

            _cardNumber = value;
        }
    }

    public string CardType
    {
        get => _cardType;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("Card type cannot be null or empty.", nameof(value));
            }

            _cardType = value;
        }
    }

    public CardPayment(string cardNumber, string cardType)
    {
        CardNumber = cardNumber;
        CardType = cardType;
    }

    public bool ProcessPayment(decimal amount)
    {
        return true;
    }

    public string GetPaymentInfo()
    {
        var lastFourDigits = CardNumber.Length >= 4 ? CardNumber[^4..] : CardNumber;
        return $"{CardType} card ending in {lastFourDigits}";
    }
}
