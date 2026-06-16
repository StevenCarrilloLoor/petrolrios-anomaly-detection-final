namespace PetrolRios.Application.ReglasPersonalizadas.Expresiones;

internal enum TipoToken
{
    Numero, Texto, Identificador,
    Mas, Menos, Por, Dividir,
    Igual, Distinto, Mayor, MayorIgual, Menor, MenorIgual,
    Y, O, No,
    ParenIzq, ParenDer, Coma,
    Fin
}

internal readonly record struct Token(TipoToken Tipo, string Lexema, double Numero = 0);

/// <summary>
/// Tokeniza una expresión de regla avanzada. Soporta números, cadenas entre
/// comillas, identificadores (campos y funciones), operadores aritméticos y de
/// comparación, y los operadores lógicos &amp;&amp; / || / ! (y sus alias and/or/not).
/// </summary>
internal sealed class Tokenizer
{
    private readonly string _texto;
    private int _pos;

    public Tokenizer(string texto) => _texto = texto;

    public List<Token> Tokenizar()
    {
        var tokens = new List<Token>();
        while (_pos < _texto.Length)
        {
            var c = _texto[_pos];

            if (char.IsWhiteSpace(c)) { _pos++; continue; }

            if (char.IsDigit(c) || (c == '.' && _pos + 1 < _texto.Length && char.IsDigit(_texto[_pos + 1])))
            {
                tokens.Add(LeerNumero());
                continue;
            }

            if (c is '\'' or '"')
            {
                tokens.Add(LeerTexto(c));
                continue;
            }

            if (char.IsLetter(c) || c == '_')
            {
                tokens.Add(LeerIdentificador());
                continue;
            }

            switch (c)
            {
                case '+': tokens.Add(new(TipoToken.Mas, "+")); _pos++; break;
                case '-': tokens.Add(new(TipoToken.Menos, "-")); _pos++; break;
                case '*': tokens.Add(new(TipoToken.Por, "*")); _pos++; break;
                case '/': tokens.Add(new(TipoToken.Dividir, "/")); _pos++; break;
                case '(': tokens.Add(new(TipoToken.ParenIzq, "(")); _pos++; break;
                case ')': tokens.Add(new(TipoToken.ParenDer, ")")); _pos++; break;
                case ',': tokens.Add(new(TipoToken.Coma, ",")); _pos++; break;
                case '=':
                    _pos++;
                    if (Actual() == '=') _pos++;
                    tokens.Add(new(TipoToken.Igual, "=="));
                    break;
                case '!':
                    _pos++;
                    if (Actual() == '=') { _pos++; tokens.Add(new(TipoToken.Distinto, "!=")); }
                    else tokens.Add(new(TipoToken.No, "!"));
                    break;
                case '<':
                    _pos++;
                    if (Actual() == '=') { _pos++; tokens.Add(new(TipoToken.MenorIgual, "<=")); }
                    else if (Actual() == '>') { _pos++; tokens.Add(new(TipoToken.Distinto, "<>")); }
                    else tokens.Add(new(TipoToken.Menor, "<"));
                    break;
                case '>':
                    _pos++;
                    if (Actual() == '=') { _pos++; tokens.Add(new(TipoToken.MayorIgual, ">=")); }
                    else tokens.Add(new(TipoToken.Mayor, ">"));
                    break;
                case '&':
                    _pos++;
                    if (Actual() == '&') _pos++;
                    tokens.Add(new(TipoToken.Y, "&&"));
                    break;
                case '|':
                    _pos++;
                    if (Actual() == '|') _pos++;
                    tokens.Add(new(TipoToken.O, "||"));
                    break;
                default:
                    throw new ExpresionException($"Carácter no reconocido: '{c}'");
            }
        }
        tokens.Add(new(TipoToken.Fin, ""));
        return tokens;
    }

    private char Actual() => _pos < _texto.Length ? _texto[_pos] : '\0';

    private Token LeerNumero()
    {
        var inicio = _pos;
        while (_pos < _texto.Length && (char.IsDigit(_texto[_pos]) || _texto[_pos] == '.')) _pos++;
        var lexema = _texto[inicio.._pos];
        if (!double.TryParse(lexema, System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out var n))
            throw new ExpresionException($"Número inválido: '{lexema}'");
        return new(TipoToken.Numero, lexema, n);
    }

    private Token LeerTexto(char comilla)
    {
        _pos++; // saltar comilla inicial
        var inicio = _pos;
        while (_pos < _texto.Length && _texto[_pos] != comilla) _pos++;
        if (_pos >= _texto.Length)
            throw new ExpresionException("Cadena de texto sin comilla de cierre.");
        var lexema = _texto[inicio.._pos];
        _pos++; // saltar comilla final
        return new(TipoToken.Texto, lexema);
    }

    private Token LeerIdentificador()
    {
        var inicio = _pos;
        while (_pos < _texto.Length && (char.IsLetterOrDigit(_texto[_pos]) || _texto[_pos] == '_')) _pos++;
        var lexema = _texto[inicio.._pos];
        return lexema.ToLowerInvariant() switch
        {
            "and" or "y" => new(TipoToken.Y, lexema),
            "or" or "o" => new(TipoToken.O, lexema),
            "not" or "no" => new(TipoToken.No, lexema),
            _ => new(TipoToken.Identificador, lexema)
        };
    }
}

/// <summary>Error de sintaxis o evaluación de una expresión de regla avanzada.</summary>
public sealed class ExpresionException(string mensaje) : Exception(mensaje);
