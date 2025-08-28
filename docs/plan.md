# Agenix.ATF Improvement Plan

Date: 2025-08-25
Sources: Synthesized from docs/ROADMAP.md, docs/tasks.md, README.md, and Directory.Build.props (requirements.md is not present in the repo).

---

## 1) Key Goals and Hard Constraints (Extracted Summary)

Goals (from ROADMAP and tasks):
- Establish async-first, I/O-bound architecture with cooperative cancellation and timeouts.
- Enrich validation and matcher ecosystem, including NUnit-focused assertions and improved XML schema validation.
- Strengthen Screenplay concurrency primitives (parallelism, error aggregation, cancellation semantics).
- Improve robustness of SQL utilities (streaming, delimiters, comments/quotes, cancellation).
- Raise code quality via nullable annotations, analyzers, consistent API naming, and immutability where appropriate.
- Introduce resilience policies for external I/O and consolidate configuration via Options + DI.
- Enhance observability (structured logging with correlation; OpenTelemetry tracing) and documentation.
- Increase test coverage and rigor (property-based tests, benchmarks, mutation testing); improve CI publishing and gates.
- Expand ecosystem modules (SSH/SFTP, MassTransit, MongoDB, DI adapters, additional test runners, YAML DSL).

Constraints (from Directory.Build.props and repo shape):
- Target Framework: net8.0; RuntimeFrameworkVersion: 8.0.11 (ensure runtime availability).
- Central package management enabled (ManagePackageVersionsCentrally=true); MinVer for versioning (tag prefix v).
- Test project detection by .csproj name suffix .Tests; shared test packages configured solution-wide.
- SourceLink and symbol packages enabled; deterministic and CI-aware builds.
- Current TreatWarningsAsErrors=false globally; tasks require elevating to errors in CI only.

---

## 2) Build, Tooling, and Repository Hygiene

Proposals
- Centralize package versions in Directory.Packages.props; migrate any PackageVersion entries from Directory.Build.props.
  - Rationale: Reduces drift, aligns with ManagePackageVersionsCentrally=true, simplifies upgrades.
- Enable analyzers with a baseline (.editorconfig) and gradually tighten rules.
  - Rationale: Catch issues early while avoiding immediate churn; supports maintainable quality growth.
- Treat warnings as errors in CI builds only (keep local flexibility), integrated with ContinuousIntegrationBuild.
  - Rationale: Prevents new issues from entering main while not blocking local exploration; aligns with tasks.md.
- Enable nullable reference types across source projects and address high-impact warnings first (Core, Screenplay, Validation).
  - Rationale: Improves API correctness and safety; lowers NRE risk in complex flows.
- Maintain MinVer-driven versioning; avoid manual edits to assembly/file versions.
  - Rationale: Consistent semantic versioning tied to tags; simpler release process.

Acceptance
- Directory.Packages.props exists; no duplicate version declarations; CI runs analyzers; warnings fail CI; nullability warnings resolved in prioritized projects.

---

## 3) Architecture & API Modernization (Async‑First)

Proposals
- Transition to async-first APIs across I/O and orchestration boundaries; preserve sync shims with [Obsolete] guidance.
  - Rationale: Aligns with ROADMAP (I/O Bound), enables efficient concurrency and resource usage.
- Standardize naming: Async suffix on asynchronous methods; avoid ambiguous sync wrappers.
  - Rationale: Consistent developer experience; reduces confusion; aligns with .NET conventions.
- Backward compatibility layer and migration guide (codefix hints where feasible).
  - Rationale: Smooth upgrade path for adopters; reduces breaking-change friction.

Acceptance
- Public surface adheres to async naming; key flows provide async APIs; obsolete sync APIs point to async equivalents.

---

## 4) Cancellation and Timeouts (Cross‑Cutting)

Proposals
- Propagate CancellationToken through Screenplay (Actor.AttemptsTo/AttemptsToAsync, IPerformable.PerformAsAsync, AnonymousPerformable* and ITask delegates).
  - Rationale: Cooperative cancellation across nested tasks; necessary for stability in large suites.
- Add optional timeout parameters (or linked tokens) for long-running operations; document defaults.
  - Rationale: Predictable behavior under stalls; supports fail-fast strategies.

Acceptance
- Tests demonstrate that cancellation/timeout at the top level cancels nested operations and yields clear error messages.

---

## 5) Screenplay Concurrency & Failure Semantics

Proposals
- Enhance InParallel to support:
  - Max degree of parallelism.
  - Cancellation propagation.
  - Error aggregation modes (fail-fast vs. collect all).
  - Optional overall timeout.
  - Rationale: Gives test authors precise control; aligns with tasks.md and concurrency goals.
- Improve error context: Include actor name, task title, and aggregated exceptions preserving stack traces.
  - Rationale: Faster diagnosis; clearer reporting for complex failures.

