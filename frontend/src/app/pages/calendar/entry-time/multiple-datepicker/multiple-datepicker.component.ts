import { Component, EventEmitter, Output, Input, ViewContainerRef, ViewChild, ElementRef, TemplateRef } from '@angular/core';
import { DateUtils, TimeEntry } from '../../../../models/calendar';
import dayjs from 'dayjs';
import DayJs = dayjs.Dayjs;

@Component({
    selector: 'ct-multiple-datepicker',
    templateUrl: 'multiple-datepicker.component.html',
    standalone: false
})

export class MultipleDatepickerComponent {
	@Input() firstDayOfWeek: number;
	@Input() timeEntry: TimeEntry;
  isProcessing: boolean = false;

	@Output() onSubmit: EventEmitter<string[]> = new EventEmitter();

	dateList: string[] = [];
  selectedDate: DayJs[] = [];

	isCalendarShown: boolean = true;
  isValidForm: boolean = false

	dateOnChange(date: DayJs[]): void {
    if(this.isProcessing) return;
    this.isProcessing = true;
    date.forEach((m: DayJs) => {
      const formateDate = DateUtils.formatDateToString(m);

      if(!this.dateList.includes(formateDate)){
        this.dateList.push(formateDate);
        this.selectedDate.push(m);
      }
      this.selectedDate = date;
    });
    this.checkValidForm();
    this.isProcessing = false
	}

  checkValidForm(): void{
    if(this.dateList.length > 0){
      this.isValidForm = true;
    }else{
      this.isValidForm = false;
    }
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
