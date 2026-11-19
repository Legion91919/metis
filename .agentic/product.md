# Product brief

> Harbourline Lending is a fictional company. All names, documents and numbers in this repo are invented.

## Problem
Harbourline's inbound loan applications are reviewed first-come-first-served, so strong deals wait behind weak ones.

## Users
Origination team (daily); head of credit (sets criteria weights).

## Outcomes
Top-decile deals reviewed within 24 hours; every rank explainable in one paragraph.

## Constraints
Ranking must be explainable; no unexplained model scores.

## Out of scope
Anything not needed for the outcomes above in the first three slices.

## Traits (for the architecture advisor)
- ComplexBusinessRules: "lending criteria: asset type, LTV, location, sponsor track record, each weighted"
- DocumentIngestion: "applications arrive with PDF packs that need key figures pulled out"
- SmallTeam: "one or two engineers"
