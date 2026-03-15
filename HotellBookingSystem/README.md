# Hotel Booking System

## Description

HotelBookingSystem is a console-based hotel booking system made for [Arbeidskrav 2](Arbeidskrav2_HotellSystem.pdf).
The project is written in C# and focuses on basic hotel management features, object-oriented programming, and simple startup testing.

## Features

- Show available rooms for selected dates
- Create booking
- Check in to an existing booking
- Show bookings for a guest
- Check out an active booking
- Register guest
- Support for different room types: `SingleRoom`, `DoubleRoom`, `Suite`
- Support for different guest types: `RegularGuest`, `VipGuest`
- Support for different payment methods: `CardPayment`, `VippsPayment`

## Project structure

- `Program.cs` - Starts the application, runs simple tests, and shows the console menu
- `Models/` - Contains the main domain classes such as hotel, room, guest, and booking
- `Payments/` - Contains payment-related classes and interface
- `Tests/` - Contains simple test code that runs at startup
- `UML_HotelBookingSystem.pdf` - UML diagram for the project

## OOP concepts used

- **Inheritance**  
  `SingleRoom`, `DoubleRoom`, and `Suite` inherit from `Room`.  
  `RegularGuest` and `VipGuest` inherit from `Guest`.

- **Polymorphism**  
  Different room and guest types can be handled through shared base classes.  
  Payment methods also work through a shared interface.

- **Abstraction**  
  The payment system uses `IPayable` to define common behavior for `CardPayment` and `VippsPayment`.

- **Encapsulation**  
  The classes keep related data and behavior together, for example booking logic inside `Booking` and hotel logic inside `Hotel`.

## Tests

The project includes simple tests in `Tests/SimpleTests.cs`.  
These tests run automatically when the program starts.

The startup tests check things like:

- VIP guest discount
- Booking total price calculation
- Preventing booking of unavailable rooms
- Making a room available again after check out
- Checking room availability based on overlapping dates

## How to run

Make sure you have the .NET SDK installed.

Run the project from the root folder:

```bash
dotnet run
```

When the program starts, the simple tests run first. After that, the console menu lets you:

- Show available rooms
- Create booking
- Check in
- Show bookings
- Check out
- Register guest

## UML

The UML diagram for the project is included in the root folder as [UML_HotelBookingSystem.pdf](UML_HotelBookingSystem.pdf).

## AI usage

Both ChatGPT and Codex were used during development of this project.

AI was used for:

- Understanding the assignment
- Planning the project structure
- Code generation support
- Debugging support
- UML generation support
- Documentation support

The final solution was reviewed, tested, and adjusted manually.

A separate file named [AI_Prompts.md](AI_Prompts.md) is included with examples of prompts used during development.
