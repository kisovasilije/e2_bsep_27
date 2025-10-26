import { Component, inject, OnInit } from '@angular/core';
import { Session } from '../models/session.model';
import { SessionService } from '../session.service';
import { User } from 'src/app/infrastructure/auth/model/user.model';
import { AuthService } from 'src/app/infrastructure/auth/auth.service';
import { MatSnackBar } from '@angular/material/snack-bar';

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

  ngOnInit(): void {
    this.initUser();
    this.initSessions();
  }

  private initUser(): void {
    this.authService.user$.subscribe((user) => (this.user = { ...user }));
  }

  private initSessions(): void {
    if (!this.user) {
      return;
    }

    this.sessionService.getByUserId(this.user.id).subscribe((sessions) => {
      this.sessions = sessions;
    });
  }

  protected revokeSession(id: number): void {
    this.sessionService.revokeSession(id).subscribe({
      next: (_) => {
        this.snackBar.open('Session revoked successfully.', 'Close', { duration: 3000 });
      },
      error: (err) => {},
    });
  }
}
