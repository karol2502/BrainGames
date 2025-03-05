"use client";

import { CountdownTimer } from "@/components/ui/countdown-timer";

type LoadingScreenProps = {
  initialTime: number;
  title: string;
  description: string;
};

export const LoadingScreen = ({
  initialTime,
  title,
  description,
}: LoadingScreenProps) => {
  return (
    <div className="flex flex-col p-8 gap-8 items-center">
      <span className="text-2xl">{title}</span>
      <span className="text-lg">{description}</span>
      <span className="text-6xl font-semibold">
        <CountdownTimer initialTime={initialTime} />
      </span>
    </div>
  );
};
