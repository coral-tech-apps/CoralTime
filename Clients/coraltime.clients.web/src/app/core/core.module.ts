import { NgModule, ErrorHandler, Injector, APP_INITIALIZER } from '@angular/core';
import { HTTP_INTERCEPTORS, HttpClient, provideHttpClient, withInterceptorsFromDi } from '@angular/common/http';
import { LoadingBarService } from '@ngx-loading-bar/core';
import * as ODataConfig from './odata-config.factory';
import { ODataServiceFactory, ODataConfiguration } from '../services/odata';
import { ConstantService } from './constant.service';
import { AuthGuard } from './auth/auth-guard.service';
import { AuthService } from './auth/auth.service';
import { NotificationService } from './notification.service';
import { NotAuthGuard } from './auth/not-auth-guard.service';
import { AclService } from './auth/acl.service';
import { CustomErrorHandler } from './raven-error-handler';
import { UserPicService } from '../services/user-pic.service';
import { ApplyTokenInterceptor } from './apply-token.interceptor';
import { RefreshTokenInterceptor } from './refresh-token.interceptor';
import { LoadingMaskModule } from '../shared/loading-indicator/loading-mask.module';
import { AppInsightsInterceptor } from './app-insights.interceptor';
import { AppInsightsService } from '../services/app-insights.service';
import { IPublicClientApplication, PublicClientApplication, InteractionType, BrowserCacheLocation } from '@azure/msal-browser';
import { MSAL_GUARD_CONFIG, MSAL_INSTANCE, MsalBroadcastService, MsalGuard, MsalGuardConfiguration, MsalInterceptor, MsalService } from '@azure/msal-angular';
import { LoginSettings } from '../pages/login/login.service';
import { firstValueFrom } from 'rxjs';

export function MSALInstanceFactory(): IPublicClientApplication {
    return new PublicClientApplication({
        auth: {
            clientId: 'placeholder',
        },
        cache: {
            cacheLocation: BrowserCacheLocation.LocalStorage
        },
    });
}

export function initializeMsal(http: HttpClient, msalService: MsalService): () => Promise<void> {
    return async () => {
        const settings = await firstValueFrom(
            http.get<LoginSettings>('/api/v1/AuthenticationSettings')
        );

        if (settings.enableAzure && settings.azureSettings) {
            const azure = settings.azureSettings;

            msalService.instance = new PublicClientApplication({
                auth: {
                    clientId: azure.clientId,
                    authority: `https://login.microsoftonline.com/${azure.tenant}`,
                    postLogoutRedirectUri: window.location.origin + '/',
                    redirectUri: azure.redirectUrl,
                },
                cache: {
                    cacheLocation: BrowserCacheLocation.LocalStorage
                },
                system: {
                    navigatePopups: false
                }
            });
        }

        await msalService.instance.initialize();
    };
};

export const loginRequest = {
    scopes: [],
};

export function MsalGuardConfigurationFactory(): MsalGuardConfiguration {
    return {
        interactionType: InteractionType.Redirect,
        authRequest: loginRequest
    };
};

@NgModule({ exports: [
        //HttpClientModule,
        LoadingMaskModule
    ], 
    imports: [LoadingMaskModule], 
    providers: [
        {
            provide: ErrorHandler,
            useClass: CustomErrorHandler,
            deps: [Injector]
        },
        {
            provide: ODataConfiguration,
            useFactory: ODataConfig.ODataConfigFactory,
            deps: [AppInsightsService]
        },
        {
            provide: HTTP_INTERCEPTORS,
            useClass: ApplyTokenInterceptor,
            multi: true
        },
        {
            provide: HTTP_INTERCEPTORS,
            useClass: RefreshTokenInterceptor,
            multi: true
        },
        {
            provide: HTTP_INTERCEPTORS,
            useClass: AppInsightsInterceptor,
            multi: true
        },
        AclService,
        AuthService,
        AuthGuard,
        ConstantService,
        LoadingBarService,
        NotAuthGuard,
        NotificationService,
        ODataServiceFactory,
        UserPicService,
        provideHttpClient(withInterceptorsFromDi()),
        {
            provide: MSAL_INSTANCE,
            useFactory: MSALInstanceFactory,
        },
        {
            provide: MSAL_GUARD_CONFIG,
            useFactory: MsalGuardConfigurationFactory,
        },
        MsalService,
        MsalBroadcastService,
        MsalGuard,
        {
            provide: APP_INITIALIZER,
            useFactory: initializeMsal,
            deps: [HttpClient, MsalService],
            multi: true,
        },
    ] })

export class CoreModule {
}
