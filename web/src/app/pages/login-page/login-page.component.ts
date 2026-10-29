import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { ApiFeedbackService } from '../../api-feedback';
import { InvoiceApi } from '../../api';

@Component({
  selector: 'app-login-page',
  standalone: true,
  imports: [FormsModule],
  templateUrl: './login-page.component.html'
})
export class LoginPageComponent {
  private readonly api = inject(InvoiceApi);
  private readonly router = inject(Router);
  readonly apiFeedback = inject(ApiFeedbackService);
  readonly email = signal('');
  readonly password = signal('');
  readonly rememberMe = signal(false);
  readonly submitting = signal(false);
  readonly message = signal('');

  async submit(): Promise<void> {
    if (this.submitting()) return;

    this.submitting.set(true);
    this.message.set('');
    try {
      await firstValueFrom(this.api.login({
        email: this.email(),
        password: this.password(),
        rememberMe: this.rememberMe()
      }));
      await this.router.navigateByUrl('/overview');
    } catch (error) {
      this.message.set(this.getErrorMessage(error));
    } finally {
      this.submitting.set(false);
    }
  }

  private getErrorMessage(error: unknown): string {
    if (error instanceof HttpErrorResponse) {
      if (typeof error.error === 'string' && error.error.trim()) return error.error;
      if (typeof error.error?.message === 'string') return error.error.message;
      if (typeof error.error?.title === 'string') return error.error.title;
      if (error.status === 0) return 'Could not reach the sign-in service. Check your connection and try again.';
    }
    return 'Sign-in failed. Check your details and try again.';
  }
}