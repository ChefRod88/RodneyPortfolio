#!/usr/bin/env python3
import os
import re

def to_json(val):
    """
    Custom lightweight JSON stringifier to support Python installations lacking the 'json' module.
    """
    if isinstance(val, str):
        # Escape backslashes, quotes, and newlines
        escaped = val.replace('\\', '\\\\').replace('"', '\\"').replace('\n', '\\n').replace('\r', '\\r').replace('\t', '\\t')
        return f'"{escaped}"'
    elif isinstance(val, bool):
        return "true" if val else "false"
    elif isinstance(val, (int, float)):
        return str(val)
    elif isinstance(val, list):
        return "[" + ", ".join(to_json(x) for x in val) + "]"
    elif isinstance(val, dict):
        parts = []
        for k, v in val.items():
            parts.append(f"{to_json(k)}: {to_json(v)}")
        return "{" + ", ".join(parts) + "}"
    elif val is None:
        return "null"
    return "null"

def parse_csharp_file(file_path, content):
    """
    Heuristically extracts class/interface declarations and method definitions from a C# file.
    """
    classes = []
    methods = []
    
    # 1. Extract classes/interfaces/structs
    class_matches = re.finditer(r'\b(class|interface|struct)\s+(\w+)', content)
    for m in class_matches:
        classes.append({
            "type": m.group(1),
            "name": m.group(2)
        })
        
    # 2. Extract methods
    method_matches = re.finditer(
        r'\b(public|private|protected|internal|override|static)\s+(?:async\s+)?(?:[a-zA-Z0-9_<>\[\]]+\s+)+([a-zA-Z0-9_]+)\s*\([^)]*\)\s*[\{;]',
        content
    )
    for m in method_matches:
        method_name = m.group(2)
        if method_name not in ("if", "for", "foreach", "while", "switch", "using", "catch", "return"):
            methods.append(method_name)
            
    return classes, methods

def build_knowledge_graph(workspace_root):
    graph = {
        "nodes": {},
        "edges": []
    }
    
    exclude_dirs = {"bin", "obj", ".git", ".vscode", "wwwroot", "node_modules", ".gemini", "IcmWorkspace"}
    symbol_to_file = {}
    
    # First pass: Identify all files and scan for declared symbols (Nodes)
    for root, dirs, files in os.walk(workspace_root):
        dirs[:] = [d for d in dirs if d not in exclude_dirs]
        
        for file in files:
            if file.endswith((".cs", ".cshtml", ".razor")):
                abs_path = os.path.join(root, file)
                rel_path = os.path.relpath(abs_path, workspace_root)
                
                try:
                    with open(abs_path, "r", encoding="utf-8") as f:
                        content = f.read()
                except Exception:
                    continue
                
                classes, methods = parse_csharp_file(rel_path, content)
                
                node_id = rel_path
                graph["nodes"][node_id] = {
                    "id": node_id,
                    "type": "file",
                    "path": rel_path,
                    "classes": [c["name"] for c in classes],
                    "methods": methods
                }
                
                for c in classes:
                    symbol_to_file[c["name"]] = node_id
                    
    # Second pass: Scan for references (Edges)
    for node_id, node_data in graph["nodes"].items():
        abs_path = os.path.join(workspace_root, node_id)
        try:
            with open(abs_path, "r", encoding="utf-8") as f:
                content = f.read()
        except Exception:
            continue
            
        for symbol, target_node_id in symbol_to_file.items():
            if target_node_id != node_id:
                if re.search(r'\b' + re.escape(symbol) + r'\b', content):
                    graph["edges"].append({
                        "source": node_id,
                        "target": target_node_id,
                        "relation": "references",
                        "symbol": symbol
                    })
                    
    return graph

def main():
    workspace_root = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
    print(f"Indexing codebase at: {workspace_root}")
    
    graph = build_knowledge_graph(workspace_root)
    index_path = os.path.join(workspace_root, "index.json")
    
    json_string = to_json(graph)
    
    with open(index_path, "w", encoding="utf-8") as f:
        f.write(json_string)
        
    print(f"Index successfully built with {len(graph['nodes'])} nodes and {len(graph['edges'])} edges.")
    print(f"Saved to: {index_path}")

if __name__ == "__main__":
    main()
