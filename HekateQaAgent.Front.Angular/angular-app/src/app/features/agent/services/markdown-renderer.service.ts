import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class MarkdownRendererService {
  render(value: string): string {
    if (!value.trim()) {
      return '';
    }

    const lines = value.replace(/\r\n/g, '\n').split('\n');
    const html: string[] = [];
    let index = 0;

    while (index < lines.length) {
      const line = lines[index];

      if (!line.trim()) {
        index++;
        continue;
      }

      if (line.trim().startsWith('```')) {
        const language = this.escapeHtml(line.trim().slice(3).trim() || 'text');
        const codeLines: string[] = [];
        index++;

        while (index < lines.length && !lines[index].trim().startsWith('```')) {
          codeLines.push(lines[index]);
          index++;
        }

        if (index < lines.length) {
          index++;
        }

        const code = codeLines.join('\n');
        if (code.trim()) {
          html.push(`<div class="code-card"><div class="code-header"><span>${language}</span></div><pre class="code-block"><code>${this.escapeHtml(code)}</code></pre></div>`);
        }
        continue;
      }

      if (this.isTableStart(lines, index)) {
        const tableLines: string[] = [];
        while (index < lines.length && this.isTableLine(lines[index])) {
          tableLines.push(lines[index]);
          index++;
        }

        html.push(this.renderTable(tableLines));
        continue;
      }

      if (line.startsWith('### ')) {
        html.push(`<h3>${this.renderInline(line.slice(4))}</h3>`);
        index++;
        continue;
      }

      if (line.startsWith('## ')) {
        html.push(`<h2>${this.renderInline(line.slice(3))}</h2>`);
        index++;
        continue;
      }

      if (line.startsWith('# ')) {
        html.push(`<h1>${this.renderInline(line.slice(2))}</h1>`);
        index++;
        continue;
      }

      if (/^\s*[-*]\s+/.test(line)) {
        const items: string[] = [];
        while (index < lines.length && /^\s*[-*]\s+/.test(lines[index])) {
          items.push(`<li>${this.renderInline(lines[index].replace(/^\s*[-*]\s+/, ''))}</li>`);
          index++;
        }

        html.push(`<ul>${items.join('')}</ul>`);
        continue;
      }

      const paragraphLines: string[] = [];
      while (
        index < lines.length &&
        lines[index].trim() &&
        !lines[index].trim().startsWith('```') &&
        !this.isTableStart(lines, index) &&
        !/^#{1,3}\s/.test(lines[index]) &&
        !/^\s*[-*]\s+/.test(lines[index])
      ) {
        paragraphLines.push(lines[index]);
        index++;
      }

      html.push(`<p>${this.renderInline(paragraphLines.join('<br>'))}</p>`);
    }

    return html.join('');
  }

  private isTableStart(lines: string[], index: number): boolean {
    if (index + 1 >= lines.length || !this.isTableLine(lines[index])) {
      return false;
    }

    if (this.isTableSeparator(lines[index + 1])) {
      return true;
    }

    return this.isTableLine(lines[index + 1]) &&
      this.splitTableRow(lines[index]).length === this.splitTableRow(lines[index + 1]).length;
  }

  private isTableLine(line: string): boolean {
    return line.trim().startsWith('|') && line.trim().endsWith('|');
  }

  private isTableSeparator(line: string): boolean {
    return /^\|?\s*:?-{3,}:?\s*(\|\s*:?-{3,}:?\s*)+\|?$/.test(line.trim());
  }

  private renderTable(lines: string[]): string {
    const header = this.splitTableRow(lines[0]);
    const bodyStartIndex = lines.length > 1 && this.isTableSeparator(lines[1]) ? 2 : 1;
    const rows = lines.slice(bodyStartIndex).map(row => this.splitTableRow(row));
    const headerHtml = header.map(cell => `<th>${this.renderInline(cell)}</th>`).join('');
    const bodyHtml = rows
      .map(row => `<tr>${row.map(cell => `<td>${this.renderInline(cell)}</td>`).join('')}</tr>`)
      .join('');

    return `<div class="table-scroll"><table><thead><tr>${headerHtml}</tr></thead><tbody>${bodyHtml}</tbody></table></div>`;
  }

  private splitTableRow(row: string): string[] {
    return row
      .trim()
      .replace(/^\|/, '')
      .replace(/\|$/, '')
      .split('|')
      .map(cell => cell.trim());
  }

  private renderInline(value: string): string {
    return this.escapeHtml(value)
      .replace(/&lt;br\s*\/?&gt;/gi, '<br>')
      .replace(/\*\*(.*?)\*\*/g, '<strong>$1</strong>')
      .replace(/`([^`]+)`/g, '<code>$1</code>');
  }

  private escapeHtml(value: string): string {
    return value
      .replace(/&/g, '&amp;')
      .replace(/</g, '&lt;')
      .replace(/>/g, '&gt;')
      .replace(/"/g, '&quot;')
      .replace(/'/g, '&#039;');
  }
}
