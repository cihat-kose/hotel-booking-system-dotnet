# Hotel Booking System | C# & .NET

## Quick start

Install the **.NET 10 SDK** (not just the runtime). From the repository root, run:

```sh
dotnet restore
dotnet build
dotnet test
dotnet run --project HotelBookingSystem/HotelBookingSystem.csproj
```

No database, API keys, payment account, or configuration is needed. Restore needs access to NuGet. The app starts with rooms **101**, **201**, **301** and demo guests **G001** (regular) and **G002** (VIP). Dates use **dd.MM.yyyy**. Choose **1. Show available rooms** to begin; **0. Exit** closes the app. Tests run only with `dotnet test`.

## About

A C#/.NET console application for hotel reservations, with automated tests and documented booking rules. It demonstrates encapsulation, inheritance, interfaces and polymorphism in a small, in-memory application. It is not a commercial hotel system and never processes real payments.

## Try it in 1â€“2 minutes

Start a fresh app session. The identifiers below are also printed on screen. Enter each value when prompted:

| Step | Menu label | Input, in order | Expected result |
| --- | --- | --- | --- |
| 1 | `1. Show available rooms` | `10.06.2030`, `12.06.2030` | Rooms 101, 201 and 301 are available for the selected dates. |
| 2 | `2. Create booking` | `G001`, `101`, `10.06.2030`, `12.06.2030`, `1` | Card DEMO success; `BookingId: BK001`, `Total price: 1600.00 NOK`, `Status: Booked`. |
| 3 | `5. Show my bookings` | `G001` | BK001 appears in the active bookings. |
| 4 | `3. Check in` | `BK001` | `Check-in completed successfully.` |
| 5 | `4. Check out` | `BK001` | `Check-out completed successfully.` |
| 6 | `5. Show my bookings` | `G001` | `You have no active bookings.` |
| 7 | `0. Exit` | â€” | The app closes normally. |

Payment choices are `1 = Card DEMO success`, `2 = Vipps DEMO success`, and `3 = Card DEMO decline`. No card or phone number is requested. After other attempts, copy the actual BookingId printed by the app; failed attempts may leave gaps in IDs. Use `8. Show rooms and guests` to redisplay identifiers, `6. Register new guest` for fictional demo data, or `7. Cancel booking` to cancel an active reservation. Invalid or blank input displays an explanation and returns to the menu. End of input exits normally.

## Features and booking rules

- Single rooms (800 NOK/night), double rooms (1200) and suites (3500), with their original room-specific amenities.
- Regular guests: at most 3 active bookings, no discount. VIP guests: at most 10, 15% discount, 10 loyalty points per successful booking.
- Whole-date stays use **[check-in, check-out)**: the checkout date is free for the next reservation. Equal/reversed dates and times of day are rejected.
- Only registered guests and rooms can be booked. Overlapping active reservations for the same room are rejected before payment. Adjacent stays are allowed.
- Room, guest, dates and payment method are fixed on creation. The agreed total is stored so a later room-rate change cannot change a paid booking.
- Declined or throwing payment simulations leave no guest booking, history entry, occupancy change or loyalty points. Provider exception details are not shown.
- `Booked â†’ CheckedIn â†’ CheckedOut`; `Booked` or `CheckedIn` may also become `Cancelled`. Completed bookings reject further transitions. Cancellation after check-in releases the room, matching the original exercise behavior.
- `Room.IsAvailable` means **unoccupied now**. `GetAvailableRooms` answers availability for selected dates. An occupied room may have a future reservation, but another check-in must wait for its current occupant to check out.
- Checkout/cancellation remove the guest's active booking and release its date range. History keeps the final booking status. Simulated payment stays marked paid; no refunds or loyalty-point reversals are modeled.

## Architecture

```text
HotelBookingSystem.slnx
HotelBookingSystem/
  Program.cs             entry point
  ConsoleApp.cs          menu, parsing and demo data
  Models/                Hotel, Booking, Room and Guest hierarchies
  Payments/              IPayable and credential-free demo implementations
HotelBookingSystem.Tests/  xUnit business-rule and console tests
docs/                     current UML, AI disclosure and publication audit
```

`Hotel` coordinates validation, date availability, payment and registration. `Booking` owns its lifecycle and agreed price. Room and guest subclasses demonstrate inheritance/polymorphism; `IPayable` keeps simulated payment interchangeable. Collections expose read-only views. There is no framework, service container or persistence layer.

See the [current UML diagram and editable Mermaid source](docs/architecture.md). Internal `HotellBookingSystem` namespaces retain the original Norwegian spelling; project paths use `HotelBookingSystem` consistently.

## Tests and CI

The five original `SimpleTests` scenarios are now xUnit tests; they no longer run at startup. Additional tests cover interval boundaries, invalid dates, limits, price stability, payment failures, cancellation, lifecycle invariants, immutable reservation terms, collection protection and invalid/empty/EOF console input. Payment fakes check both the result and whether charging was attempted. Dates are fixed rather than tied to the system clock.

`dotnet test` reports failures with a nonzero exit code. [GitHub Actions](.github/workflows/dotnet.yml) restores, builds and tests on pushes and pull requests targeting `master`. Local and clean-clone verification details are recorded in [validation](docs/validation.md); no unverified status badge is shown.

## Limitations and provenance

All data and IDs reset on process restart. This is a single-process, single-hotel demonstration, not thread-safe or authenticated. Guest IDs are selectors, not access control. Past/future dates and immediate check-in are intentionally allowed so examples are reproducible; real-time arrival/departure enforcement is absent. Email validation only checks for `@`. History is an in-memory list with final statuses, not a durable event log. Cancellation does not simulate refunds. There are no real Card/Vipps integrations, network payment calls, credentials or payment-data storage.

Originally developed as **Arbeidskrav 2 at Gokstad Akademiet**, the project has since been improved for portfolio use. Startup tests were replaced with xUnit, payment identifiers removed, and UML updated as editable documentation. Historical third-party documents are omitted; removing a file does **not** remove its historical copies.

See [C# concepts and their Java counterparts](docs/csharp-java.md), [AI/KI usage and provenance](docs/ai-usage.md) and the [pre-publication audit](docs/publication-audit.md). No license has been assumed or added. **Public release remains blocked pending the history/privacy and document-rights decisions in the audit.**
