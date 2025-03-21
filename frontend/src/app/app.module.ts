import { BrowserModule } from '@angular/platform-browser';
import { HttpClient } from '@angular/common/http';
import { NgModule } from '@angular/core';
import { TranslateModule, TranslateLoader } from '@ngx-translate/core';
import { TranslateHttpLoader } from '@ngx-translate/http-loader';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';

import { MultiSelectModule } from 'primeng/multiselect';
import { ClientsService } from './services/clients.service';
import { ErrorsModule } from './pages/errors/errors.module';
import { SharedModule } from './shared/shared.module';
import { ProjectRolesService } from './services/project-roles.service';
import { CoreModule } from './core/core.module';
import { AppComponent } from './app.component';
import { AppRoutingModule } from './app-routing.module';
import { LayoutModule } from './pages/layout/layout.module';
import { ProjectsService } from './services/projects.service';
import { TasksService } from './services/tasks.service';
import { UsersService } from './services/users.service';
import { SettingsService } from './services/settings.service';
import { ImpersonationService } from './services/impersonation.service';
import { AdminService } from './services/admin.service';
import { AppInsightsService } from './services/app-insights.service';
import { HttpClientModule } from '@angular/common/http';
import { ErrorHandler } from '@angular/core';
import { CustomErrorHandler } from './core/raven-error-handler';
import { MemberActionsService } from './services/member-action.service';

export function httpFactory(http: HttpClient) {
	return new TranslateHttpLoader(http, 'assets/translate/i18n', '.json');
}

@NgModule({
	declarations: [
		AppComponent
	],
	imports: [
		AppRoutingModule,
		BrowserModule,
		BrowserAnimationsModule,
		CoreModule,
		ErrorsModule,
		LayoutModule,
    MultiSelectModule,
		SharedModule,
    HttpClientModule,
		TranslateModule.forRoot({
			loader: {
				provide: TranslateLoader,
				useFactory: httpFactory,
				deps: [HttpClient]
			}
		}),
	],
	bootstrap: [AppComponent],
	providers: [
    ErrorHandler,
		ClientsService,
		ImpersonationService,
		ProjectRolesService,
		ProjectsService,
		AdminService,
		SettingsService,
		TasksService,
		UsersService,
		MemberActionsService,
    AppInsightsService,
    { provide: ErrorHandler, useClass: CustomErrorHandler }
	]
})

export class AppModule {
}
