import React from "react";
import type { Metadata } from "next";
import { Geist } from "next/font/google";
import { TanstackQueryProvider } from "@/contexts/tanstack/tanstack-query-provider";
import "./globals.css";
import { AuthProvider } from "@/contexts/auth-provider";

const geistSans = Geist({
  variable: "--font-geist-sans",
  subsets: ["latin"],
});

export const metadata: Metadata = {
  title: "BrainGames",
  description: "BrainGames",
};

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <html lang="en">
      <body
        className={`${geistSans.variable} antialiased h-screen w-full flex justify-center items-center`}
      >
        <TanstackQueryProvider>
          <AuthProvider>{children}</AuthProvider>
        </TanstackQueryProvider>
      </body>
    </html>
  );
}
