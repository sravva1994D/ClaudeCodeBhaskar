# 07 — Worktree Standards

**Module:** 3 — Worktree (training plan)
**Status:** Not required for this project (Project Owner decision D-9, 2026-11-10)

---

## 1. Decision

No detached worktrees or parallel verification environments are used. The Project Owner decided that all development, testing, verification and validation happen directly in `development`.

This file is kept so the Module 3 record is complete. It does not add procedures.

## 2. How the Module 3 Outcomes Are Met

| Training plan expected outcome | How it is met in this project |
|---|---|
| Develop and test changes in a separate, controlled environment | Not applicable under D-9. Development and testing happen in the primary checkout on `development` |
| Work without impact on the main branch | Met. Work is done only on `development`. `main` changes only through the single release PR (D-11) |
| Create and manage worktrees | Not applicable under D-9. No worktrees are created |

## 3. Working Location

- Primary checkout: `D:\Projects\AI\Claude\project-mgmt-system\ClaudeCodeBhaskar`.
- Branch: `development`.
- Before work starts: `git fetch origin` and `git pull --ff-only origin development`.

## 4. Checks

| Check | How | Pass when |
|---|---|---|
| Working branch | `git branch --show-current` | Prints `development` |
| Worktrees | `git worktree list` | Only the primary checkout is listed |
| `main` unchanged during development | `git rev-parse main` | Unchanged until the release PR |
