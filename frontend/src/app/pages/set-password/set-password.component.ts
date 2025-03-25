import { Component, OnInit } from '@angular/core';
import { AuthService } from '../../core/auth/auth.service';

@Component({
    templateUrl: './set-password.component.html',
    standalone: false
})

export class SetPasswordComponent implements OnInit {
	constructor(private authService: AuthService) {
	}

	ngOnInit() {
		this.authService.logout(true);
	}
}
