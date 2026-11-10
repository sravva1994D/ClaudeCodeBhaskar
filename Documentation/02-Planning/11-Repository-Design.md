# 11 — Repository Design

**Source:** `06-Entity-Design.md`, `07-Database-Design.md`, `10-Service-Design.md`
**Rule:** Thin repositories over EF Core `DbContext` (AD-04). No generic repository. Repositories return entities to the Business layer only. They do not apply business rules.
**Status:** Proposed for review

## 1. Structure

| Item | Decision |
|---|---|
| Location | `PatientManagement.Data/Repositories/` |
| Interfaces | Declared in `PatientManagement.Data/Abstractions/`, implemented in `PatientManagement.Data/Repositories/` |
| Unit of work | `DbContext.SaveChangesAsync()` is the unit of work. No separate UoW class in Phase 1 |
| Lifetime | Scoped (per HTTP request) |
| Queries | `AsNoTracking()` for read-only queries (lists, search, print model, exports) |
| Paging | Not needed for the sample size. Lists are capped by business rules (for example, recent = 5) |

## 2. Repository Interfaces

### 2.1 IUserRepository (NFR-SEC-001)
| Method | Returns | Purpose |
|---|---|---|
| GetByUsernameAsync(username) | User? | Sign-in |
| GetByEmailAsync(email) | User? | Forgot password |

Sign-in always matches the username. There is no fallback to a single account (DEC-018).
| AnyAsync() | bool | True when an account exists. Used by the seed command (DEC-018; added in Module 1, step 4) |
| AddAsync(user) | Task | Adds and saves the account. Used by the seed command only (added in step 4) |
| UpdateAsync(user) | Task | Saves the entity's values, for example a new password hash. The repository saves because Business has no unit of work (Module 1, step 2) |

### 2.2 IPatientRepository (FR-PAT-001 to FR-PAT-004, FR-SRC-001, FR-SRC-002)
| Method | Returns | Purpose | Req IDs |
|---|---|---|---|
| GetByIdAsync(id) | Patient? | Profile, edit | FR-PAT-002, FR-PAT-003 |
| SearchAsync(term) | IReadOnlyList<Patient> | Name or phone contains (PDR-23) | FR-PAT-004, FR-SRC-001 |
| FindPossibleDuplicatesAsync(name, dob, phone, excludeId?) | IReadOnlyList<Patient> | Duplicate warning | FR-PAT-001 |
| AddAsync(patient) | void | Insert | FR-PAT-001 |
| UpdateCodeAsync(id, code) | void | Set PatientCode after insert (AD-09) | FR-PAT-001 |
| ExistsAsync(id) | bool | Appointment and visit checks | FR-APT-001, FR-HIS-001 |

Recent patients is served by `IConsultationRepository.GetRecentPatientIdsAsync(5)` (see 2.4), keeping the patient table free of visit logic.

### 2.3 IAppointmentRepository (FR-APT-001 to FR-APT-003)
| Method | Returns | Purpose | Req IDs |
|---|---|---|---|
| GetByIdAsync(id) | Appointment? | Status update, archive | FR-APT-003 |
| GetDailyAsync(date) | IReadOnlyList<Appointment> (with Patient name and code) | Daily list | FR-APT-002 |
| HasOverlapAsync(startTime, excludeId?) | bool | Overlap check (non-archived) | FR-APT-001 |
| AddAsync(appointment) | void | Schedule | FR-APT-001 |

### 2.4 IConsultationRepository (FR-HIS-001 to FR-HIS-003, FR-SRC-002, FR-SRC-003)
| Method | Returns | Purpose | Req IDs |
|---|---|---|---|
| GetByIdAsync(id, includeVitals, includePrescription) | Consultation? | Working view and detail | FR-VIT-001, FR-HIS-002 |
| ListByPatientAsync(patientId, from?, to?) | IReadOnlyList<Consultation> | Visit history and filter | FR-HIS-001, FR-HIS-003 |
| GetRecentPatientIdsAsync(count) | IReadOnlyList<int> | Latest visit per patient, top N (PDR-11) | FR-SRC-002 |
| AddAsync(consultation) | void | Start | FR-HIS-001 |
| ListForExportAsync(patientId, from?, to?) | IReadOnlyList<Consultation> (with vitals, prescription, items) | Export | FR-EXP-001, FR-EXP-002 |

### 2.5 IVitalSignRepository (FR-VIT-001)
| Method | Returns | Purpose |
|---|---|---|
| GetByConsultationAsync(consultationId) | VitalSign? | Working view |
| UpsertAsync(vitalSign) | void | Save or update (1:0..1) |

### 2.6 IPrescriptionRepository (FR-RX-001, FR-RX-002)
| Method | Returns | Purpose | Req IDs |
|---|---|---|---|
| GetByConsultationAsync(consultationId, includeItems) | Prescription? | Working view, print model | FR-RX-001, FR-RX-002 |
| AddAsync(prescription) | void | First save for the consultation | FR-RX-002 |
| ReplaceItemsAsync(prescriptionId, items) | void | Replace lines (delete then insert, same unit of work) | FR-RX-001 |

### 2.7 IClinicProfileRepository (FR-RX-002)
| Method | Returns | Purpose |
|---|---|---|
| GetAsync() | ClinicProfile? | Header |
| UpsertAsync(profile) | void | Save header (single row) |

## 3. Query Rules

| Query | Rule |
|---|---|
| Daily appointments | Filter `StartTime` by date (local). Exclude `IsArchived = 1` |
| Visits in range | Filter `VisitDateTime` by date. `to` is inclusive of that whole day |
| Recent patients | Group by PatientId, latest VisitDateTime, top N. Patients with no visit excluded (PDR-11) |
| Duplicate check | Exact match on DateOfBirth and Phone; name match case-insensitive. Exact match (PDR-10) |
| Search | Contains match on FullName or Phone. Case-insensitive. Rule per PDR-23 |
| Export | Load patient, visits, vitals, prescription and items in one query with includes |

## 4. Concurrency

Phase 1 is single-user. Optimistic concurrency is not required. The app uses default EF Core behaviour and does not add row-version columns. If two browser tabs edit the same record, the last save wins. This is noted as a known limitation and is not an audit feature (OOS-013).

## 5. Test Seams

- Repositories are tested with integration tests against a real SQL Server test database (not an in-memory provider) because of SQL-specific behaviour (see `14-Testing-Strategy.md`).
- Services are tested with fake repositories (interfaces).
