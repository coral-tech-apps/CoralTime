import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MultiSelectComponent } from './multiselect/multiselect.component';
import { MultiSelectModule } from 'primeng/multiselect';
import { SelectComponent } from './select/select.component';
import { DirectivesModule } from '../directives/directives.module';
import { TextareaComponent } from './textarea/textarea.component';
import { InputListComponent } from './input-list/input-list.component';
import { JiraSettings } from './jira-settings/jira-settings.component';
import { JiraSettingFormComponent } from './jira-settings/form/jira-settings-form.component';
import { NgxMaskDirective, provideNgxMask } from 'ngx-mask';
import { NgSlimScrollModule } from 'ngx-slimscroll';
import { MaterialModule } from '../material.module';
import { ColorPickerModule } from './color-picker/color-picker.module';
import { TableModule } from 'primeng/table';

@NgModule({
	imports: [
    CommonModule,
    DirectivesModule,
    FormsModule,
    MaterialModule,
    NgSlimScrollModule,
    ReactiveFormsModule,
    MultiSelectModule,
    ColorPickerModule,
    NgxMaskDirective,
    TableModule
],
    providers: [
      provideNgxMask()
    ],
	declarations: [
		InputListComponent,
		MultiSelectComponent,
		SelectComponent,
    JiraSettings,
    JiraSettingFormComponent,
		TextareaComponent
	],
	exports: [
		FormsModule,
		MaterialModule,
		ReactiveFormsModule,
		InputListComponent,
		MultiSelectComponent,
		SelectComponent,
    JiraSettings,
    JiraSettingFormComponent,
		TextareaComponent,
		ColorPickerModule,
		NgSlimScrollModule
	]
})

export class SharedFormModule {
}
