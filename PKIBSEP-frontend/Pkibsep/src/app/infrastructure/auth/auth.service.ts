import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable, tap } from 'rxjs';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { TokenStorage } from './jwt/token.service';
import { environment } from 'src/env/environment';
import { JwtHelperService } from '@auth0/angular-jwt';
import { Login } from './model/login.model';
import { AuthenticationResponse } from './model/authentication-response.model';
import { User } from './model/user.model';
import { Registration } from './model/registration.model';
import { RegisterResponse } from './model/register-response.model';
import { PasswordRecoveryRequest } from './model/password-recovery-request.model';
import { PasswordReset } from './model/password-reset.model';

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  user$ = new BehaviorSubject<User>({ email: '', id: 0, role: '' });

  constructor(private http: HttpClient, private tokenStorage: TokenStorage, private router: Router) {}

  login(login: Login): Observable<AuthenticationResponse> {
    return this.http.post<AuthenticationResponse>(environment.apiHost + 'authentication/login', login).pipe(
      tap((authenticationResponse) => {
        this.tokenStorage.saveAccessToken(authenticationResponse.accessToken);
        this.setUser();
      })
    );
  }

  register(registration: Registration): Observable<RegisterResponse> {
    return this.http
      .post<RegisterResponse>(environment.apiHost + 'user/register', registration);
  }

  confirmEmail(token: string): Observable<{success: boolean, message: string}> {
    return this.http
      .get<{success: boolean, message: string}>(environment.apiHost + 'user/confirm-email', {
        params: { token }
      });
  }

  requestPasswordRecovery(request: PasswordRecoveryRequest): Observable<{success: boolean, message: string}> {
    return this.http
      .post<{success: boolean, message: string}>(environment.apiHost + 'user/forgot-password', request);
  }

  resetPassword(passwordReset: PasswordReset): Observable<{success: boolean, message: string}> {
    return this.http
      .post<{success: boolean, message: string}>(environment.apiHost + 'user/reset-password', passwordReset);
  }

  logout(): void {
    this.router.navigate(['/home']).then((_) => {
      this.http.patch(`${environment.apiHost}sessions/revoke-current-session`, {}).subscribe({
        next: (_) => {
          this.tokenStorage.clear();
          this.user$.next({ email: '', id: 0, role: '' });
        },
        error: (err) => alert(err.message),
      });
    });
  }

  checkIfUserExists(): void {
    const accessToken = this.tokenStorage.getAccessToken();
    if (accessToken == null) {
      return;
    }
    this.setUser();
  }

  private setUser(): void {
    const jwtHelperService = new JwtHelperService();
    const accessToken = this.tokenStorage.getAccessToken() || '';
    const decoded = jwtHelperService.decodeToken(accessToken);

    const user: User = {
      id: +decoded.sub,
      email: decoded.email,
      role: decoded.role,
    };

    this.user$.next(user);
    console.log(user);
  }
}
