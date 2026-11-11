# 10 — Service Design

**Source:** `03-Layer-Responsibilities.md`, `06-Entity-Design.md`, BRD Functional Requirements
**Rule:** Services hold the business rules. They take and return DTOs and `Result` objects, never entities or HTTP types.
**Status:** Proposed for review

## 1. Shared Types

| Type | Purpose |
|---|---|
| `Result<T>` | `Value`, `IsSuccess`, `Errors` (blocking), `Warnings` (need confirmation) |
| `Warning` | `Code`, `Message`, `Field` |
| `Error` | `Code`, `Message`, `Field` |
| `ConfirmationRequired` | Result state used when warnings exist and were not confirmed |

Warnings and errors use the same shape. The difference is the flag that allows save (AD-11).

## 2. Services

### 2.1 IAuthService (FR: NFR-SEC-001)
| Method | Input | Output | Rules | Req IDs |
|---|---|---|---|---|
| SignInAsync | username, password | Result | Verify hash. Unknown user and wrong password return the same message | NFR-SEC-001 |
| ResetPasswordAsync | email, newPassword, confirmPassword | Result | Email must match the user. Password meets policy (PDR-02). Hash and save. Caller signs in. Confirmation must match. No match returns the PDR-03 message | NFR-SEC-001 |
| CreateInitialAccountAsync | username, email, password | Result | Seed command only (DEC-018, plan A-1). Refused when an account exists. Username, valid email and password policy required. Stores the hash only | NFR-SEC-001, CON-002 |

### 2.2 IPatientService (FR-PAT-001 to FR-PAT-004, FR-SRC-001, FR-SRC-002)
| Method | Input | Output | Rules | Req IDs |
|---|---|---|---|---|
| RegisterAsync | PatientCreateDto, confirmWarnings | Result<PatientDto> | Required fields. Duplicate check (warning). Generate PatientCode. Save | FR-PAT-001 |
| UpdateAsync | PatientUpdateDto, confirmWarnings | Result<PatientDto> | Same validation as register. Duplicate check excludes the same patient | FR-PAT-002 |
| GetAsync | id | PatientDto? | Age calculated here from DOB and system date | FR-PAT-003 |
| SearchAsync | term | IReadOnlyList<PatientSummaryDto> | Contains match, case-insensitive (PDR-23). Empty term returns a Result error | FR-PAT-004, FR-SRC-001 |
| GetRecentAsync | count = 5 | IReadOnlyList<PatientSummaryDto> | Ordered by latest VisitDateTime. Only patients with visits (PDR-11) | FR-SRC-002 |

Duplicate check: same FullName, same DateOfBirth, or same Phone as an existing patient. Returns a warning listing the matches. Save continues only with confirmation (BRD: Patient Management).

### 2.3 IAppointmentService (FR-APT-001 to FR-APT-003)
| Method | Input | Output | Rules | Req IDs |
|---|---|---|---|---|
| ScheduleAsync | AppointmentCreateDto | Result<AppointmentDto> | Patient must exist. StartTime on slot grid (PDR-06). No overlap with non-archived appointments. Duration 30 | FR-APT-001 |
| GetDailyAsync | date | IReadOnlyList<AppointmentDto> | Non-archived rows for that date, ordered by StartTime | FR-APT-002 |
| GetSlotsAsync | date | IReadOnlyList<SlotDto> | Returns slot state (free or taken) for the day. Grid per PDR-06 | FR-APT-001 |
| UpdateStatusAsync | id, status | Result | Status in the six lifecycle values. Only allowed moves (`19-`); no reversal | FR-APT-003 |
| ArchiveAsync | id | Result<AppointmentDto> | Sets IsArchived; status unchanged. Manual action (PDR-09). Only Completed, Cancelled or No-show may be archived (D-M3-3). Archived or unknown appointments are not found. Returns a typed result, as the codebase has no non-generic Result (Module 3, step 4) | FR-APT-003 |

