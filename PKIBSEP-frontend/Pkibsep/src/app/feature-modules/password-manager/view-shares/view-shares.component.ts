import { Component, Inject, OnInit } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { PasswordService } from 'src/app/infrastructure/password-manager/password.service';
import { PasswordEntry } from 'src/app/infrastructure/password-manager/model/password-entry.model';
import { SharedUser } from 'src/app/infrastructure/password-manager/model/shared-user.model';

@Component({
  selector: 'xp-view-shares',
  templateUrl: './view-shares.component.html',
  styleUrls: ['./view-shares.component.css'],
})
export class ViewSharesComponent implements OnInit {
  sharedUsers: SharedUser[] = [];
  isLoading: boolean = true;
  displayedColumns: string[] = ['email', 'sharedAt'];

  constructor(
    public dialogRef: MatDialogRef<ViewSharesComponent>,
    @Inject(MAT_DIALOG_DATA) public passwordEntry: PasswordEntry,
    private passwordService: PasswordService,
    private snackBar: MatSnackBar
  ) {}

  ngOnInit(): void {
    this.loadShares();
  }

  private loadShares(): void {
    this.isLoading = true;
    this.passwordService.getPasswordShares(this.passwordEntry.id).subscribe({
      next: (shares) => {
        this.sharedUsers = shares;
        this.isLoading = false;
      },
      error: (err) => {
        console.error('Greška pri učitavanju share-ova:', err);
        this.snackBar.open('Greška pri učitavanju podataka', 'Zatvori', { duration: 3000 });
        this.isLoading = false;
      },
    });
  }

  onClose(): void {
    this.dialogRef.close();
  }
}
