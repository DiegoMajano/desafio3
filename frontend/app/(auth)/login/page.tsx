"use client";

import { Suspense, useEffect, useState } from "react";
import Link from "next/link";
import { useRouter, useSearchParams } from "next/navigation";
import { useSesion } from "@/components/SesionProvider";
import { ApiError } from "@/lib/api";

function FormularioLogin() {
  const { iniciarSesion, sesion } = useSesion();
  const router = useRouter();
  const params = useSearchParams();
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState("");
  const [enviando, setEnviando] = useState(false);

  useEffect(() => {
    if (sesion) router.replace("/inicio");
  }, [sesion, router]);

  async function enviar(e: React.FormEvent) {
    e.preventDefault();
    setError("");
    setEnviando(true);
    try {
      await iniciarSesion(email, password);
      router.replace("/inicio");
    } catch (err) {
      setError(err instanceof ApiError && err.status === 401
        ? "El email o la contraseña no coinciden."
        : err instanceof Error ? err.message : "No se pudo iniciar sesión.");
    } finally {
      setEnviando(false);
    }
  }

  return (
    <form onSubmit={enviar} className="formulario auth-form">
      <h1>Iniciar sesión</h1>
      {params.get("expirada") && <p className="nota">Tu sesión expiró. Vuelve a entrar para continuar.</p>}
      <label className="campo">
        <span>Email</span>
        <input type="email" autoComplete="email" value={email} onChange={(e) => setEmail(e.target.value)} required autoFocus />
      </label>
      <label className="campo">
        <span>Contraseña</span>
        <input type="password" autoComplete="current-password" value={password} onChange={(e) => setPassword(e.target.value)} required />
      </label>
      {error && <p className="error-form" role="alert">{error}</p>}
      <button type="submit" className="boton boton-primario boton-ancho" disabled={enviando}>
        {enviando ? "Entrando…" : "Entrar"}
      </button>
      <p className="auth-cambio">¿No tienes cuenta? <Link href="/registro">Crear una cuenta</Link></p>
    </form>
  );
}

export default function LoginPage() {
  return (
    <Suspense>
      <FormularioLogin />
    </Suspense>
  );
}
