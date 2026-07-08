import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { map, shareReplay } from 'rxjs/operators';
import { ConstantService } from '../core/constant.service';

@Injectable({ providedIn: 'root' })
export class CompanySettingsService {
	public startOfWeek: number = 0;

	constructor(private constantService: ConstantService,
	            private http: HttpClient) {
	}

	public getStartOfWeek(): void {
		this.http.get<{ startOfWeek: number }>(this.constantService.companySettingsApi + '/WeekStart').pipe(
			map(res => res.startOfWeek),
			shareReplay(1)
		).subscribe(startOfWeek => {
			this.startOfWeek = startOfWeek;
		});
	}
}
