import { Pipe } from "@angular/core";

@Pipe({
    name: 'keyword'
})
export class HighlightPipe {
    public transform(text: string, keyword: string, noteId: number, selectedKeyword: string): string {
        function replacer(match, p1) {
            return `<mark class="${selectedKeyword === `${noteId}-${p1}`? 'active': ''}">${match}</mark>`;
        }

        return text.replace(new RegExp(keyword, 'gi'), replacer);
    }
}