Acceptance
- New/updated tests cover success, fail-fast, aggregate, cancellation, timeout; error messages include actionable context.

---

## 6) Validation Ecosystem

Proposals
- Enrich core matchers (string, numeric, collection, date/time; HTTP/JSON/XML domain matchers; composite composition).
  - Rationale: Matches ROADMAP Phase 1/2; reduces custom assertion code; improves readability.
- Add Agenix.Validation.NUnit with NUnit-specific assertions and constraints.
  - Rationale: Native NUnit integration streamlines test authoring and reporting.
- Improve XML schema validation (XSD) with detailed messages and performance improvements.
  - Rationale: Tightens XML workflows; enhances developer feedback.

Acceptance
- Packages and docs available; tests cover matcher composition, NUnit-specific assertions, and improved XML errors.

---

## 7) SQL Utilities (Agenix.Sql) Robustness & Async

Proposals
- Add CancellationToken support to CreateStatementsFromFileResource overloads and propagate through streaming readers.
- Handle final statement without a trailing terminator.
- Correctly ignore delimiters inside quotes and block comments; support configurable delimiter.
- Maintain streaming/low-allocation characteristics; add thorough tests for comments, quoted delimiters, large files, and cancellation.
  - Rationale: Reliability for large SQL scripts and enterprise scenarios; aligns with tasks.md.

Acceptance
- New tests pass; API remains efficient; edge cases handled without regressions.

---

## 8) Resilience, Configuration, and DI

Proposals
- Introduce resilience policies (retry/backoff/circuit-breaker) via Polly or built-in handlers; make policies injectable.
  - Rationale: Handles transient faults in HTTP/GraphQL/SQL; improves stability of integration tests.
- Adopt Microsoft.Extensions.Options with validation for settings; avoid static singletons; integrate with DI.
  - Rationale: Standard, testable configuration pattern; safer defaults and validation.
- Provide Microsoft.DI integration helpers (Agenix.Microsoft.DI) per ROADMAP.
  - Rationale: Easier adoption in modern .NET apps; consistent bootstrapping.

Acceptance
- Policies configurable by options; integration tests simulate transient failures; invalid options fail fast with clear messages.

---

## 9) Observability: Logging, Correlation, and Tracing

Proposals
- Standardize on Microsoft.Extensions.Logging; introduce structured scopes (Actor, Task); enrich with correlation (Activity.Current/TraceId).
  - Rationale: Consistent logs across modules; improved traceability in distributed runs.
- Add OpenTelemetry spans around Screenplay execution and I/O with useful attributes.
  - Rationale: End-to-end visibility and performance insights; aligns with tasks.md.
- Provide a sample and docs for local OTLP collector.

Acceptance
- Logs include correlation IDs and structured fields; traces show end-to-end spans with attributes; sample runs end-to-end.

---

## 10) Testing, Coverage, CI, and Benchmarks

Proposals
- Increase coverage with focused tests (Actor lifecycle, teardown on failure, concurrency semantics, edge cases for validators/matchers).
- Enable code coverage collection in CI; publish artifacts; set thresholds for critical modules.
- Introduce BenchmarkDotNet projects for hot paths (Screenplay orchestration, matchers, marshalling) and track trends.
- Adopt mutation testing (Stryker.NET) for critical logic to improve test rigor.
  - Rationale: Ensures correctness under change; prevents regressions; provides performance guardrails.

Acceptance
- CI publishes coverage and benchmark artifacts; baseline mutation score exists with improvement targets.

---

## 11) Documentation & Migration Aids

Proposals
- Expand developer guides for Screenplay: Tasks, Questions, Abilities; best practices for cancellation, errors, parallelism; examples for AnonymousPerformable*, ITask.Where, InParallel.
- Add SQL scripting guidelines documenting supported features and caveats.
- Provide migration guides for API changes (async, naming) and annotate obsolete members with actionable messages.
  - Rationale: Lowers adoption friction and clarifies new behaviors.

Acceptance
- New docs published under docs/ with links from README; obsolete warnings point to guides.

---

## 12) Security, Supply‑Chain, and Governance

Proposals
- Add Dependabot (or Renovate) configuration for NuGet and GitHub Actions; license audits included in CI.
- Ensure secrets are not committed; document secure patterns for integration tests; consider strong-name signing if required.
  - Rationale: Continuous hygiene and compliance.

Acceptance
- Automated dependency update PRs; CI includes license/security checks; guidance on secrets in tests.

---

## 13) Ecosystem Expansion (Sequenced After Core Changes)

