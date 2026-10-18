import { TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { provideRouter } from '@angular/router';
import { signal } from '@angular/core';
import { App } from './app';
import { routes } from './app.routes';
import { WorkspaceStore } from './workspace.store';

const storeStub = {
  invoices: signal([]),
  customers: signal([]),
  notifications: signal([]),
  dashboard: signal({ invoiceCount: 0, overdueCount: 0, customerCount: 0, outstandingTotal: 0, currency: 'ZAR' }),
  pageFeedback: signal(null),
  simulating: signal(false),
  dialog: signal(null),
  saving: signal(false),
  detailsLoading: signal(false),
  detailsError: signal(null),
  invoiceDetails: signal(null),
  invoiceToVoid: null,
  invoiceDraft: { customerId: 0, dueDate: '', currency: 'ZAR', notes: '', items: [] },
  customerDraft: { name: '', email: '', phone: '' },
  voidReason: '',
  invoiceCountForCustomer: () => 0,
  runSimulation: jasmine.createSpy('runSimulation'),
  openInvoiceDialog: jasmine.createSpy('openInvoiceDialog'),
  openCustomerDialog: jasmine.createSpy('openCustomerDialog'),
  openInvoiceDetails: jasmine.createSpy('openInvoiceDetails'),
  markPaid: jasmine.createSpy('markPaid'),
  openVoidDialog: jasmine.createSpy('openVoidDialog'),
  closeDialog: jasmine.createSpy('closeDialog'),
  submitCustomer: jasmine.createSpy('submitCustomer'),
  submitInvoice: jasmine.createSpy('submitInvoice'),
  confirmVoid: jasmine.createSpy('confirmVoid'),
  dialogError: signal(null)
};

describe('App workspace shell and routes', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [App],
      providers: [provideRouter(routes), { provide: WorkspaceStore, useValue: storeStub }]
    }).compileComponents();
  });

  it('renders the workspace shell and primary navigation', async () => {
    const fixture = TestBed.createComponent(App);
    fixture.detectChanges();
    await fixture.whenStable();
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('.brand-copy strong')?.textContent).toContain('capitec');
    expect(compiled.querySelectorAll('.nav-item').length).toBe(4);
    expect(compiled.querySelector('router-outlet')).not.toBeNull();
  });

  it('routes each main view to its own page component', async () => {
    const fixture = TestBed.createComponent(App);
    fixture.detectChanges();
    await fixture.whenStable();
    const router = TestBed.inject(Router);

    const pages = [
      ['/overview', 'Invoice overview'],
      ['/invoices', 'Invoices'],
      ['/customers', 'Customers'],
      ['/reminders', 'Reminder history']
    ] as const;

    for (const [url, heading] of pages) {
      await router.navigateByUrl(url);
      fixture.detectChanges();
      await fixture.whenStable();
      fixture.detectChanges();
      expect(fixture.nativeElement.querySelector('h1')?.textContent).toContain(heading);
    }
  });
});
