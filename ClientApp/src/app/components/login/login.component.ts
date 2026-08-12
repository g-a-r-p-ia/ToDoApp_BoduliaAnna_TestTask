import { AfterViewInit, ChangeDetectorRef, Component, OnDestroy, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { environment } from '../../../environments/environment';

declare const google: any;

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrl: './login.component.css',
  imports: [ReactiveFormsModule]
})
export class LoginComponent implements OnInit, AfterViewInit, OnDestroy {
  isLoginMode = true;
  loginForm: FormGroup;
  registerForm: FormGroup;
  apiErrorMessage = '';
  private googleInitTimer?: ReturnType<typeof setInterval>;

  constructor(
    private router: Router,
    private cdr: ChangeDetectorRef,
    private authService: AuthService,
    private fb: FormBuilder
  ) {
    this.loginForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required]]
    });

    this.registerForm = this.fb.group({
      firstName: ['', [Validators.required, Validators.minLength(2)]],
      lastName: ['', [Validators.required, Validators.minLength(2)]],
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6)]]
    });
  }

  ngOnInit(): void {
    this.loginForm.valueChanges.subscribe(() => {
      this.apiErrorMessage = '';
    });
    this.registerForm.valueChanges.subscribe(() => {
      this.apiErrorMessage = '';
    });
  }

  ngAfterViewInit(): void {
    this.initGoogleButton();
  }

  ngOnDestroy(): void {
    if (this.googleInitTimer) {
      clearInterval(this.googleInitTimer);
    }
  }

  private initGoogleButton(): void {
    const timeoutMs = 5000;
    const startedAt = Date.now();

    const timer = setInterval(() => {
      if (typeof google !== 'undefined' && google.accounts?.id) {
        clearInterval(timer);
        google.accounts.id.initialize({
          client_id: environment.googleClientId,
          callback: this.handleCredentialResponse.bind(this)
        });
        google.accounts.id.renderButton(
          document.getElementById('google-signin-button'),
          { theme: 'outline', size: 'large', shape: 'pill' }
        );
      } else if (Date.now() - startedAt >= timeoutMs) {
        clearInterval(timer);
      }
    }, 100);

    this.googleInitTimer = timer;
  }

  toggleMode(): void {
    this.isLoginMode = !this.isLoginMode;
    this.apiErrorMessage = '';
  }

  onLoginSubmit(): void {
    this.apiErrorMessage = '';
    if (this.loginForm.invalid) {
      this.loginForm.markAllAsTouched();
      return;
    }

    const { email, password } = this.loginForm.value;
    this.authService.login(email, password).subscribe({
      next: () => this.router.navigate(['/tasks']),
      error: (err) => {
        this.loginForm.markAllAsTouched();
        if (err?.status === 404) {
          this.apiErrorMessage = 'You are not registered yet, register please.';
        } else if (err?.status === 401) {
          this.apiErrorMessage = 'Invalid login credentials.';
        } else {
          this.apiErrorMessage = this.getErrorMessage(err);
        }
        this.cdr.detectChanges();
        console.error(err);
      }
    });
  }

  onRegisterSubmit(): void {
    this.apiErrorMessage = '';
    if (this.registerForm.invalid) {
      this.registerForm.markAllAsTouched();
      return;
    }

    const { firstName, lastName, email, password } = this.registerForm.value;
    this.authService.register(firstName, lastName, email, password).subscribe({
      next: () => this.router.navigate(['/tasks']),
      error: (err) => {
        this.registerForm.markAllAsTouched();
        this.apiErrorMessage = this.getErrorMessage(err);
        this.cdr.detectChanges();
        console.error(err);
      }
    });
  }

  private handleCredentialResponse(response: any): void {
    this.authService.loginWithGoogle(response.credential).subscribe({
      next: () => this.router.navigate(['/tasks']),
      error: (err) => {
        this.apiErrorMessage = this.getErrorMessage(err);
        this.cdr.detectChanges();
        console.error(err);
      }
    });
  }

  private getErrorMessage(err: any): string {
    return err?.error?.message || err?.message || 'An unexpected error occurred. Please try again.';
  }
}
