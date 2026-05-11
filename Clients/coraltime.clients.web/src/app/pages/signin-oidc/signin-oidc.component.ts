import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../core/auth/auth.service';
import { AuthGuard } from '../../core/auth/auth-guard.service';
import { msalInstance, msalRedirectResult } from '../../core/core.module';

@Component({
    selector: 'ct-signin-oidc',
    templateUrl: 'signin-oidc.component.html',
    standalone: false
})
export class SignInOidcComponent implements OnInit {

	constructor(private auth: AuthGuard,
	            private authService: AuthService,
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
			const result = await msalInstance.handleRedirectPromise();
			if (result?.idToken) {
				return result.idToken;
			}
		} catch {
		}
		try {
			const accounts = msalInstance.getAllAccounts();
			if (accounts.length > 0) {
				const silent = await msalInstance.acquireTokenSilent({
					scopes: ['openid', 'profile'],
					account: accounts[0],
				});
				if (silent?.idToken) {
					return silent.idToken;
				}
			}
		} catch {
		}
		return null;
	}

	private loginSSO(idToken: string): void {
		this.authService.loginSSO(idToken)
			.subscribe({
				next: () => this.router.navigate(['/' + this.auth.url]),
				error: () => this.router.navigate(['/login']),
			});
	}
}
