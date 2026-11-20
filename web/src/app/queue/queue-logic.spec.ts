import { QueueItem } from '../api/models';
import { money, moverLabel, signedPoints, summarise, viewStateFor } from './queue-logic';

const item = (rank: number, waitingHours: number): QueueItem => ({
  rank, applicationId: `id-${rank}`, dealName: `Deal ${rank}`, sponsor: 'Invented Sponsor', assetType: 'Industrial', region: 'Harbourside',
  loanAmount: 1_000_000, currency: 'GBP', loanToValue: 0.6, debtServiceCover: 1.4, score: 100 - rank, movers: [], explanation: '',
  waitingHours, arrivalPosition: rank,
});

describe('queue logic', () => {
  it('flags top-decile deals waiting more than 24 hours', () => {
    const items = Array.from({ length: 20 }, (_, i) => item(i + 1, i === 0 ? 30 : i === 1 ? 20 : 50));
    const s = summarise(items);
    expect(s.open).toBe(20);
    expect(s.topDecileWaiting.map(i => i.rank)).toEqual([1]); // rank 2 is top decile but waited only 20h
    expect(s.oldest?.rank).toBe(3);
  });

  it('always shows a sign on points', () => {
    expect(signedPoints(15)).toBe('+15.0');
    expect(signedPoints(-5.44)).toBe('-5.4');
    expect(signedPoints(0)).toBe('0.0');
    expect(moverLabel({ key: 'LoanToValue', criterion: 'Loan-to-value', evidence: '72.0%', points: -5.4 }))
      .toBe('Lowered by -5.4 points: Loan-to-value, 72.0%');
  });

  it('maps data to the four view states', () => {
    expect(viewStateFor(null, false)).toBe('loading');
    expect(viewStateFor([], false)).toBe('empty');
    expect(viewStateFor([item(1, 1)], false)).toBe('success');
    expect(viewStateFor([item(1, 1)], true)).toBe('error');
  });

  it('formats money compactly in the deal currency', () => {
    expect(money(4_200_000, 'GBP')).toMatch(/^£4\.2/);
  });
});
