import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../core/auth/auth.service';
import { AuthGuard } from '../../core/auth/auth-guard.service';

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

	ngOnInit(): void {
		if (this.authService.isLoggedIn()) {
			this.router.navigate(['/' + this.auth.url]);
		} else {
			this.router.navigate(['/login']);
		}
	}
}
