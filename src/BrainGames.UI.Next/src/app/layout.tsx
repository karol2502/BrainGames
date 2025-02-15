import React from "react";
import type { Metadata } from "next";
import { DM_Sans } from "next/font/google";
import { TanstackQueryProvider } from "@/contexts/tanstack/tanstack-query-provider";
import "./globals.css";
import { AuthProvider } from "@/contexts/auth-provider";

const dmSans = DM_Sans({
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
        className={`${dmSans.variable} antialiased h-screen w-full flex justify-center`}
      >
        <TanstackQueryProvider>
          <AuthProvider>{children}</AuthProvider>
        </TanstackQueryProvider>
      </body>
    </html>
  );
}
