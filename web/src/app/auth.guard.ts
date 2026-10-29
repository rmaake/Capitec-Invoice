import { inject } from '@angular/core';
import { CanActivateChildFn, Router } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { InvoiceApi } from './api';

export const authGuard: CanActivateChildFn = async () => {
  const api = inject(InvoiceApi);
  const router = inject(Router);

  try {
    const session = await firstValueFrom(api.getAuthSession());
    return session.authenticated ? true : router.createUrlTree(['/login']);
  } catch {
    return router.createUrlTree(['/login']);
  }
};