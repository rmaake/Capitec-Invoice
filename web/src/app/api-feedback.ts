import { computed, Injectable, signal } from '@angular/core';

export type ApiResultKind = 'success' | 'failure';

export interface ApiResult {
  kind: ApiResultKind;
  title: string;
  message: string;
}

@Injectable({ providedIn: 'root' })
export class ApiFeedbackService {
  private readonly pendingRequests = signal(0);
  readonly loading = computed(() => this.pendingRequests() > 0);
  readonly result = signal<ApiResult | null>(null);

  requestStarted(): void {
    this.result.set(null);
    this.pendingRequests.update(count => count + 1);
  }

  requestFinished(): void {
    this.pendingRequests.update(count => Math.max(0, count - 1));
  }

  showResult(result: ApiResult): void {
    this.result.set(result);
  }

  dismissResult(): void {
    this.result.set(null);
  }
}
