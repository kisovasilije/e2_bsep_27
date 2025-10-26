import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { AuthService } from '../auth.service';

@Component({
  selector: 'xp-email-confirmation',
  templateUrl: './email-confirmation.component.html',
  styleUrls: ['./email-confirmation.component.css']
})
export class EmailConfirmationComponent implements OnInit {
  isLoading: boolean = true;
  success: boolean = false;
  message: string = '';

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private authService: AuthService
  ) {}

  ngOnInit(): void {
    this.route.queryParams.subscribe(params => {
      const token = params['token'];

      if (!token) {
        this.isLoading = false;
        this.success = false;
        this.message = 'Invalid verification link. No token provided.';
        return;
      }

      this.authService.confirmEmail(token).subscribe({
        next: (response) => {
          this.isLoading = false;
          this.success = response.success;
          this.message = response.message;
        },
        error: (error) => {
          this.isLoading = false;
          this.success = false;
          this.message = error.error?.message || 'Email verification failed. The link may be expired or invalid.';
        }
      });
    });
  }

  navigateToLogin(): void {
    this.router.navigate(['/login']);
  }
}
