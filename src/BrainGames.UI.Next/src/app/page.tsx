"use client";

import { useAuth0 } from "@auth0/auth0-react";
import { useRouter } from "next/navigation";
import { useEffect } from "react";
import { Spinner } from "@/components/ui/spinner";
import { Button } from "@/components/ui/button";
import Image from "next/image";
import * as icon from "./favicon.ico";

export default function Home() {
  const router = useRouter();
  const { isAuthenticated, isLoading, loginWithRedirect } = useAuth0();

  useEffect(() => {
    if (isAuthenticated) {
      router.push("/app");
    }
  }, [isAuthenticated, router]);

  if (isLoading || isAuthenticated) {
    return <Spinner />;
  } else {
    return (
      <div className="flex flex-col items-center justify-center gap-6">
        <span className="font-semibold text-xl">Brain games</span>
        <Image
          priority={true}
          src={icon}
          alt="brain-icon"
          width={64}
          height={64}
        />
        <Button variant="outline" onClick={() => loginWithRedirect()}>
          Join now!
        </Button>
      </div>
    );
  }
}
