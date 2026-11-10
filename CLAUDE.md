# CLAUDE.md — Patient Management System (Phase 1)

Operating rules for Claude Code in this repository. These rules override default behaviour.

**Repository:** `ClaudeCodeBhaskar`
**Project:** Patient Management System, a web-based application for one general physician. Localhost-only learning project. Not deployed.
**Source of truth for scope:** `BRD/Doc_BRD.md`
**Decisions:** `Documentation/02-Planning/17-Planning-Decisions-and-Open-Items.md` (all 32 decided 2026-11-10)
**Process reference:** `Reference/AI Training & Progress Tracking Plan.docx`

This file replaces the earlier CLAUDE.md (deleted in commit `8400ffb`). Its Angular and Web API stack is superseded by the architecture below.

---

## 1. Roles

Claude acts as Business Analyst, Product Owner support, Solution Architect, Technical Lead and Developer for this project. Decisions are recorded in the BRD, the planning files or the decision log, not left in chat.

The Project Owner is the sole decision-maker and approver (DEC-010). They approve BRD changes, priorities and open decisions.

---

## 2. Technology (decided)

| Layer | Technology | Decision |
|---|---|---|
| Presentation | ASP.NET Core MVC, Razor Views, ViewModels, Controllers | Requester direction; `Documentation/02-Planning/01-` |
| Business | C# services, interfaces, DTOs, validators | Requester direction |
| Data access | Entity Framework Core only | PDR-32. Dapper is not used |
| Database | SQL Server (local instance) | Requester direction |
| Printing and PDF | Printable HTML views with a Print button; browser Print and Save as PDF | PDR-16, PDR-18. No PDF library and no server-side PDF generation |
| Authentication | Cookie authentication; ASP.NET Core password hasher; custom Users table | PDR-04 |
| Hosting | localhost only | CON-022 |

Do not add a framework, library, database or hosting dependency without a Project Owner decision recorded in the decision log.

Target versions: latest LTS .NET at the start of implementation; the local SQL Server version. Record both in the repository README (PDR-31).

---

## 3. BRD-Driven Development

- `BRD/Doc_BRD.md` is the single source of functional scope. Nothing is built that does not trace to a BRD requirement.
- If a requested change is not in the BRD, stop and propose a BRD amendment first (CON-017).
- Out-of-scope items (OOS-001 to OOS-015) are not implemented in Phase 1.
- Do not guess requirements. Where the BRD is silent, record a planning decision (PDR) and ask the Project Owner.

---

## 4. Requirement Traceability

### 4.1 ID scheme

| Prefix | BRD section |
|---|---|
| `FR-PAT` | Patient Management |
| `FR-APT` | Appointment Management |
| `FR-VIT` | Vitals Capture |
| `FR-CMP` | Complaints |
| `FR-DIA` | Diagnosis |
| `FR-RX` | Medication and printable prescription |
| `FR-HIS` | Patient History |
| `FR-SRC` | Search and Navigation |
| `FR-EXP` | Data Export |
| `NFR-USB`, `NFR-PRF`, `NFR-REL`, `NFR-SEC`, `NFR-SCL`, `NFR-CMP` | Non-functional requirements |
| `SC` | Success criteria |

IDs are numbered sequentially within each prefix (for example `FR-PAT-001`). An ID is never reused or renumbered.

### 4.2 Where traceability lives

- `Documentation/01-Brainstorming/12-Requirement-Traceability-Matrix.md`: requirement to BRD section, priority, objective, criterion, acceptance and status.
- `Documentation/02-Planning/18-Planning-Traceability-Matrix.md`: requirement to screen, route, service, repository, table, test and work package.
- Every test that verifies a requirement includes its ID in the name or a comment.
- Every commit body and PR description lists the IDs it implements (`Requirements: FR-PAT-001, FR-PAT-002`).

When a change affects a requirement, update both matrices in the same change.

---

## 5. Git Workflow (user instruction, 2026-11-10)

- **`development` is the only working branch.** All project work is committed directly to it: BRD, requirements, planning, traceability, architecture, CLAUDE.md, analysis, supporting documentation and implementation. Commits are made regularly and pushed to `origin/development` under the workflow in §5.4. Each commit is reported in chat with its files and message.
- **No other working branches.** No feature, module, milestone, documentation, fix, chore or hotfix branches.
- **`main` receives releases only**: one PR from `development` to `main`, after the final review and the Project Owner's approval. Nothing is committed to `main` directly.
- Urgent fixes are committed to `development` and released in the next release (no hotfix branch).
- No force-push to shared branches.
- No `--no-verify`, no skipped hooks, no bypassed signing, unless the user asks for it.
- Tags and deletions need the user's approval. The release PR and its merge follow §5.4 (D-11). Commits and pushes to `development` follow §5.4.

