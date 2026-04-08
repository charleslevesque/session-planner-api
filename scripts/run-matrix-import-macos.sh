#!/usr/bin/env bash
# Run the SessionPlanner matrix import PowerShell script on macOS.
# Prerequisites:
#   1. API running: dotnet run --project src/SessionPlanner.Api  (default http://localhost:5290)
#   2. PowerShell:   brew install powershell
#   3. Python (for SQLite InstallCommand updates inside the .ps1):
#      brew install python
#      Ensure `python` is on PATH (or `python3` — see note below).
#
# Usage:
#   ./scripts/run-matrix-import-macos.sh
#   MATRIX_IMPORT_PS1=/path/to/import-matrix-updated.ps1 ./scripts/run-matrix-import-macos.sh
#   ./scripts/run-matrix-import-macos.sh -ApiUrl "http://localhost:5290/api/v1"

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"
DB_PATH="${DB_PATH:-$REPO_ROOT/src/SessionPlanner.Api/SessionPlanner.db}"
PS1_FILE="${MATRIX_IMPORT_PS1:-$HOME/Downloads/import-matrix-updated.ps1}"

if ! command -v pwsh >/dev/null 2>&1; then
  echo "PowerShell (pwsh) not found. Install with:"
  echo "  brew install powershell"
  exit 1
fi

if [[ ! -f "$PS1_FILE" ]]; then
  echo "Import script not found: $PS1_FILE"
  echo "Set MATRIX_IMPORT_PS1 to the full path of import-matrix-updated.ps1"
  exit 1
fi

if [[ ! -f "$DB_PATH" ]]; then
  echo "SQLite database not found: $DB_PATH"
  echo "Run the API once (migrations create the DB) or set DB_PATH."
  exit 1
fi

# The .ps1 updates InstallCommand via Python and looks for `py` or `python` (not `python3`).
# If you see "Python not found; cannot update SQLite packages", run: brew install python
# and ensure `python --version` works, or on some setups: ln -s "$(which python3)" ~/bin/python

echo "Repo:    $REPO_ROOT"
echo "DB:      $DB_PATH"
echo "Script:  $PS1_FILE"
echo ""

# The import .ps1 uses Join-Path $env:TEMP for temp files; pwsh on macOS leaves TEMP unset unless exported.
export TEMP="${TEMP:-/tmp}"
export TMPDIR="${TMPDIR:-/tmp}"

exec pwsh -NoProfile -File "$PS1_FILE" -DbPath "$DB_PATH" "$@"
