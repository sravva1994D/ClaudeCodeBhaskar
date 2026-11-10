# 03 — Commit Standards

**Applies to:** every commit on `development` (the only working branch) and every commit on `main` made by a release merge
**Basis:** Conventional Commits, with requirement traceability (CLAUDE.md §5.2)
**Status:** Proposed for review

---

## 1. Message Format

```
<type>(<scope>): <imperative summary, 72 characters or fewer>

<body: what changed and why, wrapped at 72 characters>

Requirements: FR-PAT-001, FR-PAT-002
Decisions: PDR-08
Co-Authored-By: Claude Haiku 5.5 <noreply@anthropic.com>
```

| Part | Rule |
|---|---|
| Summary | Imperative mood ("add", not "added"). No full stop. 72 characters or fewer |
| Body | Optional for trivial changes. Required for behaviour changes. Explains why |
| `Requirements:` | Required for any change that implements, modifies or removes a requirement. Use IDs from the catalogue |
| `Decisions:` | Required when the change follows a planning decision (PDR-n or DEC-n) |
| `Co-Authored-By:` | Required on commits made by Claude. Update the model name when the model changes |

## 2. Types

| Type | Use for |
|---|---|
| `feat` | New behaviour that implements a requirement |
| `fix` | Correction of a defect |
| `test` | Tests only |
| `refactor` | Restructure with no behaviour change |
| `docs` | Documentation only |
| `chore` | Tooling, dependencies, configuration |
| `build` | Build system, project files, migrations tooling |
| `ci` | Pipeline configuration (only when a pipeline exists) |

## 3. Scopes

| Scope | Covers |
|---|---|
| `web` | `PMS.Web`: controllers, views, ViewModels, static files |
| `business` | `PMS.Business`: services, DTOs, validators, mappers |
| `data` | `PMS.Data`: DbContext, entities, repositories, migrations |
| `tests` | Test projects |
| `docs` | Files under `BRD/` and `Documentation/` |
| `governance` | `CLAUDE.md` and this folder's standards |

## 4. Examples

Implementation:

```
feat(business): reject overlapping appointments

Appointments are 30-minute slots. A new booking that takes a slot already
held by a non-archived appointment is rejected with a Result error. Cancelled
and No-show appointments release their slot (PDR-07).

Requirements: FR-APT-001
Decisions: PDR-06, PDR-07
Co-Authored-By: Claude Haiku 5.5 <noreply@anthropic.com>
```

Documentation:

```
docs(docs): record appointment lifecycle and approve MoSCoW priorities

Adds the Scheduled → Checked-In → In Consultation → Completed lifecycle.
No reversal. Completed requires In Consultation (PDR-08, PDR-27). Marks
A-006 approved by the Project Owner.

Requirements: FR-APT-003
Decisions: PDR-08, PDR-27, A-006
Co-Authored-By: Claude Haiku 5.5 <noreply@anthropic.com>
```

## 5. Approval Gate

No commit is made until the user approves. The approval package (CLAUDE.md §5.1) contains:

1. Change summary
2. Files changed (Added, Modified, Deleted)
3. Requirements impacted (Implements, Modifies, Removes)
4. Proposed commit message
5. Exact Git commands

## 6. Content Rules

Never commit:

- Passwords, connection strings with credentials, API keys, certificates, `.pfx` files.
- Patient data of any kind. Test data is synthetic.
- Build output and local IDE state: `bin/`, `obj/`, `.vs/`, `*.user`, `*.suo`, `TestResults/`.
- Local user secrets. Use the .NET user-secrets store on the developer machine.
- Large binaries without approval. `Reference/*.docx` is tracked only if decision D-4 says so.

## 7. Commit Size and Build Rules

- One logical change per commit. Split a feature into several commits, each one step of the plan.
- Every implementation commit builds and its tests pass. No commit is left broken for a later commit to fix.
- Schema changes are a migration in the same commit as the entity change that needs it.
- Reformatting is its own commit (`chore`), never mixed with behaviour changes.
- Do not amend or rewrite a commit that has been pushed. Add a new commit instead.

## 8. Hooks and Signing

- Do not use `--no-verify` or skip hooks unless the user explicitly asks.
- Commit signing is not required in Phase 1. If the repository later requires it, add a decision.

## 9. Checks Before Committing

| Check | Command (reference) | Required for |
|---|---|---|
| Working tree shows only intended files | `git status` | All commits |
| Diff reviewed | `git diff --staged` | All commits |
| Build passes | `dotnet build` | Implementation commits |
| Tests pass | `dotnet test` | Implementation commits |
| No secrets or patient data in the diff | Manual review | All commits |
| Traceability matrices updated | Manual review of `12-` and `18-` | Commits that change requirement status |

Commands are shown for reference. Claude runs them only after the approval gate.