### 5.1 Approval gate

Before a tag, a deletion, or any change outside the workflow in §5.4, Claude presents an approval package in chat. The release PR and its merge follow §5.4 (D-11):

1. **Change summary**: what changed and why, in 3 to 6 bullets.
2. **Files changed**: each path marked Added, Modified or Deleted.
3. **Requirements impacted**: IDs with Implements, Modifies or Removes.
4. **Proposed commit message** (§5.2).
5. **Git commands** that will run.

Claude stops and waits for approval before those steps. Routine commits to `development` follow §5.4.

### 5.2 Branch and commit format

Branch: `development` only. No other working branches are created.

Commit message (Conventional Commits):

```
<type>(<scope>): <imperative summary, 72 characters or fewer>

<body: what and why, wrapped at 72 characters>

Requirements: FR-PAT-001, FR-PAT-002
```

- `type`: `feat`, `fix`, `test`, `refactor`, `docs`, `chore`.
- `scope`: `web`, `business`, `data`, `tests`, `docs`.
- Each commit builds.
- Claude-authored commits end with: `Co-Authored-By: Claude Haiku 5.5 <noreply@anthropic.com>`

### 5.3 Release PR description

Used only for releases to `main`. Work on `development` has no pull request.

- Title: `release: vX.Y.Z`. Base branch: `main`. Head: `development`.
- Sections: Summary, Requirements (with status), Traceability (matrices updated yes or no; BRD changed yes or no), Changes, Testing (unit, integration, manual), Risks, Checklist.
- Ends with: `🤖 Generated with [Claude Code](https://claude.com/claude-code)`

---

### 5.4 Development workflow (approved 2026-11-10; simplified by D-9 to D-11)

1. Work directly in `development`.
2. Commit and push documentation and code changes regularly, using the format in §5.2.
3. Complete all project features.
4. Test the full application. Fix defects in `development`.
5. Create one PR: `development` → `main`, once the whole application is complete and tested.
6. Merge using a merge commit (no squash, no rebase).

Each commit builds (implementation commits) and passes its checks before it is pushed. Each commit is listed in chat with its files and message.

No feature, hotfix, documentation or milestone branches are used. No detached worktrees or parallel verification environments are used. All development, testing and validation happen in `development`.

---

## 6. Architecture Rules (AD-01 to AD-15)

Full detail: `Documentation/02-Planning/01-Solution-Architecture.md`.

- Three projects: `PatientManagement.Web` (Presentation), `PatientManagement.Business` (Business), `PatientManagement.Data` (Data), plus test projects per layer.
- Dependencies: Web → Business → Data. Web references Data only for dependency-injection registration.
- Controllers hold no business rules and do not use EF Core. Services hold the rules.
- EF Core entities stay in `PatientManagement.Data`. Services return DTOs. Views use ViewModels.
- Repositories are thin, one per aggregate. No generic repository. `DbContext.SaveChangesAsync` is the unit of work.
- No `ClinicId` or tenant columns (single clinic).
- Dates and times are local clinic time. No UTC conversion.
- No database check constraints on vital-sign ranges. Ranges are warnings.
- No hard deletes. Appointments are archived. Patients are never deleted or archived in Phase 1 (PDR-25).
- Schema changes only through EF Core migrations, committed with the feature.

---

## 7. Business Rules to Keep Consistent

| Area | Rule | Source |
|---|---|---|
| Patient | Name, DOB, Gender (Male, Female, Other), phone, email, address required; DOB not in the future; phone 7 to 15 digits; email standard format | BRD; PDR-01, PDR-22 |
| Patient code | `PT-` + 6-digit zero-padded identity, for example `PT-000123` | BRD; AD-09 |
| Duplicates | Exact match on name (case-insensitive), DOB or phone: warn and allow continue | PDR-10 |
| Search | Contains match, case-insensitive, no minimum length | PDR-23 |
| Appointments | 30-minute slots; 09:00–13:00 and 17:00–20:00 (last slots 12:30 and 19:30); no overlap | PDR-06 |
| Slot holding | Scheduled, Checked-In, In Consultation and Completed hold the slot; Cancelled and No-show release it | PDR-07 |
| Status lifecycle | Scheduled → Checked-In → In Consultation → Completed; Scheduled → Cancelled or No-show; Checked-In → Cancelled. No reversal. Completed only after In Consultation | PDR-08, PDR-27; `Documentation/02-Planning/19-` |
| Archiving | Manual action; archived appointments hidden everywhere | PDR-09 |
| Vitals | Temperature 34.0–42.0 °C; systolic 70–250 mmHg; diastolic 40–150 mmHg; pulse 30–220 bpm. Out of range: warning and confirmation. Missing vitals: notice only, no confirm step | BRD; PDR-19 |
| Consultation | Same record as a visit. Can exist without an appointment. Each appointment leads to at most one consultation. Visit date set by the system and not editable | BRD; PDR-24, PDR-26 |
| Complaints and diagnosis | Free text; one block per visit | PDR-12 |
| Medicines | Only Name required. A prescription needs at least one medicine to print | PDR-13 |
| Prescription | One per consultation. Print command marks it printed. Reprint allowed. No edits after printing | PDR-14 |
| Recent patients | Five most recently visited (consultations only), by last visit | PDR-11 |
| Export | CSV: one file, one row per visit, patient details on each row. PDF: printable view and browser Save as PDF | PDR-17, PDR-18 |
| Login | Username and password. 8 to 16 characters with upper case, lower case, digit and symbol. 24-hour sliding idle timeout | PDR-02, PDR-20 |
| Forgot password | Email, new password, confirm. Message: "If the email matches an account, the password has been reset." No second verification (DEC-009) | PDR-03 |
| Single account | Created once by a seed script. Password never stored in the repository | PDR-05 |

