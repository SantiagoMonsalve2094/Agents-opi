import { Injectable, inject } from '@angular/core';
import { AgentUserHeadersService } from '../../../core/services/agent-user-headers.service';
import { AgentUserContext } from '../../../core/models/agent-user-context.model';
import { environment } from '../../../../environments/environment';
import { HttpContentType, HttpHeaderName, HttpMethod } from '../../../core/constants/http.constants';
import { HttpStreamService } from '../../../core/services/http-stream.service';
import { AgentPhase } from '../models/agent-phase.model';
import { AgentApiConstants } from '../constants/agent-api.constants';

export interface AgentPhaseStreamRequest {
  phase: AgentPhase;
  input: string;
  previousOutput: string;
  conversationId?: string;
  user: AgentUserContext;
  signal: AbortSignal;
}

export interface AgentPhaseFeedbackStreamRequest {
  phase: AgentPhase;
  feedback: string;
  conversationId: string;
  user: AgentUserContext;
  signal: AbortSignal;
}

export interface AgentPhaseStreamResponse {
  conversationId?: string;
}

@Injectable({
  providedIn: 'root'
})
export class AgentPhaseService {
  private readonly httpStream = inject(HttpStreamService);
  private readonly userHeaders = inject(AgentUserHeadersService);
  private readonly apiUrl = `${environment.apiBaseUrl}${environment.endpoints.streamPhase}`;
  private readonly feedbackApiUrl = `${environment.apiBaseUrl}${environment.endpoints.streamFeedbackPhase}`;

  async stream(request: AgentPhaseStreamRequest, onChunk: (chunk: string) => void): Promise<AgentPhaseStreamResponse> {
    const response = await this.httpStream.stream({
      url: this.apiUrl,
      method: HttpMethod.post,
      headers: {
        [HttpHeaderName.contentType]: HttpContentType.json,
        ...this.userHeaders.build(request.user)
      },
      signal: request.signal,
      body: {
        agentType: AgentApiConstants.agentType,
        phase: request.phase,
        input: request.input,
        previousOutput: request.previousOutput,
        conversationId: request.conversationId
      }
    }, onChunk);

    const conversationId = response.headers.get(HttpHeaderName.conversationId) ?? request.conversationId;
    return { conversationId: conversationId ?? undefined };
  }

  async streamFeedback(
    request: AgentPhaseFeedbackStreamRequest,
    onChunk: (chunk: string) => void
  ): Promise<AgentPhaseStreamResponse> {
    const response = await this.httpStream.stream({
      url: this.feedbackApiUrl,
      method: HttpMethod.post,
      headers: {
        [HttpHeaderName.contentType]: HttpContentType.json,
        ...this.userHeaders.build(request.user)
      },
      signal: request.signal,
      body: {
        agentType: AgentApiConstants.agentType,
        phase: request.phase,
        feedback: request.feedback,
        conversationId: request.conversationId
      }
    }, onChunk);

    const conversationId = response.headers.get(HttpHeaderName.conversationId) ?? request.conversationId;
    return { conversationId: conversationId ?? undefined };
  }
}
