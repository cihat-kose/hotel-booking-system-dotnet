# Validation record

## Environment and clean clone

Validated with **.NET SDK 10.0.204 on Windows**. A fresh `git clone --no-local --branch portfolio/tested-console` of the committed repository was created in a temporary directory, with a separate empty `NUGET_PACKAGES` directory. This checks tracked-file completeness without relying on the original working tree's build output or restored packages.

The exact README commands were run from that clone's root:

| Command | Result |
| --- | --- |
| `dotnet restore` | Exit 0; both projects restored into the empty package directory. |
| `dotnet build` | Exit 0; 0 warnings, 0 errors. |
| `dotnet test` | Exit 0; 57 passed, 0 failed, 0 skipped. |
| `dotnet run --project HotelBookingSystem/HotelBookingSystem.csproj` | Exit 0; the complete README walkthrough succeeded. |

A temporary deliberately failing xUnit test was added **only to the temporary clone**. `dotnet test --filter FullyQualifiedName~ExitCodeProbe` reported 1 failure and **exit code 1**. The probe was removed; the complete suite then passed all 57 tests again with exit 0, and `git status --porcelain` was empty. The failure probe is not committed.

Release configuration was also built/tested locally: 0 warnings/errors and 57 passing tests.

## Console usage check

The real console executable was run with redirected input, not a mocked `Hotel`. This is an agent-driven scripted usage check, not a claim of independent human acceptance testing.

Verified the README sequence: show available rooms for 10–12 June 2030 → create G001/101/Card DEMO reservation → display BK001 at 1600.00 NOK → check in → check out → show no active bookings → exit. The output included the documented menu labels, sample identifiers and success messages. No startup test messages or card/phone prompts appeared.

Additional fresh-process checks covered Card DEMO decline followed by an empty active-booking list; VIP/Vipps DEMO success followed by cancellation; and blank menu input, malformed dates and invalid email followed by recovery. Unit tests separately cover EOF and invalid choices.

All relative Markdown links were checked against existing files. The old repository address has no remaining live code/document links. The diagram was compared with final classes, inheritance, interface implementations, visibility and relationships; its Mermaid block is the maintained source.

## Remote verification

The repository was renamed to `cihat-kose/hotel-booking-system-dotnet` after tests and content review. The GitHub response confirmed the same repository ID, `private: true`, and default branch `master`. Its description matches the requested text. Local `origin` is `https://github.com/cihat-kose/hotel-booking-system-dotnet.git`; `git ls-remote` succeeded and confirmed that `master` still points to baseline `9a92cd7`.

[Build and test workflow](../.github/workflows/dotnet.yml) runs the Release restore/build/test sequence on Ubuntu for the portfolio branch and PRs. The actual run result is reported in the PR; this document does not imply that local Windows checks alone prove a GitHub Actions run passed.

**First-time usability:** with the .NET 10 SDK and NuGet connectivity, the tested portfolio revision can be restored, tested and run using only the root README. Until the PR is merged, check out the PR branch; `master` deliberately retains the original version.

**Publication:** technical verification does not resolve historical document rights or personal metadata. See [publication audit](publication-audit.md). Visibility remains private.
