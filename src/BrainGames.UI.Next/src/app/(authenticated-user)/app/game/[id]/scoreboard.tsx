"use client";

import { Lobby } from "@/models/lobby";
import { useMemo } from "react";

type Props = {
  payload: object;
  lobby: Lobby;
};

type ScoreboardPayload = {
  [playerId: string]: { score: number };
};

export const Scoreboard = ({ payload, lobby }: Props) => {
  const typedPayload = payload as ScoreboardPayload;

  const data = useMemo(() => {
    return Object.entries(typedPayload).map(([playerId, { score }]) => ({
      playerId,
      score,
      user: lobby.players.find(
        (player) => player.user.nameIdentifier === playerId,
      )?.user,
    }));
  }, [lobby.players, typedPayload]);

  return (
    <div className="flex flex-col gap-6 p-6">
      {data.map((player) => (
        <div key={player.playerId} className="flex justify-between">
          <span>{player.user?.nickname}</span>
          <span>{player.score}</span>
        </div>
      ))}
    </div>
  );
};
