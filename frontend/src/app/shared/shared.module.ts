//import { DataTableModule } from './datatable/datatable';
import { TableModule } from 'primeng/table';
import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MultiSelectModule } from 'primeng/multiselect';
import { DropdownModule } from 'primeng/dropdown';
import { SharedModule as PrimeNgSharedModule } from 'primeng/api';
import { PaginatorModule } from 'primeng/paginator';
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import { CalendarModule } from 'primeng/calendar';
import { ReadMoreComponent } from './read-more/read-more.component';
import { DirectivesModule } from './directives/directives.module';
import { DatepickerModule } from './form/datepicker/datepicker.module';
import { MenuModule } from './menu/menu.module';
import { SharedFormModule } from './form/shared-form.module';
import { UserPicComponent } from './user-pic/user-pic.component';
import { ConfirmationComponent } from './confirmation/confirmation.component';
import { ChartComponent } from './chart/chart.component';
import { TranslateModule } from '@ngx-translate/core';
import { MarkdownModule, MarkedOptions } from 'ngx-markdown';


@NgModule({
	imports: [
		CommonModule,
		PaginatorModule,
		PrimeNgSharedModule,
		ButtonModule,
		DialogModule,
		DropdownModule,
		CalendarModule,
		DirectivesModule,
		DatepickerModule,
		TableModule,
		MenuModule,
    MultiSelectModule,
		SharedFormModule,
		MarkdownModule.forRoot({
			markedOptions: {
				provide: MarkedOptions,
				useValue: {
					breaks: true,
					headerIds: false,
					pedantic: true,
				},
			},
		}),
	],
	declarations: [
		ReadMoreComponent,
		ConfirmationComponent,
		UserPicComponent,
		ChartComponent,
	],
	exports: [
		CommonModule,
		PaginatorModule,
		PrimeNgSharedModule,
		ButtonModule,
		DialogModule,
		DropdownModule,
		CalendarModule,
		ReadMoreComponent,
		DirectivesModule,
		DatepickerModule,
		TableModule,
		MenuModule,
    MultiSelectModule,
		SharedFormModule,
		UserPicComponent,
		ConfirmationComponent,
		ChartComponent,
		TranslateModule,
		MarkdownModule,
	]
})

export class SharedModule {
}
