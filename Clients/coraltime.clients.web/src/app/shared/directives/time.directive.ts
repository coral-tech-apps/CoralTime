import { Directive, ElementRef, HostListener, Output, EventEmitter, Input, Optional, Self } from '@angular/core';
import { NgControl } from '@angular/forms';

@Directive({
    selector: '[ctTime]',
    standalone: false
})

export class TimeDirective {
	@Input() min: number = 0;
	@Input() max: number = 59;
	@Input() step: number = 1;
	/**
	 * Format pattern for output. Use '#' for each digit.
	 * '##' — pad to 2 digits (e.g. 5 → '05')
	 * '###' — pad to 3 digits (e.g. 5 → '005')
	 * '#' or '' — no padding
	 */
	@Input() format: string = '##';
	@Output() timeChanged: EventEmitter<any> = new EventEmitter();

	private oldValue: string = '';

	constructor(
		private el: ElementRef,
		@Optional() @Self() private ngControl: NgControl
	) {}

	@HostListener('keypress', ['$event'])
	onKeyPress(event: KeyboardEvent) {
		//"keypress" & "beforeinput" essentially do the same thing however we need both for this to work on all browsers.
		//Chrome on Android does not support "keypress".
		//MS Edge & Firefox on desktops do not support "beforeinput".
		switch (event.key) {
			case 'Enter' :
				return true;
			default:
				return this.handleBeforeInput(event.key);
		}
	}

	@HostListener('beforeinput', ['$event'])
	onBeforeInput(event: InputEvent) {
		return this.handleBeforeInput(event.data);
	}
   
	@HostListener('input', ['$event'])
	onInput(event: InputEvent) {
		this.handleInput(this.el.nativeElement.value)
	}

	@HostListener('focus')
	onFocus() {
		this.oldValue = this.el.nativeElement.value;
		this.el.nativeElement.setSelectionRange(0, 0); 
	}

	@HostListener('blur')
	onBlur() {
		let current = this.el.nativeElement.value.trim();
		let time = (current.length == 0) ? +this.oldValue : +current;
		this.processChange(time);
	}

	private get maxInputLength(): number {
		return this.format ? Math.max(this.format.length, 1) : this.max.toString().length;
	}

	private handleBeforeInput(data: string): boolean {
		switch (data) {
			case '0' :
			case '1' :
			case '2' :
			case '3' :
			case '4' :
			case '5' :
			case '6' :
			case '7' :
			case '8' :
			case '9' :
				if (this.el.nativeElement.value.trim().length >= this.maxInputLength) {
					this.el.nativeElement.value = data;
					this.handleInput(data);
					return false;
				}
				return true;
			default:
				return false;
		}
	}

	private handleInput(data: string): void {
		var current = data.trim();
		if (current.length < this.maxInputLength) {
			let nextPossible = +(current + '0');
			if (nextPossible > this.max) {
				this.processChange(+current);
			}
		}
		else if (current.length >= this.maxInputLength) {
			this.processChange(+current);
		}
	}

	private formatTime(time: number): string {
		const padLength = this.format ? this.format.length : 0;
		if (padLength > 1) {
			return time.toString().padStart(padLength, '0');
		}
		return time.toString();
	}

	private limitTime(time: number): number {
		if (time < this.min) {
			return this.min;
		}
		else if (time > this.max) {
			return this.max;
		}
		else {
			return time;
		}
	}

	private processChange(time: number): void {
		let current: string = this.formatTime(this.limitTime(time));
		this.el.nativeElement.value = current;
		if (this.ngControl?.control) {
			this.ngControl.control.setValue(current, { emitEvent: false });
		}
		this.timeChanged.emit(current);
	}
}
