# Components (queue screen)

Build order for the frontend. Every component handles loading, empty, error and success where it shows data.

| Component | Props / inputs | States |
|---|---|---|
| `AppHeader` | `criteriaVersion: string \| null`, `asOf: string \| null`, `user: string \| null`, `logout()` | loading: version shows "…"; success: version + time |
| `LoginCard` | `submit(email, password)`, `error: string \| null`, `busy: boolean` | idle, busy, error (401 / lockout message), success (navigates to queue) |
| `QueueSummary` | `items: QueueItem[]`, `weights: Weight[]` | loading skeleton; empty hidden; success: open count, top decile waiting > 24h, oldest |
| `QueueTable` | `items: QueueItem[]`, `expandedId`, `toggle(id)` | loading skeleton rows; empty panel; error panel with Retry; success rows |
| `MoverChip` | `mover: { criterion, evidence, points }` | single state; sign + arrow + text, colour is secondary |
| `ScoreBar` | `score: number (0-100)` | single state; `role="img"` with aria-label "Score 95.8 of 100" |
| `ReasonRow` | `explanation: string` | hidden / shown (`aria-expanded` on the deal button) |
| `WeightsPanel` | `criteria: { key, name, weight }[]`, `version` | loading, error, success |

`QueueItem` = `{ rank, applicationId, dealName, sponsor, assetType, region, loanAmount, currency, loanToValue, debtServiceCover, score, movers[3], explanation, waitingHours, arrivalPosition }` (matches `GET /api/queue`).
