#!/usr/bin/env node
const fs = require('fs');
const path = require('path');
const readline = require('readline');

const workspaceRoot = path.dirname(__dirname);
const indexPath = path.join(workspaceRoot, 'index.json');

// Read the index.json database
let graph = { nodes: {}, edges: [] };
try {
  if (fs.existsSync(indexPath)) {
    graph = JSON.parse(fs.readFileSync(indexPath, 'utf8'));
  }
} catch (err) {
  console.error("MCP Server Error: Failed to read index.json", err);
}

const rl = readline.createInterface({
  input: process.stdin,
  output: process.stdout,
  terminal: false
});

// JSON-RPC processing loop
rl.on('line', (line) => {
  if (!line.trim()) return;
  try {
    const request = JSON.parse(line);
    const response = handleRequest(request);
    if (response) {
      process.stdout.write(JSON.stringify(response) + '\n');
    }
  } catch (err) {
    const errorResponse = {
      jsonrpc: '2.0',
      id: null,
      error: { code: -32700, message: 'Parse error: ' + err.message }
    };
    process.stdout.write(JSON.stringify(errorResponse) + '\n');
  }
});

function handleRequest(req) {
  const method = req.method;
  const id = req.id;

  if (method === 'initialize') {
    return {
      jsonrpc: '2.0',
      id: id,
      result: {
        protocolVersion: req.params?.protocolVersion || '2024-11-05',
        capabilities: {
          tools: {}
        },
        serverInfo: {
          name: 'medical-app-knowledge-graph',
          version: '1.0.0'
        }
      }
    };
  }

  if (method === 'tools/list') {
    return {
      jsonrpc: '2.0',
      id: id,
      result: {
        tools: [
          {
            name: 'search_symbols',
            description: 'Finds files, classes, or methods whose names match the search query.',
            inputSchema: {
              type: 'object',
              properties: {
                query: {
                  type: 'string',
                  description: 'The search query matching class names, method names, or file paths.'
                }
              },
              required: ['query']
            }
          },
          {
            name: 'get_symbol_relations',
            description: 'Gets relationships, references, and dependencies for a specific file or symbol node.',
            inputSchema: {
              type: 'object',
              properties: {
                id: {
                  type: 'string',
                  description: 'The node ID (file path, e.g. "Controllers/ChatController.cs") to inspect.'
                }
              },
              required: ['id']
            }
          }
        ]
      }
    };
  }

  if (method === 'tools/call') {
    const toolName = req.params?.name;
    const args = req.params?.arguments || {};
    
    let textResult = '';
    
    if (toolName === 'search_symbols') {
      const query = (args.query || '').toLowerCase();
      const matches = [];
      
      for (const nodeId in graph.nodes) {
        const node = graph.nodes[nodeId];
        const matchFile = node.path.toLowerCase().includes(query);
        const matchClass = node.classes.some(c => c.toLowerCase().includes(query));
        const matchMethod = node.methods.some(m => m.toLowerCase().includes(query));
        
        if (matchFile || matchClass || matchMethod) {
          matches.push({
            id: node.id,
            path: node.path,
            classes: node.classes,
            methods: node.methods
          });
        }
      }
      
      textResult = matches.length > 0 
        ? JSON.stringify(matches, null, 2)
        : `No matching symbols found for query "${args.query}"`;
    } 
    else if (toolName === 'get_symbol_relations') {
      const nodeId = args.id;
      const node = graph.nodes[nodeId];
      
      if (!node) {
        textResult = `Node not found: "${nodeId}"`;
      } else {
        // Find outgoing edges (dependencies)
        const dependencies = graph.edges.filter(e => e.source === nodeId);
        // Find incoming edges (referencers)
        const references = graph.edges.filter(e => e.target === nodeId);
        
        const resultPayload = {
          node: node,
          dependencies: dependencies.map(e => ({ file: e.target, symbol: e.symbol })),
          references: references.map(e => ({ file: e.source, symbol: e.symbol }))
        };
        
        textResult = JSON.stringify(resultPayload, null, 2);
      }
    } else {
      return {
        jsonrpc: '2.0',
        id: id,
        error: { code: -32601, message: `Method not found: ${toolName}` }
      };
    }
    
    return {
      jsonrpc: '2.0',
      id: id,
      result: {
        content: [
          {
            type: 'text',
            text: textResult
          }
        ]
      }
    };
  }

  // Stand-in responses for notifications or unsupported methods
  if (id === undefined) return null;
  
  return {
    jsonrpc: '2.0',
    id: id,
    error: { code: -32601, message: 'Method not found' }
  };
}
