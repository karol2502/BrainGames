"use client";

import React, { createContext, useContext, useState } from "react";
import { Lobby } from "@/models/lobby";

type GameLobbyContextType = {};

const GameLobbyContext = createContext<GameLobbyContextType | undefined>(
  undefined,
);

export const useGameLobby = () => useContext(GameLobbyContext);

export const GameLobbyProvider = ({
  children,
}: {
  children: React.ReactNode;
}) => {
  const [lobby, setLobby] = useState<Lobby | null>(null);
  const [commandPayload, setCommandPayload] = useState<object | null>(null);

  return (
    <GameLobbyContext.Provider value={{}}>{children}</GameLobbyContext.Provider>
  );
};
