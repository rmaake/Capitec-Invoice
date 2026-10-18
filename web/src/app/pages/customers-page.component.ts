import { CommonModule } from '@angular/common';
import { Component, computed, inject, signal } from '@angular/core';
import { PageHeadingComponent } from '../components/page-heading.component';
import { WorkspaceStore } from '../workspace.store';

@Component({
  selector: 'app-customers-page',
  standalone: true,
  imports: [CommonModule, PageHeadingComponent],
  template: `
    <app-page-heading title="Customers" subtitle="Manage the businesses and people you bill.">
      <button class="button primary" type="button" (click)="store.openCustomerDialog()">＋ New customer</button>
    </app-page-heading>
    <section class="panel full-panel">
      <div class="panel-heading"><div><h2>Customer directory</h2><p>{{ store.customers().length }} billing contacts</p></div><label class="search"><span>⌕</span><input type="search" [value]="search()" (input)="search.set($any($event.target).value)" placeholder="Search customers" aria-label="Search customers"></label></div>
      <div class="customer-list">
        @for (customer of filteredCustomers(); track customer.id) {
          <div class="customer-row"><span class="customer-mark">{{ customer.name.slice(0, 1).toUpperCase() }}</span><div><strong>{{ customer.name }}</strong><small>{{ customer.email }}</small></div><span class="customer-phone">{{ customer.phone || 'No phone number' }}</span><span class="customer-count">{{ store.invoiceCountForCustomer(customer.id) }} invoices</span><button class="small-action" type="button" (click)="store.openInvoiceDialog(customer.id)">New invoice</button></div>
        } @empty { <div class="empty"><b>No customers found</b><p>Add a billing contact or adjust your search.</p><button class="text-button" type="button" (click)="store.openCustomerDialog()">Add customer →</button></div> }
      </div>
    </section>
  `
})
export class CustomersPageComponent {
  readonly store = inject(WorkspaceStore);
  readonly search = signal('');
  readonly filteredCustomers = computed(() => {
    const term = this.search().trim().toLowerCase();
    return this.store.customers().filter(customer =>
      !term || customer.name.toLowerCase().includes(term) || customer.email.toLowerCase().includes(term));
  });
}
