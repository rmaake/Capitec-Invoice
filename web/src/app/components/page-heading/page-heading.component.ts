import { Component, input, output } from '@angular/core';

@Component({
  selector: 'app-page-heading',
  standalone: true,
  templateUrl: './page-heading.component.html'
})
export class PageHeadingComponent {
  readonly title = input.required<string>();
  readonly subtitle = input.required<string>();
  readonly eyebrow = input('FINANCE OPERATIONS');
}
