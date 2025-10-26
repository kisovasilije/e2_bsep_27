import { Component, OnInit } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { PasswordService } from 'src/app/infrastructure/password-manager/password.service';
import { KeyService } from 'src/app/infrastructure/password-manager/key.service';
import { PasswordEntry } from 'src/app/infrastructure/password-manager/model/password-entry.model';
import { AddPasswordComponent } from '../add-password/add-password.component';
import { ViewPasswordComponent } from '../view-password/view-password.component';

@Component({
  selector: 'xp-password-list',
  templateUrl: './password-list.component.html',
  styleUrls: ['./password-list.component.css'],
})
export class PasswordListComponent implements OnInit {
  passwords: PasswordEntry[] = [];
  filteredPasswords: PasswordEntry[] = [];
  isLoading: boolean = true;
  hasKeys: boolean = false;
  searchTerm: string = '';

  displayedColumns: string[] = ['siteName', 'username', 'owner', 'createdAt', 'actions'];

  constructor(
    private passwordService: PasswordService,
    private keyService: KeyService,
    private dialog: MatDialog,
    private snackBar: MatSnackBar
  ) {}

  ngOnInit(): void {
    this.checkKeys();
  }

  private checkKeys(): void {
    this.keyService.getPublicKey().subscribe({
      next: (response) => {
        this.hasKeys = response.hasKey;
        if (this.hasKeys) {
          this.loadPasswords();
        } else {
          this.isLoading = false;
        }
      },
      error: (err) => {
        console.error('Greška pri proveri ključeva:', err);
        this.isLoading = false;
      },
    });
  }

  private loadPasswords(): void {
    this.isLoading = true;
    this.passwordService.getPasswords().subscribe({
      next: (passwords) => {
        this.passwords = passwords;
        this.filteredPasswords = passwords;
        this.isLoading = false;
      },
      error: (err) => {
        console.error('Greška pri učitavanju lozinki:', err);
        this.snackBar.open('Greška pri učitavanju lozinki', 'Zatvori', { duration: 3000 });
        this.isLoading = false;
      },
    });
  }

  onSearch(): void {
    if (!this.searchTerm) {
      this.filteredPasswords = this.passwords;
      return;
    }

    const term = this.searchTerm.toLowerCase();
    this.filteredPasswords = this.passwords.filter(
      (p) =>
        p.siteName.toLowerCase().includes(term) ||
        p.username.toLowerCase().includes(term) ||
        p.ownerEmail.toLowerCase().includes(term)
    );
  }

  openAddPasswordDialog(): void {
    const dialogRef = this.dialog.open(AddPasswordComponent, {
      width: '500px',
    });

    dialogRef.afterClosed().subscribe((result) => {
      if (result) {
        this.loadPasswords(); // Refresh lista
      }
    });
  }

  viewPassword(password: PasswordEntry): void {
    this.dialog.open(ViewPasswordComponent, {
      width: '500px',
      data: password,
    });
  }

  deletePassword(password: PasswordEntry): void {
    if (!password.isOwner) {
      this.snackBar.open('Samo vlasnik može da obriše lozinku', 'Zatvori', { duration: 3000 });
      return;
    }

    if (confirm(`Da li ste sigurni da želite da obrišete lozinku za "${password.siteName}"?`)) {
      this.passwordService.deletePassword(password.id).subscribe({
        next: (response) => {
          this.snackBar.open(response.message, 'Zatvori', { duration: 3000 });
          this.loadPasswords(); // Refresh lista
        },
        error: (err) => {
          this.snackBar.open(err.error?.message || 'Greška pri brisanju lozinke', 'Zatvori', {
            duration: 3000,
          });
        },
      });
    }
  }
}
