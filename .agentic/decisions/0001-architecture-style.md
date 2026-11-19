# 0001. Architecture style

- Status: Accepted
- Date: 2026-11-19

## Context

Harbourline's inbound loan applications are reviewed first-come-first-served, so strong deals wait behind weak ones. Traits: ComplexBusinessRules, DocumentIngestion, SmallTeam.

## Decision

Build a Clean Architecture rules engine for the score, with the model limited to writing the explanation.

## Alternatives considered

- Let the model score deals directly: fast to build, but nobody could explain or audit a rank.
- Separate scoring service: no independent scaling need yet.

## Consequences

- Clear boundaries that tests can enforce.
- Room to extract a part later if its load demands it.
