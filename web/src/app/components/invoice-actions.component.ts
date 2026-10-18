import { Component, input, output } from '@angular/core';
import { Invoice } from '../api';

@Component({
  selector: 'app-invoice-actions',
  standalone: true,
  template: `
    <div class="row-actions">
      <button class="small-action" type="button" (click)="details.emit(invoice())">Details</button>
      @if (invoice().status === 'Sent') {
        <button class="small-action" type="button" (click)="paid.emit(invoice())">Mark paid</button>
        <button class="small-action danger-action" type="button" (click)="voidRequested.emit(invoice())">Void</button>
      }
    </div>
  `
})
export class InvoiceActionsComponent {
  readonly invoice = input.required<Invoice>();
  readonly details = output<Invoice>();
  readonly paid = output<Invoice>();
  readonly voidRequested = output<Invoice>();
}
