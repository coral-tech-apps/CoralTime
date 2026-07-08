import { Injectable } from '@angular/core';
import { DateStatic } from '../../../models/reports';
import dayjs from 'dayjs';
import DayJs = dayjs.Dayjs;
import { CompanySettingsService } from 'src/app/services/company-settings.service';

export class DatePeriod {
	dateFrom: DayJs;
	dateTo: DayJs;

	constructor(dateFrom: DayJs, dateTo?: DayJs) {
		this.dateFrom = dateFrom;
		this.dateTo = dateTo || dateFrom;
	}
}

export interface DateResponse {
	datePeriod: DatePeriod;
	dateStaticId: number
}

export enum DatesStaticIds {
	Lifetime = 0,
	Today = 1,
	ThisWeek = 2,
	ThisMonth = 3,
	ThisYear = 4,
	Yesterday = 5,
	LastWeek = 6,
	LastMonth = 7,
	LastYear = 9,
	ThisQuarter = 10,
	LastQuarter = 11,
}

@Injectable()
export class RangeDatepickerService {
	dateStaticList: DateStatic[];

	constructor(
		private companySettingsService: CompanySettingsService
	) {
		this.dateStaticList = this.buildDateStaticList(this.companySettingsService.startOfWeek);
	}

	private buildDateStaticList(startOfWeek: number, today: Date = new Date()): DateStatic[] {
		const base = dayjs(today).startOf('day');

		const thisWeekFrom = this.startOfWeekDay(base, startOfWeek);
		const thisWeek = { from: thisWeekFrom, to: thisWeekFrom.add(7, 'day').subtract(1, 'millisecond') };
		const lastWeekFrom = thisWeekFrom.subtract(7, 'day');
		const lastWeek = { from: lastWeekFrom, to: lastWeekFrom.add(7, 'day').subtract(1, 'millisecond') };

		const thisMonth = { from: base.startOf('month'), to: base.endOf('month').startOf('day').add(1, 'day').subtract(1, 'millisecond') };
		const lastMonthBase = base.subtract(1, 'month');
		const lastMonth = { from: lastMonthBase.startOf('month'), to: lastMonthBase.endOf('month').startOf('day').add(1, 'day').subtract(1, 'millisecond') };

		const thisQuarter = this.quarterRange(base);
		const lastQuarter = this.quarterRange(base.subtract(3, 'month'));

		const thisYear = { from: base.startOf('year'), to: base.endOf('year').startOf('day').add(1, 'day').subtract(1, 'millisecond') };
		const lastYearBase = base.subtract(1, 'year');
		const lastYear = { from: lastYearBase.startOf('year'), to: lastYearBase.endOf('year').startOf('day').add(1, 'day').subtract(1, 'millisecond') };

		const today0 = { from: base, to: base };
		const yesterdayBase = base.subtract(1, 'day');
		const yesterday = { from: yesterdayBase, to: yesterdayBase };
		const lifetimeMin = dayjs(new Date(1, 0, 1)).year(1);
		const lifetime = { from: lifetimeMin, to: lifetimeMin };

		this.dateStaticList = [
			this.toDateStatic(DatesStaticIds.ThisWeek, 'This Week', thisWeek),
			this.toDateStatic(DatesStaticIds.ThisMonth, 'This Month', thisMonth),
			this.toDateStatic(DatesStaticIds.ThisQuarter, 'This Quarter', thisQuarter),
			this.toDateStatic(DatesStaticIds.ThisYear, 'This Year', thisYear),
			this.toDateStatic(DatesStaticIds.LastWeek, 'Last Week', lastWeek),
			this.toDateStatic(DatesStaticIds.LastMonth, 'Last Month', lastMonth),
			this.toDateStatic(DatesStaticIds.LastQuarter, 'Last Quarter', lastQuarter),
			this.toDateStatic(DatesStaticIds.LastYear, 'Last Year', lastYear),
			this.toDateStatic(DatesStaticIds.Today, 'Today', today0),
			this.toDateStatic(DatesStaticIds.Yesterday, 'Yesterday', yesterday),
			this.toDateStatic(DatesStaticIds.Lifetime, 'Lifetime', lifetime),
		];

		return this.dateStaticList;
	}

	private startOfWeekDay(date: DayJs, startOfWeek: number): DayJs {
		let diff = date.day() - startOfWeek;
		if (diff < 0) {
			diff += 7;
		}
		return date.subtract(diff, 'day').startOf('day');
	}

	private quarterRange(date: DayJs): { from: DayJs, to: DayJs } {
		const quarterId = Math.floor(date.month() / 3);
		const from = dayjs(new Date(date.year(), quarterId * 3, 1)).startOf('day');
		const to = from.add(3, 'month').subtract(1, 'millisecond');
		return { from, to };
	}

