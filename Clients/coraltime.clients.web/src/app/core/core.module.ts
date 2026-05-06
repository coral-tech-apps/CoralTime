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
import { PublicClientApplication, BrowserCacheLocation, AuthenticationResult, NavigationClient, NavigationOptions } from '@azure/msal-browser';
import { LoginSettings } from '../pages/login/login.service';

export let msalInstance: PublicClientApplication | null = null;
export let msalRedirectResult: AuthenticationResult | null = null;

class NoReloadNavigationClient extends NavigationClient {
    override async navigateInternal(_url: string, _options: NavigationOptions): Promise<boolean> {
        return false;
    }
}

export function initializeMsal(): () => Promise<void> {
    return async () => {
        const response = await fetch('/api/v1/AuthenticationSettings');
        const settings: LoginSettings = await response.json();

        if (settings.enableAzure && settings.azureSettings) {
            const azure = settings.azureSettings;

            msalInstance = new PublicClientApplication({
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
                    navigationClient: new NoReloadNavigationClient(),
                },
            });

            await msalInstance.initialize();
            const result = await msalInstance.handleRedirectPromise();
            if (result?.idToken) {
                msalRedirectResult = result;
            }
        }
    };
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
        },
    ] })

export class CoreModule {
}
