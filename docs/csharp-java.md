# C# concepts for a Java interview

| In this project | C# | Java counterpart and explanation |
| --- | --- | --- |
| Fixed booking terms | Getter-only properties (`Room { get; }`) and `readonly` price field | `private final` fields plus getters. References cannot be reassigned; the referenced object is not automatically deeply immutable. |
| Room/guest hierarchies | `abstract class`, `: Room`, `override` | `abstract class`, `extends Room`, `@Override`. Subtypes implement room descriptions or discounts. |
| Payment strategy | `IPayable`, `: IPayable` | `interface`, `implements`. Production demo classes and test fakes share the same contract. |
| Hidden mutation | `internal` constructors/methods | Roughly package-private intent, but C# grants access across the entire assembly, not just a package. External callers cannot construct bookings or change occupancy directly. |
| Read-only collections | `IReadOnlyList<T>` with `AsReadOnly()` | `List<T>` exposed through `Collections.unmodifiableList`. This is a live view, not a deep copy; declaring an interface alone would not protect the backing list. |
| Pricing | `decimal` and literals such as `800m` | Usually `BigDecimal`. Decimal arithmetic avoids binary floating-point artifacts in monetary calculations. |
| Queries | LINQ `Any`, `Where`, `ToList` | Stream `anyMatch`, `filter`, `toList`. `Any` stops at the first match. Interval overlap is `start < otherEnd && otherStart < end`. |
| Lifecycle | `enum BookingStatus`, private setters, guarded methods | `enum` plus encapsulated state transitions. Checking state before mutation prevents invalid transitions. |
| Tests | xUnit `[Fact]`, `[Theory]`, `[InlineData]`, `Assert.Throws` | JUnit `@Test`, `@ParameterizedTest`, data providers, `assertThrows`. Tests verify observable rules and side effects, not console startup messages as a test runner. |
| Input/output | `TextReader`, `TextWriter`, `StringReader`, `StringWriter` | `Reader`/`Writer` and string streams. Injecting streams makes the actual menu testable without a terminal. |
| Nullable references | `IPayable?`, nullable analysis | Java annotations such as `@Nullable`; C# compiler analysis is enabled here. `ThrowIfNull` still provides runtime protection. |
| IDs | `Interlocked.Increment` | `AtomicInteger.incrementAndGet`. Atomic counters do not make the whole hotel/collections thread-safe. |

An interview explanation: `Hotel.CreateBooking` validates registration, dates, overlaps and capacity first. It constructs an unregistered booking, captures the price, and tries the payment simulation. Only success adds it to the guest/history and awards VIP points. A failure therefore cannot leave a ghost reservation. This is an in-memory sequence for a single-threaded console, not a distributed transaction or a real payment guarantee.

Date availability and physical occupancy answer different questions. Adjacent reservations share a boundary without overlapping; a room can be reserved for later while occupied now. Check-in separately prevents a second simultaneous occupant. The demo intentionally does not enforce the current calendar date.
