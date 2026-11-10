# 00 — Worktree and Git Workflow Review Package (Module 3)

**Project:** Patient Management System, Phase 1 (localhost learning project)
**Module:** 3 — Worktree
**Status:** Awaiting review. No Git operation has been performed. Nothing has been committed, branched, pushed or merged.

---

## 1. Contents

| # | Artifact | File |
|---|---|---|
| 00 | Review package (this file) | `00-Worktree-Review-Package.md` |
| 01 | Git Strategy (branch model, flows, current state, gaps) | `01-Git-Strategy.md` |
| 02 | Branching Standards (names, lifetime, protection) | `02-Branching-Standards.md` |
| 03 | Commit Standards (format, types, scopes, examples, checks) | `03-Commit-Standards.md` |
| 04 | Review Process (stages, PR template, Module 7 review) | `04-Review-Process.md` |
| 05 | Release Process (versioning, steps, checklist, notes template) | `05-Release-Process.md` |
| 06 | Rollback Process (eight cases with commands) | `06-Rollback-Process.md` |
| 07 | Worktree Standards (layout, create, remove, Module 3 checks) | `07-Worktree-Standards.md` |

Added beyond the list: **07 Worktree Standards**, because the folder is named for Module 3 and the training plan expects worktree practice to be documented.

## 2. Inputs Reviewed

| Input | What was used |
|---|---|
| Planning artifacts (`Documentation/02-Planning/`) | Milestones M0–M9, WBS (WP-1.1 to WP-10.2), roadmap, testing strategy, traceability, PDR decisions |
| `CLAUDE.md` (root) | §5 Git workflow and approval gate; §5.2 commit format; §5.3 PR template; §8 worktree rules; §11 documentation standards |
| Earlier CLAUDE.md (commit `c3bb2ec`, deleted in `8400ffb`) | Hard rules, release standards (§13), worktree location, rollback intent |
| Training plan (`Reference/AI Training & Progress Tracking Plan.docx`) | Module 3 (Worktree) and Module 4 (Implementation) expected outcomes and progress measures |
| Repository state | `main`, `origin/main` and `origin/development` all at `115126c`. One worktree. No tags. No local `development` |

## 3. Decisions for Review

| # | Decision | Options | Recommendation | Affects |
|---|---|---|---|---|
| **D-1** | How documentation reaches `development` | (a) Direct commit on `development` after the approval gate. (b) `docs/*` branch and PR into `development` | **Approved 2026-11-10: (a).** Documentation (BRD, requirements, traceability, architecture, CLAUDE.md, analysis and supporting documents) is committed and pushed to `development`, together with implementation work | 01 §3.1; 02 §1; 03 §5 |
| **D-2** | Branch scope for Module 4 | (a) One branch `feature/module-4-implementation` for all of Module 4, as requested. (b) One branch per milestone (M1 to M9) | (a), as requested. Each work package is a separate commit with its own requirement IDs. It needs one CLAUDE.md change (§5 rule 4: one feature per branch) | 01 §1; 02 §1 |
| **D-3** | Merge method | (a) Squash feature into `development`; merge commit from `development` into `main`. (b) Merge commit for both | (a). A squashed Module 4 gives one clear entry on `development`. A merge commit into `main` keeps the release point visible | 01 §3; 05 §3 |
| **D-4** | Bringing the current untracked files under Git | One approved commit on `development` with `BRD/`, `Documentation/` and `CLAUDE.md`. Decide whether `Reference/` (contains a binary `.docx`) is tracked | Commit the three text folders. For `Reference/`, track the `.docx` so the process reference is versioned. Note the `.docx` is binary and grows the repository | 01 §4, §5; 03 §6 |
| **D-5** | Branch of the primary checkout | (a) `development`, used for documentation and review. (b) `main`, as the current state is | (a). Keeps `main` untouched between releases (Module 3 measure) | 07 §2 |
| **D-6** | GitHub branch protection on `main` and `development` | Enable as in `02-` §4. Repository admin action by the Project Owner | Enable. Without it, direct pushes are possible; nothing in Git stops them | 02 §4; 06 §7 |
| **D-7** | First version number | `v0.1.0` (from the earlier CLAUDE.md) or another starting point | `v0.1.0` | 05 §1 |
| **D-8** | Module 7 review turnaround | Set a target (for example, comments resolved within 2 working days) or leave open | Set a target. The training plan's progress measure needs one; the value is the Project Owner's choice | 04 §5, §7 |

## 4. Proposed Changes to CLAUDE.md (not applied)

CLAUDE.md is governance. It changes only with approval (CLAUDE.md §12). The following changes follow from D-1, D-2 and D-5. They are not applied yet.

| CLAUDE.md section | Current wording | Proposed wording | Depends on |
|---|---|---|---|
| §5 Git rules (never commit or push to `main` or `development` directly) | Never commit to or push to `main` or `development` directly | Keep for `main`. Allow approved documentation commits on `development` | D-1 |
| §5 Hard rule 4 | One feature per branch | One feature per branch, except the Module 4 branch `feature/module-4-implementation`, which holds the whole of Module 4 | D-2 |
| §5 Base branch | PR base `development` | Keep. Add: release PRs base `main`, from `development` | Process |
| §5.2 Scopes | `web`, `business`, `data`, `tests`, `docs` | Add `governance` and `build` (as in `03-` §3) | — |
| §8 Worktree | Sibling folder `ClaudeCodeBhaskar-wt` | Keep. Add: primary checkout on `development` (D-5) | D-5 |
| §5 Working rule | "Documentation is not committed" (user instruction, 2026-11-10) | Replace with the approved documentation flow once D-1 and D-4 are decided | D-1, D-4 |

## 5. Corrections Made While Preparing This Package

| Item | Correction |
|---|---|
| Module numbering in planning files | Implementation is **Module 4** in the training plan. Two planning references said Module 3. Corrected in `02-Planning/00-` and `02-Planning/19-` |
| Release checklist | The earlier generic release standard required backup and restore verification. Removed for Phase 1, because backups are deferred (OOS-011; RSK-004). Recorded in `05-` §6 |
| Rollback without backups | Phase 1 rollback for data is by rebuilding from migrations and the seed generator (`06-` §5) |

## 6. Gaps Found in the Current Repository

| Gap | Effect | Resolution |
|---|---|---|
| No local `development` branch | Documentation cannot be committed to `development` yet | Create it after D-1 and D-4 are approved |
| Untracked documentation on `main` | Work is at risk if the working tree is lost | D-4 |
| No branch protection | Direct pushes are possible | D-6 |
| No tags | Release points are not visible | First release after Module 4 (D-7) |
| Single worktree on `main` | Implementation would start on `main` if not switched | D-5 and `07-` §3 |

## 7. Current Git State (read-only check, 2026-11-10)

```
main                     115126c  Delete README.md
origin/main              115126c
origin/development       115126c
worktrees                primary checkout on main
tags                     none
untracked                BRD/, CLAUDE.md, Documentation/, Reference/
```

## 8. Not Done

- No branch, commit, push, pull request, merge, tag or worktree has been created.
- No CLAUDE.md change has been applied.
- No branch protection has been set.

## 9. Next Steps After Review

1. Reviewer answers D-1 to D-8.
2. Claude applies the approved CLAUDE.md changes (§4), on a governance change that the Project Owner approves.
3. Claude prepares the approval package for D-4 (the first documentation commit). No commit until approved.
4. Module 4 starts only when M0 is closed and the approved plan is confirmed (Module 2 approval).
