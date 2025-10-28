import { Component, OnInit } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatDialog } from '@angular/material/dialog';
import { CryptoService } from 'src/app/infrastructure/password-manager/crypto.service';
import { KeyService } from 'src/app/infrastructure/password-manager/key.service';
import { KeyPair } from 'src/app/infrastructure/password-manager/model/key-pair.model';
import { ConfirmDialogComponent } from './confirm-dialog/confirm-dialog.component';

@Component({
  selector: 'xp-key-setup',
  templateUrl: './key-setup.component.html',
  styleUrls: ['./key-setup.component.css'],
})
export class KeySetupComponent implements OnInit {
  hasKey: boolean = false;
  keyGeneratedAt: Date | null = null;
  isLoading: boolean = false;

  constructor(
    private cryptoService: CryptoService,
    private keyService: KeyService,
    private snackBar: MatSnackBar,
    private dialog: MatDialog
  ) {}

  ngOnInit(): void {
    this.checkExistingKey();
  }

  private checkExistingKey(): void {
    this.keyService.getPublicKey().subscribe({
      next: (response) => {
        this.hasKey = response.hasKey;
        this.keyGeneratedAt = response.keyGeneratedAt;
      },
      error: (err) => {
        console.error('Greška pri proveri ključa:', err);
      },
    });
  }

  generateKeys(): void {
    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      width: '500px',
      data: {
        title: 'Generisanje ključeva za Password Manager',
        message:
          'Klikom na "Potvrdi" generisaćete par ključeva. Privatni ključ ćete preuzeti kao fajl. ' +
          'Čuvajte privatni ključ na sigurnom mestu!',
      },
    });

    dialogRef.afterClosed().subscribe((confirmed) => {
      if (confirmed) {
        this.performKeyGeneration();
      }
    });
  }

  private performKeyGeneration(): void {
    this.isLoading = true;

    try {
      // Generisanje ključeva (može trajati nekoliko sekundi)
      setTimeout(() => {
        const keyPair: KeyPair = this.cryptoService.generateKeyPair();

        // Download privatnog ključa
        this.cryptoService.downloadPrivateKey(
          keyPair.privateKeyPem,
          `password_manager_private_key_${Date.now()}.pem`
        );

        // Slanje javnog ključa na backend
        this.keyService.savePublicKey(keyPair.publicKeyPem).subscribe({
          next: (response) => {
            this.isLoading = false;
            this.hasKey = true;
            this.keyGeneratedAt = new Date();
            this.snackBar.open(
              'Ključevi su uspešno generisani! Privatni ključ je preuzet. Čuvajte ga na sigurnom mestu!',
              'Zatvori',
              { duration: 5000 }
            );
          },
          error: (err) => {
            this.isLoading = false;
            this.snackBar.open(
              err.error?.message || 'Greška pri čuvanju javnog ključa',
              'Zatvori',
              { duration: 3000 }
            );
          },
        });
      }, 100);
    } catch (error) {
      this.isLoading = false;
      this.snackBar.open('Greška pri generisanju ključeva', 'Zatvori', { duration: 3000 });
    }
  }
}
