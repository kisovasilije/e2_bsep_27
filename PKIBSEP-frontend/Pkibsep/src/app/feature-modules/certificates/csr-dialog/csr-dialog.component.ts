import { Component, inject } from '@angular/core';
import { Csr } from '../models/csr.model';
import { CertificateService } from '../certificate.service';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatDialogRef } from '@angular/material/dialog';

@Component({
  selector: 'xp-csr-dialog',
  templateUrl: './csr-dialog.component.html',
  styleUrls: ['./csr-dialog.component.css'],
})
export class CsrDialogComponent {
  protected csr: Csr | undefined = undefined;

  protected isCreateCsrButtonDisabled = true;

  private readonly certificateService = inject(CertificateService);

  private readonly snackBar = inject(MatSnackBar);

  private readonly dialogRef = inject(MatDialogRef<CsrDialogComponent>);

  protected onCsrPemChanged($event: Event): void {
    const input = $event.target as HTMLInputElement;
    if (!input.files?.length) {
      return;
    }

    const file = input.files[0];
    const reader = new FileReader();

    reader.onload = () => {
      const text = reader.result as string;
      this.csr = {
        csrPem: text,
      };

      this.isCreateCsrButtonDisabled = false;
    };

    reader.readAsText(file);
  }

  protected onCreateCsr(): void {
    if (!this.csr) {
      console.log('CSR is undefined');
      return;
    }

    this.certificateService.createCsr(this.csr).subscribe((csrResponse) => {
      this.snackBar.open('CSR created successfully.', 'Close', { duration: 3000 });

      this.dialogRef.close();
    });
  }
}
