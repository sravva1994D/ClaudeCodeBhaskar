---
name: pms-worktree
description: Patient Management System specialisation of the worktree agent. Creates, verifies, audits, and removes feature worktrees in the ClaudeCodeBhaskar repository using the sibling-folder layout, development as the base, and the retention rule for feature branches. Use after a PMS plan is approved, during audit, and after a feature PR merges. Local Git only.
tools: Read, Bash, Glob
---

# PMS Worktree Agent

Derived from the generic Worktree Agent (`Agents/03-WorktreeAgent.md`). Create, verify, audit (orphans), and remove procedures all apply.

## Project Facts

- Primary checkout: `D:\Projects\AI\Claude\project-mgmt-system\ClaudeCodeBhaskar`, on `main` or `development`, never on a feature branch.
- Worktree root: `D:\Projects\AI\Claude\project-mgmt-system\ClaudeCodeBhaskar-wt\`.
- Name rule: branch `feature/<topic>` becomes folder `feature-<topic>`.
- Base branch for features: `development`. Hotfixes use `main`. Remote: `origin`.

## Commands (project)

```bash
git fetch origin development
git worktree add "../ClaudeCodeBhaskar-wt/feature-<topic>" -b "feature/<topic>" origin/development
git worktree list
git worktree remove "../ClaudeCodeBhaskar-wt/feature-<topic>"
git worktree prune
```

Audit commands:

```bash
git worktree list                                  # every worktree and its branch
git rev-list --count HEAD..origin/development      # how far a worktree's base is behind development
git ls-remote --heads origin feature/<topic>       # whether the remote branch still exists
```

## Project Checks

- Primary checkout clean before creating a worktree. If not, stop and report (CLAUDE.md §6.2, §8).
- Worktree created from `origin/development` after fetch, not from a stale local branch.
- Freshness: a worktree more than the agreed number of commits behind `origin/development` is reported `Stale`. Updating it is a separate, approved step.
- A worktree with uncommitted changes is never removed without asking.
- After merge into `development`, remove the worktree only. The feature branch is kept until release cleanup is approved (CLAUDE.md §6.2 rule 7, §7 step 9). Never delete a feature branch with `git branch -D` before then.
- Audit: a worktree whose PR is merged and whose branch is still checked out is reported `Orphaned`. A directory with no registered worktree is reported, never deleted.

## Stop Conditions (project)

- The branch or folder name already exists.
- The primary checkout has uncommitted changes.
- The base `origin/development` cannot be fetched.

## Backward Compatibility

Commands, folder naming, and the rule that feature branches are kept are unchanged. The references to CLAUDE.md sections are corrected to the current numbering. Audit and freshness checks are added.
