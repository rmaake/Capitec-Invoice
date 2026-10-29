import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

export type InvoiceStatus = 'Sent' | 'Paid' | 'PendingAuthorization' | 'Void';
export interface Customer { id: number; name: string; email: string; phone: string | null; }
export interface InvoiceLine { id: number; description: string; quantity: number; unitPrice: number; total: number; }
export interface Invoice {
  id: number; number: string; customerId: number; customerName: string; createdAtUtc: string;
  dueAtUtc: string; status: InvoiceStatus; currency: string; notes: string | null;
  voidReason: string | null; total: number; overdueDays: number; items: InvoiceLine[];
}
export interface Notification {
  id: number; invoiceId: number; invoiceNumber: string; customerName: string;
  overdueDays: number; simulatedAtUtc: string; channel: string; policyWindow: number;
}
export interface Dashboard { invoiceCount: number; overdueCount: number; customerCount: number; outstandingTotal: number; currency: string; }
export interface CreateInvoice {
  customerId: number; dueDate: string; currency: 'ZAR'; notes: string;
  items: Array<{ description: string; quantity: number; unitPrice: number }>;
}
export interface LoginRequest { email: string; password: string; rememberMe: boolean; }
export interface AuthSession {
  authenticated: boolean; roles: string[]; firstName: string; lastName: string; email: string;
}
export interface ManagedUser {
  id: string; email: string; firstName: string; lastName: string; role: string; isLockedOut: boolean;
}
export interface ManagedUserDraft {
  email: string; firstName: string; lastName: string; role: string;
}

@Injectable({ providedIn: 'root' })
export class InvoiceApi {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = '/api';
  login(value: LoginRequest): Observable<void> { return this.http.post<void>(`${this.baseUrl}/auth/login`, value); }
  getAuthSession(): Observable<AuthSession> { return this.http.get<AuthSession>(`${this.baseUrl}/auth/session`); }
  logout(): Observable<void> { return this.http.post<void>(`${this.baseUrl}/auth/logout`, {}); }
  getManagedUsers(): Observable<ManagedUser[]> { return this.http.get<ManagedUser[]>(`${this.baseUrl}/admin/users`); }
  createManagedUser(value: ManagedUserDraft & { password: string }): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/admin/users`, value);
  }
  updateManagedUser(id: string, value: ManagedUserDraft): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/admin/users/${id}`, value);
  }
  unlockManagedUser(id: string): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/admin/users/${id}/unlock`, {});
  }
  resetManagedUserPassword(id: string, password: string, confirmPassword: string): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/admin/users/${id}/reset-password`, { password, confirmPassword });
  }
  deleteManagedUser(id: string): Observable<void> { return this.http.delete<void>(`${this.baseUrl}/admin/users/${id}`); }
  getDashboard(): Observable<Dashboard> { return this.http.get<Dashboard>(`${this.baseUrl}/dashboard`); }
  getCustomers(): Observable<Customer[]> { return this.http.get<Customer[]>(`${this.baseUrl}/customers`); }
  createCustomer(value: { name: string; email: string; phone: string }): Observable<Customer> {
    return this.http.post<Customer>(`${this.baseUrl}/customers`, value);
  }
  getInvoices(): Observable<Invoice[]> { return this.http.get<Invoice[]>(`${this.baseUrl}/invoices`); }
  getInvoice(id: number): Observable<Invoice> { return this.http.get<Invoice>(`${this.baseUrl}/invoices/${id}`); }
  createInvoice(value: CreateInvoice, key: string): Observable<Invoice> {
    return this.http.post<Invoice>(`${this.baseUrl}/invoices`, value, { headers: new HttpHeaders({ 'Idempotency-Key': key }) });
  }
  setStatus(id: number, status: InvoiceStatus): Observable<Invoice> {
    return this.http.put<Invoice>(`${this.baseUrl}/invoices/${id}/status`, { status });
  }
  markPaid(id: number, proofOfPayment: File): Observable<Invoice> {
    const formData = new FormData();
    formData.append('proofOfPayment', proofOfPayment, proofOfPayment.name);
    return this.http.post<Invoice>(`${this.baseUrl}/invoices/${id}/payment-proof`, formData);
  }
  voidInvoice(id: number, reason: string): Observable<Invoice> {
    return this.http.post<Invoice>(`${this.baseUrl}/invoices/${id}/void`, { reason });
  }
  getNotifications(): Observable<Notification[]> { return this.http.get<Notification[]>(`${this.baseUrl}/notifications`); }
  simulate(): Observable<{ createdCount: number; notifications: Notification[] }> {
    return this.http.post<{ createdCount: number; notifications: Notification[] }>(`${this.baseUrl}/notifications/simulate`, {});
  }
}