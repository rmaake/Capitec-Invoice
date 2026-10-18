import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { finalize } from 'rxjs';
import { ApiFeedbackService } from './api-feedback';

export const apiLoadingInterceptor: HttpInterceptorFn = (request, next) => {
  if (!request.url.startsWith('/api/')) return next(request);

  const feedback = inject(ApiFeedbackService);
  feedback.requestStarted();
  return next(request).pipe(finalize(() => feedback.requestFinished()));
};
