import { Routes } from '@angular/router';
import { authGuard } from './auth.guard';
import { LoginPageComponent } from './pages/login-page/login-page.component';
import { WorkspaceShellComponent } from './workspace-shell.component';

export const routes: Routes = [
	{ path: '', pathMatch: 'full', redirectTo: 'login' },
	{ path: 'login', component: LoginPageComponent, title: 'Sign in | Capitec Business Invoicing' },
	{
		path: '',
		component: WorkspaceShellComponent,
		canActivateChild: [authGuard],
		children: [
			{ path: '', pathMatch: 'full', redirectTo: 'overview' },
			{ path: 'overview', loadComponent: () => import('./pages/overview-page/overview-page.component').then(page => page.OverviewPageComponent), title: 'Overview | Capitec Business Invoicing' },
			{ path: 'invoices', loadComponent: () => import('./pages/invoices-page/invoices-page.component').then(page => page.InvoicesPageComponent), title: 'Invoices | Capitec Business Invoicing' },
			{ path: 'customers', loadComponent: () => import('./pages/customers-page/customers-page.component').then(page => page.CustomersPageComponent), title: 'Customers | Capitec Business Invoicing' },
			{ path: 'reminders', loadComponent: () => import('./pages/reminders-page/reminders-page.component').then(page => page.RemindersPageComponent), title: 'Reminders | Capitec Business Invoicing' }
		]
	},
	{ path: '**', redirectTo: 'login' }
];
