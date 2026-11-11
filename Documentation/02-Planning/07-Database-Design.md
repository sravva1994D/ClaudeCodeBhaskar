# 07 — Database Design

**Source:** `06-Entity-Design.md`; BRD: Performance, Scalability, Security
**Platform:** SQL Server, local instance, database name `PatientManagement` (AD-15)
**Status:** Proposed for review

## 1. Tables

| Table | Entity | Rows (Phase 1 sample) | Req IDs |
|---|---|---|---|
| Users | User | 1 | NFR-SEC-001 |
| Patients | Patient | up to 1,000 | FR-PAT-001 to FR-PAT-004, NFR-SCL-001 |
| Appointments | Appointment | sample-dependent | FR-APT-001 to FR-APT-003 |
| Consultations | Consultation | sample-dependent | FR-HIS-001 to FR-HIS-003 |
| VitalSigns | VitalSign | ≤ Consultations | FR-VIT-001 |
| Prescriptions | Prescription | ≤ Consultations | FR-RX-002 |
| PrescriptionItems | PrescriptionItem | ≥ 0 per prescription | FR-RX-001 |
| ClinicProfile | ClinicProfile | 1 | FR-RX-002 |

The "sample" is the local test dataset of up to 1,000 patients (BRD: Performance). It is not a production volume target.

## 2. Column Definitions

### Users
| Column | Type | Null | Constraint |
|---|---|---|---|
| Id | int | No | PK, identity |
| Username | nvarchar(50) | No | Unique |
| PasswordHash | nvarchar(256) | No | |
| Email | nvarchar(256) | No | |

### Patients
| Column | Type | Null | Constraint |
|---|---|---|---|
| Id | int | No | PK, identity |
| PatientCode | nvarchar(20) | No | Unique |
| FullName | nvarchar(200) | No | |
| DateOfBirth | date | No | |
| Gender | nvarchar(20) | No | Check: Male, Female, Other (PDR-01) |
| Phone | nvarchar(20) | No | |
| Email | nvarchar(256) | No | |
| Address | nvarchar(500) | No | |

### Appointments
| Column | Type | Null | Constraint |
|---|---|---|---|
| Id | int | No | PK, identity |
| PatientId | int | No | FK → Patients.Id |
| StartTime | datetime2(0) | No | Index (daily list) |
| DurationMinutes | smallint | No | Default 30; check = 30 |
| Status | nvarchar(20) | No | Check: Scheduled, Checked-In, In Consultation, Completed, Cancelled, No-show |
| IsArchived | bit | No | Default 0 |

### Consultations
| Column | Type | Null | Constraint |
|---|---|---|---|
| Id | int | No | PK, identity |
| PatientId | int | No | FK → Patients.Id |
| AppointmentId | int | Yes | FK → Appointments.Id |
| VisitDateTime | datetime2(0) | No | Index with PatientId |
| Complaints | nvarchar(max) | Yes | |
| Diagnosis | nvarchar(max) | Yes | |

### VitalSigns
| Column | Type | Null | Constraint |
|---|---|---|---|
| ConsultationId | int | No | PK, FK → Consultations.Id |
| TemperatureC | decimal(4,1) | No | Type only. **No range check** (AD-14) |
| SystolicMmHg | smallint | No | Type only |
| DiastolicMmHg | smallint | No | Type only |
| PulseBpm | smallint | No | Type only |

### Prescriptions
| Column | Type | Null | Constraint |
|---|---|---|---|
| Id | int | No | PK, identity |
| ConsultationId | int | No | FK, unique |
| IsPrinted | bit | No | Default 0 |
| PrintedAt | datetime2(0) | Yes | Set when IsPrinted = 1 |

### PrescriptionItems
| Column | Type | Null | Constraint |
|---|---|---|---|
| Id | int | No | PK, identity |
| PrescriptionId | int | No | FK → Prescriptions.Id, on delete cascade |
| Name | nvarchar(200) | No | |
| Dosage | nvarchar(100) | Yes | Free text |
| Frequency | nvarchar(100) | Yes | Free text |
| Duration | nvarchar(100) | Yes | Free text |
| Instructions | nvarchar(500) | Yes | Free text |
| SortOrder | smallint | No | |

Dosage, Frequency, Duration and Instructions are nullable in the database. Whether the BRD requires each one is not stated. The BRD lists them as fields but not as required (PDR-13). The validator enforces only what PDR-13 decides.

### ClinicProfile
| Column | Type | Null | Constraint |
|---|---|---|---|
| Id | int | No | PK. Single row (check Id = 1) |
| ClinicName | nvarchar(200) | Yes | |
| Address | nvarchar(500) | Yes | |
| Phone | nvarchar(20) | Yes | |
| DoctorName | nvarchar(200) | Yes | |
| RegistrationNumber | nvarchar(100) | Yes | |

