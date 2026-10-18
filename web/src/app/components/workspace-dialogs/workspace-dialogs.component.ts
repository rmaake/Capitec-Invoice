import { CommonModule, CurrencyPipe, DatePipe } from '@angular/common';
import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { WorkspaceStore } from '../../workspace.store';

@Component({
  selector: 'app-workspace-dialogs',
  standalone: true,
  imports: [CommonModule, FormsModule, CurrencyPipe, DatePipe],
  templateUrl: './workspace-dialogs.component.html'
})
export class WorkspaceDialogsComponent {
  readonly store = inject(WorkspaceStore);
  get draftTotal(): number { return this.store.invoiceDraft.items.reduce((sum, item) => sum + item.quantity * item.unitPrice, 0); }
  addLine(): void { this.store.invoiceDraft.items.push({ description: '', quantity: 1, unitPrice: 0 }); }
  removeLine(index: number): void { if (this.store.invoiceDraft.items.length > 1) this.store.invoiceDraft.items.splice(index, 1); }
}
