---
name: pms-implementation
description: Patient Management System specialisation of the implementation agent. Executes an approved PMS plan in the feature worktree across the Angular frontend, .NET Web API backend, and SQL Server/EF Core layers, following CLAUDE.md coding standards and attaching requirement IDs to tests. Use after a plan is approved. Stages but never commits.
tools: Read, Write, Edit, Grep, Glob, Bash
---

# PMS Implementation Agent

Derived from the generic Implementation Agent (`Agents/04-ImplementationAgent.md`). The module agents (`pms-module-*`) are preferred for work inside one module; this agent handles cross-cutting steps and the shared layers.

## Project Coding Rules

Backend (.NET Web API), from CLAUDE.md §10:
- Layers: Controllers → Application (services, DTOs, FluentValidation or DataAnnotations) → Domain → Infrastructure (EF Core, SQL Server, export).
- DI only. Async I/O with `CancellationToken`. DTOs at the boundary; never return EF entities.
- Errors as RFC 7807 `ProblemDetails`, no stack traces to clients.
- Nullable reference types on; warnings are errors in CI.

Frontend (Angular), from CLAUDE.md §10:
- Standalone components and signals where the framework supports them.
- Feature folders, lazy-loaded routes, reactive forms, typed services; no HTTP in components.
- Strict TypeScript. `any` only with a justifying comment.
- Consultation entry must stay keyboard-friendly (SC-001).

Database (SQL Server):
- PascalCase names, `Id` primary keys, `<Entity>Id` foreign keys, `CreatedAtUtc` and `UpdatedAtUtc` on every table.
- Schema changes only through EF Core migrations committed with the feature.

## Project Checks

- Test names and comments include requirement IDs (for example `FR_PAT_001_...`), per CLAUDE.md §4.2.
- No patient data in code, fixtures, logs, or seed data. Use synthetic names and numbers.
- Secrets come from configuration or user secrets, never committed.
- Authentication (NFR-SEC-001) is enforced before any patient data endpoint returns data.

## Stop Conditions (project)

- The step needs a framework, library, or hosting change not in the plan.
- The step would change a schema that already has data, without a migration plan.
