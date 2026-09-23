# Current class diagram

This Mermaid block is the editable source and renders directly on GitHub. It replaces the historical PDF, which showed mutable reservation terms and payment identifiers that no longer exist. `+` is public, `~` is internal. Only principal members are shown.

```mermaid
classDiagram
    class ConsoleApp {
        +Run() void
    }
    class Hotel {
        +Rooms IReadOnlyList~Room~
        +Guests IReadOnlyList~Guest~
        +BookingHistory IReadOnlyList~Booking~
        +RegisterRoom(Room) void
        +RegisterGuest(Guest) void
        +GetAvailableRooms(DateTime, DateTime) List~Room~
        +CreateBooking(string, string, DateTime, DateTime, IPayable) Booking
        +GetGuestBookings(string) List~Booking~
        +CheckInBooking(string) void
        +CheckOutBooking(string) void
        +CancelBooking(string) void
    }
    class Room {
        <<abstract>>
        +RoomNumber string
        +PricePerNight decimal
        +IsAvailable bool
        +RoomType string
        +MaxGuests int
        ~CheckIn() void
        ~CheckOut() void
        +DisplayRoomInfo() string
    }
    class SingleRoom {
        +HasDesk bool
    }
    class DoubleRoom {
        +HasExtraBed bool
    }
    class Suite {
        +HasJacuzzi bool
        +HasLounge bool
    }
    class Guest {
        <<abstract>>
        +GuestId string
        +Name string
        +Email string
        +ActiveBookings IReadOnlyList~Booking~
        +GetDiscount(decimal) decimal
        ~EnsureBookingCapacity() void
        ~AddBooking(Booking) void
        ~RemoveBooking(Booking) void
    }
    class RegularGuest
    class VipGuest {
        +LoyaltyPoints int
        ~AddBookingLoyaltyPoints() void
    }
    class Booking {
        ~Booking(Room, Guest, DateTime, DateTime, IPayable)
        +BookingId string
        +Room Room
        +Guest Guest
        +CheckInDate DateTime
        +CheckOutDate DateTime
        +PaymentMethod IPayable
        +IsPaid bool
        +IsActive bool
        +Status BookingStatus
        +CalculateTotalPrice() decimal
        +CheckIn() void
        +CheckOut() void
        +Cancel() void
        ~ProcessPayment() bool
    }
    class BookingStatus {
        <<enumeration>>
        Booked
        CheckedIn
        CheckedOut
        Cancelled
    }
    class IPayable {
        <<interface>>
        +ProcessPayment(decimal) bool
        +GetPaymentInfo() string
    }
    class CardPayment {
        +CardPayment(bool simulateSuccess)
    }
    class VippsPayment {
        +VippsPayment(bool simulateSuccess)
    }
    Room <|-- SingleRoom
    Room <|-- DoubleRoom
    Room <|-- Suite
    Guest <|-- RegularGuest
    Guest <|-- VipGuest
    IPayable <|.. CardPayment
    IPayable <|.. VippsPayment
    ConsoleApp --> Hotel : coordinates demo
    Hotel "1" o-- "0..*" Room : registered rooms
    Hotel "1" o-- "0..*" Guest : registered guests
    Hotel "1" o-- "0..*" Booking : history
    Guest "1" --> "0..*" Booking : active bookings
    Booking "0..*" --> "1" Room
    Booking "0..*" --> "1" Guest
    Booking "0..*" --> "1" IPayable
    Booking --> BookingStatus
```

Booking identity, room, guest, dates and payment reference are getter-only. Price is captured at creation. Status and paid flags have private setters. Guest identity/email and room number cannot be changed publicly; collection views cannot be mutated. Only `Hotel` constructs bookings. Room occupancy mutations and payment processing are internal. Card/Vipps classes are credential-free simulations with success enabled by default.
