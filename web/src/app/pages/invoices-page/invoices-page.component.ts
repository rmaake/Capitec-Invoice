import { CommonModule, CurrencyPipe, DatePipe } from '@angular/common';
import { Component, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { InvoiceActionsComponent } from '../../components/invoice-actions/invoice-actions.component';
import { PageHeadingComponent } from '../../components/page-heading/page-heading.component';
import { WorkspaceStore } from '../../workspace.store';

@Component({
  selector: 'app-invoices-page',
  standalone: true,
  imports: [CommonModule, FormsModule, CurrencyPipe, DatePipe, PageHeadingComponent, InvoiceActionsComponent],
  templateUrl: './invoices-page.component.html'
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
