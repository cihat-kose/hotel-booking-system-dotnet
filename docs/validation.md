# Validation record

Validation is performed from the repository root on the `master` branch with the .NET 10 SDK.

```sh
dotnet restore
dotnet build
dotnet test
dotnet run --project HotelBookingSystem/HotelBookingSystem.csproj
```

The current solution builds successfully with no warnings or errors. The test suite passes with **57 tests**: 57 expanded xUnit cases, with no failures or skips.

The console walkthrough verifies sample room and guest output, date-based availability, booking creation, payment simulation, check-in, check-out, and normal exit. The test suite also covers invalid input, end-of-input handling, booking limits, overlapping and adjacent dates, payment failures, immutable booking terms, collection protection, and lifecycle invariants.

Generated `bin/`, `obj/`, IDE, and temporary files are excluded by `.gitignore`.
