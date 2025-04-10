import { AppInsightsService } from '../services/app-insights.service';
import { ODataConfiguration } from '../services/odata';

export function ODataConfigFactory(appInsightsService: AppInsightsService) {
	let config = new ODataConfiguration(appInsightsService);
	config.baseUrl = '/api/v1/odata';

	return config;
}
