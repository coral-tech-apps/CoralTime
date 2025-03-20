import { ErrorHandler, Injectable, Injector  } from '@angular/core';
import { AppInsightsService } from '../app-insights.service';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})

export class CustomErrorHandler implements ErrorHandler {
     constructor(private injector: Injector) {
     }

	handleError(err: any): void {
	    const appInsightsService = this.injector.get(AppInsightsService);
    	appInsightsService.trackException(err);

		if (!environment.production) {
			throw err;
		}
	}
}
