import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { AgentUserContext } from '../../../core/models/agent-user-context.model';
import { AgentUserHeadersService } from '../../../core/services/agent-user-headers.service';
import { HttpService } from '../../../core/services/http.service';
import { environment } from '../../../../environments/environment';
import { ConversationDetail, ConversationSummary } from '../models/conversation.model';
import { AgentApiConstants } from '../constants/agent-api.constants';

@Injectable({
  providedIn: 'root'
})
export class ConversationService {
  private readonly http = inject(HttpService);
  private readonly userHeaders = inject(AgentUserHeadersService);
  private readonly baseUrl = `${environment.apiBaseUrl}${environment.endpoints.conversations}`;
  private readonly agentQuery = `agentType=${AgentApiConstants.agentType}`;

  list(user: AgentUserContext): Observable<ConversationSummary[]> {
    return this.http.get<ConversationSummary[]>(`${this.baseUrl}?${this.agentQuery}`, this.buildHeaders(user));
  }

  getById(id: string, user: AgentUserContext): Observable<ConversationDetail> {
    return this.http.get<ConversationDetail>(`${this.baseUrl}/${id}?${this.agentQuery}`, this.buildHeaders(user));
  }

  delete(id: string, user: AgentUserContext): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}?${this.agentQuery}`, this.buildHeaders(user));
  }

  private buildHeaders(user: AgentUserContext): Record<string, string> {
    return this.userHeaders.build(user);
  }
}
