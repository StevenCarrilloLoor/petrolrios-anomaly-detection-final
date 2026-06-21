using System.Globalization;

namespace PetrolRios.Application.ReglasPersonalizadas.Expresiones;

/// <summary>Valor tipado que produce la evaluación de un nodo: número, texto o booleano.</summary>
public readonly struct Valor
{
    public readonly double? Numero;
    public readonly string? Texto;
    public readonly bool? Booleano;

    private Valor(double? n, string? t, bool? b) { Numero = n; Texto = t; Booleano = b; }

    public static Valor DeNumero(double n) => new(n, null, null);
    public static Valor DeTexto(string t) => new(null, t, null);
    public static Valor DeBool(bool b) => new(null, null, b);

    public bool EsNumero => Numero.HasValue;
    public bool EsTexto => Texto is not null;
    public bool EsBool => Booleano.HasValue;

    public double ComoNumero() => Numero ?? throw new ExpresionException("Se esperaba un número.");
    public bool ComoBool() => Booleano ?? throw new ExpresionException("Se esperaba una condición verdadero/falso.");
    public string ComoTexto() => Texto ?? Numero?.ToString(CultureInfo.InvariantCulture) ?? "";
}

/// <summary>Contexto de evaluación: resuelve el valor de un campo del registro actual.</summary>
public interface IContextoEvaluacion
{
    Valor ObtenerCampo(string nombre);
}

internal abstract class Nodo
{
    public abstract Valor Evaluar(IContextoEvaluacion ctx);

    /// <summary>Sub-nodos directos (para recorrer el árbol, p. ej. al listar campos usados).</summary>
    public virtual IEnumerable<Nodo> Hijos() => [];
}

internal sealed class NodoNumero(double valor) : Nodo
{
    public override Valor Evaluar(IContextoEvaluacion ctx) => Valor.DeNumero(valor);
}

internal sealed class NodoTexto(string valor) : Nodo
{
    public override Valor Evaluar(IContextoEvaluacion ctx) => Valor.DeTexto(valor);
}

internal sealed class NodoCampo(string nombre) : Nodo
{
    public string Nombre => nombre;
    public override Valor Evaluar(IContextoEvaluacion ctx) => ctx.ObtenerCampo(nombre);
}

internal sealed class NodoUnario(TipoToken op, Nodo operando) : Nodo
{
    private readonly Nodo _operando = operando;
    public override IEnumerable<Nodo> Hijos() => [_operando];

    public override Valor Evaluar(IContextoEvaluacion ctx)
    {
        var v = _operando.Evaluar(ctx);
        return op switch
        {
            TipoToken.No => Valor.DeBool(!v.ComoBool()),
            TipoToken.Menos => Valor.DeNumero(-v.ComoNumero()),
            _ => throw new ExpresionException($"Operador unario no soportado: {op}")
        };
    }
}

internal sealed class NodoBinario(TipoToken op, Nodo izq, Nodo der) : Nodo
{
    private readonly Nodo _izq = izq;
    private readonly Nodo _der = der;
    public override IEnumerable<Nodo> Hijos() => [_izq, _der];

    public override Valor Evaluar(IContextoEvaluacion ctx)
    {
        // Cortocircuito lógico
        if (op == TipoToken.Y) return Valor.DeBool(_izq.Evaluar(ctx).ComoBool() && _der.Evaluar(ctx).ComoBool());
        if (op == TipoToken.O) return Valor.DeBool(_izq.Evaluar(ctx).ComoBool() || _der.Evaluar(ctx).ComoBool());

        var a = _izq.Evaluar(ctx);
        var b = _der.Evaluar(ctx);

        switch (op)
        {
            case TipoToken.Mas:
                // Si alguno es texto, concatena; si no, suma
                if (a.EsTexto || b.EsTexto) return Valor.DeTexto(a.ComoTexto() + b.ComoTexto());
                return Valor.DeNumero(a.ComoNumero() + b.ComoNumero());
            case TipoToken.Menos: return Valor.DeNumero(a.ComoNumero() - b.ComoNumero());
            case TipoToken.Por: return Valor.DeNumero(a.ComoNumero() * b.ComoNumero());
            case TipoToken.Dividir:
                var divisor = b.ComoNumero();
                return Valor.DeNumero(divisor == 0 ? 0 : a.ComoNumero() / divisor);
            case TipoToken.Igual: return Valor.DeBool(SonIguales(a, b));
            case TipoToken.Distinto: return Valor.DeBool(!SonIguales(a, b));
            case TipoToken.Mayor: return Valor.DeBool(a.ComoNumero() > b.ComoNumero());
            case TipoToken.MayorIgual: return Valor.DeBool(a.ComoNumero() >= b.ComoNumero());
            case TipoToken.Menor: return Valor.DeBool(a.ComoNumero() < b.ComoNumero());
            case TipoToken.MenorIgual: return Valor.DeBool(a.ComoNumero() <= b.ComoNumero());
            default: throw new ExpresionException($"Operador no soportado: {op}");
        }
    }

