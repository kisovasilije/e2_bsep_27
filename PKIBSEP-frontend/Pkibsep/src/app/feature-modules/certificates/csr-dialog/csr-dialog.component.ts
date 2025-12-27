import { Component, inject, OnInit } from '@angular/core';
import { Csr } from '../models/csr.model';
import { CertificateService } from '../certificate.service';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatDialogRef } from '@angular/material/dialog';
import { Ca } from '../models/ca.model';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { CsrForm } from '../models/csr-form.model';

@Component({
  selector: 'xp-csr-dialog',
  templateUrl: './csr-dialog.component.html',
  styleUrls: ['./csr-dialog.component.css'],
})
export class CsrDialogComponent implements OnInit {
  protected issuers: Ca[] = [];

  protected readonly form: FormGroup<CsrForm>;

  private readonly certificateService = inject(CertificateService);

  private readonly snackBar = inject(MatSnackBar);

  private readonly dialogRef = inject(MatDialogRef<CsrDialogComponent>);

  constructor() {
    this.form = new FormGroup<CsrForm>({
      issuer: new FormControl<Ca | null>(null, { nonNullable: false, validators: Validators.required }),
      notBefore: new FormControl<Date | null>(null, { validators: Validators.required }),
      notAfter: new FormControl<Date | null>(null, { validators: Validators.required }),
      csrPem: new FormControl<string | null>(null, { validators: Validators.required }),
    });
  }

  ngOnInit(): void {
    this.init();
  }

  private init(): void {
    this.certificateService.getCAs().subscribe(issuers => {
      this.issuers = [...issuers];
    });
  }

  protected onCsrPemChanged($event: Event): void {
    const input = $event.target as HTMLInputElement;
    if (!input.files?.length) {
      return;
    }

    const file = input.files[0];
    const reader = new FileReader();

    reader.onload = () => {
      this.form.patchValue({ csrPem: reader.result as string });
      this.form.get('csrPem')?.markAsDirty();
    };

    reader.readAsText(file);
  }

  protected onCreateCsr(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();

      console.log('CSR is undefined');
      return;
    }

    const { issuer, notBefore, notAfter, csrPem } = this.form.controls;
    const payload: Csr = {
      caId: issuer.value?.id!,
      notBefore: notBefore.value!,
      notAfter: notAfter.value!,
      csrPem: csrPem.value!,
    };

    this.certificateService.createCsr(payload).subscribe({
      next: csrResponse => {
        this.snackBar.open('CSR created successfully.', 'Close', { duration: 3000 });

        this.dialogRef.close();
      },
      error: err => {
        console.log(err);
        this.snackBar.open(`Error creating CSR: ${err.error || err.message}`, 'Close', { duration: 5000 });
      },
    });
  }
}
