import { Component, inject, signal } from '@angular/core';
import { Session } from './api/api';
import { LoginCard } from './login-card';
import { QueuePage } from './queue/queue-page';

@Component({
  selector: 'app-root',
  imports: [LoginCard, QueuePage],
  template: `
    <header class="app-header">
      <span class="brand">
        <svg width="26" height="26" viewBox="0 0 26 26" aria-hidden="true"><rect width="26" height="26" rx="6" fill="#0B6B5D"/><rect x="5" y="6" width="16" height="3" rx="1.5" fill="#fff"/><rect x="5" y="11.5" width="11" height="3" rx="1.5" fill="#fff"/><rect x="5" y="17" width="6" height="3" rx="1.5" fill="#fff"/></svg>
        Metis
      </span>
      <span class="sub hide-sm">Harbourline Lending · origination</span>
      <span class="meta">
        @if (session.token()) {
          <span>Criteria {{ version() ? 'v' + version() : '…' }}</span>
          <span class="hide-sm">{{ session.email() }}</span>
          <button type="button" (click)="session.signOut()">Sign out</button>
        }
      </span>
    </header>
    <main>
      @if (session.token()) { <app-queue-page (version)="version.set($event)" /> } @else { <app-login-card /> }
    </main>
  `,
})
export class App {
  protected readonly session = inject(Session);
  protected readonly version = signal<string | null>(null);
}
