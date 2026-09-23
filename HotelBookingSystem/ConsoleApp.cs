using System.Globalization;
using HotelBookingSystem.Models;
using HotelBookingSystem.Payments;

namespace HotelBookingSystem;

/// <summary>The console adapter. Text streams also allow repeatable input/output tests.</summary>
public sealed class ConsoleApp(TextReader input, TextWriter output)
{
    private readonly Hotel _hotel = CreateDemoHotel();

    private static Hotel CreateDemoHotel()
    {
        var hotel = new Hotel("Hotel Gokstad");
        hotel.RegisterRoom(new SingleRoom("101", hasDesk: true));
        hotel.RegisterRoom(new DoubleRoom("201", hasExtraBed: true));
        hotel.RegisterRoom(new Suite("301", hasJacuzzi: true, hasLounge: true));
        hotel.RegisterGuest(new RegularGuest("Ola Nordmann (demo)", "ola@example.com"));
        hotel.RegisterGuest(new VipGuest("Kari Hansen (demo)", "kari@example.com"));
        return hotel;
    }

    public void Run()
    {
        output.WriteLine("Hotel Gokstad - hotel booking demonstration");
        output.WriteLine("Explore rooms, create bookings, and simulate check-in/check-out.");
        output.WriteLine("Sample rooms and guests are ready. All data resets on exit.");
        output.WriteLine("Payments are simulations only. Never enter real payment details.");
        output.WriteLine("Dates: dd.MM.yyyy. Start with 1. Show available rooms, then 2. Create booking.");
        ShowSampleData();
        while (true)
        {
            output.WriteLine();
            output.WriteLine("1. Show available rooms");
            output.WriteLine("2. Create booking");
            output.WriteLine("3. Check in");
            output.WriteLine("4. Check out");
            output.WriteLine("5. Show my bookings");
            output.WriteLine("6. Register new guest");
            output.WriteLine("7. Cancel booking");
            output.WriteLine("8. Show rooms and guests");
            output.WriteLine("0. Exit");
            output.Write("Choose an option: ");
            var choice = input.ReadLine()?.Trim();
            if (choice is null or "0") return;
            try
            {
                switch (choice)
                {
                    case "1": ShowAvailableRooms(); break;
                    case "2": CreateBooking(); break;
                    case "3":
                        _hotel.CheckInBooking(ReadRequired("Booking ID"));
                        output.WriteLine("Check-in completed successfully.");
                        break;
                    case "4":
                        _hotel.CheckOutBooking(ReadRequired("Booking ID"));
                        output.WriteLine("Check-out completed successfully.");
                        break;
                    case "5": ShowGuestBookings(); break;
                    case "6": RegisterGuest(); break;
                    case "7":
                        _hotel.CancelBooking(ReadRequired("Booking ID"));
                        output.WriteLine("Booking cancelled. No real payment or refund took place.");
                        break;
                    case "8": ShowSampleData(); break;
                    default: output.WriteLine("Invalid choice. Choose a menu number from 0 to 8."); break;
                }
            }
            catch (EndOfStreamException) { return; }
            catch (ArgumentException ex) { output.WriteLine(ex.Message); }
            catch (InvalidOperationException ex) { output.WriteLine(ex.Message); }
        }
    }

    private string ReadRequired(string label)
    {
        output.Write($"Enter {label}: ");
        var value = input.ReadLine() ?? throw new EndOfStreamException();
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException($"{label} is required. Returning to menu.");
        return value.Trim();
    }

    private DateTime ReadDate(string label)
    {
        var value = ReadRequired($"{label} date (dd.MM.yyyy)");
        if (!DateTime.TryParseExact(value, "dd.MM.yyyy", CultureInfo.InvariantCulture,
                DateTimeStyles.None, out var date))
            throw new ArgumentException("Invalid date format. Please use dd.MM.yyyy.");
        return date;
    }

    private void ShowSampleData()
    {
        output.WriteLine("Rooms (current occupancy, not date availability):");
        foreach (var room in _hotel.Rooms) output.WriteLine(room.DisplayRoomInfo());
        output.WriteLine("Registered guests:");
        foreach (var guest in _hotel.Guests)
            output.WriteLine($"Guest ID: {guest.GuestId}, Name: {guest.Name}, Type: {guest.GetType().Name}");
    }

    private void ShowAvailableRooms()
    {
        var rooms = _hotel.GetAvailableRooms(ReadDate("check-in"), ReadDate("check-out"));
        output.WriteLine("Available for the selected dates:");
        if (rooms.Count == 0) output.WriteLine("No available rooms found.");
        foreach (var room in rooms) output.WriteLine(room.DisplayRoomInfo());
    }

    private void CreateBooking()
    {
        ShowSampleData();
        var guestId = ReadRequired("Guest ID");
        var roomNumber = ReadRequired("Room number");
        var checkIn = ReadDate("check-in");
        var checkOut = ReadDate("check-out");
        output.WriteLine("Choose payment simulation (no real money or payment details):");
        output.WriteLine("1 = Card simulation approved");
        output.WriteLine("2 = Vipps simulation approved");
        output.WriteLine("3 = Card simulation declined");
        IPayable payment = ReadRequired("payment option") switch
        {
            "1" => new CardPayment(),
            "2" => new VippsPayment(),
            "3" => new CardPayment(simulateSuccess: false),
            _ => throw new ArgumentException("Invalid payment option. Choose 1, 2 or 3.")
        };
        var booking = _hotel.CreateBooking(guestId, roomNumber, checkIn, checkOut, payment);
        output.WriteLine("Booking created successfully:");
        ShowBooking(booking);
        output.WriteLine(booking.PaymentMethod.GetPaymentInformation());
    }

    private void ShowBooking(Booking booking) => output.WriteLine(
        $"Booking ID: {booking.BookingId}, Room number: {booking.Room.RoomNumber}, " +
        $"Check-in: {booking.CheckInDate:dd.MM.yyyy}, Check-out: {booking.CheckOutDate:dd.MM.yyyy}, " +
        $"Total price: {booking.CalculateTotalPrice().ToString("0.00", CultureInfo.InvariantCulture)} NOK, " +
        $"Paid (simulation): {booking.IsPaid}, Status: {booking.Status}");

    private void ShowGuestBookings()
    {
        var bookings = _hotel.GetGuestBookings(ReadRequired("Guest ID"));
        if (bookings.Count == 0) output.WriteLine("You have no active bookings.");
        foreach (var booking in bookings) ShowBooking(booking);
    }

    private void RegisterGuest()
    {
        output.WriteLine("Guest type: 1 = Regular guest, 2 = VIP guest. Use fictional demo data.");
        var type = ReadRequired("guest type");
        if (type is not ("1" or "2")) throw new ArgumentException("Invalid guest type. Choose 1 or 2.");
        var name = ReadRequired("name");
        var email = ReadRequired("email");
        Guest guest = type == "1" ? new RegularGuest(name, email) : new VipGuest(name, email);
        _hotel.RegisterGuest(guest);
        output.WriteLine($"Guest registered successfully. Guest ID: {guest.GuestId}");
    }
}
