import { DatePipe } from '@angular/common';
import { ChangeDetectorRef, Component, ElementRef, OnDestroy, OnInit, ViewChild, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Store } from '@ngrx/store';
import { firstValueFrom } from 'rxjs';
import { AgentUserContext } from '../../../../core/models/agent-user-context.model';
import { AzureDevOpsUserService } from '../../../../core/services/azure-devops-user.service';
import { ClipboardService } from '../../../../core/services/clipboard.service';
import { AgentTiming, AgentUiText } from '../../constants/agent-ui.constants';
import { AGENT_PHASE_ACTIONS, AgentPhase } from '../../models/agent-phase.model';
import { ConversationDetail, ConversationMessage, ConversationSummary } from '../../models/conversation.model';
import { AgentPhaseService } from '../../services/agent-phase.service';
import { ConversationService } from '../../services/conversation.service';
import { MarkdownRendererService } from '../../services/markdown-renderer.service';
import { selectConversation, setConversations } from '../../store/agent.actions';
import { AgentOutputValidator } from '../../validators/agent-output.validator';

interface PhaseState {
  output: string;
  renderedOutput: string;
  isStale: boolean;
  completedAt?: string;
}

interface TimelineMessage {
  id: string;
  role: 'user' | 'agent' | 'system';
  type: string;
  phase?: AgentPhase;
  content: string;
  renderedContent: string;
  createdAt: string;
  isStreaming?: boolean;
}

@Component({
  selector: 'app-agent-workspace',
  imports: [FormsModule, DatePipe],
  templateUrl: './agent-workspace.html',
  styleUrl: './agent-workspace.css'
})
export class AgentWorkspace implements OnInit, OnDestroy {
  @ViewChild('chatBody') private chatBody?: ElementRef<HTMLElement>;

  private readonly agentPhaseService = inject(AgentPhaseService);
  private readonly azureDevOpsUserService = inject(AzureDevOpsUserService);
  private readonly changeDetector = inject(ChangeDetectorRef);
  private readonly clipboardService = inject(ClipboardService);
  private readonly conversationService = inject(ConversationService);
  private readonly markdownRenderer = inject(MarkdownRendererService);
  private readonly outputValidator = inject(AgentOutputValidator);
  private readonly store = inject(Store);
  readonly phaseActions = AGENT_PHASE_ACTIONS;
  readonly uiText = AgentUiText;

  private readonly phases = AGENT_PHASE_ACTIONS.map(action => action.phase);
  private phaseStates = this.createEmptyPhaseStates();
  private activeController?: AbortController;
  private stopListeningForUser: () => void = () => undefined;
  private user: AgentUserContext = this.azureDevOpsUserService.fallbackUser;
  private isUserReady = false;

  input = '';
  composerText = '';
  currentPhase: AgentPhase = this.phases[0];
  feedbackPhase?: AgentPhase;
  stalePromptPhase?: AgentPhase;
  completedPhase?: AgentPhase;
  currentConversationId?: string;
  conversations: ConversationSummary[] = [];
  messages: TimelineMessage[] = [];
  searchTerm = '';
  statusText = '';
  copyInputText: string = AgentUiText.copyInput;
  copyOutputText: string = AgentUiText.copyOutput;
  isLoading = false;
  isHistoryLoading = false;
  errorMessage = '';
  userDisplayName = '';

  get greeting(): string {
    return this.userDisplayName ? `Hola, ${this.userDisplayName}` : 'Hola';
  }

  get filteredConversations(): ConversationSummary[] {
    const query = this.searchTerm.trim().toLowerCase();
    if (!query) {
      return this.conversations;
    }

    return this.conversations.filter(conversation => conversation.title.toLowerCase().includes(query));
  }

  get composerPlaceholder(): string {
    return this.feedbackPhase ? AgentUiText.feedbackPlaceholder : AgentUiText.inputPlaceholder;
  }

  get canSend(): boolean {
    return !this.isLoading && this.isUserReady && this.composerText.trim().length > 0;
  }

