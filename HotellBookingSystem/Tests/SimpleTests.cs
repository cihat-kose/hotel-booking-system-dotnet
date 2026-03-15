using HotellBookingSystem.Models;
using HotellBookingSystem.Payments;

namespace HotellBookingSystem.Tests;

public static class SimpleTests
{
    public static void RunAll()
    {
        var passedTests = 0;
        var totalTests = 5;

        if (RunTest("VipGuest.GetDiscount(1000m) returns 850m", TestVipGuestDiscount))
        {
            passedTests++;
        }

        if (RunTest("Booking.CalculateTotalPrice() returns 2400m for 3 nights", TestBookingTotalPrice))
        {
            passedTests++;
        }

        if (RunTest("Hotel.CreateBooking() throws when room is unavailable", TestCreateBookingUnavailableRoom))
        {
            passedTests++;
        }

        if (RunTest("Booking.CheckOut() makes room available again", TestCheckOutMakesRoomAvailable))
        {
            passedTests++;
        }

        if (RunTest("Hotel.GetAvailableRooms() respects overlapping booking dates", TestAvailableRoomsUsesDateOverlap))
        {
            passedTests++;
        }

        Console.WriteLine($"{passedTests} of {totalTests} tests passed.");
    }

    private static bool RunTest(string testName, Action test)
    {
        try
        {
            test();
            Console.WriteLine($"PASSED: {testName}");
            return true;
        }
        catch
        {
            Console.WriteLine($"FAILED: {testName}");
            return false;
        }
    }

    private static void TestVipGuestDiscount()
    {
        var vipGuest = new VipGuest("Alice", "alice@example.com");
        var discountedPrice = vipGuest.GetDiscount(1000m);

        if (discountedPrice != 850m)
        {
            throw new InvalidOperationException("Discount calculation is incorrect.");
        }
    }

    private static void TestBookingTotalPrice()
    {
        var guest = new RegularGuest("Bob", "bob@example.com");
        var room = new SingleRoom("101", hasDesk: true);
        var payment = new CardPayment("1234567812345678", "Visa");
        var booking = new Booking(room, guest, new DateTime(2026, 3, 15), new DateTime(2026, 3, 18), payment);

        var totalPrice = booking.CalculateTotalPrice();

        if (totalPrice != 2400m)
        {
            throw new InvalidOperationException("Total price calculation is incorrect.");
        }
    }

    private static void TestCreateBookingUnavailableRoom()
    {
        var hotel = new Hotel("Test Hotel");
        var guest = new RegularGuest("Charlie", "charlie@example.com");
        var room = new SingleRoom("102", hasDesk: false);
        var payment = new VippsPayment("99999999");

        hotel.RegisterGuest(guest);
        hotel.RegisterRoom(room);
        hotel.CreateBooking(guest.GuestId, room.RoomNumber, new DateTime(2026, 3, 20), new DateTime(2026, 3, 22), payment);

        try
        {
            hotel.CreateBooking(guest.GuestId, room.RoomNumber, new DateTime(2026, 3, 21), new DateTime(2026, 3, 23), payment);
        }
        catch (InvalidOperationException)
        {
            return;
        }

        throw new InvalidOperationException("Expected booking creation to fail for unavailable room.");
    }

    private static void TestCheckOutMakesRoomAvailable()
    {
        var guest = new RegularGuest("Dana", "dana@example.com");
        var room = new SingleRoom("103", hasDesk: true);
        var payment = new CardPayment("8765432187654321", "Mastercard");
        var booking = new Booking(room, guest, new DateTime(2026, 4, 1), new DateTime(2026, 4, 3), payment);

        booking.CheckIn();
        booking.CheckOut();

        if (!room.IsAvailable || booking.Status != BookingStatus.CheckedOut)
        {
            throw new InvalidOperationException("Room should be available after checkout.");
        }
    }

    private static void TestAvailableRoomsUsesDateOverlap()
    {
        var hotel = new Hotel("Overlap Test Hotel");
        var guest = new RegularGuest("Erik", "erik@example.com");
        var room = new DoubleRoom("201", hasExtraBed: true);
        var payment = new CardPayment("1111222233334444", "Visa");

        hotel.RegisterGuest(guest);
        hotel.RegisterRoom(room);
        hotel.CreateBooking(guest.GuestId, room.RoomNumber, new DateTime(2026, 5, 10), new DateTime(2026, 5, 12), payment);

        var availableRooms = hotel.GetAvailableRooms(new DateTime(2026, 5, 11), new DateTime(2026, 5, 13));

        if (availableRooms.Any(r => r.RoomNumber == room.RoomNumber))
        {
            throw new InvalidOperationException("Booked room should not be available in an overlapping period.");
        }
    }
}
