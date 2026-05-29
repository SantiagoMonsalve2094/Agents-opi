import { Injectable } from '@angular/core';
import { DegeneratedOutputRules } from '../constants/agent-ui.constants';

@Injectable({
  providedIn: 'root'
})
export class AgentOutputValidator {
  hasDegeneratedOutput(value: string): boolean {
    if (
      DegeneratedOutputRules.repeatedCharacterPattern.test(value) ||
      DegeneratedOutputRules.repeatedSymbolPattern.test(value)
    ) {
      return true;
    }

    if (DegeneratedOutputRules.repeatedWordPattern.test(value)) {
      return true;
    }

    const tail = value.slice(-DegeneratedOutputRules.tailLength);
    const compactTail = tail.replace(/\s/g, '');
    return compactTail.length > DegeneratedOutputRules.compactTailMinLength &&
      DegeneratedOutputRules.compactTailPattern.test(compactTail);
  }
}