  ngOnInit(): void {
    this.stopListeningForUser = this.azureDevOpsUserService.listen(user => {
      this.user = user;
      this.isUserReady = true;
      this.userDisplayName = user.displayName;
      void this.loadConversations();
      this.changeDetector.detectChanges();
    });
  }

  ngOnDestroy(): void {
    this.stopListeningForUser();
    this.activeController?.abort();
  }

  async submitComposer(): Promise<void> {
    if (this.feedbackPhase) {
      await this.sendFeedback();
      return;
    }

    await this.generate(this.currentPhase);
  }

  async generate(phase: AgentPhase, forceRegenerate = false): Promise<void> {
    if (!this.isUserReady) {
      this.errorMessage = AgentUiText.loadingUser;
      return;
    }

    if (!this.input.trim() && !this.composerText.trim()) {
      this.errorMessage = AgentUiText.missingInput;
      return;
    }

    this.selectPhase(phase, false);
    const state = this.phaseStates[phase];
    if (state.output.trim() && state.isStale && !forceRegenerate) {
      this.stalePromptPhase = phase;
      return;
    }

    if (state.output.trim() && !forceRegenerate) {
      return;
    }

    if (!this.input.trim()) {
      this.input = this.composerText.trim();
      this.addLocalMessage('user', 'InitialInput', undefined, this.input);
      this.composerText = '';
    }

    const phaseLabel = this.getPhaseLabel(phase);
    const controller = new AbortController();
    const agentMessage = this.addLocalMessage('agent', 'PhaseOutput', phase, '', true);
    let firstChunk = true;
    this.activeController = controller;
    this.startGeneration(phaseLabel);

    try {
      const result = await this.agentPhaseService.stream(
        {
          phase,
          input: this.input,
          previousOutput: this.buildPreviousOutput(phase),
          conversationId: this.currentConversationId,
          user: this.user,
          signal: controller.signal
        },
        chunk => this.appendChunk(agentMessage, chunk, controller, first => firstChunk = first)
      );

      this.currentConversationId = result.conversationId ?? this.currentConversationId;
      this.store.dispatch(selectConversation({ conversationId: this.currentConversationId }));
      this.finishPhaseGeneration(phase, agentMessage.content);
      await this.afterConversationMutation();
    } catch (error) {
      this.handleGenerationError(error);
    } finally {
      this.finishLoading();
    }
  }

  async sendFeedback(): Promise<void> {
    if (!this.feedbackPhase || !this.currentConversationId) {
      return;
    }

    const feedback = this.composerText.trim();
    if (!feedback) {
      this.errorMessage = AgentUiText.missingInput;
      return;
    }

    const phase = this.feedbackPhase;
    this.selectPhase(phase, false);
    this.addLocalMessage('user', 'Feedback', phase, feedback);
    this.composerText = '';

    const controller = new AbortController();
    const agentMessage = this.addLocalMessage('agent', 'RefinedPhaseOutput', phase, '', true);
    let firstChunk = true;
    this.activeController = controller;
    this.startGeneration(this.getPhaseLabel(phase));

    try {
      await this.agentPhaseService.streamFeedback(
        {
          phase,
          feedback,
          conversationId: this.currentConversationId,
          user: this.user,
          signal: controller.signal
        },
        chunk => this.appendChunk(agentMessage, chunk, controller, first => firstChunk = first)
      );

      this.finishPhaseGeneration(phase, agentMessage.content);
      this.markLaterPhasesStale(phase);
      this.feedbackPhase = undefined;
      await this.afterConversationMutation();
    } catch (error) {
      this.handleGenerationError(error);
    } finally {
      this.finishLoading();
    }
  }

  requestFeedback(phase: AgentPhase): void {
    if (!this.phaseStates[phase].output.trim()) {
      return;
    }

    this.feedbackPhase = phase;
    this.currentPhase = phase;
    this.composerText = '';
    this.errorMessage = '';
  }

  async approveAndContinue(phase: AgentPhase): Promise<void> {
    const nextPhase = this.getNextPhase(phase);
    if (!nextPhase) {
      return;
    }

    await this.generate(nextPhase);
  }

