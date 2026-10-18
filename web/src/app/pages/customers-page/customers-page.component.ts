import { CommonModule } from '@angular/common';
import { Component, computed, inject, signal } from '@angular/core';

import { WorkspaceStore } from '../../workspace.store';
import { PageHeadingComponent } from '../../components/page-heading/page-heading.component';

@Component({
  selector: 'app-customers-page',
  standalone: true,
  imports: [CommonModule, PageHeadingComponent],
  templateUrl: './customers-page.component.html'
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
