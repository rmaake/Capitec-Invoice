import { Routes } from '@angular/router';
import { CustomersPageComponent } from './pages/customers-page.component';
import { InvoicesPageComponent } from './pages/invoices-page.component';
import { OverviewPageComponent } from './pages/overview-page.component';
import { RemindersPageComponent } from './pages/reminders-page.component';

export const routes: Routes = [
	{ path: '', pathMatch: 'full', redirectTo: 'overview' },
	{ path: 'overview', component: OverviewPageComponent, title: 'Overview | Capitec Business Invoicing' },
	{ path: 'invoices', component: InvoicesPageComponent, title: 'Invoices | Capitec Business Invoicing' },
	{ path: 'customers', component: CustomersPageComponent, title: 'Customers | Capitec Business Invoicing' },
	{ path: 'reminders', component: RemindersPageComponent, title: 'Reminders | Capitec Business Invoicing' },
	{ path: '**', redirectTo: 'overview' }
];
