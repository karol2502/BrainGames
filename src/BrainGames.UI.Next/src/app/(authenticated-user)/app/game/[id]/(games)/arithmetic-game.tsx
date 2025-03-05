"use client";

import { Input } from "@/components/ui/input";
import React, { useEffect, useState } from "react";
import { connection } from "@/app/(authenticated-user)/app/game/[id]/hub-connector";
import { CountdownTimer } from "@/components/ui/countdown-timer";

type Props = {
  payload: object;
};

type ArithmeticGamePayload = {
  roundNumber: number;
  question: string;
  gameDuration: number;
};

export const ArithmeticGame = ({ payload }: Props) => {
  const [value, setValue] = useState("");
  const typedPayload = payload as ArithmeticGamePayload;

  useEffect(() => {
    setValue("");
  }, [payload]);

  const onChangeValue = (e: React.ChangeEvent<HTMLInputElement>) => {
    const newValue = e.target.value;
    if (newValue.length > value.length) {
      connection.send("HandleGameCommand", "GameAction", { answer: newValue });
    }
    setValue(newValue);
  };

  return (
    <div className="flex flex-col p-8 gap-8">
      <div>
        <span>Time left: </span>
        <span>
          <CountdownTimer initialTime={typedPayload.gameDuration} />
        </span>
      </div>
      <span>{`Round: ${typedPayload.roundNumber}`}</span>
      <div className="flex flex-col gap-4">
        <span>{typedPayload.question}</span>
        <Input
          type="number"
          value={value}
          autoFocus
          onBlur={(e) => e.target.focus()}
          onChange={onChangeValue}
        />
      </div>
    </div>
  );
};
