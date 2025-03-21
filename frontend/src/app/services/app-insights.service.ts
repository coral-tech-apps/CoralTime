import { Injectable } from "@angular/core";
import { AngularPlugin } from "@microsoft/applicationinsights-angularplugin-js";
import { ApplicationInsights, ICustomProperties, IExceptionTelemetry } from "@microsoft/applicationinsights-web";
import { Router } from '@angular/router';

@Injectable({
  providedIn: 'root'
})

export class AppInsightsService{
  private angularPlugin: AngularPlugin;
  private appInsights: ApplicationInsights | null = null;

  constructor(private router: Router) {
    this.angularPlugin = new AngularPlugin();
    this.appInsights = new ApplicationInsights({
      config: {
        enableAutoRouteTracking: true,
        enableCorsCorrelation: true,
        enableRequestHeaderTracking: true,
        enableResponseHeaderTracking: true,
        extensions: [this.angularPlugin],
        extensionConfig: {
          [this.angularPlugin.identifier]: { router: this.router }
        }
      }
    });
  }

  addInstrumentationKey(instrumentationKey: string){
      this.appInsights.config = {instrumentationKey: instrumentationKey};
      this.appInsights.loadAppInsights();
  }

  setAuthenticatedUser(authenticatedUserId: string, accountId?: string, storeInCookie?: boolean){
      this.appInsights.setAuthenticatedUserContext(authenticatedUserId, accountId);
  }

  clearAuthenticatedUser(){
    this.appInsights.clearAuthenticatedUserContext();
  }

  trackEvent(name: string, properties?: {[key: string]: any}){
    if(this.appInsights){
      this.appInsights.trackEvent({name, properties});
    }
  }

  trackException(exception: IExceptionTelemetry, customProperties?: ICustomProperties){
    if(this.appInsights){
      this.appInsights.trackException(exception, customProperties);
    }
  }

  getInstance(): ApplicationInsights | null {
    return this.appInsights;
  }
}


