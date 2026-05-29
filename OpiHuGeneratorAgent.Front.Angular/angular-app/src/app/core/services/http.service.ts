import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

export type HttpHeadersMap = Record<string, string>;

@Injectable({
  providedIn: 'root'
})
export class HttpService {
  private readonly http = inject(HttpClient);

  get<TResponse>(url: string, headers?: HttpHeadersMap): Observable<TResponse> {
    return this.http.get<TResponse>(url, { headers });
  }

  delete<TResponse>(url: string, headers?: HttpHeadersMap): Observable<TResponse> {
    return this.http.delete<TResponse>(url, { headers });
  }
}
