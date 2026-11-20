# Quality

## Definition of done
- Acceptance criteria met and demonstrated (acceptance tests over real HTTP with seeded invented data).
- Unit tests for rules, integration tests for each adapter, one end-to-end test per key flow.
- CI green: build (warnings as errors), format, tests + coverage gate, web tests + build, secret scan, privacy guard, readiness.
- Docs in `.agentic/` updated in the same change.

## Service levels
| SLI | SLO | Why |
|---|---|---|
| Queue API success rate (`GET /api/queue`, non-4xx) | 99.9% over 30 days | analysts start every day here |
| p95 latency `GET /api/queue` (<= 500 open deals) | < 300 ms | scoring is in-process; template explainer is ~0 ms |
| Criteria freshness: time from saved edit to new `criteriaVersion` served | < 10 s | "re-ranks without a deploy" |
| Explanation fallback rate (LLM adapter only) | < 5% of explanations per day | guard rejects or timeouts mean the model is not earning its keep |
| Product outcome: top-decile deals reviewed within 24 h | 95% | from `product.md` |

## Test map (slice 1)
| Acceptance criterion | Tests |
|---|---|
| Deterministic order | `QueueAcceptanceTests.AC1_*`, `ScoringEngineTests.Same_weights_*`, `Equal_scores_*` |
| Reason names top three criteria | `QueueAcceptanceTests.AC2_*`, `ScoringEngineTests.Top_movers_*`, `ExplainerTests.*` |
| Weight change re-ranks without deploy | `QueueAcceptanceTests.AC3_*`, `An_invalid_weight_edit_*`, smoke test editing the criteria file |
