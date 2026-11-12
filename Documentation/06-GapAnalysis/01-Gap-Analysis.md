# 06 — Gap Analysis

**Date:** 2026-11-12
**Status:** For Project Owner review. No fixes implemented.
**Compared:** BRD (`BRD/Doc_BRD.md`, amended 2026-11-12), RTM (`01-Brainstorming/12-Requirement-Traceability-Matrix.md`), planning matrix (`02-Planning/18-Planning-Traceability-Matrix.md`), current code on `pms-Implementation` at `6f52ec1`, review packages `04-Implementation/16-` and `17-`, acceptance record `04-Implementation/19-`, and verification report `05-Verification/01-`.
**Not assessed:** Won't Have items (DEC-025) and OOS-001 to OOS-015. Excluded checks G1 to G4 (owner decision, 2026-11-12) are not gaps here.

## 1. Coverage Summary

| Measure | Result |
|---|---|
| In-scope requirement IDs | 26 (17 Must FR, 4 Should FR, 3 Must NFR, 2 Must SC) |
| Missing (no implementation) | 0 |
| Partially implemented or partially evidenced | 1 (NFR-PRF-001) |
| Implemented with an automated test carrying the ID | 23 |
| Evidence browser-only or manual | 3 (NFR-PRF-001, SC-004, SC-005) |
| Untested (no automated or manual evidence) | 0 |
| Functionality with no BRD or planning record | 4 |
| Documentation updates missing | 6 (4 in section 2.5; 2 evidence records in section 2.3) |

Headline: every in-scope ID is implemented and has evidence. The gaps are timing evidence that predates the UI change, four behaviours that no BRD or planning document records, and stale or missing documentation.

## 2. Gap List

### 2.1 Missing requirements

None. Each of the 26 in-scope IDs has code, and each has an automated test or a manual check.

### 2.2 Partially implemented

| Req ID | Description | Impact | Recommended fix |
|---|---|---|---|
| NFR-PRF-001 | Page-load timing was recorded in M8 (Chrome 157.0.8103.52, `16-`). The DataTables library and the layout changes (`cb4f917`) came after that run. The M9 pass did not re-time the pages. | The timing may not describe the current pages. Impact low to medium: DataTables now loads on four screens. | Re-time the ten M8 pages on the current code (median of five, as in `16-`). Or the owner accepts the M8 timing as final. |

### 2.3 Untested, or evidence not recorded

| Req ID | Description | Impact | Recommended fix |
|---|---|---|---|
| NFR-SEC-001 | Forgot-password tests exist (`AccountControllerTests`, `NFR_SEC_001_forgot_password_*`, three tests). The acceptance record (`19-` §2) and the verification report (§3) do not cite them. | Low. The evidence exists but is not traced. | Add the three test names to the NFR-SEC-001 row in `19-` §2 and to the verification report §3. |
| FR-EXP-001, SC-005 | M12 passed, but the spreadsheet tool used to open the CSV is not recorded (`19-` §3, note column). | Low. The check passed; the record is incomplete. | Record the spreadsheet tool name and version in `19-` §3 M12, from the owner's report. |
| DEC-026 (date format, no ID) | No test sets a non-English culture for the dd-MMM-yyyy display (L7, accepted in M9). | Low. The code pins the invariant culture (`11f5fe4`). | Optional: add a culture test. Or keep L7 as accepted. |

### 2.4 Functionality not defined in the BRD

