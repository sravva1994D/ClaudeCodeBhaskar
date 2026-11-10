# 04 — Review Process

**Applies to:** documentation changes, implementation pull requests, and release pull requests
**Reviewers:** Project Owner (sole approver, DEC-010). Mentor / Lead for Module 7 code review (STK-006)
**Status:** Proposed for review

---

## 1. Review Stages

| Stage | Who | Output | Gate |
|---|---|---|---|
| 1. Self-check | Claude | Checklist in the approval package | Must pass before stage 2 |
| 2. Approval package | Claude → Project Owner | Chat package (CLAUDE.md §5.1) | User approval before any Git write |
| 3. Pull request review | Project Owner | Approve, or request changes | One approval, per branch rules |
| 4. Code review (Module 7) | Mentor / Lead | Review comments | Comments resolved within the agreed timeline |
| 5. Release sign-off | Project Owner | Approved release PR | Required before merge to `main` |

## 2. Self-Check (stage 1)

| Item | Documentation | Implementation |
|---|---|---|
| Requirement IDs listed and correct | Yes | Yes |
| Traceability matrices updated (`12-`, `18-`) | If requirement status changed | Yes |
| BRD unchanged, or amendment proposed first (CON-017) | Yes | Yes |
| Decisions referenced (PDR or DEC) | If a decision is applied | If a decision is applied |
| Build passes | Not applicable | Yes |
| Tests pass | Not applicable | Yes |
| No secrets, no patient data, no build output in the diff | Yes | Yes |
| Commit messages follow `03-Commit-Standards.md` | Yes | Yes |

## 3. Approval Package (stage 2)

Contents as CLAUDE.md §5.1: change summary, files changed, requirements impacted, proposed commit message, exact Git commands. The package is shown in chat. Claude waits for approval.

Approval words are explicit. "Looks fine" is a request to ask again. "Approved, commit" is an approval.

## 4. Pull Request Review (stage 3)

### 4.1 PR template

Taken from CLAUDE.md §5.3:

```markdown
## Summary
<what and why>

## Requirements
- FR-XXX-000 — <Implemented | Partial | Modified | Removed>

## Traceability
- Matrices updated: yes / no
- BRD changed: yes / no

## Changes
- <area>: <change>

## Testing
- Unit: <result>
- Integration: <result>
- Manual / browser: <result and browser version>

## Risks and rollback
<notes; see 06-Rollback-Process.md>

## Checklist
- [ ] Builds
- [ ] Tests pass
- [ ] No secrets or patient data
- [ ] Docs updated
- [ ] Traceability matrices updated
```

The PR description ends with: `🤖 Generated with [Claude Code](https://claude.com/claude-code)`

### 4.2 What the reviewer checks

| Area | Check |
|---|---|
| Scope | Change traces to a requirement. Nothing outside the BRD |
| Layers | Controllers hold no rules. No EF Core in controllers. Web references Data only for DI |
| Rules | Matches the business rules table in CLAUDE.md §7 and the planning files |
| Data | Migration present for every entity change. No check constraint on vital ranges |
| Security | Passwords hashed. No secrets. Anti-forgery on POST |
| Tests | Each requirement ID has a test. Failing tests are fixed, not skipped |
| Traceability | Matrices show the new status |

### 4.3 Review outcomes

| Outcome | Action |
|---|---|
| Approve | Merge using the method in `01-Git-Strategy.md` §3 |
| Request changes | New commits on the same branch. Do not force-push |
| Reject | Close the PR. Record the reason in the decision log (DEC) |

## 5. Module 7 Code Review (stage 4)

- Mentor / Lead reviews the Module 4 PR (and any fix PRs) using §4.2.
- Review comments are logged with an ID (`RV-001`, `RV-002`, …) in the PR or in `Documentation/03-Worktree/` if the PR is not the place.
- Each comment is marked resolved, or deferred with a reason.
- Progress measure (training plan): comments resolved within the agreed timeline. The timeline is not yet agreed (decision D-8).

## 6. Documentation Review

Documentation commits on `development` are reviewed in the approval package (stage 2). If decision D-1 chooses a documentation PR, §4 applies to those PRs too.

Review focus for documentation:
- Requirement IDs and BRD wording are correct.
- Decisions are recorded with date and owner.
- No duplicate content; links instead of copies.
- Superseded items are marked, not deleted.

## 7. Turnaround

No fixed turnaround is set. The Project Owner is the only approver, so the review queue is short. Decision D-8 asks whether a target turnaround should be set for Module 7.
