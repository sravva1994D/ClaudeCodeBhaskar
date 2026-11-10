# 06 — Rollback Process

**Applies to:** `development` and `main` in `ClaudeCodeBhaskar`
**Principle:** history is never rewritten on a shared branch. A bad change is undone by a new commit (revert), not by deleting or rewriting history.
**Status:** Amended 2026-11-10 for one working branch. Commands are shown for reference and are not run until approved.

---

## 1. Decide Which Case Applies

| Case | Situation | Section |
|---|---|---|
| A | Mistake in a local commit that has not been pushed | §2 |
| B | Bad commit already pushed to `development` | §3 |
| C | Bad release on `main` | §4 |
| D | Database schema or data problem (local) | §5 |
| E | Worktree problem | §6 |
| F | Commit made directly on `main` | §7 |

## 2. Case A: Unpushed Local Commit

Before any push, a local commit can be changed, with the user's approval first.

| Goal | Command | Effect |
|---|---|---|
| Undo the last commit, keep the changes staged | `git reset --soft HEAD~1` | Changes stay staged |
| Undo the last commit, keep the files | `git reset HEAD~1` | Changes stay in the working tree |
| Discard the last commit and its changes | `git reset --hard HEAD~1` | **Destructive.** Only with explicit approval, and only for commits that exist nowhere else |
| Fix the last commit message | `git commit --amend` | Only before push |

## 3. Case B: Bad Commit Already on `development`

Do not rewrite `development`. Add a revert commit:

```
git revert <bad-commit-sha>
```

The revert is shown in the approval package (CLAUDE.md §5.1), approved, committed and pushed. The history keeps the mistake and its fix.

If several commits must be reverted, revert them in reverse order, or use `git revert --no-commit <oldest>..<newest>` followed by one commit.

## 4. Case C: Bad Release on `main`

Two options. The Project Owner chooses:

| Option | Steps | When |
|---|---|---|
| Revert the release | Revert the release merge on `main` through a release PR (`git revert -m 1 <merge-sha>`). Re-tag the previous version as the current build | Fast, keeps the release history |
| Fix forward | Fix on `development` (approval gate), then release the next patch version (`vX.Y.(Z+1)`) | The fix is small and known |

After either option:

1. Check out the previous tag to confirm it works, in a detached worktree (`07-Worktree-Standards.md` §4): `git worktree add --detach ../ClaudeCodeBhaskar-wt/verify-<tag> <tag>`.
2. Run the tests against that tag.
3. Record the rollback in the decision and issue log (§9).
4. Remove the verification worktree when done.

Published tags are never deleted or moved.

## 5. Case D: Database Problems

Phase 1 has no backups (OOS-011). Recovery is by rebuilding, not by restoring.

| Problem | Recovery |
|---|---|
| Bad migration applied locally | Apply the previous migration (`dotnet ef database update <previous-migration>`), if it has a working Down method. Otherwise drop and recreate the local database |
| Bad seed data | Re-run the seed script on a fresh database |
| Local data lost | No recovery from backup. Phase 1 holds synthetic test data only (CON-022). Recreate it with the seed generator |
| Schema differs from the code | Drop the local database. Apply the migrations in `development` |

Rules:
- A migration without a Down method is noted in the approval package before it is committed.
- The seed generator is deterministic, so a rebuild gives the same result.
- Accepted risk: RSK-004, deferred under OOS-011.

## 6. Case E: Worktree Problems

| Problem | Command | Note |
|---|---|---|
| Worktree folder deleted by hand | `git worktree prune` | Removes the stale record |
| Need to remove a worktree | `git worktree remove <path>` then `git worktree prune` | Only after its verification is finished |
| Worktree has uncommitted changes | Do not force removal | `git worktree remove` refuses. Resolve with the user first |

## 7. Case F: Commit Made Directly on `main`

This must not happen. If it does:

1. Stop. Do not push.
2. If not pushed: undo it locally (§2), then make the change on `development`.
3. If pushed: raise a release PR from `development` that reverts the commit, and record the incident (§9).
4. Branch protection is not required (decision D-6). The approval gate is the control.

## 8. Force-Push

Not allowed on `development` or `main`, with no exception. Use §3 or §4 instead.

## 9. Rollback Record

Each rollback is recorded in `Documentation/01-Brainstorming/14-Decision-and-Issue-Log.md` (action log):

| Field | Content |
|---|---|
| Date | YYYY-MM-DD |
| Case | A to F |
| Affected commit or tag | SHA or tag name |
| Reason | One line |
| Action taken | Command or PR |
| Verified by | Test or check run after rollback |
