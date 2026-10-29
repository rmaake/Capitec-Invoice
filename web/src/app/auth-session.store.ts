import { Injectable, signal } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class AuthSessionStore {
  readonly roles = signal<string[]>([]);

  setRoles(roles: string[]): void {
    this.roles.set(roles);
  }

  canAuthorizeVoid(): boolean {
    return this.roles().some(role => role === 'Admin' || role === 'Manager');
  }
}