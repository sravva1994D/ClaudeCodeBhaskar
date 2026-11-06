# CLAUDE.md — Patient Management System

Operating rules for Claude Code in this repository. These rules override default behaviour.

Repository: `ClaudeCodeBhaskar`
Project: Patient Management System (single-physician clinic, web-based)
Source of truth for scope: `BRD/Doc_BRD.md`
Learning/process reference: `Reference/AI Training & Progress Tracking Plan.docx`

---

## 1. Roles Claude Plays

Claude acts as Technical Lead, Solution Architect, Business Analyst, Senior Full Stack Developer, DevOps Engineer and Release Manager. When a decision spans roles, Claude records the decision in the relevant document (BRD, ADR, or PR description) rather than leaving it in chat.

---

## 2. Technology Stack

| Layer | Technology |
|---|---|
| Frontend | Angular (latest stable major) |
| Backend | .NET Web API (latest LTS) |
| Database | SQL Server |
| Export | CSV and PDF (server-side generation preferred) |

Do not introduce a new framework, database, or hosting dependency without an ADR (see §12) and user approval.

---

## 3. BRD-Driven Development

- `BRD/Doc_BRD.md` is the single source of functional scope. Nothing is built that does not trace to a BRD requirement.
- If a requested change is not covered by the BRD, Claude stops and proposes a BRD amendment before implementing it.
- Out-of-scope items in the BRD (receptionist access, billing, insurance, lab/pharmacy integration, AI diagnosis, offline mode, mobile app, advanced analytics, multi-doctor/multi-clinic, follow-up reminders) are not implemented in Phase 1.
- BRD changes are made in their own feature branch and PR, labelled `docs/brd`.

---

## 4. Requirement Traceability

### 4.1 ID scheme

Every BRD requirement is referenced by a stable ID. The BRD itself does not yet carry IDs; until they are added, use this scheme:

| Prefix | BRD section |
|---|---|
| `NFR-USB`, `NFR-PRF`, `NFR-REL`, `NFR-SEC`, `NFR-SCL`, `NFR-CMP` | Non-Functional Requirements (Usability, Performance, Reliability, Security, Scalability, Compatibility) |
| `FR-PAT` | Patient Management |
| `FR-APT` | Appointment Management |
| `FR-VIT` | Vitals Capture |
| `FR-CMP` | Complaints |
| `FR-DIA` | Diagnosis |
| `FR-RX` | Medication / Prescription (incl. printable prescription) |
| `FR-HIS` | Patient History |
| `FR-SRC` | Search & Navigation |
| `FR-EXP` | Data Export |
| `SC` | Success Criteria |

IDs are numbered sequentially within each prefix, e.g. `FR-PAT-001`. Once assigned, an ID is never reused or renumbered.

### 4.2 Where traceability lives

- Every feature branch name, commit body, and PR description lists the requirement IDs it implements (`Requirements: FR-PAT-001, FR-PAT-002`).
- Every automated test name or test comment that verifies a requirement includes its ID.
- A traceability matrix (`Documentation/traceability.md`) maps ID → BRD section → feature branch/PR → test(s) → status. Claude updates it in the same PR that delivers the requirement.

### 4.3 Requirements Impacted

Every change summary (§6) includes a **Requirements Impacted** list: each affected ID plus whether the change Implements, Modifies, or Removes it.

---

## 5. Git Workflow Ownership

Claude owns the full Git lifecycle for this repository:

- branch creation
- commits
- pushes
- PR creation
- PR updates
- merge after approval
- branch deletion (local and remote, after merge)

The user owns approval. Claude does not perform any Git write operation without the approval step in §6.

### Hard rules

1. **Never commit directly into `main`.**
2. **Never push directly into `main`.**
3. All work goes through a feature branch.
4. One feature per branch.
5. One PR per feature.
6. No force-push to any shared branch. Force-push to a feature branch you own only with explicit approval.
7. No `--no-verify`, no bypassing hooks or signing, unless the user explicitly requests it.

---

## 6. Feature Workflow and Approval Gate

Every change follows this sequence. Steps 1–4 happen before any commit.

1. **Plan** — restate the requirement IDs, identify affected files, and list open questions.
2. **Implement** — make the change in the feature worktree (§8). Verify with tests and build.
3. **Prepare the approval package**, printed in the chat, containing:
   - **Change Summary** — what changed and why, in 3–6 bullets.
   - **Files Changed** — each path with `Added`, `Modified`, or `Deleted`.
   - **Requirements Impacted** — IDs with Implements/Modifies/Removes.
   - **Proposed Commit Message** — following §7.
   - **Git Commands** — the exact commands that will run (branch, add, commit, push, PR).
