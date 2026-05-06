import {finalize} from 'rxjs/operators';
import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../core/auth/auth.service';
import { AuthGuard } from '../../core/auth/auth-guard.service';
import { LoadingMaskService } from '../../shared/loading-indicator/loading-mask.service';
import { msalInstance, msalRedirectResult } from '../../core/core.module';

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

	async ngOnInit(): Promise<void> {
		const idToken = await this.resolveIdToken();
		if (idToken) {
			this.loginSSO(idToken);
		} else {
			this.router.navigate(['/login']);
		}
	}

	private async resolveIdToken(): Promise<string | null> {
		if (msalRedirectResult?.idToken) {
			return msalRedirectResult.idToken;
		}
		if (!msalInstance) {
			return null;
		}
		try {
			const redirect = await msalInstance.handleRedirectPromise();
			if (redirect?.idToken) {
				return redirect.idToken;
			}
		} catch {
			
		}
		const accounts = msalInstance.getAllAccounts();
		if (!accounts.length) {
			return null;
		}
		try {
			const silent = await msalInstance.acquireTokenSilent({
				account: accounts[0],
				scopes: ['openid', 'profile'],
			});
			return silent?.idToken ?? null;
		} catch {
			return null;
		}
	}

	private loginSSO(idToken: string): void {
		this.loadingService.addLoading();
		this.authService.loginSSO(idToken).pipe(
			finalize(() => this.loadingService.removeLoading()))
			.subscribe({
				next: () => this.router.navigate(['/' + this.auth.url]),
				error: () => this.router.navigate(['/login']),
			});
	}
}
