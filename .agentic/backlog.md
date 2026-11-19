# Backlog

Vertical slices, smallest valuable first.

## Now
- [ ] Slice 1: Origination sees today's queue ranked by criteria score, with a one-paragraph reason per deal
  - Given criteria weights in config, when deals are scored, then the order is deterministic
  - Given a deal, then the reason names the top three criteria that moved its score
  - Given a change to a weight, then the queue re-ranks without a deploy
  - Test notes: one acceptance test per criterion; seeded invented data.

## Next
- [ ] Sketch leg: tokens + three variants of the slice-1 screen (`sketch-ui`)
- [ ] CI green with all gates before the first feature commit

## Later
