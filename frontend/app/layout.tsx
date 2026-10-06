import type { Metadata } from "next";
import "@fontsource-variable/bricolage-grotesque";
import "@fontsource-variable/instrument-sans";
import "./globals.css";
import { SesionProvider } from "@/components/SesionProvider";
import { AvisoProvider } from "@/components/Aviso";

export const metadata: Metadata = {
  title: "CompanyManagement",
  description: "Gestión de clientes, órdenes y reportes",
};

export default function RootLayout({ children }: { children: React.ReactNode }) {
  return (
    <html lang="es" suppressHydrationWarning>
      <body suppressHydrationWarning>
        <SesionProvider>
          <AvisoProvider>{children}</AvisoProvider>
        </SesionProvider>
      </body>
    </html>
  );
}
