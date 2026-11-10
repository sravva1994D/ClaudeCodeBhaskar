# 01 — Git Strategy

**Applies to:** Repository `ClaudeCodeBhaskar`, remote `origin` (GitHub)
**Status:** Approved 2026-11-10 (workflow; D-4, D-6, D-7 to D-11)

---

## 1. Branch Model

```
main
└── Final approved release

development
├── Documentation
├── Entity Framework Core
├── SQL Server
├── ASP.NET MVC
├── Testing
├── Bug Fixes
└── Final Validation
```

| Branch | Purpose | Who writes to it | Direct commit | Receives from |
|---|---|---|---|---|
| `development` | The only working branch. All documentation, code, tests and fixes | Claude, regularly | Yes | Commits made here |
| `main` | Final approved release only. Each release is a merge commit and a tag | One PR, from `development`, once the application is complete and tested | No | `development` |

No other branches are used: no feature, module, milestone, documentation or hotfix branches.

## 2. Rules

1. **Work directly in `development`.**
2. **Commit and push documentation and code changes regularly.** Commit format: `03-Commit-Standards.md`. Each commit is reported in chat with its files and message.
3. **Complete all features, then test the full application** in `development`. Fix defects in `development`.
4. **One PR, `development` → `main`,** once the application is complete and tested.
5. **Merge with a merge commit.** No squash and no rebase (D-11).
6. **Nothing is committed to `main` directly.**
7. **No force-push** to `development` or `main`. Mistakes are reverted by new commits (`06-Rollback-Process.md`).
8. **Every change is traceable.** Commit messages list requirement IDs and decision references.
9. **Tags and deletions need the user's approval** (CLAUDE.md §5.1).

## 3. Workflow

```
1. Work directly in development.
2. Commit and push documentation and code changes.
3. Complete all project features.
4. Test the full application.
5. Create one PR: development → main.
6. Merge using a merge commit.
```

### 3.1 Urgent fixes

Fixes are committed directly to `development` (D-10). They reach `main` in the next release PR. No hotfix branch is used.

### 3.2 Release

- The first release is `v0.1.0` (D-7). Each release is tagged on `main` after its merge.
- Release steps: `05-Release-Process.md`.

## 4. Current State

| Item | State |
|---|---|
| `development` | Pushed to `origin/development`. Latest commit is pushed after this update |
| `main` | `115126c`, unchanged. No release yet |
| Working branches | `development` only |
| Worktrees | None in use (D-9) |
| Tags | None. `v0.1.0` at first release |
| `Reference/` | Being tracked in Git (D-4) |

## 5. Not Required

| Item | Decision |
|---|---|
| Branch protection | Not required (D-6) |
| Detached worktrees and parallel verification | Not required (D-9) |
| Hotfix branch | Not required (D-10) |
| Formal review SLA | Not required (D-8) |

## 6. Decisions

See `00-Worktree-Review-Package.md` §3.
