import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from 'src/env/environment';
import { SavePublicKey } from './model/save-public-key.model';
import { PublicKeyResponse } from './model/public-key-response.model';
import { UserPublicKey } from './model/user-public-key.model';

@Injectable({
  providedIn: 'root',
})
export class KeyService {
  constructor(private http: HttpClient) {}

  /**
   * Čuva javni ključ na backend
   */
  savePublicKey(publicKeyPem: string): Observable<{ success: boolean; message: string }> {
    const body: SavePublicKey = { publicKeyPem };
    return this.http.post<{ success: boolean; message: string }>(
      `${environment.apiHost}key/save-public-key`,
      body
    );
  }

  /**
   * Preuzima sopstveni javni ključ
   */
  getPublicKey(): Observable<PublicKeyResponse> {
    return this.http.get<PublicKeyResponse>(`${environment.apiHost}key/public-key`);
  }

  /**
   * Preuzima javni ključ drugog korisnika (za deljenje lozinki)
   */
  getUserPublicKey(userId: number): Observable<UserPublicKey> {
    return this.http.get<UserPublicKey>(`${environment.apiHost}key/public-key/${userId}`);
  }
}
