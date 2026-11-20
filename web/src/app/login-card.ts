import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpErrorResponse } from '@angular/common/http';
import { MetisApi } from './api/api';

@Component({
  selector: 'app-login-card',
  imports: [FormsModule],
  template: `
    <form class="login" (ngSubmit)="submit()" aria-labelledby="login-title">
      <h1 id="login-title">Sign in</h1>
      <p class="muted">Development demo users: analyst&#64;example.com or credit&#64;example.com (password from DemoUsers__Password).</p>
      <label for="email">Email</label>
      <input id="email" name="email" type="email" autocomplete="username" required [(ngModel)]="email" />
      <label for="password">Password</label>
      <input id="password" name="password" type="password" autocomplete="current-password" required [(ngModel)]="password" />
      @if (error()) { <p class="error" role="alert">{{ error() }}</p> }
      <button class="primary" type="submit" [disabled]="busy()">{{ busy() ? 'Signing in…' : 'Sign in' }}</button>
    </form>
  `,
})
export class LoginCard {
  private readonly api = inject(MetisApi);
  protected email = 'analyst@example.com';
  protected password = '';
  protected readonly busy = signal(false);
  protected readonly error = signal<string | null>(null);

  submit(): void {
    this.busy.set(true);
    this.error.set(null);
    this.api.login(this.email, this.password).subscribe({
      next: () => this.busy.set(false),
      error: (e: HttpErrorResponse) => {
        this.busy.set(false);
        this.error.set(e.status === 401 ? 'Email or password is wrong, or the account is locked for a few minutes.' : 'Sign-in is unavailable. Try again shortly.');
      },
    });
  }
}
