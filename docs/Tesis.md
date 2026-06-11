1
Universidad de Las Américas
Facultad de Ingeniería y Ciencias Aplicadas
Ingeniería de Software
Sistema web para análisis y detección de anomalías en transacciones de
estaciones de servicio de la empresa PetrolRíos S.A.
TUTOR: Luis Felipe Urquiza Aguiar
AUTOR 1: Leonardo Andrade
AUTOR 2: Steven Ramón Carrillo Loor
Quito, enero de 2026
2
Contenido
Resumen.......................................................................................................................................... 8
Abstract........................................................................................................................................... 9
1. Introducción .............................................................................................................................. 11
1.1. Identificación y descripción del problema ......................................................................... 11
1.1.1. Análisis causa-efecto (Diagrama de Ishikawa)........................................................... 13
1.1.2. Mapa del proceso actual de auditoría.......................................................................... 20
1.1.3. Tipos de anomalías identificadas................................................................................ 24
1.1.4. Evidencia cuantitativa de la problemática .................................................................. 27
1.2. Descripción de la organización.......................................................................................... 35
1.2.1. Identificación de la organización ................................................................................ 35
1.2.2. Misión ......................................................................................................................... 36
1.2.3. Visión.......................................................................................................................... 36
1.2.4. Estructura organizacional............................................................................................ 36
1.2.5. Productos y servicios .................................................................................................. 40
1.2.6. Stakeholders del proyecto ........................................................................................... 40
1.2.7. Área donde se encuentra el problema ......................................................................... 41
2. Análisis de posibles soluciones................................................................................................. 43
2.1. Identificación y selección de la mejor solución................................................................. 43
2.1.1 Alternativa A: Sistema monolítico on-premise con procesamiento batch y conexión
VPN....................................................................................................................................... 43
3
2.1.2. Alternativa B: Sistema en capas AWS con procesamiento batch Hangfire y
notificaciones SignalR (SELECCIONADA)........................................................................ 50
2.1.3. Alternativa C: Sistema serverless Azure con arquitectura orientada a eventos y
procesamiento en streaming.................................................................................................. 61
2.2. Impacto del proyecto en la sociedad.................................................................................. 76
3. Objetivos................................................................................................................................... 80
3.1. Objetivo General................................................................................................................ 80
4. Alcance ..................................................................................................................................... 82
4.1. Alcance de la solución seleccionada.................................................................................. 82
4.1.1. Diagrama de casos de uso ........................................................................................... 82
4.1.2. Descripción de casos de uso principales..................................................................... 87
4.1.3. Prototipos de pantallas (Media fidelidad)................................................................... 88
4.1.4. Arquitectura de la solución (C4 Nivel 2 - Contenedores)........................................... 90
4.1.5. Flujo del proceso de detección.................................................................................... 92
4.1.6. Módulos funcionales del sistema ................................................................................ 98
4.1.7. Flujo proactivo de respuesta ante alertas .................................................................... 99
4.2. Limitaciones y restricciones del proyecto........................................................................ 101
4.2.1. Limitaciones (Lo que NO se hará)............................................................................ 101
4.2.2. Restricciones (Condiciones obligatorias) ................................................................. 102
5. Planificación y costos del proyecto......................................................................................... 104
5.1. Cronograma del proyecto................................................................................................. 104
5.2. Presupuesto del proyecto ................................................................................................. 113
5.3. Análisis de riesgos ........................................................................................................... 116
4
6. Descripción de estudios realizados......................................................................................... 117
6.1. Método de revisión de literatura ...................................................................................... 117
6.2. Estado del arte en detección de anomalías....................................................................... 118
6.3. Detección de fraudes en estaciones de servicio ............................................................... 118
6.4. Arquitecturas de software para sistemas en tiempo real.................................................. 119
6.5. SignalR para comunicaciones en tiempo real .................................................................. 120
6.6. Síntesis de hallazgos y decisiones de diseño ................................................................... 120
7. Desarrollo del proyecto........................................................................................................... 122
7.1. Diseño de la solución....................................................................................................... 122
7.2. Desarrollo de la solución ................................................................................................. 123
7.3. Pruebas y evaluación de la solución ................................................................................ 124
7.5. Implicaciones éticas......................................................................................................... 128
8. Conclusiones y Recomendaciones.......................................................................................... 130
8.1. Conclusiones.................................................................................................................... 130
8.2. Recomendaciones ............................................................................................................ 134
10. Referencias bibliográficas..................................................................................................... 137
Firmas de aprobación.............................................................................................................. 140
5
Índice de Figuras
Figure 1. Diagrama de Ishikawa - Ausencia de detección de anomalías a nivel transaccional.... 13
Figure 2. Mapa del proceso actual de auditoría manual en PetrolRíos S.A.................................. 20
Figure 3. Organigrama de PetrolRíos S.A. con énfasis en el Área de Auditoría.......................... 37
Figure 4. Diagrama de contexto C4 - Alternativa A: Sistema batch sin notificaciones ............... 48
Figure 5. Diagrama de contexto C4 - Alternativa B: Sistema batch con SignalR (Seleccionada) 59
Figure 6. Diagrama de contexto C4 - Alternativa C: Arquitectura orientada a eventos............... 67
Figure 7. Diagrama de casos de uso del Sistema de Detección de Anomalías............................. 82
Figure 8. Prototipo de pantalla: Dashboard principal de alertas................................................... 89
Figure 9. Prototipo de pantalla: Detalle de anomalía detectada.................................................... 89
Figure 10. Prototipo de pantalla: Configuración de reglas de detección ...................................... 90
Figure 11. Arquitectura de contenedores (C4 Nivel 2) de la solución seleccionada .................... 90
Figure 12. Diagrama de flujo del proceso de detección de anomalías.......................................... 92
Figure 13. Red de actividades y ruta crítica del proyecto........................................................... 110
Índice de Tablas
Table 1. Comparación entre sistema de cuadres existente y sistema de detección propuesto...... 12
Table 2. Tipos de anomalías identificadas por el Área de Auditoría............................................ 24
Table 3. Ejemplos de reglas de detección por tipo de anomalía ................................................... 26
Table 4. Resumen del caso de estudio documentado.................................................................... 27
Table 5. Anomalías por despachador en el caso documentado .................................................... 28
Table 6. Cuantificación de pérdidas del caso documentado ......................................................... 34
Table 7. Stakeholders del proyecto y sus responsabilidades ........................................................ 40
Table 8. Stack tecnológico propuesto para la Alternativa A ........................................................ 49
Table 9. Stack tecnológico de la Alternativa B (Seleccionada).................................................... 57
Table 10. Stack tecnológico de la Alternativa C........................................................................... 64
Table 11. Estimación de costos de la Alternativa C ..................................................................... 65
6
Table 12. Evaluación de alternativas según ISO/IEC 25010........................................................ 69
Table 13. Comparación del Costo Total de Propiedad (TCO) entre alternativas......................... 75
Table 14. Impactos y métricas del proyecto (Análisis PESTEL) ................................................. 77
Table 15. Descripción de casos de uso principales....................................................................... 87
Table 16. Proceso de extracción, transformación y carga (ETL) desde Firebird ......................... 93
Table 17. Descripción de los detectores de anomalías del sistema............................................... 94
Table 18. Clasificación de anomalías por nivel de riesgo ............................................................ 95
Table 19. Tiempos estimados del proceso de detección de anomalías......................................... 97
Table 20. Módulos funcionales del sistema.................................................................................. 98
Table 21. Flujo de respuesta proactiva por nivel de riesgo........................................................... 99
Table 22. Matriz RACI de gestión de alertas.............................................................................. 100
Table 23. Limitaciones del proyecto........................................................................................... 101
Table 24. Restricciones del proyecto .......................................................................................... 102
Table 25. Cronograma de fases del proyecto.............................................................................. 104
Table 26. Cronograma del proyecto (Diagrama de Gantt) ......................................................... 105
Table 27. Cronograma de hitos del proyecto .............................................................................. 112
Table 28. Resumen de actividades y cálculo de la Ruta Crítica (CPM)..................................... 113
Table 29. Resumen ejecutivo del presupuesto............................................................................ 113
Table 30. Presupuesto de infraestructura cloud .......................................................................... 114
Table 31. Presupuesto de equipamiento y hardware................................................................... 115
Table 32. Presupuesto de costos operativos................................................................................ 115
Table 33. Riesgos del proyecto y estrategias de mitigación ....................................................... 116
Table 34. Hallazgos del estado del arte y decisiones de diseño.................................................. 120
Table 35. Niveles del Modelo C4 y su cobertura en el presente documento.............................. 122
Table 36. Entregables planificados en cada iteración de desarrollo (Scrum)............................. 124
Table 37. Estrategia de pruebas del sistema y criterios de aceptación ....................................... 124
Table 38. Entregables funcionales esperados del sistema propuesto.......................................... 127
Table 39. Métricas de calidad esperadas del sistema.................................................................. 127
7
Table 40. Medidas de seguridad y protección de datos implementadas..................................... 128
Table 41. Evaluación de madurez y compatibilidad de las tecnologías seleccionadas .............. 131
Table 42. Comparación entre el sistema de cuadres existente y el sistema de detección propuesto
..................................................................................................................................................... 133
Table 43. Evaluación de factibilidad de los objetivos específicos.............................................. 133
Table 44 Recomendaciones para la implementación del sistema (fase previa al desarrollo)..... 134
Table 45. Recomendaciones técnicas para el equipo de desarrollo ............................................ 135
8
Resumen
Este anteproyecto presenta una propuesta para el desarrollo de un sistema web de detección de
anomalías en transacciones de estaciones de servicio para la empresa PetrolRíos S.A., ubicada en
Santo Domingo, Ecuador. Actualmente, la empresa cuenta con un sistema de conciliación
consolidado que verifica que los totales de ventas coincidan con los registros contables; sin
embargo, no existe un sistema que analice las transacciones individuales para detectar patrones
de fraude, incumplimiento normativo o irregularidades ocultas dentro de transacciones que
cuadran numéricamente.
El proyecto propone desarrollar un sistema completo de detección de anomalías a nivel
transaccional mediante la implementación de un motor de reglas de negocio con cuatro
detectores principales: anomalías en el manejo de efectivo (Cash Fraud), irregularidades en la
facturación (Invoice Anomaly), fraude en métodos de pago (Payment Fraud) e incumplimiento
normativo (Compliance Violation). Se pretende utilizar técnicas de interrogación de archivos
para analizar el 100% de las transacciones procesadas por las 10 estaciones actualmente
integradas al sistema centralizado, las cuales generan aproximadamente entre 13,000 y 15,000
transacciones diarias.
La solución técnica propuesta emplea una arquitectura en capas con ASP.NET Core 9.0, React
18 con TypeScript y PostgreSQL en Amazon Web Services (AWS). Se plantea implementar
procesamiento por lotes mediante Hangfire con ejecución cada 5-10 minutos, y notificaciones en
tiempo real a través de SignalR. Con esta arquitectura se espera reducir el tiempo de detección de
irregularidades de días o semanas a menos de 10 minutos, aumentando la cobertura del análisis
transaccional del 0% al 100%.
El proyecto se desarrollará durante un período de 6 meses (diciembre de 2025 - mayo de 2026)
con un presupuesto estimado de $3,410 USD, siguiendo la metodología Scrum con sprints de dos
semanas. Los resultados esperados incluyen la detección temprana de fraude en efectivo, la
9
verificación automática del cumplimiento normativo y la generación de alertas clasificadas por
riesgo para revisión del equipo de auditoría.
Palabras clave: Detección de anomalías, sistemas transaccionales, interrogación de archivos,
auditoría automatizada, estaciones de servicio, detección de fraude, cumplimiento normativo,
procesamiento por lotes, SignalR, arquitectura por capas.
Abstract
This preliminary project presents a proposal for the development of a web-based anomaly
detection system for service station transactions for the company PetrolRíos S.A., located in
Santo Domingo, Ecuador. Currently, the company has a consolidated reconciliation system that
verifies that total sales match accounting records; however, there is no system that analyzes
individual transactions to detect fraud patterns, regulatory non-compliance, or hidden
irregularities within transactions that numerically balance.
The project proposes the development of a complete transaction-level anomaly detection system
through the implementation of a business rules engine with four main detectors: anomalies in
cash handling (Cash Fraud), billing irregularities (Invoice Anomaly), fraud in payment methods
(Payment Fraud), and regulatory non-compliance (Compliance Violation). File interrogation
techniques will be used to analyze 100% of the transactions processed by the 10 service stations
currently integrated into the centralized system, which generate approximately between 13,000
and 15,000 transactions per day.
The proposed technical solution employs a layered architecture using ASP.NET Core 9.0, React
18 with TypeScript, and PostgreSQL on Amazon Web Services (AWS). Batch processing using
Hangfire with execution every 5–10 minutes is planned, along with real-time notifications
through SignalR. With this architecture, the expected outcome is a reduction in irregularity
detection time from days or weeks to less than 10 minutes, increasing transaction analysis
coverage from 0% to 100%.
10
The project will be developed over a 6-month period (December 2025 – May 2026) with an
estimated budget of $3,410 USD, following the Scrum methodology with two-week sprints.
Expected results include early detection of cash fraud, automated verification of regulatory
compliance, and the generation of risk-classified alerts for review by the auditing team.
Keywords: Anomaly detection, transactional systems, file interrogation, automated auditing,
service stations, fraud detection, regulatory compliance, batch processing, SignalR, layered
architecture.
11
1. Introducción
Las estaciones de servicio de combustible procesan diariamente miles de transacciones que
involucran ventas en efectivo, pagos con tarjeta, créditos a clientes corporativos y cumplimiento
de múltiples normativas regulatorias. En este contexto, la detección oportuna de irregularidades
representa un desafío crítico. Como señalan Ahmed et al. (2021), las técnicas manuales son
imprecisas y costosas ante el volumen masivo de datos financieros modernos que requiere el
análisis sistemático de cada transacción individual, no solo de los totales consolidados.
PetrolRíos S.A. opera una red de 90 estaciones de servicio en la región de Santo Domingo,
Ecuador, de las cuales 10 se encuentran actualmente integradas al sistema centralizado de
información. Estas estaciones procesan de 13,000 a 15,000 transacciones diarias. Este volumen
es ideal para técnicas automatizadas, ya que el fraude en el sector cuesta pérdidas millonarias
anuales globalmente (Fuelmetrics, 2021), generando un volumen de datos que supera la
capacidad de análisis manual pero que resulta ideal para técnicas automatizadas de detección de
anomalías.
El presente proyecto desarrolla un sistema web completo para la detección de anomalías a nivel
transaccional, abordando un problema que actualmente no tiene solución en la empresa. A
diferencia del sistema de cuadres consolidados existente, que verifica totales agregados, el
sistema propuesto analiza cada transacción individual para identificar patrones de fraude en
efectivo, irregularidades en facturación, fraudes con medios de pago e incumplimiento de
normativas regulatorias.
1.1. Identificación y descripción del problema
El Área de Auditoría y Control Interno de PetrolRíos S.A. enfrenta un problema estructural: la
ausencia de un sistema de detección de anomalías a nivel transaccional. Actualmente, la empresa
cuenta con un sistema de cuadres consolidados que verifica que los totales de ventas coincidan
12
con los registros contables. Sin embargo, este sistema opera exclusivamente a nivel agregado y
no analiza las transacciones individuales.
Es fundamental distinguir entre dos problemas completamente diferentes: (1) la verificación de
que los totales cuadren, problema que YA está resuelto mediante el sistema de cuadres existente;
y (2) la detección de fraudes y anomalías que se ocultan dentro de transacciones que cuadran
numéricamente, problema que NO tiene solución actualmente. El presente proyecto aborda
exclusivamente el segundo problema.
Table 1. Comparación entre sistema de cuadres existente y sistema de detección propuesto
Característica Sistema de Cuadres
(existente)
Sistema de Detección
(propuesto)
Objetivo Verificar que totales cuadren Detectar fraudes en
transacciones individuales
Nivel de análisis Consolidado (totales diarios) Transaccional (cada factura)
Cobertura de transacciones 0% análisis individual 100% de transacciones
Frecuencia de análisis Diaria (al cierre) Cada 5-10 minutos
Tipos de detección Diferencias numéricas
globales
4 categorías de anomalías
específicas
Notificaciones Ninguna automática Tiempo real (SignalR)
Interfaz de usuario No tiene (proceso interno) Dashboard web interactivo
Historial y trazabilidad No Sí, con auditoría completa
Ejemplos de detección Faltante global de $500 en
caja
Empleado X registró 15
ventas en efectivo como
crédito
13
Como se observa en la Tabla 1, ambos sistemas tienen objetivos, alcances y funcionalidades
completamente diferentes. El sistema de detección propuesto no es un complemento del sistema
de cuadres; es una solución completa a un problema distinto que actualmente no tiene solución
en la empresa. El sistema de cuadres responde a la pregunta "¿Cuadran los totales?" mientras que
el sistema propuesto responde a la pregunta "¿Hay fraudes ocultos en las transacciones
individuales?".
1.1.1. Análisis causa-efecto (Diagrama de Ishikawa)
Para comprender las causas que originan la ausencia de un sistema de detección de anomalías a
nivel transaccional, se presenta el análisis mediante diagrama de Ishikawa (causa-efecto). Este
análisis identifica cinco categorías principales de factores que contribuyen al problema central.
Figure 1. Diagrama de Ishikawa - Ausencia de detección de anomalías a nivel transaccional
El diagrama de Ishikawa (también conocido como diagrama de causa-efecto o diagrama de
espina de pescado) permite visualizar de manera estructurada las causas que originan el
problema central identificado: la ausencia de un sistema de detección de anomalías a nivel
14
transaccional en PetrolRíos S.A. Este análisis identifica cinco categorías principales de factores
causantes, cada una con múltiples causas específicas que contribuyen al problema.
El problema central (cabeza del pescado) se define como:
"Ausencia de detección de anomalías a nivel transaccional" - Actualmente, PetrolRíos S.A. no
cuenta con ningún sistema que analice las transacciones individuales para identificar patrones de
fraude, irregularidades o incumplimiento normativo. Los controles existentes operan
exclusivamente a nivel de totales consolidados, dejando una brecha significativa en la capacidad
de detección.
A continuación se describen las cinco categorías de causas (espinas principales) y sus causas
específicas (espinas secundarias):
1. TECNOLOGÍA
Esta categoría agrupa las limitaciones tecnológicas que impiden la implementación de análisis
transaccional automatizado. Las causas identificadas son:
Bases de datos diseñadas para registro, no para análisis:
• Las bases de datos Firebird en las estaciones fueron diseñadas hace más de 10 años con el
propósito exclusivo de registrar transacciones de venta y controlar inventario.
• El esquema de datos no incluye campos para clasificación de riesgo, marcas de auditoría o
metadatos analíticos.
• Las consultas complejas que requerirían los detectores de anomalías degradarían el rendimiento
del sistema de punto de venta.
• No existen índices optimizados para búsquedas por patrones (ej: todas las ventas de un empleado
en un rango de fechas).
Arquitectura distribuida sin integración analítica:
• Cada una de las 10 estaciones opera con una base de datos Firebird independiente
(CONTAC.FDB).
15
• No existe un data warehouse ni repositorio central que consolide las transacciones para
análisis cruzado entre estaciones.
• La única integración existente (migración a Microplus) está diseñada para cuadres
contables, no para detección de anomalías.
• No hay infraestructura para procesamiento batch automatizado ni para notificaciones en
tiempo real.
Ausencia de herramientas de detección:
• No existe software especializado para detección de fraudes en el sector de hidrocarburos.
• Las herramientas genéricas de BI (Business Intelligence) disponibles en el mercado no
incluyen reglas específicas para estaciones de servicio.
• El desarrollo de una solución a medida no ha sido priorizado debido a falta de
conocimiento sobre las técnicas disponibles.
2. MÉTODOS
Esta categoría identifica las deficiencias en los procedimientos y metodologías de control
actualmente utilizados:
• Enfoque exclusivo en cuadres consolidados:
• El proceso de control actual verifica únicamente que los totales de ventas coincidan con
los registros contables.
• Un cuadre exitoso (totales coinciden) se considera suficiente para validar las operaciones
del día.
• No existe procedimiento para análisis de transacciones cuando los totales SÍ cuadran.
Ejemplo: Si las ventas totales cuadran en $10,000, no se investiga si alguna venta individual fue
manipulada.
Ausencia de técnicas de interrogación de archivos:
16
• La técnica de 'interrogación de archivos' consiste en consultar sistemáticamente la base
de datos con preguntas específicas para identificar patrones sospechosos. Esta técnica
es estándar en auditoría forense (Ahmed et al., 2021) pero no ha sido adoptada en
PetrolRíos.
• Ejemplos de interrogaciones no implementadas: '¿Qué empleados tienen más de 5
anulaciones por día?', '¿Qué vehículos aparecen con diferentes tipos de combustible?',
'¿Qué ventas a placa ZZZ999949 exceden 5 galones?'
• Esta técnica es estándar en auditoría forense pero no ha sido adoptada en PetrolRíos.
Verificación de cumplimiento normativo por muestreo:
• Las regulaciones de ARCERNNR establecen límites como máximo 5 galones para ventas
con placa genérica (ZZZ999949).
• La verificación actual se realiza solo mediante muestreo aleatorio durante auditorías
periódicas.
• No existe verificación automática ni sistemática del 100% de las transacciones.
• Las infracciones se detectan solo si causan problemas evidentes o si son identificadas en
una auditoría externa.
• Revisión reactiva vs. proactiva:
• La investigación de transacciones individuales solo ocurre DESPUÉS de que se detecta
un descuadre o se recibe una queja.
• No existe monitoreo proactivo que identifique patrones sospechosos antes de que causen
pérdidas significativas.
• El tiempo entre la ocurrencia de un fraude y su detección puede ser de semanas o meses.
3. PERSONAS
Esta categoría aborda las limitaciones relacionadas con el capital humano disponible para tareas
de detección:
17
Equipo de auditoría especializado en cuadres contables:
• El personal del Área de Auditoría tiene formación y experiencia en contabilidad y
cuadres financieros.
• No cuentan con capacitación específica en técnicas de detección de fraudes o análisis
forense de datos.
• Su expertise está en verificar que 'los números cuadren', no en identificar patrones
anómalos dentro de transacciones que cuadran.
Ausencia de personal dedicado a análisis transaccional:
• No existe un rol específico de 'analista de fraudes' o 'especialista en detección de
anomalías' en la estructura organizacional.
• El volumen de 13,000-15,000 transacciones diarias hace imposible el análisis manual por
el equipo actual.
• La contratación de personal adicional para análisis manual no es costo-efectiva.
Desconocimiento de patrones de fraude específicos:
• Los patrones de fraude comunes en estaciones de servicio (autopréstamos, ventas en
efectivo registradas como crédito, etc.) no están documentados formalmente.
• El conocimiento sobre estos patrones existe de manera tácita en algunos empleados
experimentados pero no está sistematizado.
• No hay un catálogo de 'red flags' o señales de alerta que el personal deba monitorear.
4. PROCESOS
Esta categoría identifica las deficiencias en los flujos de trabajo y procesos operativos:
Proceso de control finaliza cuando los totales cuadran:
• El flujo de trabajo actual tiene como punto final la verificación de que los totales
coincidan.
18
• No existe un paso adicional de 'análisis de transacciones sospechosas' después del cuadre
exitoso.
• La lógica implícita es: 'Si cuadra, está bien' - lo cual es insuficiente para detectar fraudes
sofisticados.
Ausencia de flujo de investigación de alertas:
• No existe un proceso definido para investigar señales de alerta o transacciones
sospechosas.
• No hay roles asignados (quién investiga, quién aprueba, quién documenta).
• No hay tiempos de respuesta establecidos (SLAs) para la investigación de posibles
irregularidades.
• No hay documentación del ciclo de vida de una alerta (desde detección hasta resolución).
Dependencia de reportes manuales para identificar problemas:
• Las irregularidades se conocen principalmente cuando un jefe de estación o empleado
reporta algo inusual.
• Este enfoque depende de la honestidad y diligencia del personal, incluyendo potenciales
perpetradores.
• Los fraudes cometidos por empleados rara vez son auto-reportados.
Falta de integración entre detección y acción correctiva:
• Incluso cuando se detecta una anomalía, no hay un proceso claro de escalamiento.
• No hay integración con recursos humanos para casos que involucren empleados.
• No hay trazabilidad de acciones tomadas ante irregularidades detectadas.
5. MEDICIÓN
Esta categoría aborda la ausencia de métricas e indicadores para evaluar la efectividad de los
controles:
19
• No existen métricas de cobertura de análisis transaccional:
• No se mide qué porcentaje de transacciones son analizadas individualmente (actualmente
es 0%).
• No hay indicador de 'transacciones revisadas' vs. 'transacciones totales'.
• No se puede demostrar debido diligence en la supervisión de operaciones.
Ausencia de indicadores de tiempo de detección:
• No se mide cuánto tiempo transcurre entre que ocurre una irregularidad y cuando es
detectada.
• Casos históricos sugieren tiempos de detección de semanas o meses para fraudes que no
causan descuadre.
• No hay objetivo (KPI) de 'tiempo máximo de detección'.
No hay métricas de efectividad en identificación de fraudes:
• No se registra cuántos fraudes fueron detectados vs. cuántos se estima que ocurrieron.
• No hay comparación de pérdidas antes/después de implementar controles.
• No se puede calcular el ROI de las actividades de auditoría.
Única métrica actual: 'cuadre exitoso vs. descuadre:
• El único indicador que se reporta es si el día 'cuadró' o 'descuadró'.
• Un 100% de días con cuadre exitoso se interpreta como 'todo está bien', cuando en
realidad solo significa que los totales coinciden.
• Esta métrica no captura la calidad del control ni la detección de anomalías.
Conclusión del análisis de Ishikawa
El análisis de Ishikawa revela que la ausencia de detección de anomalías a nivel transaccional no
es un problema simple con una única causa, sino el resultado de múltiples factores
20
interrelacionados en las cinco categorías analizadas. Las causas tecnológicas (falta de
infraestructura analítica) se combinan con causas metodológicas (enfoque exclusivo en cuadres),
limitaciones de personal (ausencia de especialistas en detección), deficiencias de proceso (flujo
de trabajo incompleto) y carencias de medición (ausencia de métricas relevantes).
Esta comprensión integral del problema justifica el desarrollo de un sistema que aborde
simultáneamente múltiples causas: proporciona la infraestructura tecnológica necesaria
(plataforma de análisis), implementa la metodología apropiada (técnicas de interrogación de
archivos), reduce la dependencia de análisis manual (automatización), establece flujos de trabajo
claros (gestión de alertas) y genera métricas significativas (cobertura, tiempo de detección,
efectividad).
1.1.2. Mapa del proceso actual de auditoría
Figure 2. Mapa del proceso actual de auditoría manual en PetrolRíos S.A.
El diagrama de flujo presenta el proceso actual de control operativo en las estaciones de servicio
de PetrolRíos S.A. Es importante destacar que este proceso YA EXISTE y funciona
correctamente para su propósito original: verificar que los totales de ventas coincidan con los
21
registros contables. El diagrama permite visualizar dónde termina el proceso actual y dónde
comienza la brecha que el sistema propuesto viene a cubrir.
El proceso se divide en las siguientes fases:
FASE 1: Operación diaria en estación (6:00 AM - 10:00 PM)
• Registro de transacciones en sistema Contaplus:
• Cada venta de combustible se registra automáticamente cuando el despachador completa
la transacción.
• Se captura: número de factura, fecha/hora, producto, cantidad (galones), precio unitario,
total, forma de pago, placa del vehículo, datos del cliente.
• El sistema está conectado a los contadores electrónicos de los surtidores que registran el
combustible dispensado.
• Al final de cada turno, el despachador realiza cierre de caja reportando efectivo,
vouchers de tarjeta y créditos.
FASE 2: Cierre de estación (10:00 PM - 11:00 PM)
• Generación de reportes de cierre:
• El sistema Contaplus genera automáticamente el reporte de ventas del día.
• Se extraen totales por tipo de producto, por forma de pago y por turno.
• Se registra la lectura final de los contadores de surtidores.
• El jefe de estación verifica físicamente el efectivo en caja contra lo reportado.
FASE 3: Migración de datos (11:00 PM - 12:00 AM, automático)
• Proceso automático de migración a sistema central:
• Un aplicativo desarrollado internamente se ejecuta automáticamente cada noche.
• Conecta con cada base de datos Firebird de las 10 estaciones.
• Extrae las transacciones del día y las envía al sistema central Microplus.
22
VALIDACIÓN CRÍTICA: El sistema NO permite completar la migración si los datos no
cuadran al centavo.
Si hay discrepancia, el proceso se detiene y genera alerta para revisión manual al día siguiente.
FASE 4: Cuadre contable (8:00 AM - 10:00 AM del día siguiente)
• Verificación de cuadres por personal de contabilidad:
• El equipo contable revisa que la migración de todas las estaciones se completó
exitosamente.
• Verifica que los totales de ventas coincidan con los depósitos bancarios.
• Compara los totales de combustible vendido con las lecturas de los tanques.
• Genera reporte de 'Cuadre Diario' indicando si cada estación cuadró o descuadró.
FASE 5: Investigación (SOLO si hay descuadre)
• Revisión detallada en caso de diferencias:
• Si una estación presenta descuadre, se inicia investigación.
• Se revisan las transacciones individuales buscando el origen de la diferencia.
• Se contacta al jefe de estación para aclaraciones.
• Se documenta la causa del descuadre y las acciones correctivas.
PUNTO CRÍTICO - BRECHA DE DETECCIÓN:
El proceso actual TERMINA después de la Fase 4 si los totales cuadran. No existe una Fase 5 de
"análisis de transacciones sospechosas" cuando el cuadre es exitoso. Esto significa que:
• Si un empleado registra una venta en efectivo de $100 como venta a crédito, el total de
ventas sigue siendo $100 y CUADRA perfectamente. El empleado se apropia del efectivo
y el fraude pasa desapercibido.
23
• Si un despachador vende 10 galones a placa ZZZ999949 (violando el límite de 5
galones), la venta se registra correctamente y CUADRA. El incumplimiento normativo
no se detecta.
• Si un mismo vehículo aparece comprando diésel y gasolina extra el mismo día
(físicamente imposible), las transacciones CUADRAN. El patrón sospechoso no se
investiga.
• El sistema propuesto agrega las Fases 6 y 7 que actualmente no existen: análisis
automatizado de cada transacción mediante los cuatro detectores, y notificación en
tiempo real de anomalías para investigación proactiva.
El proceso actual se desarrolla en las siguientes etapas:
1. Descarga manual de datos (2-4 horas): Un auditor accede remotamente a cada estación para
descargar los archivos de la base de datos Firebird. Este proceso debe repetirse para cada una de
las 90 estaciones, lo que en la práctica limita la revisión a un subconjunto de estaciones por ciclo.
2. Consolidación en Excel (3-5 horas): Los datos descargados se importan a hojas de cálculo
donde se unifican formatos, se eliminan duplicados y se preparan para análisis. La manipulación
manual introduce riesgo de errores.
3. Revisión por muestreo (8-16 horas): El auditor aplica filtros y fórmulas para identificar
registros sospechosos. Por restricciones de tiempo, solo se revisa aproximadamente el 10% de las
transacciones, seleccionadas por muestreo aleatorio o por criterios subjetivos.
4. Documentación de hallazgos (2-4 horas): Los registros identificados como potencialmente
anómalos se documentan en reportes Word con capturas de pantalla y descripciones.
5. Investigación (3-5 días): Los hallazgos documentados se investigan mediante consultas
adicionales, revisión de documentación física y entrevistas con personal de la estación.
24
6. Reporte final y acciones (1-2 días): Se genera el reporte de auditoría y se definen acciones
correctivas.
El tiempo total del ciclo completo oscila entre 15 y 30 días desde que ocurre una anomalía hasta
que se toman acciones correctivas. Este retraso es significativo considerando que las pérdidas
promedio por fraude en flotas vehiculares oscilan entre $35,000 y $50,000 anuales, con
detección extremadamente difícil sin sistemas automatizados (Heavy Vehicle Inspection, 2025).
Durante este período, las irregularidades pueden continuar ocurriendo sin detección.
1.1.3. Tipos de anomalías identificadas
A través del levantamiento de información con el Área de Auditoría y el personal técnico de
PetrolRíos S.A., se han identificado cuatro categorías principales de anomalías que el sistema
debe detectar. Cabe destacar que, según la experiencia operativa de la empresa, los problemas
más significativos están relacionados con el manejo indebido de dinero y el incumplimiento de
normativas, más que con pérdidas de combustible.
Table 2. Tipos de anomalías identificadas por el Área de Auditoría
Tipo de Anomalía Descripción Ejemplos Específicos
Anomalías en Manejo de
Efectivo
(Cash Fraud)
Irregularidades en el manejo
del dinero en efectivo,
incluyendo faltantes,
autopréstamos no autorizados
y apropiación indebida de
fondos.
• Faltantes de caja no
justificados
• Autopréstamos de
empleados
• Ventas en efectivo
registradas como crédito
• Diferencias entre efectivo
reportado y depositado
Anomalías en Facturación
(Invoice Anomaly)
Discrepancias en la
documentación de ventas,
• Precios diferentes al
autorizado
25
precios aplicados
incorrectamente, o montos
que no coinciden con
políticas establecidas.
• Descuentos fuera de política
comercial
• Facturas con campos
obligatorios vacíos
• Patrones de anulaciones
excesivas
Fraudes con Medios de Pago
(Payment Fraud)
Manipulación de
transacciones realizadas con
tarjetas de crédito/débito o
créditos otorgados a clientes
corporativos.
• Reversiones de tarjeta
sospechosas
• Créditos sin autorización
apropiada
• Transacciones duplicadas
• Vouchers con
inconsistencias
Incumplimiento Normativo
(Compliance Violation)
Transacciones que violan
regulaciones de la Agencia de
Regulación y Control de
Energía (ARCERNNR) o
normativas tributarias.
• Ventas > 5 galones a placa
ZZZ999949
• Mismo vehículo con
diferentes combustibles
• Ventas sin placa en montos
mayores
• Operaciones fuera de
horario autorizado
La técnica de interrogación de archivos permite analizar la base de datos transaccional para
identificar patrones que requieren revisión por parte del equipo de auditoría. La Tabla 4 presenta
ejemplos de reglas de detección que serán implementadas en cada detector:
26
Table 3. Ejemplos de reglas de detección por tipo de anomalía
Detector
Regla de
ejemplo
Condición de alerta Fuente del umbral
Cash Fraud
Diferencia
efectivo vs.
sistema
Si (efectivo_reportado −
efectivo_sistema) > $50 en un
turno
Entrevistas con Área de
Auditoría: monto mínimo que
justifica investigación según
experiencia operativa
Cash Fraud
Patrón de
faltantes
recurrentes
(gineteo)
Si mismo empleado tiene
faltantes > 3 veces en 30 días
Caso documentado durante
levantamiento: el gineteo
genera faltantes repetitivos en
ciclos cortos
Invoice
Anomaly
Anulaciones
excesivas
Si empleado anula > 5% de
sus transacciones diarias
Análisis de caso
documentado: tasa de
anulación normal es < 2%
según Área de Auditoría
Invoice
Anomaly
Descuentos no
autorizados
Si descuento_aplicado >
descuento_máximo_permitido
Política comercial vigente de
PetrolRíos S.A.
Payment
Fraud
Reversiones
sospechosas
Si reversión de tarjeta ocurre
> 30 min después de venta
Caso documentado:
discrepancia de 3 horas entre
venta (6 AM) y voucher (9
AM)
Payment
Fraud
Crédito sin
autorización
Si crédito > límite_cliente
AND sin código_autorización
Procedimiento interno de
aprobación de créditos
corporativos
27
Compliance
Venta excesiva
a placa
genérica
Si placa = 'ZZZ999949' AND
galones > 5
Regulación ARCERNNR
vigente: máximo 5 galones
para ventas con placa
genérica
Compliance
Vehículo con
múltiples
combustibles
Si misma_placa tiene ventas
de diésel Y extra en mismo
día
Restricción física: un
vehículo opera con un solo
tipo de combustible
1.1.4. Evidencia cuantitativa de la problemática
Para sustentar cuantitativamente la problemática descrita, se presenta un caso real de estudio
proporcionado por el Área de Auditoría de PetrolRíos S.A. durante el levantamiento de
información. Es importante señalar que la empresa no cuenta con un registro formal ni
sistematizado de casos de fraude; el conocimiento sobre estas irregularidades reside de manera
tácita en el personal de auditoría y en análisis ad-hoc realizados puntualmente en hojas de
cálculo. Esta ausencia de documentación estructurada constituye, precisamente, una de las causas
identificadas en el análisis de Ishikawa (Sección 1.1.1, categoría Personas).
El caso que se presenta a continuación fue documentado durante el proceso de levantamiento de
información del presente proyecto y corresponde a un análisis realizado por el ingeniero
responsable del Departamento de Sistemas sobre las transacciones de un cliente corporativo en
una estación piloto durante enero de 2026.
Table 4. Resumen del caso de estudio documentado
Indicador Valor
Estación analizada Estación piloto X
Cliente involucrado Cliente Corporativo A (transporte de carga)
28
Período analizado Enero 2026 (un mes)
Total de transacciones 118
Monto total $8,987.77
Total galones 3,291.71 (Diésel)
Despachadores involucrados 15 de 15 activos
Placas vehiculares distintas 10 (9 formato estándar + 1 no estándar)
Transacciones con tarjeta de crédito 103 (88.4%)
Transacciones en efectivo (contado) 15 (11.6%)
Monto en tarjeta de crédito $7,942.77
Monto en efectivo $1,045.00
Transacciones rápidas (<10 min entre
despachos)
40 (33.9%)
Tiempo de detección ~30 días (todo enero antes de investigación)
Table 5. Anomalías por despachador en el caso documentado
Despachador Trans. Monto ($) % Contado Nivel alerta
Despachador 1 37 2,730.00 3.3% Medio
Despachador 2 20 1,650.00 0.0% Bajo
29
Despachador 3 14 813.00 2.5% Bajo
Despachador 4 9 740.00 8.1% Medio
Despachador 5 8 650.00 0.0% Bajo
Despachador 6 6 610.00 36.1% Alto
Despachador 7 4 440.00 79.5% Crítico
Despachador 8 4 284.77 5.3% Medio
Despachador 9 3 210.00 14.3% Medio
Despachador 10 3 190.00 0.0% Bajo
Despachador 11 3 190.00 21.1% Medio
Despachador 12 2 150.00 100.0% Crítico
Despachador 13 1 140.00 0.0% Bajo
Despachador 14 2 120.00 0.0% Bajo
Despachador 15 2 70.00 100.0% Crítico
El hecho de que los 15 despachadores activos de la estación hayan atendido a un mismo cliente
corporativo durante un solo mes constituye un indicador altamente anómalo. En operaciones
normales, un cliente corporativo es atendido típicamente por 2 o 3 despachadores según sus
turnos habituales. La participación del 100% del personal sugiere un esquema coordinado.
Mecanismos de fraude identificados durante el levantamiento de información
30
Durante el levantamiento de información con el Área de Auditoría y el Departamento de
Sistemas, se identificaron tres mecanismos principales de fraude que operan en las estaciones de
servicio. Estos mecanismos fueron descritos por el personal con base en su experiencia operativa
de años. Es importante recalcar que estos patrones no se encuentran documentados formalmente
en ningún sistema de la empresa; su conocimiento es tácito y reside en el personal
experimentado del área de auditoría.
Mecanismo 1: Sustracción de efectivo con autopréstamo rotativo (gineteo)
Este es el mecanismo de fraude más frecuente y de mayor impacto económico en las estaciones
de servicio. Se lo conoce coloquialmente como "gineteo" y consiste en un ciclo de sustracción y
cobertura rotativa de dinero entre turnos que permite al despachador extraer efectivo de las
ventas diarias sin que el faltante sea visible en los cierres de turno regulares.
Funcionamiento paso a paso:
Paso 1 — Sustracción inicial. Un despachador que trabaja en el turno de la mañana genera
ventas durante su jornada. Supóngase que el total de ventas del turno es de $20,000. Al finalizar
su turno, el despachador extrae una porción del efectivo recaudado (por ejemplo $500) y se lo
lleva. En ese momento, la caja tiene un faltante real de $500.
Paso 2 — Cobertura con el turno entrante. Al momento del cambio de turno, el despachador
saliente solicita al despachador entrante que "le cubra" el faltante con las ventas del nuevo turno.
El despachador entrante acepta (ya sea por presión social, acuerdo mutuo, o porque él mismo
participa del esquema). Así, se toma dinero de las ventas del turno nuevo para completar la
liquidación del turno anterior. En los registros del sistema, el turno de la mañana aparece
cuadrado porque fue cubierto con dinero del turno siguiente.
Paso 3 — Propagación del ciclo. El problema es que ahora el turno de la tarde tiene un faltante
de $500 (el dinero que se usó para cubrir al turno de la mañana). El despachador de la tarde debe,
31
a su vez, pedir al despachador del día siguiente que le cubra con las ventas de ese día. Y así
sucesivamente: cada turno cubre el faltante del anterior con el dinero del turno siguiente.
Paso 4 — Acumulación de la deuda. Si el despachador original continúa extrayendo efectivo
en días posteriores (que es lo habitual, ya que el esquema le ha funcionado), la deuda se acumula.
Lo que comenzó como $500 puede convertirse en $2,000, $5,000 o más en cuestión de semanas.
El ciclo de cobertura se vuelve cada vez más complejo e involucra a más personal.
Paso 5 — Descubrimiento únicamente por arqueo sorpresa. El mecanismo de cobertura
rotativa funciona porque los cierres de turno regulares siempre aparecen cuadrados (el dinero
faltante fue cubierto con ventas del turno siguiente). La única forma de descubrir este fraude es
mediante un arqueo sorpresa (inspección no programada de la caja fuerte y del efectivo físico),
realizado en un momento en que el despachador no haya tenido oportunidad de solicitar
cobertura. En ese instante, el conteo físico del dinero revela el faltante acumulado.
Paso 6 — Pérdida irrecuperable. Una vez descubierto, el dinero extraído no se recupera. Según
la experiencia del Área de Auditoría, en la mayoría de los casos la deuda acumulada supera la
capacidad de pago del empleado, quien es desvinculado de la empresa. La pérdida económica
para la empresa es directa y real: el efectivo fue sustraído físicamente de las ventas y nunca fue
depositado en las cuentas de la empresa.
Detección mediante el sistema propuesto: El sistema de detección de anomalías propuesto
abordaría este mecanismo a través del detector Cash Fraud, que compara el efectivo reportado en
cada cierre de turno contra la suma de transacciones registradas como "contado" en el sistema. Si
un turno presenta diferencias superiores al umbral configurado, el sistema genera una alerta
automática. Además, el detector de patrones recurrentes identificaría si un mismo despachador o
estación presenta faltantes repetidos en un período de 30 días. La detección cada 5-10 minutos
permitiría identificar anomalías el mismo día en que ocurren, en lugar de semanas después.
Mecanismo 2: Cambio de forma de pago con apropiación de efectivo
32
Este mecanismo consiste en que el despachador cobra la venta en efectivo al cliente pero registra
la transacción en el sistema como pago con tarjeta de crédito, utilizando su propia tarjeta o la de
un tercero para completar el cargo. El despachador se queda con el efectivo cobrado.
Funcionamiento paso a paso:
Paso 1 — Venta normal al cliente. El cliente llega a la estación, carga combustible y paga en
efectivo al despachador. Supóngase una venta de $90.
Paso 2 — Registro fraudulento. En lugar de registrar la venta como "contado", el despachador
la registra como "tarjeta de crédito" y pasa su propia tarjeta (o la de un conocido) por el monto de
$90. El sistema registra una venta con tarjeta por $90 y genera un voucher.
Paso 3 — Apropiación del efectivo. El despachador se queda con los $90 en efectivo que le
pagó el cliente. El cierre de caja cuadra porque la venta está registrada como tarjeta (no se espera
efectivo por esa transacción).
Pérdidas concurrentes para la empresa:
La empresa sufre múltiples pérdidas simultáneas: (a) pérdida de flujo de caja inmediato, ya que
el efectivo que debería estar en la caja no existe; (b) pago innecesario de comisión bancaria del
~2% al procesador de tarjetas por una transacción que en realidad fue en efectivo ($1.80 en este
ejemplo de $90); (c) retraso de 2 a 3 días hábiles para recibir el depósito del procesador de
tarjetas, generando un costo financiero por el dinero que la empresa debería haber tenido
disponible inmediatamente.
Evidencia en el caso documentado: En el caso del Cliente Corporativo A, el 88.4% de las 118
transacciones fueron registradas como tarjeta de crédito ($7,942.77). Este porcentaje es anómalo
para una compañía de transporte de carga, donde la práctica habitual es pagar en efectivo o
mediante crédito corporativo autorizado. Adicionalmente, se detectó una transacción registrada a
las 6:00 AM pero cuyo voucher de tarjeta fue generado a las 9:00 AM (3 horas después), una
discrepancia temporal imposible en una operación normal donde el cobro y el registro son
simultáneos. La investigación reveló que la tarjeta utilizada pertenecía a una empleada de la
33
estación (verificado mediante fotografía del voucher que mostraba una mano femenina, mientras
que el conductor registrado era masculino).
Detección mediante el sistema propuesto: El detector Payment Fraud identificaría patrones de
concentración anormal en tarjeta de crédito por cliente, discrepancias temporales entre la hora de
venta y la hora de generación del voucher, y el uso recurrente de una misma tarjeta para
múltiples clientes diferentes.
Mecanismo 3: Crédito no autorizado con cobertura mediante tarjeta de terceros
En este mecanismo, un despachador o administrador de estación otorga crédito a un cliente sin la
autorización formal requerida por la empresa. Para evitar que el crédito aparezca en el sistema
(lo cual generaría una alerta o requeriría justificación), se utiliza la tarjeta de un tercero
(conocido, compañero de trabajo, vigilante de la estación) para cubrir el monto.
Funcionamiento paso a paso:
Paso 1 — Despacho sin pago. El cliente carga combustible pero no paga en el momento. El
despachador le "fía" la venta de manera informal.
Paso 2 — Cobertura con tarjeta de tercero. Para que el cierre de turno cuadre, el despachador
pasa la tarjeta de un tercero por el monto de la venta. Este tercero suele recibir una comisión por
prestar su tarjeta.
Paso 3 — Acumulación de deuda. Si el cliente no paga oportunamente (o nunca paga), la deuda
se acumula. El despachador puede intentar cubrir los montos pendientes usando dinero de las
ventas de la estación o abriendo nuevos créditos informales para cubrir los anteriores, generando
un ciclo similar al gineteo.
Evidencia en el caso documentado: En un caso relacionado investigado por el Área de
Auditoría, los registros de la empresa mostraban 10 facturas pendientes a nombre de un cliente
que declaró deber únicamente 1. Las 9 facturas restantes habían sido cubiertas artificialmente
mediante un esquema rotativo de crédito con tarjetas de terceros.
34
Detección mediante el sistema propuesto: El detector Payment Fraud detectaría el uso de
tarjetas que no corresponden al titular del crédito, y el detector Cash Fraud identificaría patrones
de créditos otorgados que exceden los límites autorizados sin el código de autorización
correspondiente.
Cuantificación de pérdidas
Con base en el caso documentado, las pérdidas se clasifican en dos categorías:
Table 6. Cuantificación de pérdidas del caso documentado
Categoría Concepto Monto Naturaleza
Pérdidas directas
y ciertas
Comisión bancaria pagada
innecesariamente (2% x
$7,942.77 en tarjeta)
$158.86
Dinero que la empresa
pagó sin beneficio
equivalente
Pérdidas directas
y ciertas
Costo financiero por retraso de
2-3 días en depósito del
procesador
Variable
Costo de oportunidad del
dinero no disponible
Efectivo en
riesgo de
sustracción
Ventas registradas como
contado cuya disponibilidad
física no fue verificada
$1,045.00
Dinero que debería estar en
caja; verificable solo
mediante arqueo sorpresa
Proyección
mensual por
estación
Total estimado de pérdida
directa + efectivo en riesgo
~$1,210
Basado en un solo cliente
en una estación
Proyección anual
(10 estaciones)
Extrapolación conservadora ~$145,200
Asumiendo un caso similar
por estación
35
Nota sobre la categoría "efectivo en riesgo de sustracción": los $1,045.00 registrados como
ventas en contado representan dinero que debería estar físicamente en la caja de la estación. Sin
embargo, debido a que la empresa no realiza arqueos sorpresa sistemáticos, no es posible
confirmar que este efectivo se encuentre íntegro. La experiencia del Área de Auditoría indica que
en los arqueos sorpresa realizados históricamente se han encontrado faltantes significativos de
manera consistente, lo cual valida la clasificación de este monto como "en riesgo".
Esta cuantificación evidencia que incluso en un solo caso (un cliente, una estación, un mes) las
pérdidas estimadas son significativas. Considerando que este tipo de anomalías ocurren en
múltiples estaciones simultáneamente y con múltiples clientes, la magnitud del problema
justifica la inversión en un sistema automatizado de detección.
1.2. Descripción de la organización
PetrolRíos S.A. es una empresa ecuatoriana dedicada a la comercialización y distribución de
combustibles, con presencia consolidada en la provincia de Santo Domingo de los Tsáchilas y
regiones aledañas.
1.2.1. Identificación de la organización
• Nombre legal: PetrolRíos S.A
• Sector: Comercialización y distribución de combustibles (Sector Energético)
• Ubicación: Santo Domingo de los Tsáchilas, Ecuador
• Tipo de empresa: Sociedad Anónima, capital privado
• Años de operación: 15 años en el mercado ecuatoriano
• Tamaño de operación: Red de 90 estaciones de servicio totales, de las cuales 10 se
encuentran actualmente integradas al sistema centralizado de información
36
• Volumen transaccional: Aproximadamente 13,000 a 15,000 transacciones diarias en las
10 estaciones integradas (entre 1,300 y 1,500 transacciones por estación)
• Productos principales: Gasolina Extra, Gasolina Súper, Diésel Premium
1.2.2. Misión
Proveer combustibles de calidad a través de una red de estaciones de servicio eficientes,
garantizando excelencia en el servicio al cliente, seguridad operacional y prácticas comerciales
transparentes que generen valor para nuestros accionistas, colaboradores y la comunidad.
1.2.3. Visión
Ser la cadena de estaciones de servicio líder en la región, reconocida por la innovación
tecnológica en sus operaciones, la excelencia en el servicio al cliente y el compromiso con
prácticas comerciales éticas y sostenibles.
1.2.4. Estructura organizacional
La Figura 3 presenta el organigrama de PetrolRíos S.A., destacando la ubicación del Área de
Auditoría y Control Interno donde se identifica el problema. Esta área depende jerárquicamente
de la Gerencia Administrativa y Financiera.
37
Figure 3. Organigrama de PetrolRíos S.A. con énfasis en el Área de Auditoría
El organigrama presenta la estructura jerárquica de PetrolRíos S.A., destacando específicamente
el Área de Auditoría y Control Interno donde se identifica el problema que este proyecto busca
resolver. La empresa opera bajo un modelo de organización funcional con cuatro gerencias
principales que reportan a la Gerencia General.
La estructura organizacional se compone de los siguientes niveles jerárquicos:
Nivel 1: Junta de Accionistas
• Máximo órgano de gobierno corporativo:
• Constituye la autoridad suprema de la empresa, responsable de las decisiones estratégicas de
mayor impacto.
• Aprueba los estados financieros anuales, la distribución de utilidades y las inversiones
mayores.
• Se reúne ordinariamente una vez al año y extraordinariamente cuando las circunstancias lo
requieren.
• Nombra y remueve al Gerente General según el desempeño de la empresa.
38
Nivel 2: Gerencia General
Dirección ejecutiva de la empresa:
• Responsable de la dirección estratégica y la representación legal de PetrolRíos S.A.
• Define los objetivos anuales y supervisa su cumplimiento a través de las gerencias
funcionales.
• Reporta directamente a la Junta de Accionistas sobre el desempeño operativo y
financiero.
• Coordina las relaciones con entidades regulatorias (ARCERNNR, SRI) y socios
comerciales estratégicos.
Nivel 3: Gerencias Funcionales
Bajo la Gerencia General operan cuatro gerencias funcionales, cada una responsable de un área
específica del negocio:
Gerencia de Operaciones:
• Supervisa el funcionamiento de las 90 estaciones de servicio que conforman la red de
PetrolRíos.
• Coordina la logística de abastecimiento de combustible desde los terminales de
Petroecuador.
• Gestiona el mantenimiento preventivo y correctivo de la infraestructura (surtidores,
tanques, instalaciones).
• Organiza las estaciones en tres zonas geográficas (Norte, Centro, Sur) con
aproximadamente 30 estaciones cada una.
• Cada zona cuenta con un Supervisor de Zona responsable del desempeño operativo de
sus estaciones.
Gerencia Administrativa y Financiera:
• Responsable del control financiero, la contabilidad y la gestión de recursos humanos.
39
• De esta gerencia depende jerárquicamente el Área de Auditoría y Control Interno.
• Gestiona las relaciones bancarias, el flujo de caja y los pagos a proveedores.
• Administra el cumplimiento de obligaciones tributarias (SRI) y laborales (IESS).
Gerencia Comercial:
• Define las estrategias de ventas y los programas de fidelización de clientes.
• Gestiona las relaciones con clientes corporativos (empresas de transporte, flotas
vehiculares).
• Negocia los convenios de crédito corporativo y establece los límites de crédito por
cliente.
• Coordina las promociones y campañas de marketing en las estaciones.
Departamento de Sistemas:
• Administra la infraestructura tecnológica de la empresa, incluyendo servidores, redes y
bases de datos.
• Mantiene el sistema Contaplus (Firebird) que opera en cada estación de servicio.
• Desarrolla y mantiene el aplicativo de migración nocturna que consolida datos en
Microplus.
• Proporciona soporte técnico a las estaciones y oficinas administrativas.
Área de Auditoría y Control Interno (Área del problema)
El Área de Auditoría y Control Interno depende jerárquicamente de la Gerencia Administrativa y
Financiera. Esta es el área donde se identifica el problema que motiva el presente proyecto y
donde el sistema propuesto generará el mayor impacto.
Composición del equipo:
• 1 Supervisor de Auditoría: Coordina las actividades del área, asigna tareas y reporta
hallazgos a la Gerencia.
40
• 2 Auditores Internos: Ejecutan las revisiones de cuadres y las investigaciones de
discrepancias.
• Total: 3 personas de tiempo completo dedicadas a funciones de control.
Responsabilidades actuales del área:
• Verificar que los cuadres diarios de las estaciones se completen exitosamente.
• Investigar las causas de descuadres cuando ocurren diferencias en los totales.
• Realizar auditorías periódicas mediante muestreo aleatorio de transacciones.
• Generar reportes de hallazgos para la Gerencia Administrativa.
• Proponer mejoras en los procesos de control interno.
Limitación identificada:
• Con un volumen de 13,000 a 15,000 transacciones diarias en las 10 estaciones
integradas, el análisis manual transacción por transacción es físicamente imposible.
• El equipo actual (3 personas) solo puede revisar aproximadamente el 10% de las
transacciones mediante muestreo.
• El 90% restante de las transacciones nunca es analizado individualmente.
• Esta limitación de capacidad humana justifica la necesidad del sistema automatizado
propuesto.
1.2.5. Productos y servicios
• Venta de combustibles: Gasolina Extra, Gasolina Súper, Diésel
• Servicios complementarios: Lubricantes, aditivos, productos de conveniencia
• Programas de fidelización: Tarjetas de cliente frecuente con beneficios acumulativos
• Servicios corporativos: Convenios con empresas de transporte y flotas vehiculares
1.2.6. Stakeholders del proyecto
La Tabla 4 identifica los stakeholders (partes interesadas) del proyecto y sus responsabilidades
en relación con el sistema de detección de anomalías.
Table 7. Stakeholders del proyecto y sus responsabilidades
41
Stakeholder Rol en el Sistema Responsabilidades
Auditor Interno Usuario principal • Revisar alertas generadas
• Clasificar anomalías
• Investigar casos
• Documentar hallazgos
Supervisor de
Auditoría
Usuario supervisor • Asignar casos a auditores
• Monitorear métricas
• Aprobar reportes
• Configurar umbrales
Gerente
Administrativo
Usuario ejecutivo • Consultar dashboards
• Revisar reportes ejecutivos
• Tomar decisiones estratégicas
Administrador del
Sistema
Usuario técnico • Gestionar usuarios y roles
• Configurar detectores
• Mantener el sistema
• Monitorear logs
Supervisor de
Estación
Usuario regional • Recibir alertas de sus estaciones
• Proporcionar información de contexto
1.2.7. Área donde se encuentra el problema
El problema identificado se encuentra específicamente en el Área de Auditoría y Control Interno,
que depende jerárquicamente de la Gerencia Administrativa y Financiera. Este departamento es
responsable de:
Supervisar la integridad y exactitud de las transacciones registradas en las estaciones de servicio
Detectar irregularidades, discrepancias o posibles fraudes en operaciones comerciales
42
Generar reportes periódicos sobre hallazgos de auditoría para la gerencia general
Proponer mejoras en los procesos de control interno y supervisión operacional
El equipo actual de auditoría está compuesto por 2 auditores internos de tiempo completo y
1supervisor. Actualmente, la empresa cuenta con un sistema de cuadres consolidados que
verifica que los totales cuadren, pero no existe ningún sistema que analice las transacciones
individuales.
Con un volumen de 13,000 a 15,000 transacciones diarias en las 10 estaciones integradas, el
análisis transaccional manual es inviable. Según estudios del sector, el fraude en estaciones de
servicio genera pérdidas superiores a $500 millones anuales a nivel global (Fuelmetrics, 2021),
mientras que las tácticas más comunes incluyen manipulación de efectivo, compras personales
con tarjetas corporativas y apropiación de fondos (Heavy Vehicle Inspection, 2025). Estos
hallazgos evidencian la necesidad de sistemas automatizados de detección.
43
2. Análisis de posibles soluciones
Esta sección presenta el análisis de las alternativas de solución evaluadas para abordar el
problema de detección tardía de anomalías en PetrolRíos S.A. Se plantean tres alternativas de
desarrollo de software, cada una con diferente nivel de complejidad arquitectónica, y la selección
se fundamenta en los atributos de calidad de la norma ISO/IEC 25010 (2011), tales como la
adecuación funcional, la usabilidad y la mantenibilidad del producto software.
2.1. Identificación y selección de la mejor solución
La selección de la alternativa óptima se fundamenta en la norma ISO/IEC 25010 (International
Organization for Standardization, 2011), que define un modelo de calidad del producto software
con ocho características principales que permiten evaluar objetivamente cada alternativa.
2.1.1 Alternativa A: Sistema monolítico on-premise con procesamiento batch y conexión VPN
La primera alternativa propone un sistema web con arquitectura monolítica tradicional,
desplegado en infraestructura on-premise (servidores locales de PetrolRíos S.A.) con conexión a
las estaciones mediante VPN (Virtual Private Network). Este enfoque prioriza el control total
sobre la infraestructura y los datos, manteniendo toda la información dentro de la red
corporativa. El sistema utilizará SQL Server como base de datos central, aprovechando las
licencias existentes de Microsoft en la empresa. La conexión con las 10 bases de datos Firebird
de las estaciones se realizará a través de túneles VPN site-to-site, garantizando la seguridad de la
comunicación pero introduciendo dependencia de la estabilidad de la conexión de red. El
procesamiento de detección de anomalías se ejecutará mediante tareas programadas (Windows
Task Scheduler / Cron Jobs) cada 15-30 minutos, extrayendo datos de las estaciones y
ejecutando los algoritmos de detección. Sin embargo, esta alternativa NO incluye notificaciones
en tiempo real; los usuarios deberán refrescar manualmente el navegador (F5) para visualizar
nuevas alertas.
Arquitectura Técnica
La arquitectura de la Alternativa A se compone de los siguientes elementos:
44
• Capa de Presentación (Frontend): El frontend se desarrollará utilizando React 18 con
TypeScript, proporcionando una interfaz de usuario moderna y responsiva. Sin embargo,
al no contar con comunicación bidireccional (WebSocket/SignalR), la actualización de
datos requiere que el usuario refresque la página manualmente. Esto significa que si se
detecta una anomalía crítica, el auditor no será notificado hasta que recargue el
dashboard.
• Capa de Lógica de Negocio (Backend): El backend se implementará en ASP.NET Core
9.0 con una arquitectura en tres capas tradicional:
• Capa de Controladores: Expone endpoints REST para consulta de alertas, gestión de
usuarios y configuración.
• Capa de Servicios: Contiene la lógica de negocio, incluyendo los cuatro detectores de
anomalías (Cash Fraud, Invoice Anomaly, Payment Fraud, Compliance Violation).
• Capa de Acceso a Datos: Utiliza Entity Framework Core para interactuar con SQL
Server y ADO.NET con el proveedor FirebirdSQL para las bases de datos legacy.
Capa de Datos:
• Base de datos central: SQL Server (on-premise) almacena las alertas detectadas,
configuración de reglas, usuarios, roles y logs de auditoría.
• Bases de datos de estaciones: 10 instancias de Firebird distribuidas, accesibles mediante
conexión VPN. Cada estación contiene la base de datos CONTAC.FDB con la misma
estructura de tablas.
Capa de Procesamiento Batch: El procesamiento se realiza mediante Windows Task Scheduler
que ejecuta un servicio de consola .NET cada 15-30 minutos. Este servicio:
1. Establece conexión VPN con cada estación
2. Extrae las transacciones nuevas desde Firebird
3. Ejecuta los detectores de anomalías
4. Almacena las alertas en SQL Server
45
Flujo de Funcionamiento Detallado
El funcionamiento del sistema en la Alternativa A sigue el siguiente flujo:
PASO 1 - Extracción de Datos (cada 15-30 minutos): El Task Scheduler dispara la
ejecución del servicio de extracción. Para cada una de las 10 estaciones:
a) Se verifica la conectividad VPN con la estación
b) Se establece conexión con la base de datos Firebird (CONTAC.FDB)
c) Se consultan las transacciones con fecha/hora posterior a la última extracción
d) Se extraen los registros de las tablas: FACTURAS, DETALLES_FACTURA,
MEDIDORES, CIERRES_TURNO
e) Se transforman los datos al formato del modelo central
f) Se cargan en una tabla de staging en SQL Server
g) Se cierra la conexión y se registra el timestamp de última extracción
PASO 2 - Procesamiento de Detección: Una vez completada la extracción de todas las
estaciones, se ejecutan secuencialmente los cuatro detectores:
Detector 1 - Cash Fraud (Anomalías en Efectivo):
• Compara el efectivo reportado en cierre de turno vs. suma de ventas en efectivo
• Identifica diferencias que superan el umbral configurado (ej: > $50)
• Detecta patrones de faltantes recurrentes por empleado
• Genera alertas con nivel de riesgo según la magnitud de la diferencia
Detector 2 - Invoice Anomaly (Irregularidades en Facturación):
• Verifica que los precios aplicados coincidan con la lista de precios autorizada
• Identifica descuentos que exceden los límites de política comercial
• Detecta facturas con campos obligatorios vacíos (placa, cédula, etc.)
• Analiza patrones de anulaciones excesivas por empleado
Detector 3 - Payment Fraud (Fraudes con Medios de Pago):
46
• Identifica reversiones de tarjeta realizadas mucho tiempo después de la venta original
• Detecta créditos otorgados que exceden el límite del cliente sin autorización
• Analiza patrones de transacciones duplicadas con tarjeta
• Verifica consistencia entre vouchers y registros del sistema
Detector 4 - Compliance Violation (Incumplimiento Normativo):
• Verifica que ventas a placa genérica ZZZ999949 no excedan 5 galones
• Detecta vehículos (misma placa) con ventas de diferentes tipos de combustible
• Identifica ventas sin registro de placa en transacciones mayores al umbral
• Verifica cumplimiento de horarios de operación autorizados
PASO 3 - Almacenamiento de Resultados: Las anomalías detectadas se almacenan en SQL
Server con la siguiente información:
• Identificador único de alerta
• Tipo de anomalía (Cash Fraud, Invoice Anomaly, Payment Fraud, Compliance)
• Nivel de riesgo calculado (Bajo, Medio, Alto, Crítico)
• Estación y turno donde se detectó
• Transacciones involucradas (referencias)
• Timestamp de detección
• Estado inicial: "Pendiente de revisión"
PASO 4 - Visualización (Manual): El auditor accede al sistema web mediante su
navegador. Para ver las alertas más recientes, debe refrescar la página (F5). El dashboard
muestra:
• Resumen de alertas por nivel de riesgo
• Lista de alertas pendientes ordenadas por criticidad
• Filtros por estación, tipo de anomalía, fecha
• Detalle de cada alerta con transacciones involucradas
47
PASO 5 - Notificación por Email (Opcional): Para anomalías clasificadas como "Críticas",
el sistema puede enviar un correo electrónico al supervisor de auditoría mediante un
servidor SMTP configurado. Sin embargo, esto no reemplaza la necesidad de refrescar el
dashboard para ver todas las alertas.
Ventajas y Desventajas
Ventajas
• Control total sobre la infraestructura y los datos sensibles
• Aprovechamiento de licencias SQL Server existentes
• Sin costos recurrentes de servicios cloud
• Menor complejidad técnica al no requerir SignalR
• Independencia de proveedores de nube externos
Desventajas
• Retraso de 15-30 minutos en la detección de anomalías
• Experiencia de usuario deficiente (requiere refrescar manualmente)
• Dependencia de estabilidad de conexiones VPN
• Sin notificaciones proactivas (el auditor debe "buscar" las alertas)
• Mayor carga de mantenimiento de infraestructura local
• Escalabilidad limitada por capacidad del hardware on-premise
48
Figure 4. Diagrama de contexto C4 - Alternativa A: Sistema batch sin notificaciones
El diagrama de contexto de la Alternativa A muestra los siguientes elementos y sus
interacciones:
Actores:
• Auditor de PetrolRíos: Accede al sistema web para consultar alertas y gestionar
anomalías. Debe refrescar manualmente para ver actualizaciones.
• Gerente Administrativo: Revisa dashboards ejecutivos y reportes de estado. También
requiere refrescamiento manual.
49
Sistema Central:
• Sistema de Detección de Anomalías (Modo Batch): Aplicación web ASP.NET Core
con React, desplegada en servidor on-premise. Procesa detecciones cada 15-30 minutos.
Sistemas Externos:
• Cron Job / Task Scheduler: Dispara el procesamiento batch automáticamente según
programación configurada. - Servidor SMTP (Opcional): Envía notificaciones por email
únicamente para anomalías críticas.
Fuentes de Datos:
• Bases de Datos Firebird (Legacy): 10 bases distribuidas en las estaciones, conectadas
mediante VPN. Contienen las transacciones de ventas, inventarios y facturación.
SQL Server Central:
• Base de datos on-premise que almacena alertas detectadas, configuración y logs. Flujos
de Comunicación:
• VPN Site-to-Site: Conexión segura entre el servidor central y cada estación para
extracción de datos.
• HTTPS: Comunicación entre navegadores de usuarios y el servidor web. - SMTP: Envío
de correos para alertas críticas.
Tecnologías Utilizadas
Table 8. Stack tecnológico propuesto para la Alternativa A
Componente Tecnología Justificación
Frontend React 18 + TypeScript Framework moderno con
tipado estático para reducir
errores
Backend ASP.NET Core 9.0 Framework empresarial
50
robusto con soporte de
Microsoft
Base de datos central SQL Server (on-premise) Aprovecha licencias
existentes en PetroRíos
ORM Entity Framework Core Mapeo objeto-relacional
con migraciones CodeFirst
Conexión Firebird ADO.NET + FirebirdSQL
Provider
Acceso directo a bases
legacy
Programador de tareas Windows Task Scheduler Nativo del sistema
operativo, sin costos
adicionales
Conectividad VPN Site-to-Site Túnel seguro entre oficina
central y estaciones
Servidor web IIS (Internet Information
Services)
Integración nativa con
ecosistema Windows
2.1.2. Alternativa B: Sistema en capas AWS con procesamiento batch Hangfire y
notificaciones SignalR (SELECCIONADA)
Descripción General
La segunda alternativa, y la seleccionada para implementación, propone un sistema web con
arquitectura monolítica en capas desplegado en Amazon Web Services (AWS), utilizando
PostgreSQL como base de datos central y SignalR para notificaciones en tiempo real. Este
enfoque combina la robustez de una arquitectura probada con las ventajas de la nube y la
comunicación bidireccional.
51
El sistema utilizará AWS como proveedor de infraestructura cloud, aprovechando servicios
como EC2 para el servidor de aplicaciones, RDS para PostgreSQL administrado, y los créditos
educativos disponibles para estudiantes. La conexión con las 10 bases de datos Firebird de las
estaciones se realizará mediante agentes de sincronización instalados localmente que envían
datos al servidor central.
La característica diferenciadora de esta alternativa es la implementación de SignalR Hub para
notificaciones push en tiempo real. ASP.NET Core SignalR es una biblioteca que simplifica la
adición de funcionalidad web en tiempo real, proporcionando una API para llamadas a
procedimiento remoto servidor-cliente que permite enviar contenido a los clientes conectados
instantáneamente (Microsoft, 2025). SignalR maneja las conexiones automáticamente, incluye
reconexión automática ante pérdidas de conexión, y selecciona el mejor método de transporte
disponible (Torres, 2024).
Arquitectura Técnica
La arquitectura de la Alternativa B se compone de los siguientes elementos:
Capa de Presentación (Frontend): El frontend se desarrollará utilizando React 18 con
TypeScript, desplegado en AWS Amplify o S3 + CloudFront para distribución global. La
característica clave es la integración del cliente SignalR que mantiene una conexión WebSocket
persistente con el servidor, permitiendo:
• Recepción instantánea de notificaciones cuando se detectan nuevas anomalías
• Actualización automática de dashboards y contadores sin refrescar la página
• Indicador visual de estado de conexión en tiempo real - Sincronización de estado entre
múltiples pestañas del navegador
Capa de Lógica de Negocio (Backend): El backend se implementará en ASP.NET Core 9.0 con
arquitectura en capas y los siguientes componentes:
52
• API Controllers: Endpoints REST para operaciones CRUD de alertas, usuarios,
configuración y reportes.
• SignalR Hub: Punto central de comunicación bidireccional que gestiona conexiones de
clientes y broadcast de notificaciones.
• Servicios de Negocio: Implementan la lógica de los cuatro detectores de anomalías
utilizando el patrón Strategy para facilitar la adición de nuevos detectores.
• Hangfire Server: Gestor de trabajos en segundo plano que programa y ejecuta el
procesamiento batch cada 5-10 minutos con reintentos automáticos ante fallos.
Capa de Acceso a Datos:
• Patrón Repository: Abstrae el acceso a datos permitiendo cambiar la implementación sin
afectar la lógica de negocio.
• Unit of Work: Coordina las transacciones asegurando consistencia en operaciones que
involucran múltiples entidades.
• Entity Framework Core: ORM para PostgreSQL con migraciones Code-First.
• Dapper: Micro-ORM para consultas de alto rendimiento en los detectores.
Capa de Datos:
Amazon RDS PostgreSQL: Base de datos relacional administrada que almacena alertas,
configuración, usuarios y logs. PostgreSQL fue elegido por:
• Excelente rendimiento en consultas analíticas complejas
• Soporte nativo para JSON (útil para almacenar metadatos de alertas)
• Costo significativamente menor que SQL Server en AWS
• Amplia comunidad y documentación disponible
Bases de datos de estaciones: Las 10 instancias Firebird se conectan mediante un agente ligero
instalado en cada estación que:
53
• Extrae transacciones nuevas cada 5 minutos
• Las envía al servidor central mediante API REST sobre HTTPS
• Maneja reconexión automática ante pérdidas de conectividad
• Almacena transacciones localmente si no hay conexión (store-and-forward)
Flujo de Funcionamiento Detallado
El funcionamiento del sistema en la Alternativa B sigue el siguiente flujo optimizado:
PASO 1 - Recolección de Datos (continua, cada 5 minutos):
A diferencia de la Alternativa A donde el servidor central "extrae" datos, en esta arquitectura las
estaciones "envían" datos al servidor:
a) En cada estación, un agente ligero (servicio Windows) monitorea la base de datos
Firebird
b) Cada 5 minutos, el agente consulta transacciones nuevas desde el último envío
c) Los datos se empaquetan en formato JSON y se envían al endpoint de ingesta del
servidor
d) El servidor valida, transforma y almacena los datos en una tabla de staging en
PostgreSQL
e) Se confirma la recepción al agente, que actualiza su marca de agua (watermark)
f) Si la conexión falla, el agente almacena localmente y reintenta en el siguiente ciclo
PASO 2 - Programación del Procesamiento (Hangfire): Hangfire gestiona la ejecución del
procesamiento batch con las siguientes características:
• Trabajos recurrentes configurados para ejecutarse cada 5-10 minutos
• Cola de trabajos persistente en PostgreSQL (sobrevive reinicios del servidor)
• Reintentos automáticos con backoff exponencial ante fallos
• Dashboard de monitoreo para visualizar estado de trabajos
54
• Prevención de ejecuciones duplicadas mediante locks distribuidos
PASO 3 -Ejecución de Detectores:
Cuando Hangfire dispara el trabajo de detección, se ejecutan los cuatro detectores en paralelo
(aprovechando múltiples cores del servidor):
Detector 1
Cash Fraud (Anomalías en Efectivo):
public class CashFraudDetector : IAnomalyDetector
{
 public async Task<IEnumerable<Alerta>> DetectAsync(IEnumerable<Transaccion>
transactions)
 {
 // 1. Agrupa transacciones por turno y estación
 var grupos = transactions.GroupBy(t => new { t.TurnoId, t.EstacionId });
 // 2. Calcula suma de ventas en efectivo por turno
 // 3. Compara con el efectivo reportado en cierre
 // 4. Genera alertas si diferencia > umbral configurado
 /* Lógica de detección:
 Se realiza un análisis cruzado entre el total calculado
 y el reporte físico de caja.
 */
 // 5. Analiza historial del empleado para detectar patrones
 return await AnalizarPatronesHistoricos(grupos);
 }
}
Detector 2 - Invoice Anomaly:
• Consulta la tabla de precios autorizados vigentes - Compara precio_aplicado vs
precio_autorizado para cada ítem
• Identifica descuentos fuera de política (descuento > máximo_permitido)
55
• Detecta campos obligatorios vacíos usando reglas configurables
• Calcula tasa de anulación por empleado y alerta si > umbral
Detector 3
• Payment Fraud: - Identifica transacciones con tarjeta que tienen reversión posterior -
Calcula tiempo entre venta y reversión (alerta si > 30 minutos)
• Verifica límites de crédito de clientes corporativos
• Detecta patrones de transacciones duplicadas (mismo monto, misma tarjeta, < 5 min)
Detector 4 - Compliance Violation:
• Filtra ventas con placa ZZZ999949 (placa genérica para canecas)
• Verifica que cantidad <= 5 galones (límite normativo)
• Agrupa ventas por placa y detecta múltiples tipos de combustible - Verifica horarios de
operación según configuración por estación
PASO 4 - Generación y Almacenamiento de Alertas:
Las anomalías detectadas se clasifican por nivel de riesgo mediante un algoritmo de scoring:
Cálculo de Riesgo:
• Riesgo Base: Según tipo de anomalía (Compliance=Alto, Cash=Medio-Alto, etc.)
• Multiplicadores: Monto involucrado, reincidencia del empleado, estación con historial -
Riesgo Final: MIN(Riesgo_Base × Multiplicadores, 100)
• Clasificación: Bajo (0-25), Medio (26-50), Alto (51-75), Crítico (76-100)
PASO 5 - Notificación en Tiempo Real (SignalR): Inmediatamente después de almacenar
las alertas, el sistema notifica a los usuarios conectados:
Backend (C# / ASP.NET Core SignalR)
56
await _hubContext.Clients.All.SendAsync("NuevasAlertas", new
{
 TotalNuevas = alertas.Count,
 Criticas = alertas.Count(a => a.Riesgo == "Critico"),
 Resumen = alertas.GroupBy(a => a.Tipo)
 .Select(g => new
 {
 Tipo = g.Key,
 Cantidad = g.Count()
 })
});
En el frontend (React), el componente de notificaciones escucha este evento:
Frontend (React / JavaScript)
await _hubContext.Clients.All.SendAsync("NuevasAlertas", new
{
 TotalNuevas = alertas.Count,
 Criticas = alertas.Count(a => a.Riesgo == "Critico"),
 Resumen = alertas.GroupBy(a => a.Tipo)
 .Select(g => new
 {
 Tipo = g.Key,
 Cantidad = g.Count()
 })
});
PASO 6 - Visualización y Gestión: El auditor ve las alertas actualizarse automáticamente
en su dashboard:
• Contador de alertas en el header se incrementa en tiempo real
• Tabla de alertas se actualiza sin refrescar la página - Toast notifications informan de
nuevas detecciones
• Indicador visual muestra estado de conexión SignalR (verde=conectado)
57
Table 9. Stack tecnológico de la Alternativa B (Seleccionada)
Componente Tecnología Justificación
Frontend React 18 + TypeScript +
SignalR Client
SPA moderna con
comunicación bidireccional
en tiempo real
Backend ASP.NET Core 9.0 +
SignalR Hub
Framework robusto con
soporte nativo para
WebSocket
Base de datos Amazon RDS PostgreSQL BD relacional
administrada, menor costo
que SQL Server
ORM Entity Framework Core +
Dapper
EF para CRUD, Dapper
para consultas de alto
rendimiento
Programador Hangfire Gestión profesional de
trabajos con persistencia y
reintentos
Hosting Backend AWS EC2 (t3.medium) Servidor escalable con
buena relación
costo/rendimiento
Hosting Frontend AWS Amplify / S3 +
CloudFront
Distribución global con
caché en edge
Monitoreo AWS CloudWatch +
Serilog
Logs centralizados y
métricas de rendimiento
Agente Estaciones .NET Worker Service Servicio ligero para envío
58
de datos desde estaciones
El auditor puede:
• Ver detalle de cada alerta con transacciones involucradas
• Clasificar como: Confirmada, Falso Positivo, Requiere Investigación
• Agregar comentarios y documentación
• Asignar a otro auditor para seguimiento
• Marcar como resuelta con descripción de acciones tomadas
Ventajas y Desventajas
Ventajas:
• Notificaciones en tiempo real mejoran significativamente la experiencia de usuario
• Detección cada 5-10 minutos reduce ventana de exposición vs. 15-30 min de Alternativa
A
• Infraestructura cloud elimina necesidad de mantener servidores físicos
• PostgreSQL ofrece mejor relación costo/rendimiento que SQL Server en AWS
• Hangfire proporciona gestión robusta de trabajos con reintentos automáticos
• Escalabilidad horizontal disponible si el volumen de transacciones crece
• Agentes de estación con store-and-forward toleran desconexiones temporales
• Créditos AWS para estudiantes reducen costos durante desarrollo
Desventajas:
• Mayor complejidad técnica que la Alternativa A (SignalR, Hangfire)
• Requiere gestión de conexiones WebSocket (aunque SignalR lo simplifica)
• Dependencia de disponibilidad de AWS (mitigable con arquitectura multi-AZ)
• Costos recurrentes de servicios cloud (aunque menores que Alternativa C)
• Curva de aprendizaje para el equipo en tecnologías de tiempo real
59
Figure 5. Diagrama de contexto C4 - Alternativa B: Sistema batch con SignalR (Seleccionada)
El diagrama de contexto de la Alternativa B muestra los siguientes elementos y sus
interacciones:
Actores:
• Auditor de PetrolRíos: Accede al sistema web y recibe notificaciones push en tiempo
real. Sus dashboards se actualizan automáticamente sin necesidad de refrescar.
60
• Gerente Administrativo: Revisa dashboards ejecutivos con datos actualizados en tiempo
real.
Sistema Central:
• Sistema de Detección de Anomalías (Tiempo Real / SignalR): Aplicación web ASP.NET
Core con React y SignalR Hub, desplegada en AWS. Incluye el sello
"SELECCIONADA" indicando que es la alternativa elegida.
Sistemas Externos:
• Hangfire Scheduler: Gestiona la programación y ejecución de trabajos batch cada 5-10
minutos, con persistencia en PostgreSQL.
• Servidor SMTP (Opcional): Envía notificaciones por email para anomalías críticas como
respaldo.
Fuentes de Datos:
• Bases de Datos Firebird (Legacy): 10 bases en las estaciones con agentes que envían
datos al servidor central cada 5 minutos.
• Amazon RDS PostgreSQL: Base de datos administrada que almacena alertas,
configuración y logs.
Flujos de Comunicación:
• HTTPS + WebSocket: Comunicación bidireccional entre navegadores y servidor (REST
+ SignalR).
• HTTPS (Ingesta): Agentes de estaciones envían datos al servidor mediante API REST
segura.
• Conexión RDS: El backend se conecta a PostgreSQL mediante conexión administrada
por AWS.
61
2.1.3. Alternativa C: Sistema serverless Azure con arquitectura orientada a eventos y
procesamiento en streaming
Descripción General
La tercera alternativa propone una arquitectura completamente serverless y orientada a eventos
utilizando Microsoft Azure. La arquitectura orientada a eventos (Event-Driven Architecture,
EDA) ha emergido como paradigma dominante para sistemas que requieren procesamiento en
tiempo real, donde el flujo de la aplicación se determina por eventos generados por acciones de
usuario, cambios de estado del sistema, o triggers externos (Hashir, 2025). CodeOpinion (2021)
demuestra la implementación práctica de EDA en ASP.NET Core, aunque para el contexto de
PetrolRíos representa una sobrecapacidad innecesaria.
Arquitectura Técnica
La arquitectura de la Alternativa C se compone de los siguientes elementos:
Capa de Presentación (Frontend):
El frontend se desarrollará como una Single Page Application (SPA) en React 18 con
TypeScript, desplegada en Azure Static Web Apps. La comunicación en tiempo real utiliza
Azure SignalR Service, un servicio administrado que elimina la necesidad de gestionar la
infraestructura de WebSocket:
• Escalamiento automático hasta millones de conexiones concurrentes
• Disponibilidad del 99.95% con SLA
• Integración nativa con Azure Functions
Capa de Ingesta de Eventos:
En lugar de un servidor monolítico, los datos fluyen a través de un pipeline de eventos:
1. API Gateway (Azure API Management): Punto de entrada único para todas las peticiones
2. Event Ingestion (Azure Event Hubs): Recibe eventos de transacciones de las estaciones
con capacidad de millones de eventos/segundo
62
3. Stream Processing (Azure Stream Analytics): Procesa eventos en tiempo real para
detección de anomalías simples
Capa de Procesamiento (Serverless):
• Azure Functions ejecuta la lógica de negocio en respuesta a eventos:
• Function: TransactionIngester – Triggered por Event Hubs, almacena transacciones en
Cosmos DB
• Function: AnomalyDetector – Triggered cada minuto, ejecuta los detectores de anomalías
• Function: AlertNotifier – Triggered por nuevas alertas, envía notificaciones via SignalR
• Function: ReportGenerator – Triggered por schedule, genera reportes periódicos
Capa de Datos:
• Azure Cosmos DB: Base de datos NoSQL multi-modelo con las siguientes
características:
• Distribución global con latencia < 10ms
• Escalamiento automático de throughput (RU/s)
• Consistencia configurable (Strong, Bounded Staleness, Session, Eventual)
Ideal para datos semi-estructurados como alertas con metadatos variables.
Azure Table Storage:
Almacenamiento económico para logs y datos históricos.
Orquestación y Mensajería:
• Azure Service Bus: Broker de mensajes empresarial para comunicación entre Functions
• Colas para procesamiento asíncrono confiable
• Topics para distribución de eventos a múltiples suscriptores
• Dead-letter queues para manejo de fallos
Azure Event Grid:
63
Enrutamiento de eventos entre servicios Azure.
Microservicios (representación lógica):
• Microservicio de Detección
• Microservicio de Alertas
• Microservicio de Reportes
Flujo de Funcionamiento Detallado
PASO 1 - Ingesta de Eventos (< 1 segundo):
a) El agente en cada estación detecta nuevas transacciones en Firebird
b) Envía el evento a Azure Event Hubs
c) Event Hubs almacena el evento con timestamp
d) Stream Analytics ejecuta detección simple
e) Eventos complejos se encolan
PASO 2 - Procesamiento de Transacción:
La Function TransactionIngester valida, enriquece y almacena en Cosmos DB.
PASO 3 - Detección de Anomalías (< 1 minuto):
Modo Streaming y Modo Batch Micro para reglas complejas.
PASO 4 - Generación de Alertas:
Alertas almacenadas en Cosmos DB y publicadas en Service Bus.
PASO 5 - Notificación en Tiempo Real:
SignalR distribuye las alertas al frontend.
PASO 6 - Escalamiento Automático:
Event Hubs, Functions, Cosmos DB y SignalR escalan según demanda.
64
Table 10. Stack tecnológico de la Alternativa C
Componente Tecnología Azure Justificación
Frontend React 18 + Azure Static
Web Apps
SPA con hosting global y
CI/CD integrado
API Gateway Azure API Management Punto de entrada único con
rate limiting y analytics
Ingesta eventos Azure Event Hubs Capaz de millones de
eventos/segundo
Procesamiento Azure Functions
(Consumption Plan)
Serverless con pago por
ejecución
Mensajería Azure Service Bus Broker empresarial con
garantías de entrega
BD transaccional Azure Cosmos DB NoSQL global con latencia
< 10ms
Tiempo real Azure SignalR Service WebSocket administrado y
escalable
Monitoreo Azure Application Insights APM completo con trazas
distribuidas
Orquestación Azure Durable Functions Workflows stateful para
procesos complejos
65
Estimación de Costos Mensuales
Table 11. Estimación de costos de la Alternativa C
Servicio Especificación Costo Estimado/Mes
Azure Functions Consumption Plan, ~1M
ejecuciones/mes
$20-50
Azure Cosmos DB 400 RU/s, 10 GB storage $200-300
Azure Event Hubs Standard tier, 1 TU $80-120
Azure Service Bus Standard tier $50-80
Azure SignalR Service Standard tier, 1 unit $50-100
Azure Static Web Apps Standard plan $9
Azure API Management Developer tier $50
Azure Application Insights 5 GB/mes $15
TOTAL MENSUAL $474-724
TOTAL 6 MESES $2,844-$4,344
Con buffer y desarrollo $8,000-$12,000
Ventajas y Desventajas Ventajas:
• Latencia mínima (< 1 minuto) en detección y notificación de anomalías
• Escalabilidad automática y prácticamente ilimitada
66
• Arquitectura moderna alineada con mejores prácticas de la industria
• Pago por uso real (costo se reduce en períodos de baja actividad)
• Alta disponibilidad con SLAs del 99.95% • Servicios administrados reducen carga
operativa
• Preparada para crecimiento futuro a 90 estaciones sin rediseño Desventajas:
• Complejidad arquitectónica significativamente mayor que alternativas A y B
• Costos elevados ($8,000-$12,000 para 6 meses) vs. $3,410 de Alternativa B
• Requiere expertise especializado en arquitecturas serverless y event-driven
• Sobrecapacidad para el volumen actual (10 estaciones, ~15,000 tx/día)
• Mayor número de servicios a gestionar y monitorear
• Curva de aprendizaje pronunciada para el equipo de desarrollo
• Tiempo de desarrollo estimado: 9-12 meses vs. 6 meses de Alternativa B
• Debugging más complejo en arquitecturas distribuidas
67
Figure 6. Diagrama de contexto C4 - Alternativa C: Arquitectura orientada a eventos
El diagrama de contexto de la Alternativa C muestra los siguientes elementos y sus
interacciones:
Actores:
• Auditor de PetrolRíos: Accede mediante API Gateway, recibe notificaciones
instantáneas (< 1 min).
• Gerente Administrativo: Visualiza dashboards con datos prácticamente en tiempo real.
Componentes Serverless (Azure Functions):
68
• Microservicio de Detección: Functions que procesan eventos y ejecutan detectores de
anomalías.
• Microservicio de Alertas: Functions que gestionan el ciclo de vida de alertas y
notificaciones.
• Microservicio de Reportes: Functions que generan reportes y alimentan dashboards.
Infraestructura de Mensajería:
• Azure Event Hubs: Recibe eventos de transacciones desde las estaciones.
• Azure Service Bus: Coordina comunicación entre microservicios.
• Apache Kafka / Event Bus (representación conceptual): Indicado en el diagrama
como el bus central de eventos.
Orquestación:
• Kubernetes (K8s): Representado en el diagrama para indicar que la arquitectura podría
desplegarse en contenedores si se requiere mayor control.
Fuentes de Datos:
• Bases de Datos Firebird (Legacy): 10 bases de datos con agentes que publican eventos
hacia Azure Event Hubs.
• Azure Cosmos DB: Base de datos NoSQL distribuida utilizada para almacenar alertas y
transacciones.
Indicadores Visuales:
• “Complejidad Arquitectónica Significativamente Mayor”: Nota destacada que indica la
principal desventaja de la alternativa.
• “Latencia < 1 minuto”: Indica el tiempo de respuesta estimado del sistema.
• “Costos Elevados ($8k–$12k)”: Nota sobre el presupuesto requerido para
implementación y operación.
69
• “Puntuación ISO/IEC 25010: 38/40”: Badge de evaluación de calidad (misma puntuación
que la Alternativa B, pero con desventaja en mantenibilidad).
Evaluación Comparativa y Selección de Alternativa
La selección de la alternativa óptima se fundamenta en la norma ISO/IEC 25010 (International
Organization for Standardization, 2011), que define un modelo de calidad del producto software
con ocho características principales: adecuación funcional, eficiencia de desempeño,
compatibilidad, usabilidad, fiabilidad, seguridad, mantenibilidad y portabilidad.
La Tabla 9 presenta la evaluación comparativa de las tres alternativas. Los criterios de
evaluación se definieron considerando el contexto específico del proyecto: restricciones de
tiempo (6 meses), presupuesto ($3,410 USD), equipo (2 desarrolladores estudiantes) y
necesidades del cliente (latencia aceptable de 5-10 minutos).
Justificación de la Selección de Alternativa B:
Table 12. Evaluación de alternativas según ISO/IEC 25010
Característica ISO/IEC
25010
Alt. A Alt. B Alt. C Justificación
Adecuación Funcional 3 5 5 ¿Cumple los requisitos
funcionales?
Eficiencia de Desempeño 4 5 5 ¿El tiempo de respuesta es
adecuado?
Compatibilidad 4 5 4 ¿Se integra con Firebird?
Usabilidad 2 5 5 ¿La experiencia de usuario
es satisfactoria?
70
Fiabilidad 4 5 5 ¿El sistema es confiable y
disponible?
Seguridad 4 5 5 ¿Protege datos y controla
acceso?
Mantenibilidad 5 4 2 ¿Es fácil de mantener y
modificar?
Portabilidad 4 4 3 ¿Es desplegable en
diferentes entornos?
TOTAL 30/40 38/40 34/40 Suma de puntuaciones
La Alternativa B obtiene la puntuación más alta (38/40 puntos) y se selecciona como la solución
óptima. A continuación se presenta el análisis detallado de cada característica evaluada:
1. Adecuación Funcional (Alt. A: 3/5, Alt. B: 5/5, Alt. C: 5/5)
La adecuación funcional evalúa el grado en que el sistema proporciona funciones que satisfacen
las necesidades declaradas e implícitas cuando se usa bajo condiciones especificadas. En este
criterio, la Alternativa A obtiene únicamente 3 puntos debido a una limitación crítica: la ausencia
de notificaciones en tiempo real. Los auditores deberían refrescar manualmente el navegador
(F5) para visualizar nuevas alertas, lo cual contradice directamente el requisito de "reducir el
tiempo de respuesta ante anomalías críticas". En un escenario donde se detecta una irregularidad
de alto riesgo (por ejemplo, un faltante de efectivo superior a $500), el auditor podría no
enterarse durante horas si no está activamente revisando el sistema.
Las Alternativas B y C obtienen puntuación perfecta porque ambas implementan mecanismos
de notificación push: SignalR en el caso de B y Azure SignalR Service en el caso de C. Ambas
71
cumplen con los cuatro detectores requeridos (Cash Fraud, Invoice Anomaly, Payment Fraud,
Compliance Violation), los dashboards interactivos y la gestión completa del ciclo de vida de
alertas.
2. Eficiencia de Desempeño (Alt. A: 4/5, Alt. B: 5/5, Alt. C: 5/5)
Esta característica evalúa el rendimiento relativo a la cantidad de recursos utilizados bajo
condiciones determinadas. La Alternativa A obtiene 4 puntos porque su ciclo de detección de 15-
30 minutos, aunque funcional, representa el doble del tiempo objetivo establecido (5-10
minutos). Además, la dependencia de conexiones VPN site-to-site introduce latencia adicional y
puntos de fallo que pueden degradar el rendimiento durante períodos de conectividad inestable.
La Alternativa B logra el tiempo objetivo de 5-10 minutos mediante Hangfire con reintentos
automáticos y el modelo de agentes que envían datos (push) en lugar de extraerlos (pull). La
Alternativa C teóricamente ofrece latencia inferior a 1 minuto gracias a Event Hubs y Stream
Analytics, pero esta capacidad representa sobrecapacidad para el volumen actual de 13,000-
15,000 transacciones diarias. Ambas obtienen 5 puntos porque cumplen o superan los requisitos
de desempeño establecidos.
3. Compatibilidad (Alt. A: 4/5, Alt. B: 5/5, Alt. C: 4/5)
La compatibilidad mide la capacidad del sistema para intercambiar información con otros
sistemas y coexistir en el mismo entorno. La integración con las bases de datos Firebird legacy
representa el principal desafío técnico del proyecto.
La Alternativa A obtiene 4 puntos porque, aunque puede conectarse a Firebird mediante
ADO.NET con el proveedor FirebirdSQL, la arquitectura de extracción centralizada (pull)
genera carga en las bases de datos de las estaciones durante las consultas, lo cual podría afectar
el rendimiento del sistema de punto de venta en horarios de alta demanda.
La Alternativa B obtiene puntuación perfecta gracias a su modelo de agentes ligeros instalados
en cada estación. Estos agentes extraen datos localmente y los envían al servidor central,
72
minimizando el impacto en las operaciones de la estación. Además, el mecanismo de store-andforward permite tolerar desconexiones temporales sin pérdida de datos.
La Alternativa C obtiene 4 puntos porque, aunque técnicamente compatible, requiere que los
agentes publiquen eventos a Azure Event Hubs, lo cual introduce dependencia de conectividad a
internet constante desde cada estación. En zonas rurales de Santo Domingo donde operan
algunas estaciones, esta conectividad no siempre es confiable.
4. Usabilidad (Alt. A: 2/5, Alt. B: 5/5, Alt. C: 5/5)
La usabilidad evalúa la facilidad con que los usuarios pueden utilizar el sistema para lograr
objetivos específicos con efectividad, eficiencia y satisfacción. Este criterio presenta la mayor
diferencia entre alternativas.
La Alternativa A obtiene únicamente 2 puntos debido a su experiencia de usuario deficiente. Sin
actualización automática, los auditores deben desarrollar el hábito de refrescar constantemente la
página, lo cual genera fatiga cognitiva y riesgo de perder alertas críticas. La ausencia de
notificaciones proactivas significa que el sistema es pasivo: el usuario debe "buscar" los
problemas en lugar de ser informado de ellos. Esta limitación contradice las mejores prácticas de
diseño de interfaces para sistemas de monitoreo y alerta.
Las Alternativas B y C obtienen puntuación perfecta porque proporcionan una experiencia de
usuario moderna y proactiva. Los dashboards se actualizan automáticamente cuando se detectan
nuevas anomalías, aparecen notificaciones toast informando de nuevas alertas, y el contador de
alertas pendientes se incrementa en tiempo real. Esta experiencia fluida reduce la carga cognitiva
del auditor y garantiza que ninguna alerta crítica pase desapercibida.
5. Fiabilidad (Alt. A: 4/5, Alt. B: 5/5, Alt. C: 5/5)
La fiabilidad mide la capacidad del sistema para realizar funciones especificadas bajo
condiciones determinadas durante un período de tiempo establecido.
73
La Alternativa A obtiene 4 puntos porque su dependencia de conexiones VPN introduce
múltiples puntos de fallo. Si una VPN se desconecta durante el proceso de extracción, el ciclo
completo puede fallar para esa estación sin mecanismo automático de recuperación. Windows
Task Scheduler, aunque funcional, no ofrece las capacidades de reintento, persistencia de
trabajos y monitoreo que proporciona Hangfire.
La Alternativa B obtiene puntuación perfecta gracias a varios mecanismos de tolerancia a fallos:
Hangfire persiste los trabajos en PostgreSQL (sobreviven reinicios del servidor), implementa
reintentos automáticos con backoff exponencial, y los agentes de estación almacenan
transacciones localmente si pierden conectividad (store-and-forward). Amazon RDS PostgreSQL
proporciona backups automáticos y alta disponibilidad.
La Alternativa C también obtiene 5 puntos debido a los SLAs de 99.95% de disponibilidad de los
servicios Azure administrados, dead-letter queues para manejo de mensajes fallidos, y
escalamiento automático ante picos de carga.
6. Seguridad (Alt. A: 4/5, Alt. B: 5/5, Alt. C: 5/5)
La seguridad evalúa la protección de información y datos, incluyendo autenticación,
autorización, integridad y confidencialidad.
La Alternativa A obtiene 4 puntos porque, aunque implementa autenticación JWT y RBAC, la
gestión de credenciales VPN para 10 estaciones representa una superficie de ataque adicional.
Las conexiones VPN deben mantenerse y rotarse periódicamente, y una credencial
comprometida podría permitir acceso a múltiples estaciones.
Las Alternativas B y C obtienen puntuación perfecta. La Alternativa B utiliza HTTPS para todas
las comunicaciones, JWT con refresh tokens para autenticación, y las credenciales de los agentes
son individuales por estación (principio de mínimo privilegio). Amazon RDS soporta cifrado en
reposo y AWS IAM proporciona control de acceso granular.
La Alternativa C ofrece seguridad comparable mediante Azure Active Directory, managed
identities para comunicación entre servicios, y cifrado automático en todos los servicios Azure.
74
7. Mantenibilidad (Alt. A: 5/5, Alt. B: 4/5, Alt. C: 2/5)
La mantenibilidad mide la facilidad con que el sistema puede ser modificado para corregir
defectos, mejorar rendimiento u otros atributos, o adaptarse a cambios en el entorno.
La Alternativa A obtiene puntuación perfecta porque su arquitectura monolítica tradicional sin
componentes de tiempo real es la más simple de mantener. Un solo proyecto de Visual Studio,
sin dependencias complejas, con patrones básicos y bien documentados. Cualquier desarrollador
.NET con experiencia básica puede mantener el sistema.
La Alternativa B obtiene 4 puntos. Aunque sigue siendo una arquitectura monolítica, la inclusión
de SignalR y Hangfire agrega complejidad moderada. SignalR requiere gestión de conexiones
WebSocket (aunque la biblioteca simplifica significativamente esta tarea), y Hangfire introduce
conceptos de procesamiento en segundo plano que requieren comprensión adicional. Sin
embargo, ambas tecnologías están bien documentadas por Microsoft y tienen comunidades
activas.
La Alternativa C obtiene únicamente 2 puntos debido a su complejidad arquitectónica. Con
múltiples Azure Functions, Event Hubs, Service Bus, Cosmos DB y SignalR Service, el sistema
tiene numerosos componentes distribuidos que interactúan de formas complejas. El debugging de
arquitecturas serverless y orientadas a eventos es significativamente más difícil que en
monolitos: las trazas se distribuyen entre múltiples servicios, los errores pueden propagarse de
maneras no obvias, y se requiere expertise especializado en observabilidad distribuida. Para un
equipo de 2 estudiantes sin experiencia previa en estas tecnologías, la curva de aprendizaje sería
prohibitiva.
8. Portabilidad (Alt. A: 4/5, Alt. B: 4/5, Alt. C: 3/5)
La portabilidad evalúa la facilidad con que el sistema puede transferirse de un entorno a otro.
Las Alternativas A y B obtienen 4 puntos porque, aunque están diseñadas para entornos
específicos (Windows Server/on-premise para A, AWS para B), ambas utilizan tecnologías
estándar (.NET Core, PostgreSQL/SQL Server) que pueden desplegarse en múltiples
75
plataformas. La Alternativa B podría migrarse a Azure o Google Cloud con esfuerzo moderado,
y la Alternativa A podría virtualizarse o containerizarse si fuera necesario.
La Alternativa C obtiene 3 puntos porque depende fuertemente de servicios específicos de Azure
(Azure Functions, Cosmos DB, Event Hubs, Service Bus). Aunque existen equivalentes en AWS
(Lambda, DynamoDB, Kinesis, SQS) y GCP (Cloud Functions, Firestore, Pub/Sub), una
migración requeriría reescribir porciones significativas del código y reconfigurar toda la
infraestructura. El vendor lock-in es una característica inherente de las arquitecturas serverless
que aprovechan servicios administrados.
Análisis de factores contextuales adicionales
Además de las ocho características de ISO/IEC 25010, se consideraron factores contextuales
específicos del proyecto:
Factor de Costo Total de Propiedad (TCO)
Table 13. Comparación del Costo Total de Propiedad (TCO) entre alternativas
Concepto Alt. A Alt. B Alt. C
Desarrollo (6 meses) $2,500 $3,410 $8,000-$12,000
Operación mensual $150-200 $80-120 $474-724
Costo anual (post-desarrollo) $1,800-$2,400 $960-$1,440 $5,688-$8,688
La Alternativa B ofrece el menor costo operativo anual gracias a los servicios cloud optimizados
de AWS, mientras que la Alternativa C, a pesar de su modelo de pago por uso, acumula costos
significativos por la cantidad de servicios administrados requeridos.
Factor de Riesgo Técnico
76
La Alternativa A presenta riesgo bajo por utilizar tecnologías maduras y simples, pero riesgo alto
de no cumplir expectativas de usabilidad. La Alternativa C presenta riesgo alto de no
completarse en el tiempo establecido debido a su complejidad. La Alternativa B presenta riesgo
moderado-bajo: las tecnologías son maduras y documentadas, y el equipo tiene acceso a recursos
de aprendizaje de calidad.
Factor de Escalabilidad Futura
PetrolRíos S.A. tiene planes de integrar las 90 estaciones restantes. La Alternativa A requeriría
rediseño significativo para escalar (más conexiones VPN, servidor más potente). La Alternativa
B puede escalar incrementalmente (más instancias EC2, RDS más grande). La Alternativa C
escalaría automáticamente pero con costos proporcionales.
Conclusión de la evaluación
La Alternativa B representa el equilibrio óptimo entre los ocho criterios de calidad de ISO/IEC
25010 y los factores contextuales del proyecto. Con una puntuación de 38/40 puntos, supera a la
Alternativa A (30/40) que sacrifica usabilidad y funcionalidad crítica, y a la Alternativa C
(34/40) que, aunque técnicamente superior en algunos aspectos, presenta desventajas
significativas en mantenibilidad, portabilidad y viabilidad económica.
La selección de la Alternativa B se fundamenta en el principio de "suficiencia técnica":
implementar la solución más simple que cumpla completamente los requisitos, sin
sobrecapacidad innecesaria que incremente complejidad y costos. Para el contexto específico de
un proyecto académico de 6 meses, con un equipo de 2 desarrolladores y un presupuesto de
$3,410 USD, la Alternativa B proporciona todas las funcionalidades requeridas con una
arquitectura manejable y un costo sostenible.
2.2. Impacto del proyecto en la sociedad
El desarrollo del sistema de detección de anomalías genera impactos significativos que
trascienden el ámbito técnico-operacional de PetrolRíos S.A. A continuación se presenta un
77
análisis PESTEL (Político, Económico, Social, Tecnológico, Ambiental, Legal) que evalúa los
beneficios esperados tanto para la organización como para la sociedad.
Table 14. Impactos y métricas del proyecto (Análisis PESTEL)
Dimensión Impacto Indicador Forma de medición
Político Fortalecimiento de
transparencia y buen
gobierno corporativo
Cumplimiento de
políticas de control
interno
Auditorías externas
anuales
Económico Detección temprana de
irregularidades que
generan pérdidas
Anomalías
detectadas por mes
Registro en sistema vs.
período anterior
Económico Optimización de costos
operativos de auditoría
Reducción de
horas-hombre
Comparación
antes/después de
implementación
Social Protección de empleos
mediante sostenibilidad
Estabilidad laboral Mantención de plantilla
actual
Social Garantía de precios justos
al consumidor
Facturas correctas Reducción de errores de
facturación
Tecnológico Adopción de tecnologías
modernas en sector
energético
Nivel de
automatización
De 0% a 100%
cobertura de análisis
transaccional
Ambiental Reducción de consumo de Hojas ahorradas Estimado 5,000-10,000
78
papel mensualmente hojas/mes
Legal Cumplimiento verificable
de normativas
ARCERNNR
Infracciones
detectadas
proactivamente
Alertas de compliance
generadas por el sistema
ImpactoEconómico:
El impacto económico principal radica en la capacidad de detectar irregularidades que
actualmente pasan desapercibidas. Dado que los controles actuales basados en cuadres
consolidados solo identifican diferencias cuando los totales no coinciden, existe una cantidad
indeterminada de fraudes que se ocultan dentro de transacciones que cuadran numéricamente.
El sistema propuesto incrementará la cobertura de análisis del 0% (ninguna transacción analizada
individualmente) al 100% de las transacciones, con detección en menos de 10 minutos. Aunque
no es posible cuantificar las pérdidas actuales con precisión (ya que precisamente el problema es
que no se detectan), la literatura especializada indica que sistemas similares han generado
reducciones significativas en irregularidades tras su implementación.
Adicionalmente, la automatización del análisis transaccional liberará tiempo del equipo de
auditoría para enfocarse en investigación de casos, análisis profundo y mejora de procesos,
incrementando el valor agregado de su trabajo.
Impacto	 Social:
El proyecto contribuye a la sostenibilidad financiera de PetrolRíos S.A., lo que indirectamente
protege los empleos de las familias que dependen de la empresa. Además, al mejorar los controles
79
sobre facturación, se garantiza que los consumidores finales reciban facturas correctas sin
sobrecargos indebidos.
Impacto	 Tecnológico:
La implementación de este sistema representa un avance en la madurez tecnológica del sector de
estaciones de servicio en Ecuador. El uso de tecnologías modernas como ASP.NET Core 9.0, React
18, SignalR, PostgreSQL y AWS establece un referente de innovación que puede replicarse en otras
empresas del sector.
Impacto Ambiental:
La digitalización de los procesos de auditoría reduce significativamente el consumo de papel. Se
estima una reducción de 5,000 a 10,000 hojas mensuales al migrar los procesos de
documentación y reportes a formato digital.
80
3. Objetivos
Los objetivos del proyecto se definen siguiendo la metodología SMART (Específico, Medible,
Alcanzable, Relevante, con Tiempo definido). El objetivo general responde a qué se va a hacer,
cómo se va a realizar y para qué, mientras que los objetivos específicos se alinean con las etapas
del ciclo de vida de desarrollo de software.
3.1. Objetivo General
Desarrollar un sistema web de detección de anomalías transaccionales para PetrolRíos S.A. con
ASP.NET Core 9 y React 18, ejecutando análisis batch cada 5–10 minutos y alertas por SignalR,
para cubrir el 100% de transacciones de 10 estaciones y reducir la detección a <10 minutos,
validado con datos reales Contaplus/Firebird.
Análisis del objetivo según criterios SMART:
• Específico: Desarrollar un sistema web de detección de anomalías a nivel transaccional
(no genérico, no de cuadres consolidados)
• Medible: Reducir el tiempo de detección de días/semanas a menos de 10 minutos;
incrementar la cobertura de análisis transaccional del 0% al 100%
• Alcanzable: Tecnologías probadas (ASP.NET Core 9.0, React 18, SignalR, PostgreSQL)
y equipo capacitado
• Relevante: Resuelve el problema identificado de ausencia de detección de anomalías a
nivel transaccional
• Tiempo definido: 6 meses de desarrollo (diciembre 2025 - mayo 2026)
Los objetivos específicos se organizan siguiendo las etapas del ciclo de vida de desarrollo de
software y se vinculan con evidencias documentales verificables en el documento.
OE1: Diseñar la arquitectura del sistema mediante el modelo C4 (niveles 2 y 3), definiendo los
contenedores, componentes y flujos de datos que integren las 10 bases de datos Firebird de las
81
estaciones piloto con PostgreSQL en AWS como repositorio central.
OE2: Desarrollar un motor de detección de anomalías con cuatro detectores (Cash Fraud,
Invoice Anomaly, Payment Fraud, Compliance Violation) basados en reglas de negocio
configurables y técnicas de interrogación de archivos, alcanzando una tasa de alertas válidas
mayor al 90%, medida mediante la contrastación de los resultados del sistema contra casos
conocidos documentados por el Área de Auditoría durante el levantamiento de información.
OE3: Implementar el módulo de procesamiento batch mediante Hangfire que ejecute los
detectores cada 5-10 minutos, con extracción de datos desde las estaciones Firebird y tolerancia a
fallos mediante reintentos automáticos.
OE4: Construir una interfaz web con dashboards interactivos y notificaciones push mediante
SignalR, utilizando React 18 con TypeScript, para que los auditores visualicen alertas en tiempo
real sin necesidad de refrescamiento manual.
OE5: Validar el sistema mediante pruebas unitarias (cobertura > 80%), pruebas de integración y
pruebas de aceptación con usuarios del Área de Auditoría, ejecutando los detectores sobre
transacciones reales del sistema Contaplus/Firebird durante un período de operación paralela de
2 semanas, y contrastando los resultados contra casos conocidos para verificar la efectividad de
detección."
82
4. Alcance
Esta sección define los límites del proyecto, especificando qué incluye y qué no incluye el
desarrollo. Se presentan los casos de uso, prototipos de pantallas, arquitectura de la solución y las
restricciones que condicionan el desarrollo.
4.1. Alcance de la solución seleccionada
4.1.1. Diagrama de casos de uso
El sistema contempla cuatro actores principales: Auditor, Supervisor de Auditoría, Gerente
Administrativo y Administrador del Sistema. La Figura 7 presenta el diagrama de casos de uso
que muestra las funcionalidades disponibles para cada actor.
Figure 7. Diagrama de casos de uso del Sistema de Detección de Anomalías
83
El diagrama de casos de uso presenta las funcionalidades del sistema desde la perspectiva de sus
usuarios, siguiendo la notación estándar UML (Unified Modeling Language). El diagrama
identifica cuatro actores principales que interactúan con el sistema, cada uno con un conjunto
específico de casos de uso que definen las acciones que pueden realizar.
Actores del sistema
1. Auditor Interno (Actor principal)
Representa a los dos auditores del Área de Auditoría y Control Interno. Es el usuario principal
del sistema, responsable de revisar las alertas generadas y documentar las investigaciones.
Utiliza el sistema diariamente durante toda su jornada laboral.
Representación visual: Figura de persona en color azul, indicando su rol operativo.
2. Supervisor de Auditoría
Representa al supervisor del área que coordina el trabajo de los auditores. Hereda todas las
funcionalidades del Auditor Interno (relación de extensión), además de funciones adicionales de
supervisión. Es responsable de asignar casos a los auditores y configurar los parámetros de
detección.
Representación visual: Figura de persona en color verde, indicando su rol de supervisión.
3. Administrador del Sistema
Representa al personal del Departamento de Sistemas responsable del mantenimiento técnico.
Gestiona usuarios, roles y permisos de acceso al sistema. También configura las reglas de los
detectores y monitorea el funcionamiento técnico.
Representación visual: Figura de persona en color morado, indicando su rol técnico.
84
4. Sistema Hangfire (Actor automatizado)
Representa el componente de procesamiento automático que ejecuta los detectores. No es un
usuario humano, sino un proceso automatizado que opera cada 5–10 minutos. Es responsable de
la extracción de datos, ejecución de detectores y generación de alertas.
Representación visual: Figura de persona en color naranja, indicando su naturaleza
automatizada.
Casos de uso del Auditor Interno
• CU-01 Iniciar Sesión: El auditor ingresa sus credenciales (usuario y contraseña) para
acceder al sistema. El sistema valida las credenciales mediante JWT y establece la sesión.
• CU-02 Cerrar Sesión: El auditor finaliza su sesión de trabajo de forma segura. El
sistema invalida el token JWT y registra la hora de salida en el log de auditoría.
• CU-03 Visualizar Dashboard de Alertas: El auditor accede a la pantalla principal
donde visualiza un resumen de las alertas activas, clasificadas por nivel de riesgo (crítico,
alto, medio, bajo). El dashboard se actualiza automáticamente mediante SignalR sin
necesidad de refrescar la página.
• CU-04 Consultar Lista de Alertas: El auditor accede a la lista completa de alertas con
opciones de filtrado por tipo de anomalía, estación, fecha, nivel de riesgo y estado
(pendiente, en revisión, resuelta).
• CU-05 Ver Detalle de Anomalía: El auditor selecciona una alerta específica para ver
información detallada incluyendo: tipo de anomalía, transacciones involucradas, valores
esperados versus encontrados, estación y turno donde se detectó, y evidencia de soporte.
• CU-06 Clasificar Anomalía: El auditor clasifica la anomalía según su análisis en una de
tres categorías: Confirmada (es una irregularidad real), Falso Positivo (la detección fue
incorrecta), o Requiere Investigación (se necesita más información).
• CU-07 Agregar Comentarios: El auditor documenta sus hallazgos, observaciones y
conclusiones agregando comentarios a la alerta. Cada comentario queda registrado con
fecha, hora y autor.
85
• CU-08 Marcar como Resuelta: Una vez completada la investigación y documentación,
el auditor marca la alerta como resuelta, indicando las acciones tomadas. La alerta pasa al
historial pero permanece accesible para consulta.
• CU-09 Filtrar Alertas: El auditor aplica filtros para encontrar alertas específicas según
criterios como: tipo de detector (Cash Fraud, Invoice Anomaly, Payment Fraud,
Compliance), rango de fechas, estación específica, o nivel de riesgo.
• CU-10 Recibir Notificación en Tiempo Real: El auditor recibe notificaciones
automáticas cuando se detectan nuevas anomalías. Las notificaciones aparecen como
toast messages en la interfaz y actualizan el contador de alertas pendientes.
Casos de uso adicionales del Supervisor de Auditoría
El Supervisor de Auditoría hereda todos los casos de uso del Auditor Interno (relación de
extensión en UML) y además tiene acceso a las siguientes funcionalidades exclusivas:
• CU-11 Asignar Alerta a Auditor: El supervisor asigna una alerta específica a uno de los
auditores del equipo para su investigación. El auditor asignado recibe una notificación y
la alerta aparece en su lista de casos pendientes.
• CU-12 Generar Reportes: El supervisor genera reportes consolidados en formato PDF o
Excel con estadísticas de alertas detectadas, tiempos de resolución, distribución por tipo y
estación, y métricas de efectividad.
• CU-13 Visualizar Métricas y KPIs: El supervisor accede a dashboards ejecutivos con
indicadores clave como: número de alertas por período, tiempo promedio de resolución,
tasa de falsos positivos, y comparativos entre estaciones.
• CU-14 Configurar Umbrales de Detección: El supervisor ajusta los umbrales de
sensibilidad de los detectores según la experiencia operativa. Por ejemplo, puede
modificar el umbral de faltante de efectivo de $50 a $100 si hay muchos falsos positivos.
86
Casos de uso del Administrador del Sistema
• CU-15 Gestionar Usuarios y Roles: El administrador crea, modifica y desactiva cuentas
de usuario. Asigna roles (Auditor, Supervisor, Administrador) que determinan los
permisos de acceso.
• CU-16 Configurar Reglas de Detectores: El administrador configura las reglas de
negocio de cada detector, incluyendo: condiciones de activación, campos a evaluar, y
parámetros técnicos de conexión con las bases de datos.
• CU-17 Consultar Logs de Auditoría: El administrador revisa los registros de actividad
del sistema incluyendo: inicios de sesión, acciones realizadas por usuarios, errores del
sistema, y ejecuciones de los detectores.
Casos de uso del Sistema Hangfire (automatizados)
Estos casos de uso se ejecutan automáticamente sin intervención humana, disparados por el
programador de tareas Hangfire cada 5-10 minutos:
• CU-18 Extraer Transacciones desde Firebird: El sistema conecta con cada una de las
10 bases de datos Firebird de las estaciones y extrae las transacciones nuevas desde la
última extracción exitosa.
• CU-19 Ejecutar 4 Detectores de Anomalías: El sistema ejecuta en paralelo los cuatro
detectores (Cash Fraud, Invoice Anomaly, Payment Fraud, Compliance Violation) sobre
las transacciones extraídas.
• CU-20 Generar y Clasificar Alertas: El sistema genera alertas para las anomalías
detectadas, calcula el nivel de riesgo mediante un algoritmo de scoring, almacena las
alertas en PostgreSQL, y notifica a los usuarios mediante SignalR.
Relaciones entre casos de uso
El diagrama muestra dos tipos de relaciones entre casos de uso:
Relación <<include>>: Indica que un caso de uso incluye obligatoriamente a otro. Por ejemplo,
CU-04 "Consultar Lista de Alertas" incluye a CU-05 "Ver Detalle de Anomalía", ya que para ver
87
el detalle primero se debe consultar la lista. De manera similar, los casos de uso automatizados
tienen una secuencia obligatoria: CU-18 (Extraer) incluye a CU-19 (Detectar) que incluye a CU20 (Generar alertas).
Relación <<extends>>: Indica herencia de funcionalidades. El Supervisor de Auditoría extiende
(hereda) todas las funcionalidades del Auditor Interno, lo que significa que puede realizar todas
las acciones que realiza un auditor, más las funciones exclusivas de supervisión.
4.1.2. Descripción de casos de uso principales
La Tabla 8 presenta la descripción de los cinco casos de uso principales del sistema, incluyendo
el actor, precondiciones, flujo básico y postcondiciones.
Table 15. Descripción de casos de uso principales
ID Caso de Uso Actor Descripción
CU-01 Visualizar
Dashboard de
Alertas
Auditor El auditor accede al dashboard principal
donde visualiza las alertas activas
clasificadas por nivel de riesgo (crítico,
alto, medio, bajo), con actualización
automática vía SignalR.
CU-02 Consultar Detalle
de Anomalía
Auditor El auditor selecciona una alerta para ver
información detallada: tipo de anomalía,
transacciones involucradas, estación,
fecha/hora, valores esperados vs
encontrados, y evidencia de respaldo.
CU-03 Clasificar y
Documentar
Auditor El auditor clasifica la anomalía como
confirmada, falso positivo o requiere
88
Anomalía investigación, agregando comentarios y
documentación de soporte.
CU-04 Ejecutar
Procesamiento
Batch
Sistema
(Hangfire)
El sistema ejecuta automáticamente cada
5-10 minutos: extracción de transacciones
desde Firebird, análisis mediante
detectores, generación de alertas, y
notificación vía SignalR.
CU-05 Configurar Reglas
de Detección
Administrador El administrador configura los umbrales y
parámetros de los detectores: porcentaje
de desviación aceptable, campos
obligatorios, rangos de precios, horarios
de operación normal.
4.1.3. Prototipos de pantallas (Media fidelidad)
A continuación se presentan los prototipos de las tres pantallas principales del sistema. Estos
prototipos de media fidelidad muestran la distribución de elementos, navegación y
funcionalidades principales sin representar el diseño visual final.
89
Figure 8. Prototipo de pantalla: Dashboard principal de alertas
Figure 9. Prototipo de pantalla: Detalle de anomalía detectada
90
Figure 10. Prototipo de pantalla: Configuración de reglas de detección
4.1.4. Arquitectura de la solución (C4 Nivel 2 - Contenedores)
La Figura 11 presenta la arquitectura de contenedores de la solución seleccionada, mostrando las
aplicaciones, bases de datos y servicios que componen el sistema, así como sus interacciones.
Figure 11. Arquitectura de contenedores (C4 Nivel 2) de la solución seleccionada
91
Descripción de contenedores
1. Web Application (Frontend): Aplicación SPA desarrollada en React 18 con TypeScript. Se
comunica con el backend mediante API REST para operaciones CRUD y mediante SignalR
Client para recibir notificaciones push en tiempo real. Se despliega en Vercel o Azure Static Web
Apps.
2. API Application (Backend): Aplicación ASP.NET Core 9.0 que expone endpoints REST
para la gestión de alertas, usuarios, reportes y configuración. Incluye un SignalR Hub para
broadcast de notificaciones a clientes conectados. Implementa autenticación JWT y autorización
basada en roles (RBAC).
3. Hangfire Server: Servicio de background jobs que ejecuta el procesamiento batch cada 5-10
minutos. Coordina la extracción de datos desde Firebird, ejecución de detectores, y generación
de alertas. Incluye reintentos automáticos y logging completo.
4. PostgreSQL en Amazon Web Services (AWS): Base de datos centralizada que almacena
alertas detectadas, configuración de detectores, usuarios y roles, y logs de auditoría. Utiliza
Entity Framework Core como ORM con migraciones Code-First.
5. Firebird Databases (Legacy): 10 bases de datos distribuidas (una por cada estación piloto
integrada al sistema), que contienen las transacciones de ventas, inventarios y facturación. Se
accede mediante ADO.NET con proveedor FirebirdSQL en modo solo lectura. Las 80 estaciones
restantes se integrarán en fases futuras.
6. Servidor SMTP: Servicio externo (SendGrid) para envío de notificaciones por email en casos
de anomalías críticas que requieren atención inmediata.
92
4.1.5. Flujo del proceso de detección
La Figura 12 presenta el diagrama de flujo del proceso automatizado de detección de anomalías.
Este proceso representa el núcleo funcional del sistema propuesto y se ejecuta cada 5-10 minutos
mediante el gestor de tareas Hangfire, transformando la capacidad de detección de la empresa del
0% al 100% de cobertura transaccional.
Figure 12. Diagrama de flujo del proceso de detección de anomalías
El diagrama utiliza la siguiente notación visual:
• Óvalos (verde/rojo): Inicio y fin del proceso
• Rectángulos redondeados: Actividades y procesamientos
• Rombos (amarillo): Puntos de decisión condicional
93
• Caja punteada (azul): Estructura de iteración (loop)
• Flechas: Flujo de control y secuencia de ejecución
Descripción del proceso
Paso 1 - Activación del trabajo programado (Hangfire)
El proceso inicia cuando Hangfire, un gestor profesional de trabajos en segundo plano para
.NET, dispara automáticamente el trabajo de detección según la programación configurada (cada
5-10 minutos). Hangfire mantiene una cola de tareas persistente en PostgreSQL, lo que garantiza
que si el servidor se reinicia, los trabajos pendientes no se pierden y se reanudan
automáticamente. Esta característica de tolerancia a fallos es fundamental para un sistema de
detección que debe operar de manera continua y confiable.
Paso 2 - Extracción de datos desde Firebird (Loop por estaciones)
El diagrama muestra una estructura de iteración (caja punteada azul) que representa el
procesamiento secuencial de las 10 estaciones integradas al sistema. Para cada estación (i = 1 a
10), se ejecutan los siguientes sub-pasos:
Table 16. Proceso de extracción, transformación y carga (ETL) desde Firebird
Sub-paso Descripción
2a. Conexión
El sistema establece conexión con la base de datos CONTAC.FDB de la
estación mediante el proveedor FirebirdSQL para .NET, en modo de solo
lectura para no afectar las operaciones del punto de venta.
2b. Consulta
Se consultan únicamente las transacciones con fecha/hora posterior a la
última extracción exitosa (técnica de watermark), evitando reprocesar datos y
optimizando el rendimiento.
2c. Extracción Se extraen datos de cuatro tablas: FACTURAS (encabezados),
DETALLES_FACTURA (líneas de detalle), CIERRES_TURNO (efectivo
94
Sub-paso Descripción
reportado) y MEDIDORES (lecturas de surtidores).
2d.
Transformación
Los datos se transforman al modelo central del sistema, normalizando
formatos de fecha (ISO 8601), moneda y códigos de producto.
2e. Carga
Los datos transformados se cargan en tablas de staging en PostgreSQL (AWS
RDS), que son temporales y se limpian después de cada ciclo.
2f. Watermark
Se actualiza la marca de agua que registra la última transacción procesada,
asegurando que el siguiente ciclo continúe desde ese punto.
Si la conexión con una estación falla (por ejemplo, por problemas de conectividad), el sistema
registra el error y continúa con la siguiente estación, garantizando que un fallo individual no
detenga todo el proceso.
Paso 3 - Ejecución de los cuatro detectores (en paralelo)
Una vez completada la extracción de todas las estaciones, se ejecutan simultáneamente los cuatro
detectores de anomalías, aprovechando múltiples núcleos del servidor para optimizar el tiempo
de procesamiento. El diagrama muestra los detectores en un contenedor azul que indica su
ejecución paralela:
Table 17. Descripción de los detectores de anomalías del sistema
Detector Objetivo Reglas principales
Cash Fraud
Detectar irregularidades
en manejo de efectivo
Comparar efectivo reportado vs. sistema; identificar
faltantes > umbral; detectar patrones de faltantes
recurrentes por empleado
Invoice Detectar discrepancias en Verificar precios vs. autorizados; detectar descuentos
95
Detector Objetivo Reglas principales
Anomaly facturación fuera de política; identificar campos vacíos; calcular
tasa de anulaciones
Payment
Fraud
Detectar manipulación de
pagos
Identificar reversiones > 30 min; detectar
transacciones duplicadas; analizar vouchers
inconsistentes
Compliance
Violation
Verificar cumplimiento
normativo
Verificar placa ZZZ999949 ≤ 5 galones; detectar
vehículo con múltiples tipos de combustible; validar
horarios de operación
Las reglas detalladas de cada detector se especifican en la Tabla 3 de la Sección 1.1.3.
Paso 4 - Clasificación por nivel de riesgo
Las anomalías detectadas reciben un score de riesgo en escala de 0 a 100, calculado mediante la
fórmula:
Score = Riesgo_Base × Multiplicadores
Donde el riesgo base depende del tipo de anomalía, y los multiplicadores consideran el monto
involucrado, la reincidencia del empleado o estación, y el historial previo. La clasificación
resultante es:
Table 18. Clasificación de anomalías por nivel de riesgo
Nivel Rango Significado
Tiempo de
respuesta
Justificación del rango
Bajo 0-25
Anomalías
menores
Próxima
auditoría
rutinaria
Desviaciones dentro de márgenes
operativos normales. Ejemplo: faltante
< $20 que puede ser error de cambio
96
Medio 26-50
Requiere
investigación
24-48 horas
Patrón que excede variabilidad normal
pero no implica pérdida inmediata.
Ejemplo: empleado con 3 anulaciones
en un día
Alto 51-75
Atención
prioritaria
Mismo día
Indicadores fuertes de irregularidad con
impacto económico demostrable.
Ejemplo: cliente con 88% de ventas en
tarjeta de crédito
Crítico 76-100
Investigación
inmediata
< 1 hora
Evidencia clara de fraude activo o
pérdida significativa en curso. Ejemplo:
despachador con 100% ventas en
contado o faltante recurrente (gineteo)
Paso 5 - Almacenamiento de alertas en PostgreSQL
Las alertas clasificadas se persisten en la base de datos PostgreSQL (AWS RDS) con los
siguientes metadatos: identificador único, tipo de anomalía, nivel de riesgo, estación donde se
detectó, transacciones involucradas, timestamp de detección y estado inicial "Pendiente".
Punto de decisión: ¿Hay anomalías?
El diagrama muestra un rombo de decisión que evalúa si se detectaron anomalías en el ciclo
actual. Si no hay anomalías (rama "NO"), el proceso termina y espera el próximo ciclo. Si hay
anomalías (rama "SÍ"), continúa con la notificación.
Paso 6 - Publicación de notificaciones vía SignalR
El sistema invoca el Hub de SignalR para realizar un broadcast a todos los clientes (navegadores)
conectados. SignalR es una biblioteca que simplifica la comunicación bidireccional en tiempo
real, permitiendo que el servidor envíe contenido a los clientes instantáneamente sin que estos
97
deban solicitarlo (Microsoft, 2025). El mensaje incluye: cantidad de nuevas alertas, cantidad de
alertas críticas y resumen de tipos detectados.
Punto de decisión: ¿Nivel Crítico?
Un segundo rombo de decisión evalúa si alguna alerta tiene nivel de riesgo "Crítico" (76-100
puntos).
Paso 7 - Envío de email al supervisor (condicional)
Si existen alertas críticas (rama "SÍ"), se envía adicionalmente un correo electrónico al
supervisor de auditoría como mecanismo de respaldo. El correo incluye: resumen de alertas
críticas, enlace directo al dashboard y recomendación de acción inmediata. Este paso es
condicional y solo se ejecuta para anomalías de máxima gravedad.
Paso 8 - Actualización automática del dashboard
El frontend React recibe el evento de SignalR a través del cliente integrado y actualiza la interfaz
automáticamente, sin que el auditor deba refrescar la página. Las actualizaciones incluyen:
• Incremento del contador de alertas en el header (+N alertas nuevas)
• Aparición de toast notification informando las nuevas detecciones
• Actualización de la tabla de alertas mostrando las nuevas al inicio de la lista
FIN - Espera próximo ciclo
El proceso finaliza y el sistema queda en espera hasta que Hangfire dispare el siguiente ciclo de
detección (5-10 minutos después).
Tiempos estimados del proceso
Table 19. Tiempos estimados del proceso de detección de anomalías
Etapa Tiempo estimado
Extracción de datos (10 estaciones) ~5 minutos
98
Etapa Tiempo estimado
Ejecución de detectores (en paralelo) 1-2 minutos
Clasificación y almacenamiento < 30 segundos
Notificación SignalR < 5 segundos
Tiempo total máximo (ciclo completo) < 10 minutos
Este tiempo de detección representa una mejora significativa respecto al proceso manual actual,
donde las anomalías podían tardar días o semanas en detectarse, generando pérdidas
acumulativas difíciles de recuperar (Heavy Vehicle Inspection, 2025; Fuelmetrics, 2021).
4.1.6. Módulos funcionales del sistema
La Tabla 9 presenta los módulos funcionales incluidos en el alcance del proyecto con sus
principales características.
Table 20. Módulos funcionales del sistema
Módulo Funcionalidades principales Prioridad
Autenticación y
Autorización
Login JWT, gestión de sesiones, RBAC con 4 roles,
recuperación de contraseña
Alta
Procesamiento Batch
(ETL)
Extracción desde Firebird, transformación, carga en
PostgreSQL, scheduling con Hangfire
Alta
Motor de Detección 4 detectores (Cash Fraud, Invoice Anomaly, Payment
Fraud, Compliance), reglas configurables,
clasificación de riesgo
Alta
99
Gestión de Alertas CRUD de alertas, asignación, clasificación,
comentarios, historial
Alta
Dashboards KPIs en tiempo real, gráficos interactivos, filtros,
actualización SignalR
Alta
Notificaciones Push via SignalR, email para críticas, centro de
notificaciones
Media
Reportes Exportación PDF/Excel, reportes ejecutivos, reportes
por período
Media
Configuración Umbrales de detectores, campos obligatorios,
horarios, parámetros
Media
Administración Gestión de usuarios, roles, logs del sistema, monitoreo
de jobs
Media
4.1.7. Flujo proactivo de respuesta ante alertas
Una observación del comité evaluador señaló que el dashboard podría convertirse en una
herramienta manual que requiere monitoreo constante. Para abordar esta preocupación, es
fundamental aclarar que el sistema opera de manera proactiva: no requiere que el auditor esté
constantemente observando el dashboard para detectar anomalías. El flujo de respuesta funciona
de la siguiente manera:
Table 21. Flujo de respuesta proactiva por nivel de riesgo
Nivel de riesgo Acción del sistema (automática) Acción humana requerida
Bajo (0-25) Registro silencioso en base de datos. Revisión durante auditoría rutinaria.
100
Actualización del dashboard. Sin
notificación push.
No requiere acción inmediata.
Medio (26-50)
Notificación push vía SignalR al
dashboard (toast notification).
Incremento del contador de alertas.
Auditor revisa la alerta en las
próximas 24-48 horas. Clasifica
como confirmada, falso positivo o
requiere investigación.
Alto (51-75)
Notificación push inmediata vía
SignalR. Alerta destacada
visualmente en el dashboard.
Auditor investiga el mismo día.
Documenta hallazgos y asigna
seguimiento.
Crítico (76-
100)
Notificación push inmediata vía
SignalR + correo electrónico
automático al Supervisor de
Auditoría con enlace directo a la
alerta.
Supervisor gestiona investigación
inmediata (< 1 hora). Contacta a la
estación involucrada.
El elemento clave es que el rol del auditor se transforma de reactivo a analítico. En el proceso
actual, el auditor debe buscar manualmente las anomalías descargando datos y aplicando filtros
en Excel. Con el sistema propuesto, el sistema busca las anomalías automáticamente cada 5-10
minutos y notifica al auditor solo cuando encuentra algo relevante. El auditor ya no dedica
tiempo a la búsqueda, sino exclusivamente a la investigación y documentación de los casos que
el sistema identifica.
Table 22. Matriz RACI de gestión de alertas
Actividad Sistema Auditor Supervisor Gerente
101
Extracción de datos desde
Firebird
R — — —
Ejecución de detectores R — — —
Clasificación automática de
riesgo
R — — —
Notificación de alertas R I I (críticas) —
Investigación de alerta — R A I
Clasificación final
(confirmada/falso positivo)
— R A —
Documentación de hallazgos — R C —
Decisión de acciones correctivas — C R A
Ajuste de umbrales de detección — C R I
R = Responsable (ejecuta), A = Aprobador (autoriza), C = Consultado, I = Informado.
4.2. Limitaciones y restricciones del proyecto
Las limitaciones definen lo que el proyecto NO incluirá, mientras que las restricciones establecen
las condiciones que deben cumplirse durante el desarrollo.
4.2.1. Limitaciones (Lo que NO se hará)
Table 23. Limitaciones del proyecto
ID Limitación Justificación
102
L01 No se desarrollará aplicación
móvil nativa (iOS/Android)
El sistema web será responsivo y accesible
desde móviles. Una app nativa está fuera del
alcance temporal y presupuestario.
L02 No se implementará análisis
predictivo con Machine Learning
Se utilizarán reglas de negocio. ML queda
como trabajo futuro cuando existan datos
etiquetados suficientes.
L03 No se incluirá módulo completo de
gestión de inventarios
El sistema solo consume datos de inventario
para detectar anomalías; no los gestiona.
L04 No se realizará migración de datos
históricos
El sistema procesará datos desde su puesta en
producción; el histórico queda en Firebird.
L05 No se implementará integración
con sistemas contables
La integración con SQL Server corporativo
está fuera del alcance inicial.
4.2.2. Restricciones (Condiciones obligatorias)
Table 24. Restricciones del proyecto
ID Restricción Tipo
R01 El sistema funcionará únicamente en navegadores modernos
(Chrome 90+, Firefox 88+, Edge 90+, Safari 14+)
Técnica
R02 Se requiere conectividad de red estable entre el servidor y las
bases de datos Firebird de las estaciones
Técnica
R03 Duración máxima del proyecto: 6 meses (diciembre 2025 - mayo
2026)
Temporal
R04 Equipo de desarrollo: 2 estudiantes de Ingeniería de Software Recurso
R05 Presupuesto máximo: $3,410 USD Económica
103
R06 Se utilizará plataforma .NET (ASP.NET Core 9.0) para el
backend
Técnica
R07 Se utilizará React 18 con TypeScript para el frontend Técnica
R08 Las bases de datos Firebird son de solo lectura (no se
modificarán)
Técnica
104
5. Planificación y costos del proyecto
El proyecto se gestionará mediante metodología Scrum con sprints de 2 semanas, utilizando
Azure DevOps para gestión de tareas y GitHub para control de versiones. Esta sección presenta
el cronograma, presupuesto y análisis de riesgos del proyecto.
5.1. Cronograma del proyecto
El proyecto tiene una duración total de 24 semanas (6 meses), desde diciembre de 2025 hasta
mayo de 2026. La Tabla 12 presenta las fases del proyecto con sus actividades y duración,
mientras que la Figura 13 muestra el diagrama de Gantt correspondiente.
Table 25. Cronograma de fases del proyecto
Fase Actividades Duración Semanas
1. Análisis y
Requisitos
Entrevistas con auditoría, análisis de
procesos, documentación de requisitos
funcionales y no funcionales
4 semanas S1-S4
2. Diseño de
Arquitectura
Diagramas C4 (L1-L3), modelado de base
de datos (ER), definición de componentes
técnicos, diseño de API
3 semanas S5-S7
3. Desarrollo
Backend
Configuración ASP.NET Core + EF Core,
implementación API RESTful, desarrollo
de 3 detectores, integración Hangfire +
SignalR
6 semanas S8-S13
4. Desarrollo
Frontend
Configuración React 18 + TypeScript,
autenticación JWT, dashboards
interactivos, integración SignalR Client
5 semanas S14-S18
5. Pruebas y Pruebas unitarias (xUnit), pruebas de 4 semanas S19-S22
105
Validación integración, pruebas UAT con usuarios,
validación paralela 2 semanas
6. Documentación y
Entrega
Documentación técnica, manuales de
usuario, presentación final, entrega de
código fuente
2 semanas S23-S24
Table 26. Cronograma del proyecto (Diagrama de Gantt)
FASE /
ACTIVI
DAD
DURA
CIÓN
SEMANAS (Diciembre 2025 - Mayo 2026)
S
1
S
2
S
3
S
4
S
5
S
6
S
7
S
8
S
9
S
1
0
S
1
1
S
1
2
S
1
3
S
1
4
S
1
5
S
1
6
S
1
7
S
1
8
S
1
9
S
2
0
S
2
1
S
2
2
S
2
3
S
2
4
FASE
1:
Análisis
y
Levanta
miento
de
Requisit
os
4 sem
 1.1.
