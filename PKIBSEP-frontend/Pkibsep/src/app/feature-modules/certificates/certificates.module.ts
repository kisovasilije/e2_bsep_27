import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule } from '@angular/forms';
import { MaterialModule } from 'src/app/infrastructure/material/material.module';
import { CertificatesRoutingModule } from './certificates-routing.module';
import { CertificateListComponent } from './certificate-list/certificate-list.component';
import { CreateRootCertificateComponent } from './create-root-certificate/create-root-certificate.component';
import { IssueIntermediateCertificateComponent } from './issue-intermediate-certificate/issue-intermediate-certificate.component';
import { CsrDialogComponent } from './csr-dialog/csr-dialog.component';
import { MatButtonModule } from '@angular/material/button';

@NgModule({
  declarations: [CertificateListComponent, CreateRootCertificateComponent, IssueIntermediateCertificateComponent, CsrDialogComponent],
  imports: [CommonModule, ReactiveFormsModule, MaterialModule, CertificatesRoutingModule, MatButtonModule],
})
export class CertificatesModule {}
