import axios from "axios";
import { isServer } from "@tanstack/react-query";
import { getCookie } from "cookies-next";

export const api = axios.create({
  baseURL: process.env.NEXT_PUBLIC_API_BASE_URL,
});

api.interceptors.request.use(async (request: any) => {
  let accessToken;

  if (isServer) {
    const { cookies } = await import("next/headers");
    accessToken = await getCookie("ACCESS_TOKEN", { cookies });
  } else {
    accessToken = await getCookie("ACCESS_TOKEN");
  }

  if (accessToken) {
    request.headers.Authorization = `Bearer ${accessToken}`;
  }
  return request;
});
