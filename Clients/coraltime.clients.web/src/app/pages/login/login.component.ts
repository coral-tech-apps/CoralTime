
import {finalize, switchMap, tap} from 'rxjs/operators';
import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { AuthService } from '../../core/auth/auth.service';
import { AuthGuard } from '../../core/auth/auth-guard.service';
import { LoginSettings } from './login.service';
import { LoadingMaskService } from '../../shared/loading-indicator/loading-mask.service';
import { AppInsightsService } from 'src/app/services/app-insights.service';
import { MsalService } from '@azure/msal-angular';
	
@Component({
    templateUrl: 'login.component.html',
    standalone: false
})

export class LoginComponent implements OnInit {
	enableAzure: boolean = false;
	errorMessage: string;
	password: string;
	username: string;

	constructor(private authService: AuthService,
	            private auth: AuthGuard,
	            private loadingService: LoadingMaskService,
	            private route: ActivatedRoute,
	            private router: Router,
							private msalService: MsalService,
                private appInsightsService: AppInsightsService) {
	}

	ngOnInit() {
		this.route.data.forEach((data: { loginSettings: LoginSettings }) => {
			this.setupAppInsights(data.loginSettings.instrumentationKey);
			if (data.loginSettings.enableAzure) {
				this.enableAzure = true;
			}
		});
	}

	login(): void {
		this.errorMessage = null;
		this.loadingService.addLoading();
		this.authService.login(this.username, this.password).pipe(
			finalize(() => this.loadingService.removeLoading()))
			.subscribe(
				data => this.router.navigateByUrl('/' + this.auth.url),
				error => this.handleError(error)
			);
	}

	loginSSO(): void {
		this.errorMessage = null;
		this.loadingService.addLoading();
		this.msalService.loginPopup({
			scopes: ['openid', 'profile']
		}).pipe(
			finalize(() => this.loadingService.removeLoading()),
			tap(result => this.msalService.instance.setActiveAccount(result.account)),
			switchMap(result => this.authService.loginSSO(result.idToken)))
			.subscribe({
				next: (result) => {
					if (result) {
						this.router.navigateByUrl('/' + this.auth.url);
					} else {
						this.errorMessage = 'Authentication failed';
					}
				},
				error: () => {
					this.loadingService.removeLoading();
					this.errorMessage = 'Azure login was cancelled or failed';
				}
			});
	}

	private handleError(error: any): void {
		if (this.username.length < 1 && this.password.length < 1) {
			this.errorMessage = 'Login and password are required!';
		} else if (this.username.length < 1 && this.password.length > 1) {
			this.errorMessage = 'Login is required!';
		} else if (this.password.length < 1 && this.username.length > 1) {
			this.errorMessage = 'Password is required!';
		} else {
			this.errorMessage = error.status === 400 ? 'Invalid username or password' : 'Server error';
		}

    this.appInsightsService.trackException({ //
      exception: error,
      properties:{
        comonent: 'login.component',
        login: this.username,
        errorMessage: this.errorMessage,
        error_description: error?.error?.error_description
      }
    });

/*
        this.appInsightsService.trackException(
        	error,
			'login.component',
			{
				'login': this.username,
				'errorMessage': this.errorMessage,
				'error_description': error.error.error_description
            })*/
	}

	private setupAppInsights(instrumentationKey: string ): void {
    localStorage.setItem('instrumentationKey', instrumentationKey);
		
		if (instrumentationKey!= null && instrumentationKey !='') {
        this.appInsightsService.addInstrumentationKey(instrumentationKey);
    }
	}
}
