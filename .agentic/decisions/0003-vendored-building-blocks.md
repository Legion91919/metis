# 0003. Vendored building blocks

- Status: Accepted
- Date: 2026-11-20

## Context
Login, ProblemDetails, correlation ids, health checks and `Result`/`Money` are solved by the in-house `Agentic.Blocks.*`
packages (0.1.0). They are published to a private feed that CI and the client may not have access to.

## Decision
Vendor the used packages (`Core`, `Web`, `Auth`, `Testing`) as `.nupkg` files in `eng/packages/` and add a repo `nuget.config`
with two sources: `eng/packages` (mapped to `Agentic.Blocks.*` only) and nuget.org (everything else, e.g. xUnit).

## Alternatives considered
- Private feed: needs credentials in CI and at the client; breaks offline builds.
- Copy the source into the repo: easier to patch, but forks the blocks and loses their tests and versioning.

## Consequences
- The repo restores anywhere without secrets. Upgrading a block = replace the `.nupkg` and bump the version.
- About 90 KB of binaries in git (marked binary in `.gitattributes`).