	private toDateStatic(id: DatesStaticIds, description: string, range: { from: DayJs, to: DayJs }): DateStatic {
		return {
			id,
			description,
			dateFrom: range.from.toISOString(),
			dateTo: range.to.toISOString()
		};
	}

	setDateStringPeriod(period: DatePeriod): string {
		for (let dateStatic of this.dateStaticList) {
			if (this.isDatePeriodEqual(period, dateStatic)) {
				return dateStatic.description;
			}
		}

		if (this.isFromPeriod(period, 1)) {
			let weekDayFrom = period.dateFrom.toDate().toLocaleString('en-us', {weekday: 'short'});
			let weekDayTo = period.dateTo.toDate().toLocaleString('en-us', {weekday: 'short'});

			return this.isSingleDay(period) ? weekDayFrom : weekDayFrom + ' - ' + weekDayTo;
		}

		let dateString: string;
		let monthFormat: 'long' | 'short' = 'long';
		let monthNameFrom = period.dateFrom.toDate().toLocaleString('en-us', {month: monthFormat});
		let yearFrom = period.dateFrom.year();

		if (this.isFromOneMonth(period)) {
			dateString = monthNameFrom + ' ' + this.uniteDays(period);
			return this.isFromPeriod(period, 3) ? dateString : dateString + ' ' + yearFrom;
		}

		monthFormat = this.isIntegerNumberOfMonths(period) ? 'long' : 'short';
		let monthDayFrom = period.dateFrom.toDate().getDate();
		let monthDayTo = period.dateTo.toDate().getDate();
		monthNameFrom = period.dateFrom.toDate().toLocaleString('en-us', {month: monthFormat});
		let monthNameTo = period.dateTo.toDate().toLocaleString('en-us', {month: monthFormat});

		if (this.isFromOneYear(period)) {
			dateString = monthNameFrom + ' ' + monthDayFrom + ' - ' + monthNameTo + ' ' + monthDayTo;
			return this.isFromPeriod(period, 3) ? dateString : dateString + ', ' + period.dateFrom.year();
		}

		let yearTo = period.dateTo.year();

		return monthNameFrom + ' ' + monthDayFrom + ', ' + yearFrom + ' - ' + monthNameTo + ' ' + monthDayTo + ', ' + yearTo;
	}

	isDatePeriodEqual(a: DatePeriod, b: DateStatic): boolean {
		return a.dateFrom.toDate().toDateString() === new Date(b.dateFrom).toDateString()
			&& a.dateTo.toDate().toDateString() === new Date(b.dateTo).toDateString()
	}

	isIntegerNumberOfMonths(period: DatePeriod): boolean {
		let nextDay = (d => new Date(d.setDate(d.getDate() + 1)))(period.dateTo.toDate());
		return period.dateFrom.date() === 1 && nextDay.getDate() === 1;
	}

	private isFromPeriod(period: DatePeriod, periodId: number): boolean {
		return period.dateFrom.toDate().getTime() >= new Date(this.dateStaticList[periodId].dateFrom).getTime() &&
			period.dateTo.toDate().getTime() <= new Date(this.dateStaticList[periodId].dateTo).getTime();
	}

	private isFromOneMonth(period: DatePeriod): boolean {
		let d: Date = period.dateFrom.toDate();
		let monthBeginDay: Date = new Date(d.getFullYear(), d.getMonth(), 1);
		let monthEndDay: Date = new Date(d.getFullYear(), d.getMonth() + 1, 1, 0, 0, 0, -1);

		return period.dateFrom.toDate().getTime() >= monthBeginDay.getTime() &&
			period.dateTo.toDate().getTime() <= monthEndDay.getTime();
	}

	private isFromOneYear(period: DatePeriod): boolean {
		let d: Date = period.dateFrom.toDate();
		let yearBeginDay: Date = new Date(d.getFullYear(), 0, 1);
		let yearEndDay: Date = new Date(d.getFullYear() + 1, 1, 0, 0, 0, -1);

		return period.dateFrom.toDate().getTime() >= yearBeginDay.getTime() &&
			period.dateTo.toDate().getTime() <= yearEndDay.getTime();
	}

	private isSingleDay(period: DatePeriod): boolean {
		return period.dateFrom.toDate().toDateString() === period.dateTo.toDate().toDateString();
	}

	private uniteDays(period: DatePeriod): string {
		let showDays = !this.isIntegerNumberOfMonths(period);
		let monthDayFrom = period.dateFrom.toDate().getDate();
		let monthDayTo = !this.isSingleDay(period) ? period.dateTo.toDate().getDate() : 0;

		return showDays ? (monthDayTo > 0 ? monthDayFrom + ' - ' + monthDayTo : monthDayFrom + '') : '';
	}
}
