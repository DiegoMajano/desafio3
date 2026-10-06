"use client";

import { useEffect } from "react";
import { useRouter } from "next/navigation";
import { useSesion } from "@/components/SesionProvider";

export default function Raiz() {
  const { sesion, cargando } = useSesion();
  const router = useRouter();

  useEffect(() => {
    if (!cargando) router.replace(sesion ? "/inicio" : "/login");
  }, [sesion, cargando, router]);

  return null;
}
