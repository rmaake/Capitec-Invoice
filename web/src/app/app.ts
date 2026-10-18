import { DatePipe } from '@angular/common';
import { Component, inject } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { ApiFeedbackService } from './api-feedback';
import { WorkspaceDialogsComponent } from './components/workspace-dialogs.component';
import { WorkspaceStore } from './workspace.store';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [DatePipe, RouterLink, RouterLinkActive, RouterOutlet, WorkspaceDialogsComponent],
  templateUrl: './app.html'
})
export class App {
  readonly store = inject(WorkspaceStore);
  readonly apiFeedback = inject(ApiFeedbackService);
  readonly today = new Date();

  dismissPageFeedback(): void { this.store.pageFeedback.set(null); }
}
