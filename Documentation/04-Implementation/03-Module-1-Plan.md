# 03 — Module 1 Implementation Plan (Foundation and Authentication)

**Project:** Patient Management System, Phase 1 (localhost learning project)
**Covers:** Implementation item 1 (Authentication & Authorization) and the Foundation shared components (Module 12)
**Milestone:** M1 (roadmap `Documentation/02-Planning/15-Development-Roadmap.md`)
**Status:** Plan approved in principle 2026-11-10 with two changes (test database; password hasher interface). Implementation starts on the Project Owner's go-ahead.
**Source rules:** BRD (amended 2026-11-10, DEC-017 to DEC-022); CLAUDE.md §3, §5.4, §8, §9, §10

---

## 1. Scope

| In scope | Requirement IDs | Work packages |
|---|---|---|
| Test database and its setup script | All data tests (infrastructure) | WP-2.7 |
| Users table and migration 1 (`AddUsers`) | NFR-SEC-001 | WP-2.2 (partial) |
| Repository for the single account | NFR-SEC-001 | WP-2.3 |
| Password hasher interface (Business) and adapter (Web) | NFR-SEC-002 (hashing) | WP-2.3 |
| Password policy (8 to 16 characters; upper, lower, digit, symbol) | NFR-SEC-001 (PDR-02) | WP-2.3 |
| Sign-in and forgot-password services | NFR-SEC-001 | WP-2.3 |
| Cookie authentication, 24-hour sliding timeout, global authorization | NFR-SEC-001 | WP-2.3 |
| Login and forgot-password pages (SCR-01, SCR-02), logout | NFR-SEC-001 | WP-2.3 |
| Console seed command (DEC-018) | NFR-SEC-001, CON-002 | WP-2.4 |
| Layout shell with top bar and logout (no search yet) | NFR-USB-001, NFR-CMP-001 | WP-2.5 |
| Shared Result and validator base (already exists; extend only if needed) | FR-VIT-001, FR-PAT-001 (base only) | WP-2.6 |

Out of scope for this module: patients, appointments, consultations, search results, CSV or PDF, and any BRD feature beyond login.

## 2. Requirement mapping (Module 1 login part)

