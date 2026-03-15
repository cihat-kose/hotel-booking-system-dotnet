# AI Usage and Example Prompts

Both ChatGPT and Codex were used during development of this project.

AI was used as support for:

- understanding the assignment
- planning the solution
- generating parts of the code
- debugging and improving the solution
- generating UML support
- writing documentation

The final solution was reviewed, tested, and adjusted manually.

## Example prompts used

Below are examples of the kinds of prompts that were used during development of this project.

1. Understanding the assignment text  
   "Read this Arbeidskrav 2 assignment and explain what the program needs to do, which classes I probably need, and which OOP concepts I should show in the solution."

2. Creating a step-by-step project plan  
   "Make a simple step-by-step plan for building a C# console-based hotel booking system. I need rooms, guests, bookings, payments, tests, and a UML diagram."

3. Generating Room-related classes  
   "Help me create a base `Room` class and subclasses `SingleRoom`, `DoubleRoom`, and `Suite` for a hotel booking system in C#. Keep it simple and suitable for a student project."

4. Generating Guest-related classes  
   "Generate a `Guest` base class and two subclasses called `RegularGuest` and `VipGuest`. The VIP guest should support a discount in the booking system."

5. Generating payment classes and interface  
   "Create a simple payment interface for my C# project and add two payment methods: `CardPayment` and `VippsPayment`. I want a shared way to display payment info."

6. Generating Booking and Hotel classes  
   "Help me build `Booking` and `Hotel` classes for a console-based hotel booking system. The hotel should be able to register guests, create bookings, show available rooms for a date range, handle check-in, and handle checkout."

7. Generating simple tests  
   "Write simple startup tests for this project without using a full test framework. I want to test VIP discount, booking total price, unavailable room booking, and that checkout makes the room available again."

8. Extending the console menu  
   "Extend my `Program.cs` menu so the user can show available rooms, create a booking, check in, show bookings, check out, and register a new guest. Use console input and keep it simple."

9. Generating PlantUML for the UML diagram  
   "Generate PlantUML for my hotel booking system based on these classes: `Hotel`, `Booking`, `Room`, `SingleRoom`, `DoubleRoom`, `Suite`, `Guest`, `RegularGuest`, `VipGuest`, `IPayable`, `CardPayment`, and `VippsPayment`."

10. Writing the README  
   "Write a clean and simple README in English for my C# console project called `HotellBookingSystem`. Include description, features, project structure, OOP concepts, tests, how to run, UML, and AI usage."

Not every small follow-up prompt is listed word for word here, but this file shows the most important prompt types used during development.
