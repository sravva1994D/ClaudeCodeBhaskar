# 06 — Entity Design

**Source:** BRD: Functional Requirements (Patient Management, Appointment Management, Consultation Workflow, Patient History, Data Export)
**Naming:** C# entity names are singular. Tables are plural (see `07-Database-Design.md`).
**Status:** Proposed for review. Items marked **PDR-n** are decided in `17-Planning-Decisions-and-Open-Items.md` (2026-11-10).

## 1. Entity List

| Entity | Purpose | Req IDs |
|---|---|---|
| User | The single physician account | NFR-SEC-001 |
| Patient | A registered patient | FR-PAT-001 to FR-PAT-004 |
| Appointment | A 30-minute slot for a patient | FR-APT-001 to FR-APT-003 |
| Consultation | A visit. Same record as "consultation" in the BRD (OQ-028) | FR-CMP-001, FR-DIA-001, FR-HIS-001 |
| VitalSign | Temperature, blood pressure and pulse for one consultation | FR-VIT-001 |
| Prescription | The one printable prescription for a consultation | FR-RX-002 |
| PrescriptionItem | One medicine line | FR-RX-001 |
| ClinicProfile | Header data for printed prescriptions | FR-RX-002 (PDR-15) |

## 2. Entity Definitions

### 2.1 User
| Attribute | Type | Rules | Req IDs |
|---|---|---|---|
| Id | int | PK | |
| Username | string(50) | Required, unique | NFR-SEC-001 |
| PasswordHash | string(256) | Required. Never plain text | NFR-SEC-001 |
| Email | string(256) | Required. Matched by forgot-password | NFR-SEC-001 |

Only one row is expected. No role field (CON-002). Created once by the console seed command; the password is typed at a prompt and never stored (PDR-05 as amended, DEC-018).

### 2.2 Patient
| Attribute | Type | Rules | Req IDs |
|---|---|---|---|
| Id | int | PK, identity | |
| PatientCode | string(20) | Unique. Format `PT-` + 6-digit zero-padded Id (e.g. PT-000123). Set after insert (AD-09) | FR-PAT-001 |
| FullName | string(200) | Required (Name) | FR-PAT-001 |
| DateOfBirth | date | Required | FR-PAT-001 |
| Gender | string(20) | Required. Allowed values: Male, Female, Other (PDR-01, decided 2026-11-10) | FR-PAT-001 |
| Phone | string(20) | Required | FR-PAT-001, FR-PAT-004 |
| Email | string(256) | Required | FR-PAT-001 |
| Address | string(500) | Required | FR-PAT-001 |

Not stored: **Age**. Calculated from DateOfBirth and the system date (BRD: Patient Management).

### 2.3 Appointment
| Attribute | Type | Rules | Req IDs |
|---|---|---|---|
| Id | int | PK | |
| PatientId | int | FK → Patient. Required. Patient must exist before scheduling | FR-APT-001 |
| StartTime | datetime2 | Required. Slot start (grid per PDR-06) | FR-APT-001, FR-APT-002 |
| DurationMinutes | smallint | Fixed at 30 (BRD: slots are 30 minutes). Stored so the rule is visible in data | FR-APT-001 |
| Status | string(20) | Scheduled, Checked-In, In Consultation, Completed, Cancelled, No-show (lifecycle in `19-`) | FR-APT-003 |
| IsArchived | bool | Default false. Archived rows are hidden, not deleted | FR-APT-003 |

Overlap rule: a new appointment must not overlap an existing non-archived appointment. Because every appointment takes a full slot, overlap means the same slot start. Cancelled and No-show release the slot (PDR-07).

### 2.4 Consultation (Visit)
| Attribute | Type | Rules | Req IDs |
|---|---|---|---|
| Id | int | PK | |
| PatientId | int | FK → Patient. Required | FR-HIS-001 |
| AppointmentId | int? | FK → Appointment. Optional. Walk-ins have none (OQ-006) | FR-APT-001, FR-HIS-001 |
| VisitDateTime | datetime2 | Required. Set at start | FR-HIS-001, FR-HIS-003 |
| Complaints | string(max) | Free text. Optional at save (BRD: free text only). One text block per visit (PDR-12) | FR-CMP-001 |
| Diagnosis | string(max) | Free text | FR-DIA-001 |

A consultation can exist without an appointment (OQ-006). "Last visit" for recent patients is the latest VisitDateTime (PDR-11).

