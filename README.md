# Hotel Booking System | C# and .NET

## Quick start

Install the **.NET 10 SDK**. From the repository root:

```sh
dotnet restore
dotnet build
dotnet test
dotnet run --project HotelBookingSystem/HotelBookingSystem.csproj
```

The application is an in-memory console demonstration. It has no database, authentication, external API, payment account, or real payment processing. Payment classes only simulate approval and decline outcomes.

## Features

- Single rooms, double rooms, and suites with room-specific amenities.
- Regular and VIP guests with booking limits, discounts, and loyalty points.
- Date-based availability using the half-open interval `[check-in, check-out)`.
- Booking lifecycle: `Booked`, `CheckedIn`, `CheckedOut`, and `Cancelled`.
- Immutable booking terms and a price captured at booking creation.
- xUnit tests for booking rules, payment outcomes, lifecycle transitions, and console input.

## Console walkthrough

The application starts with sample rooms **101**, **201**, and **301**, plus sample guests **G001** and **G002**. Dates use `dd.MM.yyyy`.

1. Choose `1. Show available rooms` and enter `10.06.2030` and `12.06.2030`.
2. Choose `2. Create booking` and enter `G001`, room `101`, the same dates, and payment option `1`.
3. Choose `5. Show my bookings` and use `G001`.
4. Choose `3. Check in` and enter the displayed booking ID.
5. Choose `4. Check out` and enter the same booking ID.
6. Choose `0. Exit`.

All names and email addresses are fictional sample data. Never enter real payment details.

## Architecture

```text
HotelBookingSystem.slnx
HotelBookingSystem/
  Program.cs             application entry point
  ConsoleApp.cs          menu, input parsing, and sample data
  Models/                hotel, booking, room, and guest types
  Payments/              payment simulation abstractions and implementations
HotelBookingSystem.Tests/ xUnit booking-rule and console tests
docs/                    architecture, validation, AI disclosure, and design notes
```

`Hotel` coordinates registration, availability, booking, payment simulation, and lifecycle operations. `Booking` owns its agreed price and state transitions. The application uses inheritance, interfaces, encapsulation, and read-only collection views.

See the [architecture diagram](docs/architecture.md), [validation record](docs/validation.md), [AI-use disclosure](docs/ai-usage.md), and [C# and Java design notes](docs/csharp-java.md).

## Tests and CI

The current test run passes with **57 tests** and no failures or skips. GitHub Actions runs the same restore, Release build, and test workflow for pushes to `master` and pull requests targeting `master`.

## Limitations

Data and generated IDs reset when the process exits. This is a single-process demonstration without persistence, authentication, authorization, concurrency guarantees, real-time arrival rules, refunds, or production payment integrations.
