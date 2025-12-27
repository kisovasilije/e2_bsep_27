import { Component, inject, Inject, OnInit } from '@angular/core';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { RevocationRequestForm } from '../models/revocation-request-form.model';
import { RevocationReason } from '../models/revocation-reason.model';
import { CertificateService } from '../certificate.service';
import { MatSnackBar } from '@angular/material/snack-bar';
import { RevocationRequest } from '../models/revocation-request.model';
import { CertificateObserverService } from '../certificate-observer.service';
import { HttpErrorResponse } from '@angular/common/http';
import { CertificatesPreviewComponent } from '../certificates-preview/certificates-preview.component';

@Component({
  selector: 'xp-revocation-request-dialog',
  templateUrl: './revocation-request-dialog.component.html',
  styleUrls: ['./revocation-request-dialog.component.css'],
})
export class RevocationRequestDialogComponent implements OnInit {
  protected readonly form: FormGroup<RevocationRequestForm>;

  protected reasons: RevocationReason[] = [];

  private readonly certificateService = inject(CertificateService);

  private readonly certificateObserverService = inject(CertificateObserverService);

  private readonly snackBar = inject(MatSnackBar);

  private readonly dialogRef = inject(MatDialogRef<CertificatesPreviewComponent>);

  constructor(@Inject(MAT_DIALOG_DATA) private data: { certificateId: number }) {
    this.form = new FormGroup<RevocationRequestForm>({
      reason: new FormControl<number | null>(null, { validators: Validators.required }),
      comment: new FormControl<string | null>(null, { validators: Validators.maxLength(255) }),
    });
  }

  ngOnInit(): void {
    this.certificateService.getRevocationReasons().subscribe(reasons => {
      this.reasons = [...reasons];
    });
  }

  protected revokeCertificate(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const { reason, comment } = this.form.controls;
    if (reason.value === null) {
      this.snackBar.open('Please select a revocation reason.', 'Close', { duration: 3000 });
      return;
    }

    const payload: RevocationRequest = {
      certificateId: this.data.certificateId,
      reason: reason.value,
      comment: comment.value,
    };

    this.certificateService.revokeCertificate(payload).subscribe({
      next: certificate => {
        this.snackBar.open('Certificate revoked successfully.', 'Close', { duration: 3000 });

        this.certificateObserverService.revokeCertificate(certificate);
        this.dialogRef.close();
      },
      error: (error: HttpErrorResponse) => {
        console.error(error.message);
      },
    });
  }
}
