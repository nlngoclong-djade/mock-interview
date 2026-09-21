# Mock Assessment 04 — Concurrent Order Processing

Original Senior C# practice exercise.

## Format

- Total time limit: **50 minutes**
- Task 1: maximum **25 minutes**
- Task 2: maximum **25 minutes**
- Work only on `attempt/04`
- Do not use AI assistance while the timer is running
- Do not install additional packages

## Prepare the attempt branch

```bash
git fetch origin
git switch mock/04-starter
git pull
git switch -c attempt/04
git push -u origin attempt/04
```

Verify the starter before beginning:

```bash
dotnet run --project tests/ConcurrentOrders.Tests
```

All visible checks must pass. Do not inspect or modify source code before creating the start marker.

## Start marker

Start the timer and immediately run:

```bash
git commit --allow-empty -m "START mock 04"
```

## Task 1 — Make order processing safe under concurrency

**Maximum time: 25 minutes**

The supplied order service works in simple sequential scenarios, but production occasionally sends duplicate/concurrent requests.

Improve the implementation so that:

- Concurrent requests for the same order do not charge the customer more than once.
- A successfully completed order is not processed again.
- Requests for different order IDs should not unnecessarily block one another.
- If charging fails, the order must remain retryable.
- Cancellation must be honored while waiting on asynchronous dependencies.
- Preserve existing public API compatibility and externally observable behavior where these requirements do not require a change.
- Do not replace asynchronous work with blocking calls such as `.Result`, `.Wait()`, or `Thread.Sleep`.
- Do not add external libraries.
- Do not over-engineer.

At completion—or exactly when 25 minutes expires:

```bash
git add .
git commit -m "TASK 1 COMPLETE"
```

## Task 2 — Add idempotent request handling

**Maximum time: 25 minutes**

Clients may retry the same logical request using the same idempotency key.

Implement idempotent processing with these requirements:

- The same non-empty idempotency key must not cause the payment gateway to be charged more than once.
- Concurrent calls using the same key must observe one logical processing operation.
- A successful retry with the same key returns the previously completed result.
- Reusing a key for a different order ID must be rejected.
- Failed or cancelled attempts must remain retryable; do not permanently cache failures.
- Idempotency keys are case-sensitive but ignore surrounding whitespace.
- Avoid holding a global lock while awaiting external I/O.
- Memory used for completed idempotency entries must be bounded. Use a configurable capacity with a sensible default.
- Preserve the existing public API. You may add overloads or private/internal helpers.
- Keep all visible checks passing.

At completion—or exactly when the total 50 minutes expires:

```bash
git add .
git commit -m "TASK 2 COMPLETE"
git push
```

## Timing rules

- `START mock 04` → `TASK 1 COMPLETE`: maximum **25:00**
- `TASK 1 COMPLETE` → `TASK 2 COMPLETE`: maximum **25:00**
- `START mock 04` → `TASK 2 COMPLETE`: maximum **50:00**
- Marker names are case-sensitive and must match exactly.
- Missing or incorrectly named markers fail timing verification.
- Do not amend, squash, rebase, or alter timestamps.
- Commits after `TASK 2 COMPLETE` are excluded.

## Submission

Push `attempt/04` and request a strict review.
