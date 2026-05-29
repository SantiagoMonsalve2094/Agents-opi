import { Injectable } from '@angular/core';
import { AgentUserContext } from '../models/agent-user-context.model';
import { HttpAuthScheme, HttpHeaderName } from '../constants/http.constants';

@Injectable({
  providedIn: 'root'
})
export class AgentUserHeadersService {
  build(user: AgentUserContext): Record<string, string> {
    const headers: Record<string, string> = {
      [HttpHeaderName.azureUserId]: this.encodeHeaderValue(user.id),
      [HttpHeaderName.azureUserEmail]: this.encodeHeaderValue(user.email),
      [HttpHeaderName.azureUserName]: this.encodeHeaderValue(user.displayName)
    };

    if (user.backendToken) {
      headers[HttpHeaderName.authorization] = `${HttpAuthScheme.bearer} ${user.backendToken}`;
    }

    return headers;
  }

  private encodeHeaderValue(value: string): string {
    return encodeURIComponent(value ?? '');
  }
}
