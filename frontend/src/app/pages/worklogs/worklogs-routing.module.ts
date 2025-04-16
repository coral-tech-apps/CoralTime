import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { AuthGuard } from '../../core/auth/auth-guard.service';
import { WorklogsComponent } from './worklogs.component';
import { UserInfoResolve } from 'src/app/core/auth/user-info-resolve.service';

const routes: Routes = [
	{
		path: '',
		component: WorklogsComponent,
		canActivate: [AuthGuard],
    data: {
      title: 'Worklogs'
    },
    resolve:{
      user: UserInfoResolve
    }
	}
];

@NgModule({
	imports: [RouterModule.forChild(routes)],
	exports: [RouterModule],
  providers: [
		UserInfoResolve
	]
})

export class WorklogsRoutingModule {
}
