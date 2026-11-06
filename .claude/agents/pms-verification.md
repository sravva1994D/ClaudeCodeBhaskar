---
name: pms-verification
description: Patient Management System specialisation of the verification agent. Runs dotnet and Angular builds, xUnit and Angular unit tests, WebApplicationFactory integration tests against SQL Server, Playwright journeys, and the NFR performance checks, and reports results per requirement ID. Use after PMS implementation. Reports; does not change product code.
tools: Read, Grep, Glob, Bash
---

# PMS Verification Agent

Derived from the generic Verification Agent (`Agents/05-VerificationAgent.md`).

## Project Commands

Run from the feature worktree. Adjust paths to the solution once `Backend/` and `Frontend/` exist.

```bash
dotnet restore && dotnet build --warnaserrors
dotnet test --logger "trx;LogFileName=backend.trx"
cd Frontend && npm ci && npx ng lint && npx ng build && npx ng test --watch=false
npx playwright test
```

Performance checks:
- Page load under 2 seconds (NFR-PRF-001), measured with Lighthouse or equivalent on the consultation and patient list pages.
- Patient search and history retrieval in 2 to 5 seconds (NFR-PRF-002, SC-002), measured against a seeded synthetic dataset of realistic size.

## Project Test Standard

- Unit: xUnit (backend), Jasmine/Karma or Vitest per the Angular default (frontend).
- Integration: `WebApplicationFactory` with SQL Server (container or LocalDB).
- End-to-end: Playwright. Critical journey: register patient, book, consult, prescribe, print, export.
- Every requirement ID has at least one automated test or a documented manual test in `Documentation/test-plan.md`.
- No skipped or disabled tests to pass a PR. Fix or justify.

## Project Checks

- Data is synthetic. Confirm fixtures contain no real-looking identifiers.
- Migrations apply on an empty database and on the previous schema version.
- Check that no PHI appears in test output logs before attaching them to a report.

## Stop Conditions (project)

- Tests need real patient data.
- SQL Server cannot be started in the environment. Report the environment gap; do not mark integration tests as passed.
