# Agenix Improvement Tasks Checklist

Note: Each item is actionable and ordered to reduce risk and maximize foundational benefits first. Check off items as they are completed.

1. [ ] Enable solution-wide nullable reference types and address warnings
   - Action: Turn on <Nullable>enable</Nullable> in Directory.Build.props (source projects) and fix resulting warnings in critical projects first (Core, Screenplay, Validation). 
   - Acceptance: No nullable warnings in CI for targeted projects; critical APIs annotated appropriately.

2. [ ] Introduce and configure code analyzers with baseline
   - Action: Add Microsoft.CodeAnalysis.NetAnalyzers (and optionally StyleCop.Analyzers) to solution; set severity rules; generate a .editorconfig baseline to avoid churn, then incrementally tighten.
   - Acceptance: CI runs analyzers; new code adheres to rules; baseline violations tracked and gradually reduced.

3. [ ] Treat warnings as errors in CI while allowing local opt-out
   - Action: For Release/CI builds, set TreatWarningsAsErrors=true (retain current local flexibility). 
   - Acceptance: CI fails on new warnings in Release or CI environment; developer local Debug unaffected.

4. [ ] Centralize package versions in Directory.Packages.props (MPCM)
   - Action: Add Directory.Packages.props to pin package versions across solution; move scattered PackageVersion entries from Directory.Build.props into it.
   - Acceptance: All PackageReference items resolve from central versions; no duplicate version declarations remain.

5. [ ] Standardize logging and correlation across modules
   - Action: Ensure all libraries use Microsoft.Extensions.Logging abstractions consistently; add correlation (Activity.Current/TraceId) enrichment and structured logging scopes for Actor/Task runs.
   - Acceptance: Logs include correlation identifiers; Screenplay task execution produces consistent, structured logs.

6. [ ] Propagate CancellationToken across Screenplay APIs
   - Action: Update Actor.AttemptsTo/AttemptsToAsync and related flows to accept optional CancellationToken and pass it to IPerformable.PerformAsAsync; update AnonymousPerformable*, ITask delegates to accept and respect cancellation.
   - Acceptance: Cancellation from calling tests cancels nested tasks; new tests prove cooperative cancellation.

7. [ ] Unify error handling strategy in Screenplay
   - Action: Define clear exception taxonomy (assertion vs. operational); ensure Actor.Should and error tallying classify and report with context (task title, actor name), preserving stack traces.
   - Acceptance: Failures are reported with actionable context; tests for BrokenQuestion reflect improved messages.

8. [ ] Provide parallel execution primitives with robust failure semantics
   - Action: Review InParallel API; document and/or enhance to support: max degree of parallelism, cancellation, error aggregation modes (fail-fast vs. collect), and timeout.
   - Acceptance: Parallel tests cover success, fail-fast, aggregate errors, and cancellation; APIs documented.

9. [ ] Improve SqlUtils robustness and async behavior
   - Action: 
     - Add CancellationToken support to CreateStatementsFromFileResource overloads and propagate it through StreamReader.ReadLineAsync.
     - Handle last statement without trailing terminator.
     - Handle semicolons within quoted strings and block comments; optionally support configurable delimiter.
     - Add tests covering comments, quoted delimiters, large files, and cancellation.
   - Acceptance: New unit tests pass; method remains allocation-efficient and streaming-friendly.

10. [ ] Strengthen exception types and messages for infrastructure modules
    - Action: Audit Agenix.Api.Exceptions usage; ensure wrapping preserves inner exceptions; add contextual data (resource path, SQL line numbers, actor/task titles) to message and Data.
    - Acceptance: Exceptions provide enough context for troubleshooting; regression tests updated.

11. [ ] Introduce resilience policies for external I/O (HTTP/GraphQL/SQL)
    - Action: Use Polly (or built-in handlers) for retry/backoff/circuit-breaker where appropriate; ensure policies are injectable and testable.
    - Acceptance: Integration tests simulate transient failures and verify resilience; policies configurable via options.

