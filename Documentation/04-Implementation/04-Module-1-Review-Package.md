# 04 — Module 1 Review Package (Foundation and Authentication)

**Project:** Patient Management System, Phase 1 (localhost learning project)
**Milestone:** M1 (roadmap `Documentation/02-Planning/15-Development-Roadmap.md`)
**Plan:** `Documentation/04-Implementation/03-Module-1-Plan.md`
**Status:** Built and verified, 2026-11-10. Awaiting the Project Owner's review.

---

## 1. Scope Delivered

| Plan step | Work | Status |
|---|---|---|
| 0 | Test database script, fixture and name guard | Done (`4921d34`) |
| 1 | `User` entity, configuration and `AddUsers` migration | Done (`2719608`) |
| 2 | User repository (lookups by username and email, update) | Done (`f006464`) |
| 3 | Password policy validator (8 to 16 characters; upper, lower, digit, symbol) | Done (`022f74a`) |
| 4 | Auth service (sign-in, forgot-password reset, seed account); hasher interface | Done (`6955540`) |
| 5 | Hasher adapter, sign-in cookie, global authorization | Done (`7400695`) |
| 6 | Sign-in, forgot-password and sign-out pages (SCR-01, SCR-02) | Done (`59ef706`) |
| 7 | Signed-in top bar with sign out; skeleton banner after sign-in | Done (`5021a37`) |
| 8 | Console seed command (`seed-user`) | Done (`8d1f2d1`) |
| 9 | Documentation and this review package | Done (this commit) |

Out of scope for Module 1, by design: patients, appointments, consultations, search, export, and any BRD feature beyond login.

## 2. Requirements Covered

| Requirement | Acceptance criteria (`10-Acceptance-Criteria.md`) | Tests |
|---|---|---|
| NFR-SEC-001 | AC-1 no access without login; AC-2 one account; AC-3 password policy; AC-4 24-hour sliding timeout; AC-5 forgot-password message | TC-NFR-SEC-001-1, -2, -3 (see §3) |
| NFR-SEC-002 (hashing and transport only) | AC-3 passwords hashed; AC-1 TLS beyond localhost (configuration, not run on localhost). AC-2 at-rest encryption is out of scope (DEC-017) | TC-NFR-SEC-002-1 |
| NFR-USB-001, NFR-CMP-001 (shell only) | Qualitative; full checks in M8 | Layout tests; browser check (§4) |

No feature requirement (FR-) is implemented in this module.

## 3. Test Results

Final run, 2026-11-10, on `development` at `8d1f2d1` plus this commit:

| Check | Command | Result |
|---|---|---|
| Build (clean) | `dotnet build PatientManagement.sln --no-incremental` | 0 errors, 0 warnings |
| Unit tests | `dotnet test PatientManagement.sln` | 28 passed, 0 failed |
| Integration tests | same | 47 passed, 0 failed |

Test IDs in this module:

| Test ID | What it checks | Where |
|---|---|---|
| TC-NFR-SEC-001-1 | Protected page redirects to sign-in with a return address | `AuthorizationTests`, `HomePageTests` |
| TC-NFR-SEC-001-2 | One account; wrong password and unknown username give the same generic message; POST without anti-forgery token is refused | `AccountControllerTests`, `AuthServiceTests` |
| TC-NFR-SEC-001-3 | Seed command output never contains the password; the web configuration and source contain no password value | `SeedUserRunnerTests` |
| TC-NFR-SEC-002-1 | Stored value is a hash that verifies with the original password and is not the plain value | `AspNetPasswordHasherTests`, `UserRepositoryTests` |

Infrastructure tests (no requirement ID, named in their comments): test database guard and connection (`TestDatabaseFixtureTests`), and the skeleton banner after sign-in (`LayoutTests`).

## 4. Manual Checks (Plan §6)

| Check | Method | Result |
|---|---|---|
| Migration applied to the development database | `dotnet ef database update`; read-only query of `Users` | Done. Table created |
| Seed account created | Console command (`seed-user`), run by the Project Owner | Done. "Account 'bmunukuri' created." |
| Stored password is a hash | Read-only query: hash length 84, prefix `AQAAAA` | Done. One account |
| Protected pages redirect when signed out | Browser and HTTP check | Done |
| Wrong password gives the generic message | HTTP check on the running app | Done |
| Sign-in and home page | Browser (Project Owner) | Done. Home page shown after sign-in |
| Sign out returns to sign-in | Browser | Not yet confirmed by the Project Owner |
| Browser matrix | Chrome and Edge, latest; record versions | Chrome or Edge version not recorded yet. Safari not required (DEC-019) |
| Repository and configuration scan | `git grep` for password assignments and the instance name | Done. Only synthetic test values found, in test files |
| Secrets | `dotnet user-secrets` holds the local connection only | Done |

