import { computed, inject, Injectable, signal } from '@angular/core';
import { firstValueFrom, forkJoin } from 'rxjs';
import { Customer, CreateInvoice, Dashboard, Invoice, InvoiceApi, Notification } from './api';
import { ApiFeedbackService, ApiResultKind } from './api-feedback';

export type WorkspaceDialog = 'customer' | 'invoice' | 'details' | 'void' | 'payment-proof' | null;

@Injectable({ providedIn: 'root' })
export class WorkspaceStore {
  private readonly api = inject(InvoiceApi);
  private readonly feedback = inject(ApiFeedbackService);

  readonly dashboard = signal<Dashboard | null>(null);
  readonly customers = signal<Customer[]>([]);
  readonly invoices = signal<Invoice[]>([]);
  readonly notifications = signal<Notification[]>([]);
  readonly loading = signal(true);
  readonly simulating = signal(false);
  readonly saving = signal(false);
  readonly dialog = signal<WorkspaceDialog>(null);
  readonly invoiceDetails = signal<Invoice | null>(null);
  readonly detailsLoading = signal(false);
  readonly detailsError = signal<string | null>(null);
  readonly dialogError = signal<string | null>(null);
  readonly pageFeedback = signal<{ kind: ApiResultKind; message: string } | null>(null);

  customerDraft = { name: '', email: '', phone: '' };
  invoiceDraft = this.emptyInvoiceDraft();
  invoiceToVoid: Invoice | null = null;
  invoiceToPay: Invoice | null = null;
  paymentProofFile: File | null = null;
  voidReason = '';
  private invoiceIdempotencyKey = globalThis.crypto.randomUUID();
  private customerForInvoice = false;
  private returnToDetailsAfterVoid = false;
  private returnToDetailsAfterPayment = false;

  constructor() { void this.refresh(); }

  async refresh(): Promise<void> {
    this.loading.set(true);
    try {
      this.applyData(await this.fetchAll());
    } catch {
      this.reportFailure('Could not load workspace data', 'The service could not be reached. Check that the API is running, then try again.');
    } finally {
      this.loading.set(false);
    }
  }

  openCustomerDialog(forInvoice = false): void {
    this.customerDraft = { name: '', email: '', phone: '' };
    this.customerForInvoice = forInvoice;
    this.dialogError.set(null);
    this.dialog.set('customer');
  }

  openInvoiceDialog(customerId = this.customers()[0]?.id ?? 0): void {
    this.invoiceDraft = this.emptyInvoiceDraft();
    this.invoiceDraft.customerId = customerId;
    this.invoiceIdempotencyKey = globalThis.crypto.randomUUID();
    this.dialogError.set(null);
    this.dialog.set('invoice');
  }

  openVoidDialog(invoice: Invoice): void {
    this.returnToDetailsAfterVoid = this.dialog() === 'details';
    this.invoiceToVoid = invoice;
    this.voidReason = '';
    this.dialogError.set(null);
    this.dialog.set('void');
  }

  openPaymentProofDialog(invoice: Invoice): void {
    this.returnToDetailsAfterPayment = this.dialog() === 'details';
    this.invoiceToPay = invoice;
    this.paymentProofFile = null;
    this.dialogError.set(null);
    this.dialog.set('payment-proof');
  }

  setPaymentProofFile(file: File | null): void {
    this.paymentProofFile = file;
    this.dialogError.set(null);
  }

  async openInvoiceDetails(invoice: Invoice): Promise<void> {
    this.invoiceDetails.set(null);
    this.detailsError.set(null);
    this.detailsLoading.set(true);
    this.dialog.set('details');
    try {
      const details = await firstValueFrom(this.api.getInvoice(invoice.id));
      if (this.dialog() === 'details') {
        this.invoiceDetails.set(details);
        this.reportSuccess('Invoice details loaded', `${details.number} details are ready.`);
      }
    } catch {
      this.detailsError.set('Invoice details could not be loaded. Close this window and try again.');
      this.reportFailure('Could not load invoice details', this.detailsError()!);
    } finally {
      this.detailsLoading.set(false);
    }
  }

  closeDialog(): void {
    if (this.saving()) return;
    if (this.dialog() === 'payment-proof' && this.returnToDetailsAfterPayment) {
      this.dialog.set('details');
      this.returnToDetailsAfterPayment = false;
      this.invoiceToPay = null;
      this.paymentProofFile = null;
      return;
    }
    if (this.dialog() === 'void' && this.returnToDetailsAfterVoid) {
      this.dialog.set('details');
      this.returnToDetailsAfterVoid = false;
      this.invoiceToVoid = null;
      return;
    }
    this.dialog.set(null);
  }

  async submitCustomer(): Promise<void> {
    this.saving.set(true);
    this.dialogError.set(null);
    try {
      const customer = await firstValueFrom(this.api.createCustomer(this.customerDraft));
      await this.reloadData();
      this.invoiceDraft.customerId = customer.id;
      this.dialog.set(this.customerForInvoice ? 'invoice' : null);
      this.customerForInvoice = false;
      this.reportSuccess('Customer saved', `${customer.name} is ready for invoicing.`);
    } catch {
      const message = 'This customer could not be saved. Check the details and try again.';
      this.dialogError.set(message);
      this.reportFailure('Customer could not be saved', message);
    } finally {
      this.saving.set(false);
    }
  }

