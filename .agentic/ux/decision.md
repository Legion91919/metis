# Decision: queue screen

Reviewed at 1440px and 390px (headless browser screenshots). Scores 1-5.

| Variant | Main job speed | Clarity of numbers | Accessibility | Fits tone | Total |
|---|---|---|---|---|---|
| A: dense table (`queue-a.html`) | 5 | 5 | 4 | 5 | 19 |
| B: summary cards (`queue-b.html`) | 3 | 3 | 4 | 4 | 14 |
| C: guided review (`queue-c.html`) | 3 | 4 | 5 | 4 | 16 |

**Pick: A, plus B's summary strip and C's paragraph.** Analysts compare deals side by side every morning, so the ranked table wins on speed. B's strip makes the product outcome visible ("top-decile deals waiting > 24h"). C's one-paragraph reason is shown when a row is expanded (rank 1 open by default), so every rank is explained without leaving the queue.

Rules carried into the build:
- Movers show a sign, an arrow and the criterion name: never colour alone.
- The "FCFS pos." column stays: it shows the cost of the old process at a glance.
- The criteria version is in the header so a screenshot always says which weights produced it.
- Error state hides the last view rather than showing stale ranks.

Final: `screens/queue.html` (state switcher: loading, empty, error, success).