Proposals (dependent on async/I-O modernization and resilience foundations):
- Agenix.Ssh and Agenix.Sftp for remote command execution and file transfer.
- Agenix.MassTransit for brokered messaging (ActiveMQ, RabbitMQ, Azure SB, Kafka).
- Agenix.MongoDB connector for document-based testing and GridFS.
- Test runner integrations (Agenix.XUnit.Runtime, Agenix.VSUnit.Runtime).
- Agenix.Microsoft.DI helpers for service registration and scoped execution.
- Agenix-YAML-DSL for DSL-based authoring and generation to C#.
  - Rationale: Expands reach and supports additional platforms/workflows; follow ROADMAP sequencing.

Acceptance
- Each module delivers documented, test-backed features with clear examples; dependencies on core items resolved first.

---

## 14) Phased Timeline & Dependencies (Practical Mapping)

Phase A (0–2 months)
- Build/tooling hygiene: nullable, analyzers, warnings-as-errors in CI, package centralization.
- Screenplay cancellation + timeout propagation; API naming consistency and [Obsolete] routes.
- SqlUtils robustness backlog and tests.
- Logging standardization + correlation scopes.

Phase B (2–4 months)
- InParallel enhancements (MDoP, cancellation, error aggregation, timeout) with tests.
- Validation expansions (core matchers) and NUnit validation package.
- XML schema validation improvements.
- Resilience policies (HTTP/GraphQL/SQL) + Options/DI consolidation.

Phase C (4–6 months)
- Coverage push and CI publishing; benchmarks and initial mutation testing baselines.
- Documentation expansions and migration guides.

Phase D (6–9 months)
- Ecosystem modules: SSH/SFTP, MassTransit, MongoDB, DI helpers, xUnit/MSTest runtimes; groundwork for YAML DSL.

Note: Sequencing respects dependencies called out in ROADMAP (e.g., I/O-bound framework updates before dependent modules).

---

## 15) Risks & Mitigations

- Breaking Changes from Async Migration
  - Mitigation: Provide adapters, Obsolete messages with guidance, and migration docs; version via MinVer pre-releases.
- Flaky Concurrency Tests
  - Mitigation: Deterministic test harness, timeouts, and isolation; clear FQN filtering for heavy UI/network tests as needed.
- Analyzer Noise/Churn
  - Mitigation: Baseline with .editorconfig; tighten rules incrementally; treat as errors in CI only.
- Performance Regressions
  - Mitigation: Benchmarks in CI, performance budgets, and PR perf checks for hot paths.

---

## 16) Concrete Next Steps (Backlog Seeds)

1. Add Directory.Packages.props and migrate version declarations.
2. Add analyzers and .editorconfig baseline; wire analyzer execution in CI.
3. Set TreatWarningsAsErrors=true for CI/Release via props conditions; keep local debug flexible.
4. Enable <Nullable>enable</Nullable> in source projects; fix high-priority warnings in Core/Screenplay/Validation.
5. Design CancellationToken propagation for Screenplay; draft API changes and tests; mark old APIs [Obsolete].
6. Implement SqlUtils parsing/cancellation improvements with comprehensive tests.
7. Introduce logging scopes and correlation; add OpenTelemetry instrumentation points with a sample.
8. Implement InParallel enhancements and test matrix (success, fail-fast, aggregate, cancellation, timeout).
9. Enrich matchers; create Agenix.Validation.NUnit; improve XML validation; add docs/samples.
10. Add resilience policies and Options/DI pattern across I/O modules.
11. Expand docs: Screenplay guide, SQL scripting, migration guides; link from README.
12. CI enhancements: coverage publish, caching, matrices as appropriate; add mutation testing baseline.

---

## 17) Acceptance Criteria Summary by Theme

- Build/Tooling: Central package mgmt, analyzers in CI, warnings as errors (CI), nullable enabled with prioritized fixes.
- Architecture/API: Async-first with consistent naming and compatibility shims.
- Cancellation/Timeouts: Propagation across Screenplay; timeouts documented and enforced.
- Concurrency: InParallel supports MDoP, cancellation, error aggregation, timeout with clear error context.
- Validation: Rich matchers, NUnit validation package, improved XML validation.
- SQL: Robust parsing and cancellation; streaming-efficient; comprehensive tests.
- Resilience/Config/DI: Injectable policies; Options pattern with validation; DI helpers available.
- Observability: Structured logging with correlation; OTEL traces around key operations.
- Testing/CI: Increased coverage, benchmark artifacts, mutation baseline; CI surfaces results.
- Documentation: Expanded guides and migration docs; README links updated.
- Security/Supply-chain: Automated dependency updates and license/security checks.
- Ecosystem: New modules added post‑core modernization with examples and tests.

---

Notes
- This plan intentionally aligns the actionable tasks (docs/tasks.md) with the strategic ROADMAP and current repo constraints. It provides rationales to help prioritize and communicate trade‑offs as work proceeds.
