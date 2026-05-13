import { NgModule, ErrorHandler, Injector, APP_INITIALIZER } from '@angular/core';
import { HTTP_INTERCEPTORS, provideHttpClient, withInterceptorsFromDi } from '@angular/common/http';
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
import { IPublicClientApplication, PublicClientApplication, BrowserCacheLocation } from '@azure/msal-browser';
import { MSAL_INSTANCE, MsalService, MsalBroadcastService } from '@azure/msal-angular';
import { firstValueFrom } from 'rxjs';
import { LoginSettings } from '../pages/login/login.service';

export let msalInstance: IPublicClientApplication | null = null;
export let isAzureSsoEnabled: boolean = false;

export function initializeMsal(authService: AuthService): () => Promise<void> {
    return async () => {
        const response = await fetch('/api/v1/AuthenticationSettings');
        const settings: LoginSettings = await response.json();

        const azureEnabled = !!(settings.enableAzure && settings.azureSettings);
        isAzureSsoEnabled = azureEnabled;

        const clientId = azureEnabled ? settings.azureSettings.clientId : '00000000-0000-0000-0000-000000000000';
        const authority = azureEnabled
            ? `https://login.microsoftonline.com/${settings.azureSettings.tenant}`
            : 'https://login.microsoftonline.com/common';
        const redirectUri = settings.azureSettings.redirectUrl;

        msalInstance = new PublicClientApplication({
            auth: {
                clientId,
                authority,
                postLogoutRedirectUri: window.location.origin + '/',
                redirectUri,
            },
            cache: {
                cacheLocation: BrowserCacheLocation.LocalStorage,
            },
        });

        await msalInstance.initialize();

        try {
            const result = await msalInstance.handleRedirectPromise({ navigateToLoginRequestUrl: false });
            if (result?.idToken) {
                msalInstance.setActiveAccount(result.account);
                try {
                    await firstValueFrom(authService.loginSSO(result.idToken));
                } catch (err) {
                    console.error('[initializeMsal] loginSSO failed:', err);
                }
            }
        } catch (err) {
            console.error('[initializeMsal] handleRedirectPromise failed:', err);
        }
    };
}

export function msalInstanceFactory(): IPublicClientApplication {
    return msalInstance;
}

@NgModule({ exports: [
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
            provide: APP_INITIALIZER,
            useFactory: initializeMsal,
            multi: true,
            deps: [AuthService],
        },
        {
            provide: MSAL_INSTANCE,
            useFactory: msalInstanceFactory,
        },
        MsalService,
        MsalBroadcastService,
    ]
})

export class CoreModule {
}
