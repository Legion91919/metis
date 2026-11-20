# 0002. Vertical slices around a pure, config-weighted rules engine

- Status: Accepted
- Date: 2026-11-20
- Refines: ADR-0001 (keeps its judgement: rules decide the score, a model may only write the explanation)

## Context
The advisor recommended Clean Architecture (ComplexBusinessRules, DocumentIngestion, SmallTeam) and listed a vertical slice
monolith as "Possible". Metis is one business area with a handful of features (submit, queue, criteria, later document
extraction and review actions) built by one or two engineers, often with AI agents. The rules that must be right are a
single scoring function, not a deep domain model.

## Decision
- Two modules in one deployable: **Intake** (owns applications) and **Triage** (scores, ranks, explains). Triage reads Intake
  only through the public `Metis.Intake.Contracts` namespace (`IApplicationReader`, `ApplicationSnapshot`).
- Inside each module, **one folder per feature** (`Features/<Feature>/`) holding its Request, Validator, Handler and Endpoint.
  Slices never reference each other; shared code lives outside `Features/` (`Scoring/`, `Explanation/`, `Data/`, `Shared/`).
- The **rules engine** (`Triage/Scoring`) is pure and deterministic: decimal arithmetic, canonical criterion order, total
  ordering (score, arrival, id). Weights come from `criteria.json` via `IOptionsMonitor`, so a weight change re-ranks on the
  next request without a deploy. Invalid edits are refused and the last good criteria stay live.
- The model is limited to the **`IDealExplainer` port**. It receives only already-decided facts and its output passes a guard
  (one paragraph, <= 800 chars, names every top mover) or the template adapter is used.
- Architecture tests enforce: no slice-to-slice references, every slice has the four parts, Triage uses only Intake contracts,
  Intake never references Triage, internals open only to the test project.

## Alternatives considered
- **Clean Architecture projects per layer (Domain/Application/Infrastructure/Api):** strongest isolation of rules, but four
  projects and a mapping layer for a scoring function we can already test without HTTP or storage. Revisit if the domain grows
  real aggregates (credit committee workflow, conditions, covenants).
- **Model scores deals directly:** rejected in ADR-0001; nobody could explain or audit a rank.
- **Weights in a database table with an admin UI:** needs audit and authorisation work now; config file + version hash is
  enough for slice 1. Becomes slice 3 if the head of credit wants self-service.

## Consequences
- A change usually touches one folder; agents extend features by copying a slice.
- Shared rules must stay in `Scoring/`, or slices start duplicating them (watched in review and by the slice test).
- Every rank carries a `criteriaVersion` (SHA-256 prefix of the criteria), so screenshots and logs are traceable.
