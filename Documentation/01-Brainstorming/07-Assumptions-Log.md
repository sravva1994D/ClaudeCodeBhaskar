# 07 — Assumptions Log

**Rule for this module:** No assumptions. Where the BRD is silent, the gap is logged as an Open Question (`13-Open-Questions.md`), not filled in.
**Status:** Refreshed 2026-11-10

The entries below are the only items that had to be stated to frame this analysis. Each needs confirmation.

## 1. Assumptions Made to Frame the Analysis

| ID | Assumption | Basis | Impact if Wrong | Confirmed By | Status |
|---|---|---|---|---|---|
| A-001 | `BRD/Doc_BRD.md` is the approved baseline for this implementation | Accepted by requester, who is the sole decision-maker (DEC-010). BRD amended 2026-11-10. | All artifacts must be re-baselined | STK-003 (Product Owner) | Accepted by requester (sole decision-maker, DEC-010) |
| A-002 | This work is Module 1 (Brainstorming) of Phase 1; all planning, traceability and implementation artifacts align to that scope | Accepted by requester. Training plan: Module 1 = Brainstorming. | Wrong module outputs | STK-006 (Mentor / Lead) | Accepted by requester (sole decision-maker, DEC-010) |
| A-003 | Requirement IDs use the proposed ID scheme (originally in the deleted `CLAUDE.md` §4.1, recovered from git history commit `c3bb2ec`), applied consistently across requirements, acceptance criteria, decisions, risks, assumptions and traceability | Accepted by requester. Renumbering is not allowed once IDs are in use. | IDs would need renumbering | STK-003 | Accepted by requester (sole decision-maker, DEC-010) |
| A-004 | Owners suggested for open questions (Clinic Owner, Product Owner, General Physician) follow the BRD stakeholder list | BRD: Users and Stakeholders | Questions routed to wrong person | STK-003 | Routed 2026-11-10; owners recorded in the decision and issue log |
| A-005 | Risk likelihood and impact ratings in the Risk Register are analyst judgement | Standard risk practice | Risk ranking changes; no requirement change | STK-006 | Judgement; for review |
| A-006 | MoSCoW priorities proposed in the BRD ("Requirement Priorities") | Requested by requester; ratings are analyst proposals based on whether the BRD lists the item as Phase 1 scope or a stated requirement | Scope boundary changes if priorities change; Won't Have items would become Phase 1 scope | STK-003 | Approved by the Project Owner 2026-11-10 (sole approver) |

## 2. Items Deliberately NOT Assumed (Logged as Open Questions)

All open questions in this table are now answered, deferred or excluded (see `13-Open-Questions.md`).

| Topic | Why Not Assumed | Open Question |
|---|---|---|
| Whether "consultation" and "visit" are the same entity | BRD uses both terms | OQ-028 |
| Mandatory patient fields at registration | BRD lists fields but not which are required | OQ-002 |
| Contact detail fields | BRD says "Contact details" only | OQ-001 |
| Vitals units and whether vitals can be skipped | Not stated | OQ-007 |
| Authentication method and session rules | Not stated | OQ-015 |
| Encryption standard | Not stated | OQ-016 |
| Backup frequency, retention, and restore targets | Not stated | OQ-018, OQ-019 |
| Browser versions | Not stated | OQ-022 |
| Patient volume | "Moderate" not defined | OQ-023 |
| Data retention, deletion, consent, and applicable regulation | Not in BRD | OQ-017 |
| Requirement priorities (MoSCoW or otherwise) | Not in BRD | MoSCoW in the BRD, approved by the Project Owner 2026-11-10 (A-006). |

## 3. Assumptions Carried Forward to Later Modules

None. Module 2 (Planning) is complete for review. All cited open questions are closed (2026-11-10).
