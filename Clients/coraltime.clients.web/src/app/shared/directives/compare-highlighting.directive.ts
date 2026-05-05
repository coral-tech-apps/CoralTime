import { AfterViewInit, Directive, ElementRef, Input } from "@angular/core";

@Directive({
    selector: '[comparisonHighlights]',
    standalone: false
})

export class ComparisonHighlightsDirective implements AfterViewInit {
  @Input() leftValue: any;
  @Input() rightValue: any;

  constructor(private _elementRef: ElementRef) {}


  ngAfterViewInit(): void {
    const element = this._elementRef.nativeElement as HTMLElement;
    const indicator = document.createElement('div');
    indicator.classList.add('ct-comparison-highlight');
    
    if (this.leftValue !== this.rightValue) {
      indicator.classList.add('different');
      
      element.setAttribute('ct-tooltip-data', 'Values are different');
      element.style.position = 'relative';
      element.appendChild(indicator);
    }
  }
}