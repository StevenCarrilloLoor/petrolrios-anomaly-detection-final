export function LoginPage() {
  return (
    <div className="flex min-h-screen items-center justify-center bg-muted">
      <div className="w-full max-w-md rounded-lg border border-border bg-background p-8 shadow-lg">
        <h1 className="mb-6 text-center text-2xl font-bold text-foreground">
          PetrolRios
        </h1>
        <p className="mb-8 text-center text-sm text-muted-foreground">
          Sistema de Deteccion de Anomalias
        </p>
        <form className="space-y-4">
          <div>
            <label className="mb-1 block text-sm font-medium" htmlFor="email">
              Correo
            </label>
            <input
              id="email"
              type="email"
              className="w-full rounded-md border border-border bg-background px-3 py-2 text-sm"
              placeholder="usuario@petrolrios.com"
            />
          </div>
          <div>
            <label className="mb-1 block text-sm font-medium" htmlFor="password">
              Contrasena
            </label>
            <input
              id="password"
              type="password"
              className="w-full rounded-md border border-border bg-background px-3 py-2 text-sm"
            />
          </div>
          <button
            type="submit"
            className="w-full rounded-md bg-primary px-4 py-2 text-sm font-medium text-primary-foreground hover:bg-primary/90"
          >
            Iniciar Sesion
          </button>
        </form>
      </div>
    </div>
  );
}