| Requirement | Acceptance criteria (`10-Acceptance-Criteria.md`) | Test IDs (`14-Testing-Strategy.md`) |
|---|---|---|
| NFR-SEC-001 | AC-1 no access without login; AC-2 one account; AC-3 password policy; AC-4 24 h sliding timeout; AC-5 forgot-password message | TC-NFR-SEC-001-1 (protected URL redirects to login); TC-NFR-SEC-001-2 (one account only; wrong password gives generic error); TC-NFR-SEC-001-3 (automated scan: the seed command's output, logs and repository contain no password) |
| NFR-SEC-002 (hashing) | AC-3 passwords hashed, never plain text | TC-NFR-SEC-002-1 (stored value is a hash and verifies with the original password; the plain value is not stored) |
| NFR-USB-001, NFR-CMP-001 (shell only) | Qualitative; layout renders in Chrome and Edge | Manual check at M1 exit; full checks in M8 |

Every test that verifies a requirement carries its ID in the name (CLAUDE §4.2, §10).

## 3. Design points applied

| Point | Design | Source |
|---|---|---|
| Password policy | 8 to 16 characters; at least one upper case, lower case, digit and symbol. Checked in the Business layer, returns errors (AD-11) | PDR-02; `13-` §3.7 |
| Hashing interface | `IPasswordHasher` in Business: `Hash(password)` and `Verify(hash, password)`. Business holds no ASP.NET types (Developer Guide §2) | Approved change (hasher interface) |
| Hashing implementation | Adapter in the Web project over the ASP.NET Core `PasswordHasher` (salted PBKDF2). Web may reference ASP.NET Core. Registered in `Program.cs` | PDR-04 (hasher only); Developer Guide §2 |
| Test database | Separate database `PatientManagement_Test`. See §3A | Approved change (test database) |
| Sign-in | Same message for unknown username and wrong password | `12-` §3.1 |
| Forgot password | Email, new password, confirm. On match: password updated and user signed in. No match: "If the email matches an account, the password has been reset." No email sent | PDR-03; DEC-009; `12-` §3.2 |
| Sign-in lookup | By username only. No fallback to a single account (DEC-018) | `11-` §2.1 |
| Cookie | Name `PatientManagement.Auth`; 24 h; sliding; HttpOnly; SameSite Lax; login path `/Account/Login`. Secure flag only when HTTPS is used (localhost is HTTP) | `12-` §4; PDR-20; PDR-21 |
| Global authorization | Fallback policy requires a signed-in user. `[AllowAnonymous]` only on login and forgot password | `09-` §3 |
| Anti-forgery | `[ValidateAntiForgeryToken]` on every POST | CLAUDE §9 |
| Seed command | Console mode (`-- seed-user`) runs before the web host starts, prompts for username, email and password (masked, typed twice), creates the account if none exists, then exits. Stores only the hash. Never runs at normal startup | DEC-018; `12-` §8 |
| Logging | No password, hash or email written to logs | CLAUDE §8; Developer Guide §7 |

## 3A. Test database (approved)

| Item | Decision |
|---|---|
| Name | `PatientManagement_Test` |
| Creation | `database/Scripts/02-create-test-database.sql`. Idempotent, same pattern as `01-create-database.sql`. Run once per machine |
| Schema | EF Core migrations, applied by the test fixture at the start of a test run. No hand-written schema |
| Connection | Test-only setting `ConnectionStrings:PatientManagementTest` in the test project's configuration. Neutral default `Server=localhost`; the local instance goes in user secrets. Tests never read the development connection string |
| Safety guard | The fixture refuses to run unless the database name ends in `_Test`. It stops before touching any other database |
| Isolation, data tests | Each test runs in a transaction that is rolled back at the end |
| Isolation, web tests | The app commits its own work, so the fixture clears the test tables before each web test, in foreign-key order |
| Provider | SQL Server only. No in-memory provider (`14-` §1) |
| Data | Synthetic only (CLAUDE §10) |

## 4. Work steps (each one builds, passes tests and is committed on `development`)

Each step: build with 0 warnings, run `dotnet test`, commit with the format in CLAUDE §5.2, push to `origin/development`, and list the commit in chat.

| Step | Work | Files (expected) | Tests added (project) |
|---|---|---|---|
| 0 | Test database script and test fixture: creates the schema through migrations, the `_Test` guard, the per-test rollback and table reset helpers | `database/Scripts/02-create-test-database.sql`; `tests/PatientManagement.IntegrationTests/Fixtures/` | Integration: the guard refuses a non-`_Test` name (unit-level check of the guard) |
| 1 | Users entity, configuration, `DbSet<User>`, migration `AddUsers`. Check the generated SQL with `dotnet ef migrations script`. Verifies the `dotnet-ef` 10.0.14 tooling (not yet verified on this machine) | `Data/Entities/User.cs`, `Data/Configurations/`, `Data/Context/`, `Data/Migrations/` | Integration (data): unique Username; not-null columns |
| 2 | `IUserRepository` in `Data/Abstractions`; implementation in `Data/Repositories`. Methods: GetByUsername, GetByEmail, Update. No GetSingleUser | `Data/Abstractions/IUserRepository.cs`, `Data/Repositories/UserRepository.cs` | Integration (data): lookup by username and by email; update saves the new hash |
| 3 | Password policy validator in Business, returning errors | `Business/Validators/PasswordPolicyValidator.cs` | Unit: each rule on its own; accepts a valid 8-character and a valid 16-character password; rejects 7 and 17 characters |
| 4 | `IPasswordHasher` in Business (interface only). `IAuthService` / `AuthService`: SignInAsync, ResetPasswordAsync, CreateInitialAccountAsync. Uses `IPasswordHasher` and `IUserRepository`. Returns Result | `Business/Interfaces/IPasswordHasher.cs`, `Business/Interfaces/IAuthService.cs`, `Business/Services/AuthService.cs`, `Business/DTOs/` | Unit (NSubstitute on `IUserRepository` and `IPasswordHasher`): wrong password and unknown user return the same error; reset with no email match returns the generic message; reset with match signs in; initial account refused when one exists |
| 5 | Web adapter `AspNetPasswordHasher` implements `IPasswordHasher`; registered in `Program.cs`. Cookie authentication and global authorization in `Program.cs`. `AddBusinessLayer` registers `IAuthService`; `AddDataLayer` registers `IUserRepository` | `Web/Security/AspNetPasswordHasher.cs`, `Web/Program.cs`, `Business/BusinessServiceCollectionExtensions.cs`, `Data/Context/DataServiceCollectionExtensions.cs` | Integration (web): hasher round trip (hash is not the password; verify succeeds for the right password and fails for a wrong one) (TC-NFR-SEC-002-1). Unauthenticated GET of Home redirects to `/Account/Login?ReturnUrl=…` (TC-NFR-SEC-001-1). Cookie options read back as 24 h, sliding, HttpOnly, SameSite Lax |
| 6 | `AccountController` (Login GET/POST, Logout POST, ForgotPassword GET/POST), ViewModels, views for SCR-01 and SCR-02 | `Web/Controllers/AccountController.cs`, `Web/ViewModels/Account/`, `Web/Views/Account/` | Integration (web): POST without token is rejected; wrong password shows the generic error (TC-NFR-SEC-001-2); forgot password with no match shows the PDR-03 message |
| 7 | Layout shell: top bar with the app name and a Sign out button (POST). Quick search box is added in item 2 with a placeholder | `Web/Views/Shared/_Layout.cshtml` | Integration (web): home page shows signed-in state after login |
| 8 | Seed console command: `Web/Program.cs` argument check and `Web/Setup/SeedUserRunner.cs`. Masked password input; no echo; no default values in code | `Web/Setup/SeedUserRunner.cs`, `Web/Program.cs` | Unit: runner calls CreateInitialAccountAsync with the typed values. Automated scan for TC-NFR-SEC-001-3: the seed command's output and test logs contain no password, and the repository contains none |
| 9 | Documentation: Setup Guide §5A and Developer Guide updated for the command. Implementation review package for M1 | `Documentation/04-Implementation/` | — |

Steps 0 to 4 are data and business logic. Steps 5 to 7 are web. Step 8 depends on steps 1 to 5.

## 5. Design additions

| # | Addition | Status |
|---|---|---|
| Approved | Test database `PatientManagement_Test` (§3A) | Approved 2026-11-10 |
| Approved | `IPasswordHasher` interface in Business; adapter in Web (§3) | Approved 2026-11-10. Adapter location chosen in Web so that Business and Data keep their rules |
| A-1 | `IAuthService.CreateInitialAccountAsync` (not in `10-Service-Design` §2.1) | Needs confirmation. The seed command needs a service method so the rule (only when no account exists) stays in the Business layer (DEC-018) |
| A-2 | Seed command entry as `dotnet run … -- seed-user` (argument, not a separate project) | Needs confirmation. Keeps one project; CLAUDE §6 lists three projects |
| A-3 | Username is typed at the prompt, not fixed in code. PDR-05 records the username as `bmunukuri` | Needs confirmation. No value is hard-coded (B1) |
| A-4 | Login does not offer "remember me" or lockout | Needs confirmation. Not in the BRD (`12-` §6) |

## 6. Verification at M1 exit (gate from `15-` §2)

| Check | How | Pass rule |
|---|---|---|
| Build | `dotnet build PatientManagement.sln` | 0 warnings, 0 errors |
| Tests | `dotnet test PatientManagement.sln` | All pass, including the tests in §2 |
| Migration | `dotnet ef database update` on the local database, then migration script check | Users table created; script reviewed |
| Login | Run the app (`http`, port 5092), seed the account, sign in | Signed-in home page; wrong password gives generic error |
| Protected pages | Open Home while signed out | Redirects to login |
| Timeout | Config and cookie options test (integration). A real 24 h wait is not practical | Options read back as 24 h sliding |
| Secrets | Automated scan (TC-NFR-SEC-001-3) runs in the test suite | No password found |
| Browser | Chrome and Edge, latest version; record the version | Login and layout work (Safari not required, DEC-019) |

Results are recorded in the M1 review package with the exact output, as CLAUDE §10 requires.

## 7. Risks for this module

| Risk | Effect | Mitigation |
|---|---|---|
| `dotnet-ef` 10.0.14 not yet verified on this machine | Migration step delayed | Verify at step 1 with a read-only script check first |
| Test database accidentally points at the development database | Test runs change development data | Name guard in the fixture (§3A); tests never read the development connection string |
| Microsoft.Extensions.Identity.Core version for the hasher | Restore or security warnings | Pin an explicit version in the Web project; check for advisories as with Pkcs (`Directory.Build.props`) |
| Interactive prompt in a non-interactive shell | Seed command cannot run in some tools | Documented as an interactive command; not used in tests |
| Cookie `SecurePolicy` left on by mistake | Login fails on localhost over HTTP | Setting follows the scheme (`12-` §4); covered by the test in step 5 |
| Password shown in console history | Credential leak on the machine | Masked input; nothing echoed; no argument carries the password |

## 8. Completion criteria for Module 1

- All steps in §4 are committed on `development`, each building and passing tests.
- TC-NFR-SEC-001-1, -2 and -3 pass. TC-NFR-SEC-002-1 passes.
- Verification in §6 recorded with exact results.
- No BRD feature beyond login is built.
- Items A-1 to A-4 are confirmed or changed by the Project Owner.
