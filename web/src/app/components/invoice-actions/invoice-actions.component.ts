import { Component, input, output } from '@angular/core';
import { Invoice } from '../../api';

@Component({
  selector: 'app-invoice-actions',
  standalone: true,
templateUrl: './invoice-actions.component.html'
})


export class InvoiceActionsComponent {
  readonly invoice = input.required<Invoice>();
  readonly details = output<Invoice>();
  readonly paid = output<Invoice>();
  readonly voidRequested = output<Invoice>();
}
