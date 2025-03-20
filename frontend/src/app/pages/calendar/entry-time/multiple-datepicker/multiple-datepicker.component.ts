import { Component, EventEmitter, Output, Input } from '@angular/core';
import { DateUtils, TimeEntry } from '../../../../models/calendar';
import * as dayjs from 'dayjs';
import DayJs = dayjs.Dayjs;

@Component({
	selector: 'ct-multiple-datepicker',
	templateUrl: 'multiple-datepicker.component.html'
})

export class MultipleDatepickerComponent {
	@Input() firstDayOfWeek: number;
	@Input() timeEntry: TimeEntry;

	@Output() onSubmit: EventEmitter<string[]> = new EventEmitter();

	dateList: string[];
	isCalendarShown: boolean = true;

	dateOnChange(date: DayJs[]): void {
		this.dateList = [];
		date.forEach((m: DayJs) => {
			this.dateList.push(DateUtils.formatDateToString(m));
		});
	}

	getHours(time: number = 0): string {
		let hours = Math.floor(time / 3600 );
		return this.formatTime(hours);
	}

	getMinutes(time: number = 0): string {
		let min = Math.floor((time % 3600) / 60) ;
		return this.formatTime(min);
	}

	submit(): void {
		this.isCalendarShown = false;
		this.onSubmit.emit(this.dateList);
	}

	private formatTime(time: number): string {
		return (time >= 0 && time < 10) ? '0' + time : time + '';
	}
}
