import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatDialogRef } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { CryptoService } from 'src/app/infrastructure/password-manager/crypto.service';
import { KeyService } from 'src/app/infrastructure/password-manager/key.service';
import { PasswordService } from 'src/app/infrastructure/password-manager/password.service';

@Component({
  selector: 'xp-add-password',
  templateUrl: './add-password.component.html',
  styleUrls: ['./add-password.component.css'],
})
export class AddPasswordComponent implements OnInit {
  passwordForm: FormGroup;
  isSubmitting: boolean = false;
  hidePassword: boolean = true;
  publicKeyPem: string | null = null;

  constructor(
    private fb: FormBuilder,
    private dialogRef: MatDialogRef<AddPasswordComponent>,
    private passwordService: PasswordService,
    private keyService: KeyService,
    private cryptoService: CryptoService,
    private snackBar: MatSnackBar
  ) {
    this.passwordForm = this.fb.group({
      siteName: ['', [Validators.required, Validators.maxLength(255)]],
      username: ['', [Validators.required, Validators.maxLength(255)]],
      password: ['', [Validators.required, Validators.minLength(1)]],
    });
  }

  ngOnInit(): void {
    this.loadPublicKey();
  }

  private loadPublicKey(): void {
    this.keyService.getPublicKey().subscribe({
      next: (response) => {
        if (response.hasKey && response.publicKeyPem) {
          this.publicKeyPem = response.publicKeyPem;
        } else {
          this.snackBar.open('Nemate generisan javni ključ', 'Zatvori', { duration: 3000 });
          this.dialogRef.close();
        }
      },
      error: (err) => {
        console.error('Greška pri učitavanju javnog ključa:', err);
        this.snackBar.open('Greška pri učitavanju javnog ključa', 'Zatvori', { duration: 3000 });
        this.dialogRef.close();
      },
    });
  }

  async onSubmit(): Promise<void> {
    if (this.passwordForm.invalid || !this.publicKeyPem) {
      return;
    }

    this.isSubmitting = true;

    try {
      const formValue = this.passwordForm.value;

      const encryptedPassword = await this.cryptoService.encryptPassword(
        formValue.password,
        this.publicKeyPem
      );

      const savePasswordDto = {
        siteName: formValue.siteName,
        username: formValue.username,
        encryptedPassword: encryptedPassword,
      };

      this.passwordService.createPassword(savePasswordDto).subscribe({
        next: (response) => {
          this.snackBar.open(response.message, 'Zatvori', { duration: 3000 });
          this.dialogRef.close(true); // true znači da je lozinka uspešno kreirana
        },
        error: (err) => {
          console.error('Greška pri čuvanju lozinke:', err);
          this.snackBar.open(err.error?.message || 'Greška pri čuvanju lozinke', 'Zatvori', {
            duration: 3000,
          });
          this.isSubmitting = false;
        },
      });
    } catch (error) {
      console.error('Greška pri enkripciji:', error);
      this.snackBar.open('Greška pri enkripciji lozinke', 'Zatvori', { duration: 3000 });
      this.isSubmitting = false;
    }
  }

  onCancel(): void {
    this.dialogRef.close();
  }
}
