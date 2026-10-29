import { inject } from '@angular/core';
import { CanActivateChildFn, Router } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { InvoiceApi } from './api';
import { AuthSessionStore } from './auth-session.store';

export const authGuard: CanActivateChildFn = async () => {
  const api = inject(InvoiceApi);
  const sessionStore = inject(AuthSessionStore);
  const router = inject(Router);

  try {
    const session = await firstValueFrom(api.getAuthSession());
    if (session.authenticated) {
      sessionStore.setSession(session);
      return true;
    }
    sessionStore.clear();
    return router.createUrlTree(['/login']);
  } catch {
    sessionStore.clear();
    return router.createUrlTree(['/login']);
  }
};