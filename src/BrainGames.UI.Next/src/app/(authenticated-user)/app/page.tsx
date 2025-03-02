"use client";

import { useAuth0 } from "@auth0/auth0-react";
import { Button } from "@/components/ui/button";
import Image from "next/image";
import { useMutation } from "@tanstack/react-query";
import { LobbyService } from "@/services/lobby-service";
import { useRouter } from "next/navigation";

export default function AppPage() {
  const router = useRouter();
  const { user, isAuthenticated, logout } = useAuth0();

  const createLobby = useMutation({
    mutationFn: () => LobbyService.createLobby(),
    onSuccess: (response) => {
      router.push(`/app/game/${response.data}`);
    },
  });

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

      <Button onClick={() => createLobby.mutate()}>Create lobby</Button>
    </div>
  );
}
