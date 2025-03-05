"use client";

import { Lobby } from "@/models/lobby";
import { useMemo } from "react";
import { connection } from "@/app/(authenticated-user)/app/game/[id]/hub-connector";
import { Button } from "@/components/ui/button";

type Props = {
  payload: object;
  lobby: Lobby;
  isAdmin: boolean;
};

type ScoreboardPayload = {
  [playerId: string]: { score: number };
};

export const Scoreboard = ({ payload, lobby, isAdmin }: Props) => {
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
      <span>Scoreboard</span>
      {data.map((player) => (
        <div key={player.playerId} className="flex justify-between gap-2">
          <span>{`${player.user?.nickname}:`}</span>
          <span className="font-semibold">{player.score}</span>
        </div>
      ))}
      {isAdmin && (
        <Button
          onClick={() => {
            connection.send("HandleGameCommand", "GoBackToLobby", {});
          }}
        >
          Go back to lobby
        </Button>
      )}
    </div>
  );
};
