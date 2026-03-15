using System.Globalization;
using HotellBookingSystem.Models;
using HotellBookingSystem.Payments;
using HotellBookingSystem.Tests;

SimpleTests.RunAll();

var hotel = new Hotel("Hotel Gokstad");

hotel.RegisterRoom(new SingleRoom("101", hasDesk: true));
hotel.RegisterRoom(new DoubleRoom("201", hasExtraBed: true));
hotel.RegisterRoom(new Suite("301", hasJacuzzi: true, hasLounge: true));

hotel.RegisterGuest(new RegularGuest("Ola Nordmann", "ola@example.com"));
hotel.RegisterGuest(new VipGuest("Kari Hansen", "kari@example.com"));

Console.WriteLine("Registered guests:");
foreach (var guest in hotel.Guests)
{
    Console.WriteLine($"GuestId: {guest.GuestId}, Name: {guest.Name}, Email: {guest.Email}");
}

while (true)
{
    Console.WriteLine();
    Console.WriteLine("1. Show available rooms");
    Console.WriteLine("2. Create booking");
    Console.WriteLine("3. Check in");
    Console.WriteLine("4. Check out");
    Console.WriteLine("5. Show my bookings");
    Console.WriteLine("6. Register new guest");
    Console.WriteLine("0. Exit");
    Console.Write("Choose an option: ");

    var choice = Console.ReadLine();

    if (choice == "0")
    {
        break;
    }

    if (choice == "1")
    {
        Console.Write("Enter check-in date (dd.MM.yyyy): ");
        var checkInInput = Console.ReadLine();

        Console.Write("Enter check-out date (dd.MM.yyyy): ");
        var checkOutInput = Console.ReadLine();

        var checkInValid = DateTime.TryParseExact(
            checkInInput,
            "dd.MM.yyyy",
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out var checkInDate);

        var checkOutValid = DateTime.TryParseExact(
            checkOutInput,
            "dd.MM.yyyy",
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out var checkOutDate);

        if (!checkInValid || !checkOutValid)
        {
            Console.WriteLine("Invalid date format. Please use dd.MM.yyyy.");
            continue;
        }

        try
        {
            var availableRooms = hotel.GetAvailableRooms(checkInDate, checkOutDate);

            if (availableRooms.Count == 0)
            {
                Console.WriteLine("No available rooms found.");
                continue;
            }

            foreach (var room in availableRooms)
            {
                Console.WriteLine(room.DisplayRoomInfo());
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }

        continue;
    }

    if (choice == "2")
    {
        Console.Write("Enter GuestId: ");
        var guestId = Console.ReadLine();

        Console.Write("Enter RoomNumber: ");
        var roomNumber = Console.ReadLine();

        Console.Write("Enter check-in date (dd.MM.yyyy): ");
        var checkInInput = Console.ReadLine();

        Console.Write("Enter check-out date (dd.MM.yyyy): ");
        var checkOutInput = Console.ReadLine();

        var checkInValid = DateTime.TryParseExact(
            checkInInput,
            "dd.MM.yyyy",
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out var checkInDate);

        var checkOutValid = DateTime.TryParseExact(
            checkOutInput,
            "dd.MM.yyyy",
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out var checkOutDate);

        if (!checkInValid || !checkOutValid)
        {
            Console.WriteLine("Invalid date format. Please use dd.MM.yyyy.");
            continue;
        }

        Console.WriteLine("Choose payment method:");
        Console.WriteLine("1 = Card");
        Console.WriteLine("2 = Vipps");
        Console.Write("Payment option: ");
        var paymentChoice = Console.ReadLine();

        IPayable? payment = null;

        if (paymentChoice == "1")
        {
            Console.Write("Enter CardNumber: ");
            var cardNumber = Console.ReadLine();

            Console.Write("Enter CardType: ");
            var cardType = Console.ReadLine();

            payment = new CardPayment(cardNumber ?? string.Empty, cardType ?? string.Empty);
        }
        else if (paymentChoice == "2")
        {
            Console.Write("Enter PhoneNumber: ");
            var phoneNumber = Console.ReadLine();

            payment = new VippsPayment(phoneNumber ?? string.Empty);
        }
        else
        {
            Console.WriteLine("Invalid payment option.");
            continue;
        }

        try
        {
            var booking = hotel.CreateBooking(
                guestId ?? string.Empty,
                roomNumber ?? string.Empty,
                checkInDate,
                checkOutDate,
                payment);

            Console.WriteLine("Booking created successfully:");
            Console.WriteLine($"Booking ID: {booking.BookingId}");
            Console.WriteLine($"Guest Name: {booking.Guest.Name}");
            Console.WriteLine($"Room Number: {booking.Room.RoomNumber}");
            Console.WriteLine($"Check-in: {booking.CheckInDate:dd.MM.yyyy}");
            Console.WriteLine($"Check-out: {booking.CheckOutDate:dd.MM.yyyy}");
            Console.WriteLine($"Total Price: {booking.CalculateTotalPrice()}");
            Console.WriteLine($"Payment Info: {booking.PaymentMethod.GetPaymentInfo()}");
            Console.WriteLine("Status: Booked");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }

        continue;
    }

    if (choice == "3")
    {
        Console.Write("Enter BookingId: ");
        var bookingId = Console.ReadLine();

        try
        {
            hotel.CheckInBooking(bookingId ?? string.Empty);
            Console.WriteLine("Check-in completed successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }

        continue;
    }

    if (choice == "4")
    {
        Console.Write("Enter BookingId: ");
        var bookingId = Console.ReadLine();

        try
        {
            hotel.CheckOutBooking(bookingId ?? string.Empty);
            Console.WriteLine("Check-out completed successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }

        continue;
    }

    if (choice == "5")
    {
        Console.Write("Enter GuestId: ");
        var guestId = Console.ReadLine();

        try
        {
            var bookings = hotel.GetGuestBookings(guestId ?? string.Empty);

            if (bookings.Count == 0)
            {
                Console.WriteLine("You have no active bookings.");
                continue;
            }

            foreach (var booking in bookings)
            {
                Console.WriteLine(
                    $"BookingId: {booking.BookingId}, Room: {booking.Room.RoomNumber}, Check-in: {booking.CheckInDate:dd.MM.yyyy}, Check-out: {booking.CheckOutDate:dd.MM.yyyy}, Total price: {booking.CalculateTotalPrice()}, Paid: {booking.IsPaid}, Status: {booking.Status}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }

        continue;
    }

    if (choice == "6")
    {
        Console.WriteLine("Choose guest type:");
        Console.WriteLine("1 = Regular guest");
        Console.WriteLine("2 = VIP guest");
        Console.Write("Guest type: ");
        var guestTypeChoice = Console.ReadLine();

        Console.Write("Enter name: ");
        var name = Console.ReadLine();

        Console.Write("Enter email: ");
        var email = Console.ReadLine();

        try
        {
            Guest guest;

            if (guestTypeChoice == "1")
            {
                guest = new RegularGuest(name ?? string.Empty, email ?? string.Empty);
            }
            else if (guestTypeChoice == "2")
            {
                guest = new VipGuest(name ?? string.Empty, email ?? string.Empty);
            }
            else
            {
                Console.WriteLine("Invalid guest type.");
                continue;
            }

            hotel.RegisterGuest(guest);

            Console.WriteLine("Guest registered successfully:");
            Console.WriteLine($"GuestId: {guest.GuestId}");
            Console.WriteLine($"Name: {guest.Name}");
            Console.WriteLine($"Email: {guest.Email}");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }

        continue;
    }

    Console.WriteLine("Invalid choice.");
}
