# Mock Assessment 02 — C# Refactoring and LRU Cache

This is an original practice exercise. It is not Speechify code and is not based on their private assessment repository.

## Format

- Total time limit: **50 minutes**
- Task 1: maximum **30 minutes**
- Task 2: maximum **20 minutes**
- Deliverable: committed changes on the `attempt/02` branch
- Do not use AI assistance while the timer is running
- Do not add external libraries

## Prepare the attempt branch

```bash
git fetch origin
git switch mock/02-starter
git pull
git switch -c attempt/02
git push -u origin attempt/02
```

Before starting the timer, verify the project:

```bash
dotnet test tests/LegacyStore.Tests
```

All tests must pass. Do not inspect or modify the source before creating the start commit.

## Start

Start your timer and immediately create:

```bash
git commit --allow-empty -m "START mock 02"
```

## Task 1 — Refactor the catalog component

**Maximum time: 30 minutes**

Refactor the legacy catalog and checkout component to improve its structure, readability, maintainability, and separation of concerns.

Requirements:

- Preserve all existing externally observable behavior.
- Keep the public API compatible unless a change is clearly justified.
- Keep all existing checks passing.
- Do not add external libraries.
- Do not over-engineer the solution.
- Do not begin Task 2 before committing Task 1.

The instructions are intentionally ambiguous. Decide which improvements provide the most value within the time limit.

When Task 1 is complete—or when 30 minutes expires—commit immediately:

```bash
git add .
git commit -m "TASK 1 COMPLETE"
```

## Task 2 — Integrate the LRU cache

**Maximum time: 20 minutes**

Integrate the provided `LruCache<TKey, TValue>` into the data layer.

Requirements:

- Repeated product lookups should avoid unnecessary reads from the underlying data store.
- Product updates must not leave stale cached values.
- Missing products must continue to behave exactly as they do now.
- Callers must not be able to mutate stored or cached product state accidentally.
- Cache capacity must be configurable with a sensible default.
- Do not modify the supplied LRU implementation unless you find a correctness bug.
- Keep all existing checks passing.

When Task 2 is complete—or when the total 50 minutes expires—commit immediately:

```bash
git add .
git commit -m "TASK 2 COMPLETE"
git push
```

## Timing rules

- `START mock 02` → `TASK 1 COMPLETE`: at most **30:00**
- `TASK 1 COMPLETE` → `TASK 2 COMPLETE`: at most **20:00**
- `START mock 02` → `TASK 2 COMPLETE`: at most **50:00**
- Exceeding any limit fails that timing requirement.
- Missing or incorrectly named marker commits fails timing verification.
- Do not amend, squash, rebase, or alter commit timestamps.
- Commits after `TASK 2 COMPLETE` are excluded.
- Push time is not counted.

## Submission checklist

1. Run all checks.
2. Review the diff for behavior changes.
3. Confirm all three marker commits exist.
4. Push `attempt/02`.
5. Send the branch for strict review.
