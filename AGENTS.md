# AGENTS.md

Metis ranks Harbourline's inbound loan applications. Today they are reviewed first-come-first-served, so strong deals wait behind weak ones.

## What is live
Slice 1: `GET /api/queue` (ranked, top-three movers, one-paragraph reason), `GET /api/criteria`, `POST/GET /api/applications`, Blocks Auth at `/auth`, Angular queue in `web/`. Weights in `src/Metis.Api/criteria.json` reload live. Port 5202.

## Read first
Everything about this project lives in the hidden `.agentic/` folder.
Start with `.agentic/README.md`; it says which file answers which question.
Load only the files your task needs.

## Hard rules
- Work in thin vertical slices: one ticket from `.agentic/backlog.md` at a time.
- No change without a test that fails first. Run the full test suite before you finish.
- A decision that is hard to reverse gets an ADR in `.agentic/decisions/`.
- Never commit secrets, real client data or personal data. Use `.env.example`.
- Do not grow this file. New knowledge goes in the right `.agentic/` file; history goes in `.agentic/changelog.md`.
- When you learn something that would help on the next project, add it to `.agentic/lessons.md`.