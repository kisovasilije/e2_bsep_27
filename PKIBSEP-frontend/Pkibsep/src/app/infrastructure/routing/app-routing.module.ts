import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { HomeComponent } from 'src/app/feature-modules/layout/home/home.component';
import { LoginComponent } from '../auth/login/login.component';
import { EquipmentComponent } from 'src/app/feature-modules/administration/equipment/equipment.component';
import { AuthGuard } from '../auth/auth.guard';
import { RegistrationComponent } from '../auth/registration/registration.component';
import { ProfileComponent } from 'src/app/feature-modules/profile/profile/profile.component';
import { EmailConfirmationComponent } from '../auth/email-confirmation/email-confirmation.component';
import { PasswordResetComponent } from '../auth/password-reset/password-reset.component';

const routes: Routes = [
  {path: 'home', component: HomeComponent},
  {path: 'login', component: LoginComponent},
  {path: 'register', component: RegistrationComponent},
  {path: 'confirm-email', component: EmailConfirmationComponent},
  {path: 'reset-password', component: PasswordResetComponent},
  {path: 'equipment', component: EquipmentComponent, canActivate: [AuthGuard],},
  {
    path: 'profile',
    component: ProfileComponent
  },
  {
    path: 'password-manager',
    loadChildren: () => import('../../feature-modules/password-manager/password-manager.module').then(m => m.PasswordManagerModule),
    canActivate: [AuthGuard]
  }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
