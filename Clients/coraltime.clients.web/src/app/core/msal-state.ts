import { IPublicClientApplication, AuthenticationResult } from '@azure/msal-browser';

export let msalInstance: IPublicClientApplication | null = null;
export let msalRedirectResult: AuthenticationResult | null = null;
export let isAzureSsoEnabled: boolean = false;

export function setMsalInstance(instance: IPublicClientApplication): void {
    msalInstance = instance;
}

export function setMsalRedirectResult(result: AuthenticationResult): void {
    msalRedirectResult = result;
}

export function setAzureSsoEnabled(enabled: boolean): void {
    isAzureSsoEnabled = enabled;
}
