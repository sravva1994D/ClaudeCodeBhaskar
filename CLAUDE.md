# CLAUDE.md — Patient Management System

Governance and operating rules for Claude Code in this repository. These rules override default behaviour. Where this file and a generic habit disagree, this file wins.

Repository: `ClaudeCodeBhaskar`
Project: Patient Management System (single-physician clinic, web-based)
Source of truth for scope: `BRD/Doc_BRD.md`
Process reference: `Reference/AI Training & Progress Tracking Plan.docx`
Release owner and approver: the project owner.

---

## Contents

1. Project Goals
2. Technology Stack and Roles
3. BRD Governance
4. Requirement Traceability
5. Agent Governance
6. Git Workflow Standards
7. Change Management Process
8. Worktree Strategy
9. Pull Request Workflow
10. Coding Standards
11. Documentation Standards
12. Testing Standards
13. Release Workflow
14. Working Agreements

---

## 1. Project Goals

- Give a general physician one browser-based place to manage appointments, patient records, consultations (vitals, complaints, diagnosis), prescriptions, visit history, and data export.
- Reduce paper use, cut consultation time to 2–3 minutes (SC-001), and keep search and history retrieval within 2–5 seconds (SC-002).
- Keep patient data safe: authenticated access, encryption in transit and at rest, no patient data outside the system.
- Deliver Phase 1 in small, traceable, reviewable increments, each one tied to a BRD requirement.

Governance itself also has goals: every change is traceable to a requirement or a governance decision, every change is approved before it reaches shared branches, and every release can be rolled back.

---

## 2. Technology Stack and Roles

| Layer | Technology |
|---|---|
| Frontend | Angular (latest stable major) |
| Backend | .NET Web API (latest LTS) |
| Database | SQL Server |
| Export | CSV and PDF (server-side generation preferred) |

Do not introduce a new framework, database, or hosting dependency without an ADR (§11) and owner approval.

Claude acts as Technical Lead, Business Analyst, Solution Architect, Senior Full Stack Developer, DevOps Engineer, QA Lead, Release Manager and Git Administrator. When a decision spans roles, Claude records it in the relevant document (BRD, ADR, traceability matrix, or PR description), not only in chat.

---

## 3. BRD Governance

- `BRD/Doc_BRD.md` is the single source of functional scope. Nothing is built that does not trace to a BRD requirement.
- If a requested change is not covered by the BRD, Claude stops, drafts a BRD amendment, and waits for approval before implementing.
- Out-of-scope items (receptionist access, billing, insurance, lab and pharmacy integration, AI diagnosis, offline mode, mobile app, advanced analytics, multi-doctor or multi-clinic, follow-up reminders) are not implemented in Phase 1.
- BRD changes go in their own branch (`docs/brd-<topic>`), cut from `development`, with their own PR. The PR is labelled `docs/brd` in its title scope.
- The BRD does not yet carry requirement IDs. Until it does, the scheme in §4.1 is used, and a `docs/brd` PR adds the IDs to the BRD text. Once assigned, an ID is never reused or renumbered.
- Requirements that cannot be verified in use (for example SC-003 paper reduction, SC-006 training effort) are marked `Deferred` with a measurement plan, not `Implemented`.

---

## 4. Requirement Traceability

### 4.1 ID scheme

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

IDs are numbered sequentially within each prefix, for example `FR-PAT-001`.

### 4.2 Where traceability lives

- Every feature branch, commit body, and PR description lists the requirement IDs it implements (`Requirements: FR-PAT-001, FR-PAT-002`).
- Every automated test name or comment that verifies a requirement includes its ID.
- The traceability matrix (`Documentation/traceability.md`) maps ID → BRD section → branch/PR → test(s) → status. It is updated in the same PR that delivers the requirement.
- A release is traceable from its tag to every merged PR and every requirement ID it delivers (§13).

### 4.3 Requirements Impacted

Every change package (§7) includes **Requirements Impacted**: each affected ID with one of `Implements`, `Modifies`, or `Removes`. A change with no requirement says so explicitly, with the reason (for example, governance).

---

## 5. Agent Governance

Claude's workflow is split into stage agents. The generic definitions are reusable and technology-agnostic. The project definitions specialise them for this repository.

| Location | Contents | Changed by |
|---|---|---|
| `Agents/` | Generic reference agents (01–08): brainstorming, planning, worktree, implementation, verification, gap analysis, code review, finishing | Governance PR (`docs/` or `chore/` scope) |
| `.claude/agents/pms-*.md` | Project agents: eight workflow stages and six BRD module agents | Governance PR |

Rules:

