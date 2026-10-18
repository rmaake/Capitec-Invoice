import { CommonModule, CurrencyPipe, DatePipe } from '@angular/common';
import { Component, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { InvoiceActionsComponent } from '../../components/invoice-actions/invoice-actions.component';
import { PageHeadingComponent } from '../../components/page-heading/page-heading.component';
import { WorkspaceStore } from '../../workspace.store';

@Component({
  selector: 'app-overview-page',
  standalone: true,
  imports: [CommonModule, CurrencyPipe, DatePipe, RouterLink, PageHeadingComponent, InvoiceActionsComponent],
  templateUrl: './overview-page.component.html'
})
export class OverviewPageComponent {
  readonly store = inject(WorkspaceStore);
}