  selectPhase(phase: AgentPhase, promptIfStale = true): void {
    if (!this.canUsePhase(phase)) {
      return;
    }

    this.currentPhase = phase;
    this.feedbackPhase = undefined;
    this.errorMessage = '';
    this.stalePromptPhase = promptIfStale && this.phaseStates[phase].isStale ? phase : undefined;
  }

  async resolveStalePhase(regenerate: boolean): Promise<void> {
    const phase = this.stalePromptPhase;
    this.stalePromptPhase = undefined;
    if (phase && regenerate) {
      await this.generate(phase, true);
    }
  }

  newChat(): void {
    this.clear();
    this.currentConversationId = undefined;
    this.store.dispatch(selectConversation({ conversationId: undefined }));
  }

  clear(): void {
    this.input = '';
    this.composerText = '';
    this.currentPhase = this.phases[0];
    this.feedbackPhase = undefined;
    this.stalePromptPhase = undefined;
    this.completedPhase = undefined;
    this.statusText = '';
    this.copyInputText = AgentUiText.copyInput;
    this.copyOutputText = AgentUiText.copyOutput;
    this.errorMessage = '';
    this.messages = [];
    this.phaseStates = this.createEmptyPhaseStates();
  }

  async openConversation(conversationId: string): Promise<void> {
    if (this.isLoading) {
      return;
    }

    try {
      const conversation = await firstValueFrom(this.conversationService.getById(conversationId, this.user));
      this.hydrateConversation(conversation);
      this.store.dispatch(selectConversation({ conversationId }));
    } catch (error) {
      this.errorMessage = error instanceof Error ? error.message : AgentUiText.openConversationError;
    }
  }

  async deleteConversation(conversationId: string, event: Event): Promise<void> {
    event.stopPropagation();

    if (this.isLoading) {
      return;
    }

    await firstValueFrom(this.conversationService.delete(conversationId, this.user));
    if (this.currentConversationId === conversationId) {
      this.newChat();
    }

    await this.loadConversations();
  }

  async copyInput(): Promise<void> {
    await this.copyToClipboard(this.input, 'input');
  }

  async copyOutput(): Promise<void> {
    await this.copyToClipboard(this.phaseStates[this.currentPhase].output, 'output');
  }

  canUsePhase(phase: AgentPhase): boolean {
    if (this.isLoading || !this.isUserReady) {
      return false;
    }

    return this.getPhaseOrder(phase) <= this.getPhaseOrder(this.completedPhase) + 1 || !!this.phaseStates[phase].output;
  }

  getPhaseStatus(phase: AgentPhase): string {
    const state = this.phaseStates[phase];
    if (state.isStale) {
      return AgentUiText.stale;
    }

    if (this.isLoading && this.currentPhase === phase) {
      return AgentUiText.inProgress;
    }

    if (state.output.trim()) {
      return AgentUiText.completed;
    }

    return this.canUsePhase(phase) ? AgentUiText.available : AgentUiText.blocked;
  }

  getPhaseLabel(phase: AgentPhase): string {
    return this.phaseActions.find(action => action.phase === phase)?.label ?? phase;
  }

  getPhaseNumber(phase: AgentPhase): number {
    return this.getPhaseOrder(phase) + 1;
  }

  getInitials(): string {
    return (this.userDisplayName || 'Usuario')
      .split(/\s+/)
      .filter(Boolean)
      .slice(0, 2)
      .map(part => part[0]?.toUpperCase())
      .join('') || 'U';
  }

  trackConversation(_: number, conversation: ConversationSummary): string {
    return conversation.id;
  }

  trackMessage(_: number, message: TimelineMessage): string {
    return message.id;
  }

  trackPhase(_: number, phaseAction: { phase: AgentPhase }): string {
    return phaseAction.phase;
  }

  private async loadConversations(): Promise<void> {
    this.isHistoryLoading = true;
    try {
      this.conversations = await firstValueFrom(this.conversationService.list(this.user));
      this.store.dispatch(setConversations({ conversations: this.conversations }));
    } finally {
      this.isHistoryLoading = false;
      this.changeDetector.detectChanges();
    }
  }

