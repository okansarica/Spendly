// CHANGED_BY_AI: 2026-03-10 - Add Plaid mode-aware requests for update flow
import apiClient from "./apiClient.ts";

export type PlaidFlowMode = 'create' | 'update';

export type CompleteIntegrationRequest = {
    publicToken: string;
    mode?: PlaidFlowMode;
    bankId?: string;
    accounts: {
        verificationStatus?:  string | undefined;
        type: string;
        mask?:  string | undefined;
        name?:  string | undefined;
        subtype: string;
        id: string;
    }[],
    institution:{
        name:string,
        id:string,
    },
    linkSessionId:string,
}

export type CreateLinkTokenRequest = {
    mode?: PlaidFlowMode;
    bankId?: string;
}

export type CreateLinkTokenResponse = {
    linkToken: string;
}

export const plaidService = {
    createPlaidLinkToken: (request?: CreateLinkTokenRequest) => apiClient.post<CreateLinkTokenResponse>('/api/v1/plaid/create-link-token', request ?? {}),
    completeIntegration: (request :CompleteIntegrationRequest) => apiClient.post('/api/v1/plaid/complete-integration', request),
    removeIntegration: (bankId: string) => apiClient.post('/api/v1/plaid/remove-integration', { bankId }),
}
