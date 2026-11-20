import { Mover, QueueItem } from '../api/models';

export type ViewState = 'loading' | 'empty' | 'error' | 'success';

export interface QueueSummary { open: number; topDecileWaiting: QueueItem[]; oldest: QueueItem | null; }

/** Top decile = the best-ranked 10% (at least one deal). The product outcome: these are reviewed within 24 hours. */
export function summarise(items: QueueItem[], total = items.length): QueueSummary {
  const cutoff = Math.max(1, Math.ceil(total * 0.1));
  const topDecileWaiting = items.filter(i => i.rank <= cutoff && i.waitingHours > 24);
  const oldest = items.reduce<QueueItem | null>((o, i) => (o === null || i.waitingHours > o.waitingHours ? i : o), null);
  return { open: total, topDecileWaiting, oldest };
}

/** "+15.0" / "-5.4": the sign always carries meaning, colour is only a second signal. */
export function signedPoints(points: number): string {
  const fixed = Math.abs(points).toFixed(1);
  return points > 0 ? `+${fixed}` : points < 0 ? `-${fixed}` : fixed;
}

export function moverLabel(m: Mover): string {
  return `${m.points >= 0 ? 'Raised' : 'Lowered'} by ${signedPoints(m.points)} points: ${m.criterion}, ${m.evidence}`;
}

export function money(amount: number, currency: string): string {
  return new Intl.NumberFormat('en-GB', { style: 'currency', currency, notation: 'compact', maximumFractionDigits: 2 }).format(amount);
}

export function viewStateFor(items: QueueItem[] | null, failed: boolean): ViewState {
  if (failed) return 'error';
  if (items === null) return 'loading';
  return items.length === 0 ? 'empty' : 'success';
}