  private async afterConversationMutation(): Promise<void> {
    await this.loadConversations();
    if (this.currentConversationId) {
      const conversation = await firstValueFrom(this.conversationService.getById(this.currentConversationId, this.user));
      this.hydrateConversation(conversation);
    }
  }

  private hydrateConversation(conversation: ConversationDetail): void {
    this.currentConversationId = conversation.id;
    this.input = conversation.initialInput;
    this.composerText = '';
    this.feedbackPhase = undefined;
    this.stalePromptPhase = undefined;
    this.phaseStates = this.createEmptyPhaseStates();

    for (const phaseOutput of conversation.phaseOutputs) {
      const phase = this.normalizePhase(phaseOutput.phase);
      if (phase) {
        this.phaseStates[phase] = {
          output: phaseOutput.output,
          renderedOutput: this.markdownRenderer.render(phaseOutput.output),
          isStale: phaseOutput.isStale,
          completedAt: phaseOutput.completedAt
        };
      }
    }

    this.messages = conversation.messages?.length
      ? conversation.messages.map(message => this.mapMessage(message))
      : this.buildMessagesFromOutputs(conversation);
    this.completedPhase = this.getHighestCompletedPhase();
    this.currentPhase = this.completedPhase ?? this.phases[0];
    this.statusText = this.currentPhase ? this.getPhaseLabel(this.currentPhase) : '';
    this.errorMessage = '';
    this.changeDetector.detectChanges();
    this.scrollChatToBottom();
  }

  private buildMessagesFromOutputs(conversation: ConversationDetail): TimelineMessage[] {
    const messages = [
      this.createMessage('user', 'InitialInput', undefined, conversation.initialInput, conversation.createdAt)
    ];

    for (const output of conversation.phaseOutputs) {
      const phase = this.normalizePhase(output.phase);
      messages.push(this.createMessage('agent', 'PhaseOutput', phase, output.output, output.completedAt));
    }

    return messages;
  }

  private mapMessage(message: ConversationMessage): TimelineMessage {
    return this.createMessage(
      message.role === 'Agent' ? 'agent' : message.role === 'System' ? 'system' : 'user',
      message.type,
      message.phase ? this.normalizePhase(message.phase) : undefined,
      message.content,
      message.createdAt,
      message.id);
  }

  private addLocalMessage(
    role: 'user' | 'agent' | 'system',
    type: string,
    phase: AgentPhase | undefined,
    content: string,
    isStreaming = false
  ): TimelineMessage {
    const message = this.createMessage(role, type, phase, content, new Date().toISOString());
    message.isStreaming = isStreaming;
    this.messages = [...this.messages, message];
    this.scrollChatToBottom();
    return message;
  }

  private createMessage(
    role: 'user' | 'agent' | 'system',
    type: string,
    phase: AgentPhase | undefined,
    content: string,
    createdAt: string,
    id: string = crypto.randomUUID()
  ): TimelineMessage {
    return {
      id,
      role,
      type,
      phase,
      content,
      renderedContent: role === 'agent' ? this.markdownRenderer.render(content) : this.escapeHtml(content),
      createdAt
    };
  }

  private appendChunk(
    message: TimelineMessage,
    chunk: string,
    controller: AbortController,
    updateFirstChunk: (value: boolean) => void
  ): void {
    if (!message.content) {
      this.statusText = AgentUiText.writing;
      updateFirstChunk(false);
    }

    message.content += chunk;
    message.renderedContent = this.markdownRenderer.render(message.content);
    if (this.outputValidator.hasDegeneratedOutput(message.content)) {
      controller.abort();
      message.content = '';
      message.renderedContent = '';
      throw new Error(AgentUiText.degeneratedOutputError);
    }

    this.changeDetector.detectChanges();
    this.scrollChatToBottom();
  }