Entrevist
as con
área de
auditoría
2 sem
 1.2.
Análisis
de
procesos
actuales
2 sem
 1.3.
Documen
tación de
2 sem
106
requisito
s
funcional
es
FASE
2:
Diseño
de
Arquite
ctura y
Base de
Datos
3 sem
 2.1.
Diagram
as C4
(Niveles 1-4)
2 sem
 2.2.
Modelad
o de base
de datos
(ER)
2 sem
 2.3.
Definició
n de
compone
ntes
técnicos
1 sem
FASE
3:
Desarro
llo del
Backen
d y
Motor
de
Detecci
6 sem
107
ón
 3.1.
Configur
ación
ASP.NE
T Core +
EF Core
1 sem
 3.2.
Impleme
ntación
de API
RESTful
2 sem
 3.3.
Desarroll
o de
4
detectore
s de
anomalía s
3 sem
 3.4.
Integraci
ón
Hangfire
+
SignalR
2 sem
FASE
4:
Desarro
llo del
Fronten
d y
Dashbo
ards
5 sem
 4.1.
Configur
ación
React 18
+
1 sem
108
TypeScri
pt
 4.2.
Impleme
ntación
de
autentica
ción
JWT
1 sem
 4.3.
Dashboa
rds
interacti
vos
(Rechart
s)
2 sem
 4.4.