### 2.4 IConsultationService (FR-VIT-001, FR-CMP-001, FR-DIA-001, FR-HIS-002)
| Method | Input | Output | Rules | Req IDs |
|---|---|---|---|---|
| StartAsync | patientId, appointmentId? | Result<ConsultationDto> | Patient must exist. Appointment (if given) must belong to the patient and must be Checked-In; otherwise an error names the current status. Sets VisitDateTime. From an appointment, sets it to In Consultation (`19-`); an appointment already linked to a consultation is rejected (PDR-26) | FR-HIS-001, FR-APT-003 |
| GetWorkingViewAsync | consultationId | ConsultationDto | Includes vitals, complaints, diagnosis and medicine lines. Read-only flag when printed | FR-VIT-001, FR-CMP-001, FR-DIA-001 |
| SaveAsync | ConsultationSaveDto, confirmWarnings | Result<ConsultationDto> | Vitals: range warnings (AD-14). Missing vitals: warning only. Complaints and diagnosis: free text. Blocked when printed | FR-VIT-001, FR-CMP-001, FR-DIA-001 |
| GetDetailAsync | consultationId | ConsultationDetailDto | Everything for the visit detail view | FR-HIS-002 |
| FinishAsync | consultationId | Result | If linked to an appointment: In Consultation → Completed. Any other appointment status is rejected, naming the current status. Walk-in consultation: no status change (DEC-022) | FR-APT-003 |

Vitals validator (warnings):

| Field | Warning when | Req IDs |
|---|---|---|
| TemperatureC | < 34.0 or > 42.0 | FR-VIT-001 |
| SystolicMmHg | < 70 or > 250 | FR-VIT-001 |
| DiastolicMmHg | < 40 or > 150 | FR-VIT-001 |
| PulseBpm | < 30 or > 220 | FR-VIT-001 |
| (all four absent) | Missing vitals warning | FR-VIT-001 |

Values are never rounded or changed by the service. The warning shows the entered value.

### 2.5 IVisitService (FR-HIS-001, FR-HIS-003)
| Method | Input | Output | Rules | Req IDs |
|---|---|---|---|---|
| ListAsync | patientId, from?, to? | IReadOnlyList<VisitSummaryDto> | Newest first. From and to are optional. Single date sets from = to | FR-HIS-001, FR-HIS-003 |

IVisitService and IConsultationService may be one class if the team prefers. The split is for readability only.

### 2.6 IPrescriptionService (FR-RX-001, FR-RX-002)
| Method | Input | Output | Rules | Req IDs |
|---|---|---|---|---|
| SaveItemsAsync | consultationId, items | Result<PrescriptionDto> | Creates the prescription if none (one per consultation). Replaces lines. Blocked when printed. Required-field rules per PDR-13 | FR-RX-001 |
| MarkPrintedAsync | consultationId | Result<PrescriptionPrintDto> | Sets IsPrinted and PrintedAt once. Blocked when there is no prescription or no medicine line (PDR-13) | FR-RX-002 |
| GetPrintModelAsync | consultationId | PrescriptionPrintDto | Header from ClinicProfile, patient details, vitals, diagnosis, medicines, footer | FR-RX-002 |

### 2.7 IExportService (FR-EXP-001, FR-EXP-002)
| Method | Input | Output | Rules | Req IDs |
|---|---|---|---|---|
| GetOptionsAsync | patientId | ExportOptionsDto | Patient name and visit date range for the form | FR-EXP-001, FR-EXP-002 |
| BuildCsvAsync | patientId, from?, to? | ExportFile (bytes, name) | Patient details with visits, vitals, diagnosis, prescriptions. Single patient, optional range | FR-EXP-001 |
| BuildPrintViewAsync | patientId, from?, to? | PrintViewDto | Same content as CSV, shaped for an A4 HTML print view. Browser Save as PDF produces the PDF (PDR-18) | FR-EXP-002 |

Export content per BRD: patient data, with its visits, vitals, diagnosis and prescriptions. CSV layout (DEC-020, resolves PDR-17): one row per visit.

