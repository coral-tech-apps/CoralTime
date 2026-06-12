import { AfterViewInit, ComponentRef, Directive, ElementRef, EmbeddedViewRef, EventEmitter, Input, OnChanges, OnDestroy, Output, Renderer2, SimpleChanges, ViewContainerRef } from "@angular/core";
import { Skeleton } from "primeng/skeleton";

@Directive({
    selector: '[ctPaginationScrolling]',
    standalone: false
})
export class PaginationScrollingDirective implements AfterViewInit, OnChanges, OnDestroy {
  @Input() isLoading: boolean = false;
  @Input() skeletonRows: number = 3;
  @Input() skeletonHeight: string = '1.5rem';
  @Output() onEndScroll = new EventEmitter<void>();

  private scrollContainer: HTMLElement | null = null;
  private skeletonWrapper: HTMLElement | null = null;
  private skeletonRefs: ComponentRef<Skeleton>[] = [];
  private scrollHandler = () => this.checkScroll();

  constructor(
    private el: ElementRef,
    private renderer: Renderer2,
    private viewContainerRef: ViewContainerRef,
  ) {}

  ngAfterViewInit(): void {
    this.scrollContainer = this.el.nativeElement.querySelector('.p-datatable-table-container') ?? this.el.nativeElement;
    if (this.scrollContainer) {
      this.scrollContainer.addEventListener('scroll', this.scrollHandler);
    }
    this.syncSkeleton();
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['isLoading']) {
      this.syncSkeleton();
    }
  }

  ngOnDestroy(): void {
    if (this.scrollContainer) {
      this.scrollContainer.removeEventListener('scroll', this.scrollHandler);
    }
    this.removeSkeleton();
  }

  private checkScroll(): void {
    if (!this.scrollContainer || this.isLoading) {
      return;
    }

    const { scrollTop, scrollHeight, clientHeight } = this.scrollContainer;
    const preloadThreshold = 20;
    if (scrollTop + clientHeight >= scrollHeight - preloadThreshold) {
      this.onEndScroll.emit();
    }
  }

  private syncSkeleton(): void {
    if (!this.scrollContainer) {
      return;
    }
    if (this.isLoading) {
      this.showSkeleton();
    } else {
      this.removeSkeleton();
    }
  }

  private showSkeleton(): void {
    if (this.skeletonWrapper || !this.scrollContainer) {
      return;
    }
    const wrapper = this.renderer.createElement('div');
    this.renderer.addClass(wrapper, 'ct-pagination-skeleton');
    this.renderer.setStyle(wrapper, 'padding', '0.5rem 1rem');
    this.renderer.setStyle(wrapper, 'display', 'flex');
    this.renderer.setStyle(wrapper, 'flex-direction', 'column');
    this.renderer.setStyle(wrapper, 'gap', '0.5rem');

    for (let i = 0; i < this.skeletonRows; i++) {
      const ref = this.viewContainerRef.createComponent(Skeleton);
      ref.setInput('height', this.skeletonHeight);
      ref.setInput('width', '100%');
      ref.changeDetectorRef.detectChanges();
      const node = (ref.hostView as EmbeddedViewRef<unknown>).rootNodes[0];
      this.renderer.appendChild(wrapper, node);
      this.skeletonRefs.push(ref);
    }

    this.renderer.appendChild(this.scrollContainer, wrapper);
    this.skeletonWrapper = wrapper;
  }

  private removeSkeleton(): void {
    this.skeletonRefs.forEach(ref => ref.destroy());
    this.skeletonRefs = [];
    if (this.skeletonWrapper && this.scrollContainer) {
      this.renderer.removeChild(this.scrollContainer, this.skeletonWrapper);
    }
    this.skeletonWrapper = null;
  }
}
