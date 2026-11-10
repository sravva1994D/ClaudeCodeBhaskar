# 07 — Worktree Standards

**Module:** 3 — Worktree (training plan)
**Expected outcome (training plan):** develop and test changes in a separate, controlled environment, without impact on the main branch
**Progress measure (training plan):** successful creation and management of worktrees; no changes made directly on `main`
**Status:** Proposed for review

---

## 1. Principle

Each implementation branch is checked out in its own worktree. The primary checkout is used for reading, reviewing and documentation. `main` is never edited directly.

## 2. Layout

| Folder | Branch | Purpose |
|---|---|---|
| `D:\Projects\AI\Claude\project-mgmt-system\ClaudeCodeBhaskar` (primary) | `development` | Documentation and review. Not used for implementation |
| `D:\Projects\AI\Claude\project-mgmt-system\ClaudeCodeBhaskar-wt\feature-module-4-implementation` | `feature/module-4-implementation` | Module 4 implementation |

Sibling folder name: the branch name with `/` replaced by `-`. The `-wt` folder sits beside the primary checkout, so it is never inside the repository.

## 3. Create

Run from the primary checkout, after confirming `development` is current:

```
git fetch origin
git switch development
git pull --ff-only origin development
git worktree add ../ClaudeCodeBhaskar-wt/feature-module-4-implementation -b feature/module-4-implementation development
```

Rules:
- `--ff-only`: never create a merge commit on `development` by accident.
- The branch is cut from `development`, never from `main`.
- Creation is done after the approval gate (CLAUDE.md §5.1).

## 4. Work

- Open the worktree folder as the working directory for all implementation commands.
- Run `dotnet build` and `dotnet test` inside the worktree.
- Commit only in the worktree.
- Push from the worktree: `git push -u origin feature/module-4-implementation` (after approval).
- Keep the worktree up to date with `development` by rebasing or merging only with approval. Prefer a merge from `development` if others have changed it; never force-push.

## 5. List and Inspect

```
git worktree list
git status          (run inside the worktree folder)
```

Each worktree is listed with its path, commit and branch. A worktree whose branch is merged is removed (§7).

## 6. Rules

| Rule | Reason |
|---|---|
| One branch per worktree | Git does not allow the same branch in two worktrees |
| Never run implementation in the primary checkout while it is on `main` | Keeps `main` clean |
| Never check out `main` in a worktree for editing | `main` changes only through release PRs |
| Do not commit build output or local settings | See `03-Commit-Standards.md` §6 |
| Worktree folder is never inside the repository folder | Avoids nested repositories |
| Remove a worktree after its branch is merged | Keeps the folder list short |

## 7. Remove

After the feature branch is merged to `development` and deleted:

```
git worktree remove ../ClaudeCodeBhaskar-wt/feature-module-4-implementation
git worktree prune
git branch -d feature/module-4-implementation
```

`git worktree remove` refuses to remove a folder with uncommitted changes. Do not force it without approval (see `06-Rollback-Process.md` §6).

## 8. Checks for the Worktree Module (Module 3)

| Check | How | Pass when |
|---|---|---|
| Worktree created from `development` | `git worktree list`; `git log -1 feature/module-4-implementation` | Path and branch are as in §2; branch starts at the `development` commit |
| No changes on `main` | `git rev-parse main` before and after | Unchanged |
| Worktree removed after merge | `git worktree list` after cleanup | Only the primary checkout remains |
| Prune run after manual folder deletion | `git worktree prune` | No stale entries |

## 9. Windows Notes

- Paths use backslashes in Windows tools and forward slashes in Git commands. Both work from Git Bash.
- Keep the total path length short. Use the sibling folder name given in §2.
- Antivirus or sync tools that lock build folders can block `git worktree remove`. Close IDEs and build tools first.
