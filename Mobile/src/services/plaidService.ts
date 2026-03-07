import apiClient from "./apiClient.ts";

export type CompleteIntegrationRequest = {
    publicToken: string;
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

export type CreateLinkTokenResponse = {
    linkToken: string;
}

export const plaidService = {
    createPlaidLinkToken: () => apiClient.post<CreateLinkTokenResponse>('/api/v1/plaid/create-link-token'),
    completeIntegration: (request :CompleteIntegrationRequest) => apiClient.post('/api/v1/plaid/complete-integration', request),
}
