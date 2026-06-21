# Propuesta: Motor de Reglas 2.0 — reglas básicas y avanzadas

> Investigación y propuesta para mejorar la creación de reglas personalizadas
> (modo básico y modo avanzado) en PetrolRíos. Junio 2026.

## 1. Diagnóstico del sistema actual

El motor (`CustomRuleDetector` + catálogo `CatalogoReglasPersonalizadas` + DSL en
`Expresiones/`) ya es **seguro y bien hecho**, pero ambos modos tienen techos claros.

**Modo básico** (`CondicionRegla[]` + `AgregacionRegla?`):
- Las condiciones se combinan **solo con AND**. No hay OR ni grupos anidados → es
  imposible expresar `A y (B o C)`.
- Operadores escasos (6 numéricos, 6 de texto). Falta `entre`, `en lista`, `empieza/termina`.
- Una sola agregación: agrupar por **un** campo + `Conteo/Suma/Promedio` + un umbral.
  Falta `Mín/Máx/ConteoDistinto`, ventanas de tiempo y "having" con varios agregados.
- **UX confusa**: la relación entre "condiciones" y "agregación" no es evidente, no hay
  vista previa y no se resume en lenguaje claro lo que la regla hará.

**Modo avanzado** (`ExpresionAvanzada`, DSL propio):
- Parser **seguro** (AST, no ejecuta código) con aritmética, comparación, lógica
  (`&&/||/!`), paréntesis y 9 funciones (`abs, redondear, minusculas, mayusculas,
  longitud, vacio, contiene, empieza, termina`).
- Pero **evalúa registro por registro**: no hay agregación, ni fechas, ni listas, ni
  acceso a registros relacionados. El editor es un `textarea` plano (sin autocompletado
  ni ayuda de campos/funciones).
- La salida es solo un booleano → una alerta por registro. No se puede personalizar el
  mensaje, el ámbito ni el score desde la expresión.

**Fortaleza a conservar:** el evaluador propio es seguro y es un activo de tesis (no
dependemos de ejecutar código de usuario). La meta no es tirarlo, sino potenciarlo.

## 2. Qué hace la industria (investigación web)

**Constructores visuales (frontend).** `react-querybuilder` es el estándar de facto en
React: soporta **grupos AND/OR/NOT anidados**, produce un JSON limpio, valida, permite
editores de valor personalizados por campo y **exporta** a SQL, MongoDB, JSONLogic y CEL.
Resuelve de raíz el problema #1 del modo básico (solo-AND + UX) con una librería probada
que encaja con React + TS + shadcn.

**Formatos de reglas portables (backend).**
- **JSONLogic**: el formato más usado para reglas; está pensado para **compartir la lógica
  entre front y back** y guardarla junto al registro en BD. En .NET hay implementación
  moderna sobre `System.Text.Json` (json-everything, sin reflexión, AOT-friendly).
- **CEL (Common Expression Language, Google)**: lenguaje **no-Turing, seguro y sandboxed**
  con listas, mapas, fechas/duraciones y macros (`has`, `exists`, `all`, `filter`).
  Implementaciones .NET: `telus-oss/cel-net`, `rayokota/cel.net`.

**UX de productos de fraude (referencia: Stripe Radar).**
- Las reglas son **condiciones + acciones** (allow/block/review/3DS). Cientos de atributos.
- **Backtest histórico antes de guardar**: muestra cuántas transacciones habría afectado
  la regla y cómo habrían terminado, *antes* de activarla. Es el patrón de mayor valor.
- **Asistente lenguaje-natural → regla**: el usuario escribe en español y se traduce a la
  sintaxis de la regla.
- Buenas prácticas no-code: resumen en lenguaje natural, validación en línea, plantillas.

## 3. Propuesta — 3 pilares

