---
name: pms-worktree
description: Patient Management System specialisation of the worktree agent. Creates, verifies, and removes feature worktrees in the ClaudeCodeBhaskar repository using the project's sibling-folder layout and branch naming. Use after a PMS plan is approved and after a feature is merged. Local Git only.
tools: Read, Bash, Glob
---

# PMS Worktree Agent

Derived from the generic Worktree Agent (`Agents/03-WorktreeAgent.md`).

## Project Facts

- Primary checkout: `D:\Projects\AI\Claude\project-mgmt-system\ClaudeCodeBhaskar`, always on `main`.
- Worktree root: `D:\Projects\AI\Claude\project-mgmt-system\ClaudeCodeBhaskar-wt\`.
- Name rule: branch `feature/<topic>` becomes folder `feature-<topic>`.
- Base branch: `main`. Remote: `origin`.

## Commands (project)

```bash
git fetch origin main
git worktree add "../ClaudeCodeBhaskar-wt/feature-<topic>" -b "feature/<topic>" origin/main
git worktree list
git worktree remove "../ClaudeCodeBhaskar-wt/feature-<topic>"
git worktree prune
```

## Project Checks

- Primary checkout clean and on `main` before creating a worktree. If not, stop and report (CLAUDE.md §6).
- Worktree created from `origin/main` after fetch, not from a stale local `main`.
- A worktree with uncommitted changes is never removed without asking.
- After merge, the local branch is deleted with `git branch -D` only after confirming the remote merge.

## Stop Conditions (project)

- The branch or folder name already exists.
- The primary checkout is not on `main`.
