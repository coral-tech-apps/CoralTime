
import {map} from 'rxjs/operators';
import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot } from '@angular/router';
import { Observable } from 'rxjs';
import { LoginService, LoginSettings } from './login.service';
import { LoadingMaskService } from '../../shared/loading-indicator/loading-mask.service';

@Injectable()
export class LoginResolve  {
	constructor(private loadingService: LoadingMaskService,
	            private service: LoginService) {
	}

	resolve(route: ActivatedRouteSnapshot): Observable<LoginSettings> {
		this.loadingService.addLoading();
		return this.service.getAuthenticationSettings().pipe(
			map((loginSettings: LoginSettings) => {
				this.loadingService.removeLoadingCompletely();
				return loginSettings;
			}));
	}
}
