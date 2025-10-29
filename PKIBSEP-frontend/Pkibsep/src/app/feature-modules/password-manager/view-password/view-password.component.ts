import { Component, Inject, OnInit } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { CryptoService } from 'src/app/infrastructure/password-manager/crypto.service';
import { PasswordEntry } from 'src/app/infrastructure/password-manager/model/password-entry.model';

@Component({
  selector: 'xp-view-password',
  templateUrl: './view-password.component.html',
  styleUrls: ['./view-password.component.css'],
})
export class ViewPasswordComponent implements OnInit {
  decryptedPassword: string | null = null;
  privateKeyPem: string | null = null;
  isDecrypting: boolean = false;
  hidePassword: boolean = true;

  constructor(
    public dialogRef: MatDialogRef<ViewPasswordComponent>,
    @Inject(MAT_DIALOG_DATA) public passwordEntry: PasswordEntry,
    private cryptoService: CryptoService,
    private snackBar: MatSnackBar
  ) {}

  ngOnInit(): void {}

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      const file = input.files[0];
      this.loadPrivateKey(file);
    }
  }

  private async loadPrivateKey(file: File): Promise<void> {
    try {
      this.isDecrypting = true;
      this.privateKeyPem = await this.cryptoService.readPrivateKeyFromFile(file);

      this.decryptPassword();
    } catch (error: any) {
      console.error('Greška pri učitavanju privatnog ključa:', error);
      this.snackBar.open(error.message || 'Greška pri učitavanju privatnog ključa', 'Zatvori', {
        duration: 3000,
      });
      this.isDecrypting = false;
    }
  }

  private async decryptPassword(): Promise<void> {
    if (!this.privateKeyPem) {
      return;
    }

    try {
      this.decryptedPassword = await this.cryptoService.decryptPassword(
        this.passwordEntry.encryptedPassword,
        this.privateKeyPem
      );
      this.isDecrypting = false;
    } catch (error: any) {
      console.error('Greška pri dekripciji:', error);
      this.snackBar.open(
        error.message || 'Greška pri dekripciji. Proverite da li ste koristili ispravan privatni ključ.',
        'Zatvori',
        { duration: 5000 }
      );
      this.isDecrypting = false;
      this.privateKeyPem = null;
    }
  }

  copyToClipboard(): void {
    if (this.decryptedPassword) {
      navigator.clipboard.writeText(this.decryptedPassword).then(
        () => {
          this.snackBar.open('Lozinka je kopirana u clipboard', 'Zatvori', { duration: 2000 });
        },
        (err) => {
          console.error('Greška pri kopiranju:', err);
          this.snackBar.open('Greška pri kopiranju u clipboard', 'Zatvori', { duration: 2000 });
        }
      );
    }
  }

  onClose(): void {
    this.dialogRef.close();
  }
}
