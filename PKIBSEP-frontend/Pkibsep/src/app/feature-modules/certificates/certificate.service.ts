import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from 'src/env/environment';
import { CertificateDto } from './models/certificate.model';
import { CreateRootDto } from './models/create-root-dto.model';
import { IssueIntermediateDto } from './models/issue-intermediate-dto.model';
import { UserDto } from './models/user.model';
import { Csr } from './models/csr.model';

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

  createCsr(csr: Csr): Observable<void> {
    return this.http.post<void>(environment.apiHost + 'certificates/csr', csr);
  }
}
