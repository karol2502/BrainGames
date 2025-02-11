import React from "react";

type Props = {
  children: React.ReactNode;
};

export default function LoginLayout({ children }: Props) {
  return (
    <div className="w-full h-full flex justify-center items-center">
      {children}
    </div>
  );
}
