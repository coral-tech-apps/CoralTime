import {
	Component, Input, Output, EventEmitter, forwardRef, HostBinding, ViewChild, AfterContentInit,
  ElementRef,
  OnInit,
  ChangeDetectionStrategy
} from '@angular/core';
import { NG_VALUE_ACCESSOR, ControlValueAccessor } from '@angular/forms';
import { coerceBooleanProperty } from '@angular/cdk/coercion';
import { IDatePickerDirectiveConfig, DatePickerComponent } from 'ng2-date-picker';
import { IDay } from 'ng2-date-picker';
import { WeekDays } from 'ng2-date-picker/lib/common/types/week-days.type';
import dayjs from 'dayjs';
import DayJs =  dayjs.Dayjs;
import { DateUtils } from '../../../models/calendar';

const WEEK_DAYS: WeekDays[] = ['su', 'mo', 'tu', 'we', 'th', 'fr', 'sa'];

export const INPUT_CONTROL_VALUE_ACCESSOR: any = {
	provide: NG_VALUE_ACCESSOR,
	useExisting: forwardRef(() => DatepickerComponent),
	multi: true
};

@Component({
    selector: 'ct-datepicker',
    changeDetection: ChangeDetectionStrategy.OnPush,
    templateUrl: 'datepicker.component.html',
    providers: [INPUT_CONTROL_VALUE_ACCESSOR],
    host: {
        '(document:keydown)': 'onKeyDown($event)'
    },
    standalone: false
})

export class DatepickerComponent implements ControlValueAccessor, OnInit, AfterContentInit {
	@HostBinding('class.ct-datepicker') addClass: boolean = true;
	@Input() config: IDatePickerDirectiveConfig;
	@Input() date: any;
	@Input() disabledDays: string[] = [];
	@Input() displayDate: any;
	@Input() firstDayOfWeek: number = 1;
	@Input() multiselect: boolean = false;
	@Input() required: boolean = false;

  @ViewChild('datepickerContainer', {static: true}) datepickerContainer!: ElementRef;
	@ViewChild('dayPicker', { static: true }) datePicker: DatePickerComponent;
	@Output() closed: EventEmitter<void> = new EventEmitter<void>();
	@Output() dateAPI: EventEmitter<DatePickerComponent> = new EventEmitter();
	@Output() dateChanged: EventEmitter<string[] | DayJs[]> = new EventEmitter();
	@Output() dateClicked: EventEmitter<DayJs> = new EventEmitter();

	private _disabled: boolean = false;

	get disabled(): boolean {
		return this._disabled;
	}

	set disabled(value) {
		this._disabled = coerceBooleanProperty(value);
	}

	private _controlValueAccessorChangeFn: (value: any) => void = () => {};
	private onTouched: () => any = () => {};

	constructor() {
	}

	ngOnInit() {
    this.date = this.convertValueToArrayOfString(this.date);
    this.config = {
      allowMultiSelect: this.multiselect,
      firstDayOfWeek: WEEK_DAYS[this.firstDayOfWeek],
      format: 'YYYY-MM-DD',
      weekDayFormat: 'dd',
      showGoToCurrent: false,
      inputElementContainer: this.datepickerContainer.nativeElement,
    };
    this.dateAPI.emit(this.datePicker);
  }

  ngAfterContentInit(): void {
    setTimeout(() => {
      if (this.datePicker?.api) {
        this.datePicker.api.open();
      }
      if (this.datePicker) {
        this.datePicker.onBodyClick = this.onBodyClick.bind(this.datePicker);
        this.datePicker.onSelect.subscribe((day: IDay) => {
          this.dateClicked.emit(day.date);
          this.handleDateSelection(day);
        });
        this.datePicker.onChange.subscribe((value: string[] | DayJs[]) => {
          if (value) {
            this.date = Array.isArray(value) ?
              value.map(d => typeof d === 'string' ? d : d.format('YYYY-MM-DD')) :
              [value];
            this.changeSelectedDate(this.date);
          }
        });
      }
      if (this.datePicker?.dayCalendarRef?.dayCalendarService) {
        this.datePicker.dayCalendarRef.dayCalendarService.isDateDisabled =
          (date: DayJs, config: any) => this.checkDisabledDate(date);
      }
      if (this.datePicker?.dayCalendarRef) {
        if (typeof this.datePicker.dayCalendarRef.dayClicked === 'function') {
          this.datePicker.dayCalendarRef.dayClicked = this.dayClicked.bind(this);
        }
      }
    }, 0);
  }

	/**
	 * Rewrite default methods
	 */

	onBodyClick(this): void {
		this.hideStateHelper = false;
	}

  private checkDisabledDate(date: DayJs): boolean {
    const mockDay = { date: date };
    if (this.isDateInDisabledList(mockDay as any, this.disabledDays)) {
      return true;
    }

    if (this.config.min && date.isBefore(this.config.min, 'day')) {
      return true;
    }

    return !!(this.config.max && date.isAfter(this.config.max, 'day'));
  }

  isDateDisabled(day: IDay): boolean {
    if (this.config.isDayDisabledCallback && day.date) {
      return this.config.isDayDisabledCallback(day.date);
    }

    return this.checkDisabledDate(day.date);
  }

  isDateInDisabledList(day: { date: DayJs }, disabledList: string[]): boolean {
    if (!disabledList || !day.date) {
      return false;
    }

    let result: boolean = false;
    disabledList.forEach((date: string) => {
      if (dayjs(day.date).format('YYYY-MM-DD') === dayjs(date).format('YYYY-MM-DD')) {
        result = true;
      }
    });

    return result;
  }

  private handleDateSelection(day: IDay): void {
    if (!this.multiselect) {
      this.date = [day.date.format('YYYY-MM-DD')];
    } else {
      const dateStr = day.date.format('YYYY-MM-DD');
      const index = this.date.indexOf(dateStr);
      if (index > -1) {
        this.date.splice(index, 1);
      } else {
        this.date.push(dateStr);
      }
    }
    this.changeSelectedDate(this.date);
  }

  dayClicked(day: IDay): void {
    this.dateClicked.emit(day.date);
    this.handleDateSelection(day);
  }

	/**
	 * Implemented as part of ControlValueAccessor.
	 */
	writeValue(date: any) {
      //this.date = this.convertValueToArrayOfString(date);
			this._controlValueAccessorChangeFn(this.date);
	}

	/**
	 * Implemented as part of ControlValueAccessor.
	 */
	registerOnChange(fn: (value: any) => void) {
		this._controlValueAccessorChangeFn = fn;
	}

	/**
	 * Implemented as part of ControlValueAccessor.
	 */
	registerOnTouched(fn: any) {
		this.onTouched = fn;
	}

	/**
	 * Implemented as part of ControlValueAccessor.
	 */
	setDisabledState(isDisabled: boolean) {
		this.disabled = isDisabled;
	}

	_onInputBlur() {
		this.onTouched();
	}

	onKeyDown(event: KeyboardEvent) {
		if (event.key === 'Escape') {
			this.closed.emit();
		}
	}

	changeSelectedDate(date: string[]): void {
		this._controlValueAccessorChangeFn(date);
    this.dateChanged.emit(date);
	}

	private convertValueToArrayOfString(date: any): string[] {
		if (!this.multiselect) {
			date = date ? [date] : [];
		} else {
			date = date ? [...date] : [];
		}
		return date.map(date => DateUtils.formatDateToString(date));
	}
}
