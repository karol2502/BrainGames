"use client";

import { Button } from "@/components/ui/button";
import { useAuth0 } from "@auth0/auth0-react";

export default function Login() {
  const { loginWithRedirect } = useAuth0();

  return (
    <div>
      <Button onClick={() => loginWithRedirect()}>Login</Button>
    </div>
  );
}
