import { Directive, ElementRef, Input } from "@angular/core";

@Directive({
    selector: '[ctTooltip]',
    standalone: false
})
export class TooltipDirective {
  @Input('ct-tooltip-data') set tooltipText(value: string) {
    this.el.nativeElement.setAttribute('ct-tooltip-data', value);
  }

  constructor(private el: ElementRef) {}
}