import { GameContent } from "@/app/(authenticated-user)/app/game/[id]/game-content";

export default async function GamePage({
  params,
}: {
  params: Promise<{ id: string }>;
}) {
  const { id } = await params;

  return <GameContent id={id} />;
}
