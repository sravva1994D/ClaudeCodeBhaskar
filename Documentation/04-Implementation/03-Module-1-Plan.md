# 03 — Module 1 Implementation Plan (Foundation and Authentication)

**Project:** Patient Management System, Phase 1 (localhost learning project)
**Covers:** Implementation item 1 (Authentication & Authorization) and the Foundation shared components (Module 12)
**Milestone:** M1 (roadmap `Documentation/02-Planning/15-Development-Roadmap.md`)
**Status:** Plan for review, 2026-11-10. No code written yet. Implementation starts after approval.
**Source rules:** BRD (amended 2026-11-10, DEC-017 to DEC-022); CLAUDE.md §3, §5.4, §8, §9, §10

---

## 1. Scope

| In scope | Requirement IDs | Work packages |
|---|---|---|
| Users table and migration 1 (`AddUsers`) | NFR-SEC-001 | WP-2.2 (partial) |
| Repository for the single account | NFR-SEC-001 | WP-2.3 |
| Password policy (8 to 16 characters; upper, lower, digit, symbol) | NFR-SEC-001 (PDR-02) | WP-2.3 |
| Sign-in and forgot-password services | NFR-SEC-001 | WP-2.3 |
| Cookie authentication, 24-hour sliding timeout, global authorization | NFR-SEC-001 | WP-2.3 |
| Login and forgot-password pages (SCR-01, SCR-02), logout | NFR-SEC-001 | WP-2.3 |
| Console seed command (DEC-018) | NFR-SEC-001, CON-002 | WP-2.4 |
| Layout shell with top bar and logout (no search yet) | NFR-USB-001, NFR-CMP-001 | WP-2.5 |
| Shared Result and validator base (already exists; extend only if needed) | FR-VIT-001, FR-PAT-001 (base only) | WP-2.6 |
| Test harness for web and data tests | All (infrastructure) | WP-2.7 |

Out of scope for this module: patients, appointments, consultations, search results, CSV or PDF, and any BRD feature beyond login.

## 2. Requirement mapping (Module 1 login part)

| Requirement | Acceptance criteria (`10-Acceptance-Criteria.md`) | Test IDs (`14-Testing-Strategy.md` §4) |
|---|---|---|
| NFR-SEC-001 | AC-1 no access without login; AC-2 one account; AC-3 password policy; AC-4 24 h sliding timeout; AC-5 forgot-password message | TC-NFR-SEC-001-1 (protected URL redirects to login), TC-NFR-SEC-001-2 (one account only; wrong password gives generic error), TC-NFR-SEC-001-3 (seed command stores no password) |
| NFR-SEC-002 (hashing part) | AC-3 passwords hashed, never plain text | Hash check: stored value is not the password; checked in the data integration test |
| NFR-USB-001, NFR-CMP-001 (shell only) | Qualitative; layout renders in Chrome and Edge | Manual check at M1 exit; full checks in M8 |

Every test that verifies a requirement carries its ID in the name (CLAUDE §4.2, §10).

## 3. Design points applied

| Point | Design | Source |
|---|---|---|
| Password policy | 8 to 16 characters; at least one upper case, lower case, digit and symbol. Checked in the Business layer, returns errors (AD-11) | PDR-02; `13-` §3.7 |
| Hashing | ASP.NET Core `PasswordHasher` (hasher only, custom Users table) | PDR-04; `12-` §2 |
| Sign-in | Same message for unknown username and wrong password | `12-` §3.1 |
| Forgot password | Email, new password, confirm. On match: password updated and user signed in. No match: "If the email matches an account, the password has been reset." No email sent | PDR-03; DEC-009; `12-` §3.2 |
| Sign-in lookup | By username only. No fallback to a single account (DEC-018) | `11-` §2.1 |
| Cookie | Name `PatientManagement.Auth`; 24 h; sliding; HttpOnly; SameSite Lax; login path `/Account/Login`. Secure flag only when HTTPS is used (localhost is HTTP) | `12-` §4; PDR-20; PDR-21 |
| Global authorization | Fallback policy requires a signed-in user. `[AllowAnonymous]` only on login and forgot password | `09-` §3 |
| Anti-forgery | `[ValidateAntiForgeryToken]` on every POST | CLAUDE §9 |
| Seed command | Console mode (`-- seed-user`) runs before the web host starts, prompts for username, email and password (masked, typed twice), creates the account if none exists, then exits. Stores only the hash. Never runs at normal startup | DEC-018; `12-` §8 |
| Logging | No password, hash or email written to logs | CLAUDE §8; Developer Guide §7 |

## 4. Work steps (each one builds, passes tests and is committed on `development`)

Each step: build with 0 warnings, run `dotnet test`, commit with the format in CLAUDE §5.2, push to `origin/development`, and list the commit in chat.

