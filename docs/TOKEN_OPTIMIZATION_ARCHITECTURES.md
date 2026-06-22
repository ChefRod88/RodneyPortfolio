# Knowledge Graph vs. RTK (Rust Tool Kit) for Token Optimization

When optimization systems attempt to make AI agents work with large codebases, they generally use one of two architectures: Retrieval-Based Code Search (often powered by Rust-based indexing tools, abbreviated as "RTK" or Rust Tool Kit) vs. Structural Knowledge Graphs.

This comparison explains why the Knowledge Graph system performs significantly better for token optimization and speed:

---

## 1. How RTK / Retrieval-Based Indexing Works
- **The System:** RTK splits your codebase files into text "chunks" and indexes them using vector embeddings or keyword search.
- **The Query:** When you ask a question, the agent searches for semantically similar chunks (e.g., searching for "Urine drug screen") and inserts those chunks into the AI's prompt context.
- **The Flaw:** It lacks structural context. It might fetch the UI code for the urine screen, but miss the backend service that processes the save operation because they use different words. To compensate, retrieval tools must load large files and surrounding contexts. This leads to:
  - **High Token Waste:** You are copying hundreds of lines of code into the context just to find a class relationship.
  - **Fragmented Understanding:** The AI has to guess the connections between pieces of retrieved code.

---

## 2. How the Knowledge Graph System Works
- **The System:** A parser scans the code ahead of time to build a relational graph. It registers files, classes, methods, and components as **Nodes**, and inheritances, calls, and references as **Edges**.
- **The Query:** When the AI agent wants to modify code, it calls the MCP server to find structural neighbors (e.g., "Tell me all files referencing 'IPatientUrineScreenService'").
- **The Edge:** The MCP server returns a small list of node names and file paths instead of raw code.
- **The Benefits:**
  - **Extreme Token Savings:** The AI receives a list of exact file paths and dependencies (a few hundred characters) rather than raw file contents (thousands of lines of code).
  - **Zero Guesswork:** The AI has an explicit map of the codebase architecture, preventing it from making assumptions about how classes connect.
  - **Speed:** The agent's thinking process is cut from minutes to seconds because it doesn't have to read multiple files to figure out dependencies.

---

## Summary of Differences

| Feature | RTK / Retrieval-Based Indexing | Knowledge Graph System |
| :--- | :--- | :--- |
| **Analogy** | Like giving the AI a Google Search bar for your codebase. It reads pages of search results to find the answer. | Like giving the AI an interactive directory map. It knows the exact room and bookshelf where the target information is stored. |
| **Token Usage** | High waste (loads raw file contents and surrounding chunks). | Low waste (loads file names, paths, and relationships). |
| **Context Clarity** | AI guesses class/method connections. | AI gets explicit node-edge dependency map. |
| **Execution Speed** | Slower (minutes of context reading). | Extremely fast (seconds of graph traversal). |

By combining the structural map (`index.json`) with behavioral rules (`AGENTS.md`), the Antigravity CLI operates with maximum efficiency, keeping token usage to an absolute minimum.
