import { Component, inject, OnInit } from '@angular/core';
import { Session } from '../models/session.model';
import { SessionService } from '../session.service';
import { User } from 'src/app/infrastructure/auth/model/user.model';
import { AuthService } from 'src/app/infrastructure/auth/auth.service';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatDialog } from '@angular/material/dialog';
import { CertificatesPreviewComponent } from '../../certificates/certificates-preview/certificates-preview.component';

@Component({
  selector: 'xp-profile',
  templateUrl: './profile.component.html',
  styleUrls: ['./profile.component.css'],
})
export class ProfileComponent implements OnInit {
  protected user: User | undefined;

  protected sessions: Session[] = [];

  private readonly authService = inject(AuthService);
  private readonly sessionService = inject(SessionService);

  private readonly snackBar = inject(MatSnackBar);

  private readonly dialog = inject(MatDialog);

  ngOnInit(): void {
    this.initUser();
    this.initSessions();
  }

  private initUser(): void {
    this.authService.user$.subscribe(user => (this.user = { ...user }));
  }

  private initSessions(): void {
    if (!this.user) {
      return;
    }

    this.sessionService.getByUserId(this.user.id).subscribe(sessions => {
      this.sessions = sessions;
    });
  }

  protected revokeSession(id: number): void {
    this.sessionService.revokeSession(id).subscribe({
      next: _ => {
        this.sessions = this.sessions.filter(s => s.id !== id);
        this.snackBar.open('Session revoked successfully.', 'Close', { duration: 3000 });
      },
      error: _ => {
        this.snackBar.open(`Failed to revoke session`, 'Close', { duration: 3000 });
      },
    });
  }

  protected revokeAllSessions(): void {
    this.sessionService.revokeAllSessions().subscribe({
      next: _ => {
        this.sessions = this.sessions.filter(s => s.isThisSession);
        this.snackBar.open('All other sessions revoked successfully.', 'Close', { duration: 3000 });
      },
      error: _ => {
        this.snackBar.open(`Failed to revoke all sessions`, 'Close', { duration: 3000 });
      },
    });
  }

  protected requestPasswordRecovery(): void {
    if (!this.user?.email) {
      this.snackBar.open('User email not found', 'Close', { duration: 3000 });
      return;
    }

    this.authService.requestPasswordRecovery({ email: this.user.email }).subscribe({
      next: response => {
        this.snackBar.open(response.message || 'Password recovery email sent. Please check your inbox.', 'Close', { duration: 5000 });
      },
      error: err => {
        this.snackBar.open(err.error?.message || 'Failed to send password recovery email', 'Close', { duration: 3000 });
      },
    });
  }

  protected openMyCertificates(): void {
    this.dialog.open(CertificatesPreviewComponent, {
      width: '60%',
      disableClose: false,
      autoFocus: false,
    });
  }
}
