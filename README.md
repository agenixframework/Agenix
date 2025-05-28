# Agenix Framework

**Agenix** is a comprehensive test automation framework built with **C# .NET 8.0** that enables the creation of fully automated end-to-end use case tests for enterprise SOA applications. Designed with modern testing patterns and practices, Agenix provides a robust foundation for testing complex distributed systems, APIs, databases, and user workflows.

## Key Features

- **🎭 Screenplay Pattern**: Actor-based testing with fluent, readable test scenarios using Tasks, Questions, and Abilities
- **🌐 Multi-Protocol Support**: Built-in support for HTTP/REST APIs, SQL databases, and web services
- **🔧 Framework Integration**: Seamless integration with NUnit and Reqnroll (SpecFlow successor) for BDD testing
- **✅ Rich Validation**: Comprehensive validation capabilities for JSON, text, binary data, and custom formats
- **🏗️ Modular Architecture**: Clean separation of concerns with extensible, plugin-based design
- **📊 Advanced Reporting**: Detailed test execution reports with contextual information
- **🔄 Enterprise Ready**: Built for complex SOA environments with session management and context handling

## What Makes Agenix Different

Unlike traditional testing frameworks, Agenix focuses on **business-readable test automation** that bridges the gap between technical implementation and business requirements. The framework's Screenplay pattern allows you to write tests that read like natural language while maintaining the full power of programmatic test automation.


## Table of Contents

- [Overview](#overview)
- [Prerequisites](#prerequisites)
- [Getting Started](#getting-started)
- [Module overview](#module-overview)
- [Development Guidelines](#development-guidelines)
- [License](#license)

## Overview

Agenix.ATF is a comprehensive test framework built on .NET 8.0, designed to provide robust testing capabilities with modern testing patterns and practices.

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

| Module | Description | Key Features |
|--------|-------------|--------------|
| **Agenix.Api** | Core interfaces and contracts for the testing framework | • API definitions<br>• Interface contracts<br>• Common types<br>• Exception definitions |
| **Agenix.Core** | Main implementation of the framework logic | • Test execution engine<br>• Context management<br>• Action builders<br>• Session handling |

### 🌐 Protocol & Communication Modules

| Module | Description | Key Features |
|--------|-------------|--------------|
| **Agenix.Http** | HTTP client testing capabilities and utilities | • HTTP request/response handling<br>• REST API testing<br>• Web service integration<br>• HTTP validation |
| **Agenix.Sql** | Database testing and SQL execution utilities | • Database connectivity<br>• SQL query execution<br>• Data validation<br>• Transaction management |

### 🎭 Testing Pattern Implementations

| Module | Description | Key Features |
|--------|-------------|--------------|
| **Agenix.Screenplay** | Screenplay pattern implementation for readable tests | • Actor-based testing<br>• Task and Question abstractions<br>• Ability pattern<br>• Fluent test scenarios |

### 🔌 Framework Integration Modules

| Module | Description | Key Features |
|--------|-------------|--------------|
| **Agenix.NUnit.Runtime** | NUnit test framework integration | • NUnit test execution<br>• Custom attributes<br>• Test lifecycle management<br>• Result reporting |
| **Agenix.ReqnrollPlugin** | Reqnroll (SpecFlow successor) BDD integration | • Gherkin syntax support<br>• Step definitions<br>• Feature file parsing<br>• BDD test execution |

### ✅ Validation & Assertion Modules

| Module | Description | Key Features |
|--------|-------------|--------------|
| **Agenix.Validation.Binary** | Binary file and data validation | • Binary file comparison<br>• Byte array validation<br>• File format verification<br>• Checksum validation |
| **Agenix.Validation.Json** | JSON schema and content validation | • JSON schema validation<br>• Content comparison<br>• Path-based assertions<br>• Structure verification |
| **Agenix.Validation.NHamcrest** | Hamcrest-style matcher library | • Fluent assertion syntax<br>• Custom matchers<br>• Readable error messages<br>• Composable assertions |
| **Agenix.Validation.Text** | Text content and format validation | • String comparison<br>• Pattern matching<br>• Format validation<br>• Text transformation |

### 🧪 Test Project Modules

| Test Project | Purpose | Test Types |
|-------------|---------|------------|
| **Agenix.Core.NUnitTestProject** | Core framework functionality testing | • Unit tests<br>• Integration tests<br>• Framework behavior validation |
| **Agenix.Screenplay.Tests** | Screenplay pattern feature validation | • Actor behavior tests<br>• Task execution tests<br>• Question answering tests |
| **Agenix.ReqnrollPlugin.Tests** | BDD plugin functionality testing | • Step definition tests<br>• Feature parsing tests<br>• Plugin integration tests |
| **Agenix.Validation.Binary.Tests** | Binary validation module testing | • Binary comparison tests<br>• File validation tests |
| **Agenix.Validation.Json.Tests** | JSON validation module testing | • Schema validation tests<br>• JSON comparison tests |
| **Agenix.Validation.NHamcrest.Tests** | Hamcrest matcher testing | • Matcher behavior tests<br>• Assertion tests |
| **Agenix.Validation.Text.Tests** | Text validation module testing | • Text comparison tests<br>• Pattern matching tests |
| **Agenix.Acceptance.Test.API.Specs** | End-to-end API acceptance testing | • API workflow tests<br>• Full integration tests<br>• Acceptance criteria validation |

### 📋 Configuration & Support Files

| Component | Purpose | Features |
|-----------|---------|----------|
| **Solution Configuration** | Project organization and build settings | • Multi-project solution<br>• Shared build configuration<br>• Dependency management |
| **IDE Settings** | Development environment standardization | • Code style enforcement<br>• License header automation<br>• Shared formatting rules |
| **Templates** | Code generation and standardization | • License header templates<br>• File templates<br>• Coding standards |

### 🎯 Framework Capabilities

| Capability | Supported Modules | Use Cases |
|------------|------------------|-----------|
| **API Testing** | Http, Core, Validation.Json | REST API validation, HTTP workflows |
| **Database Testing** | Sql, Core, Validation.Text | Data validation, SQL execution |
| **BDD Testing** | ReqnrollPlugin, Screenplay | Behavior-driven development |
| **Unit Testing** | NUnit.Runtime, Core | Component testing, mocking |
| **Integration Testing** | All modules | End-to-end workflows |
| **Data Validation** | All Validation.* modules | Content verification, format checking |

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
1. **main**: Production-ready code
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

## License

MIT License

Copyright (c) 2025 Agenix

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
