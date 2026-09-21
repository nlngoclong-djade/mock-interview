# Mock Assessment 03 — Legacy Payment Refactoring and LRU Cache

This is an original Senior C# practice exercise. It is not based on any private assessment repository.

## Format

- Total time limit: **50 minutes**
- Task 1: maximum **30 minutes**
- Task 2: maximum **20 minutes**
- Work only on `attempt/03`
- Do not use AI assistance while the timer is running
- Do not install additional packages

## Prepare the attempt branch

```bash
git fetch origin
git switch mock/03-starter
git pull
git switch -c attempt/03
git push -u origin attempt/03
```

Verify the starter before beginning:

```bash
dotnet test tests/LegacyPayments.Tests
```

All tests must pass. Do not inspect or modify source code before creating the start marker.

## Start marker

Start the timer and immediately run:

```bash
git commit --allow-empty -m "START mock 03"
```

## Task 1 — Refactor the legacy payment component

**Maximum time: 30 minutes**

Refactor the payment component to improve readability, maintainability, and separation of concerns.

Requirements:

- Preserve all externally observable behavior.
- Preserve public API compatibility.
- Keep case-insensitive payment-ID behavior.
- Keep all existing tests passing.
- Do not add external libraries.
- Do not over-engineer the solution.
- Do not begin Task 2 before committing Task 1.

The instructions are intentionally ambiguous. Prioritize the highest-value improvements possible within 30 minutes.

At completion—or exactly when 30 minutes expires—run:

```bash
git add .
git commit -m "TASK 1 COMPLETE"
```

## Task 2 — Integrate the supplied LRU cache

**Maximum time: 20 minutes**

Integrate `LruCache<TKey, TValue>` into the payment data-access path.

Requirements:

- Repeated lookups must avoid unnecessary underlying-store reads.
- Saves must not leave stale cached values.
- A failed underlying save must not make unpersisted data visible through cache.
- Payment IDs remain case-insensitive and ignore surrounding whitespace.
- Missing payments retain their existing behavior.
- Callers cannot mutate persisted or cached payment state accidentally, including nested state.
- Cache capacity is configurable with a sensible default.
- Preserve the existing public API.
- Do not modify the supplied LRU implementation unless it contains a correctness bug.
- Keep all existing tests passing.

At completion—or exactly when the total 50 minutes expires—run:

```bash
git add .
git commit -m "TASK 2 COMPLETE"
git push
```

## Timing rules

- `START mock 03` → `TASK 1 COMPLETE`: maximum **30:00**
- `TASK 1 COMPLETE` → `TASK 2 COMPLETE`: maximum **20:00**
- `START mock 03` → `TASK 2 COMPLETE`: maximum **50:00**
- Marker names are case-sensitive and must match exactly.
- Missing or incorrectly named markers fail timing verification.
- Exceeding a limit by any amount fails that timing requirement.
- Do not amend, squash, rebase, or alter timestamps.
- Commits after `TASK 2 COMPLETE` are excluded.

## Submission

Push `attempt/03` and request a strict review.
