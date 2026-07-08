import { SharedModule } from '../../shared/shared.module';
import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { WorklogsComponent } from './worklogs.component';
import { WorklogsRoutingModule } from './worklogs-routing.module';
import { RangeDatepickerService } from '../reports/range-datepicker/range-datepicker.service';

@NgModule({
    imports: [
      CommonModule,
      SharedModule,
      WorklogsRoutingModule,
    ],
    declarations: [
      WorklogsComponent,
    ],
    providers: [
      RangeDatepickerService,
    ],
    exports: [
      WorklogsComponent,
    ]
})

export class WorklogsModule{
}
