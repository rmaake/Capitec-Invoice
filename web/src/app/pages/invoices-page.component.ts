import { CommonModule, CurrencyPipe, DatePipe } from '@angular/common';
import { Component, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { InvoiceActionsComponent } from '../components/invoice-actions.component';
import { PageHeadingComponent } from '../components/page-heading.component';
import { WorkspaceStore } from '../workspace.store';

@Component({
  selector: 'app-invoices-page',
  standalone: true,
  imports: [CommonModule, FormsModule, CurrencyPipe, DatePipe, PageHeadingComponent, InvoiceActionsComponent],
  template: `
    <app-page-heading title="Invoices" subtitle="Create, review and keep every customer invoice up to date.">
      <button class="button primary" type="button" (click)="store.openInvoiceDialog()">＋ New invoice</button>
    </app-page-heading>
    <section class="panel full-panel">
      <div class="toolbar"><div class="filters"><label class="search"><span>⌕</span><input type="search" [ngModel]="search()" (ngModelChange)="search.set($event)" placeholder="Search invoices or customers" aria-label="Search invoices or customers"></label><select [ngModel]="statusFilter()" (ngModelChange)="statusFilter.set($event)" aria-label="Filter invoices"><option value="all">All statuses</option><option value="sent">Sent</option><option value="paid">Paid</option><option value="void">Void</option></select></div><button class="button secondary" type="button" (click)="exportInvoices()">Export CSV</button></div>
      <div class="table-wrap"><table><thead><tr><th>INVOICE</th><th>CUSTOMER</th><th>ISSUED</th><th>DUE DATE</th><th>STATUS</th><th class="right">AMOUNT</th><th>ACTION</th></tr></thead><tbody>
        @for (invoice of filteredInvoices(); track invoice.id) {
          <tr><td><button class="invoice-link" type="button" (click)="store.openInvoiceDetails(invoice)">{{ invoice.number }}</button><small>{{ invoice.items.length }} line {{ invoice.items.length === 1 ? 'item' : 'items' }}</small></td><td>{{ invoice.customerName }}</td><td>{{ invoice.createdAtUtc | date:'d MMM y' }}</td><td [class.late-text]="invoice.overdueDays > 0">{{ invoice.dueAtUtc | date:'d MMM y' }}<small>{{ invoice.overdueDays ? invoice.overdueDays + ' days overdue' : '' }}</small></td><td><span class="status" [class.late]="invoice.overdueDays > 0" [class.paid]="invoice.status === 'Paid'" [class.void-status]="invoice.status === 'Void'">{{ invoice.overdueDays ? 'Overdue' : invoice.status }}</span></td><td class="right amount">{{ invoice.total | currency:invoice.currency:'symbol-narrow':'1.2-2':'en-ZA' }}</td><td><app-invoice-actions [invoice]="invoice" (details)="store.openInvoiceDetails($event)" (paid)="store.markPaid($event)" (voidRequested)="store.openVoidDialog($event)" /></td></tr>
        } @empty { <tr><td colspan="7"><div class="empty"><b>No matching invoices</b><p>Adjust the filters or create a new invoice.</p></div></td></tr> }
      </tbody></table></div>
    </section>
  `
})
export class InvoicesPageComponent {
  readonly store = inject(WorkspaceStore);
  readonly search = signal('');
  readonly statusFilter = signal('all');
  readonly filteredInvoices = computed(() => {
    const term = this.search().trim().toLowerCase();
    return this.store.invoices().filter(invoice =>
      (!term || invoice.number.toLowerCase().includes(term) || invoice.customerName.toLowerCase().includes(term)) &&
      (this.statusFilter() === 'all' || invoice.status.toLowerCase() === this.statusFilter()));
  });

  exportInvoices(): void {
    const rows = [['Invoice', 'Customer', 'Issued', 'Due', 'Status', 'Total'],
      ...this.filteredInvoices().map(invoice => [invoice.number, invoice.customerName, invoice.createdAtUtc,
        invoice.dueAtUtc, invoice.status, invoice.total.toFixed(2)])];
    const csv = rows.map(row => row.map(value => `"${String(value).replaceAll('"', '""')}"`).join(',')).join('\r\n');
    const url = URL.createObjectURL(new Blob([csv], { type: 'text/csv;charset=utf-8' }));
    const link = document.createElement('a');
    link.href = url;
    link.download = 'capitec-invoices.csv';
    link.click();
    URL.revokeObjectURL(url);
  }
}
