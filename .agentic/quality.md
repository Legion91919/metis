# Quality

## Definition of done
- Acceptance criteria met and demonstrated.
- Unit tests for rules, integration tests for each adapter, one end-to-end test per key flow.
- CI green: build, tests, lint, dependency and secret scan.
- Docs in `.agentic/` updated in the same change.

## Service levels
| SLI | SLO |
|---|---|
| API success rate | 99.9% over 30 days |
| p95 API latency | < 300 ms |