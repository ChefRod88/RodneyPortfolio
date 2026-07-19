#!/bin/bash
set -e

# Scans HTML and CSS files in the given directory (default: dist)
# and checks if local references (src, href, url) exist.
TARGET_DIR="${1:-dist}"

echo "Running Asset Validation on $TARGET_DIR..."

if [ ! -d "$TARGET_DIR" ]; then
  echo "Error: Directory $TARGET_DIR does not exist."
  exit 1
fi

missing_assets=0
temp_file=$(mktemp)

# Find all HTML and CSS files and extract references
find "$TARGET_DIR" -type f \( -name "*.html" -o -name "*.css" \) -print0 | while IFS= read -r -d '' file; do
  grep -Eo '(src|href)="[^"]+"|url\([^)]+\)' "$file" | grep -v -E 'http|mailto:|tel:' | while read -r match; do
    
    ref=$(echo "$match" | sed -E 's/.*="([^"]+)".*/\1/' | sed -E 's/.*url\(([^)]+)\).*/\1/' | tr -d "'" | tr -d '"')

    if [[ "$ref" == data:* ]] || [[ "$ref" == \#* ]] || [[ "$ref" == ~* ]] || [[ -z "$ref" ]]; then
      continue
    fi

    # Clean query strings and hashes
    clean_ref=$(echo "$ref" | cut -d'?' -f1 | cut -d'#' -f1)

    if [[ "$ref" == /* ]]; then
      physical_path="${TARGET_DIR}${clean_ref}"
    else
      dir=$(dirname "$file")
      physical_path="${dir}/${clean_ref}"
    fi

    if [ ! -f "$physical_path" ]; then
      if [ ! -f "${physical_path}/index.html" ] && [ ! -f "${physical_path}.html" ]; then
          echo "::error file=$file::Missing local asset: $ref (Expected: $physical_path)"
          echo "1" > "$temp_file"
      fi
    fi
  done
done

if grep -q "1" "$temp_file"; then
  rm -f "$temp_file"
  echo "Asset validation failed due to missing files."
  exit 1
else
  rm -f "$temp_file"
  echo "All local assets validated successfully!"
fi
