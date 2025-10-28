import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from 'src/app/infrastructure/auth/auth.service';
import { User } from 'src/app/infrastructure/auth/model/user.model';
import { CertificateService } from '../certificate.service';
import { CertificateDto } from '../models/certificate.model';

@Component({
  selector: 'app-certificate-list',
  templateUrl: './certificate-list.component.html',
  styleUrls: ['./certificate-list.component.css']
})
export class CertificateListComponent implements OnInit {
  certificates: CertificateDto[] = [];
  displayedColumns: string[] = ['serialNumber', 'commonName', 'organization', 'validFrom', 'validTo', 'type', 'actions'];
  user: User | undefined;

  constructor(
    private certificateService: CertificateService,
    private authService: AuthService,
    private router: Router
  ) { }

  ngOnInit(): void {
    this.authService.user$.subscribe(user => {
      this.user = user;
    });
    // TODO: Uncomment when backend GET endpoint is ready
    // this.loadCertificates();
  }

  loadCertificates(): void {
    this.certificateService.getCertificates().subscribe({
      next: (certificates) => {
        this.certificates = certificates;
      },
      error: (err) => {
        console.error('Error loading certificates:', err);
      }
    });
  }

  isAdmin(): boolean {
    return this.user?.role === 'Admin';
  }

  isCaUser(): boolean {
    return this.user?.role === 'CAUser';
  }

  getCertificateType(cert: CertificateDto): string {
    return cert.isCa ? 'CA Certificate' : 'End-Entity';
  }

  navigateToCreateRoot(): void {
    this.router.navigate(['/certificates/create-root']);
  }

  navigateToIssueIntermediate(): void {
    this.router.navigate(['/certificates/issue-intermediate']);
  }

  downloadCertificate(id: number): void {
    this.certificateService.downloadCertificate(id).subscribe({
      next: (blob) => {
        const url = window.URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = `certificate-${id}.pem`;
        link.click();
        window.URL.revokeObjectURL(url);
      },
      error: (err) => {
        console.error('Error downloading certificate:', err);
      }
    });
  }
}
