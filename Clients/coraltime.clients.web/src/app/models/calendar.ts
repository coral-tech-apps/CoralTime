import dayjs from 'dayjs';
import DayJs = dayjs.Dayjs;
import { GRAY_COLOR, hexToNumber } from '../shared/form/color-picker/color-picker.component';
import { OUTPUT_DATE_FORMAT } from '../core/constant.service';

export class CalendarDay {
	date: string;
	timeEntries: TimeEntry[];
	plannedTime: number;
	trackedTime: number;

	constructor(data: any) {
		if (!data) {
			return;
		}
		this.date = data.date;
		this.timeEntries = data.timeEnries || [];
		this.plannedTime = data.plannedTime || 0;
		this.trackedTime = data.trackedTime || 0;
	}
}

export class Time {
	hours: string;
	minutes: string;
	seconds: string;
	period: string;

	constructor(hours: string, minutes: string, seconds?: string, period?: string) {
		this.hours = hours;
		this.minutes = minutes;
		this.seconds = seconds;
		this.period = period;
	}
}

export interface TimeOptions {
	isFromToShow: boolean;
	timeTimerStart: number;
}

export interface TimeValues {
	timeActual: number;
	timeEstimated: number;
	timeFrom: number;
	timeTo: number;
}

export class TimeEntry {
	color: number;
	date: string;
	description: string;
	id: number;
	isLocked: boolean;
	isProjectActive: boolean;
	isTaskTypeActive: boolean;
	isUserManagerOnProject: boolean;
	memberId: number;
	memberName: string;
	projectId: number;
	projectName: string;
	jiraWorklogId: string;
	taskName: string;
	taskTypesId: number;
	timeOptions: TimeOptions;
	timeValues: TimeValues;

	constructor(data: any) {
		this.color = data && data.color || hexToNumber(GRAY_COLOR);
		this.date = data && data.date;
		this.description = data && data.description;
		this.id = data && data.id;
		this.isLocked = data && data.isLocked;
		this.isProjectActive = data && data.isProjectActive;
		this.isTaskTypeActive = data && data.isTaskTypeActive;
		this.isUserManagerOnProject = data && data.isUserManagerOnProject;
		this.memberId = data && data.memberId;
		this.memberName = data && data.memberName;
		this.projectId = data && data.projectId;
		this.projectName = data && data.projectName;
		this.taskName = data && data.taskName;
		this.taskTypesId = data && data.taskTypesId;
		this.jiraWorklogId = data && data.jiraWorklogId;
		this.timeOptions = {
			isFromToShow: data && data.timeOptions && data.timeOptions.isFromToShow,
			timeTimerStart: data && data.timeOptions && data.timeOptions.timeTimerStart || 0
		};
		this.timeValues = {
			timeActual: data && data.timeValues && data.timeValues.timeActual || 0,
			timeEstimated: data && data.timeValues && data.timeValues.timeEstimated || 0,
			timeFrom: data && data.timeValues && data.timeValues.timeFrom,
			timeTo: data && data.timeValues && data.timeValues.timeTo
		};
	}
}

export class TimerResponse {
	timeEntry: TimeEntry;
	trackedTime: number;

	constructor(data: any) {
		if (data) {
			this.timeEntry = data.timeEntry ? new TimeEntry(data.timeEntry) : null;
			this.trackedTime = data.trackedTime;
		}
	}
}

export class DateUtils {

	static isToday(date: Date | string): boolean {
		return dayjs().format(OUTPUT_DATE_FORMAT) === dayjs(date).format(OUTPUT_DATE_FORMAT);
	}

	static getSecondsFromStartDay(isUTC?: boolean): number {
		let d = new Date();
		if (isUTC) {
			return d.getHours() * 3600 + d.getMinutes() * 60 + d.getSeconds() + d.getTimezoneOffset() * 60;
		}

		return d.getHours() * 3600 + d.getMinutes() * 60 + d.getSeconds();
	}

	static convertMomentToUTC(dayjs: DayJs): Date {
		let date = dayjs.toDate();
		return new Date(Date.UTC(date.getFullYear(), date.getMonth(), date.getDate()));
	}

	static convertMomentToUTCMoment(m: DayJs): DayJs {
		return dayjs(this.convertMomentToUTC(m));
	}

	static formatDateToString(d: any): string {
		return dayjs(d).format(OUTPUT_DATE_FORMAT);
	}

	static formatStringToDate(d: string): Date {
		return dayjs(d).toDate();
	}

  static reformatDate(d: string, format: string): string {
		return dayjs(d, format).format(OUTPUT_DATE_FORMAT);
  }
}
