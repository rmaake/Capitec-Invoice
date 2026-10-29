import { Component } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { AuthSessionStore } from './auth-session.store';
import { adminGuard } from './admin.guard';

@Component({ standalone: true, selector: 'test-users-page', template: 'Users' })
class UsersTestComponent {}

@Component({ standalone: true, selector: 'test-overview-page', template: 'Overview' })
class OverviewTestComponent {}

describe('adminGuard', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [UsersTestComponent, OverviewTestComponent],
      providers: [provideRouter([
        { path: 'overview', component: OverviewTestComponent },
        { path: 'users', component: UsersTestComponent, canActivate: [adminGuard] }
      ])]
    }).compileComponents();
  });

  it('allows administrators and redirects other roles to overview', async () => {
    const router = TestBed.inject(Router);
    const session = TestBed.inject(AuthSessionStore);

    session.setRoles(['Admin']);
    expect(await router.navigateByUrl('/users')).toBeTrue();
    expect(router.url).toBe('/users');

    session.setRoles(['Manager']);
    await router.navigateByUrl('/overview');
    expect(await router.navigateByUrl('/users')).toBeFalse();
    expect(router.url).toBe('/overview');
  });
});
