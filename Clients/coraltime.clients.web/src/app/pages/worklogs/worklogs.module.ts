import { SharedModule } from '../../shared/shared.module';
import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { WorklogsComponent } from './worklogs.component';
import { WorklogsRoutingModule } from './worklogs-routing.module';
import { RangeDatepickerService } from '../reports/range-datepicker/range-datepicker.service';
import { ReportsService } from 'src/app/services/reposts.service';

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
      ReportsService,
      RangeDatepickerService,
    ],
    exports: [
      WorklogsComponent,
    ]
})

export class WorklogsModule{
}
