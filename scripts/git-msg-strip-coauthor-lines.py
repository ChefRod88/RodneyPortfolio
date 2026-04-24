#!/usr/bin/env python3
"""
Remove Co-authored-by: lines from a Git commit message (stdin -> stdout).
If stripping would remove the entire message, the original is restored.
Used by githooks/commit-msg and by git filter-repo --msg-filter.
"""
import sys


def main() -> None:
    text = sys.stdin.read()
    if not text:
        return
    out_lines: list[str] = []
    for line in text.splitlines(keepends=True):
        # Compare without trailing newline for the co-authored test
        bare = line.rstrip("\r\n")
        if bare.lstrip().lower().startswith("co-authored-by:"):
            continue
        out_lines.append(line)
    out = "".join(out_lines)
    if not out.strip():
        out = text
    sys.stdout.write(out)


if __name__ == "__main__":
    main()
