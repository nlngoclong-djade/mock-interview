# Mock Assessment 01 — C# Refactoring and LRU Cache

This is an original practice exercise. It is not Speechify code and is not based on their private assessment repository.

## Format

- Total time limit: **50 minutes**
- Task 1: maximum **30 minutes**
- Task 2: maximum **20 minutes**
- Deliverable: committed changes on the `attempt/01` branch
- Do not use AI assistance while the timer is running
- Do not add external libraries

## Prepare the attempt branch

Create your attempt from this starter branch:

```bash
git fetch origin
git switch mock/01-starter
git pull
git switch -c attempt/01
git push -u origin attempt/01
```

Before starting the timer, verify the project:

```bash
dotnet run --project tests/LegacyPayments.Tests
```

All checks should print `PASS`. Do not inspect or modify the code before creating the start commit.

## Start

Start your timer and immediately create the following marker:

```bash
git commit --allow-empty -m "START mock 01"
```

The timestamp of this commit is the official start time.

## Task 1 — Refactor the payment component

**Maximum time: 30 minutes**

Refactor the legacy payment component to improve its structure, readability, maintainability, and separation of concerns.

Requirements:

- Preserve all existing externally observable behavior.
- Keep the public API compatible unless a change is clearly justified.
- Keep the existing checks passing.
- Do not over-engineer the solution.
- Do not begin Task 2 before completing and committing Task 1.

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

- Frequently requested payments should avoid unnecessary reads from the underlying data store.
- Creating or changing payment data must not leave stale cached values.
- Missing payments must continue to behave exactly as they do now.
- The cache capacity should be configurable; use a sensible default.
- Do not modify the supplied LRU implementation unless you find a correctness bug.
- Keep all existing checks passing.

When Task 2 is complete—or when the total 50 minutes expires—commit immediately:

```bash
git add .
git commit -m "TASK 2 COMPLETE"
git push
```

## Timing rules

The assessment is graded strictly from Git commit timestamps:

- `START mock 01` → `TASK 1 COMPLETE`: at most **30:00**
- `TASK 1 COMPLETE` → `TASK 2 COMPLETE`: at most **20:00**
- `START mock 01` → `TASK 2 COMPLETE`: at most **50:00**
- Exceeding any limit means the corresponding timing requirement fails.
- Missing a marker commit means timing verification fails.
- Do not amend, squash, rebase, or alter commit timestamps.
- Commits after `TASK 2 COMPLETE` are excluded from assessment.
- Push time is not counted; the completion commit timestamp is authoritative.

## Submission checklist

1. Run all checks.
2. Review the diff for accidental behavior changes.
3. Confirm the three marker commits exist.
4. Push `attempt/01`.
5. Send the branch for strict review.
