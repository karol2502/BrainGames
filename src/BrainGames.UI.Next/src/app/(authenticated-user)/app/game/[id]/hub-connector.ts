"use client";

import { HubConnectionBuilder, LogLevel } from "@microsoft/signalr";
import { AuthInterceptor } from "@/services/auth-interceptor";

const gameHubUrl = (lobbyId: string) =>
  `${process.env.NEXT_PUBLIC_API_BASE_URL}/hubs/game?lobbyId=${lobbyId}`;

export const connection = new HubConnectionBuilder()
  .withUrl(process.env.NEXT_PUBLIC_API_BASE_URL + "/hubs/game", {
    accessTokenFactory: async () => {
      if (!AuthInterceptor.getToken) {
        return "";
      }
      return await AuthInterceptor.getToken();
    },
  })
  .configureLogging(LogLevel.Debug)
  .build();

export const startConnection = async (lobbyId: string) => {
  try {
    connection.baseUrl = gameHubUrl(lobbyId);
    await connection.start();
    console.log("connected");
  } catch (err) {
    console.log(err);
    // setTimeout(startConnection, 5000);
  }
};
