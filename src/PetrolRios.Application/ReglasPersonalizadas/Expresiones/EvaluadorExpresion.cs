namespace PetrolRios.Application.ReglasPersonalizadas.Expresiones;

/// <summary>
/// Punto de entrada del motor de expresiones avanzadas. Compila una expresión a un
/// árbol (validación) y la evalúa sobre un registro. Es la pieza que da a las
/// reglas personalizadas "lógica de programación" (operadores, aritmética entre
/// campos, AND/OR/NOT, paréntesis y funciones) de forma segura y sin ejecutar código.
/// </summary>
public sealed class EvaluadorExpresion
{
    private readonly Nodo _arbol;

    private EvaluadorExpresion(Nodo arbol) => _arbol = arbol;

    /// <summary>Compila la expresión; lanza <see cref="ExpresionException"/> si es inválida.</summary>
    public static EvaluadorExpresion Compilar(string expresion) =>
        new(Parser.Parsear(expresion));

    /// <summary>true/false según la expresión evaluada sobre el registro dado.</summary>
    public bool Evaluar(IContextoEvaluacion contexto)
    {
        var resultado = _arbol.Evaluar(contexto);
        if (!resultado.EsBool)
            throw new ExpresionException("La expresión debe dar un resultado verdadero/falso (use una comparación).");
        return resultado.ComoBool();
    }

    /// <summary>
    /// Valida una expresión sin evaluarla, comprobando además que todos los campos
    /// referenciados existan en la fuente indicada. Devuelve la lista de errores.
    /// </summary>
    public static IReadOnlyList<string> Validar(string expresion, string fuente)
    {
        var errores = new List<string>();
        Nodo arbol;
        try
        {
            arbol = Parser.Parsear(expresion);
        }
        catch (ExpresionException ex)
        {
            errores.Add(ex.Message);
            return errores;
        }

        // En fuentes del catálogo se comprueba que los campos existan. En fuentes configurables
        // (tablas arbitrarias) no hay catálogo estático: se valida solo la sintaxis y los campos
        // se resuelven en tiempo de ejecución.
        if (CatalogoReglasPersonalizadas.Fuentes.ContainsKey(fuente))
        {
            foreach (var campo in CamposReferenciados(arbol))
            {
                if (CatalogoReglasPersonalizadas.BuscarCampo(fuente, campo) is null)
                    errores.Add($"El campo '{campo}' no existe en la fuente '{fuente}'.");
            }
        }
        return errores;
    }

    private static IEnumerable<string> CamposReferenciados(Nodo nodo)
    {
        var campos = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        Recorrer(nodo, campos);
        return campos;
    }

    private static void Recorrer(Nodo nodo, HashSet<string> campos)
    {
        if (nodo is NodoCampo c) campos.Add(c.Nombre);
        foreach (var hijo in nodo.Hijos())
            Recorrer(hijo, campos);
    }
}