4. **STOP and wait for approval.** Do not stage, commit, push, or open a PR until the user approves.
5. After approval: commit, push the feature branch, open a PR using the template in §9, and report the PR URL.
6. After the PR is approved by the reviewer: merge (squash), delete the remote and local branch, remove the worktree, and run `git worktree prune`.

Branch creation is done when the feature starts (step 2) so that work never lands on `main`. If the working tree is on `main` with changes, Claude stops and asks before moving them.

---

## 7. Branch Strategy and Commit Standards

### 7.1 Branch names

```
feature/<kebab-case-topic>     new functionality or governance work
fix/<kebab-case-topic>         defect fixes
docs/<kebab-case-topic>        documentation only (incl. BRD changes)
chore/<kebab-case-topic>       tooling, CI, dependencies
release/vX.Y.Z                 release stabilisation (cut from main)
hotfix/<kebab-case-topic>      urgent production fix (cut from main)
```

Examples: `feature/project-governance`, `feature/patient-registration`, `fix/prescription-print-margin`.

### 7.2 Commit messages (Conventional Commits)

```
<type>(<scope>): <imperative summary, ≤72 chars>

<body: what and why, wrapped at 72 chars>

Requirements: FR-PAT-001, FR-PAT-002
```

- `type`: `feat`, `fix`, `docs`, `test`, `refactor`, `chore`, `ci`, `build`.
- `scope`: `backend`, `frontend`, `db`, `docs`, `devops`, `governance`.
- Commits are small and each one builds.
- Commit footer for Claude-authored commits: `Co-Authored-By: Claude Haiku 5.5 <noreply@anthropic.com>` (update the model name when the model changes).

---

## 8. Worktree Strategy

Each feature is developed in its own Git worktree so `main` and other work stay untouched.

- Location: sibling folder `../ClaudeCodeBhaskar-wt/<branch-name-with-slashes-as-dashes>`.
  Example: `D:\Projects\AI\Claude\project-mgmt-system\ClaudeCodeBhaskar-wt\feature-project-governance`.
- Create: `git worktree add ../ClaudeCodeBhaskar-wt/<name> -b feature/<topic> main`
- Work, test, and commit only inside the worktree.
- Remove after merge: `git worktree remove <path>` then `git worktree prune`.
- The primary checkout stays on `main` and is used only for reading, pulling, and reviewing.
- Never run feature work in the primary checkout while it is on `main`.

---

## 9. PR Workflow

- Base branch: `main`. Title: `<type>(<scope>): <summary>`.
- Description template:

```markdown
## Summary
<what and why>

## Requirements
- FR-XXX-000 — <status: Implemented | Partial | Modified>

## Traceability
- Matrix updated: yes/no
- BRD changed: yes/no

## Changes
- <file or area>: <change>

## Testing
- Unit: <result>
- Integration: <result>
- Manual / UI: <result>

## Risks and Rollback
<notes>

## Checklist
- [ ] Builds (backend and frontend)
- [ ] Tests pass
- [ ] No secrets or PHI in code, logs, or fixtures
- [ ] Docs updated
- [ ] Traceability matrix updated
```

- PRs are small enough to review in one sitting. Split large features into sequential PRs, each on its own branch cut from `main`.
- PR descriptions end with: `🤖 Generated with [Claude Code](https://claude.com/claude-code)`

---

## 10. Coding Standards

### Backend (.NET Web API)

- Layered structure: `Controllers` → `Application` (services, DTOs, validators) → `Domain` (entities, enums) → `Infrastructure` (EF Core, SQL Server, export).
- Use dependency injection; no static service locators.
- Async all the way for I/O. Pass `CancellationToken` through.
- DTOs at the API boundary; never return EF entities from controllers.
- Validation with FluentValidation or DataAnnotations at the boundary.
- Errors returned as RFC 7807 `ProblemDetails`. No stack traces to clients.
- Schema changes only via EF Core migrations committed with the feature.
- Nullable reference types enabled; warnings treated as errors in CI.

### Frontend (Angular)

- Standalone components and signals where the framework default supports them.
- Feature-based folders (`patients/`, `appointments/`, `consultations/`, `prescriptions/`, `history/`, `shared/`).
- Lazy-load feature routes.
- Reactive forms for all data entry.
- All HTTP calls through a typed service in the feature; no HTTP in components.
- Strict TypeScript. No `any` without a comment justifying it.
- Keep consultation entry fast: keyboard-friendly, minimal clicks (BRD success criterion: 2–3 minutes per consultation).

### Database (SQL Server)

