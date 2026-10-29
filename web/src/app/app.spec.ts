import { provideHttpClient } from '@angular/common/http';
import { TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { provideRouter } from '@angular/router';
import { signal } from '@angular/core';
import { of } from 'rxjs';
import { InvoiceApi } from './api';
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

describe('App login and workspace routes', () => {
  const apiStub = {
    getAuthSession: () => of({ authenticated: true, roles: ['Admin'], firstName: 'Alex', lastName: 'Mokoena', email: 'alex@example.test' }),
    logout: jasmine.createSpy('logout').and.returnValue(of(void 0))
  };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [App],
      providers: [
        provideHttpClient(),
        provideRouter(routes),
        { provide: InvoiceApi, useValue: apiStub },
        { provide: WorkspaceStore, useValue: storeStub }
      ]
    }).compileComponents();
  });

  it('opens on the login page without rendering the workspace shell', async () => {
    const fixture = TestBed.createComponent(App);
    const router = TestBed.inject(Router);
    await router.navigateByUrl('/');
    fixture.detectChanges();
    await fixture.whenStable();
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('#login-title')?.textContent).toContain('Welcome back');
    expect(compiled.querySelector('#login-email')).not.toBeNull();
    expect(compiled.querySelector('.sidebar')).toBeNull();
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

  it('shows the signed-in user initials and signs out to login', async () => {
    const fixture = TestBed.createComponent(App);
    const router = TestBed.inject(Router);
    await router.navigateByUrl('/overview');
    fixture.detectChanges();
    await fixture.whenStable();
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('.profile .avatar')?.textContent?.trim()).toBe('AM');
    expect(compiled.querySelector('.profile-copy strong')?.textContent).toContain('Alex Mokoena');

    compiled.querySelector<HTMLButtonElement>('.profile-signout')?.click();
    await fixture.whenStable();
    fixture.detectChanges();

    expect(apiStub.logout).toHaveBeenCalled();
    expect(router.url).toBe('/login');
  });
});
