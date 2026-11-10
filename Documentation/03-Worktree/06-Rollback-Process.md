# 06 — Rollback Process

**Applies to:** `development` and `main` in `ClaudeCodeBhaskar`
**Principle:** history is never rewritten on a shared branch. A bad change is undone by a new commit (revert), not by deleting or rewriting history.
**Status:** Approved 2026-11-10 for the single working branch. Commands are reference only and run only with the user's approval where CLAUDE.md §5.1 requires it.

---

## 1. Decide Which Case Applies

| Case | Situation | Section |
|---|---|---|
| A | Mistake in a local commit that has not been pushed | §2 |
| B | Bad commit already pushed to `development` | §3 |
| C | Bad release on `main` | §4 |
| D | Database schema or data problem (local) | §5 |
| E | Commit made directly on `main` | §6 |

## 2. Case A: Unpushed Local Commit

| Goal | Command | Effect |
|---|---|---|
| Undo the last commit, keep the changes staged | `git reset --soft HEAD~1` | Changes stay staged |
| Undo the last commit, keep the files | `git reset HEAD~1` | Changes stay in the working tree |
| Discard the last commit and its changes | `git reset --hard HEAD~1` | **Destructive.** Only with the user's explicit approval, and only for commits that exist nowhere else |
| Fix the last commit message | `git commit --amend` | Only before push |

## 3. Case B: Bad Commit Already on `development`

Do not rewrite `development`. Add a revert commit:

```
git revert <bad-commit-sha>
```

The revert is committed and pushed, and reported in chat. The history keeps the mistake and its fix. Several commits are reverted in reverse order, or with `git revert --no-commit <oldest>..<newest>` followed by one commit.

## 4. Case C: Bad Release on `main`

Two options:

| Option | Steps | When |
|---|---|---|
| Revert the release | Revert the release merge on `main` through a PR from `development` (`git revert -m 1 <merge-sha>` on `development`, then PR). Re-tag the previous version as the current build | Keeps the release history |
| Fix forward | Fix on `development`, then release the next patch version (`vX.Y.(Z+1)`) | The fix is small and known |

After either option:

1. Check the previous tag works: `git switch --detach vX.Y.(Z-1)` in the primary checkout, then run the tests.
2. Return to `development`: `git switch development`.
3. Record the rollback in the decision and issue log (§8).

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
- A migration without a Down method is noted in the commit message.
- The seed generator is deterministic, so a rebuild gives the same result.
- Accepted risk: RSK-004, deferred under OOS-011.

## 6. Case E: Commit Made Directly on `main`

This must not happen. If it does:

1. Stop. Do not push.
2. If not pushed: undo it locally (§2), then make the change on `development`.
3. If pushed: revert it through a PR from `development` (or a commit on `development` that is included in the next release), and record it (§8).

## 7. Force-Push

Not allowed on `development` or `main`, with no exception. Use §3 or §4 instead.

## 8. Rollback Record

Each rollback is recorded in `Documentation/01-Brainstorming/14-Decision-and-Issue-Log.md` (action log):

| Field | Content |
|---|---|
| Date | YYYY-MM-DD |
| Case | A to E |
| Affected commit or tag | SHA or tag name |
| Reason | One line |
| Action taken | Command or PR |
| Verified by | Test or check run after rollback |