  async submitInvoice(): Promise<void> {
    this.saving.set(true);
    this.dialogError.set(null);
    try {
      await firstValueFrom(this.api.createInvoice(this.invoiceDraft, this.invoiceIdempotencyKey));
      await this.reloadData();
      this.dialog.set(null);
      this.reportSuccess('Invoice created', 'The invoice was saved and marked as sent.');
    } catch {
      const message = 'The invoice could not be saved. Your draft is unchanged, so retrying will not create a duplicate.';
      this.dialogError.set(message);
      this.reportFailure('Invoice could not be saved', message);
    } finally {
      this.saving.set(false);
    }
  }

  async markPaid(invoice: Invoice): Promise<void> {
    this.openPaymentProofDialog(invoice);
  }

  async confirmPayment(): Promise<void> {
    if (!this.invoiceToPay || !this.paymentProofFile) {
      this.dialogError.set('Select a proof-of-payment document before continuing.');
      return;
    }

    this.saving.set(true);
    this.dialogError.set(null);
    try {
      const invoice = this.invoiceToPay;
      const updated = await firstValueFrom(this.api.markPaid(invoice.id, this.paymentProofFile));
      await this.reloadData();
      this.invoiceToPay = null;
      this.paymentProofFile = null;
      if (this.returnToDetailsAfterPayment) {
        this.invoiceDetails.set(updated);
        this.dialog.set('details');
        this.returnToDetailsAfterPayment = false;
      } else {
        this.dialog.set(null);
      }
      this.reportSuccess('Invoice marked as paid', `${invoice.number} is now paid.`);
    } catch {
      const message = 'The payment could not be recorded. Check the proof document and try again.';
      this.dialogError.set(message);
      this.reportFailure('Invoice update failed', message);
    } finally {
      this.saving.set(false);
    }
  }

  async confirmVoid(): Promise<void> {
    if (!this.invoiceToVoid || !this.voidReason.trim()) {
      const message = 'A reason is required to void this invoice.';
      this.dialogError.set(message);
      this.reportFailure('Invoice was not voided', message);
      return;
    }
    this.saving.set(true);
    this.dialogError.set(null);
    try {
      await firstValueFrom(this.api.voidInvoice(this.invoiceToVoid.id, this.voidReason.trim()));
      const invoice = this.invoiceToVoid;
      await this.reloadData();
      this.dialog.set(null);
      this.invoiceToVoid = null;
      if (this.returnToDetailsAfterVoid) {
        this.returnToDetailsAfterVoid = false;
        this.invoiceDetails.set(this.invoices().find(item => item.id === invoice.id) ?? null);
        this.dialog.set('details');
      } else {
        this.invoiceDetails.set(null);
      }
      this.reportSuccess('Invoice voided', `${invoice.number} was voided. The reason is saved in the invoice record.`);
    } catch {
      const message = 'The invoice could not be voided. Refresh and try again.';
      this.dialogError.set(message);
      this.reportFailure('Invoice could not be voided', message);
    } finally {
      this.saving.set(false);
    }
  }

  async runSimulation(): Promise<void> {
    this.simulating.set(true);
    try {
      const result = await firstValueFrom(this.api.simulate());
      await this.reloadData();
      this.reportSuccess('Simulation complete', result.createdCount === 0
        ? 'No new overdue reminders were due. Existing reminders were left unchanged.'
        : `${result.createdCount} simulated ${result.createdCount === 1 ? 'reminder was' : 'reminders were'} recorded.`);
    } catch {
      this.reportFailure('Reminder simulation failed', 'The reminder simulation could not run. Please try again.');
    } finally {
      this.simulating.set(false);
    }
  }

  invoiceCountForCustomer(customerId: number): number {
    return this.invoices().filter(invoice => invoice.customerId === customerId).length;
  }

  private async fetchAll() {
    return firstValueFrom(forkJoin({
      dashboard: this.api.getDashboard(),
      customers: this.api.getCustomers(),
      invoices: this.api.getInvoices(),
      notifications: this.api.getNotifications()
    }));
  }

  private async reloadData(): Promise<void> { this.applyData(await this.fetchAll()); }

  private applyData(data: { dashboard: Dashboard; customers: Customer[]; invoices: Invoice[]; notifications: Notification[] }): void {
    this.dashboard.set(data.dashboard);
    this.customers.set(data.customers);
    this.invoices.set(data.invoices);
    this.notifications.set(data.notifications);
  }

  private emptyInvoiceDraft(): CreateInvoice {
    const date = new Date();
    date.setDate(date.getDate() + 14);
    return {
      customerId: 0,
      dueDate: `${date.getFullYear()}-${String(date.getMonth() + 1).padStart(2, '0')}-${String(date.getDate()).padStart(2, '0')}`,
      currency: 'ZAR',
      notes: '',
      items: [{ description: '', quantity: 1, unitPrice: 0 }]
    };
  }

  private reportSuccess(title: string, message: string): void {
    this.pageFeedback.set({ kind: 'success', message });
    this.feedback.showResult({ kind: 'success', title, message });
  }

  private reportFailure(title: string, message: string): void {
    this.pageFeedback.set({ kind: 'failure', message });
    this.feedback.showResult({ kind: 'failure', title, message });
  }
}
