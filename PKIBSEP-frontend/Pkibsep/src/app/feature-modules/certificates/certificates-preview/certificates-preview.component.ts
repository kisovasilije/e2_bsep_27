import { Component, inject, OnInit } from '@angular/core';
import { CertificatePreview, ReadonlyCertificatePreview } from '../models/certificate-preview.model';
import { CertificateService } from '../certificate.service';
import { MatDialog, MatDialogRef } from '@angular/material/dialog';
import { RevocationRequestDialogComponent } from '../revocation-request-dialog/revocation-request-dialog.component';
import { CertificateObserverService } from '../certificate-observer.service';

@Component({
  selector: 'xp-certificates-preview',
  templateUrl: './certificates-preview.component.html',
  styleUrls: ['./certificates-preview.component.css'],
})
export class CertificatesPreviewComponent implements OnInit {
  protected displayedColumns: string[] = ['issuedToCn', 'issuedByCn', 'notBefore', 'notAfter', 'isRevoked', 'actions'];

  protected certificates: CertificatePreview[] = [];

  private readonly certificateService = inject(CertificateService);

  private readonly certificateObserverService = inject(CertificateObserverService);

  private readonly dialogRef = inject(MatDialogRef<CertificatesPreviewComponent>);

  private readonly dialog = inject(MatDialog);

  ngOnInit(): void {
    this.init();
  }

  private init(): void {
    this.certificateObserverService.revokedCertificate$.subscribe(certificate => {
      this.updateCertificateRevocationStatus(certificate);
    });

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

  protected openCertificateRevocationDialog(cert: ReadonlyCertificatePreview): void {
    this.dialog.open(RevocationRequestDialogComponent, {
      width: '35%',
      disableClose: false,
      autoFocus: false,
      data: { certificateId: cert.id },
    });
  }

  private updateCertificateRevocationStatus(certificate: ReadonlyCertificatePreview): void {
    const cert = this.certificates.find(c => c.id == certificate.id);
    if (!cert) {
      console.warn(`Certificate revocation data not updated.`);
      return;
    }

    cert.isRevoked = certificate.isRevoked;
  }
}
