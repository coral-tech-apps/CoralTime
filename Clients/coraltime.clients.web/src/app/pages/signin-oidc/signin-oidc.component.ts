import { finalize } from 'rxjs/operators';
import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../core/auth/auth.service';
import { AuthGuard } from '../../core/auth/auth-guard.service';
import { LoadingMaskService } from '../../shared/loading-indicator/loading-mask.service';
import { msalRedirectResult } from '../../core/core.module';

@Component({
    selector: 'ct-signin-oidc',
    templateUrl: 'signin-oidc.component.html',
    standalone: false
})
export class SignInOidcComponent implements OnInit {

	constructor(private auth: AuthGuard,
	            private authService: AuthService,
	            private loadingService: LoadingMaskService,
	            private router: Router) {
	}

	ngOnInit(): void {
		console.log('[signin-oidc] msalRedirectResult:', msalRedirectResult);
		if (msalRedirectResult?.idToken) {
			this.loginSSO(msalRedirectResult.idToken);
		} else {
			console.warn('[signin-oidc] no idToken in msalRedirectResult, navigating to /login');
			this.router.navigate(['/login']);
		}
	}

	private loginSSO(idToken: string): void {
		this.loadingService.addLoading();
		this.authService.loginSSO(idToken)
			.pipe(finalize(() => this.loadingService.removeLoading()))
			.subscribe({
				next: () => this.router.navigate(['/' + this.auth.url]),
				error: (err) => {
					console.error('[signin-oidc] loginSSO failed:', err);
					this.router.navigate(['/login']);
				},
			});
	}
}