Nullable here because the BRD does not say when the header must be complete (PDR-15).

## 3. Keys and Relationships

| Relationship | On delete | Note |
|---|---|---|
| Patients → Appointments | Restrict | No hard delete of patients is in the BRD |
| Patients → Consultations | Restrict | |
| Appointments → Consultations | Set null | Keeps the visit if the appointment is removed (should not happen; archive only) |
| Consultations → VitalSigns | Cascade | Vitals belong to the visit |
| Consultations → Prescriptions | Restrict | Prescriptions are kept with the visit |
| Prescriptions → PrescriptionItems | Cascade | Items belong to the prescription. Edits blocked once printed (AD-13) |

## 4. Indexes

| Index | Columns | Supports | Req IDs |
|---|---|---|---|
| IX_Patients_FullName | FullName | Name search (`LIKE 'x%'` prefix; see note) | FR-PAT-004, FR-SRC-001 |
| IX_Patients_Phone | Phone | Phone search | FR-PAT-004, FR-SRC-001 |
| IX_Patients_PatientCode (unique) | PatientCode | Lookup by code; uniqueness | FR-PAT-001 |
| IX_Patients_DOB_Phone (duplicate check) | DateOfBirth, Phone | Duplicate warning | FR-PAT-001 |
| IX_Appointments_StartTime | StartTime, IsArchived | Daily appointment list | FR-APT-002 |
| IX_Appointments_Slot (unique, filtered) | StartTime where IsArchived = 0 and Status <> Cancelled and Status <> No-show | DB-level no-overlap guard. The service check is the primary rule (D6). Written with <> because SQL Server filtered indexes do not accept IN; equivalent for the six statuses (Module 3, step 1) | FR-APT-001 |
| IX_Appointments_PatientId | PatientId | Foreign key lookup (added in Module 3, step 1; not in the first draft) | FR-APT-001 |
| IX_Consultations_Patient_Visit | PatientId, VisitDateTime DESC | Visit history, last visit, recent patients | FR-HIS-001, FR-HIS-003, FR-SRC-002 |
| IX_Consultations_AppointmentId (unique, filtered) | AppointmentId where not null | One consultation per appointment (PDR-26) | FR-APT-001, FR-HIS-001 |
| IX_Prescriptions_ConsultationId (unique) | ConsultationId | One prescription per visit | FR-RX-002 |

**Search note:** BRD says "search by name or phone" (FR-PAT-004). A contains-match (`%term%`) cannot use a prefix index. For up to 1,000 rows this is expected to meet NFR-PRF-002 (2–5 s). If measurement fails, add a full-text index (decision recorded in the test report, not assumed now). Matching rule: contains, case-insensitive (PDR-23).

## 5. Constraints Summary

| Type | Where | Purpose |
|---|---|---|
| NOT NULL | All required BRD fields | Structural completeness |
| UNIQUE | PatientCode, Username, ConsultationId on Prescriptions | Identity and one-per-visit |
| CHECK | Appointments.Status, Appointments.DurationMinutes, Patients.Gender (PDR-01), ClinicProfile.Id = 1 | Enumerated values |
| No CHECK | Vital-sign ranges | Warnings are business rules (AD-14) |

## 6. Data Types and Time

- Strings: `nvarchar` (Unicode for names and addresses).
- Dates and times: local clinic time (AD-07). `datetime2(0)` (seconds precision).
- Money: none. Billing is out of scope (OOS-002).
- Files: none stored. The print view uses a static placeholder logo image in the web project (DEC-021).

## 7. Storage and Encryption

| Item | Approach | Req IDs | Status |
|---|---|---|---|
| Storage encryption | Out of Phase 1 scope (DEC-017). No TDE. | NFR-SEC-002 | Amended 2026-11-10 |
| Password storage | Hash only (`PasswordHash`) | NFR-SEC-001 | Decided (BRD) |
| Connection | Local connection string. Encrypted connection beyond localhost only | NFR-SEC-002 | PDR-21 |

## 8. Migrations

- Initial migration creates all tables above.
- Account seed (console command, DEC-018; not a migration): one User. The password is typed at the prompt and never stored. A ClinicProfile row (blank values) is created on first use.
- Test data (local testing only): a generated sample of up to 1,000 patients for NFR-PRF tests. Sample data is clearly marked as test data and is not clinical data.
- No data migration from paper or other systems is in scope (BRD: Problem Statement mentions paper, but no import requirement exists).
