# 00 — Git Workflow and Governance Review Package (Module 3)

**Project:** Patient Management System, Phase 1 (localhost learning project)
**Module:** 3 — Worktree
**Status:** All decisions D-1 to D-11 resolved by the Project Owner (2026-11-10). Governance documents updated to match.

---

## 1. Contents

| # | Artifact | File |
|---|---|---|
| 00 | This review package | `00-Worktree-Review-Package.md` |
| 01 | Git Strategy (one working branch, workflow, release, current state) | `01-Git-Strategy.md` |
| 02 | Branching Standards (two branches, tags, no protection) | `02-Branching-Standards.md` |
| 03 | Commit Standards (format, types, scopes, examples, checks) | `03-Commit-Standards.md` |
| 04 | Review Process (approval gate, checks, final review, release review) | `04-Review-Process.md` |
| 05 | Release Process (versioning, release steps, checklist, notes template) | `05-Release-Process.md` |
| 06 | Rollback Process (cases A to E, commands) | `06-Rollback-Process.md` |
| 07 | Worktree Standards (not required under D-9) | `07-Worktree-Standards.md` |

## 2. Approved Branching Strategy

```
main
└── Final approved release

development
├── Documentation
├── Entity Framework Core
├── SQL Server
├── ASP.NET MVC
├── Testing
├── Bug Fixes
└── Final Validation
```

Workflow:

1. Work directly in `development`.
2. Commit and push documentation and code changes.
3. Complete all project features.
4. Test the full application.
5. Create one PR: `development` → `main`.
6. Merge using a merge commit.

## 3. Decisions

| # | Decision | Resolution (2026-11-10) | Where applied |
|---|---|---|---|
| D-1 | Documentation committed directly to `development` | Approved | `CLAUDE.md` §5; commits `a88551d` to `a875aac` |
| D-2 | Branch scope for Module 4 | Superseded: no module branch. All work on `development` | `CLAUDE.md` §5.4; `01-` §2 |
| D-3 | Merge method | Superseded by D-11: merge commit, no squash or rebase | `05-` §3 |
| D-4 | Track `Reference/` training plan (`.docx`) in Git | Approved: track it, commit with the other documentation | Committed to `development` (see §5) |
| D-5 | Primary checkout on `development` | Approved | `02-` §4 |
| D-6 | Branch protection | Not required. Single developer, localhost only | `02-` §3; `01-` §5 |
| D-7 | First version number | Approved: `v0.1.0` | `05-` §1 |
| D-8 | Review turnaround | No formal SLA. Reviews performed as needed by the Project Owner | `04-` §5, §8 |
| D-9 | Detached worktrees for verification | Not required. All development, testing and validation in `development` | `07-` (not required); `CLAUDE.md` §5.4 |
| D-10 | Urgent fixes | Approved: direct fixes in `development`. No hotfix branch | `01-` §3.1; `06-` |
| D-11 | Release PR | Approved: one PR `development` → `main` once the application is complete and tested. Merge commit | `05-` §3; `CLAUDE.md` §5.4 |

## 4. Governance Changes Applied

| Document | Change |
|---|---|
| `CLAUDE.md` §5 | Workflow in §5.4 replaced with the approved six steps. No feature, hotfix, documentation or milestone branches. No detached worktrees. Release PR and merge rules. Tags and deletions need approval |
| `CLAUDE.md` §5.1 | Approval gate covers tags, deletions and changes outside the workflow. The release PR follows §5.4 |
| `01-Git-Strategy.md` | Rewritten around the approved branching strategy and workflow |
| `02-Branching-Standards.md` | Two branches, tags, no protection, no worktrees |
| `04-Review-Process.md` | Routine commits follow the workflow. Final review and single PR. No SLA |
| `05-Release-Process.md` | `v0.1.0`; one PR; merge commit; tag after merge |
| `06-Rollback-Process.md` | Rewritten for one branch. Worktree case removed |
| `07-Worktree-Standards.md` | Replaced with a note that worktrees are not required (D-9) |
| `Documentation/02-Planning/15-` | Commit plan per milestone on `development`. No milestone branches |
| `Documentation/01-Brainstorming/14-` | DEC-016 added |

## 5. Commits

Committed to `development` in this round:

1. The training plan `.docx` (D-4).
2. Governance changes above (D-6 to D-11 and the workflow).

## 6. Corrections Made While Preparing This Package

| Item | Correction |
|---|---|
| Module numbering | Implementation is Module 4. Corrected in `02-Planning/00-` and `19-` |
| Release checklist | Backup and restore verification removed for Phase 1 (OOS-011; RSK-004) |
| Rollback without backups | Data is rebuilt from migrations and the seed generator (`06-` §5) |
| Worktree expectation | Module 3 worktree outcome is not applicable under D-9. Recorded in `07-` |

## 7. Current State

| Item | State |
|---|---|
| `development` | Pushed to `origin/development`, latest commit shown in `git log` |
| `main` | `115126c`, unchanged. Release PR when the application is complete |
| Working branches | `development` only |
| Tags | None. `v0.1.0` at first release |
| Untracked | None after the commit in §5 |

## 8. Not Done

- No merge to `main` (waits for the completed and tested application).
- No tag (`v0.1.0` at first release).
- No implementation code has been written yet.
