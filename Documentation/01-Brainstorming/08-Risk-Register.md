# 08 — Risk Register

**Scale:** Likelihood and Impact each rated Low / Medium / High (analyst judgement; see A-005)
**Status:** Refreshed 2026-11-10

| Risk ID | Risk | Affected Req IDs | Original Rating | Status (2026-11-10) | Mitigation / Residual | Open Question |
|---|---|---|---|---|---|---|
| RSK-001 | Core clinical record (consultation/visit) undefined, causing rework across six requirements | FR-VIT-001, FR-CMP-001, FR-DIA-001, FR-RX-001, FR-HIS-001, FR-HIS-002 | High | **Closed** | Consultation and visit are the same record; can exist without an appointment | OQ-006, OQ-028 |
| RSK-002 | Success criteria cannot be verified, so Phase 1 acceptance is disputed | SC-001, SC-002, SC-003, SC-006, NFR-PRF-001, NFR-PRF-002, NFR-USB-001 | High | **Mitigated** | SC-001, SC-002 and NFR-PRF measured on localhost test data (PDR-30). SC-003 and SC-006 deferred (OOS-014). Residual: localhost timings are guidelines | OQ-020, OQ-021 |
| RSK-003 | Patient data handled without a defined security standard or regulatory basis | NFR-SEC-001, NFR-SEC-002 | High | **Accepted** (residual Medium) | Login, password rules, sliding timeout and hashed passwords defined. Encryption at rest out of Phase 1 (DEC-017). Email-only password reset accepted (DEC-009): anyone who knows the account email can take over the account. Regulation and retention deferred (OOS-015) | OQ-015, OQ-016, OQ-017 |
| RSK-004 | Data loss from undefined backup and recovery | NFR-REL-001, NFR-REL-002 | High | **Accepted** | Backups and recovery deferred (OOS-011) for a localhost-only learning project. Revisit if deployed | OQ-018, OQ-019 |
| RSK-005 | Vitals rules conflict with clinic workflow (hard block vs. optional) | FR-VIT-001, SC-001 | Medium | **Closed** | Warn and allow; missing vitals a notice only (OQ-007) | OQ-007 |
| RSK-006 | Prescription output does not meet clinic or legal expectations | FR-RX-002, SC-004 | High | **Mitigated** | A4 printable view with header, footer and signature defined (OQ-010). Residual Low | OQ-010, OQ-011 |
| RSK-007 | Scope creep toward out-of-scope features | FR-SRC-002, FR-APT-003, FR-EXP-001/002 | Medium | **Open** | Boundary cases resolved in `03-` §3. Any addition needs a BRD amendment first (CON-017) | OQ-013, OQ-014 |
| RSK-008 | BRD states "Open Questions: None" while many gaps exist | All | High | **Closed** | BRD Open Questions section updated (OQ-027) | OQ-027 |
| RSK-009 | Browser support claimed but untested for versions in use | NFR-CMP-001 | Low | **Mitigated** | Latest Chrome and Edge; desktop only; Safari not required (OQ-022, DEC-019). Browser version recorded per test run (PDR-28) | OQ-022 |
| RSK-010 | Patient data cannot be corrected or removed safely | FR-PAT-002, FR-APT-003 | Medium | **Accepted** | Archive, not hard delete (OQ-024). No patient deletion or archiving (PDR-25). No edit history (OQ-025, OOS-013) | OQ-024, OQ-025 |
| RSK-011 | Single-owner decision making: role overlap or absent Product Owner delays answers | All open questions | Medium | **Closed** | One person holds all roles (OQ-026) | OQ-026 |
| RSK-012 | Volume assumptions wrong, so performance targets not met | NFR-SCL-001, NFR-PRF-001, NFR-PRF-002 | Low | **Closed** | Volume is up to 1,000 patient records (OQ-023) | OQ-023 |
| RSK-013 | Audit trail excluded from Phase 1, so no access or change logging for disputes or compliance | BO-003, RSK-010, NFR-SEC-001 | High (proposed 2026-11-10) | **Accepted** (re-rated Medium) | Excluded by Project Owner decision (OQ-025, OOS-013). Re-rated Medium for a single-user, localhost-only learning project. For review | OQ-025 |
| RSK-014 | Priorities used for planning before Product Owner approval, so scope is set on unapproved ratings | All | Medium | **Closed** | Priorities approved by the Project Owner 2026-11-10 (A-006, DEC-006) | A-006 |
| RSK-015 | Plain HTTP on localhost means traffic on localhost is not encrypted in transit (NFR-SEC-002 in-transit unmet on localhost) | NFR-SEC-002 | Medium | **Accepted** (localhost only) | Decided under OQ-016 and PDR-21. TLS 1.2+ required beyond localhost | OQ-016 |

## Summary

- **Closed (6):** RSK-001, RSK-005, RSK-008, RSK-011, RSK-012, RSK-014
- **Mitigated (3):** RSK-002, RSK-006, RSK-009
- **Accepted (5):** RSK-003, RSK-004, RSK-010, RSK-013, RSK-015
- **Open (1):** RSK-007
- Every risk above the Medium rating from the original register is now closed, mitigated or accepted by the Project Owner.
