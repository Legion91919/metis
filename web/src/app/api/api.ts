import { HttpClient, HttpInterceptorFn } from '@angular/common/http';
import { Injectable, inject, signal } from '@angular/core';
import { Observable, tap } from 'rxjs';
import { CriteriaResponse, LoginResponse, QueueResponse } from './models';

/** Holds the access token in memory only (never localStorage). */
@Injectable({ providedIn: 'root' })
export class Session {
  readonly token = signal<string | null>(null);
  readonly email = signal<string | null>(null);
  signOut(): void { this.token.set(null); this.email.set(null); }
}

/** Adds the bearer token to API calls; a 401 elsewhere signs the user out. */
export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const token = inject(Session).token();
  return next(token ? req.clone({ setHeaders: { Authorization: `Bearer ${token}` } }) : req);
};

@Injectable({ providedIn: 'root' })
export class MetisApi {
  private readonly http = inject(HttpClient);
  private readonly session = inject(Session);

  login(email: string, password: string): Observable<LoginResponse> {
    return this.http.post<LoginResponse>('/auth/login', { email, password }).pipe(
      tap(r => { this.session.token.set(r.accessToken); this.session.email.set(email); }));
  }

  queue(take = 50): Observable<QueueResponse> { return this.http.get<QueueResponse>(`/api/queue?take=${take}`); }

  criteria(): Observable<CriteriaResponse> { return this.http.get<CriteriaResponse>('/api/criteria'); }
}
