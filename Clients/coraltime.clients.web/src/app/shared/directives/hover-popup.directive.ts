
import { Overlay, OverlayConfig, OverlayRef, PositionStrategy } from "@angular/cdk/overlay";
import { TemplatePortal } from "@angular/cdk/portal";
import { AfterViewInit, ContentChild, Directive, ElementRef, HostListener, OnDestroy, TemplateRef, ViewContainerRef } from "@angular/core";

@Directive({
    selector: '[ctHoverPopup]',
    standalone: false
})

export class HoverPopupDirective implements OnDestroy, AfterViewInit {
  @ContentChild('overlayTemplate', { static: true }) popupContentContainerRef: TemplateRef<any>;

  private overlayRef: OverlayRef;
  private closeTimer: any;

	constructor(private el: ElementRef,
    private viewContainerRef: ViewContainerRef,
    private overlay: Overlay
  ) {
	}

  ngAfterViewInit(): void {
    this.el.nativeElement.classList.add('ct-hover-popup');
  }

  @HostListener('mouseenter') onMouseEnter() {
    this.cancelClose();
    this.open();
  }

  @HostListener('mouseleave') onMouseLeave() {
    this.scheduleClose();
  }

  private open() {
    if (!this.overlayRef) {
      const positionStrategy: PositionStrategy = this.overlay.position()
        .flexibleConnectedTo(this.el.nativeElement)
        .withDefaultOffsetY(17)
        .withPositions([
          { originX: 'start', originY: 'bottom', overlayX: 'start', overlayY: 'top' },
          { originX: 'start', originY: 'top', overlayX: 'start', overlayY: 'bottom' }
        ]);
      this.overlayRef = this.overlay.create(new OverlayConfig({ 
        positionStrategy, 
        panelClass: 'ct-hover-popup-content' 
      }));

      const host = this.overlayRef.overlayElement;
      host.addEventListener('mouseenter', () => this.cancelClose());
      host.addEventListener('mouseleave', () => this.scheduleClose());
    }

    if (!this.overlayRef.hasAttached()) {
      const portal = new TemplatePortal(this.popupContentContainerRef, this.viewContainerRef);
      this.overlayRef.attach(portal);
    }
  }

  private scheduleClose() {
    this.cancelClose();
    this.closeTimer = setTimeout(() => {
      if (this.overlayRef?.hasAttached()) {
        this.overlayRef.detach();
      }
    }, 100);
  }

  private cancelClose() {
    if (this.closeTimer) {
      clearTimeout(this.closeTimer);
      this.closeTimer = null;
    }
  }

  ngOnDestroy() {
    this.cancelClose();
    this.overlayRef?.dispose();
  }
}