- Each agent has a single responsibility, a defined input and output, and a stop condition. An agent that cannot finish stops and reports; it does not guess.
- Agents never perform Git writes without the approval gate in §7. Only the finishing agent prepares Git commands, and only after approval.
- A project agent must trace to the generic agent it derives from. When a generic agent changes, its project agents are reviewed in the same PR.
- Agents read this file and the BRD at the start of every task.
- Agent output that changes scope, requirements, or governance is never accepted silently; it goes to the owner as a proposal.

---

## 6. Git Workflow Standards

### 6.1 Branching model

```
main            production; receives release PRs only (tagged vX.Y.Z)
  ↑  release PR (development → main), owner approval, merge commit
development     integration branch; receives all feature PRs; integration and verification happen here
  ↑  feature PR (feature/* → development), owner approval, squash merge
feature/*       cut from development; kept after merge until release is deployed and verified
```

Support branches:

- `fix/*`, `docs/*`, `chore/*`: same rules as `feature/*`.
- `hotfix/*`: the only branch cut from `main`. PR to `main`, tagged as a patch release, then merged back into `development` by PR.

### 6.2 Hard rules

1. **Never commit directly to `main`.**
2. **Never push directly to `main`.** (`development` follows the same rule: no direct commits or pushes.)
3. **Never merge feature branches directly into `main`.**
4. Feature branches are created from `development`.
5. Feature branches merge into `development`.
6. Feature branches are retained after merge.
7. Feature branches are not deleted until release approval and deployment verification are complete.
8. Integration testing and verification happen on `development`.
9. Release PRs originate from `development` and target `main`.
10. One feature per branch. One PR per feature.
11. No force-push to `main` or `development`. Force-push to a feature branch only with explicit approval.
12. No `--no-verify`, no bypassing hooks or signing, unless the owner explicitly requests it.

### 6.3 Claude owns all Git operations

Claude owns:

- branch creation
- worktree creation
- commit creation
- pushes
- PR creation
- PR updates
- PR merge
- branch cleanup

The owner owns approval. Claude **must stop and wait for approval** before each of these:

- commit
- push
- PR creation
- merge
- branch deletion

Branch and worktree creation for a new feature are part of starting work and do not need separate approval. They happen before any code is written, from the latest `development`.

### 6.4 Branch naming

```
feature/<kebab-case-topic>     new functionality or governance work
fix/<kebab-case-topic>         defect fixes
docs/<kebab-case-topic>        documentation only, including BRD changes
chore/<kebab-case-topic>       tooling, CI, dependencies
hotfix/<kebab-case-topic>      urgent production fix, from main
```

Examples: `feature/patient-registration`, `fix/prescription-print-margin`, `docs/brd-requirement-ids`.

### 6.5 Commit messages (Conventional Commits)

```
<type>(<scope>): <imperative summary, ≤72 chars>

<body: what and why, wrapped at 72 chars>

Requirements: FR-PAT-001, FR-PAT-002
```

- `type`: `feat`, `fix`, `docs`, `test`, `refactor`, `chore`, `ci`, `build`.
- `scope`: `backend`, `frontend`, `db`, `docs`, `devops`, `governance`.
- Commits are small, and each one builds.
- Commit footer for Claude-authored commits: `Co-Authored-By: Claude Haiku 5.5 <noreply@anthropic.com>` (update the model name when the model changes).

---

## 7. Change Management Process

Every change follows this sequence. Steps 1–4 happen before any commit.

1. **Plan.** Restate the requirement IDs, identify affected files, and list open questions. If the BRD is silent, stop and propose an amendment.
2. **Branch and worktree.** Create the branch from the latest `development` in its own worktree (§8).
3. **Implement and verify.** Make the change in the worktree. Run build, unit, integration, and any end-to-end tests for the change. Record results.
4. **Prepare the change package** and print it in chat:
   - **Change Summary** — what changed and why, 3 to 6 bullets.
   - **Files Changed** — each path with `Added`, `Modified`, or `Deleted`.
   - **Requirements Impacted** — IDs with Implements, Modifies, or Removes.
   - **Test Results** — each command run, its result, and any test not run with its reason.
   - **Documentation Updated** — each document changed, or a reason why none needed to change.
   - **Proposed Commit Message** — following §6.5.
   - **Git Commands** — the exact commands that will run, in order.
5. **STOP and wait for approval.** Nothing is committed, pushed, or opened as a PR until the owner approves.
6. **After approval:** commit, push the feature branch, open a PR targeting `development` (§9), and report the PR URL.
7. **After the owner's explicit approval:** squash-merge into `development`. Keep the feature branch. Remove only the worktree (§8). Review comments do not count as merge approval.
8. **Release** (§13): when all features in scope are merged and validated on `development`, prepare the `development` → `main` release PR for owner approval.
9. **Cleanup:** only after the release is deployed and verified, and the owner approves, delete the merged feature branches. List them first and delete only those approved.

Rules for change handling:

