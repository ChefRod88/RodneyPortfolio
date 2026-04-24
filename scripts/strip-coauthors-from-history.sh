#!/usr/bin/env bash
# Rewrite all existing commits to remove Co-authored-by: lines from messages.
# Requires: git filter-repo  (pip install git-filter-repo  or  package manager)
# After running:  git push --force-with-lease origin <your-branch>
set -euo pipefail
ROOT="$(cd "$(dirname "$0")/.." && pwd)"
cd "$ROOT"
STRIPPY="$ROOT/scripts/git-msg-strip-coauthor-lines.py"

if [[ ! -f "$STRIPPY" ]]; then
  echo "Missing $STRIPPY" >&2
  exit 1
fi
if ! command -v git-filter-repo &>/dev/null; then
  echo "git-filter-repo is not on PATH. Install, for example:" >&2
  echo "  pip install git-filter-repo" >&2
  echo "  or:  brew install git-filter-repo" >&2
  exit 1
fi

echo "This rewrites every commit in this clone to drop Co-authored-by: lines from messages."
echo "You must then:  git push --force-with-lease origin <branch>"
if [[ "${1:-}" != "-y" && "${1:-}" != "--yes" && "${STRIP_COAUTHORS_YES:-}" != "1" ]]; then
  read -r -p "Continue? [y/N] " ok || true
  if [[ "${ok:-}" != "y" && "${ok:-}" != "Y" ]]; then
    echo "Aborted."
    exit 0
  fi
fi

git filter-repo --force \
  --msg-filter "python3 \"$STRIPPY\""

echo "Done. Review with:  git log -1  and  git log --oneline -5"
echo "Then:  git push --force-with-lease origin [branch]"
