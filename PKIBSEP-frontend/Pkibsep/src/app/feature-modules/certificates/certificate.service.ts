import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { map, Observable, tap } from 'rxjs';
import { environment } from 'src/env/environment';
import { CertificateDto } from './models/certificate.model';
import { CreateRootDto } from './models/create-root-dto.model';
import { IssueIntermediateDto } from './models/issue-intermediate-dto.model';
import { UserDto } from './models/user.model';
import { Csr } from './models/csr.model';
import { CsrResponse } from './models/csr-response.model';
import { Ca } from './models/ca.model';
import { extractCn, populateCertificateCns } from 'src/app/shared/utils/certificates.util';
import { CertificatePreview, ReadonlyCertificatePreview } from './models/certificate-preview.model';
import { RevocationReason } from './models/revocation-reason.model';
import { RevocationRequest } from './models/revocation-request.model';

@Injectable({
  providedIn: 'root',
})
export class CertificateService {
  constructor(private http: HttpClient) {}

  createRootCertificate(request: CreateRootDto): Observable<CertificateDto> {
    return this.http.post<CertificateDto>(environment.apiHost + 'certificates/create-root', request);
  }

  issueIntermediateCertificate(request: IssueIntermediateDto): Observable<CertificateDto> {
    return this.http.post<CertificateDto>(environment.apiHost + 'certificates/issue-intermediate', request);
  }

  getCertificates(): Observable<CertificateDto[]> {
    return this.http.get<CertificateDto[]>(environment.apiHost + 'certificates');
  }

  getMyCACertificates(): Observable<CertificateDto[]> {
    return this.http.get<CertificateDto[]>(environment.apiHost + 'certificates/my-ca-certificates');
  }

  getCaUsers(): Observable<UserDto[]> {
    return this.http.get<UserDto[]>(environment.apiHost + 'user/ca-users');
  }

  downloadCertificate(id: number): Observable<Blob> {
    return this.http.get(environment.apiHost + `certificates/${id}/download`, { responseType: 'blob' });
  }

  createCsr(csr: Csr): Observable<CsrResponse> {
    return this.http.post<CsrResponse>(environment.apiHost + 'certificates/csr', csr);
  }

  getCAs(): Observable<Ca[]> {
    return this.http.get<Ca[]>(environment.apiHost + 'certificates/issuers').pipe(
      map(issuers =>
        issuers.map(i => ({
          ...i,
          cn: extractCn(i.subjectDn) || 'Unknown CN',
        }))
      )
    );
  }

  /**
   * Fetches all certificates belonging to the currently authenticated user.
   * User ID is inferred from the authentication context on the server side
   */
  getAllByUserId(): Observable<CertificatePreview[]> {
    return this.http.get<CertificatePreview[]>(environment.apiHost + 'certificates/user').pipe(tap(populateCertificateCns));
  }

  getRevocationReasons(): Observable<RevocationReason[]> {
    return this.http.get<RevocationReason[]>(environment.apiHost + 'certificates/revocation-reasons');
  }

  revokeCertificate(revocationRequest: RevocationRequest): Observable<CertificatePreview> {
    return this.http.post<CertificatePreview>(environment.apiHost + `certificates/${revocationRequest.certificateId}/revoke`, revocationRequest);
  }
}