| Req ID | Description | Impact | Recommended fix |
|---|---|---|---|
| FR-APT-001, FR-APT-004 | Day guard: a slot must be on the day shown on the form (`AppointmentValidator.ValidateSlotDay`, added in `9e7f0c3`). No BRD or planning document records it. | A business rule exists in code without a record. CLAUDE.md §3 and §7 require rules to be recorded. | Owner decides: record as a decision (PDR or DEC) with its wording, or remove it. |
| FR-APT-001 | Past slots today stay bookable and reschedulable (DEC-028). The BRD is silent on past times. DEC-028 is in `14-Decision-and-Issue-Log`, not in the BRD decision log. | The scope boundary is not in the BRD. | Owner confirms the rule. Then add DEC-028 to the BRD decision log (CLAUDE.md §3). |
| FR-RX-002 (header) | Clinic details edit screen (`ClinicProfileController`). The BRD defines the header content and the placeholder logo (DEC-021), but not how the header is maintained. No requirement ID covers the screen. | Low. The screen is useful, but it has no traceable ID. | Trace it to FR-RX-002 in `18-`, or add one BRD line for clinic details. Owner decides. |
| DEC-026 (no ID) | DEC-026 names DataTables for the patient, consultation, medicine and prescription lists. Only the four existing lists use it (L2). The BRD has no requirement for those lists. | The decision and the build differ in wording. | Owner confirms that the three lists are not needed, then amend DEC-026. Or add a requirement. |

### 2.5 Documentation updates missing

| Req ID | File | Issue | Recommended fix |
|---|---|---|---|
| FR-APT-004 and the RTM header | `01-Brainstorming/12-` | The Result section (line 87) still says FR-APT-004 "is implemented and awaits approval". Line 82 says priorities are "proposed; pending approval". Line 4 says "proposed". The header says "Refreshed 2026-11-10". The verification report §8 did not list line 87 as corrected. | Update lines 4, 82 and 87 and the header date. |
| DEC-028 | `BRD/Doc_BRD.md` decision log | Not recorded (see 2.4). | After the owner confirms, add it. |
| UI review package | `04-Implementation/17-` §6 and §7 | §6 still says browser behaviour needs a Chrome check, and §7 item 3 is still open. Both were done in M15. §7 items 1 and 2 are accepted as L1 and L2 in `19-` §5 but are not marked. | Mark §6 and §7 as done or accepted. |
| PDR-31 | `README.md` | PDR-31 says the .NET and SQL Server versions are recorded in the README. The README has only the sqlcmd step. The versions are in `04-Implementation/01-Setup-Guide.md` only. | Add one line to the README that gives the versions and points to the setup guide. |

## 3. Recommended Actions

Ordered by what the owner needs to decide first. No code change is needed for items 1 to 3.

1. **Owner decisions on section 2.4.** Four behaviours need a record: the day guard, DEC-028 past slots, the clinic details screen, and the three unbuilt DEC-026 lists. For each, the owner records a decision or asks for removal.
2. **Re-time NFR-PRF-001** on the current code, or accept the M8 timing as final (section 2.2).
3. **Documentation corrections** in section 2.5 and the forgot-password and spreadsheet evidence in section 2.3. These are one `docs(docs)` commit on `pms-Implementation` after the owner's approval. BRD changes wait for the owner (CLAUDE.md §3).
4. **Optional:** add the culture test for the date display (section 2.3, L7).

No gap blocks the release PR on the evidence in this review. Items 1 and 3 need the owner's decision or approval before any change.

## 4. Owner Decisions (2026-11-12)

| Item | Decision | Action |
|---|---|---|
| NFR-PRF-001 (section 2.2) | Accept the M8 timing as final. No re-timing for this learning project | Recorded here. Nothing re-timed |
| Documentation corrections (section 2.5) | Approved | Applied in this change: RTM lines 4, 8, 82 and 87; UI review package §6 and §7; README status and versions (also stale, not in 2.5); verification and acceptance evidence for forgot password (section 2.3) |
| Day guard, DEC-028, clinic details screen, unbuilt DEC-026 lists (section 2.4) | Not answered | Open. The BRD decision log is not changed until the owner decides |
| M12 spreadsheet tool name (section 2.3) | Not answered | Open. Needs the owner's report of the tool and version |
| L7 culture test (section 2.3) | Not answered | Open. Optional |

## 5. Review Requested

1. Confirm the four functionality items in section 2.4, or name the ones to remove.
2. Confirm that NFR-PRF-001 is to be re-timed, or accepted as it stands.
3. Approve the documentation corrections in section 2.5.
