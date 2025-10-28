import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { CertificateService } from '../certificate.service';
import { CreateRootDto } from '../models/create-root-dto.model';
import { KeyUsageDto } from '../models/key-usage.model';
import { X500NameDto } from '../models/x500-name.model';
import { UserDto } from '../models/user.model';
import { AuthService } from 'src/app/infrastructure/auth/auth.service';
import { User } from 'src/app/infrastructure/auth/model/user.model';

@Component({
  selector: 'app-create-root-certificate',
  templateUrl: './create-root-certificate.component.html',
  styleUrls: ['./create-root-certificate.component.css']
})
export class CreateRootCertificateComponent implements OnInit {
  rootForm: FormGroup;
  isSubmitting = false;
  errorMessage = '';
  caUsers: UserDto[] = [];
  currentUser: User | undefined;

  constructor(
    private fb: FormBuilder,
    private certificateService: CertificateService,
    private authService: AuthService,
    private router: Router
  ) {
    this.rootForm = this.fb.group({
      // Target CA User (for Admin only)
      targetCaUserId: [null, Validators.required],

      // Subject fields
      commonName: ['', Validators.required],
      organization: [''],
      organizationalUnit: [''],
      country: ['', [Validators.maxLength(2), Validators.minLength(2)]],
      locality: [''],
      state: [''],

      // Certificate fields
      validityDays: [3650, [Validators.required, Validators.min(1)]],
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

    // Admin mora da učita listu CA korisnika
    if (this.isAdmin()) {
      this.loadCaUsers();
    }
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
    if (this.rootForm.invalid) {
      return;
    }

    this.isSubmitting = true;
    this.errorMessage = '';

    const formValue = this.rootForm.value;

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

    const request: CreateRootDto = {
      targetCaUserId: formValue.targetCaUserId,
      subject,
      validityDays: formValue.validityDays,
      pathLenConstraint: formValue.pathLenConstraint || undefined,
      keyUsage
    };

    this.certificateService.createRootCertificate(request).subscribe({
      next: (certificate) => {
        console.log('Root certificate created:', certificate);
        this.router.navigate(['/certificates']);
      },
      error: (err) => {
        console.error('Error creating root certificate:', err);
        this.errorMessage = err.error?.message || 'Failed to create root certificate';
        this.isSubmitting = false;
      }
    });
  }

  onCancel(): void {
    this.router.navigate(['/certificates']);
  }
}
