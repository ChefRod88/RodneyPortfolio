import ragData from "./rag-index.json" with { type: "json" };

/**
 * Computes cosine similarity between two unit-normalized vectors.
 * Because vectors from OpenAI text-embedding-3-small are already L2 normalized,
 * cosine similarity is equal to their dot product.
 * @param {number[]} vecA
 * @param {number[]} vecB
 * @returns {number}
 */
export function cosineSimilarity(vecA, vecB) {
  if (!vecA || !vecB || vecA.length !== vecB.length) return 0;
  let dot = 0;
  for (let i = 0; i < vecA.length; i++) {
    dot += vecA[i] * vecB[i];
  }
  return dot;
}

/**
 * Fetches 512-dimension embedding for query using OpenAI text-embedding-3-small.
 * @param {string} query
 * @param {string} apiKey
 * @returns {Promise<number[]>}
 */
export async function getQueryEmbedding(query, apiKey) {
  const response = await fetch("https://api.openai.com/v1/embeddings", {
    method: "POST",
    headers: {
      "content-type": "application/json",
      "authorization": `Bearer ${apiKey}`,
    },
    body: JSON.stringify({
      model: "text-embedding-3-small",
      dimensions: 512,
      input: query,
    }),
  });

  if (!response.ok) {
    const errorText = await response.text();
    throw new Error(`OpenAI embedding failed (${response.status}): ${errorText}`);
  }

  const data = await response.json();
  if (!data?.data?.[0]?.embedding) {
    throw new Error("Invalid response format from OpenAI embeddings API");
  }

  return data.data[0].embedding;
}

/**
 * Searches the precomputed multi-document knowledge base for the most relevant chunks.
 * @param {number[]} queryVector
 * @param {number} topK
 * @param {number} minScore
 * @returns {Array<object>}
 */
export function retrieveRelevantChunks(queryVector, topK = 3, minScore = 0.35) {
  if (!queryVector || !Array.isArray(queryVector)) return [];
  const chunks = ragData.chunks || [];

  const scored = chunks.map((chunk) => ({
    ...chunk,
    score: cosineSimilarity(queryVector, chunk.embedding),
  }));

  scored.sort((a, b) => b.score - a.score);

  const filtered = scored.filter((c) => c.score >= minScore);
  return (filtered.length > 0 ? filtered : scored).slice(0, topK);
}

/**
 * Formats retrieved chunks into a structured context string for GPT prompt injection.
 * @param {Array<object>} chunks
 * @returns {string}
 */
export function formatRagContext(chunks) {
  if (!chunks || chunks.length === 0) {
    return "No specific documents matched above confidence threshold. Rely on Rodney's core profile.";
  }

  return chunks
    .map((c, i) => {
      return `[DOCUMENT ${i + 1}]: ${c.title}
Source: ${c.source} (${c.category})
URL: ${c.url}
Similarity Score: ${(c.score || 0).toFixed(4)}
Content:
${c.text}`;
    })
    .join("\n\n---\n\n");
}
