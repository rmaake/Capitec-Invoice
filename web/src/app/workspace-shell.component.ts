import { DatePipe } from '@angular/common';
import { Component, inject } from '@angular/core';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { ApiFeedbackService } from './api-feedback';
import { AuthSessionStore } from './auth-session.store';
import { WorkspaceDialogsComponent } from './components/workspace-dialogs/workspace-dialogs.component';
import { InvoiceApi } from './api';
import { WorkspaceStore } from './workspace.store';

@Component({
  selector: 'app-workspace-shell',
  standalone: true,
  imports: [DatePipe, RouterLink, RouterLinkActive, RouterOutlet, WorkspaceDialogsComponent],
  templateUrl: './workspace-shell.component.html'
})
export class WorkspaceShellComponent {
  readonly store = inject(WorkspaceStore);
  readonly apiFeedback = inject(ApiFeedbackService);
  readonly authSession = inject(AuthSessionStore);
  private readonly api = inject(InvoiceApi);
  private readonly router = inject(Router);
  readonly today = new Date();
  readonly signOutError = { message: '' };

  dismissPageFeedback(): void { this.store.pageFeedback.set(null); }

  async signOut(): Promise<void> {
    this.signOutError.message = '';
    try {
      await firstValueFrom(this.api.logout());
      this.authSession.clear();
      await this.router.navigateByUrl('/login');
    } catch {
      this.signOutError.message = 'Could not sign out. Check your connection and try again.';
    }
  }
}