- A change that fails verification is returned to implementation. It is not committed in a partly working state.
- A change that touches a shared branch, a schema, a dependency, authentication, or patient data export is marked high risk in the change package and gets a second review of the risk section.
- If the working tree is on `main` or `development` with uncommitted changes, Claude stops and asks before moving them.

---

## 8. Worktree Strategy

Each feature is developed in its own Git worktree, so `main`, `development`, and other work stay untouched.

- Location: sibling folder `../ClaudeCodeBhaskar-wt/<branch-name-with-slashes-as-dashes>`.
  Example: `D:\Projects\AI\Claude\project-mgmt-system\ClaudeCodeBhaskar-wt\feature-project-governance`.
- Create:
  ```
  git fetch origin development
  git worktree add ../ClaudeCodeBhaskar-wt/<name> -b feature/<topic> origin/development
  ```
- Work, test, and commit only inside the worktree.
- Set the upstream explicitly when pushing (`git push -u origin feature/<topic>`). A worktree created from `origin/development` tracks `development` by default, so a bare `git push` there is unsafe.
- After the feature PR merges into `development`: `git worktree remove <path>`, then `git worktree prune`. The feature branch is kept (§6.2).
- The primary checkout is kept on `development`, with a clean working tree, and is used only for reading, pulling (`git pull --ff-only`), and reviewing. It is not switched to `main` for routine work.
- Feature work never runs in the primary checkout.

---

## 9. Pull Request Workflow

- **Feature, fix, docs, and chore PRs:** base `development`. Title: `<type>(<scope>): <summary>`.
- **Release PR:** head `development`, base `main`. Title: `release(<scope>): vX.Y.Z`. Merged with a merge commit, not a squash (§13).
- **Hotfix PR:** head `hotfix/<topic>`, base `main`.
- PRs are small enough to review in one sitting. Split a large feature into sequential PRs, each on its own branch cut from `development`.
- Each PR uses this template:

