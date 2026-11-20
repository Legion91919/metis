#!/usr/bin/env bash
# Runs every test suite in this repo, the same way CI does.
set -euo pipefail
cd "$(dirname "$0")/.."
echo "==> dotnet test Metis.sln -c Release --nologo"
dotnet test Metis.sln -c Release --nologo
echo "==> (cd web && npm test)"
(cd web && npm test)
echo "==> (cd web && npm run build)"
(cd web && npm run build)
echo 'All tests passed.'
