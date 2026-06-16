namespace PetrolRios.Application.ReglasPersonalizadas.Expresiones;

/// <summary>
/// Parser recursivo descendente para expresiones de reglas avanzadas. Precedencia
/// (de menor a mayor): || → &amp;&amp; → comparación → + - → * / → unario → primario.
/// Produce un árbol seguro: NO ejecuta código arbitrario, solo evalúa la expresión
/// sobre los campos del registro.
/// </summary>
internal sealed class Parser
{
    private readonly List<Token> _tokens;
    private int _pos;

    private Parser(List<Token> tokens) => _tokens = tokens;

    public static Nodo Parsear(string expresion)
    {
        if (string.IsNullOrWhiteSpace(expresion))
            throw new ExpresionException("La expresión está vacía.");
        var tokens = new Tokenizer(expresion).Tokenizar();
        var parser = new Parser(tokens);
        var arbol = parser.ParsearO();
        if (parser.Actual.Tipo != TipoToken.Fin)
            throw new ExpresionException($"Sintaxis inesperada cerca de '{parser.Actual.Lexema}'.");
        return arbol;
    }

    private Token Actual => _tokens[_pos];

    private Token Consumir() => _tokens[_pos++];

    private bool Coincide(params TipoToken[] tipos)
    {
        if (Array.IndexOf(tipos, Actual.Tipo) >= 0) { _pos++; return true; }
        return false;
    }

    private Nodo ParsearO()
    {
        var nodo = ParsearY();
        while (Actual.Tipo == TipoToken.O) { Consumir(); nodo = new NodoBinario(TipoToken.O, nodo, ParsearY()); }
        return nodo;
    }

    private Nodo ParsearY()
    {
        var nodo = ParsearComparacion();
        while (Actual.Tipo == TipoToken.Y) { Consumir(); nodo = new NodoBinario(TipoToken.Y, nodo, ParsearComparacion()); }
        return nodo;
    }

    private Nodo ParsearComparacion()
    {
        var nodo = ParsearAditivo();
        while (Actual.Tipo is TipoToken.Igual or TipoToken.Distinto or TipoToken.Mayor
               or TipoToken.MayorIgual or TipoToken.Menor or TipoToken.MenorIgual)
        {
            var op = Consumir().Tipo;
            nodo = new NodoBinario(op, nodo, ParsearAditivo());
        }
        return nodo;
    }

    private Nodo ParsearAditivo()
    {
        var nodo = ParsearMultiplicativo();
        while (Actual.Tipo is TipoToken.Mas or TipoToken.Menos)
        {
            var op = Consumir().Tipo;
            nodo = new NodoBinario(op, nodo, ParsearMultiplicativo());
        }
        return nodo;
    }

    private Nodo ParsearMultiplicativo()
    {
        var nodo = ParsearUnario();
        while (Actual.Tipo is TipoToken.Por or TipoToken.Dividir)
        {
            var op = Consumir().Tipo;
            nodo = new NodoBinario(op, nodo, ParsearUnario());
        }
        return nodo;
    }

    private Nodo ParsearUnario()
    {
        if (Actual.Tipo is TipoToken.No or TipoToken.Menos)
        {
            var op = Consumir().Tipo;
            return new NodoUnario(op, ParsearUnario());
        }
        return ParsearPrimario();
    }

    private Nodo ParsearPrimario()
    {
        var token = Actual;
        switch (token.Tipo)
        {
            case TipoToken.Numero:
                Consumir();
                return new NodoNumero(token.Numero);
            case TipoToken.Texto:
                Consumir();
                return new NodoTexto(token.Lexema);
            case TipoToken.Identificador:
                Consumir();
                if (Actual.Tipo == TipoToken.ParenIzq)
                    return ParsearLlamadaFuncion(token.Lexema);
                return new NodoCampo(token.Lexema);
            case TipoToken.ParenIzq:
                Consumir();
                var expr = ParsearO();
                if (!Coincide(TipoToken.ParenDer))
                    throw new ExpresionException("Falta un paréntesis de cierre ')'.");
                return expr;
            default:
                throw new ExpresionException($"Se esperaba un valor, campo o '(' cerca de '{token.Lexema}'.");
        }
    }

    private Nodo ParsearLlamadaFuncion(string nombre)
    {
        Consumir(); // '('
        var args = new List<Nodo>();
        if (Actual.Tipo != TipoToken.ParenDer)
        {
            do { args.Add(ParsearO()); } while (Coincide(TipoToken.Coma));
        }
        if (!Coincide(TipoToken.ParenDer))
            throw new ExpresionException($"Falta ')' al cerrar la función '{nombre}'.");
        return new NodoFuncion(nombre, args);
    }
}