### Pilar A · Básico visual con grupos (adoptar `react-querybuilder`)
Reemplazar la lista solo-AND por un constructor visual con **grupos AND/OR/NOT anidados**,
alimentado por el catálogo de campos actual (tipos y operadores por campo), con operadores
nuevos (`entre`, `en lista`, `vacío`), y un **resumen en prosa** debajo ("Alerta cuando la
factura es en efectivo Y (placa vacía O monto > $200)").

### Pilar B · Avanzado con superpoderes (extender el DSL propio)
Mantener el evaluador seguro y **ampliarlo**:
- Funciones de **fecha/hora** (`hora`, `diaSemana`, `fecha`, `diferenciaMinutos`),
  **matemáticas** (`min`, `max`, `piso`, `techo`, `modulo`), **listas** (`en [a,b,c]`),
  **texto** (`coincide`/regex acotado), y coalescencia de nulos.
- **Editor de verdad**: CodeMirror/Monaco con autocompletado de campos y funciones, errores
  en línea y una paleta de campos/funciones (en vez del `textarea`).

### Pilar C · Transversal (lo que faltaba en ambos)
- **Agregación v2**: `Mín/Máx/ConteoDistinto`, **ventanas de tiempo** ("N eventos en M
  minutos") y varias condiciones de umbral.
- **Backtest / vista previa**: antes de guardar, correr la regla contra los últimos N días
  de datos de staging y mostrar "esto habría generado X alertas / coincidió con estos
  registros". Altísimo valor y muy demostrable en la defensa.
- **Plantillas** (galería de ejemplos) + **resumen en lenguaje natural** + (opcional)
  **lenguaje natural → regla**.
- **Salida configurable**: que la regla defina su **mensaje**, **ámbito** y **score** — así
  el poder/"libertad de creación" no está solo en el filtro, también en lo que se emite.

## 4. Decisión de arquitectura (mi recomendación)

Mantener el **AST seguro propio como evaluador canónico**. El constructor visual (Pilar A)
y el texto avanzado (Pilar B) **compilan al mismo árbol lógico** que el motor ya sabe
evaluar (`NodoBinario` AND/OR es justo lo que produce `react-querybuilder`). Resultado: **un
solo motor para ambos modos**, sin nueva dependencia pesada, y una historia de arquitectura
limpia para la tesis.

JSONLogic queda como **alternativa estándar** si se prefiere no mantener el DSL (json-everything
en .NET); es más "estándar de industria" pero menos "trabajo propio demostrable".

## 5. Plan por fases sugerido

1. **Backtest/vista previa** (mayor valor, transversal, demostrable). 
2. **Básico visual** con OR/grupos (`react-querybuilder`) + resumen en prosa.
3. **Avanzado**: funciones de fecha/listas/matemáticas + editor con autocompletado.
4. **Plantillas** + **salida configurable** + (stretch) **lenguaje natural → regla**.

## Fuentes

- [react-querybuilder (npm)](https://www.npmjs.com/package/react-querybuilder) · [Docs / Rules engine](https://react-querybuilder.js.org/docs/rules-engine) · [GitHub](https://github.com/react-querybuilder/react-querybuilder)
- [JsonLogic](https://jsonlogic.com/) · [JsonLogic en .NET (json-everything)](https://docs.json-everything.net/logic/basics/) · [JsonLogic.Net](https://github.com/yavuztor/JsonLogic.Net)
- [json-rules-engine vs json-logic (npm trends)](https://npmtrends.com/json-logic-js-vs-json-rules-engine) · [Decision engines in production](https://hackernoon.com/decision-engines-in-production-json-logic-rules-engines-and-when-to-scale)
- [CEL — Common Expression Language](https://cel.dev/) · [Overview](https://cel.dev/overview/cel-overview) · [cel-net (.NET)](https://github.com/telus-oss/cel-net) · [cel.net (.NET)](https://github.com/rayokota/cel.net)
- [Stripe Radar — reglas](https://docs.stripe.com/radar/rules) · [Rules 101](https://stripe.com/guides/radar-rules-101) · [Testing / backtest](https://docs.stripe.com/radar/testing)
- [Rule Builder design pattern (ui-patterns)](https://ui-patterns.com/patterns/rule-builder) · [Visual Rule Builder UX (InRule)](https://inrule.com/ux-builder/)
