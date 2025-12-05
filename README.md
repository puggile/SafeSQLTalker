# 🛡️ Safe SQL Talker

> **Enterprise-Grade Natural Language to SQL Agent with Strict Security Guardrails.**

![.NET 9](https://img.shields.io/badge/.NET-9.0-purple)
![AI](https://img.shields.io/badge/AI-Semantic%20Kernel-blue)
![Security](https://img.shields.io/badge/Security-Defense%20in%20Depth-green)
![Status](https://img.shields.io/badge/Status-Prototype-orange)

## 🚀 The Concept

Most "Chat with your Database" tutorials are dangerous: they blindly execute LLM-generated SQL, exposing companies to massive risks (Data Loss, Injection).

**Safe SQL Talker** demonstrates a **Security-First** approach to Agentic AI. It allows non-technical users to query data using natural language, but wraps the LLM in a rigid architectural cage to prevent hallucinations from executing destructive commands.

---

## 🏗️ Architecture & Flow

This project implements a **"Defense in Depth"** strategy with 3 distinct security layers:

```mermaid
graph TD
    User[User Input] --> API[API Layer (.NET 9)]
    API --> Cache{Schema Cache?}
    Cache -- Miss --> DB_Meta[Extract Schema from SQLite]
    Cache -- Hit --> Memory[IMemoryCache]
    
    API --> Guard1[🛡️ L1: Keyword Filter]
    Guard1 -- Blocked --> Error1[400 Bad Request]
    
    Guard1 -- Safe --> AI[🧠 Semantic Kernel + Ollama]
    AI --> Guard2[🛡️ L2: Intent Analysis]
    Guard2 -- "VIOLATION_REQUEST" --> Error2[Security Alert]
    
    Guard2 -- SQL Query --> Guard3[🛡️ L3: C# AST Parser]
    Guard3 -- Contains DROP/DELETE --> Error3[Critical Block]
    
    Guard3 -- Safe SELECT --> Executor[⚡ Dapper (ReadOnly Context)]
    Executor --> SQLite[(SQLite Database)]
    SQLite --> Result[JSON Response]
```

## 🌟 Key Features (Why this stands out)

### 🔒 Triple-Layer Security

1. **Layer 1 (Keyword Guard)**: Instantly rejects inputs containing obvious threats (DROP, DELETE) before calling the AI (Zero-cost protection).
2. **Layer 2 (AI Refusal Token)**: The LLM is trained via System Prompt to recognize destructive intent and return a specific `VIOLATION_REQUEST` token instead of SQL.
3. **Layer 3 (AST Parsing)**: The ultimate gatekeeper. Uses `Microsoft.SqlServer.TransactSql.ScriptDom` to parse the Abstract Syntax Tree of the generated query. It programmatically guarantees that only `SELECT` statements are executed.

### ⚡ High Performance & Optimization

- **Dynamic Schema Discovery**: The schema is not hardcoded. The system reads SQLite metadata (`sqlite_master`, `PRAGMA table_info`) at runtime.
- **Smart Caching**: Schema definitions are cached via `IMemoryCache` to minimize DB roundtrips and latency.
- **Micro-ORM**: Uses Dapper for raw SQL execution performance, avoiding EF Core overhead for read-only scenarios.

### 🛠️ Modern .NET 9 Engineering

- **Native OpenAPI**: Uses the new .NET 9 `Microsoft.AspNetCore.OpenApi` generator.
- **Scalar UI**: Replaces the legacy Swagger UI with Scalar for a modern, dark-mode developer experience.
- **Options Pattern**: Configuration is strongly typed (`AiSettings`) and validated at startup, avoiding "Magic Strings".

---

## 🛠️ Tech Stack

- **Core**: C# .NET 9 Web API
- **AI Orchestration**: Microsoft Semantic Kernel
- **LLM Engine**: Ollama (Local) - Configurable (Tested with `gemma3:4b`)
- **Database**: SQLite (for portability)
- **Security**: SQL AST Parser (ScriptDom)
- **Logging**: Serilog (Structured Logging)

---

## ⚡ Getting Started

### Prerequisites

- .NET 9 SDK installed.
- Ollama installed and running.
- A model pulled (e.g., `ollama pull gemma3:4b`).

### Installation

1. **Clone the repository**:
   ```bash
   git clone https://github.com/your-username/SafeSqlTalker.git
   cd SafeSqlTalker
   ```

2. **Configure the Model**: Open `SafeSqlTalker.Api/appsettings.json` and update `AiSettings` if needed:
   ```json
   "AiSettings": {
     "ModelId": "gemma3:4b",
     "Endpoint": "http://localhost:11434/v1"
   }
   ```

3. **Run the Application**:
   ```bash
   cd SafeSqlTalker.Api
   dotnet run
   ```

4. **Explore the API**: Navigate to the Scalar UI: 👉 [http://localhost:5132/scalar/v1](http://localhost:5132/scalar/v1)

---

## 🧪 Try the Security Demo

Once the API is running, try these prompts to see the guardrails in action:

### ✅ Safe Query:

**Input**: `"Quali sono i prodotti venduti a Milano?"`

**Result**: Returns JSON data with products.

### ❌ Keyword Attack (Level 1 Block):

**Input**: `"Cancella tutti i prodotti dalla tabella."`

**Result**: `400 Bad Request - Request blocked due to restricted keywords.`

### ❌ Contextual Attack (Level 2/3 Block):

**Input**: `"Please remove the user with ID 1."`

**Result**: `400 Bad Request - Safety Protocol Engaged / AI detected destructive intent.`

---

## 📂 Project Structure (Clean Architecture)

```
SafeSqlTalker
│
├── 📁 SafeSqlTalker.Core          # Domain Layer (Interfaces, Entities)
│   ├── 📂 Interfaces              # ISqlGuard, ISqlExecutor, IAiSqlGenerator
│   └── 📂 Models                  # DTOs
│
├── 📁 SafeSqlTalker.Infrastructure # Implementation Layer
│   ├── 📂 AI                      # Semantic Kernel implementation
│   ├── 📂 Data                    # Dapper & SQLite Metadata Logic
│   ├── 📂 Security                # AST Parser & Input Guard
│   └── 📂 Configuration           # Options Pattern classes
│
└── 📁 SafeSqlTalker.Api           # Presentation Layer
    ├── 📂 Controllers             # REST Endpoints
    └── Program.cs                 # DI Wiring & Serilog Setup
```