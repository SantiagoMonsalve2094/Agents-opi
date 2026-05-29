import { Injectable } from '@angular/core';
import { HttpErrorMessage } from '../constants/http.constants';

export interface HttpStreamRequest {
  url: string;
  method: string;
  headers: Record<string, string>;
  body: unknown;
  signal: AbortSignal;
}

export interface HttpStreamResponse {
  headers: Headers;
}

@Injectable({
  providedIn: 'root'
})
export class HttpStreamService {
  async stream(request: HttpStreamRequest, onChunk: (chunk: string) => void): Promise<HttpStreamResponse> {
    const response = await fetch(request.url, {
      method: request.method,
      headers: request.headers,
      signal: request.signal,
      body: JSON.stringify(request.body)
    });

    if (!response.ok || !response.body) {
      const errorText = await response.text();
      throw new Error(this.extractError(errorText));
    }

    const reader = response.body.getReader();
    const decoder = new TextDecoder();
    await this.readStream(reader, decoder, onChunk);

    return { headers: response.headers };
  }

  private async readStream(
    reader: ReadableStreamDefaultReader<Uint8Array>,
    decoder: TextDecoder,
    onChunk: (chunk: string) => void
  ): Promise<void> {
    const result = await reader.read();
    if (result.done) {
      const tail = decoder.decode();
      if (tail) {
        onChunk(tail);
      }

      return;
    }

    const chunk = decoder.decode(result.value, { stream: true });
    if (chunk) {
      onChunk(chunk);
    }

    await this.readStream(reader, decoder, onChunk);
  }

  private extractError(value: string): string {
    try {
      const parsed = JSON.parse(value) as { error?: string };
      return parsed.error || HttpErrorMessage.defaultGenerationError;
    } catch {
      return value || HttpErrorMessage.defaultGenerationError;
    }
  }
}
