# 07 — Worktree Standards

**Module:** 3 — Worktree (training plan)
**Expected outcome (training plan):** develop and test changes in a separate, controlled environment, without impact on the main branch
**Progress measure (training plan):** successful creation and management of worktrees; no changes made directly on `main`
**Status:** Amended 2026-11-10 for one working branch. Decision D-9 asks for approval of the approach.

---

## 1. Constraint

Git does not allow the same branch to be checked out in two worktrees. `development` is the only working branch, so it can be checked out in one place only: the primary checkout. A second worktree on `development` is not possible.

Worktrees are therefore used for **isolated verification of a commit**, not for parallel development. They use a detached HEAD, which creates no branch.

## 2. Layout

| Location | Checkout | Used for |
|---|---|---|
| `D:\Projects\AI\Claude\project-mgmt-system\ClaudeCodeBhaskar` (primary) | `development` | All development, documentation and commits |
| `D:\Projects\AI\Claude\project-mgmt-system\ClaudeCodeBhaskar-wt\verify-<name>` | Detached at a commit | Isolated build, test or check of a commit or tag |

The `-wt` folder sits beside the primary checkout, never inside it.

## 3. Development Rules

- Commits are made only in the primary checkout, on `development`.
- The primary checkout is not switched to `main` for editing.
- Before work starts: `git fetch origin` and `git pull --ff-only origin development`.

## 4. Create a Verification Worktree

Run from the primary checkout, after the commit to verify is pushed:

```
git worktree add --detach ../ClaudeCodeBhaskar-wt/verify-<short-sha> <sha>
```

- `--detach`: no branch is created.
- Use a commit SHA or a tag (for example `v0.1.0`) as `<sha>`.
- Run the build and tests inside that folder: `dotnet build`, `dotnet test`.
- Do not commit inside a detached worktree. Nothing made there is kept.

## 5. Remove a Verification Worktree

```
git worktree remove ../ClaudeCodeBhaskar-wt/verify-<short-sha>
git worktree prune
```

`git worktree remove` refuses if the folder has uncommitted changes. Do not force it without the user's approval (`06-Rollback-Process.md` §6).

## 6. Checks

| Check | How | Pass when |
|---|---|---|
| Primary checkout on `development` | `git branch --show-current` | Prints `development` |
| No other branches created | `git branch -a` | Only `development`, `main` and remote tracking branches |
| Verification worktree is detached | `git worktree list` | Shows `(detached HEAD)` for each verify folder |
| No stale worktrees | `git worktree list` after cleanup | Only the primary checkout remains |
| `main` unchanged by Module 3 work | `git rev-parse main` before and after | Unchanged unless a release was approved |

## 7. Module 3 Position

The Module 3 expected outcome is met in part:

| Expected outcome | Status under the single-branch rule |
|---|---|
| Develop and test in a separate, controlled environment | Partly. Verification is isolated in a detached worktree. Development happens in the primary checkout |
| Work without impact on the main branch | Met. `main` changes only by release PR |
| Create and manage worktrees | Met. Creation and removal as in §4 and §5 |

Decision D-9 asks whether this approach is acceptable for the training.

## 8. Windows Notes

- Use Git Bash for the commands above. Forward slashes work in Git commands.
- Close IDEs and build tools before removing a worktree, otherwise locked build folders can block removal.
- Keep the sibling folder name short to avoid path-length problems.
