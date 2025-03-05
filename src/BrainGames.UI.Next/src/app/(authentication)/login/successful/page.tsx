"use client";

import { useAuth0 } from "@auth0/auth0-react";
import { useQuery } from "@tanstack/react-query";
import { AuthService } from "@/services/auth-service";
import { useRouter } from "next/navigation";
import { useEffect } from "react";
import { Spinner } from "@/components/ui/spinner";

export default function LoginSuccessfulPage() {
  const router = useRouter();
  const { getIdTokenClaims, isLoading, isAuthenticated } = useAuth0();

  const { data: idToken } = useQuery({
    queryKey: ["idToken", "google"],
    queryFn: getIdTokenClaims,
    gcTime: 0,
    enabled: !isLoading && isAuthenticated,
  });

  const { isSuccess, isFetched } = useQuery({
    queryKey: ["api", "google"],
    queryFn: () => AuthService.sendOAuthIdToken(idToken),
    gcTime: 0,
    enabled: !!idToken,
  });

  useEffect(() => {
    if (isFetched && isSuccess) {
      router.push("/app");
    }

    if (!isLoading && !isAuthenticated) {
      router.push("/");
    }
  }, [isAuthenticated, isFetched, isLoading, isSuccess, router]);

  return (
    <div>
      <Spinner />
    </div>
  );
}
