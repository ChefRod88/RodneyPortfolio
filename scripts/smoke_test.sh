#!/bin/bash
set -e

TARGET_DIR="${1:-dist}"
INDEX_FILE="$TARGET_DIR/index.html"

echo "Running Smoke Test on $INDEX_FILE..."

if [ ! -f "$INDEX_FILE" ]; then
  echo "::error::Index file $INDEX_FILE does not exist!"
  exit 1
fi

echo "Checking for 'Enterprise Software Engineer'..."
if ! grep -qi "Enterprise Software Engineer" "$INDEX_FILE"; then
  echo "::error::Did not find 'Enterprise Software Engineer' text in index.html"
  exit 1
fi

echo "Checking for WebP profile image reference..."
if ! grep -qiE "149A60A7-E1B9-45FA-A196-804C7DB392C7.*\.webp" "$INDEX_FILE"; then
  echo "::error::Did not find the WebP profile image reference in index.html"
  exit 1
fi

echo "Checking for semantic elements (e.g., <nav>, <footer>)..."
if ! grep -qi "<nav" "$INDEX_FILE" || ! grep -qi "<footer" "$INDEX_FILE"; then
  echo "::error::Did not find semantic tags in index.html"
  exit 1
fi

echo "Smoke test passed successfully!"
