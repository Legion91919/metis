import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { QueuePage } from './queue-page';
import { QueueResponse } from '../api/models';

const response: QueueResponse = {
  criteriaVersion: '988f5b88', asOf: '2026-11-20T09:00:00Z', total: 2,
  items: [
    { rank: 1, applicationId: 'a', dealName: 'Pier 7 Cold Store', sponsor: 'Northwake Estates', assetType: 'Industrial', region: 'Harbourside',
      loanAmount: 4200000, currency: 'GBP', loanToValue: 0.5385, debtServiceCover: 1.62, score: 95.8,
      movers: [{ key: 'LoanToValue', criterion: 'Loan-to-value', evidence: '53.8%', points: 15 },
               { key: 'DebtServiceCover', criterion: 'Debt service cover', evidence: '1.62x', points: 10 },
               { key: 'SponsorTrackRecord', criterion: 'Sponsor track record', evidence: '10 completed deals', points: 8.3 }],
      explanation: 'Pier 7 Cold Store ranks #1 of 2 with a score of 95.8 out of 100.', waitingHours: 30, arrivalPosition: 2 },
    { rank: 2, applicationId: 'b', dealName: 'Linden Row Apartments', sponsor: 'Fernbridge Homes', assetType: 'Multifamily', region: 'Millbrook',
      loanAmount: 9500000, currency: 'GBP', loanToValue: 0.72, debtServiceCover: 1.31, score: 52.2,
      movers: [{ key: 'LoanToValue', criterion: 'Loan-to-value', evidence: '72.0%', points: -5.4 }],
      explanation: 'Linden Row Apartments ranks #2.', waitingHours: 52, arrivalPosition: 1 },
  ],
};

describe('QueuePage', () => {
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({ imports: [QueuePage], providers: [provideHttpClient(), provideHttpClientTesting()] });
    http = TestBed.inject(HttpTestingController);
  });

  async function render(flush: (req: ReturnType<HttpTestingController['expectOne']>) => void) {
    const fixture = TestBed.createComponent(QueuePage);
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelector('[aria-busy="true"]')).not.toBeNull(); // loading state first
    flush(http.expectOne('/api/queue?take=50'));
    await fixture.whenStable();
    fixture.detectChanges();
    return fixture.nativeElement as HTMLElement;
  }

  it('shows ranked rows with signed movers and the first reason expanded', async () => {
    const el = await render(r => r.flush(response));
    const rows = el.querySelectorAll('tbody tr:first-child');
    expect(rows.length).toBe(2);
    expect(rows[0].textContent).toContain('Pier 7 Cold Store');
    expect(rows[0].textContent).toContain('▲ +15.0');
    expect(rows[1].textContent).toContain('▼ -5.4');
    expect(el.querySelector('tr.reason')?.textContent).toContain('ranks #1 of 2');
    expect(el.querySelector('.strip')?.textContent).toContain('52h');
  });

  it('shows the empty state when nothing is open', async () => {
    const el = await render(r => r.flush({ ...response, total: 0, items: [] }));
    expect(el.textContent).toContain('No open applications');
  });

  it('shows the error state with a retry that reloads', async () => {
    const el = await render(r => r.flush('down', { status: 503, statusText: 'Unavailable' }));
    expect(el.querySelector('[role="alert"]')?.textContent).toContain('could not be loaded');
    (el.querySelector('[role="alert"] button') as HTMLButtonElement).click();
    http.expectOne('/api/queue?take=50').flush(response);
  });

  afterEach(() => http.verify());
});
