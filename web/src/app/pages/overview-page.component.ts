import { CommonModule, CurrencyPipe, DatePipe } from '@angular/common';
import { Component, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { InvoiceActionsComponent } from '../components/invoice-actions.component';
import { PageHeadingComponent } from '../components/page-heading.component';
import { WorkspaceStore } from '../workspace.store';

@Component({
  selector: 'app-overview-page',
  standalone: true,
  imports: [CommonModule, CurrencyPipe, DatePipe, RouterLink, PageHeadingComponent, InvoiceActionsComponent],
  template: `
    <app-page-heading title="Invoice overview" subtitle="A clear view of what is owed, what is late, and what is moving.">
      <button class="button secondary" type="button" (click)="store.runSimulation()" [disabled]="store.simulating()">◷ {{ store.simulating() ? 'Running…' : 'Simulate reminders' }}</button>
      <button class="button primary" type="button" (click)="store.openInvoiceDialog()">＋ New invoice</button>
    </app-page-heading>
    <section class="metrics">
      <article class="metric dark"><span>OUTSTANDING</span><strong>{{ store.dashboard()?.outstandingTotal ?? 0 | currency:'ZAR':'symbol-narrow':'1.2-2':'en-ZA' }}</strong><small>Open receivables · ZAR</small><i></i></article>
      <article class="metric"><span>OVERDUE</span><strong>{{ store.dashboard()?.overdueCount ?? 0 }}</strong><small>Needs a follow-up</small><i class="warm"></i></article>
      <article class="metric"><span>INVOICES</span><strong>{{ store.dashboard()?.invoiceCount ?? 0 }}</strong><small>All statuses</small><i class="green"></i></article>
      <article class="metric"><span>CUSTOMERS</span><strong>{{ store.dashboard()?.customerCount ?? 0 }}</strong><small>Billing contacts</small><i class="blue"></i></article>
    </section>
    <section class="overview-grid">
      <div class="panel"><div class="panel-heading"><div><h2>Recent invoices</h2><p>Latest activity across your accounts</p></div><a class="text-button" routerLink="/invoices">View all →</a></div>
        <div class="table-wrap"><table><thead><tr><th>INVOICE</th><th>CUSTOMER</th><th>DUE DATE</th><th>STATUS</th><th class="right">AMOUNT</th><th>ACTION</th></tr></thead><tbody>
          @for (invoice of store.invoices().slice(0, 6); track invoice.id) {
            <tr><td><button class="invoice-link" type="button" (click)="store.openInvoiceDetails(invoice)">{{ invoice.number }}</button><small>{{ invoice.items.length }} {{ invoice.items.length === 1 ? 'item' : 'items' }}</small></td><td>{{ invoice.customerName }}</td><td [class.late-text]="invoice.overdueDays > 0">{{ invoice.dueAtUtc | date:'d MMM y' }}</td><td><span class="status" [class.late]="invoice.overdueDays > 0" [class.paid]="invoice.status === 'Paid'" [class.void-status]="invoice.status === 'Void'">{{ invoice.overdueDays > 0 ? 'Overdue' : invoice.status }}</span></td><td class="right amount">{{ invoice.total | currency:invoice.currency:'symbol-narrow':'1.2-2':'en-ZA' }}</td><td><app-invoice-actions [invoice]="invoice" (details)="store.openInvoiceDetails($event)" (paid)="store.markPaid($event)" (voidRequested)="store.openVoidDialog($event)" /></td></tr>
          } @empty { <tr><td colspan="6"><div class="empty"><b>No invoices yet</b><p>Create an invoice to start tracking receivables.</p><button class="text-button" type="button" (click)="store.openInvoiceDialog()">Create invoice →</button></div></td></tr> }
        </tbody></table></div>
      </div>
      <aside class="panel activity"><div class="panel-heading"><div><h2>Reminder activity</h2><p>Simulated, never delivered</p></div><a class="text-button" routerLink="/reminders">→</a></div>
        @for (item of store.notifications().slice(0, 4); track item.id) { <div class="activity-row"><span class="activity-icon">◷</span><div><strong>{{ item.invoiceNumber }} reminder</strong><p>{{ item.customerName }} · {{ item.overdueDays }} days overdue</p><small>{{ item.simulatedAtUtc | date:'d MMM, HH:mm' }} · Simulated</small></div></div> }
        @if (!store.notifications().length) { <div class="activity-empty"><span>◷</span><b>No reminder activity</b><p>Overdue simulations will appear here.</p></div> }
        <a class="activity-link" routerLink="/reminders">Open reminder log <span>→</span></a>
      </aside>
    </section>
  `
})
export class OverviewPageComponent {
  readonly store = inject(WorkspaceStore);
}
