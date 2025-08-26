Agenix.ATF – Development Guidelines (Project‑Specific)

Audience: Advanced .NET contributors working on Agenix.ATF.
Scope: Build/configuration, testing practices, and repo‑specific development tips. Keep this close when adding code or tests.

1. Build and configuration

- Toolchain
  - .NET SDK: global.json pins to 8.0.0 with rollForward=latestMajor. Use any 8.x SDK, but ensure the .NET 8 runtime 8.0.11 is available because Directory.Build.props sets RuntimeFrameworkVersion=8.0.11.
    - Verify: dotnet --info
  - Target Framework: net8.0 across projects (Directory.Build.props).
  - Central package management: ManagePackageVersionsCentrally=true. Prefer adding versions to central management rather than in individual .csproj files when introducing new packages.

- Solution‑wide MSBuild shape (Directory.Build.props)
  - Test project detection: Projects whose name ends with .Tests get IsTestProject=true and IsPackable=false; they also receive common test packages automatically (Microsoft.NET.Test.Sdk, NUnit 4.x, NUnit3TestAdapter 5.x, NSubstitute, coverlet.collector).
  - SourceLink and symbols enabled for better debugging.
  - MinVer is used for versioning; tag prefix v (see MinVerTagPrefix). Avoid hand‑editing assembly/file versions.
  - CI semantics: ContinuousIntegrationBuild=true for Release or when GITHUB_ACTIONS/CI is set.

- Repo structure tips
  - Library modules (Agenix.*) are packable by default; test modules (*.Tests) are not.
  - JetBrains.Annotations package is available solution‑wide; use attributes (e.g., [PublicAPI]) where it clarifies intent.

2. Testing

- Framework & adapters
  - NUnit 4 with NUnit3TestAdapter and Microsoft.NET.Test.Sdk drive discovery/execution.
  - Many integration tests use Agenix.NUnit.Runtime (custom NUnit integration). If your test needs Agenix context injection or custom lifecycle, annotate the test class or assembly with [NUnitAgenixSupport] from namespace Agenix.NUnit.Runtime.Agenix.NUnit.Attribute.

- Running tests (CLI)
  - All tests for the solution:
    - dotnet test Agenix.ATF.sln
  - Single project:
    - dotnet test Agenix.Screenplay.Tests/Agenix.Screenplay.Tests.csproj
  - Filtered by Fully Qualified Name (fastest and most reliable):
    - dotnet test Agenix.Screenplay.Tests/Agenix.Screenplay.Tests.csproj --filter "FullyQualifiedName~Agenix.Screenplay.Tests.Parallel.WhenRunningTasksInParallel.ParallelTasksShouldRunWithASingleAction"
  - Filtered by namespace or class:
    - dotnet test Agenix.Core.Tests/Agenix.Core.Tests.csproj --filter "FullyQualifiedName~Agenix.Core.Tests.NUnitIntegration"

- Code coverage
  - coverlet.collector is already referenced. Use:
    - dotnet test --collect:"XPlat Code Coverage"
  - Coverage artifacts will be emitted under TestResults/**/coverage.* (format depends on your collector configuration and toolchain).

- UI/External integration test notes
  - Playwright
    - Microsoft.Playwright is used by Agenix.Playwright and Agenix.Playwright.Tests. Browsers must be installed before running any tests touching Playwright APIs.
    - Option A (global tool):
      - dotnet tool install --global Microsoft.Playwright.CLI
      - playwright install
    - Option B (per‑project script after build):
      - dotnet build Agenix.Playwright/Agenix.Playwright.csproj
      - pwsh ./Agenix.Playwright/bin/Debug/net8.0/playwright.ps1 install
    - In CI or headless environments without browsers, avoid running Playwright tests by filtering them out (e.g., --filter "FullyQualifiedName!~Agenix.Playwright.Tests").
  - Selenium
    - Agenix.Selenium and Agenix.Selenium.Tests require appropriate drivers/browsers in PATH. In constrained environments, exclude with a filter (e.g., --filter "FullyQualifiedName!~Agenix.Selenium.Tests").
  - SQL
    - Agenix.Sql.Tests contains integration tests. If your environment lacks the required database or configuration, run unit‑only subsets or exclude integration namespaces via FQN filtering.

