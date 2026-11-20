# Backlog

Vertical slices, smallest valuable first.

## Now
- [x] Slice 1: Origination sees today's queue ranked by criteria score, with a one-paragraph reason per deal
  - Given criteria weights in config, when deals are scored, then the order is deterministic
  - Given a deal, then the reason names the top three criteria that moved its score
  - Given a change to a weight, then the queue re-ranks without a deploy
  - Test notes: one acceptance test per criterion; seeded invented data.

## Next
- [x] Sketch leg: tokens + three variants of the slice-1 screen (`sketch-ui`)
- [ ] CI green with all gates before the first feature commit

## Later
- [ ] Slice 2: persistence (database adapter for Intake behind `IApplicationReader`), persisted Data Protection keys, deploy pipeline
- [ ] Slice 3: PDF pack ingestion worker (queue + extraction + human review of low-confidence fields, per ADR-0001 patterns)
- [ ] Slice 4: LLM explainer adapter behind `IDealExplainer`, with eval set and fallback-rate SLO
- [ ] Mark a deal reviewed (removes it from the open queue); audit of criteria edits
- [ ] Playwright e2e for flow 1 with axe
