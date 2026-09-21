# Mock Assessment 06 — Notification Dispatch Refactor

Senior C#/.NET assessment focused on clean architecture, clean code, design patterns, refactoring, async behavior and unit testing.

## Format
- Total: **55 minutes**
- Task 1: **30 minutes**
- Task 2: **25 minutes**
- Work on `attempt/06`, created directly from `main`.
- No AI during the timer. No external packages beyond the supplied test packages.

## Prepare
```bash
git switch main
git pull
git switch -c attempt/06
git push -u origin attempt/06
dotnet test mock06/Mock06.sln
```

Start:
```bash
git commit --allow-empty -m "START mock 06"
```

## Task 1 — Refactor legacy dispatch
Refactor `LegacyNotificationService` for maintainability and extensibility while preserving observable behavior.

Requirements:
- Preserve public API and existing behavior.
- Business orchestration should not depend on provider-specific branching.
- Adding a new delivery channel should require minimal modification to existing orchestration.
- Separate validation/rules/provider concerns where justified.
- Preserve async/cancellation behavior.
- Avoid speculative abstractions and over-engineering.
- Keep tests passing.

Commit: `TASK 1 COMPLETE`

## Task 2 — Reliability
Production can retry requests and providers can fail.

Requirements:
- A notification with the same non-empty request key must not be delivered successfully more than once.
- Concurrent calls using the same key must represent one logical operation.
- Successful retry returns the previous successful result.
- Failed or cancelled attempts remain retryable.
- Reusing a key for a different notification target must be rejected.
- Keys are case-sensitive and ignore surrounding whitespace.
- Different keys must not be globally serialized while awaiting provider I/O.
- Completed request state must be bounded with configurable capacity and a sensible default.
- No `.Result`, `.Wait()`, or `Thread.Sleep`.

Commit: `TASK 2 COMPLETE`

## Timing
START → Task1 <= 30:00; Task1 → Task2 <= 25:00; total <= 55:00. Do not amend/rebase marker commits.
