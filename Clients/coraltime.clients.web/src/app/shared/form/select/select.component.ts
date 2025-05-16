import { Component, Input, Output, EventEmitter, forwardRef, ChangeDetectorRef, ChangeDetectionStrategy, ViewChild, ElementRef, Renderer2, TemplateRef, ViewContainerRef, HostListener } from '@angular/core';
import { NG_VALUE_ACCESSOR, ControlValueAccessor } from '@angular/forms';
import { coerceBooleanProperty } from '@angular/cdk/coercion';
import { Overlay, OverlayRef, OverlayConfig, PositionStrategy } from '@angular/cdk/overlay';
import { TemplatePortal } from '@angular/cdk/portal';

export const LIST_ITEM_HEIGHT = 42;

export const SELECT_CONTROL_VALUE_ACCESSOR: any = {
  provide: NG_VALUE_ACCESSOR,
  useExisting: forwardRef(() => SelectComponent),
  multi: true
};

export class SelectChange {
  source: SelectComponent;
  value: any[];
}

@Component({
  selector: 'ct-select',
  templateUrl: 'select.component.html',
  providers: [SELECT_CONTROL_VALUE_ACCESSOR],
  host: {
    '(document:keydown)': 'onKeyDown($event)'
  },
  changeDetection: ChangeDetectionStrategy.OnPush,
  standalone: false
})
export class SelectComponent implements ControlValueAccessor {
  @Input('name') name: string;
  @Input('displayName') displayName: string;
  @Input('trackBy') trackBy: string;
  @Input('options') options: any[];
  @Input('defaultValue') defaultValue: string;
  @Input('canClickOverlay') canClickOverlay: boolean = false;
  @Input('maxHeight') maxHeight: number = 168;
  @Input('container') container: HTMLDivElement;
  @Output() change: EventEmitter<SelectChange> = new EventEmitter<SelectChange>();

  isOpen: boolean = false;
  isAnimate: boolean = false;
  isListShowToTop: boolean = false;
  selectedObject: any;

  @ViewChild('slimScroll', { static: true }) slimScroll: any;
  @ViewChild('matList', { read: ElementRef, static: true }) matList: ElementRef;
  @ViewChild('overlayTemplate', { static: true }) overlayTemplate: TemplateRef<any>;

  private overlayRef: OverlayRef;
  private _disabled: boolean = false;
  get disabled(): boolean { return this._disabled; }
  set disabled(value) { this._disabled = coerceBooleanProperty(value); }

  private oldSelectedObject: any;
  private scrollTopNumber: number = 0;
  private _controlValueAccessorChangeFn: (value: any) => void = () => { };
  private onTouched: () => any = () => { };

  constructor(
    private el: ElementRef,
    private ref: ChangeDetectorRef,
    private renderer: Renderer2,
    private overlay: Overlay,
    private vcr: ViewContainerRef
  ) {
  }

  getSelectedOptionsText() {
    return this.selectedObject ? this.getDisplayedName(this.selectedObject) : this.defaultValue;
  }

  selectOption(option: any, close: boolean = true) {
    if (option && option.disabled) {
      return;
    }
    this.selectedObject = option;
    if (close) {
      this.closeSelect();
    }
    this.onTouched();
    if (this.getOptionValue(option) !== this.getOptionValue(this.oldSelectedObject)) {
      this._emitChangeEvent();
    }
  }

  getOptionIndex(option: any): number {
    let optionIndex = -1;
    if (option) {
      this.options.forEach((opt, i) => {
        if (this.getOptionValue(opt) === this.getOptionValue(option)) {
          optionIndex = i;
        }
      });
    }
    return optionIndex;
  }

  getOptionValue(option: any): any {
    return option ? (this.trackBy ? option[this.trackBy] : option) : null;
  }

  isOptionSelected(option): boolean {
    return this.selectedObject === option;
  }

  getDisplayedName(option: any) {
    const result = option ? (this.displayName ? option[this.displayName] : option) : '';
    if (result.name) {
      return result.name;
    }
    return result;
  }

  trackByFn(index: number, item: any) {
    return item[this.trackBy];
  }

