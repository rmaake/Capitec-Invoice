import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { Component } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { authGuard } from './auth.guard';
import { AuthSessionStore } from './auth-session.store';

@Component({ standalone: true, selector: 'test-login', template: 'Login' })
class LoginTestComponent {}

@Component({ standalone: true, selector: 'test-workspace', template: 'Workspace' })
class WorkspaceTestComponent {}

describe('authGuard', () => {
  let http: HttpTestingController;
  let router: Router;
  let sessionStore: AuthSessionStore;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [LoginTestComponent, WorkspaceTestComponent],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideRouter([
          { path: 'login', component: LoginTestComponent },
          { path: '', canActivateChild: [authGuard], children: [{ path: 'overview', component: WorkspaceTestComponent }] }
        ])
      ]
    }).compileComponents();
    http = TestBed.inject(HttpTestingController);
    router = TestBed.inject(Router);
    sessionStore = TestBed.inject(AuthSessionStore);
  });

  afterEach(() => http.verify());

  it('allows workspace navigation when the server confirms the session', async () => {
    const navigation = router.navigateByUrl('/overview');
    await Promise.resolve();
    http.expectOne('/api/auth/session').flush({ authenticated: true, roles: ['User'] });

    expect(await navigation).toBeTrue();
    expect(router.url).toBe('/overview');
    expect(sessionStore.roles()).toEqual(['User']);
    expect(sessionStore.canAuthorizeVoid()).toBeFalse();
  });

  it('recognizes Admin and Manager roles as void approvers', async () => {
    const navigation = router.navigateByUrl('/overview');
    await Promise.resolve();
    http.expectOne('/api/auth/session').flush({ authenticated: true, roles: ['Manager'] });

    expect(await navigation).toBeTrue();
    expect(sessionStore.canAuthorizeVoid()).toBeTrue();
  });

  it('redirects unauthenticated users to login', async () => {
    const navigation = router.navigateByUrl('/overview');
    await Promise.resolve();
    http.expectOne('/api/auth/session').flush({ authenticated: false });

    expect(await navigation).toBeTrue();
    expect(router.url).toBe('/login');
  });

  it('fails closed and redirects to login if session verification fails', async () => {
    const navigation = router.navigateByUrl('/overview');
    await Promise.resolve();
    http.expectOne('/api/auth/session').error(new ProgressEvent('network error'));

    expect(await navigation).toBeTrue();
    expect(router.url).toBe('/login');
  });
});