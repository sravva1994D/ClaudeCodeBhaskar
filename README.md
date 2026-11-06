# Patient Management System

A lightweight, browser-based application for a general physician to manage daily clinical work: patient records, appointments, consultations (vitals, complaints, diagnosis), prescriptions, visit history, and data export.

The goal is to cut paper-based workflows, speed up consultations, and keep patient history accurate and easy to find.

> Scope and requirements: [`BRD/Doc_BRD.md`](BRD/Doc_BRD.md)
> Engineering rules for contributors and AI assistants: [`CLAUDE.md`](CLAUDE.md)

---

## Phase 1 Features

- Patient registration and profile management
- Appointment scheduling and daily list (Scheduled / Completed / Cancelled / No-show)
- Mandatory vitals capture (temperature, blood pressure, pulse)
- Complaints and diagnosis notes
- Medication and printable prescriptions
- Patient visit history with date filter
- Search by name or phone number
- Export to CSV and PDF

Out of scope for Phase 1 is listed in the BRD (for example: multi-user access, billing, mobile app, offline mode).

---

## Technology Stack

| Layer | Technology |
|---|---|
| Frontend | Angular (latest stable) |
| Backend | .NET Web API (latest LTS) |
| Database | SQL Server |

---

## Repository Layout

```
.
├── BRD/                  Business Requirements Document
├── Backend/              .NET Web API solution
├── Database/             SQL Server scripts and EF Core migrations reference
├── Documentation/        Traceability matrix, ADRs, test plan
├── Frontend/             Angular application
├── Reference/            Training and progress plan
├── CLAUDE.md             Engineering and Git rules
└── README.md
```

---

## Getting Started

> Setup steps will be completed as the Backend and Frontend projects are created. Until then, this section lists the prerequisites.

### Prerequisites

- Git
- .NET SDK (latest LTS)
- Node.js (LTS) and Angular CLI (latest stable)
- SQL Server (Developer edition, LocalDB, or a container)

### Planned steps

1. Clone the repository.
2. Restore backend packages and apply database migrations.
3. Configure connection string and secrets through environment variables or user secrets (never commit them).
4. Run the Web API.
5. Install frontend packages and start the Angular dev server.

---

## Contributing

- Never commit or push directly to `main`. All work is done on a `feature/*`, `fix/*`, `docs/*`, or `chore/*` branch, one feature per branch, one PR per feature.
- Each feature is developed in its own Git worktree (see [CLAUDE.md §8](CLAUDE.md#8-worktree-strategy)).
- PR descriptions list the requirement IDs they implement.
- Commit messages follow Conventional Commits (see [CLAUDE.md §7.2](CLAUDE.md#72-commit-messages-conventional-commits)).

---

## Documentation

| Document | Purpose |
|---|---|
| [BRD](BRD/Doc_BRD.md) | Business and functional requirements |
| [CLAUDE.md](CLAUDE.md) | Governance, Git workflow, coding and testing standards |
| `Documentation/traceability.md` | Requirement-to-code-to-test mapping (planned) |
| `Documentation/adr/` | Architecture decision records (planned) |

---

## Privacy and Security

This system holds patient health information. Do not place real patient data in the repository, test fixtures, screenshots, logs, or issue/PR text. Use synthetic data only.
