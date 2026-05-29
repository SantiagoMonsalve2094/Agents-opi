import { Component } from '@angular/core';
import { AgentWorkspace } from './features/agent/pages/agent-workspace/agent-workspace';

@Component({
  selector: 'app-root',
  imports: [AgentWorkspace],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {}
