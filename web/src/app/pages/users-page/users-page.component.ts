import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { firstValueFrom } from 'rxjs';
import { InvoiceApi, ManagedUser, ManagedUserDraft } from '../../api';
import { PageHeadingComponent } from '../../components/page-heading/page-heading.component';

type UserDialog = 'create' | 'edit' | 'reset' | null;

@Component({
  selector: 'app-users-page',
  standalone: true,
  imports: [CommonModule, FormsModule, PageHeadingComponent],
  templateUrl: './users-page.component.html'
})
export class UsersPageComponent {
  private readonly api = inject(InvoiceApi);
  readonly users = signal<ManagedUser[]>([]);
  readonly loading = signal(true);
  readonly saving = signal(false);
  readonly search = signal('');
  readonly dialog = signal<UserDialog>(null);
  readonly selectedUser = signal<ManagedUser | null>(null);
  readonly message = signal('');
  readonly messageKind = signal<'success' | 'error'>('success');
  readonly filteredUsers = computed(() => {
    const term = this.search().trim().toLowerCase();
    return this.users().filter(user =>
      !term || `${user.firstName} ${user.lastName} ${user.email} ${user.role}`.toLowerCase().includes(term));
  });

  draft: ManagedUserDraft & { password: string; confirmPassword: string } = this.emptyDraft();

  constructor() { void this.loadUsers(); }

  async loadUsers(): Promise<void> {
    this.loading.set(true);
    try {
      this.users.set(await firstValueFrom(this.api.getManagedUsers()));
    } catch (error) {
      this.showError(error, 'Users could not be loaded. Try again.');
    } finally {
      this.loading.set(false);
    }
  }

  openCreate(): void {
    this.selectedUser.set(null);
    this.draft = this.emptyDraft();
    this.message.set('');
    this.dialog.set('create');
  }

  openEdit(user: ManagedUser): void {
    this.selectedUser.set(user);
    this.draft = { email: user.email, firstName: user.firstName, lastName: user.lastName, role: user.role, password: '', confirmPassword: '' };
    this.message.set('');
    this.dialog.set('edit');
  }

  openReset(user: ManagedUser): void {
    this.selectedUser.set(user);
    this.draft = this.emptyDraft();
    this.message.set('');
    this.dialog.set('reset');
  }

  closeDialog(): void {
    if (!this.saving()) this.dialog.set(null);
  }

  async submitDialog(): Promise<void> {
    if (this.saving()) return;
    const mode = this.dialog();
    const user = this.selectedUser();
    if (!mode) return;
    if ((mode === 'edit' || mode === 'reset') && !user) return;
    if ((mode === 'create' || mode === 'reset') && this.draft.password !== this.draft.confirmPassword) {
      this.messageKind.set('error');
      this.message.set('Passwords do not match.');
      return;
    }

    this.saving.set(true);
    this.message.set('');
    try {
      if (mode === 'create') {
        await firstValueFrom(this.api.createManagedUser({ ...this.profileDraft(), password: this.draft.password }));
      } else if (mode === 'edit' && user) {
        await firstValueFrom(this.api.updateManagedUser(user.id, this.profileDraft()));
      } else if (mode === 'reset' && user) {
        await firstValueFrom(this.api.resetManagedUserPassword(user.id, this.draft.password, this.draft.confirmPassword));
      }
      this.dialog.set(null);
      this.reportSuccess(mode === 'reset' ? 'Password reset.' : mode === 'create' ? 'User added.' : 'User updated.');
      await this.loadUsers();
    } catch (error) {
      this.showError(error, 'The change could not be saved. Check the details and try again.');
    } finally {
      this.saving.set(false);
    }
  }

  async unlock(user: ManagedUser): Promise<void> {
    try {
      await firstValueFrom(this.api.unlockManagedUser(user.id));
      this.reportSuccess(`${user.firstName} ${user.lastName} can sign in again.`);
      await this.loadUsers();
    } catch (error) {
      this.showError(error, 'This account could not be unlocked. Try again.');
    }
  }

  async delete(user: ManagedUser): Promise<void> {
    if (!globalThis.confirm(`Remove ${user.firstName} ${user.lastName}? Their account will be deactivated.`)) return;
    try {
      await firstValueFrom(this.api.deleteManagedUser(user.id));
      this.reportSuccess(`${user.firstName} ${user.lastName} was removed.`);
      await this.loadUsers();
    } catch (error) {
      this.showError(error, 'This account could not be removed. Try again.');
    }
  }

  private profileDraft(): ManagedUserDraft {
    return {
      email: this.draft.email.trim(),
      firstName: this.draft.firstName.trim(),
      lastName: this.draft.lastName.trim(),
      role: this.draft.role
    };
  }

  private emptyDraft(): ManagedUserDraft & { password: string; confirmPassword: string } {
    return { email: '', firstName: '', lastName: '', role: 'User', password: '', confirmPassword: '' };
  }

  private reportSuccess(message: string): void {
    this.messageKind.set('success');
    this.message.set(message);
  }

  private showError(error: unknown, fallback: string): void {
    let message = fallback;
    if (error instanceof HttpErrorResponse) {
      const details = error.error;
      if (typeof details === 'string' && details.trim()) message = details;
      else if (typeof details?.message === 'string') message = details.message;
      else if (Array.isArray(details) && details[0]?.description) message = details[0].description;
      else if (typeof details?.title === 'string') message = details.title;
    }
    this.messageKind.set('error');
    this.message.set(message);
  }
}
