![Logo][1]

<div align="center">

Agenix Integration Testing ![Icon][2]
==============
<!-- Tagline -->
  <p><strong>Test the Pieces.Verify the Whole.</strong></p>
  <p><em>Intelligent integration testing framework for modern applications</em></p>

[![Build Status](https://github.com/agenixframework/Agenix/actions/workflows/dotnet-pr-check.yml/badge.svg)](https://github.com/agenixframework/Agenix/actions)
[![Nuget](https://img.shields.io/nuget/v/Agenix.Core.svg)](https://www.nuget.org/packages/Agenix.Core/)
[![License](https://img.shields.io/badge/license-Apache--2.0-blue.svg)](https://github.com/agenixframework/agenix/blob/master/LICENSE)

</div>

**Agenix** is a comprehensive test automation framework built with **C# .NET 8.0** that enables the creation of fully
automated end-to-end use case tests for enterprise SOA applications. Designed with modern testing patterns and
practices, Agenix provides a robust foundation for testing complex distributed systems, APIs, databases, and user
workflows.

## Key Features

- **🎭 Screenplay Pattern**: Actor-based testing with fluent, readable test scenarios using Tasks, Questions, and
  Abilities
- **🌐 Multi-Protocol Support**: Built-in support for HTTP/REST APIs, GraphQL, SQL databases, and web services
- **🔧 Framework Integration**: Seamless integration with NUnit and Reqnroll (SpecFlow successor) for BDD testing
- **✅ Rich Validation**: Comprehensive validation capabilities for JSON, text, binary data, and custom formats
- **🏗️ Modular Architecture**: Clean separation of concerns with extensible, plugin-based design
- **📊 Advanced Reporting**: Detailed test execution reports with contextual information
- **🔄 Enterprise Ready**: Built for complex SOA environments with session management and context handling

## What Makes Agenix Different

### 1. **Intelligent Marshalling & Serialization Architecture**

- **Multi-Protocol Marshalling**: Agenix provides sophisticated object marshalling capabilities with automatic type
  detection and resolution
- **Pluggable Serializers**: Support for multiple serialization formats (JSON, XML, Binary) with automatic serializer
  selection
- **Dynamic Content Resolution**: Built-in variable substitution and dynamic content generation during test execution

### 2. **Enterprise-Grade Resource Management**

- **Resource Path Type Resolution**: Unique that can dynamically resolve types and properties from assembly resources
  `ResourcePathTypeResolver`
- **Cached Type Resolution**: Intelligent caching system for type lookups to improve performance in large test suites
- **Flexible Resource Loading**: Support for loading configuration and types from embedded resources with fallback
  mechanisms

### 3. **Comprehensive Validation Ecosystem**

- **Multi-Format Validation**: Dedicated validation modules for JSON, XML, Text, and Binary data
- **NHamcrest Integration**: Fluent assertion syntax with composable matchers
- **Context-Aware Validation**: Validation contexts that understand the testing environment and can adapt accordingly

### 4. **Advanced Message Building Pattern**

- **Polymorphic Payload Builders**: Support for different payload building strategies (marshalling, serialization,
  static content)
- **Header Data Builders**: Specialized builders for message headers with marshalling support
- **Type-Safe Message Construction**: Strong typing throughout the message building process

### 5. **Modular Protocol Support**

- **Protocol Abstraction**: Clean separation between protocol implementations (HTTP, GraphQL, SQL, etc.)
- **Endpoint Configuration**: Flexible endpoint configuration system that supports multiple communication protocols
- **Producer/Consumer Pattern**: Consistent messaging patterns across different protocols

### 6. **Intelligent Reference Resolution**

- **Dependency Injection**: Built-in reference resolver that acts as a lightweight DI container
- **Auto-Detection**: Automatic detection of available services and marshallers
- **Named Resolution**: Support for named instances and type-specific resolution

### 7. **Enterprise Integration Features**

- **Session Management**: Built-in session handling for complex enterprise workflows
- **Context Propagation**: Test context that maintains state across test steps
- **Transaction Support**: Database transaction management for data integrity

### 8. **BDD Integration Excellence**

- **Reqnroll Plugin**: Deep integration with Reqnroll (SpecFlow successor) for modern BDD testing
- **Gherkin Support**: Native support for Gherkin syntax with step definition automation
- **Business-Readable Tests**: Framework designed to bridge technical implementation and business requirements

### 9. **Test Execution Engine**

- **Action-Based Architecture**: Test actions as first-class citizens with lifecycle management
- **Sequence Management**: Before/after test sequences with dependency resolution
- **Listener Pattern**: Extensible test action listeners for custom behavior

### 10. **Developer Experience Focus**

- **Fluent APIs**: Consistent fluent interface design across all modules
- **Type Safety**: Strong typing throughout the framework to catch errors at compile time
- **IntelliSense Support**: Rich metadata and documentation for IDE integration

The framework's architecture demonstrates a sophisticated approach to test automation that goes beyond simple API
testing, providing a comprehensive platform for complex enterprise testing scenarios with emphasis on maintainability,
readability, and extensibility.

## ️ Framework Origins

This framework is **based on the [Citrus Framework](https://citrusframework.org/)**, originally developed for Java-based
integration testing. Key portions of this .NET implementation were derived from and inspired by the Citrus Framework
codebase.

- **Original Citrus Framework**: Copyright (C) 2006-2024 the original author or authors
- **License**: Both projects are licensed under Apache License 2.0
- **Agenix Implementation**: Copyright (C) 2025 Agenix

We extend our gratitude to the Citrus Framework maintainers and contributors for their excellent work that served as the
foundation for this .NET adaptation.

## License

This project is licensed under the Apache License 2.0 - see the [LICENSE](LICENSE) file for details.

This project includes software derived from the Citrus Framework - see the [NOTICE](NOTICE) file for additional
attribution details.

## Related Projects

- [Citrus Framework (Java)](https://github.com/citrusframework/citrus) - The original framework that inspired this
  implementation

## Table of Contents

- [Overview](#overview)
- [Prerequisites](#prerequisites)
- [Getting Started](#getting-started)
- [Module overview](#module-overview)
- [Development Guidelines](#development-guidelines)

## Overview

Agenix.ATF is a comprehensive test framework built on .NET 8.0, designed to provide robust testing capabilities with
modern testing patterns and practices.

### Technologies Used

- **.NET 8.0** - Target framework
- **C# 12.0** - Programming language
- **NUnit 3.x-4.x** - Testing framework
- **NSubstitute** - Mocking library

## Prerequisites

Before you begin, ensure you have the following installed:

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or later
- [JetBrains Rider](https://www.jetbrains.com/rider/) (recommended) or Visual Studio 2022+
- Git for version control

## Getting Started

### 1. Clone the Repository

```bash
git clone <repository-url>
cd Agenix.ATF
```

## Module Overview

### 🏗️ Core Framework Modules

| Module          | Description                                             | Key Features                                                                               |
|-----------------|---------------------------------------------------------|--------------------------------------------------------------------------------------------|
| **Agenix.Api**  | Core interfaces and contracts for the testing framework | • API definitions<br>• Interface contracts<br>• Common types<br>• Exception definitions    |
| **Agenix.Core** | Main implementation of the framework logic              | • Test execution engine<br>• Context management<br>• Action builders<br>• Session handling |

### 🌐 Protocol & Communication Modules

| Module             | Description                                       | Key Features                                                                                      |
|--------------------|---------------------------------------------------|---------------------------------------------------------------------------------------------------|
| **Agenix.Http**    | HTTP client testing capabilities and utilities    | • HTTP request/response handling<br>• REST API testing<br>• HTTP validation                       |
| **Agenix.GraphQL** | GraphQL client testing capabilities and utilities | • GraphQL query/mutation handling<br>• GraphQL server integration<br>• Response validation        |
| **Agenix.Sql**     | Database testing and SQL execution utilities      | • Database connectivity<br>• SQL query execution<br>• Data validation<br>• Transaction management |

### 🎭 Testing Pattern Implementations

| Module                | Description                                          | Key Features                                                                                              |
|-----------------------|------------------------------------------------------|-----------------------------------------------------------------------------------------------------------|
| **Agenix.Screenplay** | Screenplay pattern implementation for readable tests | • Actor-based testing<br>• Task and Question abstractions<br>• Ability pattern<br>• Fluent test scenarios |

### 🔌 Framework Integration Modules

| Module                    | Description                                   | Key Features                                                                                       |
|---------------------------|-----------------------------------------------|----------------------------------------------------------------------------------------------------|
| **Agenix.NUnit.Runtime**  | NUnit test framework integration              | • NUnit test execution<br>• Custom attributes<br>• Test lifecycle management<br>• Result reporting |
| **Agenix.ReqnrollPlugin** | Reqnroll (SpecFlow successor) BDD integration | • Gherkin syntax support<br>• Step definitions<br>• Feature file parsing<br>• BDD test execution   |

### ✅ Validation & Assertion Modules

| Module                          | Description                        | Key Features                                                                                                                    |
|---------------------------------|------------------------------------|---------------------------------------------------------------------------------------------------------------------------------|
| **Agenix.Validation.Binary**    | Binary file and data validation    | • Binary file comparison<br>• Byte array validation<br>• File format verification<br>• Checksum validation                      |
| **Agenix.Validation.Json**      | JSON schema and content validation | • JSON schema validation<br>• Content comparison<br>• Path-based assertions<br>• Structure verification                         |
| **Agenix.Validation.NHamcrest** | Hamcrest-style matcher library     | • Fluent assertion syntax<br>• Custom matchers<br>• Readable error messages<br>• Composable assertions                          |
| **Agenix.Validation.Text**      | Text content and format validation | • String comparison<br>• Pattern matching<br>• Format validation<br>• Text transformation                                       |
| **Agenix.Validation.Xml**       | XML schema and content validation  | • XML schema validation<br>• Content comparison<br>• XPath-based assertions<br>• Structure verification<br>• Namespace handling |

### 🧪 Unit Test Project Modules

| Test Project                          | Purpose                               | Test Types                                                                       |
|---------------------------------------|---------------------------------------|----------------------------------------------------------------------------------|
| **Agenix.Core.NUnitTestProject**      | Core framework functionality testing  | • Unit tests<br>• Integration tests<br>• Framework behavior validation           |
| **Agenix.Screenplay.Tests**           | Screenplay pattern feature validation | • Actor behavior tests<br>• Task execution tests<br>• Question answering tests   |
| **Agenix.ReqnrollPlugin.Tests**       | BDD plugin functionality testing      | • Step definition tests<br>• Feature parsing tests<br>• Plugin integration tests |
| **Agenix.Validation.Binary.Tests**    | Binary validation module testing      | • Binary comparison tests<br>• File validation tests                             |
| **Agenix.Validation.Json.Tests**      | JSON validation module testing        | • Schema validation tests<br>• JSON comparison tests                             |
| **Agenix.Validation.NHamcrest.Tests** | Hamcrest matcher testing              | • Matcher behavior tests<br>• Assertion tests                                    |
| **Agenix.Validation.Text.Tests**      | Text validation module testing        | • Text comparison tests<br>• Pattern matching tests                              |

### 📋 Configuration & Support Files

| Component                  | Purpose                                 | Features                                                                             |
|----------------------------|-----------------------------------------|--------------------------------------------------------------------------------------|
| **Solution Configuration** | Project organization and build settings | • Multi-project solution<br>• Shared build configuration<br>• Dependency management  |
| **IDE Settings**           | Development environment standardization | • Code style enforcement<br>• License header automation<br>• Shared formatting rules |
| **Templates**              | Code generation and standardization     | • License header templates<br>• File templates<br>• Coding standards                 |

### 🎯 Framework Capabilities

| Capability              | Supported Modules           | Use Cases                             |
|-------------------------|-----------------------------|---------------------------------------|
| **API Testing**         | Http, Core, Validation.Json | REST API validation, HTTP workflows   |
| **Database Testing**    | Sql, Core, Validation.Text  | Data validation, SQL execution        |
| **BDD Testing**         | ReqnrollPlugin, Screenplay  | Behavior-driven development           |
| **Unit Testing**        | NUnit.Runtime, Core         | Component testing, mocking            |
| **Integration Testing** | All modules                 | End-to-end workflows                  |
| **Data Validation**     | All Validation.* modules    | Content verification, format checking |

## Development Guidelines

### 🛠️ Prerequisites

- **.NET 8.0 SDK** or later
- **JetBrains Rider** or **Visual Studio 2022** (recommended IDEs)
- **Git** for version control
- **NuGet** package manager

### 📝 Coding Standards

#### **Naming Conventions**

- **Classes**: PascalCase (`CustomerService`, `HttpRequestBuilder`)
- **Methods**: PascalCase (`PerformAs`, `AnsweredBy`)
- **Properties**: PascalCase (`Name`, `RequestUrl`)
- **Fields**: camelCase with underscore prefix for private (`_httpClient`, `_connectionString`)
- **Constants**: PascalCase (`MaxRetryCount`, `DefaultTimeout`)
- **Interfaces**: PascalCase with 'I' prefix (`IPerformable`, `IQuestion<T>`)

#### **Code Style**

- **File scoped namespaces** for new files
- **Primary constructors** where appropriate (C# 12 feature)
- **Expression-bodied members** for simple implementations
- **Nullable reference types** enabled
- **XML documentation** for public APIs

### 🧪 Testing Guidelines

#### **Test Structure**

- **Arrange-Act-Assert** pattern for unit tests
- **Given-When-Then** structure for BDD scenarios
- **Descriptive test names** that explain the scenario

#### **Test Categories**

- **Unit Tests**: Fast, isolated, test single components
- **Integration Tests**: Test component interactions
- **Acceptance Tests**: End-to-end business scenarios using Screenplay pattern

### 🔧 Development Workflow

#### **Branch Strategy**

1. **master**: Production-ready code
2. **develop**: Integration branch for features
3. **feature/**: Individual feature development
4. **bugfix/**: Bug fixes
5. **release/**: Release preparation

#### **Commit Guidelines**

- Use **conventional commits** format:
  ```
  type(scope): description

  feat(screenplay): add question chaining support
  fix(http): resolve timeout issue in HttpClient
  docs(readme): update installation instructions
  test(validation): add binary comparison tests
  ```

### 📦 Dependencies

#### **Core Dependencies**

- **NUnit 4.x**: Primary testing framework
- **NSubstitute/Moq**: Mocking library
- **Newtonsoft.Json**: JSON processing
- **System.Net.Http**: HTTP client operations

#### **Adding New Dependencies**

1. Evaluate necessity and alternatives
2. Check license compatibility
3. Add to appropriate project only
4. Update documentation

### 🚀 Building and Testing

#### **Local Development**

```console
# Restore packages
dotnet restore

# Build solution
dotnet build

# Run all tests
dotnet test

# Run specific test project
dotnet test Agenix.Screenplay.Tests
```

#### **IDE Configuration**

- **EditorConfig** settings are enforced
- **ReSharper/Rider** settings in `.sln.DotSettings`
- **License headers** automatically applied using templates

### 📋 Pull Request Guidelines

#### **Before Submitting**

- [ ] All tests pass locally
- [ ] Code follows established conventions
- [ ] XML documentation added for public APIs
- [ ] License headers present in new files
- [ ] Breaking changes documented

#### **PR Description Should Include**

- **What**: Brief description of changes
- **Why**: Reason for the change
- **How**: Technical approach used
- **Testing**: How the change was tested
- **Breaking Changes**: Any breaking changes

### 🐛 Issue Reporting

When reporting issues, please include:

- **Framework version**
- **Target framework** (.NET version)
- **Minimal reproduction case**
- **Expected vs actual behavior**
- **Environment details** (OS, IDE, etc.)

### 🔒 Security Considerations

- **No hardcoded credentials** in code or tests
- **Sensitive data** should use configuration or environment variables
- **Dependencies** regularly updated for security patches
- **Test data** should not contain real customer information

[1]: .assets/logos/agenix-logo-large.png "Agenix"

[2]: .assets/icons/icon-64.png "Agenix"

