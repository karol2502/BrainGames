import { ArithmeticGame } from "./arithmetic-game";

type Props = {
  gameName: string;
  payload: object;
};

export const GameFactory = ({ gameName, payload }: Props) => {
  switch (gameName) {
    case "ArithmeticGame":
      return <ArithmeticGame payload={payload} />;
    default:
      return null;
  }
};
