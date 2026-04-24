#!/usr/bin/env bash
# Rewrite all existing commits to remove Co-authored-by: lines from messages.
# Uses git filter-repo when it runs cleanly; otherwise falls back to git filter-branch
# (e.g. minimal Python missing gettext, which breaks some git-filter-repo installs).
# After running:  git push --force-with-lease origin <your-branch>
set -euo pipefail
ROOT="$(cd "$(dirname "$0")/.." && pwd)"
cd "$ROOT"
STRIPPY="$ROOT/scripts/git-msg-strip-coauthor-lines.py"

if [[ ! -f "$STRIPPY" ]]; then
  echo "Missing $STRIPPY" >&2
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

# Prefer filter-repo; fall back if binary missing or broken (e.g. missing gettext in Python)
use_filter_repo=false
if command -v git-filter-repo &>/dev/null && git filter-repo --version &>/dev/null; then
  use_filter_repo=true
fi

if $use_filter_repo; then
  git filter-repo --force --msg-filter "python3 \"$STRIPPY\""
else
  echo "Using git filter-branch (filter-repo not available or not runnable on this system)." >&2
  export FILTER_BRANCH_SQUELCH_WARNING=1
  # shellcheck disable=SC2016
  git filter-branch -f --msg-filter "python3 \"$STRIPPY\"" -- --all
fi

echo "Done. Review with:  git log -1  and  git log --oneline -5"
echo "Then:  git push --force-with-lease origin [branch]"
