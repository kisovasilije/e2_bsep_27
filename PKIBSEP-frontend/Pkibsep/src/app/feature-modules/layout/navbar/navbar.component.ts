import { Component, inject, OnInit } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { AuthService } from 'src/app/infrastructure/auth/auth.service';
import { User } from 'src/app/infrastructure/auth/model/user.model';
import { CsrDialogComponent } from '../../certificates/csr-dialog/csr-dialog.component';

@Component({
  selector: 'xp-navbar',
  templateUrl: './navbar.component.html',
  styleUrls: ['./navbar.component.css'],
})
export class NavbarComponent implements OnInit {
  user: User | undefined;

  private readonly dialog = inject(MatDialog);

  private readonly authService = inject(AuthService);

  ngOnInit(): void {
    this.authService.user$.subscribe((user) => {
      this.user = user;
    });
  }

  onLogout(): void {
    this.authService.logout();
  }

  protected onGenerateCsr(): void {
    this.dialog.open(CsrDialogComponent, {
      width: '35%',
      disableClose: true,
      autoFocus: false,
    });
  }
}
