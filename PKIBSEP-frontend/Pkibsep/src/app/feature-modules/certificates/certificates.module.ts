import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule } from '@angular/forms';
import { MaterialModule } from 'src/app/infrastructure/material/material.module';
import { CertificatesRoutingModule } from './certificates-routing.module';
import { CertificateListComponent } from './certificate-list/certificate-list.component';
import { CreateRootCertificateComponent } from './create-root-certificate/create-root-certificate.component';
import { IssueIntermediateCertificateComponent } from './issue-intermediate-certificate/issue-intermediate-certificate.component';

@NgModule({
  declarations: [
    CertificateListComponent,
    CreateRootCertificateComponent,
    IssueIntermediateCertificateComponent
  ],
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MaterialModule,
    CertificatesRoutingModule
  ]
})
export class CertificatesModule { }
