# Release Roadmap

Plans the releases of the Patient Management System and the requirements each one covers. Requirement IDs and their status are in [`traceability.md`](traceability.md). Release rules are in `CLAUDE.md` §13.

No release dates are set in this roadmap. Dates are added only when the owner sets them.

## Releases

| Release | Version | Scope | Status | Release gate |
|---|---|---|---|---|
| Phase 1 MVP | `v0.1.0` (per CLAUDE.md §13) | 34 requirements in scope: FR-PAT-001 to FR-PAT-005, FR-APT-001 to FR-APT-003, FR-VIT-001, FR-CMP-001, FR-DIA-001, FR-RX-001, FR-RX-002, FR-HIS-001 to FR-HIS-003, FR-SRC-001 to FR-SRC-003, FR-EXP-001, FR-EXP-002, NFR-USB-001, NFR-PRF-001, NFR-PRF-002, NFR-REL-001, NFR-REL-002, NFR-SEC-001, NFR-SEC-002, NFR-SCL-001, NFR-CMP-001, SC-001, SC-002, SC-004, SC-005 | Planned; not started | Full release gate (CLAUDE.md §13) |
| Next planned release after Phase 1 | `v0.2.0` (proposed; owner to confirm) | SC-003 and SC-006 are targeted here, as owner-approved deferrals. The scope of this release is set by the owner. | Planned | Deferred requirements are re-reviewed; they enter scope only on a new owner decision |

## Deferred Requirements by Release

| Requirement | Status | Target release | Threshold | Measurement timing |
|---|---|---|---|---|
| SC-003 (at least 80% paper reduction) | Deferred | Next planned release after Phase 1 MVP | At least 80% (BRD) | Four weeks before go-live (baseline) and four weeks after (measurement) |
| SC-006 (high usability with minimal training) | Deferred | Next planned release after Phase 1 MVP | TBD (proposed; pending business confirmation) | First two weeks of use |

## Rules

- A requirement is in a release's scope only when the owner records that decision in the traceability matrix.
- The Phase 1 MVP release gate covers only the requirements listed under Phase 1 MVP. Deferred requirements are not counted in it (CLAUDE.md §13).
- The roadmap is updated in the same PR as any change to a release scope, deferral, or target release.
- Version numbers follow CLAUDE.md §13 (Semantic Versioning). The version of a release is recorded when the release is prepared.
