# 00 — Worktree and Git Workflow Review Package (Module 3)

**Project:** Patient Management System, Phase 1 (localhost learning project)
**Module:** 3 — Worktree
**Status:** Amended 2026-11-10 for one working branch. Awaiting review of the open decisions below. The amendments are not yet committed (see §6).

---

## 1. Contents

| # | Artifact | File |
|---|---|---|
| 00 | Review package (this file) | `00-Worktree-Review-Package.md` |
| 01 | Git Strategy (one working branch, flows, current state, gaps) | `01-Git-Strategy.md` |
| 02 | Branching Standards (two branches, tags, protection) | `02-Branching-Standards.md` |
| 03 | Commit Standards (format, types, scopes, examples, checks) | `03-Commit-Standards.md` |
| 04 | Review Process (approval gate, post-commit check, Module 7 review, release review) | `04-Review-Process.md` |
| 05 | Release Process (versioning, release steps, checklist, notes template) | `05-Release-Process.md` |
| 06 | Rollback Process (cases A to F, commands) | `06-Rollback-Process.md` |
| 07 | Worktree Standards (detached verification worktrees, Module 3 checks) | `07-Worktree-Standards.md` |

## 2. Instructions Applied

| Instruction | Date | Applied in |
|---|---|---|
| Documentation work stays on `development`; implementation uses a feature branch | Original request | Superseded (see below) |
| D-1: documentation committed directly to `development` | 2026-11-10 | Committed (`a88551d` to `a875aac`) |
| Approved workflow: work in `development`; commit and push regularly; test and fix in `development`; review; one PR `development` → `main`; merge after final approval | 2026-11-10 | CLAUDE.md §5.4; `01-Git-Strategy.md` §3 |
| D-6: no branch protection required | 2026-11-10 | `02-` §3 |
| **Single working branch:** `development` only. No feature, module, milestone or documentation branches. Remove or amend the "one feature per branch" rule | 2026-11-10 | This amendment: CLAUDE.md §5; `01` to `07`; planning roadmap; Module 1 logs |

The single-branch instruction supersedes the earlier branch model (`feature/module-4-implementation`) and the earlier D-2, D-3 and D-5 options.

## 3. Decisions

| # | Decision | Status | Note |
|---|---|---|---|
| D-1 | Documentation committed directly to `development` | **Approved 2026-11-10** | Done |
| D-2 | Branch scope for Module 4 | **Superseded** | No module branch. All work on `development` |
| D-3 | Merge method | **Superseded** | No feature merges. Release PR to `main` is a merge commit (`05-` §3) |
| D-4 | Track `Reference/` (binary `.docx`) | **Open** | `Reference/` is still untracked |
| D-5 | Primary checkout on `development` | **Approved 2026-11-10** (as part of the single-branch instruction) | Done |
| D-6 | Branch protection | **Decided 2026-11-10: not required.** Single developer, localhost only. The approval gate is the control | No repository settings change |
| D-7 | First version number | **Open** | `v0.1.0` proposed |
| D-8 | Module 7 review turnaround | **Open** | Target for the Project Owner to set |
| D-9 | Verification with detached worktrees | **Open** | Git does not allow a second worktree on `development`. Detached worktrees give isolated verification; development happens in the primary checkout. Approve, or propose another Module 3 exercise |
| D-10 | Urgent fixes without a hotfix branch | **Open** | Fix committed to `development`, then released in the next patch release. Approve, or allow a `hotfix/*` branch as an exception |
| D-11 | Releases to `main` by PR from `development`, merge commit, with Project Owner sign-off | **Open** | Confirms the one remaining use of a pull request |

## 4. Proposed CLAUDE.md Changes (applied in this amendment)

| Section | Before | After |
|---|---|---|
| §5 Git rules | Implementation on a feature branch; one feature per branch; one PR per feature | `development` is the only working branch; no other working branches; `main` receives releases only; urgent fixes go to `development` |
| §5.2 Branch | `feature/`, `fix/`, `chore/` names | `development` only |
| §5.3 PR description | Base `development` for every PR | Release PR only: `release: vX.Y.Z`, base `main`, head `development` |

## 5. Corrections Made While Preparing This Package

| Item | Correction |
|---|---|
| Module numbering | Implementation is Module 4. Corrected in `02-Planning/00-` and `02-Planning/19-` |
| Release checklist | Backup and restore verification removed for Phase 1 (OOS-011; RSK-004) |
| Rollback without backups | Data is rebuilt from migrations and the seed generator (`06-` §5) |
| Worktree expectation | Module 3 expects a separate environment for development. Under one branch, that is met only for verification (`07-` §7). Stated openly for D-9 |

## 6. Current State (for review)

| Item | State |
|---|---|
| `development` | `a875aac`, pushed to `origin/development` |
| `main` | `115126c`, unchanged |
| Working branches | `development` only |
| Uncommitted | This amendment (`CLAUDE.md`, `Documentation/02-Planning/` and `03-Worktree/` edits, and Module 1 log edits) |
| Untracked | `Reference/` |

## 7. Next Step: Commit of This Amendment

The amendment is ready to commit to `development`. The approval package (CLAUDE.md §5.1) is shown in chat before the commit:

- Change summary: single-branch rule; branch-related documents rewritten or updated.
- Files: as listed in §4 and in the change summary.
- Requirements impacted: none. Process documentation only.
- Proposed commit message: `docs(governance): make development the only working branch`.
- Git commands: `git add`, `git commit`, `git push origin development`.

No commit or push is made until the user approves.

## 8. Not Done

- No branch, commit, push, merge, tag or worktree has been created for this amendment.
- No branch protection has been set.
- No release has been made.