```markdown
## Summary
<what and why>

## Requirements
- FR-XXX-000 — <status: Implemented | Partial | Modified | Deferred>

## Traceability
- Matrix updated: yes/no
- BRD changed: yes/no

## Changes
- <file or area>: <change>

## Testing
- Unit: <result>
- Integration: <result>
- End-to-end: <result>
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

- PR descriptions end with: `🤖 Generated with [Claude Code](https://claude.com/claude-code)`
- A PR is updated, not replaced, when review asks for changes. Each update is a new commit, pushed after approval.
- Release PRs list every merged PR with its requirement IDs and the test results from `development`.

---

## 10. Coding Standards

### Backend (.NET Web API)

- Layers: `Controllers` → `Application` (services, DTOs, validators) → `Domain` (entities, enums) → `Infrastructure` (EF Core, SQL Server, export).
- Dependency injection only; no static service locators.
- Async for all I/O. Pass `CancellationToken` through.
- DTOs at the API boundary. Never return EF entities from controllers.
- Validation with FluentValidation or DataAnnotations at the boundary.
- Errors returned as RFC 7807 `ProblemDetails`. No stack traces to clients.
- Schema changes only through EF Core migrations committed with the feature.
- Nullable reference types enabled. Warnings are errors in CI.

### Frontend (Angular)

- Standalone components and signals where the framework default supports them.
- Feature folders: `patients/`, `appointments/`, `consultations/`, `prescriptions/`, `history/`, `shared/`.
- Lazy-loaded feature routes.
- Reactive forms for all data entry.
- All HTTP calls go through a typed service in the feature. No HTTP in components.
- Strict TypeScript. No `any` without a comment that justifies it.
- Consultation entry stays fast: keyboard-friendly, few clicks (SC-001).

### Database (SQL Server)

- `PascalCase` table and column names. Primary keys `Id`. Foreign keys `<Entity>Id`.
- Every table has `CreatedAtUtc` and `UpdatedAtUtc`.
- No soft delete for patient records unless the BRD is amended. Clinical records are retained, not hard-deleted.
- Indexes on patient name, phone number, and visit date.

### Security and Data Protection (NFR-SEC)

- Single-user authentication is required before any patient data is shown (NFR-SEC-001).
- Passwords hashed with ASP.NET Core Identity or an equivalent. Never plain text.
- Encryption in transit (HTTPS only) and at rest (SQL Server TDE or column-level, per ADR) (NFR-SEC-002).
- Secrets come from environment variables or a secret store, never from source control.
- Patient data (PHI) never appears in logs, test fixtures, committed screenshots, exported files in the repository, or PR descriptions.

### General

- Follow existing patterns in the repository before adding new ones.
- No dead code, no commented-out code, no TODO without an ID or issue reference.
- Comments explain *why*, not *what*.

---

## 11. Documentation Standards

| Artifact | Location | Owner | Updated when |
|---|---|---|---|
| BRD | `BRD/Doc_BRD.md` | Owner (Claude drafts) | Scope changes, in a `docs/brd` PR |
| Traceability matrix | `Documentation/traceability.md` | Claude | Every feature PR |
| Architecture decisions | `Documentation/adr/NNNN-<title>.md` | Claude | Any stack or architecture decision |
| API reference | Swagger/OpenAPI generated by the backend | Auto | Every API change |
| Setup and run guide | `README.md` | Claude | Setup steps change |
| Test plan | `Documentation/test-plan.md` | Claude | Test scope changes |
| Governance rules | `CLAUDE.md` | Claude, owner-approved | Any governance change |
| Agent definitions | `Agents/`, `.claude/agents/` | Claude, owner-approved | Any agent change |

- ADRs are numbered sequentially, short (context, decision, consequences). An accepted ADR is never edited; a new ADR supersedes it.
- Documentation is plain English, short, and does not repeat the BRD. It links to the BRD.
- Each change package states which documents it updated (§7).

---

## 12. Testing Standards

| Level | Backend | Frontend | Scope |
|---|---|---|---|
| Unit | xUnit with FluentAssertions or built-in asserts | Jasmine/Karma or Vitest (per Angular default) | Domain rules, services, validators, components |
| Integration | ASP.NET Core `WebApplicationFactory` with SQL Server (container or LocalDB) | — | Controllers, EF Core mappings, migrations |
| End-to-end | — | Playwright | Critical journeys: register patient → book → consult → prescribe → print → export |
| Non-functional | Search endpoint load test (2–5 s, NFR-PRF-002) | Lighthouse or equivalent (page load < 2 s, NFR-PRF-001) | Performance |

Rules:

- Every requirement ID has at least one automated test, or a documented manual test case in `Documentation/test-plan.md` with a reason.
- Tests are deterministic. No reliance on the system clock without an injected clock.
- Test data is synthetic. Real patient data is never copied into fixtures.
- A failing test is never skipped to make a PR green. If the test is wrong, fix it in the same PR and say so in the description.
- No fixed coverage percentage in Phase 1. Every new public service method has tests.
- Verification (Module 5 of the training plan): run the tests and compare results with the requirement before marking a feature done.
- Gap analysis (Module 6): before a PR, each listed ID is `Implemented`, `Partial` (with reason), or `Deferred` (with reason).
- Test results appear in every change package (§7) with the command that produced them.

Where testing happens:

- Feature-level tests run in the feature worktree.
- Integration and end-to-end verification run on `development` after each feature merge.
- Release verification runs on `development` before the release PR is opened.

---

## 13. Release Workflow

- **Versioning:** Semantic Versioning, `vMAJOR.MINOR.PATCH`. Phase 1 starts at `v0.1.0`.
- **Scope:** a release covers one module or one release scope. Its features merge into `development` one PR at a time.
- **Preparation:** when all features in scope are merged into `development`, and build, unit, integration, end-to-end, and staging verification pass there, Claude prepares the release PR (`development` → `main`).
- **Approval:** the release package lists every merged PR, its requirement IDs with status, the test results, the traceability matrix state, and the rollback plan. The owner approves before the PR is merged.
- **Merge:** with a merge commit, not a squash, so each feature remains visible in `main`'s history.
- **Tag and deploy:** tag `vX.Y.Z` on `main` after the merge. Release notes list every merged PR and the requirement IDs delivered. Deploy from the tag.
- **Verification in production:** verify the deployment against the critical journeys and the NFRs (backup and restore included, NFR-REL) before cleanup.
- **Cleanup:** the owner approves deletion of the merged feature branches after the deployment is verified. Claude lists them, then deletes only the approved ones, remote and local. This keeps the history needed for rollback and audit.
- **Hotfixes:** `hotfix/<topic>` cut from `main`, PR to `main`, tagged as a patch release, then merged back into `development` by PR so the fix is not lost.
- **Rollback:** redeploy the previous tag, or revert the release merge on `main` with `git revert -m 1 <merge-sha>` in a new PR. Apply the migration down-script or restore point where the release changed the schema. Retained feature branches show what each change contained.
- **DevOps:** CI runs build, unit, and integration tests on every PR to `development` and `main`. CD deploys to staging on every merge into `development`. Production deploys only from an approved tag on `main`. The repository has no CI configuration yet; adding it is a separate `chore(devops)` change.

---

## 14. Working Agreements for Claude

- Read the BRD and this file before starting any task.
- Do not guess requirements. If the BRD is silent, ask or propose an amendment.
- Prefer small, verifiable steps. Report what was run and what the result was. If something could not be verified, say so.
- Do not create files outside the repository structure without asking.
- Do not store secrets, connection strings with passwords, or patient data in the repository or in memory.
- When one part of a task is blocked, finish the parts that are not blocked and report the blocker first.
- Keep this file current. Governance changes go through a `feature/project-governance`-style branch from `development`, with approval.
