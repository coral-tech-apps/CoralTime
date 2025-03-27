import {
	Component, Input, Output, EventEmitter, forwardRef, ViewChild, HostListener,
} from '@angular/core';
import { trigger, state, style, transition, animate, AnimationEvent } from '@angular/animations';
import { NG_VALUE_ACCESSOR } from '@angular/forms';
import { SelectItem } from 'primeng/api';
import { MultiSelect, MultiSelectFilterEvent, MultiSelectSelectAllChangeEvent, MultiSelectStyle } from 'primeng/multiselect';
import { ObjectUtils } from 'primeng/utils';
import { DomHandler } from 'primeng/dom';

export class CustomSelectItem implements SelectItem {
	isActive?: boolean;
	label: string;
	value: any;

	constructor(label: string, value: any, isActive?: boolean) {
		this.label = label;
		this.value = value;
		this.isActive = isActive != null ? isActive : true;
	}
}

export const MULTISELECT_VALUE_ACCESSOR: any = {
	provide: NG_VALUE_ACCESSOR,
	useExisting: forwardRef(() => MultiSelectComponent),
	multi: true
};

@Component({
    selector: 'ct-multiselect',
    templateUrl: 'multiselect.component.html',
    animations: [
        trigger('overlayAnimation', [
            state('void', style({
                transform: 'translateY(5%)',
                opacity: 0
            })),
            state('visible', style({
                transform: 'translateY(0)',
                opacity: 1
            })),
            transition('void => visible', animate('{{showTransitionParams}}')),
            transition('visible => void', animate('{{hideTransitionParams}}'))
        ])
    ],
    providers: [DomHandler, ObjectUtils, MULTISELECT_VALUE_ACCESSOR, MultiSelectStyle],
    standalone: false
})

export class MultiSelectComponent extends MultiSelect {
	@Input() extraActionTitle: string;
	//@Input() scrollHeight: string = '306px';
	@Input() showSubmitButton: boolean = false;
	@Input() showFilterSearch: boolean = true;
	@Input() showActionsPanel: boolean = true;

	@Output() onExtraAction: EventEmitter<any> = new EventEmitter();
	@Output() onSubmitAction: EventEmitter<any> = new EventEmitter();

	@ViewChild('slimScroll', { static: true }) slimScroll: any;

	isSubmitted: boolean = false;
	oldValue: any[];

  override ngAfterViewInit(): void {
    this.renderer.removeClass(this.el.nativeElement, 'p-component');
    this.renderer.removeClass(this.el.nativeElement, 'p-multiselect');
    this.renderer.removeClass(this.el.nativeElement, 'p-inputwrapper');
    this.renderer.removeClass(this.el.nativeElement, 'p-inputwrapper-focus');
    this.renderer.removeClass(this.el.nativeElement, 'p-multiselect-open');
  }

	override show(): void {
    console.log('show: ', this.overlayVisible)
		super.show();
		this.redrowSlimScroll();

		if (this.showSubmitButton) {
			this.isSubmitted = false;
			this.oldValue = this.value;
		}
	}

	override hide(): void {
    console.log('hide: ', this.overlayVisible)
		super.hide();
		this.clearFilter();

		if (this.showSubmitButton && !this.isSubmitted) {
			this.value = this.oldValue;
			//this.updateLabel();
		}
	}

	clearFilter(): void {
		this.filterValue = null;
		this.redrowSlimScroll();
	}

  onFilter1(): void {
    this.redrowSlimScroll();
  }

  findSelectionIndex(val: any): number {
      //Mostly copied from the 'findSelectionIndex' function here.
			//https://github.com/primefaces/primeng/blob/15.4.1/src/app/components/multiselect/multiselect.ts
    let index = -1;

    if (this.value) {
        for (let i = 0; i < this.value.length; i++) {
            if (ObjectUtils.equals(this.value[i], val, this.dataKey)) {
                index = i;
                break;
            }
        }
    }

    return index;
  }

	onItemClick(event: any, option: any): void {
    if (this.isOptionDisabled(option)) {
        return;
    }

    if (!event) {
        event = new MouseEvent('click');
    }

    const optionValue = this.getOptionValue(option);
    const selectionIndex = this.findSelectionIndex(optionValue);

    if (selectionIndex !== -1) {
        this.value = this.value.filter((_, i) => i !== selectionIndex);
        //this.onModelChange(this.value);
        //this.onChange.emit({ originalEvent: event, value: this.value, itemValue: optionValue });
    } else {
        if (!this.selectionLimit || !this.value || this.value.length < this.selectionLimit) {
            this.value = [...(this.value || []), optionValue];

        }
    }
    this.onModelChange(this.value);
    this.onChange.emit({ originalEvent: event, value: this.value, itemValue: optionValue });
  }

	selectAllItems(event: any): void {
    if (!this.selectAll) {
        const toggleEvent: MultiSelectSelectAllChangeEvent = event ?
            { originalEvent: event, checked: true } :
            { originalEvent: new MouseEvent('click'), checked: true };

        super.onToggleAll(toggleEvent);
    }
    else {
    }
}

  selectNone(event: any): void {
    if (this.selectAll) {
        const toggleEvent: MultiSelectSelectAllChangeEvent = event ?
            { originalEvent: event, checked: false } :
            { originalEvent: new MouseEvent('click'), checked: false };

        super.onToggleAll(toggleEvent);
    }
    else {
        const safeEvent = event || new MouseEvent('click');
        this.value = [];
        this.onModelChange(this.value);
        this.onChange.emit({ originalEvent: safeEvent, value: this.value });
    }
  }

	doExtraAction(event): void {
		this.onExtraAction.emit(event);
	}

	submit($event): void {
		this.isSubmitted = true;
		this.oldValue = this.value;
		this.onSubmitAction.emit($event);
		this.close($event);
	}

	override toString(value: string): string {
		return value !== null + '' ? value : this.defaultLabel.slice(4) + ' (1)';
	}

	private redrowSlimScroll(): void {
		setTimeout(() => {
			this.slimScroll?.getBarHeight();
		}, 0);
	}

  onMouseclick(event,input) {

}

	@HostListener('document:keydown', ['$event'])
	override onKeyDown(event: KeyboardEvent) {
		if (this.overlayVisible && event.key === 'Enter') {
			this.submit(event);
		}
	}
}
