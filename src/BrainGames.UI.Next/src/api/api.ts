import axios from "axios";
import { AuthInterceptor } from "@/services/auth-interceptor";

export const api = axios.create({
  baseURL: process.env.NEXT_PUBLIC_API_BASE_URL,
});

api.interceptors.request.use(async (request: unknown) =>
  AuthInterceptor.intercept(request),
);