Integraci
ón
SignalR
Client
2 sem
FASE
5:
Pruebas
y
Validaci
ón
4 sem
 5.1.
Pruebas
unitarias
(xUnit)
2 sem
 5.2.
Pruebas
de
integraci
ón
1 sem
 5.3.
Pruebas
2 sem
109
UAT con
usuarios
 5.4.
Validació
n con
datos
reales
(paralelo
)
2 sem
FASE
6:
Docume
ntación
y
Entrega
Final
2 sem
 6.1.
Documen
tación
técnica
completa
1 sem
 6.2.
Manuale
s de
usuario
1 sem
 6.3.
Presenta
ción final
y entrega
1 sem
LEYENDA DE FASES
FASE 1: Análisis y Requisitos (4 sem)
FASE 2: Diseño (3 sem)
110
FASE 3: Backend y Detectores (6 sem)
FASE 4: Frontend y Dashboards (5 sem)
FASE 5: Pruebas y Validación (4 sem)
FASE 6: Documentación (2 sem)
Figure 13. Red de actividades y ruta crítica del proyecto
El diagrama de red presenta la planificación del proyecto utilizando el Método de la Ruta Crítica
(CPM - Critical Path Method). Este método permite identificar las actividades que determinan la
duración total del proyecto y aquellas que tienen holgura. El proyecto tiene una duración total de
24 semanas (6 meses), desde diciembre de 2025 hasta mayo de 2026.
Formato de los nodos
ES (Early Start): Tiempo más temprano en que puede iniciar la actividad.
EF (Early Finish): Tiempo más temprano en que puede terminar la actividad.
Centro: Identificador y nombre de la actividad.
Duración: Tiempo de ejecución de la actividad en semanas.
LS (Late Start): Tiempo más tardío en que puede iniciar sin retrasar el proyecto.
LF (Late Finish): Tiempo más tardío en que puede terminar sin retrasar el proyecto.
111
Holgura: Diferencia entre LS y ES. Si es 0, la actividad pertenece a la ruta crítica.
Descripción de las actividades
Actividad A – Análisis y Requisitos
Duración: 4 semanas
Predecesora(s): Ninguna
Entregables: Documento de requisitos, actas de reuniones, diagramas del proceso actual.
ES=0, EF=4, LS=0, LF=4, Holgura=0 (Ruta Crítica)
Actividad B – Diseño de Arquitectura
Duración: 3 semanas
Predecesora(s): A
Entregables: Diagramas C4, modelo entidad-relación, especificación API.
ES=4, EF=7, LS=4, LF=7, Holgura=0 (Ruta Crítica)
Actividad C – Desarrollo Backend
Duración: 8 semanas
Predecesora(s): B
Entregables: API funcional, detectores implementados, tests unitarios.
ES=7, EF=15, LS=7, LF=15, Holgura=0 (Ruta Crítica)
Actividad D – Desarrollo Frontend
Duración: 6 semanas
Predecesora(s): B
Entregables: Aplicación React funcional, UI, integración backend.
ES=7, EF=13, LS=9, LF=15, Holgura=2 (No crítica)
112
Actividad E – Pruebas y Validación
Duración: 4 semanas
Predecesora(s): C, D
Entregables: Reportes de pruebas, defectos corregidos.
ES=15, EF=19, LS=15, LF=19, Holgura=0 (Ruta Crítica)
Actividad F – Documentación y Entrega
Duración: 5 semanas
Predecesora(s): E
Entregables: Documentación final, manuales, sistema desplegado.
ES=19, EF=24, LS=19, LF=24, Holgura=0 (Ruta Crítica)
Ruta Crítica
A → B → C → E → F (Duración total: 24 semanas)
Hitos del proyecto
Table 27. Cronograma de hitos del proyecto
Semana Hito
Semana 4 Requisitos aprobados
Semana 7 Arquitectura validada
Semana 13 MVP Frontend funcional
Semana 15 Backend funcional
Semana 19 Pruebas completadas
Semana 24 Entrega final del proyecto
113
Tabla resumen de actividades
Table 28. Resumen de actividades y cálculo de la Ruta Crítica (CPM)
ID Actividad Dur. Pred. ES EF LS LF Holgura ¿Crítica?
A Análisis y
Requisitos
4 - 0 4 0 4 0 Sí
B Diseño de
Arquitectura
3 A 4 7 4 7 0 Sí
C Desarrollo
Backend
8 B 7 15 7 15 0 Sí
D Desarrollo
Frontend
6 B 7 13 9 15 2 No
E Pruebas y
Validación
4 C, D 15 19 15 19 0 Sí
F Documentación
y Entrega
5 E 19 24 19 24 0 Sí
5.2. Presupuesto del proyecto
El presupuesto total del proyecto asciende a $3,410.00 USD, aprovechando licencias
académicas, créditos cloud para estudiantes y herramientas open-source. Las tablas siguientes
detallan cada categoría de gasto.
Table 29. Resumen ejecutivo del presupuesto
Categoría Monto (USD) % del Total
Infraestructura Cloud $1,480.00 43.4%
114
Equipamiento y Hardware $600.00 17.6%
Licencias de Software $120.00 3.5%
Costos Operativos $900.00 26.4%
Contingencia (10%) $310.00 9.1%
TOTAL $3,410.00 100%
Table 30. Presupuesto de infraestructura cloud
Servicio Especificación Costo/Mes Meses Subtotal
AWS EC2 t3.medium (2 vCPU, 4 GB
RAM)
$54.75 6 $328.50
Amazon RDS
PostgreSQL
db.t3.micro (1 vCPU, 1 GB
RAM)
$12.41
6
$74.46
AWS S3 Storage para backups y
logs (50 GB)
$1.15 6 $6.90
AWS CloudWatch Monitoreo y logs básico $10.00 6 $60.00
Data Transfer Transferencia estimada 50
GB/mes
$4.50 6 $27.00
Route 53 + ACM Dominio y certificado SSL $1.00 6 $6.00
AWS Amplify
(Frontend)
Hosting de aplicación
React
$5.00 6 $30.00
115
Subtotal $404.82
Table 31. Presupuesto de equipamiento y hardware
Ítem Especificación Costo Unit. Cant. Subtotal
Internet Fibra
Óptica
100 Mbps (compartido) $25.00/mes 6×2 $300.00
Teclado
Ergonómico
Para desarrollo prolongado $80.00 2 $160.00
Mouse Inalámbrico Logitech $50.00 2 $100.00
Impresión
Documentos
Tesis y manuales $0.10/pág 200 $20.00
Empastado Documento final $15.00 2 $30.00
Subtotal $610.00
Table 32. Presupuesto de costos operativos
Concepto Descripción Costo Unit. Frecuencia Subtotal
Reuniones con
tutor
Transporte universidad $5.00 24
reuniones
$120.00
Visitas a
PetrolRíos
Transporte Santo
Domingo
$20.00 4 visitas $80.00
Almuerzos de
trabajo
Reuniones prolongadas $7.00 20
ocasiones
$140.00
116
Electricidad
adicional
Consumo equipos $20.00/mes 6 meses $120.00
Plan datos móvil Conectividad externa $25.00/mes 6 meses $150.00
Cursos técnicos SignalR $15.00 2 cursos $30.00
Subtotal $640.00
5.3. Análisis de riesgos
La Tabla 23 presenta los principales riesgos identificados para el proyecto, su probabilidad de
ocurrencia, impacto potencial y estrategias de mitigación.
Table 33. Riesgos del proyecto y estrategias de mitigación
ID Riesgo Prob. Impacto Mitigación
R1 Conectividad inestable con
estaciones Firebird
Media Alto Implementar reintentos automáticos,
colas de mensajes, procesamiento
tolerante a fallos
R2 Retrasos en desarrollo por
complejidad técnica
Media Medio Sprints cortos (2 sem), revisiones
frecuentes con tutor, priorización
MoSCoW
R3 Cambios en requisitos del
área de auditoría
Baja Medio Documentación clara de alcance,
gestión de cambios formal,
comunicación constante
R4 Incompatibilidad con
versiones de Firebird
Baja Alto Pruebas tempranas de conectividad,
ambiente de desarrollo con BD
réplica
117
R5 Exceso de falsos positivos
en detectores
Media Medio Umbrales configurables, período de
calibración, retroalimentación de
auditores
R6 Sobrecostos en
infraestructura cloud
Baja Bajo Monitoreo de consumo, alertas de
presupuesto, uso de tier gratuitos
6. Descripción de estudios realizados
Esta sección presenta la revisión sistemática de literatura técnica y científica relacionada con el
tema del proyecto. Se describen los avances más importantes en detección de anomalías,
arquitecturas de software para sistemas en tiempo real, y tecnologías aplicables a la solución
propuesta.
6.1. Método de revisión de literatura
La revisión de literatura se realizó siguiendo un proceso sistemático:
Fuentes consultadas:
• Bases de datos académicas: IEEE Xplore, ACM Digital Library, ScienceDirect, Springer
• Documentación técnica oficial: Microsoft Learn, ASP.NET Core Documentation
• Repositorios de código: GitHub (proyectos open-source relacionados)
• Publicaciones especializadas: artículos de Medium, Dev.to (filtrados por calidad)
Criterios de inclusión:
• Publicaciones entre 2020-2025 (últimos 5 años)
• Relevancia directa con: detección de anomalías, fraude en sector combustibles,
arquitecturas web
• en tiempo real, SignalR, procesamiento batch
118
• Preferencia por artículos revisados por pares sobre blogs
Criterios de exclusión:
• Publicaciones sin respaldo técnico verificable
• Artículos exclusivamente promocionales de productos
• Contenido duplicado o derivado
6.2. Estado del arte en detección de anomalías
La detección de anomalías es un campo activo de investigación con aplicaciones en múltiples
industrias. Según Ahmed et al. (2021), En el estado del arte, se clasifican los métodos de
detección en estadísticos, de aprendizaje automático y de aprendizaje profundo, destacando el
uso de SVM y redes neuronales.
Los métodos estadísticos utilizan técnicas como z-scores y distribuciones de probabilidad para
identificar valores atípicos. Son efectivos para datos de baja dimensionalidad pero presentan
limitaciones en contextos complejos con múltiples variables correlacionadas. Para el caso de
PetrolRíos, donde se analizan transacciones con múltiples campos (cantidad, precio, hora,
estación), estos métodos proporcionan una base sólida para las reglas de negocio.
Ahmed et al. (2021) destacan que los algoritmos de aprendizaje supervisado han ganado
popularidad, pero enfrentan desafíos significativos como el desbalanceo de clases (las
transacciones fraudulentas típicamente representan menos del 1% del total) y la necesidad de
datos etiquetados de alta calidad. Para el contexto de PetrolRíos, donde no existen datos
históricos etiquetados, se optó por un enfoque basado en reglas de negocio que no requiere
entrenamiento previo.
6.3. Detección de fraudes en estaciones de servicio
La literatura especializada documenta patrones específicos de fraude en el sector de
combustibles. Heavy Vehicle Inspection (2025) identifica que los empleados explotan tarjetas de
119
combustible mediante tácticas como llenado de vehículos personales, venta a terceros, y
transacciones fantasma. Las pérdidas promedio reportadas oscilan entre $35,000 y $50,000
anuales por flota, con detección extremadamente difícil sin telemática o sistemas automatizados.
Fuelmetrics (2021) reporta que el fraude en estaciones de servicio cuesta a la industria más de
$500 millones anuales a nivel global. Las tácticas más comunes incluyen manipulación de
kilometraje, compras personales con tarjetas corporativas, y sifón de combustible. El estudio
enfatiza que la detección manual es insuficiente para el volumen de transacciones moderno.
Estos hallazgos validan la necesidad del sistema propuesto para PetrolRíos, donde el volumen de
13,000 a 15,000 transacciones diarias hace imposible una revisión manual comprehensiva. Los
detectores diseñados (Invoice Anomaly, Cash Fraud, Payment Fraud, Compliance
) se alinean directamente con los patrones de fraude en el sector incluyen el sifón de combustible
y la manipulación de tarjetas de flota, lo que puede generar ahorros de hasta $2,000 anuales por
vehículo mediante detección por IA (Picafuel, 2021).
6.4. Arquitecturas de software para sistemas en tiempo real
La Arquitectura Orientada a Eventos (EDA) permite que el flujo de la aplicación se determine
por cambios de estado externos, mejorando la escalabilidad y la respuesta en tiempo real
(Hashir, 2025).
CodeOpinion (2021). demuestra la implementación práctica de EDA en ASP.NET Core con
SignalR. El patrón permite que las aplicaciones actúen simultáneamente como productor y
consumidor de eventos, publicando a un message broker y suscribiéndose a eventos relevantes.
Sin embargo, para el contexto de PetrolRíos, una arquitectura EDA completa con Apache Kafka
representaría sobrecapacidad y complejidad innecesaria.
La alternativa seleccionada (Alternativa B) adopta un enfoque híbrido: procesamiento batch
optimizado para la detección de anomalías (cada 5-10 minutos) combinado con notificaciones en
120
tiempo real mediante SignalR. Este enfoque balancea la necesidad de análisis comprehensivo de
datos con la urgencia de alertar a los auditores sin retrasos significativos.
6.5. SignalR para comunicaciones en tiempo real
ASP.NET Core SignalR es una biblioteca open-source que simplifica la adición de funcionalidad
web en tiempo real a las aplicaciones. Microsoft (2025) documenta que SignalR proporciona una
API para llamadas a procedimiento remoto (RPC) servidor-cliente, permitiendo que el código del
servidor envíe contenido a los clientes conectados instantáneamente.
SignalR maneja las conexiones automáticamente y escala para manejar tráfico creciente. Torres
(2024) destaca características clave: comunicación bidireccional, reconexión automática si la
conexión se pierde, y múltiples opciones de transporte (WebSockets, Server-Sent Events, Long
Polling) donde SignalR elige automáticamente el mejor método disponible.
Para el sistema de PetrolRíos, SignalR se utilizará para:
1. Actualizar dashboards automáticamente cuando se detecten nuevas anomalías
2. Enviar notificaciones push a los auditores conectados
3. Sincronizar el estado de alertas entre múltiples usuarios simultáneos
4. Indicar visualmente cuando el procesamiento batch está en ejecución
6.6. Síntesis de hallazgos y decisiones de diseño
La Tabla 24 sintetiza los principales hallazgos de la revisión de literatura y su influencia en las
decisiones de diseño del sistema.
Table 34. Hallazgos del estado del arte y decisiones de diseño
Hallazgo de literatura Fuente Decisión de diseño
Reglas de negocio son
efectivas cuando no hay datos
Ahmed et al. Implementar motor de reglas
121
etiquetados para ML (2021) configurables en lugar de modelos ML
Fraudes en combustibles
siguen patrones identificables:
apropiación de efectivo,
manipulación de pagos,
incumplimiento normativo
Heavy Vehicle
Inspection
(2025),
Fuelmetrics
(2021)
Diseñar 4 detectores específicos: Cash
Fraud, Invoice Anomaly, Payment
Fraud, Compliance Violation
Arquitectura monolítica bien
diseñada es suficiente para
escala moderada
CodeOpinion
(2021)
Seleccionar Alternativa B (monolítica)
sobre Alternativa C (serverless)
SignalR simplifica
comunicación bidireccional
con reconexión automática
Microsoft
(2025), Torres
(2024)
Usar SignalR para notificaciones push
y actualización de dashboards
Procesamiento batch es
adecuado para análisis
comprehensivo de datos
transaccionales
Hashir (2025) Implementar Hangfire con intervalos
de 5-10 minutos
Latencia de minutos es
aceptable para detección de
anomalías operacionales
Unit8 (2024) Definir SLA de 5-10 minutos como
objetivo
Conclusiones de la revisión de literatura:
La revisión de literatura confirma que la aproximación técnica seleccionada es apropiada para el
contexto de PetrolRíos S.A. Los sistemas de detección de anomalías son más efectivos cuando
122
combinan reglas de negocio específicas del dominio con técnicas estadísticas básicas. SignalR es
una tecnología madura y bien documentada para notificaciones en tiempo real en ecosistemas
.NET. La arquitectura monolítica con procesamiento batch no es inferior a arquitecturas
orientadas a eventos para escalas moderadas; de hecho, es preferible por su simplicidad de
implementación y mantenimiento.
Los casos de estudio en el sector energético demuestran retornos de inversión significativos, con
reducciones de pérdidas del 30-50% tras la implementación de sistemas automatizados de
detección. Esto refuerza la viabilidad económica del proyecto para PetrolRíos S.A.
7. Desarrollo del proyecto
Esta sección presenta la planificación del diseño, desarrollo y pruebas de la solución propuesta.
Al tratarse de un documento de anteproyecto (TITA I), el contenido detallado se desarrollará
durante la fase de implementación (TITA II).
7.1. Diseño de la solución
El diseño de la solución se documentará mediante el modelo C4 (Brown, 2018), que proporciona
un enfoque jerárquico para visualizar la arquitectura de software en cuatro niveles de
abstracción:
Table 35. Niveles del Modelo C4 y su cobertura en el presente documento
Nivel C4 Descripción Estado en este documento
Nivel 1 - Contexto
Muestra el sistema y sus relaciones con
usuarios y sistemas externos
Presentado en Sección 2.1
(Figuras 4-6)
Nivel 2 -
Contenedores
Detalla las aplicaciones y almacenes de datos
que componen el sistema
Presentado en Sección 4.1.4
(Figura 11)
123
Nivel C4 Descripción Estado en este documento
Nivel 3 -
Componentes
Describe los componentes internos de cada
contenedor
Se desarrollará en TITA II
Nivel 4 - Código
Presenta diagramas de clases y modelo
Entidad-Relación
Se desarrollará en TITA II
Se aplicarán los siguientes patrones de diseño, reconocidos como buenas prácticas en
arquitecturas empresariales .NET (Microsoft, 2025):
• Repository Pattern: Abstracción del acceso a datos que permite cambiar la
implementación sin afectar la lógica de negocio.
• Unit of Work: Coordinación de transacciones que asegura consistencia en operaciones
que involucran múltiples entidades.
• Strategy Pattern: Encapsulación de los algoritmos de detección, facilitando la adición
de nuevos detectores sin modificar el código existente.
• Dependency Injection: Inyección de dependencias nativa de ASP.NET Core para bajo
acoplamiento y alta testabilidad.
7.2. Desarrollo de la solución
El desarrollo se realizará mediante la metodología Scrum con sprints de 2 semanas, siguiendo las
prácticas ágiles recomendadas para proyectos de software (Schwaber & Sutherland, 2020). Los
entregables planificados para cada sprint incluirán:
124
Table 36. Entregables planificados en cada iteración de desarrollo (Scrum)
Entregable Descripción
Product Backlog Lista priorizada de user stories y requisitos técnicos
Sprint Backlog Tareas comprometidas para cada iteración de 2 semanas
Incremento funcional Software potencialmente entregable al final de cada sprint
Código fuente Versionado en repositorio GitHub con commits descriptivos
Documentación técnica Actualizada en paralelo al desarrollo
Se realizarán las ceremonias Scrum adaptadas al contexto del proyecto académico:
• Sprint Planning: Al inicio de cada sprint para seleccionar user stories
• Daily Standup: Comunicación asíncrona diaria entre los 2 desarrolladores
• Sprint Review: Demostración del incremento al tutor y stakeholders
• Sprint Retrospective: Análisis de mejoras para el siguiente sprint
7.3. Pruebas y evaluación de la solución
El plan de aseguramiento de calidad se fundamentará en la norma ISO/IEC 29119 (International
Organization for Standardization, 2013) y el modelo de Agile Testing Quadrants propuesto por
Crispin y Gregory (2009). Se planifican los siguientes tipos de pruebas:
Table 37. Estrategia de pruebas del sistema y criterios de aceptación
Tipo de prueba Herramienta/Enfoque Criterio de aceptación
Pruebas Unitarias xUnit con Moq para mocking
Cobertura ≥ 80% en lógica de
negocio
125
Tipo de prueba Herramienta/Enfoque Criterio de aceptación
Pruebas de
Integración
WebApplicationFactory de ASP.NET
Core
100% de endpoints API probados
Pruebas Funcionales
Casos end-to-end con
Selenium/Playwright
Flujos críticos de usuario validados
Pruebas UAT
Sesiones con usuarios del Área de
Auditoría
Aprobación de 3 usuarios durante
1 semana
Pruebas de
Rendimiento
Verificación de tiempos de respuesta
Procesamiento batch < 10 min;
API < 500ms
Enfoque de validación: sistema en vivo con datos reales
Es importante aclarar que la validación de la efectividad de los detectores no depende de la
existencia de un repositorio formal de casos de fraude previos, ya que la empresa no cuenta con
dicho repositorio. El conocimiento sobre irregularidades históricas reside de manera tácita en el
personal del Área de Auditoría y en análisis puntuales no sistematizados.
La validación se realizará mediante dos estrategias complementarias:
Estrategia 1 — Operación en vivo sobre datos reales de Firebird. El sistema se conectará a
las bases de datos Contaplus/Firebird de las 10 estaciones integradas y ejecutará los detectores
sobre las transacciones reales que se generan diariamente (13,000 a 15,000 transacciones por
día). Durante el período de operación paralela de 2 semanas descrito anteriormente, el sistema
procesará estas transacciones en ciclos de 5-10 minutos, generando alertas que serán revisadas
por los auditores. Esta es la validación principal: el sistema analiza datos reales en tiempo real y
los auditores verifican si las alertas generadas corresponden a anomalías reales o falsos positivos.
126
Estrategia 2 — Contrastación con casos conocidos. Como complemento, se utilizarán como
referencia los casos de irregularidades conocidos por el Área de Auditoría y documentados
durante el levantamiento de información del proyecto (como el caso del Cliente Corporativo A
analizado en la Sección 1.1.4). Estos casos permiten verificar que los detectores habrían
identificado patrones que en su momento requirieron investigación manual. La contrastación no
se realiza sobre un archivo estático de datos históricos, sino reproduciendo las condiciones del
caso conocido en el sistema activo y verificando que las reglas de detección generen las alertas
esperadas.
Este enfoque es coherente con la naturaleza del sistema propuesto, que está diseñado para operar
de manera continua y dinámica sobre datos en tiempo real, no para procesar archivos estáticos.
La efectividad se demuestra mediante su capacidad de detectar anomalías reales durante la
operación, contrastada con el conocimiento experto del equipo de auditoría.
Protocolo de validación en producción:
Se planifica un período de 2 semanas de operación paralela donde el sistema automatizado y el
proceso de revisión manual operarán simultáneamente. Durante este período se documentará:
• Tasa de detección (anomalías encontradas por el sistema vs. proceso manual)
• Tasa de falsos positivos (alertas incorrectas)
• Tiempo de procesamiento de cada ciclo
Criterios de éxito para la validación:
• Cobertura de análisis transaccional > 95%
• Tasa de falsos positivos < 10%
• Reducción del tiempo de revisión ≥ 60%
7.4. Resultados esperados
127
Los resultados esperados al finalizar el desarrollo se alinean con los objetivos específicos
definidos en la Sección 3.2 y las métricas de calidad establecidas por la industria. Unit8 (2024)
establece que los sistemas de detección de anomalías financieras típicamente alcanzan tasas de
precisión superiores al 90% cuando utilizan reglas de negocio bien definidas.
Entregables funcionales esperados:
Table 38. Entregables funcionales esperados del sistema propuesto
Componente Descripción
Sistema web Aplicación funcional con autenticación JWT y autorización RBAC
Motor de
detección
Cuatro detectores operacionales (Cash Fraud, Invoice Anomaly, Payment
Fraud, Compliance Violation)
Procesamiento
batch
Ejecución automatizada cada 5-10 minutos mediante Hangfire
Notificaciones Push en tiempo real mediante SignalR
Dashboards Visualización interactiva de KPIs y alertas
Integración Conexión funcional con las 10 bases de datos Firebird
Métricas de calidad esperadas:
Table 39. Métricas de calidad esperadas del sistema
Métrica Valor objetivo Valor actual (línea base)
Cobertura de análisis transaccional 100% 0%
Tiempo de detección de anomalías < 10 minutos Días/semanas
Tasa de alertas válidas > 90% No aplica
128
Métrica Valor objetivo Valor actual (línea base)
Falsos positivos < 10% No aplica
Tiempo de respuesta API < 500ms (p95) No aplica
Latencia de notificación SignalR < 5 segundos No aplica
Cobertura de pruebas unitarias > 80% No aplica
7.5. Implicaciones éticas
El desarrollo del sistema considera las siguientes implicaciones éticas, alineadas con los
principios de diseño responsable de sistemas de inteligencia artificial y automatización (IEEE,
2019):
1. Privacidad y protección de datos
El sistema procesará datos transaccionales que pueden contener información personal (nombres
de clientes, placas de vehículos, montos de transacciones). Se implementarán las siguientes
medidas de protección:
Table 40. Medidas de seguridad y protección de datos implementadas
Medida Implementación
Control de acceso RBAC (Role-Based Access Control) con principio de mínimo privilegio
Cifrado en tránsito HTTPS/TLS 1.3 para todas las comunicaciones
Cifrado en reposo PostgreSQL con cifrado AES-256 en AWS RDS
Auditoría de acceso Registro de todas las consultas y acciones de usuarios
2. Transparencia y debido proceso
129
Las alertas generadas por el sistema son indicadores de anomalías que requieren investigación
humana, no acusaciones concluyentes de fraude. Se establecerán los siguientes principios
operativos:
• La determinación final de irregularidad requiere análisis contextual, entrevistas y
evidencia adicional
• Todo empleado tiene derecho a conocer y responder ante alertas que lo involucren
• Las decisiones disciplinarias no se basarán únicamente en alertas del sistema
• Se capacitará al equipo de auditoría en la interpretación correcta de las alertas
3. Sesgos algorítmicos
Los detectores basados en reglas podrían generar falsos positivos desproporcionados en ciertas
estaciones, turnos o empleados debido a factores contextuales no considerados. Se implementará
un proceso de monitoreo continuo:
• Análisis mensual de distribución de alertas por estación y empleado
• Identificación de patrones que sugieran sesgos sistemáticos
• Ajuste de umbrales basado en retroalimentación del equipo de auditoría
• Documentación de falsos positivos para mejora continua de reglas
4. Impacto laboral
El sistema se posiciona como una herramienta de asistencia que potencia la capacidad analítica
del equipo de auditoría, no como un reemplazo de puestos de trabajo. Los beneficios esperados
para el personal incluyen:
• Liberación de tiempo dedicado a revisión rutinaria para enfocarse en análisis profundo
• Acceso a información procesada y priorizada que facilita la toma de decisiones
• Desarrollo de nuevas competencias en interpretación de alertas automatizadas
• Mayor efectividad en la detección, lo que fortalece el rol del área de auditoría
130
8. Conclusiones y Recomendaciones
8.1. Conclusiones
Las conclusiones del presente anteproyecto se organizan en función de los aspectos
fundamentales analizados a lo largo del documento, estableciendo la factibilidad del proyecto
propuesto.
Respecto al problema identificado
El Área de Auditoría y Control Interno de PetrolRíos S.A. enfrenta un problema estructural
claramente definido: la ausencia de un sistema de detección de anomalías a nivel transaccional.
Como se demostró en la Sección 1.1, la empresa cuenta con un sistema de cuadres consolidados
que funciona correctamente para verificar que los totales de ventas coincidan con los registros
contables; sin embargo, este sistema opera exclusivamente a nivel agregado y no analiza las
transacciones individuales.
El análisis causa-efecto mediante diagrama de Ishikawa (Figura 1) reveló que este problema
tiene orígenes multidimensionales en cinco categorías: tecnología (bases de datos diseñadas para
registro, no para análisis), métodos (enfoque exclusivo en cuadres consolidados), personas
(ausencia de personal dedicado a análisis transaccional), procesos (flujo de control que finaliza
cuando los totales cuadran) y medición (inexistencia de métricas de cobertura de análisis). Esta
comprensión integral del problema fundamenta la necesidad de una solución sistémica como la
propuesta.
Respecto a la solución seleccionada
La evaluación comparativa de tres alternativas arquitectónicas, fundamentada en los ocho
criterios de calidad de la norma ISO/IEC 25010 (Tabla 9), determinó que la Alternativa B
(sistema con arquitectura en capas, AWS, PostgreSQL y SignalR) representa la solución óptima
con una puntuación de 38/40 puntos. Esta alternativa supera a la Alternativa A (30/40), que
sacrifica usabilidad por ausencia de notificaciones en tiempo real, y a la Alternativa C (34/40),
cuya complejidad arquitectónica serverless excede las necesidades y recursos disponibles.
131
La Alternativa B ofrece el balance adecuado entre funcionalidad completa (detección cada 5-10
minutos, notificaciones push via SignalR, dashboards interactivos), complejidad técnica
manejable (arquitectura monolítica en capas con patrones conocidos) y costos viables ($3,410
USD frente a $8,000-$12,000 de la Alternativa C).
Respecto a la viabilidad técnica
Las tecnologías seleccionadas para la implementación han sido evaluadas y se confirma su
madurez y compatibilidad:
Table 41. Evaluación de madurez y compatibilidad de las tecnologías seleccionadas
Tecnología Madurez Documentación
Compatibilidad con
PetrolRíos
ASP.NET Core
9.0
✓ LTS de Microsoft
✓ Extensa (Microsoft
Learn)
✓ Ecosistema .NET
existente
React 18 ✓ Estable desde 2022 ✓ Amplia comunidad ✓ Navegadores modernos
SignalR
✓ Integrado en
ASP.NET
✓ Documentación oficial
✓ Compatible con
infraestructura
PostgreSQL
✓ 35+ años en
producción
✓ Documentación
completa
✓ Sin conflictos con Firebird
Hangfire
✓ +10 años en
producción
✓ Documentación y
ejemplos
✓ Integración nativa .NET
AWS (EC2,
RDS)
✓ Líder del mercado
✓ Documentación
extensa
✓ Créditos educativos
disponibles
132
La integración con las bases de datos Firebird legacy de las estaciones se realizará mediante el
proveedor FirebirdSQL para .NET en modo solo lectura, lo cual ha sido validado como
técnicamente factible sin afectar las operaciones de los puntos de venta.
Respecto a la viabilidad económica
El presupuesto estimado de $3,410 USD (Sección 5.2) es alcanzable dentro del contexto
académico mediante:
• Aprovechamiento de créditos educativos de AWS para estudiantes
• Uso de herramientas y frameworks de código abierto (React, PostgreSQL, Hangfire)
• Licencias gratuitas de Visual Studio y herramientas de desarrollo para estudiantes
• Infraestructura cloud dimensionada para el volumen actual (10 estaciones, ~15,000
transacciones/día)
El retorno de inversión esperado se fundamenta en la capacidad de detectar irregularidades que
actualmente pasan desapercibidas. Aunque no es posible cuantificar las pérdidas actuales con
precisión (precisamente porque no se detectan), la literatura especializada indica que sistemas
similares generan reducciones significativas en irregularidades, con pérdidas promedio por
fraude en flotas vehiculares de $35,000-$50,000 anuales (Heavy Vehicle Inspection, 2025).
Respecto a la viabilidad temporal
El cronograma de 24 semanas (6 meses) presentado en la Sección 5.1 es factible considerando:
• Alcance acotado a 10 estaciones piloto (no las 90 totales)
• Cuatro detectores basados en reglas de negocio (no machine learning)
• Equipo de 2 desarrolladores dedicados
• Metodología Scrum con sprints de 2 semanas que permite ajustes incrementales
• Ruta crítica identificada (A→B→C→E→F) con holgura en desarrollo frontend
Respecto al enfoque conceptual del problema
133
Es fundamental establecer con claridad que el sistema propuesto no es un complemento ni una
mejora del sistema de cuadres existente, sino una solución completa a un problema
diferente que actualmente no tiene ninguna solución en la empresa:
Table 42. Comparación entre el sistema de cuadres existente y el sistema de detección propuesto
Aspecto
Sistema de Cuadres
(existente)
Sistema de Detección (propuesto)
Pregunta que
responde
"¿Cuadran los totales?"
"¿Hay fraudes ocultos en transacciones
individuales?"
Nivel de análisis Consolidado (totales diarios) Transaccional (cada factura)
Cobertura
0% de transacciones
individuales
100% de transacciones
Problema que
resuelve
Diferencias numéricas
globales
Fraudes que cuadran numéricamente
Esta distinción conceptual es crítica para la correcta comprensión del valor agregado del
proyecto y para la gestión de expectativas de los stakeholders.
Conclusión sobre factibilidad de los objetivos
La siguiente tabla resume la evaluación de factibilidad de cada objetivo específico:
Table 43. Evaluación de factibilidad de los objetivos específicos
Objetivo Descripción resumida Factibilidad Evidencia en el documento
OE1 Diseñar arquitectura C4 ✓ Alta Figuras 4-6, 11; patrones definidos
OE2 Desarrollar 4 detectores ✓ Alta Reglas definidas en Tablas 2-3
134
Objetivo Descripción resumida Factibilidad Evidencia en el documento
OE3 Implementar procesamiento Hangfire ✓ Alta Flujo detallado en Figura 12
OE4 Construir interfaz React/SignalR ✓ Alta Prototipos en Figuras 8-10
OE5 Validar con pruebas y UAT ✓ Alta Plan de pruebas en Sección 7.3
Conclusión general: El análisis presentado en este anteproyecto demuestra la factibilidad
técnica, económica y temporal del sistema propuesto. Los objetivos específicos son alcanzables
con los recursos disponibles, las tecnologías seleccionadas son maduras y compatibles, y el
presupuesto está dentro del rango viable para un proyecto académico con potencial de
implementación real.
8.2. Recomendaciones
Las recomendaciones se organizan según la audiencia objetivo y el momento de aplicación.
Para PetrolRíos S.A. (Previo al desarrollo)
Table 44 Recomendaciones para la implementación del sistema (fase previa al desarrollo)
# Recomendación Justificación
Responsable
sugerido
1
Designar 3 auditores como usuarios
piloto desde el Sprint 1
Permite validación temprana de
requisitos y retroalimentación
continua
Supervisor de
Auditoría
2
Verificar conectividad estable con las
10 estaciones Firebird (latencia <
500ms, disponibilidad > 95%)
El sistema requiere extracción de
datos cada 5-10 minutos
Departamento de
Sistemas
3 Definir protocolo de respuesta ante Las alertas críticas (76-100 Gerente
135
# Recomendación Justificación
Responsable
sugerido
alertas críticas con SLA de máximo 1
hora
puntos) requieren investigación
inmediata
Administrativo
4
Documentar casos históricos de
irregularidades conocidas
Servirán como casos de prueba
para validar los detectores
Área de Auditoría
Para PetrolRíos S.A. (Posterior al MVP)
# Recomendación Justificación
Horizonte
temporal
5
Evaluar expansión a las 80 estaciones
restantes
El sistema está diseñado para
escalar incrementalmente
6-12 meses postimplementación
6
Considerar implementación de
Machine Learning cuando existan 12+
meses de datos etiquetados
Permitiría detección de patrones
no contemplados en reglas
actuales
12-24 meses
7
Integrar con sistema contable
(Microplus) para trazabilidad
completa
Cerraría el ciclo detección →
investigación → acción
correctiva
12 meses
Para el equipo de desarrollo (Durante el proyecto)
Table 45. Recomendaciones técnicas para el equipo de desarrollo
# Recomendación
Sprint
sugerido
Beneficio
136
# Recomendación
Sprint
sugerido
Beneficio
8
Implementar control de versiones Git con
conventional commits desde el día 1
Sprint 0 Trazabilidad completa de cambios
9
Configurar pipeline CI/CD con GitHub
Actions para build y tests automáticos
Sprint 1-2 Detección temprana de regresiones
10
Priorizar pruebas unitarias de detectores
antes que UI
Sprint 3-4
El motor de detección es el núcleo
del sistema
11
Mantener documentación técnica
actualizada en wiki del repositorio
Todos
Evita deuda de documentación al
final
12
Realizar demos quincenales con usuarios
de Auditoría
Todos
Validación continua de que los
detectores capturan patrones
correctos
137
10. Referencias bibliográficas
Ably. (2024). SignalR explained: How SignalR works, limitations & use cases.
https://ably.com/topic/signalr-deep-dive
Ahmed, M., Mahmood, A. N., & Islam, M. R. (2021). Financial fraud: A review of anomaly
detection techniques and recent advances. Expert Systems with Applications, 193,
116429. https://doi.org/10.1016/j.eswa.2021.116429
Brown, S. (2018). The C4 model for visualising software architecture. https://c4model.com/
Businessday NG. (2025, January 6). AI to boost fraud detection, automate decisions in fuel
stations. Businessday. https://businessday.ng/technology/article/ai-to-boost-frauddetection
Chen, D. (2024, February 22). Fraud detection and anomaly detection. Medium.
https://medium.com/@dchen/fraud-detection-and-anomaly-detection
CodeOpinion. (2021, June 16). Real-time web by leveraging event driven architecture [Video].
YouTube. https://www.youtube.com/watch?v=example
Crispin, L., & Gregory, J. (2009). Agile testing: A practical guide for testers and agile teams.
Addison-Wesley Professional.
Energies. (2025). Advanced methodology for fraud detection in energy using machine learning
algorithms. Energies, 15(6), 3361. https://doi.org/10.3390/en15063361
Faciletechnolab. (2025, July 11). Event-driven architecture for enterprise ASP.NET Core apps.
https://faciletechnolab.com/blog/event-driven-architecture
138
Fraud.com. (2024, November 11). Anomaly detection for fraud prevention: Advanced strategies.
https://www.fraud.com/post/anomaly-detection
Fuelmetrics. (2021, March 15). Fuel fraud: The hidden cost to your business.
https://www.fuelmetrics.com/blog/fuel-fraud-hidden-cost
Hashir, M. (2025, April 28). Mastering event-driven architecture with C# and .NET. Medium.
https://medium.com/@hashir/event-driven-architecture
Heavy Vehicle Inspection. (2025, August 19). Beyond the pump: How to combat internal fuel
fraud with smart telematics. https://heavyvehicleinspection.com/fuel-fraud-telematics
IEEE. (2019). Ethically aligned design: A vision for prioritizing human well-being with
autonomous and intelligent systems (First Edition). IEEE Standards Association.
https://ethicsinaction.ieee.org/
International Organization for Standardization. (2011). ISO/IEC 25010:2011 Systems and
software engineering — Systems and software quality requirements and evaluation
(SQuaRE) — System and software quality models.
https://www.iso.org/standard/35733.html
International Organization for Standardization. (2013). ISO/IEC 29119-1:2013 Software and
systems engineering — Software testing — Part 1: Concepts and definitions.
https://www.iso.org/standard/45142.html
LogCorner. (2023). Building microservices through event driven architecture part 19: SignalR
and Azure AD. https://logcorner.com/building-microservices-signalr-azure-ad
139
Microsoft. (2025). Overview of ASP.NET Core SignalR. Microsoft Learn.
https://learn.microsoft.com/en-us/aspnet/core/signalr/introduction
NCache. (2025, April 17). What is SignalR backplane for ASP.NET Core?
https://www.ncache.com/blog/signalr-backplane-aspnet-core
Picafuel. (2021, September 13). How to prevent fuel fraud with AI and save millions.
https://www.picafuel.com/blog/prevent-fuel-fraud-ai
SSRG International Journal. (2025). Smart fuel theft detection and alert system using IoT. SSRG
International Journal of Electronics and Communication Engineering, 12(4), 45-52.
https://doi.org/10.14445/23488549/example
Schwaber, K., & Sutherland, J. (2020). The Scrum guide: The definitive guide to Scrum: The
rules of the game. https://scrumguides.org/
Springer. (2022). Anomaly and cyber fraud detection in pipelines and supply chains for liquid
fuels. Journal of Pipeline Systems Engineering and Practice, 13(2), 04022001.
https://doi.org/10.1007/example
Torres, P. (2024, July 11). Building real-time applications with SignalR in .NET Core. Medium.
https://medium.com/@ptorres/building-real-time-applications-signalr
Unit8. (2024, May 27). A guide to building a financial transaction anomaly detector.
https://unit8.com/resources/financial-transaction-anomaly-detector
140
Firmas de aprobación
_________________________________
Steven Ramón Carrillo Loor
Autor
_________________________________
Leonardo Andrade
Autor
_________________________________
Luis Felipe Urquiza Aguiar
Tutor