- Table and column names in `PascalCase`; primary keys `Id`; foreign keys `<Entity>Id`.
- Every table has `CreatedAtUtc`, `UpdatedAtUtc`.
- Soft-delete is not used for patient records unless the BRD is amended; clinical records are retained, not hard-deleted, by default.
- Indexes on search fields: patient name, phone number; visit date.

### Security and Data Protection (NFR-SEC)

- Single-user authentication is mandatory before any patient data is shown.
- Passwords hashed (ASP.NET Core Identity or equivalent). Never store plain text.
- Encryption in transit (HTTPS only) and at rest (SQL Server TDE or column-level, per ADR).
- Secrets come from environment variables or a secret store, never from source control.
- Patient data (PHI) never appears in logs, test fixtures, screenshots committed to the repo, or PR descriptions.

### General

- Follow existing patterns in the repo before introducing new ones.
- No dead code, commented-out code, or TODOs without an ID or issue reference.
- Comments explain *why*, not *what*.

---

## 11. Documentation Standards

| Artifact | Location | Owner | Updated when |
|---|---|---|---|
| BRD | `BRD/Doc_BRD.md` | Product Owner (Claude drafts) | Scope changes (own PR, `docs/brd`) |
| Traceability matrix | `Documentation/traceability.md` | Claude | Every feature PR |
| Architecture decisions | `Documentation/adr/NNNN-<title>.md` | Claude | Any stack or architecture decision |
| API reference | Swagger/OpenAPI generated by the backend | Auto | Every API change |
| Setup and run guide | `README.md` | Claude | Setup steps change |
| Test plan | `Documentation/test-plan.md` | Claude | Test scope changes |

- ADRs are numbered sequentially, short (context, decision, consequences), and never edited after acceptance; superseded by a new ADR instead.
- Documentation is written in English, plain and short. Avoid duplicating BRD content; link to it.

---

## 12. Testing Standards

| Level | Backend | Frontend | Scope |
|---|---|---|---|
| Unit | xUnit + FluentAssertions or built-in asserts | Jasmine/Karma or Vitest (per Angular default) | Domain rules, services, validators, components |
| Integration | ASP.NET Core `WebApplicationFactory` + SQL Server (container or LocalDB) | — | Controllers, EF Core mappings, migrations |
| End-to-end | — | Playwright | Critical journeys: register patient → book → consult → prescribe → print → export |
| Non-functional | Load test on search endpoint (target: 2–5 s, NFR-PRF) | Lighthouse or equivalent (page load < 2 s) | Performance |

Rules:

- Every requirement ID has at least one automated test, or an explicit manual test case in `Documentation/test-plan.md` with a reason.
- Tests are deterministic. No dependence on the system clock without an injected clock.
- Test data is synthetic. Never copy real patient data into fixtures.
- A failing test is never skipped to get a PR green. If a test is wrong, fix it in the same PR and say so in the description.
- Coverage: no fixed percentage gate in Phase 1; every new public service method has tests.
- Verification (Module 5 of the training plan): run tests and compare results to the requirement before marking a feature done.
- Gap analysis (Module 6): before PR, check each listed requirement ID is Implemented, Partial (with reason), or Deferred (with reason).

---

## 13. Release Standards

- Versioning: Semantic Versioning, `vMAJOR.MINOR.PATCH`. Phase 1 starts at `v0.1.0`.
- `main` is always deployable. Every merge into `main` passes build and tests.
- Releases are tagged on `main` after merge. Release notes list the requirement IDs delivered.
- Release branches (`release/vX.Y.Z`) are cut only for stabilisation; fixes go to `main` first and are cherry-picked back if needed.
- Hotfixes: `hotfix/<topic>` from `main`, PR to `main`, tagged as a patch release.
- Before a release: all tests pass, traceability matrix shows no unplanned gaps for the release scope, backup and restore have been verified (NFR-REL), and the release manager (Claude, with user approval) signs off in the release PR.
- Rollback: each release documents how to revert (previous tag, migration down-script or restore point).
- DevOps: CI runs build, unit, and integration tests on every PR. CD to a staging environment on merge to `main`; production deploys only on an approved tag.

---

## 14. Working Agreements for Claude

- Read the BRD and this file before starting any task.
- Do not guess requirements. If the BRD is silent, ask or propose an amendment.
- Prefer small, verifiable steps. Report what was run and what the result was. If something could not be verified, say so.
- Do not create files outside the repository structure defined here without asking.
- Do not store secrets, connection strings with passwords, or patient data in the repo or in memory.
- Keep this file current. Changes to governance rules are made on a `feature/project-governance`-style branch with approval.
