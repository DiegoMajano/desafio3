"use client";

import { useState } from "react";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { useSesion } from "@/components/SesionProvider";

export default function RegistroPage() {
  const { registrarse } = useSesion();
  const router = useRouter();
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [confirmacion, setConfirmacion] = useState("");
  const [error, setError] = useState("");
  const [enviando, setEnviando] = useState(false);

  async function enviar(e: React.FormEvent) {
    e.preventDefault();
    setError("");
    if (password !== confirmacion) {
      setError("Las contraseñas no coinciden.");
      return;
    }
    setEnviando(true);
    try {
      await registrarse(email, password);
      router.replace("/inicio");
    } catch (err) {
      setError(err instanceof Error ? err.message : "No se pudo crear la cuenta.");
    } finally {
      setEnviando(false);
    }
  }

  return (
    <form onSubmit={enviar} className="formulario auth-form">
      <h1>Crear cuenta</h1>
      <label className="campo">
        <span>Email</span>
        <input type="email" autoComplete="email" value={email} onChange={(e) => setEmail(e.target.value)} required autoFocus />
      </label>
      <label className="campo">
        <span>Contraseña</span>
        <input type="password" autoComplete="new-password" value={password} onChange={(e) => setPassword(e.target.value)} required minLength={6} />
        <small>Mínimo 6 caracteres, con mayúscula, minúscula y un número.</small>
      </label>
      <label className="campo">
        <span>Repite la contraseña</span>
        <input type="password" autoComplete="new-password" value={confirmacion} onChange={(e) => setConfirmacion(e.target.value)} required />
      </label>
      {error && <p className="error-form" role="alert">{error}</p>}
      <button type="submit" className="boton boton-primario boton-ancho" disabled={enviando}>
        {enviando ? "Creando cuenta…" : "Crear cuenta"}
      </button>
      <p className="auth-cambio">¿Ya tienes cuenta? <Link href="/login">Iniciar sesión</Link></p>
    </form>
  );
}