| # | Column | Source | Format |
|---|---|---|---|
| 1 | PatientCode | Patient | text |
| 2 | FullName | Patient | text |
| 3 | DateOfBirth | Patient | yyyy-MM-dd |
| 4 | Gender | Patient | Male, Female or Other |
| 5 | Phone | Patient | text |
| 6 | Email | Patient | text |
| 7 | Address | Patient | text |
| 8 | VisitDate | Consultation | yyyy-MM-dd |
| 9 | VisitTime | Consultation | HH:mm, clinic local time |
| 10 | TemperatureC | VitalSign | decimal; blank if not recorded |
| 11 | SystolicMmHg | VitalSign | integer; blank if not recorded |
| 12 | DiastolicMmHg | VitalSign | integer; blank if not recorded |
| 13 | PulseBpm | VitalSign | integer; blank if not recorded |
| 14 | Complaints | Consultation | free text |
| 15 | Diagnosis | Consultation | free text |
| 16 | Medicines | PrescriptionItems | one cell; each medicine as `Name; Dosage; Frequency; Duration; Instructions` with blank parts omitted; medicines separated by ` | ` |

- Excluded: Age (calculated, not stored), prescription print status, and internal IDs.
- Format: comma-separated, header row, UTF-8 with a byte-order mark so spreadsheet tools show names correctly; values with commas or line breaks are quoted.
- A patient with no visits gives one row with the patient columns filled and the visit columns blank.
- The from and to filter applies to VisitDate.

### 2.8 IClinicProfileService (FR-RX-002)
| Method | Input | Output | Rules | Req IDs |
|---|---|---|---|---|
| GetAsync | — | ClinicProfileDto | Single row. Created blank if missing | FR-RX-002 |
| UpdateAsync | ClinicProfileDto | Result | Field rules per PDR-15 | FR-RX-002 |

## 3. Transaction Boundaries

| Use case | Boundary |
|---|---|
| RegisterAsync | One save: patient insert, then PatientCode update in the same unit of work |
| ScheduleAsync | One save, after the overlap check in the same unit of work |
| SaveAsync (consultation) | One save: vitals, complaints and diagnosis together |
| SaveItemsAsync | One save: delete and replace lines, or add prescription and lines |
| MarkPrintedAsync | One save; must not run twice (second call returns the existing printed state) |

## 4. Dependencies

| Service | Depends on (repositories) |
|---|---|
| AuthService | IUserRepository |
| PatientService | IPatientRepository, IConsultationRepository (for recent patients), IPatientValidator |
| AppointmentService | IAppointmentRepository, IPatientRepository |
| ConsultationService | IConsultationRepository, IPatientRepository, IAppointmentRepository, IVitalSignRepository |
| VisitService | IConsultationRepository |
| PrescriptionService | IPrescriptionRepository, IConsultationRepository, IClinicProfileRepository |
| ExportService | IPatientRepository, IConsultationRepository, IPrescriptionRepository, ICsvWriter |
| ClinicProfileService | IClinicProfileRepository |

`ICsvWriter` is an abstraction in the Business layer. Its layout is set by PDR-17. No PDF renderer is needed (PDR-18).

## 5. Service Rules Summary (for the test plan)

| Rule | Service | Test type |
|---|---|---|
| Age from DOB and system date | Patient | Unit |
| Duplicate patient warning | Patient | Unit |
| Vitals range warning, not block | Consultation | Unit |
| Missing vitals warning, not block | Consultation | Unit |
| 30-minute slot, no overlap | Appointment | Unit + integration |
| Allowed moves accepted; disallowed moves (including reversal) rejected | Appointment | Unit |
| Printed prescription immutable | Prescription | Unit + integration |
| One prescription per consultation | Prescription | Integration |
| Recent patients: five, by last visit | Patient | Integration |
| Visit filter by date | Visit | Integration |
| Export content and filter | Export | Unit + file check |
