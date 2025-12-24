import { Component, inject, OnInit } from '@angular/core';
import { ReadonlyCertificatePreview } from '../models/certificate-preview.model';
import { CertificateService } from '../certificate.service';
import { MatDialogRef } from '@angular/material/dialog';

@Component({
  selector: 'xp-certificates-preview',
  templateUrl: './certificates-preview.component.html',
  styleUrls: ['./certificates-preview.component.css'],
})
export class CertificatesPreviewComponent implements OnInit {
  protected displayedColumns: string[] = ['issuedToCn', 'issuedByCn', 'notBefore', 'notAfter', 'download'];

  protected certificates: ReadonlyCertificatePreview[] = [];

  private readonly certificateService = inject(CertificateService);

  private readonly dialogRef = inject(MatDialogRef<CertificatesPreviewComponent>);

  ngOnInit(): void {
    this.certificateService.getAllByUserId().subscribe(certificates => {
      this.certificates = [...certificates];
    });
  }

  protected closeDialog(): void {
    this.dialogRef.close();
  }

  protected downloadCertificate(cert: ReadonlyCertificatePreview): void {
    const blob = new Blob([cert.pem], { type: 'application/x-pem-file' });
    const url = window.URL.createObjectURL(blob);

    const a = document.createElement('a');
    a.href = url;
    a.download = `certificate_${cert.id}.pem`;
    a.click();

    window.URL.revokeObjectURL(url);
  }
}
