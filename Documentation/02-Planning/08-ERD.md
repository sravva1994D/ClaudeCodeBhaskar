# 08 — Entity Relationship Diagram

**Source:** `06-Entity-Design.md`, `07-Database-Design.md`
**Status:** Proposed for review. Diagram notation: Mermaid `erDiagram`.

## 1. Full ERD

```mermaid
erDiagram
  USERS {
    int Id PK
    nvarchar Username UK
    nvarchar PasswordHash
    nvarchar Email
  }
  PATIENTS {
    int Id PK
    nvarchar PatientCode UK
    nvarchar FullName
    date DateOfBirth
    nvarchar Gender
    nvarchar Phone
    nvarchar Email
    nvarchar Address
  }
  APPOINTMENTS {
    int Id PK
    int PatientId FK
    datetime2 StartTime
    smallint DurationMinutes
    nvarchar Status
    bit IsArchived
  }
  CONSULTATIONS {
    int Id PK
    int PatientId FK
    int AppointmentId FK
    datetime2 VisitDateTime
    nvarchar Complaints
    nvarchar Diagnosis
  }
  VITAL_SIGNS {
    int ConsultationId PK
    decimal TemperatureC
    smallint SystolicMmHg
    smallint DiastolicMmHg
    smallint PulseBpm
  }
  PRESCRIPTIONS {
    int Id PK
    int ConsultationId UK
    bit IsPrinted
    datetime2 PrintedAt
  }
  PRESCRIPTION_ITEMS {
    int Id PK
    int PrescriptionId FK
    nvarchar Name
    nvarchar Dosage
    nvarchar Frequency
    nvarchar Duration
    nvarchar Instructions
    smallint SortOrder
  }
  CLINIC_PROFILE {
    int Id PK
    nvarchar ClinicName
    nvarchar Address
    nvarchar Phone
    nvarchar DoctorName
    nvarchar RegistrationNumber
    nvarchar LogoPath
  }

  PATIENTS ||--o{ APPOINTMENTS : "books"
  PATIENTS ||--o{ CONSULTATIONS : "has visits"
  APPOINTMENTS |o--o{ CONSULTATIONS : "may start (PDR-26)"
  CONSULTATIONS ||--o| VITAL_SIGNS : "0..1 vitals"
  CONSULTATIONS ||--o| PRESCRIPTIONS : "0..1 prescription"
  PRESCRIPTIONS ||--o{ PRESCRIPTION_ITEMS : "medicine lines"
```

`USERS` and `CLINIC_PROFILE` have no relationship to the clinical tables. `USERS` is the single login account. `CLINIC_PROFILE` is a single-row header source for the print view.

## 2. Clinical Core (simplified)

```mermaid
erDiagram
  PATIENTS ||--o{ CONSULTATIONS : "visits"
  CONSULTATIONS ||--o| VITAL_SIGNS : "vitals"
  CONSULTATIONS ||--o| PRESCRIPTIONS : "prescription"
  PRESCRIPTIONS ||--o{ PRESCRIPTION_ITEMS : "medicines"
```

## 3. Requirement Coverage per Table

| Table | Requirements that depend on it |
|---|---|
| USERS | NFR-SEC-001 |
| PATIENTS | FR-PAT-001 to FR-PAT-004, FR-SRC-001, FR-SRC-002, FR-EXP-001, FR-EXP-002 |
| APPOINTMENTS | FR-APT-001 to FR-APT-003 |
| CONSULTATIONS | FR-CMP-001, FR-DIA-001, FR-HIS-001 to FR-HIS-003, FR-SRC-002, FR-SRC-003 |
| VITAL_SIGNS | FR-VIT-001, FR-RX-002, FR-HIS-002, FR-EXP-001, FR-EXP-002 |
| PRESCRIPTIONS | FR-RX-002, SC-004 |
| PRESCRIPTION_ITEMS | FR-RX-001, FR-RX-002, FR-HIS-002 |
| CLINIC_PROFILE | FR-RX-002 |
