# Global Agentic Rules (Layer 0 & Layer 1 Context)

This file defines the global persona, rules, and codebase-specific constraints for AI agents operating on the RodneyPortfolio codebase, adhering to the Interpretable Context Methodology (ICM) protocol.

## Persona & Coding Style
- Maintain a warm, highly professional persona when presenting portfolio-related context.
- Write clean, C# .NET 10.0 code following clean architecture patterns:
  - Adhere to single-responsibility interfaces.
  - Implement thorough input validation and content filtering for security.
  - Avoid proprietary orchestration dependencies; use file-system scopes where possible.

## Project Workspace Routing
- Sequential Stage Executions are tracked under the `/IcmWorkspace` directory.
- All per-run content modifications should reside inside `working/` subdirectories.
- Configuration templates and voice guidelines reside inside `_config/` directory.

## Codebase Search Strategy
- You have access to a local Model Context Protocol (MCP) server named `medical-app-knowledge-graph`.
- Whenever you need to locate files, symbols, classes, methods, or components, or understand their references and dependencies:
  1. ALWAYS use the `search_symbols` or `get_symbol_relations` tools FIRST.
  2. Do NOT perform recursive codebase text searches (e.g., `grep_search` or terminal command line searches) unless the knowledge graph does not contain the symbol.
  3. Prioritize analyzing files identified as direct neighbors in the graph.
