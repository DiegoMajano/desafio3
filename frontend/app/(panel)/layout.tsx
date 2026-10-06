"use client";

import { useEffect, useState } from "react";
import Link from "next/link";
import { usePathname, useRouter } from "next/navigation";
import { useSesion } from "@/components/SesionProvider";

const enlaces = [
  { href: "/inicio", texto: "Resumen", soloAdmin: false },
  { href: "/clientes", texto: "Clientes", soloAdmin: false },
  { href: "/ordenes", texto: "Órdenes", soloAdmin: false },
  { href: "/reportes", texto: "Reportes", soloAdmin: true },
];

export default function PanelLayout({ children }: { children: React.ReactNode }) {
  const { sesion, cargando, esAdmin, cerrarSesion } = useSesion();
  const router = useRouter();
  const ruta = usePathname();
  const [menuAbierto, setMenuAbierto] = useState(false);

  useEffect(() => {
    if (!cargando && !sesion) router.replace("/login");
  }, [cargando, sesion, router]);

  useEffect(() => setMenuAbierto(false), [ruta]);

  if (cargando || !sesion) return null;

  return (
    <div className="panel">
      <aside className={`barra ${menuAbierto ? "barra-abierta" : ""}`}>
        <div className="barra-superior">
          <Link href="/inicio" className="logo">CompanyManagement</Link>
          <button className="boton-menu" onClick={() => setMenuAbierto((v) => !v)} aria-expanded={menuAbierto} aria-label="Menú">
            <svg width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" aria-hidden="true">
              <path d="M4 7h16M4 12h16M4 17h16" />
            </svg>
          </button>
        </div>
        <nav className="navegacion">
          {enlaces.filter((e) => !e.soloAdmin || esAdmin).map((e) => (
            <Link key={e.href} href={e.href} className={ruta.startsWith(e.href) ? "activo" : ""}
                  aria-current={ruta.startsWith(e.href) ? "page" : undefined}>
              {e.texto}
            </Link>
          ))}
        </nav>
        <div className="usuario">
          <p className="usuario-email">{sesion.email}</p>
          <p className="usuario-rol">{esAdmin ? "Administrador" : "Usuario"}</p>
          <button className="boton-salir" onClick={cerrarSesion}>Cerrar sesión</button>
        </div>
      </aside>
      <main className="contenido">{children}</main>
    </div>
  );
}
