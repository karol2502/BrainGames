"use client";

import React, { useEffect } from "react";
import { useAuth0 } from "@auth0/auth0-react";
import { setCookie } from "cookies-next";

export default function AppLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  const { getAccessTokenSilently } = useAuth0();

  useEffect(() => {
    const getUserMetadata = async () => {
      try {
        const accessToken = await getAccessTokenSilently();

        setCookie("ACCESS_TOKEN", accessToken);
      } catch (e) {
        if (e instanceof Error) {
          console.error(e.message);
        } else {
          console.error(e);
        }
      }
    };

    getUserMetadata();
  }, [getAccessTokenSilently]);

  return children;
}
