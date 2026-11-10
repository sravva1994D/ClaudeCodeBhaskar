# 15 — Pre-Commit Decision Summary (refreshed)

**Project:** Patient Management System — Phase 1
**Refreshed:** 2026-11-10
**Status:** Historical record. Documentation was committed to `development` on 2026-11-10 after approval (D-1). `development` is the only working branch.

Superseded for Git: the single-branch instruction (2026-11-10) applies. The change set is on `development`.

---

## 1. Assumptions

| ID | Assumption | Status (2026-11-10) |
|---|---|---|
| A-001 | BRD (`BRD/Doc_BRD.md`) is the approved baseline | Accepted by requester, who is the sole decision-maker (DEC-010). BRD amended |
| A-002 | This is Module 1 of Phase 1; all artifacts align to it | Accepted by requester (DEC-002) |
| A-003 | Proposed ID scheme applied consistently across all artifacts | Accepted by requester (DEC-003) |
| A-004 | Open-question owners follow the BRD stakeholder list | Routed (`14-`). All questions answered |
| A-005 | Risk ratings are analyst judgement | For review (`08-`) |
| A-006 | MoSCoW priorities | Approved by the Project Owner 2026-11-10 |

## 2. Outstanding Approvals

| # | Item | Approver | Blocks |
|---|---|---|---|
| — | None. A-006 and PDR-08 were resolved on 2026-11-10 | — | — |

Everything else is decided. There are no open questions. The Project Owner is the sole decision-maker, so A-001 to A-003 need no separate approval.

## 3. Open Questions

None open. 28 of 28 are answered (24), deferred (3: OQ-017, OQ-018, OQ-019) or excluded (1: OQ-025). See `13-Open-Questions.md`.

## 4. Phase 1 Scope Impact

| Priority | Count | Scope effect |
|---|---|---|
| Must Have | 17 functional, 7 non-functional, 4 success criteria (verification) | Core Phase 1 |
| Should Have | 3 (FR-HIS-003, FR-SRC-002, FR-SRC-003) | Could be deferred without breaking the core consultation workflow |
| Won't Have (Phase 1) | 4 (NFR-REL-001, NFR-REL-002, SC-003, SC-006) + 15 out-of-scope items | Excluded by the BRD. Moving any into Phase 1 needs a BRD amendment |

## 5. Risks

Current status of every risk is in `08-Risk-Register.md`. Summary: 5 closed, 3 mitigated, 5 accepted by the Project Owner, 2 open (RSK-007 scope creep; RSK-014 unapproved priorities).

## 6. Change Set (local, uncommitted)

| Path | Change |
|---|---|
| `BRD/Doc_BRD.md` | Amended 2026-11-10: decided wording (PDR-01 to PDR-32), decision-log rows, PDF and print wording |
| `Documentation/01-Brainstorming/` (00 to 15) | Refreshed 2026-11-10 |
| `Documentation/02-Planning/` (00 to 18) | Module 2 planning, refreshed 2026-11-10 |

Untracked directories: `BRD/` (removed from the repository in commit `0b2e86f`) and `Reference/` (contains a binary `.docx`). Neither is committed. Whether they are tracked is a decision for the implementation branch.

## 7. Next Steps

1. Reviewer reviews Module 2. PDR-08 and A-006 were resolved on 2026-11-10.
2. Reviewer reviews Module 2 (`Documentation/02-Planning/00-Planning-Review-Package.md`).
3. Implementation starts when the user instructs it, committed directly to `development` after each approval.

---

**Nothing has been committed, branched, pushed or merged.**
