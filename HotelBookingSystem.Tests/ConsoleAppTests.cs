using HotellBookingSystem;

namespace HotelBookingSystem.Tests;

public class ConsoleAppTests
{
    [Theory]
    [InlineData("\n0\n", "Invalid choice")]
    [InlineData("wrong\n0\n", "Invalid choice")]
    [InlineData("1\n31.02.2030\n0\n", "Invalid date format")]
    [InlineData("1\n\n0\n", "is required")]
    [InlineData("1\n12.06.2030\n10.06.2030\n0\n", "earlier than")]
    [InlineData("2\n\n0\n", "GuestId is required")]
    [InlineData("2\nG001\n101\n10.06.2030\n12.06.2030\n9\n0\n", "Invalid payment option")]
    [InlineData("3\nmissing\n0\n", "Booking not found")]
    [InlineData("4\n\n0\n", "BookingId is required")]
    [InlineData("6\n9\n0\n", "Invalid guest type")]
    [InlineData("6\n1\nDemo\ninvalid\n0\n", "Email must contain")]
    public void InvalidInputReturnsToMenu(string input, string message)
    {
        var output = new StringWriter();
        new ConsoleApp(new StringReader(input), output).Run();
        var text = output.ToString();
        Assert.Contains(message, text);
        Assert.Contains("Choose an option:", text[(text.IndexOf(message, StringComparison.Ordinal) + message.Length)..]);
    }

    [Theory]
    [InlineData("")]
    [InlineData("1\n")]
    [InlineData("2\n")]
    [InlineData("6\n1\n")]
    public void EndOfInputExitsNormally(string input)
    {
        var output = new StringWriter();
        new ConsoleApp(new StringReader(input), output).Run();
        Assert.Contains("Sample rooms and guests are ready", output.ToString());
    }

    [Fact]
    public void StartupShowsSampleIdentifiersAndNoTestsOrPaymentDataPrompts()
    {
        var output = new StringWriter();
        new ConsoleApp(new StringReader("0\n"), output).Run();
        var text = output.ToString();
        Assert.Contains("Room 101", text);
        Assert.Contains("GuestId: G", text);
        Assert.Contains("dd.MM.yyyy", text);
        Assert.Contains("Payments are simulations only", text);
        Assert.DoesNotContain("PASSED", text);
        Assert.DoesNotContain("CardNumber", text);
        Assert.DoesNotContain("PhoneNumber", text);
    }
}