  private startGeneration(phaseLabel: string): void {
    this.isLoading = true;
    this.errorMessage = '';
    this.statusText = AgentUiText.thinking;
    this.currentPhase = this.phaseActions.find(action => action.label === phaseLabel)?.phase ?? this.currentPhase;
  }

  private finishPhaseGeneration(phase: AgentPhase, output: string): void {
    this.phaseStates[phase] = {
      output,
      renderedOutput: this.markdownRenderer.render(output),
      isStale: false,
      completedAt: new Date().toISOString()
    };
    this.completedPhase = this.getHighestCompletedPhase();
    this.statusText = this.getPhaseLabel(phase);
  }

  private handleGenerationError(error: unknown): void {
    if (error instanceof DOMException && error.name === 'AbortError') {
      return;
    }

    this.errorMessage = error instanceof Error ? error.message : AgentUiText.generationError;
    this.statusText = AgentUiText.errorStatus;
  }

  private finishLoading(): void {
    this.isLoading = false;
    this.activeController = undefined;
    const last = this.messages[this.messages.length - 1];
    if (last) {
      last.isStreaming = false;
    }
    this.changeDetector.detectChanges();
  }

  private buildPreviousOutput(phase: AgentPhase): string {
    const currentOrder = this.getPhaseOrder(phase);
    if (currentOrder <= 0) {
      return '';
    }

    return this.phases
      .slice(0, currentOrder)
      .map(previousPhase => {
        const output = this.phaseStates[previousPhase].output.trim();
        return output ? `FASE ANTERIOR: ${this.getPhaseLabel(previousPhase)}\n${output}` : '';
      })
      .filter(Boolean)
      .join('\n\n');
  }

  private markLaterPhasesStale(phase: AgentPhase): void {
    const order = this.getPhaseOrder(phase);
    for (const laterPhase of this.phases.slice(order + 1)) {
      if (this.phaseStates[laterPhase].output.trim()) {
        this.phaseStates[laterPhase] = { ...this.phaseStates[laterPhase], isStale: true };
      }
    }
  }

  private getNextPhase(phase: AgentPhase): AgentPhase | undefined {
    return this.phases[this.getPhaseOrder(phase) + 1];
  }

  private getPhaseOrder(phase?: AgentPhase): number {
    return phase ? this.phases.indexOf(phase) : -1;
  }

  private normalizePhase(value: string): AgentPhase | undefined {
    const normalized = value
      .replace(/([a-z0-9])([A-Z])/g, '$1_$2')
      .replace(/[\s-]+/g, '_')
      .toLowerCase();

    return this.phases.find(phase => phase === normalized);
  }

  private getHighestCompletedPhase(): AgentPhase | undefined {
    return [...this.phases]
      .reverse()
      .find(phase => this.phaseStates[phase].output.trim());
  }

  private createEmptyPhaseStates(): Record<AgentPhase, PhaseState> {
    return this.phases.reduce((states, phase) => {
      states[phase] = { output: '', renderedOutput: '', isStale: false };
      return states;
    }, {} as Record<AgentPhase, PhaseState>);
  }

  private scrollChatToBottom(): void {
    window.setTimeout(() => {
      const element = this.chatBody?.nativeElement;
      if (element) {
        element.scrollTop = element.scrollHeight;
      }
    });
  }

  private async copyToClipboard(value: string, target: 'input' | 'output'): Promise<void> {
    await this.clipboardService.copy(value);

    if (target === 'input') {
      this.copyInputText = AgentUiText.copied;
      window.setTimeout(() => this.copyInputText = AgentUiText.copyInput, AgentTiming.copyResetDelayMs);
      return;
    }

    this.copyOutputText = AgentUiText.copied;
    window.setTimeout(() => this.copyOutputText = AgentUiText.copyOutput, AgentTiming.copyResetDelayMs);
  }

  private escapeHtml(value: string): string {
    return value
      .replace(/&/g, '&amp;')
      .replace(/</g, '&lt;')
      .replace(/>/g, '&gt;')
      .replace(/"/g, '&quot;')
      .replace(/'/g, '&#039;')
      .replace(/\n/g, '<br>');
  }
}
