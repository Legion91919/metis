import { Component, OnInit, computed, inject, output, signal } from '@angular/core';
import { MetisApi } from '../api/api';
import { CriteriaResponse, QueueResponse } from '../api/models';
import { MoverChip } from './mover-chip';
import { money, summarise, viewStateFor } from './queue-logic';

@Component({
  selector: 'app-queue-page',
  imports: [MoverChip],
  template: `
    <div class="queue-head">
      <h1>Today's queue <span class="muted">· ranked by criteria score, ties by arrival</span></h1>
      <button type="button" class="refresh" (click)="load()" [disabled]="state() === 'loading'"
        [attr.aria-busy]="state() === 'loading'">{{ state() === 'loading' ? 'Refreshing…' : 'Refresh' }}</button>
    </div>
    @switch (state()) {
      @case ('loading') {
        <section class="panel" style="text-align:left" aria-busy="true">
          <div class="sk" style="width:40%"></div><div class="sk"></div><div class="sk"></div>
          <span class="muted">Scoring today's applications…</span>
        </section>
      }
      @case ('empty') {
        <section class="panel"><h2>No open applications</h2>
          <p class="muted">New applications appear here as soon as intake receives them.</p></section>
      }
      @case ('error') {
        <section class="panel err" role="alert"><h2>The queue could not be loaded</h2>
          <p class="muted">The last view is hidden so nobody acts on a stale rank.</p>
          <button class="primary" (click)="load()">Retry</button></section>
      }
      @case ('success') {
        <section class="strip" aria-label="Summary">
          <div class="kpi"><b>{{ summary().open }}</b>open</div>
          <div class="kpi"><b>{{ summary().topDecileWaiting.length }}</b>top-decile deal(s) waiting &gt; 24h</div>
          @if (summary().oldest; as o) { <div class="kpi"><b>{{ o.waitingHours }}h</b>oldest (ranked #{{ o.rank }})</div> }
          <div class="kpi"><button class="link" (click)="toggleWeights()" [attr.aria-expanded]="weightsOpen()">Weights in use</button>
            @if (weightsOpen() && criteria(); as c) {
              <span class="muted"> @for (w of c.criteria; track w.key) { {{ w.name }} {{ w.weight }}@if (!$last) { · } }</span>
              @if (c.lastReloadError) { <span class="down"> · last edit refused</span> }
            }
          </div>
        </section>
        <table class="queue">
          <thead><tr><th class="num">#</th><th>Deal</th><th class="num hide-sm">Loan</th><th class="num">Score</th>
            <th>Top three movers</th><th class="num hide-sm">Waiting</th><th class="num hide-sm">FCFS pos.</th></tr></thead>
          @for (item of data()!.items; track item.applicationId) {
            <tbody>
              <tr>
                <td class="num">{{ item.rank }}</td>
                <td><button class="link" (click)="toggle(item.applicationId)" [attr.aria-expanded]="expanded() === item.applicationId"
                    [attr.aria-controls]="'r-' + item.applicationId">{{ item.dealName }}</button>
                  <div class="muted" style="font-size:var(--font-size-xs)">{{ item.sponsor }} · {{ item.assetType }} · {{ item.region }}</div></td>
                <td class="num hide-sm">{{ fmt(item.loanAmount, item.currency) }}</td>
                <td class="num"><strong>{{ item.score.toFixed(1) }}</strong>
                  <span class="bar" role="img" [attr.aria-label]="'Score ' + item.score + ' of 100'"><i [style.width.%]="item.score"></i></span></td>
                <td>@for (m of item.movers; track m.key) { <app-mover-chip [mover]="m" /> }</td>
                <td class="num hide-sm">{{ item.waitingHours }}h</td>
                <td class="num hide-sm">{{ item.arrivalPosition }}</td>
              </tr>
              @if (expanded() === item.applicationId) {
                <tr class="reason" [id]="'r-' + item.applicationId"><td></td><td colspan="6">{{ item.explanation }}</td></tr>
              }
            </tbody>
          }
        </table>
      }
    }
  `,
})
export class QueuePage implements OnInit {
  private readonly api = inject(MetisApi);
  readonly version = output<string>();
  protected readonly data = signal<QueueResponse | null>(null);
  protected readonly failed = signal(false);
  protected readonly expanded = signal<string | null>(null);
  protected readonly weightsOpen = signal(false);
  protected readonly criteria = signal<CriteriaResponse | null>(null);
  readonly state = computed(() => viewStateFor(this.data()?.items ?? null, this.failed()));
  readonly summary = computed(() => summarise(this.data()?.items ?? [], this.data()?.total ?? 0));
  protected readonly fmt = money;

  ngOnInit(): void { this.load(); }

  load(): void {
    this.failed.set(false);
    this.data.set(null);
    this.api.queue().subscribe({
      next: q => {
        this.data.set(q); this.expanded.set(q.items[0]?.applicationId ?? null); this.version.emit(q.criteriaVersion);
        if (this.weightsOpen()) this.loadCriteria(); // keep the weights shown in step with the ranks
      },
      error: () => this.failed.set(true),
    });
  }

  toggle(id: string): void { this.expanded.update(e => (e === id ? null : id)); }

  toggleWeights(): void {
    this.weightsOpen.update(o => !o);
    if (this.weightsOpen()) this.loadCriteria();
  }
  private loadCriteria(): void {
    this.api.criteria().subscribe({ next: c => this.criteria.set(c), error: () => this.criteria.set(null) });
  }
}