    private static bool SonIguales(Valor a, Valor b)
    {
        if (a.EsTexto || b.EsTexto)
            return string.Equals(a.ComoTexto(), b.ComoTexto(), StringComparison.OrdinalIgnoreCase);
        if (a.EsBool && b.EsBool) return a.ComoBool() == b.ComoBool();
        return Math.Abs(a.ComoNumero() - b.ComoNumero()) < 0.0001;
    }
}

internal sealed class NodoFuncion(string nombre, List<Nodo> args) : Nodo
{
    private readonly List<Nodo> _args = args;
    public override IEnumerable<Nodo> Hijos() => _args;

    public override Valor Evaluar(IContextoEvaluacion ctx)
    {
        Valor Arg(int i) => _args[i].Evaluar(ctx);
        var n = _args.Count;
        return nombre.ToLowerInvariant() switch
        {
            // ── Texto ──
            "minusculas" => Valor.DeTexto(Arg(0).ComoTexto().ToLowerInvariant()),
            "mayusculas" => Valor.DeTexto(Arg(0).ComoTexto().ToUpperInvariant()),
            "longitud" => Valor.DeNumero(Arg(0).ComoTexto().Trim().Length),
            "vacio" => Valor.DeBool(string.IsNullOrWhiteSpace(Arg(0).ComoTexto())),
            "contiene" => Valor.DeBool(Arg(0).ComoTexto().Contains(Arg(1).ComoTexto(), StringComparison.OrdinalIgnoreCase)),
            "empieza" => Valor.DeBool(Arg(0).ComoTexto().StartsWith(Arg(1).ComoTexto(), StringComparison.OrdinalIgnoreCase)),
            "termina" => Valor.DeBool(Arg(0).ComoTexto().EndsWith(Arg(1).ComoTexto(), StringComparison.OrdinalIgnoreCase)),
            // siVacio(a, b): devuelve b si a está vacío; si no, a (coalescencia de nulos/vacíos).
            "sivacio" => string.IsNullOrWhiteSpace(Arg(0).ComoTexto())
                ? (n >= 2 ? Arg(1) : Valor.DeTexto(""))
                : Arg(0),
            // ── Matemáticas ──
            "abs" => Valor.DeNumero(Math.Abs(Arg(0).ComoNumero())),
            // redondear(x) o redondear(x, decimales)
            "redondear" => Valor.DeNumero(n >= 2
                ? Math.Round(Arg(0).ComoNumero(), (int)Arg(1).ComoNumero())
                : Math.Round(Arg(0).ComoNumero())),
            "piso" => Valor.DeNumero(Math.Floor(Arg(0).ComoNumero())),
            "techo" => Valor.DeNumero(Math.Ceiling(Arg(0).ComoNumero())),
            "min" => Valor.DeNumero(Math.Min(Arg(0).ComoNumero(), Arg(1).ComoNumero())),
            "max" => Valor.DeNumero(Math.Max(Arg(0).ComoNumero(), Arg(1).ComoNumero())),
            "modulo" => Valor.DeNumero(Modulo(Arg(0).ComoNumero(), Arg(1).ComoNumero())),
            "raiz" => Valor.DeNumero(Math.Sqrt(Math.Max(0, Arg(0).ComoNumero()))),
            "potencia" => Valor.DeNumero(Math.Pow(Arg(0).ComoNumero(), Arg(1).ComoNumero())),
            // ── Listas ──
            // en(valor, op1, op2, …): true si el valor coincide con alguna opción (números o texto).
            "en" => Valor.DeBool(EvaluarEn(ctx)),
            _ => throw new ExpresionException($"Función desconocida: '{nombre}'")
        };
    }

    private bool EvaluarEn(IContextoEvaluacion ctx)
    {
        if (_args.Count < 2)
            throw new ExpresionException("en(valor, opcion1, opcion2, …) requiere un valor y al menos una opción.");
        var valor = _args[0].Evaluar(ctx);
        for (var i = 1; i < _args.Count; i++)
            if (Iguales(valor, _args[i].Evaluar(ctx))) return true;
        return false;
    }

    private static bool Iguales(Valor a, Valor b) =>
        a.EsTexto || b.EsTexto
            ? string.Equals(a.ComoTexto(), b.ComoTexto(), StringComparison.OrdinalIgnoreCase)
            : Math.Abs(a.ComoNumero() - b.ComoNumero()) < 0.0001;

    private static double Modulo(double a, double b) => b == 0 ? 0 : a % b;
}
