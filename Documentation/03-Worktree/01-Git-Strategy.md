# 01 — Git Strategy

**Module:** 3 — Worktree (training plan)
**Applies to:** Repository `ClaudeCodeBhaskar`, remote `origin` (GitHub)
**Status:** Approved workflow, 2026-11-10 (Project Owner)

---

## 1. Branch Model

```
main
  └── Final approved code only

development
  ├── Documentation updates
  ├── Database implementation
  ├── Entity Framework Core implementation
  ├── ASP.NET MVC implementation
  ├── Testing and bug fixes
  └── Final project stabilization
```

| Branch | Purpose | Who writes to it | Direct commit | Receives from |
|---|---|---|---|---|
| `development` | The only working branch. All documentation, planning, database, EF Core, ASP.NET MVC, testing and fixes | Claude, regularly, under the workflow in §3 | Yes | Commits made here |
| `main` | Final approved code only. Each release is a tagged merge commit | One PR, from `development`, after final approval | No | `development` |

No other branches are created.

## 2. Rules

1. **Work directly in `development`.** No feature, module, milestone, documentation, fix, chore or hotfix branches.
2. **Commit and push regularly.** Each commit uses `03-Commit-Standards.md`. Each commit is reported in chat with its files and message.
3. **`main` changes only through the single PR** from `development`, after final review and approval (§3.2).
4. **Nothing is committed to `main` directly.**
5. **No force-push** to `development` or `main`. Mistakes are reverted by new commits (`06-Rollback-Process.md`).
6. **Every change is traceable.** Commit messages list requirement IDs and decision references.
7. **No merge, tag or deletion** without the Project Owner's approval (CLAUDE.md §5.1).

## 3. Workflow (approved 2026-11-10)

### 3.1 Development

```
1. Work directly in development.
2. Commit and push changes regularly.
3. Complete all project features.
4. Test the entire application.
5. Fix defects in development.
6. Review the final solution.
```

Each commit builds (implementation) and passes its checks before it is pushed. Verification of a specific commit can use a detached worktree (`07-Worktree-Standards.md`).

### 3.2 Release to main

```
7. Create a single PR from development to main.
8. Merge into main after final approval.
```

The PR uses the template in `05-Release-Process.md` §4. The merge is a merge commit (no squash), so the release point and its history stay visible on `main`. A tag is created on `main` after the merge (`05-` §3).

### 3.3 Urgent fixes

An urgent fix is committed to `development` and released in the next release (decision D-10 asks for approval of this). There is no hotfix branch.

## 4. Current State (observed 2026-11-10)

| Item | State |
|---|---|
| `development` | Pushed to `origin/development`, latest commit `1769e60` (single-branch amendment) |
| `main` | `115126c`, unchanged. No release yet |
| Working branches | `development` only |
| Worktrees | Primary checkout on `development` |
| Tags | None |
| Untracked | `Reference/` (binary `.docx`; decision D-4, pending) |

## 5. Gaps and Actions

| Gap | Action | Owner |
|---|---|---|
| `main` has no release | First release at the end of the workflow in §3.1 | Project Owner approval (D-7 for the version) |
| `Reference/` untracked | Decide whether to track the binary `.docx` | Project Owner |
| Development isolation | Detached worktrees for verification (`07-Worktree-Standards.md`) | Project Owner approval (D-9) |

Branch protection is not required (D-6, 2026-11-10).

## 6. Decisions

See `00-Worktree-Review-Package.md` §3.
