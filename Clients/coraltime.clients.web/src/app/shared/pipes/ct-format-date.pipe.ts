import { Pipe, PipeTransform } from '@angular/core';
import dayjs from 'dayjs';

@Pipe({
  name: 'ctFormatDate'
})
export class CtFormatDatePipe implements PipeTransform {

  transform(value: string, format: string): string {
    if (!value) {
      return '';
    }
    let date = dayjs(value);
    return format ? date.format(format) : date.toDate().toLocaleDateString();
  }

}
