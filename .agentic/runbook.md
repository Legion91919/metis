# Runbook

## Run locally
```bash
# API on http://localhost:5202 (Development seeds 10 invented applications and two demo users)
export DemoUsers__Password='pick-a-local-demo-value'   # optional; otherwise generated and logged once
ASPNETCORE_ENVIRONMENT=Development ASPNETCORE_URLS=http://localhost:5202 dotnet run --project src/Metis.Api

# Web on http://localhost:4202 (dev proxy sends /api, /auth, /health to :5202)
cd web && npm install && npm start
```
Sign in as `analyst@example.com` or `credit@example.com` with the demo password.

## Tests
```bash
dotnet test Metis.sln                     # CI / any machine with nuget.org access
cd web && npm test && npm run build       # Angular unit-test builder (Vitest + jsdom), production build
```
Offline workspace (no nuget.org): build each src project with `--configfile /home/claude/tools/offline-nuget.config` and run
`/home/claude/tools/run-xunit-offline.sh tests/Metis.Tests src/Metis.Api/Metis.Api.csproj <csproj referencing Agentic.Blocks.Testing>`.

## Packages
`Agentic.Blocks.*` are vendored in `eng/packages/` (ADR-0003); `nuget.config` maps them there and everything else to nuget.org.
To upgrade a block: drop the new `.nupkg` in `eng/packages/`, bump the version in the csproj files, delete the old file.

## Change lending criteria (no deploy)
Edit `criteria.json` (or the file named by `CriteriaFile`). The next request uses the new weights; the queue's `criteriaVersion`
changes. An invalid edit (unknown key, negative weight, all zero) is refused: the old criteria stay live, a warning is logged and
`GET /api/criteria` shows `lastReloadError`. In a hosted environment mount the file from a config map / app configuration.

## Deploy
Not yet (slice 2). Plan: container per environment, criteria file mounted, Data Protection keys persisted when running more than one instance.

## Monitor and alerts
- `/health/live` (process), `/health/ready` (dependencies). Log line "Criteria edit refused" = someone needs to fix `criteria.json`.

## Common incidents
| Symptom | Cause | Fix |
|---|---|---|
| API fails at start with `OptionsValidationException` | invalid `criteria.json` on first start | fix the file; message names the key |
| Queue ranks did not change after an edit | edit refused | `GET /api/criteria` → `lastReloadError` |
| 401 on every call after a restart | in-memory token store reset | sign in again (persist keys in slice 2) |
