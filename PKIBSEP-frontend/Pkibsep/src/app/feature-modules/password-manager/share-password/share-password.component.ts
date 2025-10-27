import { Component, Inject, OnInit } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { PasswordService } from 'src/app/infrastructure/password-manager/password.service';
import { CryptoService } from 'src/app/infrastructure/password-manager/crypto.service';
import { KeyService } from 'src/app/infrastructure/password-manager/key.service';
import { PasswordEntry } from 'src/app/infrastructure/password-manager/model/password-entry.model';
import { RegularUser } from 'src/app/infrastructure/password-manager/model/regular-user.model';
import { SharePassword } from 'src/app/infrastructure/password-manager/model/share-password.model';

@Component({
  selector: 'xp-share-password',
  templateUrl: './share-password.component.html',
  styleUrls: ['./share-password.component.css'],
})
export class SharePasswordComponent implements OnInit {
  shareForm: FormGroup;
  availableUsers: RegularUser[] = [];
  isLoadingUsers: boolean = true;
  isSubmitting: boolean = false;
  privateKeyPem: string | null = null;
  privateKeyFileName: string = '';

  constructor(
    public dialogRef: MatDialogRef<SharePasswordComponent>,
    @Inject(MAT_DIALOG_DATA) public passwordEntry: PasswordEntry,
    private fb: FormBuilder,
    private passwordService: PasswordService,
    private cryptoService: CryptoService,
    private keyService: KeyService,
    private snackBar: MatSnackBar
  ) {
    this.shareForm = this.fb.group({
      targetUserId: [null, Validators.required],
    });
  }

  ngOnInit(): void {
    this.loadAvailableUsers();
  }

  private loadAvailableUsers(): void {
    this.isLoadingUsers = true;
    this.passwordService.getAvailableUsersForSharing(this.passwordEntry.id).subscribe({
      next: (users) => {
        this.availableUsers = users;
        this.isLoadingUsers = false;
      },
      error: (err) => {
        console.error('Greška pri učitavanju korisnika:', err);
        this.snackBar.open('Greška pri učitavanju korisnika', 'Zatvori', { duration: 3000 });
        this.isLoadingUsers = false;
      },
    });
  }

  onPrivateKeySelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      const file = input.files[0];
      this.privateKeyFileName = file.name;
      this.loadPrivateKey(file);
    }
  }

  private async loadPrivateKey(file: File): Promise<void> {
    try {
      this.privateKeyPem = await this.cryptoService.readPrivateKeyFromFile(file);
      this.snackBar.open('Privatni ključ je uspešno učitan', 'Zatvori', { duration: 2000 });
    } catch (error: any) {
      console.error('Greška pri učitavanju privatnog ključa:', error);
      this.snackBar.open(error.message || 'Greška pri učitavanju privatnog ključa', 'Zatvori', {
        duration: 3000,
      });
      this.privateKeyPem = null;
      this.privateKeyFileName = '';
    }
  }

  onSubmit(): void {
    if (this.shareForm.invalid) {
      return;
    }

    if (!this.privateKeyPem) {
      this.snackBar.open('Molimo učitajte vaš privatni ključ', 'Zatvori', { duration: 3000 });
      return;
    }

    const targetUserId = this.shareForm.value.targetUserId;
    const targetUser = this.availableUsers.find((u) => u.id === targetUserId);

    if (!targetUser) {
      this.snackBar.open('Korisnik nije pronađen', 'Zatvori', { duration: 3000 });
      return;
    }

    // Preuzmi javni ključ target korisnika
    this.isSubmitting = true;
    this.keyService.getUserPublicKey(targetUserId).subscribe({
      next: (response) => {
        if (!response.publicKeyPem) {
          this.snackBar.open('Korisnik nema javni ključ', 'Zatvori', { duration: 3000 });
          this.isSubmitting = false;
          return;
        }

        try {
          // Re-enkriptuj lozinku za target korisnika
          const encryptedPasswordForTarget = this.cryptoService.reEncryptForSharing(
            this.passwordEntry.encryptedPassword,
            this.privateKeyPem!,
            response.publicKeyPem
          );

          const shareData: SharePassword = {
            targetUserId: targetUserId,
            encryptedPasswordForTarget: encryptedPasswordForTarget,
          };

          // Pozovi API za deljenje
          this.passwordService.sharePassword(this.passwordEntry.id, shareData).subscribe({
            next: (result) => {
              this.snackBar.open(result.message, 'Zatvori', { duration: 3000 });
              this.dialogRef.close(true);
            },
            error: (err) => {
              console.error('Greška pri deljenju lozinke:', err);
              this.snackBar.open(err.error?.message || 'Greška pri deljenju lozinke', 'Zatvori', {
                duration: 3000,
              });
              this.isSubmitting = false;
            },
          });
        } catch (error: any) {
          console.error('Greška pri re-enkripciji:', error);
          this.snackBar.open(
            error.message || 'Greška pri re-enkripciji. Proverite da li ste koristili ispravan privatni ključ.',
            'Zatvori',
            { duration: 5000 }
          );
          this.isSubmitting = false;
        }
      },
      error: (err) => {
        console.error('Greška pri preuzimanju javnog ključa:', err);
        this.snackBar.open('Greška pri preuzimanju javnog ključa korisnika', 'Zatvori', {
          duration: 3000,
        });
        this.isSubmitting = false;
      },
    });
  }

  onCancel(): void {
    this.dialogRef.close(false);
  }
}
