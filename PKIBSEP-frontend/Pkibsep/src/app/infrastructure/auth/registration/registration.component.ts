import { Component } from '@angular/core';
import { FormGroup, FormControl, Validators, AbstractControl, ValidationErrors } from '@angular/forms';
import { Registration } from '../model/registration.model';
import { AuthService } from '../auth.service';
import { Router } from '@angular/router';

@Component({
  selector: 'xp-registration',
  templateUrl: './registration.component.html',
  styleUrls: ['./registration.component.css']
})
export class RegistrationComponent {
  errorMessage: string = '';
  isLoading: boolean = false;
  registrationSuccess: boolean = false;
  registeredEmail: string = '';

  constructor(
    private authService: AuthService,
    private router: Router
  ) {}

  registrationForm = new FormGroup({
    email: new FormControl('', [Validators.required, Validators.email]),
    username: new FormControl('', [Validators.required]),
    organization: new FormControl('', [Validators.required]),
    password: new FormControl('', [Validators.required, Validators.minLength(8)]),
    confirmPassword: new FormControl('', [Validators.required]),
  }, { validators: this.passwordMatchValidator });

  passwordMatchValidator(control: AbstractControl): ValidationErrors | null {
    const password = control.get('password');
    const confirmPassword = control.get('confirmPassword');

    if (!password || !confirmPassword) {
      return null;
    }

    return password.value === confirmPassword.value ? null : { passwordMismatch: true };
  }

  register(): void {
    this.errorMessage = '';

    if (this.registrationForm.invalid) {
      if (this.registrationForm.hasError('passwordMismatch')) {
        this.errorMessage = 'Passwords do not match';
      } else {
        this.errorMessage = 'Please fill in all required fields correctly';
      }
      return;
    }

    const registration: Registration = {
      email: this.registrationForm.value.email || "",
      username: this.registrationForm.value.username || "",
      organization: this.registrationForm.value.organization || "",
      password: this.registrationForm.value.password || "",
      confirmPassword: this.registrationForm.value.confirmPassword || "",
    };

    this.isLoading = true;
    this.authService.register(registration).subscribe({
      next: (response) => {
        this.isLoading = false;
        this.registrationSuccess = true;
        this.registeredEmail = registration.email;
      },
      error: (error) => {
        this.isLoading = false;
        this.errorMessage = error.error?.message || 'Registration failed. Please try again.';
      }
    });
  }
}
