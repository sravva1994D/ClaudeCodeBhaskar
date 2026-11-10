# 02 — Branching Standards

**Applies to:** all branches in `ClaudeCodeBhaskar`
**Status:** Proposed for review

---

## 1. Branch Names

| Type | Pattern | Example | Cut from | Merged into | Used in Phase 1 |
|---|---|---|---|---|---|
| Protected | `main` | `main` | — | — | Yes |
| Integration | `development` | `development` | — | — | Yes |
| Module 4 implementation | `feature/module-4-implementation` | `feature/module-4-implementation` | `development` | `development` (PR) | Yes |
| Feature (later modules) | `feature/<kebab-topic>` | `feature/module-7-review-fixes` | `development` | `development` (PR) | As needed |
| Fix | `fix/<kebab-topic>` | `fix/prescription-print-margin` | `development` | `development` (PR) | As needed |
| Chore | `chore/<kebab-topic>` | `chore/update-gitignore` | `development` | `development` (PR) | As needed |
| Hotfix | `hotfix/<kebab-topic>` | `hotfix/print-crash` | `main` | `main` (PR), then back-merged to `development` | Only if a released build is broken |
| Release | `release/vX.Y.Z` | `release/v0.1.0` | `development` | `main` (PR) | Only if stabilisation is needed before release |

Documentation has no branch type. It is committed on `development` (see `01-Git-Strategy.md` §3.1). The `docs/` prefix is not used unless decision D-1 changes this.

## 2. Naming Rules

- Lower-case ASCII. Words separated by hyphens. No spaces, underscores or capitals.
- Start with the type, then `/`, then the topic: `<type>/<topic>`.
- Topic: 3 to 40 characters, describes the work, not the person.
- Module branches use `module-<n>-<topic>` so the module is visible in the name.
- No ticket or ID numbers in branch names. Requirement IDs go in commit messages.
- Do not reuse a branch name after its branch is merged and deleted.

## 3. Branch Lifetime

| Branch | Lifetime |
|---|---|
| `feature/module-4-implementation` | From the start of Module 4 until its PR is merged. Delete remote and local after merge |
| `fix/*`, `chore/*` | Until merged. Delete after merge |
| `hotfix/*` | Until merged to `main`. Delete after back-merge |
| `release/*` | Until merged to `main` and tagged. Delete after tag |
| `main`, `development` | Permanent. Never deleted |

## 4. Protection

Recommended settings on GitHub (decision D-6):

| Setting | `main` | `development` |
|---|---|---|
| Require pull request before merging | Yes | Yes (documentation commits excepted if D-1 says so) |
| Require approval before merging | Yes (1 approval) | Yes (1 approval) |
| Block force-push | Yes | Yes |
| Block branch deletion | Yes | Yes |
| Require status checks | When CI exists | When CI exists |

Repository administration is done by the Project Owner, not by Claude.

## 5. Local Branch Hygiene

- The primary checkout is on `development` for documentation work (decision D-5).
- Feature work runs in a worktree (see `07-Worktree-Standards.md`).
- Local branches are pruned after merge: `git branch -d <name>` (only after confirming it is merged).
- `git fetch --prune` before starting work.

## 6. Forbidden Patterns

| Pattern | Reason |
|---|---|
| Commits directly to `main` | Released code only |
| Force-push to `main`, `development` or a shared branch | Rewrites history others depend on |
| Long-lived branches off `feature/module-4-implementation` | Keeps Module 4 one reviewable unit |
| Branches named after people | Not a topic |
| Committing secrets, local settings or build output | See `03-Commit-Standards.md` §6 |
