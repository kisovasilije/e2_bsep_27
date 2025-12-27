import { Injectable } from '@angular/core';
import { Subject } from 'rxjs';
import { ReadonlyCertificatePreview } from './models/certificate-preview.model';

@Injectable({
  providedIn: 'root',
})
export class CertificateObserverService {
  private revokedCertificateSubject = new Subject<ReadonlyCertificatePreview>();

  constructor() {}

  public revokedCertificate$ = this.revokedCertificateSubject.asObservable();

  public revokeCertificate(certificate: ReadonlyCertificatePreview): void {
    this.revokedCertificateSubject.next(certificate);
  }
}
