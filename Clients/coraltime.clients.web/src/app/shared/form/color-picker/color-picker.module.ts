import { provideNgxMask } from 'ngx-mask';
import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ColorPickerModule as ColorModule } from 'primeng/colorpicker';
import { ColorPickerComponent } from './color-picker.component';

@NgModule({
	imports: [
		FormsModule,
		ColorModule,
	],
  providers: [
    provideNgxMask()
  ],
	declarations: [
		ColorPickerComponent
	],
	exports: [
		ColorPickerComponent
	]
})

export class ColorPickerModule {
}
