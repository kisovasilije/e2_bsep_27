import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { Session } from './models/session.model';
import { environment } from 'src/env/environment';

@Injectable({
  providedIn: 'root',
})
export class SessionService {
  private readonly http = inject(HttpClient);

  public getByUserId(userId: number): Observable<Session[]> {
    return this.http.get<Session[]>(`${environment.apiHost}sessions/${userId}`);
  }

  public revokeSession(id: number): Observable<void> {
    return this.http.patch<void>(`${environment.apiHost}sessions/revoke/${id}`, {});
  }
}
