import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { LoginComponent } from './login/login.component';
import { MaterialModule } from '../material/material.module';
import { ReactiveFormsModule } from '@angular/forms';
import { RegistrationComponent } from './registration/registration.component';
import { EmailConfirmationComponent } from './email-confirmation/email-confirmation.component';
import { RecaptchaModule } from "ng-recaptcha";



@NgModule({
  declarations: [
    LoginComponent,
    RegistrationComponent,
    EmailConfirmationComponent
  ],
  imports: [
    CommonModule,
    MaterialModule,
    ReactiveFormsModule,
    RecaptchaModule
],
  exports: [
    LoginComponent
  ]
})
export class AuthModule { }
