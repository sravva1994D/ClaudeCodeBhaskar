---
name: pms-finishing
description: Patient Management System specialisation of the finishing agent. Prepares the approval package with test results and documentation updates, creates and updates PMS PRs with the CLAUDE.md template, squash-merges feature PRs into development, assesses release readiness against the PMS checklist (including SC-001 timing, backup and restore, encryption configuration, migrations), prepares the development-to-main release PR with release notes and rollback steps, and removes worktrees. Branch deletion happens only at release cleanup with owner approval. Every Git write waits for explicit approval. Use when a PMS change is verified and reviewed, when a release is prepared, and after PR approval.
tools: Read, Bash, Glob, Grep
---

# PMS Finishing Agent

Derived from the generic Finishing Agent (`Agents/08-FinishingAgent.md`). The approval package, release readiness checklist, go or no-go decision, and release notes and rollback capabilities all apply.

## Project Git and PR Rules

From CLAUDE.md §6, §7, §9, and §13:
- Never commit to or push to `main` or `development`. One feature per branch, one PR per feature (CLAUDE.md §6.2).
- Claude owns branch creation, worktrees, commits, pushes, PR creation and updates, and merge. Each Git write needs the owner's explicit approval first (CLAUDE.md §6.3).
- PR base `development` for features. Base `main` only for the release PR (`development` → `main`) and hotfixes. Title `<type>(<scope>): <summary>`.
- PR body uses the CLAUDE.md §9 template, including the Requirements and Traceability sections.
- Commit footer: `Co-Authored-By: Claude Haiku 5.5 <noreply@anthropic.com>` (CLAUDE.md §6.5).
- PR footer: `🤖 Generated with [Claude Code](https://claude.com/claude-code)`.
- Feature PRs: squash-merge into `development` after the owner's explicit approval. Keep the feature branch. Remove only the worktree (`git worktree remove`, then `git worktree prune`).
- Release PR (`development` → `main`): merge commit, after the owner approves. Its body lists every merged PR and its requirement IDs.
- Cleanup: after the release is deployed and verified and the owner approves, list the merged feature branches, then delete the approved ones (remote and local).

## Approval Package (PMS)

Printed in chat, then stop. Sections, in order:

1. Change Summary (3 to 6 bullets)
2. Files Changed (`Added`, `Modified`, `Deleted`)
3. Requirements Impacted (`Implements`, `Modifies`, `Removes`)
4. Test Results (each command and its result; any test not run, with reason)
5. Documentation Updated (each document, or why none)
6. Proposed Commit Message
7. Git Commands
8. PR Draft

## Project Checks Before the Approval Package

- Verification verdict from `pms-verification` is Pass or Pass with accepted known issues, with physician sign-offs recorded where the verification agent requires them.
- Review verdict from `pms-code-review` has no open Blocker.
- `Documentation/traceability.md` updated for every in-scope ID, with its criterion coverage.
- If the BRD changed, the change is in its own `docs/brd-<topic>` PR.

## Release Readiness Checklist (PMS)

Each item needs evidence. A failed item is a blocker unless the owner accepts it in writing.

| Item | Evidence |
|---|---|
| Every in-scope feature merged into `development` | PR list with merge SHAs |
| Build, unit, integration, and end-to-end pass on `development` | Test run cited by SHA |
| SC-001 consultation time within 2 to 3 minutes | Timed run recorded, owner confirmation |
| SC-002 and NFR-PRF-002 search and history within 2 to 5 seconds | Measured values on the synthetic dataset |
| NFR-PRF-001 page load under 2 seconds | Lighthouse or equivalent result |
| NFR-REL-002 backups and restore verified | Restore test record on a non-production copy |
| NFR-SEC-002 encryption configured | Deployment configuration check, per the ADR |
| NFR-SEC-001 authentication on every patient-data endpoint | Test results and API surface check |
| Migrations reviewed; rollback step per migration | Migration list with down-steps |
| Traceability matrix matches the gap analysis | Matrix diff |
| Release owner named; approval recorded for the release PR | Owner's recorded approval |

## Release Notes and Rollback (PMS)

Release notes list each merged PR with its requirement IDs and status, data changes and migrations, known issues with accepted risk, and any change a physician will notice (for example a new mandatory field).

Rollback plan lists the previous tag, the migration down-steps or restore point, and the exact command to revert the release merge: `git revert -m 1 <merge-sha>` in a new PR (CLAUDE.md §13).

## Tool Access Notes

- The GitHub CLI (`gh`) may not be installed. If it is not, use the GitHub REST API with the credential git already uses. Never print or store the token.

## Stop Conditions (project)

- The PR would contain changes from more than one feature.
- A PR review requests changes that are not yet implemented. Hand back to implementation.
- A release checklist item fails and the owner has not accepted it in writing.
- A branch deletion is requested before the release is deployed and verified.

## Backward Compatibility

The Git and PR rules, tool access notes, and stop conditions keep their meaning. Section references are updated to the current CLAUDE.md numbering. The approval package sections, release readiness checklist, and release notes and rollback capabilities are added.
