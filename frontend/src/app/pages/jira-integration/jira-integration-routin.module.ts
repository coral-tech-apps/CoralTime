import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { AuthGuard } from '../../core/auth/auth-guard.service';
import { JiraIntegrationComponent } from './jira-integration.component';

const routes: Routes = [
	{
		path: '',
		component: JiraIntegrationComponent,
		canActivate: [AuthGuard],
	}
];

@NgModule({
	imports: [RouterModule.forChild(routes)],
	exports: [RouterModule]
})

export class JiraIntegrationRoutingModule {
}