If code and this table disagree, the BRD and the planning decisions win. Fix the code or record an amendment.

---

## 8. Coding Standards

- Follow the existing patterns in the repository before introducing new ones.
- Dependency injection throughout. No static service locators.
- Async for all I/O. Pass `CancellationToken` through.
- Nullable reference types enabled.
- Validation in the Business layer, returning errors and warnings (AD-11). Browser checks are convenience only.
- Passwords hashed with the ASP.NET Core password hasher. Never stored or logged in plain text.
- Comments explain *why*, not *what*. No commented-out code. No TODO without a requirement ID or decision reference.
- Patient data never appears in logs, test fixtures, screenshots committed to the repository, or PR descriptions.

---

## 9. Security

- Authentication is required before any patient data is shown (NFR-SEC-001).
- Anti-forgery tokens on every POST.
- Cookie: HttpOnly, SameSite Lax, 24-hour sliding expiry.
- HTTPS required beyond localhost. Plain HTTP is allowed on localhost (PDR-21).
- Storage encryption at rest is SQL Server Transparent Data Encryption (PDR-21).
- Secrets come from environment variables or a secret store. Never from source control. No connection string with a password in the repository.

---

## 10. Testing

Full detail: `Documentation/02-Planning/14-Testing-Strategy.md`.

| Level | Tooling | Scope |
|---|---|---|
| Unit | xUnit with NSubstitute | Services, validators, mappers, rules |
| Integration (data) | xUnit against a real SQL Server test database | Repositories, constraints, migrations (no in-memory provider) |
| Integration (web) | ASP.NET Core `WebApplicationFactory` | Controllers, authentication, anti-forgery |
| Browser | Manual, following the acceptance script | Latest Chrome, Edge and Safari (desktop). Record browser version per run (PDR-28). No browser automation in Phase 1 (PDR-29) |
| Performance | Browser timing, median of 5 runs, on 1,000 seeded patients | NFR-PRF-001, NFR-PRF-002 (PDR-30) |

Rules:
- Every in-scope requirement has at least one test. Test IDs follow `TC-<Req ID>-<n>`.
- Test data is synthetic. Never copy real patient data into fixtures.
- Do not skip a failing test to get a build green. If a test is wrong, fix it and say so in the PR.
- Tests are deterministic. Inject the clock.
- Report test results exactly. If something cannot be verified, say so.

---

## 11. Documentation Standards

| Artifact | Location | Updated when |
|---|---|---|
| BRD | `BRD/Doc_BRD.md` | Scope or wording changes, after Project Owner approval |
| Module 1 (requirements) | `Documentation/01-Brainstorming/` | Requirements or statuses change |
| Module 2 (planning) | `Documentation/02-Planning/` | Design changes. Decisions go in `17-` |
| Architecture decisions | `Documentation/02-Planning/01-Solution-Architecture.md` (AD table) | Architecture changes |
| Setup and run guide | `README.md` | Setup steps change |

- Write plain, short English. Link to the BRD rather than copying it.
- Decisions are numbered (PDR, DEC) and never deleted. Superseded items are marked superseded with the date.

---

## 12. Working Agreements

- Read the BRD, this file and the relevant planning file before starting any task.
- Prefer small, verifiable steps. Say what was run and what the result was.
- Ask the Project Owner when the BRD is silent. Do not fill the gap with an assumption.
- Keep this file current. Changes to governance rules need the Project Owner's approval first.
- Do not create files outside the repository structure without asking.
- Do not store secrets, passwords, connection strings or patient data in the repository or in memory.
