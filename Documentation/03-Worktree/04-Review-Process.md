# 04 — Review Process

**Applies to:** all commits on `development`, and the release PR to `main`
**Reviewers:** Project Owner (sole approver, DEC-010). Mentor / Lead for Module 7 code review (STK-006)
**Status:** Amended 2026-11-10: commits are reviewed before they are made. No feature pull requests

---

## 1. Review Stages

| Stage | Who | Output | Gate |
|---|---|---|---|
| 1. Self-check | Claude | Checklist in the approval package | Must pass before stage 2 |
| 2. Approval package | Claude → user | Chat package (CLAUDE.md §5.1) for the release PR, merges, tags and changes outside the workflow | Approval before those steps |
| 3. Post-commit check | Claude | Build, test and diff check after the commit | Failures fixed in a new commit |
| 4. Code review (Module 7) | Mentor / Lead | Review comments | Comments resolved as needed (no formal SLA, D-8) |
| 5. Release review | Project Owner | Approved release PR to `main` | Required before merge to `main` |

Routine commits to `development` follow the approved workflow (CLAUDE.md §5.4). The final review (stage 5) and the single PR to `main` are where the solution is reviewed as a whole. No pull request is raised for work on `development`.

## 2. Self-Check (stage 1)

| Item | Documentation | Implementation |
|---|---|---|
| Requirement IDs listed and correct | Yes | Yes |
| Traceability matrices updated (`12-`, `18-`) | If requirement status changed | Yes |
| BRD unchanged, or amendment proposed and approved first (CON-017) | Yes | Yes |
| Decisions referenced (PDR or DEC) | If a decision is applied | If a decision is applied |
| Build passes | Not applicable | Yes |
| Tests pass | Not applicable | Yes |
| No secrets, patient data or build output in the diff | Yes | Yes |
| Commit message follows `03-Commit-Standards.md` | Yes | Yes |

## 3. Approval Package (stage 2)

The package has the five parts in CLAUDE.md §5.1: change summary, files changed, requirements impacted, proposed commit message and the exact Git commands. The user's approval must be explicit. A reply that only comments on the package is not an approval.

## 4. Post-Commit Check (stage 3)

After an approved commit:

1. `git show --stat HEAD`: confirm only the approved files changed.
2. For implementation: run the build and tests on `development`.
3. If a check fails: report it and fix it with a new commit in `development`. Do not amend or force-push.

## 5. Module 7 Code Review (stage 4)

- The Mentor / Lead reviews commits on `development` using the review checklist in §6.
- Commits are reviewed in order with `git log --oneline` and `git show <sha>`.
- Each finding gets an ID (`RV-001`, `RV-002`, …) in `Documentation/03-Worktree/review-log.md` (created when Module 7 starts).
- Each finding is marked resolved, or deferred with a reason. Resolution is a new commit.
- Progress measure (training plan): comments resolved. No formal review SLA (D-8, 2026-11-10); reviews are performed as needed by the Project Owner.

## 6. Review Checklist

| Area | Check |
|---|---|
| Scope | Change traces to a requirement. Nothing outside the BRD |
| Layers | Controllers hold no rules. No EF Core in controllers. Web references Data only for DI |
| Rules | Matches CLAUDE.md §7 and the planning files |
| Data | Migration present for each entity change. No check constraint on vital ranges |
| Security | Passwords hashed. No secrets. Anti-forgery on POST |
| Tests | Each requirement ID has a test. Failing tests fixed, not skipped |
| Traceability | Matrices show the new status |

## 7. Release Review (stage 5)

- A release PR from `development` to `main` is raised only for a release (`05-Release-Process.md`).
- The PR uses the release template in `05-` §4.
- The Project Owner signs off. Only then is the merge made.

## 8. Turnaround

No formal review SLA is required (D-8, 2026-11-10). Reviews are performed as needed by the Project Owner, the only approver in this single-developer learning project.
