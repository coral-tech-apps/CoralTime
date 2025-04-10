import { Component, EventEmitter, Output, Input } from '@angular/core';
import dayjs from 'dayjs';
import DayJs = dayjs.Dayjs;
import { DateStatic } from '../../../models/reports';
import { DatePeriod, DateResponse } from './range-datepicker.service';

@Component({
    selector: 'ct-range-datepicker',
    templateUrl: 'range-datepicker.component.html',
    host: {
        '(document:keydown)': 'onKeyDown($event)'
    },
    standalone: false
})

export class RangeDatepickerComponent {
	@Input() firstDayOfWeek: number;
	@Input() datePeriodList: DateStatic[];

	@Input('dateResponse')
	set assignedDatePeriod(dateResponse: DateResponse) {
		this.oldDateResponse = dateResponse;
		this.dateFrom = dateResponse.datePeriod.dateFrom;
		this.dateTo = dateResponse.datePeriod.dateTo;
		this.displayDate = this.dateFrom;
		this.selectedRange = this.getRangeBetweenDates(this.dateFrom.toDate(), this.dateTo.toDate());
	}

	@Output() closed: EventEmitter<void> = new EventEmitter<void>();
	@Output() onPeriodChanged: EventEmitter<DateResponse> = new EventEmitter();

	displayDate: DayJs;
	selectedRange: DayJs[] = [];

	private clickedDay: DayJs;
	private dateFrom: DayJs;
	private dateTo: DayJs;
	private daySelectedNumber: number = 0;
	private oldDateResponse: DateResponse;

	dateOnClick(day: DayJs): void {
		this.clickedDay = day;
		this.daySelectedNumber++;

		switch (this.daySelectedNumber) {
			case 1: {
				this.dateFrom = this.clickedDay;
				this.selectedRange = [this.clickedDay];
				break;
			}
			case 2: {
				if (this.clickedDay.isAfter(this.dateFrom)) {
					this.dateTo = this.clickedDay;
				} else {
					this.dateTo = this.dateFrom;
					this.dateFrom = this.clickedDay;
				}
				this.selectedRange = this.getRangeBetweenDates(this.dateFrom.toDate(), this.dateTo.toDate());
				break;
			}
			default: {
				this.daySelectedNumber = 1;
				this.dateTo = null;
				this.dateFrom = this.clickedDay;
				this.selectedRange = [this.clickedDay];
			}
		}

		this.displayDate = this.clickedDay;
		this.onPeriodChanged.emit({
			datePeriod: new DatePeriod(this.dateFrom, this.dateTo),
			dateStaticId: null
		});
	}

	setPeriod(period: DateStatic): void {
		this.selectedRange = this.getRangeBetweenDates(new Date(period.dateFrom), new Date(period.dateTo));
		this.onPeriodChanged.emit({
			datePeriod: new DatePeriod(dayjs(period.dateFrom), dayjs(period.dateTo)),
			dateStaticId: period.id
		});
		this.closed.emit();
	}

	onKeyDown(event: KeyboardEvent): void {
		if (event.key === 'Escape') {
			this.onPeriodChanged.emit(this.oldDateResponse);
			this.closed.emit();
		}
	}

	private getRangeBetweenDates(dateFrom: Date, dateTo: Date): DayJs[] {
		const listDate = [];
		const dateMove = dateFrom;
		let strDate = dateFrom;

		while (strDate.getTime() <= dateTo.getTime()) {
			strDate = dateMove;
			listDate.push(dayjs(strDate));
			dateMove.setDate(dateMove.getDate() + 1);
		}

		return listDate;
	}
}
