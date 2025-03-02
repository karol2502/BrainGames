"use client";

import { useEffect, useState } from "react";

export const CountdownTimer = ({ initialTime }: { initialTime: number }) => {
  const [timeLeft, setTimeLeft] = useState(initialTime);

  useEffect(() => {
    if (timeLeft > 0) {
      const timerId = setInterval(() => {
        setTimeLeft((prevTime) => prevTime - 1);
      }, 1000);

      return () => clearInterval(timerId);
    }
  }, [timeLeft]);

  return <>{timeLeft}</>;
};
