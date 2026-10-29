import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { UsersPageComponent } from './users-page.component';

describe('UsersPageComponent', () => {
  let http: HttpTestingController;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [UsersPageComponent],
      providers: [provideHttpClient(), provideHttpClientTesting()]
    }).compileComponents();
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('creates an account and refreshes the active user directory', async () => {
    const fixture = TestBed.createComponent(UsersPageComponent);
    const component = fixture.componentInstance;
    http.expectOne('/api/admin/users').flush([]);
    await fixture.whenStable();

    component.openCreate();
    component.draft = {
      email: 'alex@example.test',
      firstName: 'Alex',
      lastName: 'Mokoena',
      role: 'User',
      password: 'StrongPass1',
      confirmPassword: 'StrongPass1'
    };
    const creation = component.submitDialog();
    const request = http.expectOne('/api/admin/users');
    expect(request.request.method).toBe('POST');
    expect(request.request.body).toEqual({
      email: 'alex@example.test',
      firstName: 'Alex',
      lastName: 'Mokoena',
      role: 'User',
      password: 'StrongPass1'
    });
    request.flush(null);
    await Promise.resolve();
    http.expectOne('/api/admin/users').flush([{
      id: 'user-1',
      email: 'alex@example.test',
      firstName: 'Alex',
      lastName: 'Mokoena',
      role: 'User',
      isLockedOut: false
    }]);
    await creation;

    expect(component.users()).toHaveSize(1);
    expect(component.dialog()).toBeNull();
    expect(component.message()).toBe('User added.');
  });
});
