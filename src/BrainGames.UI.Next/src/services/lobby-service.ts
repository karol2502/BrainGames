import { api } from "@/api/api";
import { AxiosResponse } from "axios";

const controllerPath = "/api/lobby";

export const LobbyService = {
  createLobby: async (): Promise<AxiosResponse<string, any>> =>
    await api.post<string>(controllerPath + "/create-lobby"),
  getActiveGames: async (): Promise<string[]> =>
    await api.get<string[]>(controllerPath + "/games").then((res) => res.data),
};
