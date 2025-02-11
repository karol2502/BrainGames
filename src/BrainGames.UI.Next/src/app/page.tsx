"use client";

import { Button } from "@/components/ui/button";
import { useAuth0 } from "@auth0/auth0-react";
import Image from "next/image";

export default function Home() {
  const { user, isAuthenticated, isLoading, logout } = useAuth0();

  if (isLoading) {
    return <div>Loading ...</div>;
  }

  return (
    <div>
      {isAuthenticated && user && (
        <div>
          <Image
            src={user.picture ?? ""}
            alt={user.name ?? "profile"}
            width={200}
            height={200}
          />
          <h2>{user.name}</h2>
          <p>{user.email}</p>
        </div>
      )}
      <span>Home</span>
      <Button onClick={() => logout()}>logout</Button>
    </div>
  );
}
