import { CommonModule, DatePipe } from '@angular/common';
import { Component, inject } from '@angular/core';
import { PageHeadingComponent } from '../../components/page-heading/page-heading.component';
import { WorkspaceStore } from '../../workspace.store';

@Component({
  selector: 'app-reminders-page',
  standalone: true,
  imports: [CommonModule, DatePipe, PageHeadingComponent],
  templateUrl: './reminders-page.component.html'
})
export class RemindersPageComponent {
  readonly store = inject(WorkspaceStore);
}
