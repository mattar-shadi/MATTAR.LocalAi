# MATTAR.LocalAi

[![CI - Build & Test](https://github.com/mattar-shadi/MATTAR.LocalAi/actions/workflows/ci.yml/badge.svg)](https://github.com/mattar-shadi/MATTAR.LocalAi/actions/workflows/ci.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

A .NET library for running AI chat and knowledge base (RAG) workloads **entirely locally**, with no cloud dependency. MATTAR.LocalAi uses local ONNX models for inference and local vector embeddings ([bge-micro-v2](https://huggingface.co/Xenova/bge-micro-v2)) for semantic search, making it ideal for privacy-conscious or air-gapped applications.

---

## Features

- **Fully local inference** — runs a quantized ONNX language model on-device, no API keys or internet connection required
- **Retrieval-Augmented Generation (RAG)** — built-in knowledge base that enriches answers with relevant documents before querying the model
- **Streaming chat** — token-by-token streaming via `IChat.Run` with an optional `Action<string>` callback
- **Vector search** — semantic similarity search backed by HNSW index and cosine distance (384-dimensional embeddings)
- **SQLite vector store** — lightweight, file-based persistence using `Microsoft.SemanticKernel.Connectors.SqliteVec`
- **Clean abstraction layer** — `MATTAR.LocalAI.Abstraction` defines pure interfaces (`IChat`, `IKnowledgeBase`, `IDocument`, …) so you can swap implementations or mock in tests
- **DI-friendly** — ships extension methods (`AddChatForMaui`, `AddChatAgent`, `AddChatSqliteMemory`) for `IServiceCollection`
- **Targets .NET 10**

---

## Project Structure

```
MATTAR.LocalAi.sln
├── src/
│   ├── MATTAR.LocalAI.Abstraction/   # Pure interfaces – no dependencies on runtime libraries
│   │   ├── IChat.cs                  # Streaming chat session interface
│   │   ├── IChatSettings.cs          # Chat settings (system prompt, …)
│   │   ├── IDocument.cs              # Document stored in the knowledge base
│   │   ├── IKnowledgeBase.cs         # Create/search a vector knowledge base
│   │   └── IKnowledgeSearchResult.cs # Search result model
│   │
│   ├── MATTAR.LocalAi/               # Core implementation
│   │   ├── Chat.cs                   # IChat via Semantic Kernel + ONNX GenAI (full-featured)
│   │   ├── ChatAgent.cs              # IChat via Microsoft.Agents.AI (lightweight agent)
│   │   ├── ChatSettings.cs           # IChatSettings implementation
│   │   ├── Document.cs               # IDocument with vector store annotations (HNSW, 384 dims)
│   │   ├── KnowledgeBase.cs          # IKnowledgeBase – upsert & semantic search
│   │   ├── KnowledgeSearchResult.cs  # IKnowledgeSearchResult implementation
│   │   └── Extensions/
│   │       └── ChatExtensions.cs     # IServiceCollection extension methods
│   │
│   └── MATTAR.LocalAiAgent/          # Agent project (in development)
│
├── samples/
│   ├── MATTAR.LocalAi.ConsoleSample/ # Interactive console chat demo
│   └── MATTAR.LocalAi.MauiBlazorHybrid/ # MAUI Blazor Hybrid UI demo
│
└── tests/
    └── MATTAR.LocalAi.Tests/         # Unit & integration tests
```

---

## Getting Started

### Prerequisites

| Requirement | Version |
|---|---|
| [.NET SDK](https://dotnet.microsoft.com/download) | 10.0 or later |
| ONNX chat model | Place a compatible INT4-quantized ONNX model (e.g. **Phi-4**) in `models/cpu-int4-rtn-block-32-acc-level-4/` relative to your output directory |

> **Note:** The `bge-micro-v2` embedding model is already bundled in `src/MATTAR.LocalAi/models/bge-micro-v2/` and is copied to the output directory automatically by the project build.

### Clone & Build

```bash
git clone https://github.com/mattar-shadi/MATTAR.LocalAi.git
cd MATTAR.LocalAi
dotnet restore
dotnet build --configuration Release
```

### Basic Usage

#### Chat only

```csharp
using MATTAR.LocalAi;
using MATTAR.LocalAi.Abstractions;
using MATTAR.LocalAi.Extensions;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();

services.AddSingleton<IChatSettings>(_ => new ChatSettings
{
    SystemPrompt = "You are a helpful assistant."
});

// Register Chat (Semantic Kernel backend) + SQLite vector store
services.AddScoped<IChat, Chat>();
services.AddChatSqliteMemory();

var sp = services.BuildServiceProvider();
var chat = sp.GetRequiredService<IChat>();

await chat.Run(
    userQ: "What is the capital of France?",
    action: token => Console.Write(token));
```

#### Chat with RAG (Knowledge Base)

```csharp
using MATTAR.LocalAi;
using MATTAR.LocalAi.Abstractions;
using MATTAR.LocalAi.Extensions;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();

services.AddSingleton<IChatSettings>(_ => new ChatSettings
{
    SystemPrompt = "You are a helpful assistant. Use the provided knowledge base to answer questions."
});

// Register Chat + KnowledgeBase + SQLite vector store
services.AddChatForMaui();

var sp = services.BuildServiceProvider();
var knowledgeBase = sp.GetRequiredService<IKnowledgeBase>();
var chat = sp.GetRequiredService<IChat>();

// Populate the knowledge base
var documents = new List<IDocument>
{
    new Document { Id = 1, Name = "Intro", Content = "MATTAR.LocalAi runs AI models locally." },
    new Document { Id = 2, Name = "Models", Content = "It supports ONNX models such as Phi-4." }
};
await knowledgeBase.CreateKnowledgeBase("my-kb", documents);

// Query with RAG context
await chat.Run(
    userQ: "Which models are supported?",
    knowledgeBaseName: "my-kb",
    action: token => Console.Write(token));
```

### Run the Console Sample

```bash
cd samples/MATTAR.LocalAi.ConsoleSample
dotnet run
```

---

## Architecture

```
┌─────────────────────────────────────────────┐
│              Your Application               │
│  (Console, MAUI, Blazor, …)                 │
└──────────────────┬──────────────────────────┘
                   │ depends on
┌──────────────────▼──────────────────────────┐
│      MATTAR.LocalAI.Abstraction             │
│   IChat  IKnowledgeBase  IDocument  …       │
└──────────┬──────────────────────────────────┘
           │ implemented by
┌──────────▼──────────────────────────────────┐
│           MATTAR.LocalAi (Core)             │
│                                             │
│  Chat / ChatAgent                           │
│    └─ OnnxRuntimeGenAI  (LLM inference)     │
│    └─ Semantic Kernel    (orchestration)    │
│                                             │
│  KnowledgeBase                              │
│    └─ bge-micro-v2 ONNX  (embeddings)       │
│    └─ SQLite + vec0      (vector store)     │
└─────────────────────────────────────────────┘
```

- **`IChat`** — entry point for chat. Call `Run(userQ, knowledgeBaseName?, action?)` to get a streaming response. When a `knowledgeBaseName` is supplied the implementation performs a semantic search first and injects the top results into the conversation context (RAG).
- **`IKnowledgeBase`** — manages collections of `IDocument`. `CreateKnowledgeBase` upserts documents; `Search` returns the most semantically similar results for a query using the embedded **bge-micro-v2** model.
- **`Chat` vs `ChatAgent`** — `Chat` uses Semantic Kernel for full orchestration (tool calling, chat history, plugins). `ChatAgent` uses the lighter `Microsoft.Agents.AI` `ChatClientAgent` for simpler scenarios.
- **Vector Store** — backed by SQLite with the `vec0` extension via `Microsoft.SemanticKernel.Connectors.SqliteVec`. The `AddChatSqliteMemory()` extension wires everything up automatically.

---

## Contributing

Contributions are welcome! Please open an issue to discuss a feature or bug before submitting a pull request.

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/my-feature`)
3. Commit your changes (`git commit -m 'Add my feature'`)
4. Push to the branch (`git push origin feature/my-feature`)
5. Open a pull request against `master`

---

## License

This project is licensed under the [MIT License](LICENSE).  
Copyright © 2024 Shadi Mattar