  writeValue(selectedObject: any = null) {
    this.selectedObject = selectedObject;
    this.ref.markForCheck();
    if (this.selectedObject) {
      this._controlValueAccessorChangeFn(this.selectedObject);
    }
  }

  registerOnChange(fn: (value: any) => void) {
    this._controlValueAccessorChangeFn = fn;
  }
  registerOnTouched(fn: any) {
    this.onTouched = fn;
  }

  setDisabledState(isDisabled: boolean) {
    this.disabled = isDisabled;
  }

  closeSelect(): void {
    this.isOpen = false;
    this.isAnimate = false;
    if (this.overlayRef && this.overlayRef.hasAttached()) {
      this.overlayRef.detach();
    }
  }

  openSelect(): void {
    if (!this.overlayRef) {
      const positionStrategy: PositionStrategy = this.overlay.position()
        .flexibleConnectedTo(this.el.nativeElement)
        .withDefaultOffsetY(17)
        .withPositions([
          { originX: 'start', originY: 'bottom', overlayX: 'start', overlayY: 'top' },
          { originX: 'start', originY: 'top', overlayX: 'start', overlayY: 'bottom' }
        ]);
      const overlayConfig = new OverlayConfig({
        positionStrategy,
        hasBackdrop: this.canClickOverlay,
        backdropClass: 'cdk-overlay-transparent-backdrop',
        scrollStrategy: this.overlay.scrollStrategies.reposition(),
      });
      this.overlayRef = this.overlay.create(overlayConfig);
      this.overlayRef.backdropClick().subscribe(() => this.closeSelect());
    }

    this.isOpen = true;
    this.oldSelectedObject = this.selectedObject;
    setTimeout(() => {
      this.isAnimate = true;
      this.ref.markForCheck();
      const portal = new TemplatePortal(this.overlayTemplate, this.vcr);
      this.overlayRef.attach(portal);
      const pane = this.overlayRef.overlayElement;
      const width = this.el.nativeElement.getBoundingClientRect().width;
      this.renderer.addClass(pane, 'ct-select-component');
      this.renderer.addClass(pane, 'ct-select-opened');
      this.renderer.addClass(pane, "ct-select-animate");
      this.renderer.setStyle(pane, 'width', `${width}px`);
    }, 0);
  }

  toggleSelect(): void {
    if (!this._disabled) {
      this.isOpen ? this.closeSelect() : this.openSelect();
    }
  }

  onKeyDown(event: KeyboardEvent): void {
    if (!this.isOpen) {
      return;
    }
    event.preventDefault();
    event.stopPropagation();
    let optionIndex = this.getOptionIndex(this.selectedObject);
    if (event.key === 'ArrowDown') {
      optionIndex = optionIndex + 1 < this.options.length ? optionIndex + 1 : optionIndex;
      this.selectedObject = this.options[optionIndex];
      this.changeScrollTop(optionIndex);
      this.slimScroll.scrollContent(this.scrollTopNumber, false, true);
      return;
    }
    if (event.key === 'ArrowUp') {
      optionIndex = optionIndex > 0 ? optionIndex - 1 : 0;
      this.selectedObject = this.options[optionIndex];
      this.changeScrollTop(optionIndex);
      this.slimScroll.scrollContent(this.scrollTopNumber, false, true);
      return;
    }
    if (event.key === 'Enter') {
      this.selectOption(this.selectedObject);
    }
  }

  private changeScrollTop(optionIndex: number): void {
    if (optionIndex < this.scrollTopNumber) {
      this.scrollTopNumber--;
    } else if (optionIndex > this.scrollTopNumber + 3) {
      this.scrollTopNumber++;
    } else {
      return;
    }
  }

  private _emitChangeEvent() {
    let event = new SelectChange();
    event.source = this;
    event.value = this.selectedObject;
    this._controlValueAccessorChangeFn(this.selectedObject);
    this.change.emit(event);
  }

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent) {
    const target = event.target as HTMLElement;
    const clickedInside = this.el.nativeElement.contains(target);

    if (!clickedInside) {
      this.closeSelect();  // Закрываем всегда при клике вне компонента
    }
  }
}
