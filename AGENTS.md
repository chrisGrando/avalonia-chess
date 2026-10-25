# AGENTS.md

This document establishes the operational rules, architectural standards, and security guidelines that must be strictly followed by AI agents and developers when analyzing, refactoring, or generating code for this repository.

---

## 1. Environment Overview

* **Target Framework:** .NET 10 (`net10.0`)
* **Language:** C# 14
* **UI Framework:** Avalonia UI v12.1
* **Architectural Pattern:** MVVM (Model-View-ViewModel) using `CommunityToolkit.Mvvm` or `ReactiveUI`

---

## 2. .NET 10 & Avalonia UI 12.1 Development Rules

### 2.1. C# 14 Language and Conventions
* **Modern Syntax:** Use *Primary Constructors*, *File-Scoped Namespaces*, advanced *Pattern Matching*, and *Record* types for immutable DTOs.
* **Performance and Allocations:** Prefer `Span<T>`, `ReadOnlySpan<T>`, and `Memory<T>` in data manipulation operations to reduce Garbage Collector pressure.
* **Asynchronous Programming:** Follow the `async`/`await` pattern. Return `ValueTask` or `ValueTask<T>` in critical execution paths that frequently complete synchronously.

### 2.2. Architecture and Patterns with Avalonia UI 12.1
* **Strict Separation (MVVM):** 
  * The UI layer must be declared strictly via XAML or Avalonia v12.1 C# Markup.
  * Keep Code-Behind (`*.axaml.cs`) clean, containing only component initialization (`InitializeComponent()`) and view-bound logic.
* **UI Concurrency and Threading:**
  * Any UI updates originating from background tasks must be dispatched to the main Avalonia UI thread via `Dispatcher.UIThread.InvokeAsync()`.
* **UI Memory Management:**
  * Unsubscribe from events and dispose of observables (`IDisposable`) when closing Windows or *UserControls*.
  * Ensure visual components and bindings are compatible with *Trimming* and *AOT compilation*.

---

## 3. Essential Security Guidelines for AI Agents

Any code modification or generation proposed by an AI agent must strictly adhere to the following security pillars:

### 3.1. Prompt Injection Mitigation and Data Sanitization
* **Boundary Validation:** Never trust data received from external APIs, user text input, or AI model responses. Create strongly validated DTOs using libraries such as `FluentValidation`.
* **Instruction Isolation:** Keep *System Prompts* strictly separate from user inputs. AI agents must never generate or concatenate C# code or scripts to be dynamically interpreted at runtime (e.g., dynamically compiled `System.Linq.Expressions` or `Roslyn Scripting` with unsanitizing input).

### 3.2. Process Execution and File System
* **Command Injection Prevention:** Spawning system shells (`Process.Start`, `cmd.exe`, `bash`) using string concatenation with user- or agent-provided data is strictly prohibited.
* **File I/O Control:** Restrict read and write operations to the application's secure directory. Always validate and sanitize paths using `Path.GetFullPath` and check against *Path Traversal* attacks (`../`).

### 3.3. Credentials and Secrets Management
* **Zero Hardcoding:** Never hardcode API keys, passwords, connection strings, or certificates in `.cs`, `.axaml`, `.json`, or configuration files.
* **Secure Providers:** Use `appsettings.json` overridden by environment variables, `UserSecrets` during development, or *Azure Key Vault* / *HashiCorp Vault* for production environments.
* **Version Control Sanitization:** Ensure files containing secrets or local configurations are added to `.gitignore`.

### 3.4. Data Access and Persistence
* **Parameterized Queries:** All database interactions (EF Core 10, Dapper, or ADO.NET) must use parameterized queries.
* **SQL Concatenation:** Direct string concatenation in SQL commands or search filters is strictly forbidden.

### 3.5. Logging and Sensitive Data Protection (PII)
* **Masking and Log Level:** Never log credentials, authentication tokens, credit card data, or Personally Identifiable Information (PII) via `ILogger`.
* **Exception Handling:** Capture and handle exceptions securely. Do not expose full stack traces or internal infrastructure details in the Avalonia UI.

---

## 4. Checklist for Agent Changes

Before finalizing any modification or opening a Pull Request, the AI agent must verify:

1. The project compiles without errors under **.NET 10** (`dotnet build`).
2. All **Avalonia UI 12.1** Views adhere to the MVVM pattern and dispatch visual updates on the correct thread (`Dispatcher.UIThread`).
3. No credentials or secrets were introduced into the source code.
4. All external inputs are validated and protected against *SQL Injection*, *Command Injection*, and *Path Traversal*.
5. Necessary unit tests covering the new behavior have been created or updated.