| Step | Work | Files (expected) | Tests added |
|---|---|---|---|
| 1 | Users entity, configuration, `DbSet<User>`, migration `AddUsers`. Check the generated SQL with `dotnet ef migrations script`. Verifies the `dotnet-ef` 10.0.14 tooling (not yet verified on this machine) | `Data/Entities/User.cs`, `Data/Configurations/`, `Data/Context/`, `Data/Migrations/` | Integration: unique Username; not-null columns (against the test database) |
| 2 | `IUserRepository` in `Data/Abstractions`; implementation in `Data/Repositories`. Methods: GetByUsername, GetByEmail, Update. No GetSingleUser | `Data/Abstractions/IUserRepository.cs`, `Data/Repositories/UserRepository.cs` | Integration: lookup by username and by email |
| 3 | Password policy validator in Business, returning errors | `Business/Validators/PasswordPolicyValidator.cs` | Unit: each rule on its own; accepts a valid 8-character and a valid 16-character password; rejects 7 and 17 characters |
| 4 | `IAuthService` / `AuthService`: SignInAsync, ResetPasswordAsync, CreateInitialAccountAsync (for the seed command). Uses `PasswordHasher` and `IUserRepository`. Returns Result | `Business/Interfaces/IAuthService.cs`, `Business/Services/AuthService.cs`, `Business/Dtos/` | Unit (NSubstitute): wrong password and unknown user return the same error; reset with no email match returns the generic message; reset with match signs in; initial account refused when one exists |
| 5 | Cookie authentication and global authorization in `Program.cs`; registration in `AddBusinessLayer` where applicable | `Web/Program.cs` | Integration: unauthenticated GET of Home redirects to `/Account/Login?ReturnUrl=…` (TC-NFR-SEC-001-1); cookie options read back as 24 h, sliding, HttpOnly, SameSite Lax |
| 6 | `AccountController` (Login GET/POST, Logout POST, ForgotPassword GET/POST), ViewModels, views for SCR-01 and SCR-02 | `Web/Controllers/AccountController.cs`, `Web/ViewModels/Account/`, `Web/Views/Account/` | Integration: POST without token is rejected; wrong password shows the generic error (TC-NFR-SEC-001-2); forgot password with no match shows the PDR-03 message |
| 7 | Layout shell: top bar with the app name and a Sign out button (POST). Quick search box is added in item 2 with a placeholder | `Web/Views/Shared/_Layout.cshtml` | Integration: home page shows signed-in state after login |
| 8 | Seed console command: `Web/Program.cs` argument check and `Web/Setup/SeedUserRunner.cs`. Masked password input; no echo; no default values in code | `Web/Setup/SeedUserRunner.cs`, `Web/Program.cs` | Unit: runner calls CreateInitialAccountAsync with the typed values; the password is not logged. TC-NFR-SEC-001-3 (no password in repo or output) by a repository search at M1 exit |
| 9 | Documentation: Setup Guide §5A and Developer Guide updated for the command. Implementation review package for M1 | `Documentation/04-Implementation/` | — |

Steps 1 to 4 are data and business logic and can be built and tested without the web project. Steps 5 to 7 are web. Step 8 depends on steps 1 to 4.

## 5. Items that need your confirmation before step 4

These are design additions. Each traces to a decision. Confirm or change before implementation.

| # | Addition | Why |
|---|---|---|
| A-1 | `IAuthService.CreateInitialAccountAsync` (not in `10-Service-Design` §2.1) | The seed command needs a service method so the rule (only when no account exists) stays in the Business layer (DEC-018) |
| A-2 | Seed command entry as `dotnet run … -- seed-user` (argument, not a separate project) | Keeps one project; no new solution project (CLAUDE §6 lists three projects) |
| A-3 | Username is typed at the prompt, not fixed in code. PDR-05 records the username as `bmunukuri`, so this is what you will type | No value is hard-coded (your B1 decision) |
| A-4 | Login does not offer "remember me" or lockout | Not in the BRD (`12-` §6) |

## 6. Verification at M1 exit (gate from `15-` §2)

| Check | How | Pass rule |
|---|---|---|
| Build | `dotnet build PatientManagement.sln` | 0 warnings, 0 errors |
| Tests | `dotnet test PatientManagement.sln` | All pass, including new tests in §2 |
| Migration | `dotnet ef database update` on the local database, then migration script check | Users table created; script reviewed |
| Login | Run the app (`http`, port 5092), seed the account, sign in | Signed-in home page; wrong password gives generic error |
| Protected pages | Open Home while signed out | Redirects to login |
| Timeout | Config and cookie options test (integration). A real 24 h wait is not practical | Options read back as 24 h sliding |
| Secrets | Search the repository for the password and hash values used in testing | None found |
| Browser | Chrome and Edge, latest version; record the version | Login and layout work (Safari not required, DEC-019) |

Results are recorded in the M1 review package with the exact output, as CLAUDE §10 requires.

## 7. Risks for this module

| Risk | Effect | Mitigation |
|---|---|---|
| `dotnet-ef` 10.0.14 not yet verified on this machine | Migration step delayed | Verify at step 1 with a read-only script check first |
| Interactive prompt in a non-interactive shell | Seed command cannot run in some tools | Documented as an interactive command; not used in tests |
| Cookie `SecurePolicy` left on by mistake | Login fails on localhost over HTTP | Setting follows the scheme (`12-` §4) and is covered by the test in step 5 |
| Password shown in console history | Credential leak on the machine | Masked input; nothing echoed; no argument carries the password |

## 8. Completion criteria for Module 1

- All steps in §4 are committed on `development`, each building and passing tests.
- TC-NFR-SEC-001-1, -2 and -3 pass. Hash check passes.
- Verification in §6 recorded with exact results.
- No BRD feature beyond login is built.
- Items A-1 to A-4 are confirmed or changed by the Project Owner.
