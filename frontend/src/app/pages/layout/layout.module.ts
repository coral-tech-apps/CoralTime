import { RouterModule } from '@angular/router';
import { NgModule } from '@angular/core';
import { MenubarModule } from 'primeng/menubar';
import { MenuModule } from 'primeng/menu';
import { ButtonModule } from 'primeng/button';
import { SharedModule } from '../../shared/shared.module';
import { NavigationComponent } from './navigation/navigation.component';

@NgModule({
	imports: [
		MenubarModule,
		ButtonModule,
		MenuModule,
		RouterModule,
		SharedModule,

	],
	exports: [
		NavigationComponent
	],
	declarations: [
		NavigationComponent
	]
})

export class LayoutModule {
}
