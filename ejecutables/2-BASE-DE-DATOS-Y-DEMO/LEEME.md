# _arranque — recursos de la BD Firebird de demostración

> **No ejecutes nada de aquí directamente.** Esta carpeta dejó de ser el panel de arranque del
> proyecto. Todo el arranque, las demos y el diagnóstico viven ahora en **`ejecutables/`**
> (ver `ejecutables/LEEME.md`). Aquí solo quedan los archivos que esas herramientas necesitan
> para levantar y poblar la base Firebird real de demostración.

## Qué hay y quién lo usa

| Archivo | Para qué sirve | Lo invoca |
|---|---|---|
| `firebird_data/` | Backup real de Contaplus (`CONTACONSTANZA-20250609.FBK`, ~364 MB) que se restaura como `CONTAC.FDB`. | `05_firebird_demo.bat` |
| `05_firebird_demo.bat` | Levanta Firebird 3.0 en Docker y restaura el backup real. | `ejecutables/3-DIAGNOSTICO/restaurar_firebird.bat` |
| `fix_firebird_auth.bat` | Recrea el usuario SYSDBA del contenedor y prueba la conexión TCP. | `restaurar_firebird.bat` y `reparar_auth_firebird.bat` |
| `96_insertar_anomalias_firebird.bat` + `inserciones_anomalias.sql` | Inserta ventas con anomalías en la BD Firebird real (para que el agente las detecte). | `ejecutables/2-DEMO/2_insertar_ventas_anomalas.bat` |
| `inserciones_anulaciones_prueba.sql` | Inserta anulaciones de prueba (tabla `ANUL`). | `ejecutables/2-DEMO/4_insertar_anulaciones_prueba.bat` |

Los `.log` que aparezcan aquí (`*_resultado.log`, `firebird_restore.log`, `fbauth-*.log`) son salidas
generadas al correr esos scripts y están ignorados por git.
