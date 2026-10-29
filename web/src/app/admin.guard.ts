import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthSessionStore } from './auth-session.store';

export const adminGuard: CanActivateFn = () => {
  const session = inject(AuthSessionStore);
  const router = inject(Router);
  return session.isAdmin() ? true : router.createUrlTree(['/overview']);
};