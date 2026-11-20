# User flows

## Flow 1: review today's queue (slice 1)
1. Analyst signs in: `screens/queue.html` login card -> `POST /auth/login` (Agentic.Blocks.Auth).
2. Queue loads: `screens/queue.html` (loading state) -> `GET /api/queue` (Triage / GetQueue slice).
3. Analyst scans the ranked table: rank, score, three signed movers, waiting time, FCFS position (success state). No open deals -> empty state; API down -> error state with Retry.
4. Analyst expands a row to read the one-paragraph reason (rendered by the explainer port, template adapter today).
5. Analyst opens "Weights in use" -> `GET /api/criteria` (Triage / GetCriteria slice) to see the weights and the criteria version stamped on the queue.

## Flow 2: head of credit changes a weight (slice 1, no deploy)
1. Head of credit edits `criteria.json` (or the mounted config map in a hosted environment).
2. The API reloads it (IOptionsMonitor); an invalid edit is rejected and the last good weights stay live.
3. The next `GET /api/queue` returns a new `criteriaVersion` and the re-ranked order; the queue screen shows the new version in its header.

## Flow 3: a new application arrives (slice 1 API, UI in slice 2)
1. Intake system posts `POST /api/applications` with a client-generated `id` (Intake / SubmitApplication slice). Retrying with the same id and body is safe; a different body is a 409.
2. The deal appears in the next queue read, scored with the current weights.
