export default function AuthLayout({ children }: { children: React.ReactNode }) {
  return (
    <div className="auth">
      <aside className="auth-marca">
        <p className="logo">CompanyManagement</p>
        <div>
          <p className="auth-lema">Clientes, órdenes e ingresos en un solo lugar.</p>
          <p className="auth-detalle">
            Registra a tus clientes, lleva el control de cada orden y consulta los reportes de la empresa.
          </p>
        </div>
      </aside>
      <main className="auth-contenido">{children}</main>
    </div>
  );
}
