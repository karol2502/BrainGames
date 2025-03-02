"use client";

import { useEffect, useMemo, useState } from "react";
import {
  connection,
  startConnection,
} from "@/app/(authenticated-user)/app/game/[id]/hub-connector";
import { useRouter } from "next/navigation";
import { Lobby, LobbyStatus } from "@/models/lobby";
import { Button } from "@/components/ui/button";
import { LoadingScreen } from "@/app/(authenticated-user)/app/game/[id]/(games)/loading-screen";
import { GameFactory } from "@/app/(authenticated-user)/app/game/[id]/(games)/game-factory";
import { useAuth0 } from "@auth0/auth0-react";
import { useQuery } from "@tanstack/react-query";
import { LobbyService } from "@/services/lobby-service";
import { Spinner } from "@/components/ui/spinner";
import { Scoreboard } from "./scoreboard";

type Props = {
  id: string;
};

export const GameContent = ({ id }: Props) => {
  const router = useRouter();
  const { user } = useAuth0();

  const [lobby, setLobby] = useState<Lobby | null>(null);
  const [commandPayload, setCommandPayload] = useState<object | null>(null);
  const [gameName, setGameName] = useState<string | null>(null);
  const [selectedGame, setSelectedGame] = useState<string>("Random");
  const [joinedMidGame, setJoinedMidGame] = useState<boolean>(false);

  useEffect(() => {
    startConnection(id);
  }, [id]);

  connection.onclose(() => {
    router.push("/app");
  });

  const isAdmin = useMemo(
    () => user?.sub === lobby?.host.user.nameIdentifier,
    [lobby?.host.user.nameIdentifier, user?.sub],
  );

  const { data: games } = useQuery({
    queryKey: ["games"],
    queryFn: () => LobbyService.getActiveGames(),
    enabled: isAdmin,
  });

  connection.on("HandleGameCommand", (command: string, payload: object) => {
    setJoinedMidGame(false);
    switch (command) {
      case "LobbyUpdated":
        setLobby(payload as Lobby);
        break;
      case "GameStarting":
        setGameName((payload as { gameName: string }).gameName);
        setLobby((prevState) => {
          if (prevState === null) return null;
          return { ...prevState, status: LobbyStatus.LoadingScreen };
        });
        break;
      case "GameStarted":
        setLobby((prevState) => {
          if (prevState === null) return null;
          return { ...prevState, status: LobbyStatus.InGame };
        });
        break;
      case "GameUpdated":
        break;
      case "WaitForGameToEnd":
        setJoinedMidGame(true);
        break;
      case "GameEnded":
        setLobby((prevState) => {
          if (prevState === null) return null;
          return { ...prevState, status: LobbyStatus.Scoreboard };
        });
        break;
    }
    setCommandPayload(payload);
  });

  const adminSettings = () => {
    return (
      <div className="flex flex-col gap-4 mt-8">
        <span className="font-semibold">Admin settings</span>
        <div className="flex flex-col gap-2">
          <span className="text-sm">Select game</span>
          {games &&
            games.length > 0 &&
            ["Random", ...games].map((game) => (
              <Button
                variant={game === selectedGame ? "secondary" : "outline"}
                key={game}
                onClick={() => setSelectedGame(game)}
              >
                <span>{game}</span>
              </Button>
            ))}
        </div>
        <Button
          onClick={() =>
            connection.send("HandleGameCommand", "StartGame", {
              gameName: selectedGame,
            })
          }
        >
          Start game
        </Button>
      </div>
    );
  };

  if (joinedMidGame) {
    return (
      <div className="flex flex-col p-8 gap-8">
        <span>Waiting for the game to end</span>
        <Spinner />
      </div>
    );
  }

  return (
    <div className="flex flex-col p-8">
      {lobby?.status === LobbyStatus.WaitingForStart && (
        <div className="flex flex-col">
          <span>Lobby</span>
          {lobby !== null &&
            lobby.players.map((player) => (
              <div key={player.connectionId}>
                <span>{`- ${player.user.nickname}`}</span>
              </div>
            ))}
          {isAdmin && adminSettings()}
          {!isAdmin && <span>Waiting for host to start the game</span>}
        </div>
      )}
      {lobby?.status === LobbyStatus.LoadingScreen && (
        <LoadingScreen
          title={(commandPayload as { gameName: string }).gameName}
          initialTime={
            (commandPayload as { loadingScreenDuration: number })
              .loadingScreenDuration
          }
        />
      )}
      {lobby?.status === LobbyStatus.InGame && (
        <GameFactory gameName={gameName!} payload={commandPayload!} />
      )}
      {lobby?.status === LobbyStatus.Scoreboard && (
        <Scoreboard lobby={lobby} payload={commandPayload!} />
      )}
    </div>
  );
};
