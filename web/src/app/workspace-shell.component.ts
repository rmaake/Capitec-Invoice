import { DatePipe } from '@angular/common';
import { Component, inject } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { ApiFeedbackService } from './api-feedback';
import { AuthSessionStore } from './auth-session.store';
import { WorkspaceDialogsComponent } from './components/workspace-dialogs/workspace-dialogs.component';
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
  readonly today = new Date();

  dismissPageFeedback(): void { this.store.pageFeedback.set(null); }
}