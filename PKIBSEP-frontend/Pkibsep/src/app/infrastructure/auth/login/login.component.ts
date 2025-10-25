import { Component } from '@angular/core';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { AuthService } from '../auth.service';
import { Router } from '@angular/router';
import { Login } from '../model/login.model';

@Component({
  selector: 'xp-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css'],
})
export class LoginComponent {
  captchaToken: string = '';

  constructor(private authService: AuthService, private router: Router) {}

  loginForm = new FormGroup({
    email: new FormControl('', [Validators.required]),
    password: new FormControl('', [Validators.required]),
  });

  onCaptchaResolved(token: string | null) {
    if (token) {
      this.captchaToken = token;
    }
  }

  login(): void {
    if (!this.captchaToken) {
      alert('Molimo vas da rešite captcha');
      return;
    }

    const login: Login = {
      email: this.loginForm.value.email || '',
      password: this.loginForm.value.password || '',
      captchaToken: this.captchaToken,
    };

    if (this.loginForm.valid) {
      this.authService.login(login).subscribe({
        next: () => {
          this.router.navigate(['/']);
        },
      });
    }
  }
}
