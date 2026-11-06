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
├── .claude/agents/       Project-specific Claude Code subagents (PMS stages and modules)
├── Agents/               Generic, reusable workflow agent definitions (reference)
├── Backend/              .NET Web API solution
├── Database/             SQL Server scripts and EF Core migrations reference
├── Documentation/        Traceability matrix, ADRs, test plan
├── Frontend/             Angular application
├── Reference/            Training and progress plan
├── .gitignore            Build output, secrets, and patient-data exclusions
├── CLAUDE.md             Governance and engineering rules
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

## Branching Model

```
main            production; release PRs only (tagged vX.Y.Z)
  ↑
development     integration branch; all feature PRs; integration and verification happen here
  ↑
feature/*       cut from development; kept after merge until the release is verified
```

## Contributing

- Never commit or push directly to `main` or `development`. Never merge a feature branch directly into `main`.
- Each feature is a `feature/*`, `fix/*`, `docs/*`, or `chore/*` branch cut from `development`, in its own Git worktree. One feature per branch, one PR per feature, targeting `development`.
- Feature branches are kept after merge. They are deleted only after the release is deployed and verified, with the owner's approval.
- A release is one `development` → `main` PR per module or release scope, merged with a merge commit and tagged `vX.Y.Z`.
- Every change is approved before commit, push, PR, merge, or branch deletion. The change package lists the changed files, requirements impacted, test results, documentation updated, and the proposed commit message.
- PR descriptions list the requirement IDs they implement.
- Commit messages follow Conventional Commits.
- Full rules: [CLAUDE.md §6 Git Workflow](CLAUDE.md#6-git-workflow-standards), [§7 Change Management](CLAUDE.md#7-change-management-process), [§8 Worktrees](CLAUDE.md#8-worktree-strategy), [§9 Pull Requests](CLAUDE.md#9-pull-request-workflow), [§13 Release](CLAUDE.md#13-release-workflow).

---

## Documentation

| Document | Purpose |
|---|---|
| [BRD](BRD/Doc_BRD.md) | Business and functional requirements |
| [CLAUDE.md](CLAUDE.md) | Governance: goals, BRD and traceability, agents, Git, PR, release, coding, testing, documentation |
| `Agents/` | Generic workflow agent definitions (reference) |
| `.claude/agents/` | Project agents for the workflow stages and BRD modules |
| [Traceability matrix](Documentation/traceability.md) | Requirement-to-module-to-test mapping and status |
| `Documentation/adr/` | Architecture decision records (planned) |

---

## Privacy and Security

This system holds patient health information. Do not place real patient data in the repository, test fixtures, screenshots, logs, or issue/PR text. Use synthetic data only.
