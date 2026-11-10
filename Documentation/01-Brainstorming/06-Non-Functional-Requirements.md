# 06 — Non-Functional Requirements

**Source:** BRD: Non-Functional Requirements; Success Criteria
**Status:** Refreshed 2026-11-10 (open questions answered)

| Req ID | Category | Requirement (BRD wording) | BRD Section | Measurable as Written? | Open Questions |
|---|---|---|---|---|---|
| NFR-USB-001 | Usability | Simple, minimal UI optimized for fast data entry during consultations | NFR > Usability | No ("simple", "minimal", "fast" are not measured) | OQ-021 |
| NFR-PRF-001 | Performance | Page load time < 2 seconds | NFR > Performance | Yes (threshold and localhost conditions defined) | OQ-020 |
| NFR-PRF-002 | Performance | Patient search results returned within 2–5 seconds under normal localhost development conditions (updated 2026-11-10, OQ-020) | NFR > Performance; SC-002 | Yes (2–5 seconds, localhost) | OQ-020 |
| NFR-REL-001 | Reliability | No data loss | NFR > Reliability | Deferred / out of scope for Phase 1 (OOS-011) | OQ-019 |
| NFR-REL-002 | Reliability | Regular automated backups | NFR > Reliability | Deferred / out of scope for Phase 1 (OOS-011) | OQ-018 |
| NFR-SEC-001 | Security | Secure login (single user authentication) | NFR > Security | Yes (username and password; 8–16 characters with complexity; 24-hour sliding timeout) | OQ-015 |
| NFR-SEC-002 | Security | Data in transit; password hashing | NFR > Security | Yes (TLS 1.2+ beyond localhost; hashed passwords). Encryption at rest out of Phase 1 (DEC-017) | OQ-016, OQ-017 |
| NFR-SCL-001 | Scalability | Designed for a single clinic with moderate patient volume | NFR > Scalability | Yes (up to 1,000 patient records) | OQ-023 |
| NFR-CMP-001 | Compatibility | Works on modern web browsers (Chrome, Edge) | NFR > Compatibility | Yes (latest Chrome and Edge; desktop; Safari not required, DEC-019) | OQ-022 |

## Notes

- **Traceability:** Each NFR traces to the BRD subsection shown. No NFR is added beyond the BRD.
- **Non-measurable NFRs:** Only NFR-USB-001 cannot be tested as written. SC-006 (usability) is deferred (OOS-014). No target values are added here.
- **Security scope:** NFR-SEC-001 and NFR-SEC-002 are the only security requirements in the BRD. Audit logging, access control beyond single-user login, and privacy/consent requirements are not in the BRD and were raised as questions (OQ-017, OQ-025). OQ-017 is deferred (OOS-015); OQ-025 is excluded (OOS-013).
- **Reliability scope:** "No data loss" (NFR-REL-001) and automated backups (NFR-REL-002) are separate. Backups do not by themselves guarantee no data loss.
