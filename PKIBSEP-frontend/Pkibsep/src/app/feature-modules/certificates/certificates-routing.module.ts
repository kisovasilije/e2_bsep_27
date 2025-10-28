import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AuthGuard } from 'src/app/infrastructure/auth/auth.guard';
import { CertificateListComponent } from './certificate-list/certificate-list.component';
import { CreateRootCertificateComponent } from './create-root-certificate/create-root-certificate.component';
import { IssueIntermediateCertificateComponent } from './issue-intermediate-certificate/issue-intermediate-certificate.component';

const routes: Routes = [
  {
    path: '',
    component: CertificateListComponent,
    canActivate: [AuthGuard]
  },
  {
    path: 'create-root',
    component: CreateRootCertificateComponent,
    canActivate: [AuthGuard]
  },
  {
    path: 'issue-intermediate',
    component: IssueIntermediateCertificateComponent,
    canActivate: [AuthGuard]
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class CertificatesRoutingModule { }
