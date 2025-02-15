"use client";

import { useAuth0 } from "@auth0/auth0-react";
import { useEffect } from "react";
import { AuthInterceptor } from "@/services/auth-interceptor";

export const AuthInjector = () => {
  const { getAccessTokenSilently } = useAuth0();

  useEffect(() => {
    AuthInterceptor.getToken = getAccessTokenSilently;
    return () => (AuthInterceptor.getToken = undefined);
  }, [getAccessTokenSilently]);

  return null;
};
