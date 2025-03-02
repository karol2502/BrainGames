import { Player } from "@/models/player";

export type Lobby = {
  id: string;
  players: Player[];
  status: LobbyStatus;
  host: Player;
};

export enum LobbyStatus {
  WaitingForStart,
  LoadingScreen,
  InGame,
  Scoreboard,
}
