import {
  HttpEvent,
  HttpHandler,
  HttpInterceptor,
  HttpRequest,
} from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { CognitoService } from './cognito.service';

@Injectable({
  providedIn: 'root',
})
export class HttpInterceptorService implements HttpInterceptor {
  constructor(private cognitoService: CognitoService) {}

  intercept(
    request: HttpRequest<any>,
    next: HttpHandler
  ): Observable<HttpEvent<any>> {
    const modifiedRequest = request.clone({
      setHeaders: {
        Authorization: `Bearer ${localStorage.getItem('cognitoIdToken')}`,
      },
    });

    // Pass the modified request to the next interceptor or the HTTP handler
    return next.handle(modifiedRequest);
  }
}
