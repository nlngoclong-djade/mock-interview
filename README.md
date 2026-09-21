# Refactoring Assessment (C#) — Practice Repository

This is an original practice exercise. It is not Speechify code and is not based on their private assessment repository.

## Format

- Time limit: 50 minutes
- Task 1: about 30 minutes
- Task 2: about 20 minutes
- Deliverable: committed changes in this repository

Before starting, run:

```bash
dotnet run --project tests/LegacyPayments.Tests
```

All checks should print `PASS`.

## Task 1 — Refactor the payment component

Refactor the legacy payment component to improve its structure, readability, maintainability, and separation of concerns.

Requirements:

- Preserve all existing externally observable behavior.
- Keep the public API compatible unless a change is clearly justified.
- Do not add external libraries.
- Do not over-engineer the solution.
- Keep the existing checks passing.

The instructions are intentionally ambiguous. Decide which improvements provide the most value within the time limit.

## Task 2 — Integrate the LRU cache

Integrate the provided `LruCache<TKey, TValue>` into the data layer.

Requirements:

- Frequently requested payments should avoid unnecessary reads from the underlying data store.
- Creating or changing payment data must not leave stale cached values.
- Missing payments must continue to behave exactly as they do now.
- The cache capacity should be configurable; use a sensible default.
- Do not modify the supplied LRU implementation unless you find a correctness bug.
- Do not add external libraries.

## Submission checklist

1. Run all checks.
2. Review your diff for accidental behavior changes.
3. Commit your work to Git.
4. Stop when 50 minutes expires.

When you finish, send the repository or a patch for review. Do not include a written explanation unless you want to simulate a follow-up interview.
