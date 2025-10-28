import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { CertificateService } from '../certificate.service';
import { CertificateDto } from '../models/certificate.model';
import { IssueIntermediateDto } from '../models/issue-intermediate-dto.model';
import { KeyUsageDto } from '../models/key-usage.model';
import { X500NameDto } from '../models/x500-name.model';
import { UserDto } from '../models/user.model';
import { AuthService } from 'src/app/infrastructure/auth/auth.service';
import { User } from 'src/app/infrastructure/auth/model/user.model';

@Component({
  selector: 'app-issue-intermediate-certificate',
  templateUrl: './issue-intermediate-certificate.component.html',
  styleUrls: ['./issue-intermediate-certificate.component.css']
})
export class IssueIntermediateCertificateComponent implements OnInit {
  intermediateForm: FormGroup;
  isSubmitting = false;
  errorMessage = '';
  caCertificates: CertificateDto[] = [];
  caUsers: UserDto[] = [];
  currentUser: User | undefined;

  constructor(
    private fb: FormBuilder,
    private certificateService: CertificateService,
    private authService: AuthService,
    private router: Router
  ) {
    this.intermediateForm = this.fb.group({
      // Target CA User (for Admin only)
      targetCaUserId: [null],

      // Issuer selection
      issuerId: ['', Validators.required],

      // Subject fields
      commonName: ['', Validators.required],
      organization: [''],
      organizationalUnit: [''],
      country: ['', [Validators.maxLength(2), Validators.minLength(2)]],
      locality: [''],
      state: [''],

      // Certificate fields
      validityDays: [1825, [Validators.required, Validators.min(1)]],
      pathLenConstraint: [null],

      // Key Usage
      digitalSignature: [false],
      nonRepudiation: [false],
      keyEncipherment: [false],
      dataEncipherment: [false],
      keyAgreement: [false],
      keyCertSign: [true],  // Default for CA
      crlSign: [true],      // Default for CA
      encipherOnly: [false],
      decipherOnly: [false]
    });
  }

  ngOnInit(): void {
    this.authService.user$.subscribe(user => {
      this.currentUser = user;
    });

    // Load available CA certificates
    this.loadMyCACertificates();

    // Load CA users (Admin sees all, CA User sees same organization)
    this.loadCaUsers();

    // Admin MUST specify target user
    if (this.isAdmin()) {
      this.intermediateForm.get('targetCaUserId')?.setValidators([Validators.required]);
    }
  }

  loadMyCACertificates(): void {
    this.certificateService.getMyCACertificates().subscribe({
      next: (certificates) => {
        this.caCertificates = certificates;
      },
      error: (err) => {
        console.error('Error loading CA certificates:', err);
        this.errorMessage = 'Failed to load CA certificates';
      }
    });
  }

  loadCaUsers(): void {
    this.certificateService.getCaUsers().subscribe({
      next: (users) => {
        this.caUsers = users;
      },
      error: (err) => {
        console.error('Error loading CA users:', err);
        this.errorMessage = 'Failed to load CA users';
      }
    });
  }

  isAdmin(): boolean {
    return this.currentUser?.role === 'Admin';
  }

  onSubmit(): void {
    if (this.intermediateForm.invalid) {
      return;
    }

    this.isSubmitting = true;
    this.errorMessage = '';

    const formValue = this.intermediateForm.value;

    const subject: X500NameDto = {
      cn: formValue.commonName,
      o: formValue.organization || undefined,
      ou: formValue.organizationalUnit || undefined,
      c: formValue.country || undefined,
      l: formValue.locality || undefined,
      st: formValue.state || undefined
    };

    const keyUsage: KeyUsageDto = {
      digitalSignature: formValue.digitalSignature,
      nonRepudiation: formValue.nonRepudiation,
      keyEncipherment: formValue.keyEncipherment,
      dataEncipherment: formValue.dataEncipherment,
      keyAgreement: formValue.keyAgreement,
      keyCertSign: formValue.keyCertSign,
      crlSign: formValue.crlSign,
      encipherOnly: formValue.encipherOnly,
      decipherOnly: formValue.decipherOnly
    };

    const request: IssueIntermediateDto = {
      targetCaUserId: formValue.targetCaUserId || undefined,
      issuerId: formValue.issuerId,
      subject,
      validityDays: formValue.validityDays,
      pathLenConstraint: formValue.pathLenConstraint || undefined,
      keyUsage
    };

    this.certificateService.issueIntermediateCertificate(request).subscribe({
      next: (certificate) => {
        console.log('Intermediate certificate issued:', certificate);
        this.router.navigate(['/certificates']);
      },
      error: (err) => {
        console.error('Error issuing intermediate certificate:', err);
        this.errorMessage = err.error?.message || 'Failed to issue intermediate certificate';
        this.isSubmitting = false;
      }
    });
  }

  onCancel(): void {
    this.router.navigate(['/certificates']);
  }

  getCertificateDisplayName(cert: CertificateDto): string {
    return `${cert.subjectDN} (Serial: ${cert.serialHex})`;
  }
}
