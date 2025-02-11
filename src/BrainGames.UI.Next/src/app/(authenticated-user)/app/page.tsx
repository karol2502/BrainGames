"use client";

import { useEffect } from "react";
import { api } from "@/api/api";

export default function AppPage() {
  useEffect(() => {
    api.get("api/test/authorize").then((r) => {
      console.log(r.data);
    });
  }, []);

  return (
    <div>
      <span>App</span>
    </div>
  );
}