12. [ ] Consolidate configuration and options pattern
    - Action: Adopt Microsoft.Extensions.Options for module settings; provide validation for options; avoid static singletons where DI is appropriate.
    - Acceptance: Modules consume typed options; invalid configuration fails fast with clear messages.

13. [ ] Enforce immutability for value/config types
    - Action: Convert simple DTOs/options to records with init-only setters; avoid public mutable collections (use IReadOnly*); copy on write where needed.
    - Acceptance: Public API surfaces immutable state; analyzers/tests guard against unintended mutation.

14. [ ] API consistency and naming pass
    - Action: Ensure method names are consistent (async suffix for async methods, e.g., AttemptsToAsync everywhere); add overloads/obsolete attributes to guide migration where needed.
    - Acceptance: Public APIs follow .NET naming guidelines; obsolete paths have guidance and migration notes.

15. [ ] Timeouts across async operations
    - Action: Add optional TimeSpan timeout parameters (or CancellationToken with linked timeout) to long-running operations in Screenplay and I/O heavy modules; document defaults.
    - Acceptance: Operations respect timeouts; tests verify timeout behavior.

16. [ ] Telemetry and tracing integration
    - Action: Add OpenTelemetry instrumentation points around Screenplay task execution and I/O calls; export spans with attributes (actor, task title, outcome).
    - Acceptance: Example project demonstrates end-to-end traces in a local OTLP collector; docs added.

17. [ ] Documentation: Developer guide for Screenplay usage and extension
    - Action: Expand README/Docs with patterns for creating Tasks, Questions, Abilities; best practices for cancellation, error handling, parallelism; examples using ITask.Where, AnonymousPerformable*, InParallel.
    - Acceptance: New docs pages published; links from README added.

18. [ ] Documentation: SQL scripting guidelines
    - Action: Document supported SQL script features, statement terminators, comments, and edge cases; show how to use decorators and cancellation.
    - Acceptance: SqlUtils docs exist with examples and caveats.

19. [ ] Testing: Increase coverage for edge cases and concurrency
    - Action: Add tests for Actor lifecycle (Begin/EndPerformance/WrapUp), Fact listeners cleanup, teardown execution on failure; property-based tests for matchers/validators where applicable.
    - Acceptance: Coverage increases for Screenplay and Validation projects; flaky tests identified and stabilized.

20. [ ] Performance/benchmarking for hot paths
    - Action: Introduce BenchmarkDotNet projects to measure Screenplay task orchestration, matcher evaluation, and serialization/marshalling; set performance budgets.
    - Acceptance: Benchmarks run in CI (as non-blocking) and trends documented.

21. [ ] Security hygiene and supply-chain
    - Action: Add Dependabot/Updates configuration; audit package licenses; ensure strong name signing if required; review handling of secrets in tests.
    - Acceptance: Automated dependency update PRs; license and security checks included in CI.

22. [ ] CI enhancements
    - Action: Add matrix builds for OS/TFM if needed; add test result and code coverage publishing; fail on low coverage threshold for critical projects; cache NuGet packages.
    - Acceptance: CI pipeline surfaces coverage and test artifacts; deterministic builds validated.

23. [ ] Introduce mutation testing for critical logic
    - Action: Add Stryker.NET to Screenplay and Validation projects to improve test rigor.
    - Acceptance: Baseline mutation score established; improvement tracked over time.

24. [ ] Roadmap alignment tasks
    - Action: Break down ROADMAP Phase 1/2 items into GitHub issues with clear scope, dependencies, and estimates; cross-reference items above.
    - Acceptance: Public backlog reflects actionable issues linked to this checklist.

25. [ ] Deprecation and migration guides
    - Action: For API changes (cancellation, naming), provide migration docs and Obsolete attributes with messages and links.
    - Acceptance: Upgrading projects have a clear path; build warnings guide migration.
