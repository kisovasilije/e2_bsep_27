import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from 'src/env/environment';
import { SavePassword } from './model/save-password.model';
import { PasswordEntry } from './model/password-entry.model';
import { UpdatePassword } from './model/update-password.model';

@Injectable({
  providedIn: 'root',
})
export class PasswordService {
  constructor(private http: HttpClient) {}

  /**
   * Preuzimanje svih lozinki korisnika
   */
  getPasswords(): Observable<PasswordEntry[]> {
    return this.http.get<PasswordEntry[]>(`${environment.apiHost}password`);
  }

  /**
   * Preuzimanje jedne lozinke po ID-u
   */
  getPasswordById(id: number): Observable<PasswordEntry> {
    return this.http.get<PasswordEntry>(`${environment.apiHost}password/${id}`);
  }

  /**
   * Kreiranje nove lozinke
   */
  createPassword(password: SavePassword): Observable<{ success: boolean; message: string; entry: PasswordEntry }> {
    return this.http.post<{ success: boolean; message: string; entry: PasswordEntry }>(
      `${environment.apiHost}password`,
      password
    );
  }

  /**
   * Ažuriranje lozinke
   */
  updatePassword(id: number, password: UpdatePassword): Observable<{ success: boolean; message: string }> {
    return this.http.put<{ success: boolean; message: string }>(
      `${environment.apiHost}password/${id}`,
      password
    );
  }

  /**
   * Brisanje lozinke
   */
  deletePassword(id: number): Observable<{ success: boolean; message: string }> {
    return this.http.delete<{ success: boolean; message: string }>(`${environment.apiHost}password/${id}`);
  }
}