- Logging during tests
  - Some test projects ship log4net.config and copy it to output (see Agenix.Screenplay.Tests). No action needed normally; logs can help diagnose failures, particularly for integration tests.

- Parallelism considerations
  - The codebase contains explicit parallel execution utilities (e.g., InParallel in Agenix.Screenplay). When investigating ordering issues, prefer running affected namespaces/classes in isolation and inspect test logic rather than tweaking runner‑level parallelism.

- Adding a new test (verified workflow)
  - Choose an existing test project (recommended: a unit‑level project such as Agenix.Screenplay.Tests).
  - Create a file like GuidelinesDemoTests.cs with content:
    using NUnit.Framework;
    
    namespace Agenix.Screenplay.Tests;
    
    [TestFixture]
    public class GuidelinesDemoTests
    {
        [Test]
        public void Should_Pass()
        {
            Assert.Pass("Demo test executed successfully.");
        }
    }
  - Run exactly this test by FQN to confirm discovery:
    - dotnet test Agenix.Screenplay.Tests/Agenix.Screenplay.Tests.csproj --filter "FullyQualifiedName~Agenix.Screenplay.Tests.GuidelinesDemoTests.Should_Pass"
  - Remove the demo test when you are done (keep the repo clean unless the test is intended to stay).
  - If you need Agenix runtime context in a test, add [NUnitAgenixSupport] and the appropriate using:
    using Agenix.NUnit.Runtime.Agenix.NUnit.Attribute;

- Creating a new test project
  - Name it with .Tests suffix to inherit test‑project MSBuild properties and common packages from Directory.Build.props.
  - Add ProjectReference entries to the product assemblies under test and to Agenix.NUnit.Runtime if you will use [NUnitAgenixSupport].
  - Only add per‑project PackageReference items for dependencies not already provided by Directory.Build.props (e.g., specific logging or integration libs).

3. Additional development information

- Code style and language features
  - Nullable reference types and implicit usings are enabled by default in test projects (see representative .csproj files). Keep new code nullable‑clean.
  - Favor explicit namespaces aligned to folder structure (e.g., Agenix.Screenplay.Tests.*).

- Versioning and packaging
  - Do not hand‑edit assembly versions; MinVer determines versions from git tags (prefix v). When you need preview versions, use pre‑release identifiers configured in Directory.Build.props.

- SourceLink and symbols
  - SourceLink is enabled; publish/pack will embed PDB/snupkg data for better debugging. Maintain correct repository URLs and avoid rewriting history in a way that breaks SourceLink.

- Troubleshooting
  - Discovery issues: Ensure you are running with a .NET 8 SDK and that Microsoft.NET.Test.Sdk/NUnit packages are not overridden per‑project in a conflicting way. Prefer running dotnet test with a specific project and --filter on FullyQualifiedName.
  - Runtime mismatch: If tests fail to start due to missing runtimes, install .NET 8.0.11 runtime (or later 8.x that satisfies roll‑forward) matching the RuntimeFrameworkVersion in Directory.Build.props.
  - Environmental flakiness: For UI/Network/DB tests, first validate unit‑only namespaces; then add back integration tests once environment prerequisites are satisfied.

- Conventions you will see
  - Unit tests typically use class name suffix Tests; integration tests often end with IT and are decorated with [NUnitAgenixSupport].
  - Test namespaces reflect project/module structure: Agenix.<Module>.Tests.<Area>.

Appendix: Quick commands
- Restore and build all: dotnet restore && dotnet build -v minimal
- Run all tests: dotnet test Agenix.ATF.sln -v minimal
- Run a single test (FQN):
  dotnet test Agenix.Screenplay.Tests/Agenix.Screenplay.Tests.csproj \
    --filter "FullyQualifiedName~Agenix.Screenplay.Tests.Parallel.WhenRunningTasksInParallel.ParallelTasksShouldRunWithASingleAction"
- Collect coverage:
  dotnet test --collect:"XPlat Code Coverage"