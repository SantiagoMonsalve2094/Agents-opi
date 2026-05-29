import { Injectable } from '@angular/core';
import { AgentUserContext } from '../models/agent-user-context.model';

export interface AzureDevOpsUserMessage {
  source?: string;
  type?: string;
  id?: string;
  email?: string;
  displayName?: string;
  backendToken?: string;
}

@Injectable({
  providedIn: 'root'
})
export class AzureDevOpsUserService {
  readonly fallbackUser: AgentUserContext = {
    id: 'local@opi.local',
    email: 'local@opi.local',
    displayName: '',
    backendToken: '',
    isFallback: true
  };

  listen(callback: (user: AgentUserContext) => void): () => void {
    let resolved = false;
    const fallbackTimer = window.setTimeout(() => {
      if (!resolved) {
        resolved = true;
        callback(this.fallbackUser);
      }
    }, 800);

    const handler = (event: MessageEvent): void => {
      const data = event.data as AzureDevOpsUserMessage | undefined;
      if (data?.source !== 'azure-devops-extension' || data.type !== 'opi-agent-user') {
        return;
      }

      resolved = true;
      window.clearTimeout(fallbackTimer);

      const id = data.id?.trim() || data.email?.trim() || this.fallbackUser.id;
      const email = data.email?.trim() || this.buildTechnicalEmail(id);
      callback({
        id,
        email,
        displayName: data.displayName?.trim() ?? '',
        backendToken: data.backendToken?.trim() ?? '',
        isFallback: false
      });
    };

    window.addEventListener('message', handler);
    return () => {
      window.clearTimeout(fallbackTimer);
      window.removeEventListener('message', handler);
    };
  }

  private buildTechnicalEmail(id: string): string {
    const safeId = id.trim().replace(/[^a-zA-Z0-9._-]/g, '_') || 'unknown';
    return `${safeId}@azuredevops.local`;
  }
}
