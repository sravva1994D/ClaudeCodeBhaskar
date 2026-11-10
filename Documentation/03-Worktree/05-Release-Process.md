# 05 — Release Process

**Applies to:** releases of the Patient Management System to `main`
**Status:** Proposed for review

---

## 1. Versioning

Semantic Versioning: `vMAJOR.MINOR.PATCH`.

| Part | Meaning in this project |
|---|---|
| MAJOR | Incompatible change to stored data or to a released workflow. Not expected in Phase 1 |
| MINOR | A group of requirements delivered (for example, Module 4 complete) |
| PATCH | Fix to a released build, usually from a hotfix |

Phase 1 starts at `v0.1.0` (decision D-7). Every tag lists the requirement IDs it delivers.

## 2. Release Points

A release is cut only when all of these are true:

- Module 4 implementation is merged to `development`.
- Module 5 (Verification) has run the tests and recorded results.
- Module 6 (Gap Analysis) shows no unplanned gaps for the release scope (`18-Planning-Traceability-Matrix.md`).
- The Project Owner has signed off the release PR.

## 3. Release Steps

| Step | Action | Who | Result |
|---|---|---|---|
| 1 | Confirm `development` is green: build and tests pass | Claude | Test results recorded in the PR |
| 2 | Update the release notes draft (`Documentation/03-Worktree/releases/vX.Y.Z.md`) with requirement IDs delivered and known gaps | Claude | Draft ready for review |
| 3 | Raise PR `development` → `main` with the release template (§4) | Claude, after approval | PR open |
| 4 | Review and sign-off | Project Owner | Approved |
| 5 | Merge with a merge commit (no squash, so `development` history is kept on `main`) | Claude, after approval | `main` updated |
| 6 | Tag `main`: `vX.Y.Z` with an annotated tag | Claude, after approval | Tag exists locally and on `origin` after approval |
| 7 | Confirm `main` and `development` point to the same commit or that `development` only adds documentation | Claude | Recorded in the release note |
| 8 | Delete `release/*` branch if one was used | Claude, after approval | Branch removed |

Git commands for steps 5 and 6 are shown in §5. They are not run until approved.

## 4. Release PR Template

```markdown
## Release vX.Y.Z

### Requirements delivered
- FR-XXX-000 — Implemented / Verified

### Known gaps
- <requirement> — <reason> — <deferred to>

### Verification
- Build: <result>
- Tests: <result, count>
- Browser matrix (Chrome, Edge, Safari): <versions>
- Performance (median of 5 runs): <results against NFR-PRF targets>

### Rollback
See Documentation/03-Worktree/06-Rollback-Process.md. Previous tag: vX.Y.(Z-1)

### Sign-off
- [ ] Project Owner
```

## 5. Reference Commands (not run)

```
git switch main
git merge --no-ff development -m "release: vX.Y.Z"
git tag -a vX.Y.Z -m "Release vX.Y.Z: <summary>"
git push origin main
git push origin vX.Y.Z
```

These run only after the release PR is approved and the user has approved the Git commands (CLAUDE.md §5.1). Where possible, the merge happens through the PR on GitHub, not on the command line.

## 6. Release Checklist

| Item | Required | Source |
|---|---|---|
| All in-scope requirement tests pass | Yes | `14-Testing-Strategy.md` |
| Traceability shows no unplanned gaps | Yes | `18-Planning-Traceability-Matrix.md` |
| Browser matrix run on latest Chrome, Edge and Safari (desktop) | Yes | NFR-CMP-001; PDR-28 |
| Performance measured on 1,000 seeded patients | Yes | NFR-PRF-001, NFR-PRF-002; PDR-30 |
| Encryption checks (TDE, HTTPS beyond localhost) | Yes | NFR-SEC-002; PDR-21 |
| Backup and restore verified | **No.** Deferred | NFR-REL-001 and NFR-REL-002 are deferred (OOS-011). Risk accepted: RSK-004 |
| Release notes list requirement IDs | Yes | CLAUDE.md §4 |
| No secrets or patient data in the release | Yes | CLAUDE.md §9 |
| Setup guide (`README.md`) matches the release | Yes | CLAUDE.md §11 |

Note: the earlier generic release standard required backup verification. It is removed here because backups are out of Phase 1 (OOS-011).

## 7. Deployment

Phase 1 is localhost only (CON-022). A release means a tagged build that runs on the developer machine. No server or cloud deployment is part of this process. If the project is ever deployed beyond localhost, a new decision and a new version of this process are needed first.

## 8. Release Notes Template

File: `Documentation/03-Worktree/releases/vX.Y.Z.md`

```markdown
# vX.Y.Z — <date>

## Delivered
- FR-XXX-000 — <summary>

## Changed
- <area>: <change>

## Known gaps
- <requirement>: <reason>

## Upgrade notes
- Migrations: <list, or none>
- Seed data: <changed or not>

## Rollback
Previous tag: vX.Y.(Z-1). See 06-Rollback-Process.md.
```
