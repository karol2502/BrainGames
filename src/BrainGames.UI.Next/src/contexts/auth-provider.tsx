"use client";

import React from "react";
import { Auth0Provider } from "@auth0/auth0-react";
import { AuthInjector } from "@/components/auth-injector";

type Props = {
  children: React.ReactNode;
};

export const AuthProvider = ({ children }: Props) => {
  return (
    <Auth0Provider
      domain="karolangrys.eu.auth0.com"
      clientId="1W1vfgkIOwA4j68RSjZAQ0bMhA8fuOdj"
      authorizationParams={{
        redirect_uri: "http://localhost:3000/login/successful",
        audience: "https://karolangrys.eu.auth0.com/api/v2/",
      }}
      useRefreshTokens
      useRefreshTokensFallback
    >
      <AuthInjector />
      {children}
    </Auth0Provider>
  );
};
