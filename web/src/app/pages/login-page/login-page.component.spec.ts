import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { Component } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { apiLoadingInterceptor } from '../../api-loading.interceptor';
import { LoginPageComponent } from './login-page.component';

@Component({ standalone: true, template: '' })
class LoginDestinationComponent {}

describe('LoginPageComponent', () => {
  let http: HttpTestingController;
  let router: Router;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [LoginPageComponent],
      providers: [
        provideHttpClient(withInterceptors([apiLoadingInterceptor])),
        provideHttpClientTesting(),
        provideRouter([{ path: 'overview', component: LoginDestinationComponent }])
      ]
    }).compileComponents();
    http = TestBed.inject(HttpTestingController);
    router = TestBed.inject(Router);
  });

  afterEach(() => http.verify());

  it('posts the credentials and remember-me choice, then opens the workspace', async () => {
    const fixture = TestBed.createComponent(LoginPageComponent);
    const component = fixture.componentInstance;
    component.email.set('owner@example.com');
    component.password.set('example-password');
    component.rememberMe.set(true);
    fixture.detectChanges();

    const submission = component.submit();
    const request = http.expectOne('/api/auth/login');
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelector('.loading-overlay')).not.toBeNull();
    expect(request.request.method).toBe('POST');
    expect(request.request.body).toEqual({
      email: 'owner@example.com',
      password: 'example-password',
      rememberMe: true
    });
    request.flush(null);
    await submission;
    fixture.detectChanges();

    expect(router.url).toBe('/overview');
    expect(component.message()).toBe('');
    expect(fixture.nativeElement.querySelector('.loading-overlay')).toBeNull();
  });

  it('shows the API error when credentials are rejected', async () => {
    const fixture = TestBed.createComponent(LoginPageComponent);
    const component = fixture.componentInstance;
    component.email.set('owner@example.com');
    component.password.set('incorrect');

    const submission = component.submit();
    const request = http.expectOne('/api/auth/login');
    request.flush('Invalid credentials.', { status: 401, statusText: 'Unauthorized' });
    await submission;

    expect(router.url).toBe('/');
    expect(component.message()).toBe('Invalid credentials.');
  });
});