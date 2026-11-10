# 06 — Rollback Process

**Applies to:** every branch and every release of `ClaudeCodeBhaskar`
**Principle:** history is never rewritten on a shared branch. A bad change is undone by a new commit (revert), not by deleting history.
**Status:** Proposed for review. Commands are shown for reference and are not run until approved.

---

## 1. Decide Which Case Applies

| Case | Where the change is | Section |
|---|---|---|
| A. Mistake in an unpushed local commit | Local branch or worktree | §2 |
| B. Bad commit already pushed to a feature branch | `origin/feature/module-4-implementation` | §3.1 |
| C. Bad merge into `development` | `development` | §3.2 |
| D. Bad release on `main` | `main` | §4 |
| E. Bad documentation commit on `development` | `development` | §3.3 |
| F. Database schema or data problem | Local database | §5 |
| G. Worktree problem | Worktree folder | §6 |
| H. Commit accidentally made on `main` or `development` directly | Shared branch | §7 |

## 2. Case A: Unpushed Local Commit

Before any push, a local commit can be changed. The user approves the command first.

| Goal | Command | Effect |
|---|---|---|
| Undo the last commit, keep the changes | `git reset --soft HEAD~1` | Changes stay staged |
| Undo the last commit, keep the files | `git reset HEAD~1` | Changes stay in the working tree |
| Discard the last commit and its changes | `git reset --hard HEAD~1` | **Destructive.** Use only with explicit approval |
| Fix the last commit message | `git commit --amend` | Only before push |

Use `git reset --hard` only after confirming with the user, and only on commits that exist nowhere else.

## 3. Cases B, C and E: Pushed Changes

### 3.1 Bad commit on the feature branch (case B)

Do not rewrite the pushed branch. Add a revert commit:

```
git revert <bad-commit-sha>
```

Approve, commit (with `Requirements:` footer), push. The history shows the mistake and its fix.

### 3.2 Bad merge into `development` (case C)

Revert the merge commit:

```
git revert -m 1 <merge-commit-sha>
```

Use a PR for this, so the revert is reviewed. The original feature branch can be re-opened later with the fix.

### 3.3 Bad documentation commit on `development` (case E)

```
git revert <bad-commit-sha>
```

Approve, then commit. The files return to their earlier content, and the revert is logged.

## 4. Case D: Bad Release on `main`

Two options. The Project Owner chooses:

| Option | Steps | When |
|---|---|---|
| Revert the release | PR `revert-release` → `main` that reverts the merge commit (`git revert -m 1 <merge-sha>`). Tag the previous version again as the active build | Fast, keeps the release history |
| Hotfix forward | `hotfix/<topic>` from `main`, fix, PR to `main`, tag patch version. Back-merge to `development` | The fix is small and known |

After either option:

1. Check out the previous tag to confirm it works: `git switch --detach vX.Y.(Z-1)`.
2. Run the tests against that tag.
3. Record the rollback in the decision log (DEC) with the reason.
4. Return to `development`: `git switch development`.

Never delete or move a published tag. Tags are immutable once pushed.

## 5. Case F: Database Problems

Phase 1 has no backups (OOS-011). Recovery is by rebuilding, not by restoring.

| Problem | Recovery |
|---|---|
| Bad migration applied locally | Apply the previous migration (`dotnet ef database update <previous-migration>`), if the migration has a working Down method. Otherwise drop and recreate the local database |
| Bad seed data | Re-run the seed script on a fresh database |
| Patient data lost locally | No recovery. Phase 1 holds synthetic test data only (CON-022). Recreate it with the seed generator |
| Schema drift between branches | Drop the local database. Apply migrations from the current branch |

Rules:
- Migrations are reviewed before they are merged. A migration without a Down method needs an explicit note in the PR.
- The seed generator makes the same data every time (deterministic), so a rebuild gives the same result.
- Accepted risk: RSK-004 (data loss from undefined backup and recovery), deferred under OOS-011.

## 6. Case G: Worktree Problems

| Problem | Command | Note |
|---|---|---|
| Worktree folder deleted by hand | `git worktree prune` | Removes the stale record |
| Worktree listed but folder missing | `git worktree prune` | Same |
| Need to remove a worktree | `git worktree remove <path>` then `git worktree prune` | Only after the branch is pushed or merged |
| Worktree has uncommitted work to keep | Commit or stash first, with approval | `git worktree remove` refuses if there are changes unless forced. Do not force without approval |

## 7. Case H: Commit Made Directly on a Protected Branch

This must not happen. If it does:

1. Stop. Do not push.
2. If not pushed: follow §2 (undo locally, with approval), then move the work to the correct branch.
3. If pushed to `main`: raise a PR that reverts the commit. Record the incident in the decision log.
4. If pushed to `development`: revert with §3.3.
5. Check GitHub branch protection (decision D-6) so it cannot happen again.

## 8. Force-Push

- Not allowed on `main`, `development` or any branch others use.
- A feature branch that only Claude has used may be force-pushed after the user approves, and only to fix history before review. Record the reason in the PR.

## 9. Rollback Record

Every rollback gets an entry in `Documentation/01-Brainstorming/14-Decision-and-Issue-Log.md` (action log) with:

| Field | Content |
|---|---|
| Date | YYYY-MM-DD |
| Case | A to H |
| Affected commit, branch or tag | SHA or name |
| Reason | One line |
| Action taken | Command or PR |
| Verified by | Test or check run after rollback |
