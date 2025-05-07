import {finalize} from 'rxjs/operators';
import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { AuthService } from '../../core/auth/auth.service';
import { AuthGuard } from '../../core/auth/auth-guard.service';
import { LoadingMaskService } from '../../shared/loading-indicator/loading-mask.service';

@Component({
    selector: 'ct-signin-oidc',
    templateUrl: 'signin-oidc.component.html',
    standalone: false
})

export class SignInOidcComponent implements OnInit {
	id_token: string;

	constructor(private auth: AuthGuard,
	            private authService: AuthService,
	            private loadingService: LoadingMaskService,
	            private route: ActivatedRoute,
	            private router: Router) {
	}

	ngOnInit() {
		this.route.fragment.subscribe((fragment) => {
			if (fragment) {
				const idTokenMatch = fragment.match(/id_token=([^&]+)/);
				if (idTokenMatch && idTokenMatch[1]) {
					this.id_token = idTokenMatch[1];
					this.loginSSO(this.id_token);
				} else {
					console.error('Invalid fragment format or missing id_token');
					this.router.navigate(['/error']);
				}
			} else {
				console.error('No fragment found in the URL');
				this.router.navigate(['/error']);
			}
		});
	}

	loginSSO(id_token: string): void {
		this.loadingService.addLoading();
		this.authService.loginSSO(id_token).pipe(
			finalize(() => this.loadingService.removeLoading()))
			.subscribe(() => {
					this.router.navigate(['/' + this.auth.url]);
				}
			);
	}
}
