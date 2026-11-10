# 02 — Three-Tier Architecture Diagram

**Source:** `01-Solution-Architecture.md` (AD-01, AD-02, AD-03)
**Status:** Proposed for review

## 1. Logical View

```mermaid
flowchart TB
  Browser["Browser<br/>Chrome / Edge / Safari (desktop)"]

  subgraph PRES["Presentation Layer — PatientManagement.Web (ASP.NET Core MVC)"]
    CTRL["Controllers"]
    VIEWS["Razor Views"]
    VM["ViewModels"]
    AUTH["Cookie auth, global [Authorize], anti-forgery"]
  end

  subgraph BIZ["Business Layer — PatientManagement.Business"]
    SVC["Services (interfaces + implementations)"]
    DTO["DTOs"]
    VAL["Validators (errors + warnings)"]
    MAP["Mappers (entity ↔ DTO)"]
  end

  subgraph DATA["Data Layer — PatientManagement.Data"]
    REPO["Repositories"]
    CTX["EF Core DbContext"]
    ENT["Entities"]
    MIG["Migrations"]
  end

  DB[("SQL Server<br/>localhost — database PatientManagement")]

  Browser -->|"HTTP on localhost; HTTPS beyond localhost"| CTRL
  CTRL --> VM
  VIEWS --> VM
  CTRL -->|"calls interfaces"| SVC
  SVC --> DTO
  SVC --> VAL
  SVC --> MAP
  SVC -->|"calls"| REPO
  REPO --> CTX
  CTX --> ENT
  CTX --> DB
  MIG --> DB
  AUTH -.-> CTRL
```

## 2. Dependency Rules

```mermaid
flowchart LR
  WEB["PatientManagement.Web"] --> BUS["PatientManagement.Business"]
  BUS --> DAT["PatientManagement.Data"]
  WEB -. "DI registration only" .-> DAT
```

| Rule | Allowed | Forbidden |
|---|---|---|
| Web → Business | Service interfaces, DTOs | Calling repositories or DbContext directly |
| Business → Data | Repository calls, entities inside Business internals | Returning entities to Web |
| Data → Business or Web | — | Any reference upward |
| Web → Data | Startup registration only (`AddPmsData`) | Querying data from controllers |

## 3. Request Flow (example: register a patient, FR-PAT-001)

```mermaid
sequenceDiagram
  participant U as Physician (browser)
  participant C as PatientsController (Web)
  participant S as IPatientService (Business)
  participant V as PatientValidator (Business)
  participant R as IPatientRepository (Data)
  participant DB as SQL Server
  U->>C: POST /Patients/Create (form)
  C->>S: RegisterAsync(PatientCreateDto, confirmDuplicate)
  S->>V: Validate(dto)
  V-->>S: errors, warnings
  alt errors present
    S-->>C: Result (errors)
    C-->>U: Re-render form with messages
  else duplicate warning and not confirmed
    S-->>C: Result (warning: possible duplicate)
    C-->>U: Re-render with "Continue anyway"
  else ok
    S->>R: Add(patient) and SaveChanges
    R->>DB: INSERT Patients
    DB-->>R: Id
    S->>R: Set PatientCode = PT-{Id:000000}
    S-->>C: Result (ok, PatientId)
    C-->>U: Redirect to Patient Profile (SCR-06)
  end
```

## 4. Layer-to-Screen Mapping

| Screen group | Presentation | Business | Data |
|---|---|---|---|
| Login, forgot password (SCR-01, SCR-02) | AccountController, ViewModels | IAuthService | Users table |
| Home, search, patient screens (SCR-03 to SCR-07) | Patients, Home controllers | IPatientService, IVisitService | Patients, Consultations |
| Appointments (SCR-09, SCR-10) | AppointmentsController | IAppointmentService | Appointments |
| Consultation and prescription (SCR-11, SCR-12) | ConsultationsController, PrescriptionsController | IConsultationService, IPrescriptionService | Consultations, VitalSigns, Prescriptions, PrescriptionItems |
| Export (SCR-13) | ExportController | IExportService | Patients, Consultations, VitalSigns, PrescriptionItems |
| Clinic profile (SCR-14, PDR-15) | ClinicProfileController | IClinicProfileService | ClinicProfile |
