import { api } from "@/api/api";
import { IdToken } from "@auth0/auth0-react";

const controllerPath = "/api/auth";

export const AuthService = {
  sendOAuthIdToken: async (idToken?: IdToken) =>
    await api.post<void>(controllerPath + "/oauth2", idToken),
};
