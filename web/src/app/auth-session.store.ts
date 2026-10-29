import { Injectable, signal } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class AuthSessionStore {
  readonly roles = signal<string[]>([]);
  readonly profile = signal({ firstName: '', lastName: '', email: '' });

  setRoles(roles: string[]): void {
    this.roles.set(roles);
  }

  setSession(session: { roles: string[]; firstName: string; lastName: string; email: string }): void {
    this.roles.set(session.roles ?? []);
    this.profile.set({ firstName: session.firstName ?? '', lastName: session.lastName ?? '', email: session.email ?? '' });
  }

  clear(): void {
    this.roles.set([]);
    this.profile.set({ firstName: '', lastName: '', email: '' });
  }

  initials(): string {
    const profile = this.profile();
    const nameInitials = `${profile.firstName.trim().charAt(0)}${profile.lastName.trim().charAt(0)}`.toUpperCase();
    if (nameInitials) return nameInitials;
    return profile.email.trim().charAt(0).toUpperCase() || '?';
  }

  canAuthorizeVoid(): boolean {
    return this.roles().some(role => role === 'Admin' || role === 'Manager');
  }

  isAdmin(): boolean {
    return this.roles().includes('Admin');
  }
}