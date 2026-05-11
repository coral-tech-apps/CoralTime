import { finalize } from 'rxjs/operators';
import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { MsalService } from '@azure/msal-angular';
import { AuthenticationResult } from '@azure/msal-browser';
import { AuthService } from '../../core/auth/auth.service';
import { AuthGuard } from '../../core/auth/auth-guard.service';
import { LoadingMaskService } from '../../shared/loading-indicator/loading-mask.service';

@Component({
    selector: 'ct-signin-oidc',
    templateUrl: 'signin-oidc.component.html',
    standalone: false
})
export class SignInOidcComponent implements OnInit {

	constructor(private auth: AuthGuard,
	            private authService: AuthService,
	            private loadingService: LoadingMaskService,
	            private msalService: MsalService,
	            private router: Router) {
	}

	ngOnInit(): void {
		this.msalService
			.handleRedirectObservable({ navigateToLoginRequestUrl: false })
			.subscribe({
				next: (result: AuthenticationResult | null) => {
					if (result?.idToken) {
						this.loginSSO(result.idToken);
					} else {
						this.router.navigate(['/login']);
					}
				},
				error: () => this.router.navigate(['/login']),
			});
	}

	private loginSSO(idToken: string): void {
		this.loadingService.addLoading();
		this.authService.loginSSO(idToken)
			.pipe(finalize(() => this.loadingService.removeLoading()))
			.subscribe({
				next: () => this.router.navigate(['/' + this.auth.url]),
				error: () => this.router.navigate(['/login']),
			});
	}
}
