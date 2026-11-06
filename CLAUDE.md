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
- branch deletion (local and remote), only after the release is deployed and verified and the release owner approves

The user owns approval. Claude does not perform any Git write operation without the approval step in §6.

### Hard rules

1. **Never commit directly into `main` or `development`.**
2. **Never push directly into `main` or `development`.**
3. All work goes through a feature branch cut from `development`.
4. One feature per branch.
5. One PR per feature. Feature PRs target `development`. Only the release PR targets `main`.
6. Feature branches are kept after merge into `development` until the release is deployed and verified. Deleting them needs the release owner's approval (§6, step 8).
7. No force-push to any shared branch (`main`, `development`). Force-push to a feature branch you own only with explicit approval.
8. No `--no-verify`, no bypassing hooks or signing, unless the user explicitly requests it.

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
5. After approval: commit, push the feature branch, open a PR targeting `development` using the template in §9, and report the PR URL.
6. After the reviewer approves: squash-merge into `development`. Keep the feature branch. Remove only the worktree: `git worktree remove <path>`, then `git worktree prune`.
7. **Release (per module or release scope):** when every feature in scope is merged into `development` and validated there (build, unit and integration tests, staging verification), prepare a release PR `development` → `main`. The approval package lists every merged PR, its requirement IDs, and its test results. The release owner approves. Merge with a merge commit, not a squash, so each feature stays visible in `main`'s history. Then tag, deploy, and verify (§13).
8. **Cleanup:** only after the release is deployed and verified, and the release owner approves, delete the merged feature branches (remote and local). List the branches first and delete only those approved.

Branch creation is done when the feature starts (step 2), cut from the latest `development`, so that work never lands on `main` or `development`. If the working tree is on `main` or `development` with changes, Claude stops and asks before moving them.

---

## 7. Branch Strategy and Commit Standards

### 7.1 Branch names

```
feature/<kebab-case-topic>     new functionality or governance work; cut from development, PR to development
fix/<kebab-case-topic>         defect fixes; cut from development, PR to development
docs/<kebab-case-topic>        documentation only (incl. BRD changes); cut from development
chore/<kebab-case-topic>       tooling, CI, dependencies; cut from development
hotfix/<kebab-case-topic>      urgent production fix; cut from main, PR to main, then merged back into development
```

Long-lived branches:
- `main` is production. It receives only release PRs from `development` and hotfix PRs.
- `development` is the integration branch. Every feature PR merges here, and development, integration, and test runs happen from it.
- There are no `release/*` branches. A release is the `development` → `main` PR, marked by tag `vX.Y.Z` on `main`.

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

Each feature is developed in its own Git worktree so `main`, `development`, and other work stay untouched.

- Location: sibling folder `../ClaudeCodeBhaskar-wt/<branch-name-with-slashes-as-dashes>`.
  Example: `D:\Projects\AI\Claude\project-mgmt-system\ClaudeCodeBhaskar-wt\feature-project-governance`.
- Create: `git fetch origin development`, then `git worktree add ../ClaudeCodeBhaskar-wt/<name> -b feature/<topic> origin/development`
- Work, test, and commit only inside the worktree.
- After the feature PR merges into `development`: `git worktree remove <path>`, then `git worktree prune`. The feature branch is kept (§7.1, §13).
- The primary checkout stays on `main` or `development` and is used only for reading, pulling, and reviewing.
- Never run feature work in the primary checkout.

---

## 9. PR Workflow

- Base branch: `development` for feature, fix, docs, and chore PRs. `main` only for the release PR (`development` → `main`, §13) and for hotfix PRs. Title: `<type>(<scope>): <summary>`. Release PR title: `release(<scope>): vX.Y.Z`.
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

- PRs are small enough to review in one sitting. Split large features into sequential PRs, each on its own branch cut from `development`. Release PRs list every merged PR with its requirement IDs in the body, and the test results from `development`.
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
- `main` is production and always deployable. Every merge into `main` passes build and tests.
- `development` is the integration branch. Every feature PR merges there after review. Development, integration, and test runs happen from it.
- The release owner is the project owner. The release owner approves release PRs, branch cleanup, and production tags.
- A release covers one module or release scope. Its features merge into `development` one PR at a time. When all are merged and validated, the release owner approves the `development` → `main` release PR (§6, step 7).
- Releases are tagged `vX.Y.Z` on `main` after the release PR merges. Release notes list every merged PR and the requirement IDs delivered.
- Feature branches are kept until the release is deployed and verified in production. The release owner approves their deletion (§6, step 8). This keeps the history needed for rollback and audit.
- Hotfixes: `hotfix/<topic>` from `main`, PR to `main`, tagged as a patch release, then merged back into `development` by PR so the fix is not lost.
- Before a release: all tests pass on `development`, the traceability matrix shows no unplanned gaps for the release scope, backup and restore have been verified (NFR-REL), and the release owner signs off in the release PR.
- Rollback: redeploy the previous tag, or revert the release merge on `main` with `git revert -m 1 <merge-sha>` in a new PR. Also use the migration down-script or restore point. Retained feature branches show what each change contained.
- DevOps: CI runs build, unit, and integration tests on every PR to `development` and `main`. CD to staging on every merge into `development`. Production deploys only from an approved tag on `main`. No CI configuration exists in this repository yet; adding it is a separate `chore(devops)` PR.

---

## 14. Working Agreements for Claude

- Read the BRD and this file before starting any task.
- Do not guess requirements. If the BRD is silent, ask or propose an amendment.
- Prefer small, verifiable steps. Report what was run and what the result was. If something could not be verified, say so.
- Do not create files outside the repository structure defined here without asking.
- Do not store secrets, connection strings with passwords, or patient data in the repo or in memory.
- Keep this file current. Changes to governance rules are made on a `feature/project-governance`-style branch with approval.
