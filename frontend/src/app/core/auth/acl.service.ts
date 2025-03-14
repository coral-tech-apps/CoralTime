import { Injectable } from '@angular/core';
import { AuthService } from './auth.service';
import { ImpersonationService } from '../../services/impersonation.service';

@Injectable()
export class AclService {
	constructor(private authService: AuthService,
	            private impersonationService: ImpersonationService) {
	}

	isGranted(policy: string): boolean {
		if (!this.authService.isLoggedIn()) {
			return false;
		}
		let roles: string[];
        if (this.impersonationService.impersonationUser) {
            roles = Array.isArray(this.impersonationService.impersonationUser.role)
                ? this.impersonationService.impersonationUser.role
                : [this.impersonationService.impersonationUser.role];
        } else {
            roles = Array.isArray(this.authService.authUser.role)
                ? this.authService.authUser.role
                : [this.authService.authUser.role];
        }

		return this.isGrantedForRole(policy, roles);
	}

	isGrantedForRole(policy: string, roles: string[]): boolean {
		return roles.some(role => {
			if(!this.authService.authUser.policies){
				console.log(this.authService.policies);
				console.log(this.authService.authUser.policies);
				return false;
			}
			var policies = this.authService.authUser.policies as string[];
			var isGranted = (policies && policies.indexOf(policy) != -1);
			return isGranted;
        });
	}
}