### 2.5 VitalSign
| Attribute | Type | Rules | Req IDs |
|---|---|---|---|
| ConsultationId | int | PK and FK → Consultation (1:0..1) | FR-VIT-001 |
| TemperatureC | decimal(4,1) | °C. Range 34.0–42.0 gives a warning | FR-VIT-001 |
| SystolicMmHg | smallint | mmHg. Range 70–250 gives a warning | FR-VIT-001 |
| DiastolicMmHg | smallint | mmHg. Range 40–150 gives a warning | FR-VIT-001 |
| PulseBpm | smallint | bpm. Range 30–220 gives a warning | FR-VIT-001 |

- All four fields are required only when a VitalSign row is saved. Missing vitals show a warning (no row) and do not block consultation save (BRD: Vitals).
- Not in Phase 1: respiratory rate, weight, height, oxygen saturation (OOS-012).

### 2.6 Prescription
| Attribute | Type | Rules | Req IDs |
|---|---|---|---|
| Id | int | PK | |
| ConsultationId | int | FK → Consultation. Unique (one per consultation) | FR-RX-002 |
| IsPrinted | bool | Default false. Set true on print | FR-RX-002 |
| PrintedAt | datetime2? | Set when printed | FR-RX-002 |

### 2.7 PrescriptionItem
| Attribute | Type | Rules | Req IDs |
|---|---|---|---|
| Id | int | PK | |
| PrescriptionId | int | FK → Prescription | FR-RX-001 |
| Name | string(200) | Required. Free text | FR-RX-001 |
| Dosage | string(100) | Free text | FR-RX-001 |
| Frequency | string(100) | Free text | FR-RX-001 |
| Duration | string(100) | Free text | FR-RX-001 |
| Instructions | string(500) | Free text | FR-RX-001 |
| SortOrder | smallint | Display order | FR-RX-001, FR-RX-002 |

Medicines are free text. There is no medicine catalogue (BRD: free text only; no drug master is in the BRD).

### 2.8 ClinicProfile
| Attribute | Type | Rules | Req IDs |
|---|---|---|---|
| Id | int | PK. Single row | FR-RX-002 |
| ClinicName | string(200) | Required for printing | FR-RX-002 |
| Address | string(500) | Required for printing | FR-RX-002 |
| Phone | string(20) | Required for printing | FR-RX-002 |
| DoctorName | string(200) | Required for printing | FR-RX-002 |
| RegistrationNumber | string(100) | Required for printing | FR-RX-002 |
| (no logo field) | — | Phase 1 print view shows a static placeholder logo image; no upload (PDR-15 as amended, DEC-021) | FR-RX-002 |

The BRD requires these header fields but does not say where they are entered (PDR-15). "Required for printing" means the print view shows a notice when a value is missing. It does not add a rule the BRD does not state. Header entry is decided under PDR-15.

## 3. Relationships

| From | To | Cardinality | Rule |
|---|---|---|---|
| Patient | Appointment | 1 : 0..* | An appointment needs a registered patient |
| Patient | Consultation | 1 : 0..* | Every visit belongs to one patient |
| Appointment | Consultation | 0..1 : 0..1 | Optional link; one consultation per appointment (PDR-26) |
| Consultation | VitalSign | 1 : 0..1 | Vitals optional at save |
| Consultation | Prescription | 1 : 0..1 | One prescription per consultation (BRD) |
| Prescription | PrescriptionItem | 1 : 0..* | Multiple medicines (FR-RX-001 AC-2) |
| User | (none) | — | No link to patient data (single user) |
| ClinicProfile | (none) | — | Single row, read by print view |

## 4. Entity Invariants (Business Layer)

| Invariant | Enforced by | Req IDs |
|---|---|---|
| Patient has Name, DOB, Gender, Phone, Email, Address | Validator (errors) and not-null columns | FR-PAT-001 |
| Duplicate patient warning on Name, DOB or Phone match | Service returns a warning; saves only when confirmed | FR-PAT-001 |
| Appointment is 30 minutes and does not overlap | Validator and service (rule in PDR-06, PDR-07) | FR-APT-001 |
| Appointment status is one of six values | Validator and check constraint | FR-APT-003 |
| Vitals out of range need confirmation | Validator (warning). No DB check | FR-VIT-001 |
| Printed prescription cannot change | Service rejects edits when IsPrinted | FR-RX-002 |
| One prescription per consultation | Unique index and service | FR-RX-002 |
