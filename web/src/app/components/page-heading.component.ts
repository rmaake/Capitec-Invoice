import { Component, input, output } from '@angular/core';

@Component({
  selector: 'app-page-heading',
  standalone: true,
  template: `
    <div class="page-heading">
      <div>
        <p class="eyebrow">{{ eyebrow() }}</p>
        <h1>{{ title() }}</h1>
        <p class="subtitle">{{ subtitle() }}</p>
      </div>
      <div class="heading-actions"><ng-content /></div>
    </div>
  `
})
export class PageHeadingComponent {
  readonly title = input.required<string>();
  readonly subtitle = input.required<string>();
  readonly eyebrow = input('FINANCE OPERATIONS');
}
