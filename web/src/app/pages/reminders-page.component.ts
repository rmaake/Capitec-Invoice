import { CommonModule, DatePipe } from '@angular/common';
import { Component, inject } from '@angular/core';
import { PageHeadingComponent } from '../components/page-heading.component';
import { WorkspaceStore } from '../workspace.store';

@Component({
  selector: 'app-reminders-page',
  standalone: true,
  imports: [CommonModule, DatePipe, PageHeadingComponent],
  template: `
    <app-page-heading title="Reminder history" subtitle="A log of simulated overdue reminders. Nothing is delivered.">
      <button class="button secondary" type="button" (click)="store.runSimulation()" [disabled]="store.simulating()">◷ {{ store.simulating() ? 'Running…' : 'Run simulation' }}</button>
    </app-page-heading>
    <section class="panel full-panel">
      <div class="simulation-banner"><span>◷</span><div><strong>Simulated reminders only</strong><p>These are audit records. No email, SMS or other messages are sent.</p></div></div>
      <div class="panel-heading"><div><h2>Reminder log</h2><p>Most recent simulations first</p></div></div>
      <div class="table-wrap"><table><thead><tr><th>INVOICE</th><th>CUSTOMER</th><th>OVERDUE</th><th>POLICY WINDOW</th><th>CHANNEL</th><th>SIMULATED AT</th></tr></thead><tbody>
        @for (item of store.notifications(); track item.id) { <tr><td><strong>{{ item.invoiceNumber }}</strong></td><td>{{ item.customerName }}</td><td><span class="status late">{{ item.overdueDays }} days</span></td><td>Window {{ item.policyWindow + 1 }}</td><td><span class="simulation-tag">{{ item.channel }}</span></td><td>{{ item.simulatedAtUtc | date:'d MMM y, HH:mm' }}</td></tr> }
        @empty { <tr><td colspan="6"><div class="empty"><b>No simulated reminders</b><p>Run a simulation when invoices are overdue.</p></div></td></tr> }
      </tbody></table></div>
    </section>
  `
})
export class RemindersPageComponent {
  readonly store = inject(WorkspaceStore);
}
