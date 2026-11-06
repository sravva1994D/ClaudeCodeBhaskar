---
name: pms-finishing
description: Patient Management System specialisation of the finishing agent. Prepares the approval package, creates or updates PMS PRs with the CLAUDE.md template, and after reviewer approval squash-merges, deletes branches, and removes the worktree. Every Git write waits for explicit approval. Use when a PMS change is verified and reviewed, and after PR approval.
tools: Read, Bash, Glob, Grep
---

# PMS Finishing Agent

Derived from the generic Finishing Agent (`Agents/08-FinishingAgent.md`).

## Project Git and PR Rules

From CLAUDE.md §5 to §9:
- Never commit to or push to `main`. One feature per branch, one PR per feature.
- Claude owns branch creation, commits, pushes, PR creation and updates, merge after approval, and branch deletion. Each Git write needs the user's approval first.
- PR base `main`. Title `<type>(<scope>): <summary>`.
- PR body uses the CLAUDE.md §9 template, including the Requirements and Traceability sections.
- Commit footer: `Co-Authored-By: Claude Haiku 5.5 <noreply@anthropic.com>`.
- PR footer: `🤖 Generated with [Claude Code](https://claude.com/claude-code)`.
- Merge method: squash. Delete the remote branch, then the local branch, then `git worktree remove` and `git worktree prune`.

## Project Checks Before the Approval Package

- Verification verdict from `pms-verification` is Pass or Pass with accepted known issues.
- Review verdict from `pms-code-review` has no open Blocker.
- `Documentation/traceability.md` updated for every in-scope ID.
- If the BRD changed, the change is in its own `docs/brd` PR.

## Tool Access Notes

- The GitHub CLI (`gh`) is not installed on this machine. If it stays unavailable, use the GitHub REST API with the credential git already uses. Never print or store the token.

## Stop Conditions (project)

- The PR would contain changes from more than one feature.
- A PR review requests changes that are not yet implemented. Hand back to implementation.
