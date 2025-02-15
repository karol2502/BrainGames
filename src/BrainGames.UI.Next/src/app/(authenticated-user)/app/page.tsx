"use client";

import { useAuth0 } from "@auth0/auth0-react";
import { Button } from "@/components/ui/button";
import Image from "next/image";
import { Spinner } from "@/components/ui/spinner";

export default function AppPage() {
  const { user, isAuthenticated, isLoading, logout } = useAuth0();

  if (isLoading) {
    return <Spinner />;
  }

  return (
    <div className="flex flex-col items-center justify-center gap-6">
      {isAuthenticated && user && (
        <div>
          <Image
            priority={true}
            src={user.picture ?? ""}
            alt={user.name ?? "profile"}
            width={200}
            height={200}
          />
          <h2>{user.name}</h2>
          <p>{user.email}</p>
        </div>
      )}
      <Button onClick={() => logout()}>Logout</Button>
    </div>
  );
}
