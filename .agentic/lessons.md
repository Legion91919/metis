# Lessons

Generic, reusable lessons only. No client names, data or secrets.

- Validate reloadable options in your own provider, not with `IValidateOptions`: `OptionsMonitor` re-reads options inside the reload callback, so a bad edit throws there instead of being refused. Keep the last good value and report the error.
- Give every config-driven result a short hash of the config that produced it; it makes "why did this rank change?" answerable from a screenshot.
- Architecture tests for vertical slices are cheap: namespace `*.Features.X` must not reference `*.Features.Y`, and each slice folder must contain Request/Validator/Handler/Endpoint.
