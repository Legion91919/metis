# Handover

What the client needs to own this without us.

- [ ] Source, infrastructure-as-code and pipelines in the client's own accounts
- [ ] Every secret rotated and owned by the client
- [ ] Architecture and decisions explained (`architecture.md`, `decisions/`)
- [ ] Runbook rehearsed by a client engineer
- [ ] Known issues and next steps listed below
- [ ] AI agent setup documented so their team can keep using it

## Known issues
- Storage is in memory (slice 2): restarts lose submitted applications and sign-ins.
- No "mark reviewed" action yet, so the queue is every application ever received.
- Demo users exist only in Development; production identity provider not chosen (see Agentic.Blocks.Auth README for the OIDC swap).

## Recommended next steps
- Slice 2 persistence, then PDF ingestion (backlog). Keep the explainer on the template until an eval set exists.