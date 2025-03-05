"use client";

import React, { useEffect } from "react";
import { useAuth0 } from "@auth0/auth0-react";
import { Spinner } from "@/components/ui/spinner";
import { useRouter } from "next/navigation";

export default function AppLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  const router = useRouter();
  const { isAuthenticated, isLoading } = useAuth0();

  useEffect(() => {
    if (!isLoading && !isAuthenticated) {
      router.push("/");
    }
  }, [isAuthenticated, isLoading, router]);

  if (isLoading) {
    return <Spinner />;
  }

  if (!isLoading && isAuthenticated) {
    return children;
  }
}
