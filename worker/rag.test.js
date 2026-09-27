import { describe, it, expect } from "vitest";
import { cosineSimilarity, retrieveRelevantChunks, formatRagContext } from "./rag.js";

describe("Vector RAG Math & Retrieval", () => {
  it("computes cosine similarity accurately for normalized vectors", () => {
    // Identical unit vectors
    const v1 = [1, 0, 0];
    const v2 = [1, 0, 0];
    expect(cosineSimilarity(v1, v2)).toBeCloseTo(1.0);

    // Orthogonal unit vectors
    const v3 = [0, 1, 0];
    expect(cosineSimilarity(v1, v3)).toBeCloseTo(0.0);

    // Opposite vectors
    const v4 = [-1, 0, 0];
    expect(cosineSimilarity(v1, v4)).toBeCloseTo(-1.0);
  });

  it("handles null or mismatched vector lengths safely", () => {
    expect(cosineSimilarity(null, [1, 2, 3])).toBe(0);
    expect(cosineSimilarity([1, 2], [1, 2, 3])).toBe(0);
  });

  it("retrieves top matching chunks from precomputed knowledge base", () => {
    // Generate a mock query vector of length 512
    const mockQueryVector = new Array(512).fill(0.04419);
    const results = retrieveRelevantChunks(mockQueryVector, 3, -1.0);

    expect(results).toBeDefined();
    expect(results.length).toBeLessThanOrEqual(3);
    expect(results[0]).toHaveProperty("id");
    expect(results[0]).toHaveProperty("title");
    expect(results[0]).toHaveProperty("url");
    expect(results[0]).toHaveProperty("score");

    // Ensure results are sorted descending by score
    if (results.length > 1) {
      expect(results[0].score).toBeGreaterThanOrEqual(results[1].score);
    }
  });

  it("formats RAG context with clear document markers and URLs", () => {
    const mockChunks = [
      {
        id: "chunk-1",
        title: "Test Article",
        source: "Tech Blog",
        category: "Articles",
        url: "/Articles/test",
        score: 0.8521,
        text: "This is test content.",
      },
    ];

    const context = formatRagContext(mockChunks);
    expect(context).toContain("[DOCUMENT 1]: Test Article");
    expect(context).toContain("Source: Tech Blog (Articles)");
    expect(context).toContain("URL: /Articles/test");
    expect(context).toContain("Similarity Score: 0.8521");
    expect(context).toContain("This is test content.");
  });

  it("returns fallback message when no chunks are retrieved", () => {
    const emptyContext = formatRagContext([]);
    expect(emptyContext).toContain("No specific documents matched");
  });
});
