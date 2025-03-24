import {
	Component, Input, Output, EventEmitter, forwardRef, ViewChild, HostListener
} from '@angular/core';
import { trigger, state, style, transition, animate, AnimationEvent } from '@angular/animations';
import { NG_VALUE_ACCESSOR } from '@angular/forms';
import { SelectItem } from 'primeng/api';
import { MultiSelect, MultiSelectSelectAllChangeEvent } from 'primeng/multiselect';
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
	providers: [DomHandler, ObjectUtils, MULTISELECT_VALUE_ACCESSOR]
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

	override show(): void {
		super.show();
		this.redrowSlimScroll();

		if (this.showSubmitButton) {
			this.isSubmitted = false;
			this.oldValue = this.value;
		}
	}

	override hide(): void {
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

	//onFilter(): void {
	//	super.onFilter();
	//	this.redrowSlimScroll();
	//}

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

	onItemClick(event, option): void {
      //Mostly copied from the 'onOptionClick' function here.
			//https://github.com/primefaces/primeng/blob/15.4.1/src/app/components/multiselect/multiselect.ts
      if (this.isOptionDisabled(option)) {
          return;
      }

      const optionValue = this.getOptionValue(option);
      const selectionIndex = this.findSelectionIndex(optionValue);

      if (selectionIndex !== -1) {
          this.value = this.value.filter((_, i) => i !== selectionIndex);
          this.onRemove.emit({ newValue: this.value, removed: optionValue });
          if (this.selectionLimit) {
              this.maxSelectionLimitReached = false;
          }
      } else {
          if (!this.selectionLimit || !this.value || this.value.length < this.selectionLimit) {
              this.value = [...(this.value || []), optionValue];
          }
          this.checkSelectionLimit();
      }

      this.onModelChange(this.value);
      this.onChange.emit({ originalEvent: event, value: this.value, itemValue: optionValue });
	}

	selectAllItems(event: MouseEvent): void {
		if (!this.selectAll) {
			super.onToggleAll(event);
		}
		else {
			//They're already all checked, we don't need to do anything
		}
	}

	selectNone(event: MouseEvent): void {
		if (this.selectAll) {
			super.onToggleAll(event);
		}
		else {
			//Mostly copied from the 'toggleall' function here.
			//https://github.com/primefaces/primeng/blob/7.1.3/src/app/components/multiselect/multiselect.ts
			this.value = [];
			this.onModelChange(this.value);
			this.onChange.emit({ originalEvent: event, value: this.value });
			//this.updateFilledState();
			//this.updateLabel();
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

  onMouseclick(event: any, input: any): void{

  }

	@HostListener('document:keydown', ['$event'])
	override onKeyDown(event: KeyboardEvent) {
		if (this.overlayVisible && event.key === 'Enter') {
			this.submit(event);
		}
	}
}