## 5. Decisions Taken During Module 1

| Item | Decision | Reference |
|---|---|---|
| Plan items A-1 to A-4 | Taken as proposed in the plan. The Project Owner said "go ahead" for steps 4 onward, not "confirmed". Any can be changed | Plan §5 |
| Test database | `PatientManagement_Test`, guarded by name | DEC-018 context; plan §3A |
| Hasher interface | Interface in Business; adapter in Web, because Business and Data may not reference ASP.NET Core | Plan §3; Developer Guide §2 |
| Repository update saves | `UpdateAsync` saves the entity, because Business has no unit of work | `11-Repository-Design` §2.1 |
| Added repository methods | `AnyAsync` and `AddAsync`, for the seed command | `11-Repository-Design` §2.1 |
| Persistent sign-in cookie | Sign-in is persistent, so the 24-hour sliding timeout applies across browser restarts | PDR-20 (interpretation) |
| Password symbol | Any character that is not a letter, a digit or whitespace; whitespace does not count | `13-Validation-Strategy` §3.7 |
| Home page access | Requires sign-in; two home page tests changed from page content to the redirect, and the banner check moved to step 7 | NFR-SEC-001 AC-1 |
| Seed command | Runs only with `seed-user` as the first argument; prompts for username, email and password; never at normal startup | DEC-018 |
| Design rules D1 to D6 | Recorded with the recommended defaults | DEC-024; `17-` §6 |

## 6. Known Limits and Open Items

| # | Item | Effect | Proposed handling |
|---|---|---|---|
| 1 | No lockout after failed sign-in attempts | Repeated guessing is not limited | Not in the BRD; accepted for a single-user localhost app (plan A-4). Needs a BRD amendment to change |
| 2 | Unknown username returns sooner than a wrong password | Timing difference reveals whether a username exists | Same message is shown. Accepted for localhost; a dummy hash check would remove it |
| 3 | Seed command is not atomic | Two simultaneous runs could create two accounts | Single user on one machine. Accepted |
| 4 | HTTPS redirect warning on plain HTTP requests: "Failed to determine the https port for redirect" | Log noise only; pages still work | Choose: skip the redirect in Development, or set the HTTPS port. Affects transport rule (PDR-21). Not changed yet |
| 5 | Forgot-password flow has no second check (DEC-009) | Anyone who knows the email can take over the account | Accepted risk (RSK-003) |
| 6 | Browser versions not recorded | NFR-CMP-001 evidence incomplete | Record Chrome and Edge versions at the next manual run (PDR-28) |
| 7 | Sign out not yet confirmed in the browser | Last manual check open | Project Owner to confirm |
| 8 | `dotnet-ef` installed globally on this machine only | Other machines must install 10.0.14 | Documented in the Developer Guide §5 |
| 9 | Test password `Abcdef1!` is a literal in test files | Synthetic value only, not the account password | Accepted; scan (§4) confirms no real password is committed |

## 7. Commits

| Commit | Message |
|---|---|
| `3f097dd` | docs: amend BRD and CLAUDE.md for review decisions |
| `5e7f5c2` | docs: apply planning and roadmap fixes from implementation review |
| `70c84e3` | docs: add Module 1 implementation plan |
| `b7eaf2c` | docs: add test database and hasher interface to Module 1 plan |
| `4921d34` | test: add test database script and fixture with name guard |
| `2719608` | feat(data): add User entity and AddUsers migration |
| `f006464` | feat(data): add user repository with lookups and update |
| `022f74a` | feat(business): add password policy validator |
| `6955540` | feat(business): add auth service with sign-in, reset and seed account |
| `7400695` | feat(web): add sign-in cookie, global authorization and password hasher |
| `59ef706` | feat(web): add sign-in, forgot-password and sign-out pages |
| `5021a37` | feat(web): add signed-in top bar with sign out |
| `8d1f2d1` | feat(web): add console seed command for the single account |

## 8. Requested Decision

The Project Owner is asked to:

1. Confirm the sign-out check in the browser (§4), and record the Chrome and Edge versions used.
2. Decide item 4 in §6 (HTTPS redirect in Development).
3. Confirm or change plan items A-1 to A-4 (plan §5), which were taken as proposed.
4. Approve Module 1 as complete, so Module 2 (Patient Management) can start.
