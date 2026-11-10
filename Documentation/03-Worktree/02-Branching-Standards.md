# 02 — Branching Standards

**Applies to:** all branches in `ClaudeCodeBhaskar`
**Status:** Amended 2026-11-10: one working branch. Branch protection not required (D-6)

---

## 1. Branches

| Branch | Type | Created by | Deleted | Used for |
|---|---|---|---|---|
| `development` | Working branch | Exists | Never | All project work: documentation, requirements, planning, governance and implementation |
| `main` | Release branch | Exists | Never | Released code only, updated by release PR |
| Tags `vX.Y.Z` | Release markers | Created on `main` after a release PR is merged | Never (tags are not moved or deleted once pushed) | Marking releases |

No other branch is created. Feature, module, milestone, documentation, fix, chore, hotfix and release branches are not used.

## 2. Naming

- Only `development` and `main` are named. No names are invented for work.
- Tags: `vMAJOR.MINOR.PATCH` (SemVer), for example `v0.1.0` (`05-Release-Process.md`).
- Worktree folders and verification worktrees (`07-Worktree-Standards.md`) are not branches and are not named after branches.

## 3. Protection

No branch protection is required (decision D-6, 2026-11-10). The project is a personal learning project with a single developer and localhost deployment only.

Control comes from the approval gate (CLAUDE.md §5.1): no commit or push happens until the user approves it. Force-push and branch deletion are still forbidden by the rules in `01-Git-Strategy.md` §2, but GitHub does not enforce them.

## 4. Local Hygiene

- The primary checkout is on `development` (approved 2026-11-10).
- `git fetch --prune` before starting work.
- Keep `development` up to date with `git pull --ff-only origin development`. Do not create merge commits on `development` by accident.
- Remove stale verification worktrees after use (`07-Worktree-Standards.md` §5).

## 5. Forbidden Patterns

| Pattern | Reason |
|---|---|
| Creating any other working branch | Single working branch instruction (2026-11-10) |
| Committing directly to `main` | Released code only |
| Force-push to `development` or `main` | Rewrites shared history |
| Moving or deleting a pushed tag | Tags are release markers |
| Committing secrets, local settings or build output | `03-Commit-Standards.md` §6 |
