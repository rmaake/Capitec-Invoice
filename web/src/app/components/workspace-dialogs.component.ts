import { CommonModule, CurrencyPipe, DatePipe } from '@angular/common';
import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { WorkspaceStore } from '../workspace.store';

@Component({
  selector: 'app-workspace-dialogs',
  standalone: true,
  imports: [CommonModule, FormsModule, CurrencyPipe, DatePipe],
  template: `
    @if (store.dialog() === 'customer' || store.dialog() === 'invoice') {
      <div class="backdrop" role="presentation" (click)="store.closeDialog()"><section class="modal" role="dialog" aria-modal="true" [attr.aria-labelledby]="store.dialog() === 'invoice' ? 'invoice-title' : 'customer-title'" (click)="$event.stopPropagation()">
        <header class="modal-heading"><div><p class="eyebrow">{{ store.dialog() === 'invoice' ? 'NEW RECEIVABLE' : 'CUSTOMER DIRECTORY' }}</p><h2 [id]="store.dialog() === 'invoice' ? 'invoice-title' : 'customer-title'">{{ store.dialog() === 'invoice' ? 'Create invoice' : 'Add a customer' }}</h2><p>{{ store.dialog() === 'invoice' ? 'Invoice details and line items' : 'Add a billing contact to your workspace' }}</p></div><button class="close" type="button" (click)="store.closeDialog()" aria-label="Close dialog">×</button></header>
        @if (store.dialog() === 'customer') {
          <form #customerForm="ngForm" class="form" (ngSubmit)="store.submitCustomer()"><label>Name<input name="name" [(ngModel)]="store.customerDraft.name" required minlength="2" maxlength="160" placeholder="Business or contact name"></label><label>Email address<input name="email" [(ngModel)]="store.customerDraft.email" type="email" required email maxlength="254" placeholder="accounts@example.co.za"></label><label>Phone <small>OPTIONAL</small><input name="phone" [(ngModel)]="store.customerDraft.phone" maxlength="40" type="tel" placeholder="+27 11 555 0100"></label>@if (store.dialogError()) { <p class="form-error" role="alert">{{ store.dialogError() }}</p> }<div class="modal-actions"><button type="button" class="button secondary" (click)="store.closeDialog()">Cancel</button><button class="button primary" [disabled]="customerForm.invalid || store.saving()">{{ store.saving() ? 'Saving…' : 'Save customer' }}</button></div></form>
        } @else {
          <form #invoiceForm="ngForm" class="form" (ngSubmit)="store.submitInvoice()">
            @if (store.customers().length) { <label>Bill to<select name="customerId" [(ngModel)]="store.invoiceDraft.customerId" required><option [ngValue]="0" disabled>Select a customer</option>@for (customer of store.customers(); track customer.id) { <option [ngValue]="customer.id">{{ customer.name }} · {{ customer.email }}</option> }</select></label> } @else { <div class="inline-empty">No customers have been added. <button type="button" class="text-button" (click)="store.openCustomerDialog(true)">Add customer →</button></div> }
            <div class="form-grid"><label>Due date<input name="dueDate" type="date" [(ngModel)]="store.invoiceDraft.dueDate" required></label><label>Currency<input value="ZAR · South African rand" readonly></label></div>
            <div class="line-heading"><strong>Line items</strong><button type="button" class="text-button" (click)="addLine()">＋ Add line</button></div>
            @for (line of store.invoiceDraft.items; track $index; let index = $index) { <div class="line-row"><label>Description<input [name]="'description' + index" [(ngModel)]="line.description" required maxlength="240" placeholder="Work or service"></label><label class="qty">Qty<input [name]="'quantity' + index" [(ngModel)]="line.quantity" type="number" min="0.01" step="0.01" required></label><label class="unit">Unit price<input [name]="'price' + index" [(ngModel)]="line.unitPrice" type="number" min="0.01" step="0.01" required></label><button type="button" class="remove" (click)="removeLine(index)" [disabled]="store.invoiceDraft.items.length === 1" aria-label="Remove line item">×</button></div> }
            <label>Note <small>OPTIONAL</small><textarea name="notes" [(ngModel)]="store.invoiceDraft.notes" maxlength="1000" rows="2" placeholder="A short note for your records"></textarea></label>
            <div class="total"><span>Total due</span><strong>{{ draftTotal | currency:'ZAR':'symbol-narrow':'1.2-2':'en-ZA' }}</strong></div>
            @if (store.dialogError()) { <p class="form-error" role="alert">{{ store.dialogError() }}</p> }
            <div class="modal-actions"><button type="button" class="button secondary" (click)="store.closeDialog()">Cancel</button><button class="button primary" [disabled]="invoiceForm.invalid || !store.customers().length || draftTotal <= 0 || store.saving()">{{ store.saving() ? 'Saving…' : 'Create invoice' }}</button></div>
          </form>
        }
      </section></div>
    }

    @if (store.dialog() === 'details') {
      <div class="backdrop" role="presentation" (click)="store.closeDialog()"><section class="modal details-modal" role="dialog" aria-modal="true" aria-labelledby="details-title" (click)="$event.stopPropagation()">
        <header class="modal-heading"><div><p class="eyebrow">INVOICE DETAILS</p>@if (store.invoiceDetails(); as invoice) { <h2 id="details-title">{{ invoice.number }}</h2><p>{{ invoice.customerName }} · Issued {{ invoice.createdAtUtc | date:'d MMM y' }}</p> } @else { <h2 id="details-title">Invoice</h2><p>Loading invoice details</p> }</div><button class="close" type="button" (click)="store.closeDialog()" aria-label="Close details">×</button></header>
        @if (store.detailsLoading()) { <div class="details-loading" role="status">Loading invoice details…</div> }
        @if (store.detailsError()) { <div class="details-error" role="alert">{{ store.detailsError() }}</div> }
        @if (store.invoiceDetails(); as invoice) {
          <div class="details-body"><div class="details-summary"><div><span>STATUS</span><strong><span class="status" [class.late]="invoice.overdueDays > 0" [class.paid]="invoice.status === 'Paid'" [class.void-status]="invoice.status === 'Void'">{{ invoice.overdueDays > 0 ? 'Overdue' : invoice.status }}</span></strong></div><div><span>DUE DATE</span><strong>{{ invoice.dueAtUtc | date:'d MMMM y' }}</strong></div><div><span>TOTAL DUE</span><strong>{{ invoice.total | currency:invoice.currency:'symbol-narrow':'1.2-2':'en-ZA' }}</strong></div></div>
            @if (invoice.overdueDays > 0) { <p class="detail-overdue">{{ invoice.overdueDays }} days overdue</p> }
            <h3>Line items</h3><div class="table-wrap details-lines"><table><thead><tr><th>DESCRIPTION</th><th class="right">QTY</th><th class="right">UNIT PRICE</th><th class="right">TOTAL</th></tr></thead><tbody>@for (item of invoice.items; track item.id) { <tr><td>{{ item.description }}</td><td class="right">{{ item.quantity }}</td><td class="right">{{ item.unitPrice | currency:invoice.currency:'symbol-narrow':'1.2-2':'en-ZA' }}</td><td class="right amount">{{ item.total | currency:invoice.currency:'symbol-narrow':'1.2-2':'en-ZA' }}</td> }</tbody></table></div>
            @if (invoice.notes) { <section class="detail-note"><h3>Note</h3><p>{{ invoice.notes }}</p></section> }
            @if (invoice.status === 'Void' && invoice.voidReason) { <section class="detail-void-reason"><h3>Void reason</h3><p>{{ invoice.voidReason }}</p></section> }
          </div>
          <footer class="details-actions">@if (invoice.status === 'Sent') { <button class="button secondary" type="button" (click)="store.openVoidDialog(invoice)">Void invoice</button><button class="button primary" type="button" (click)="store.markPaid(invoice)">Mark paid</button> } @else { <span class="terminal-state">{{ invoice.status === 'Paid' ? 'Payment recorded' : 'This invoice is void' }}</span> }<button class="button secondary" type="button" (click)="store.closeDialog()">Close</button></footer>
        }
      </section></div>
    }

    @if (store.dialog() === 'void' && store.invoiceToVoid) {
      <div class="backdrop" role="presentation" (click)="store.closeDialog()"><section class="modal void-modal" role="dialog" aria-modal="true" aria-labelledby="void-title" (click)="$event.stopPropagation()">
        <header class="modal-heading"><div><p class="eyebrow">INVOICE CANCELLATION</p><h2 id="void-title">Void {{ store.invoiceToVoid.number }}?</h2><p>This action is recorded and cannot be undone.</p></div><button class="close" type="button" (click)="store.closeDialog()" aria-label="Close dialog">×</button></header>
        <form class="form" #voidForm="ngForm" (ngSubmit)="store.confirmVoid()"><label>Reason for voiding <small>REQUIRED</small><textarea name="voidReason" [(ngModel)]="store.voidReason" required minlength="3" maxlength="1000" rows="4" placeholder="Explain why this invoice is being cancelled"></textarea></label>@if (store.dialogError()) { <p class="form-error" role="alert">{{ store.dialogError() }}</p> }<div class="modal-actions"><button type="button" class="button secondary" (click)="store.closeDialog()">Keep invoice</button><button type="submit" class="button void-button" [disabled]="voidForm.invalid || !store.voidReason.trim() || store.saving()">{{ store.saving() ? 'Voiding…' : 'Void invoice' }}</button></div></form>
      </section></div>
    }
  `
})
export class WorkspaceDialogsComponent {
  readonly store = inject(WorkspaceStore);
  get draftTotal(): number { return this.store.invoiceDraft.items.reduce((sum, item) => sum + item.quantity * item.unitPrice, 0); }
  addLine(): void { this.store.invoiceDraft.items.push({ description: '', quantity: 1, unitPrice: 0 }); }
  removeLine(index: number): void { if (this.store.invoiceDraft.items.length > 1) this.store.invoiceDraft.items.splice(index, 1); }
}
