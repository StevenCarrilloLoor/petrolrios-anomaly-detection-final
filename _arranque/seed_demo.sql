-- Datos sinteticos para forzar deteccion en el proximo ciclo del job de Hangfire.
-- Estacion 1 (EST-001). Inserta facturas, cierres de turno, depositos, anulaciones
-- y tarjetas que disparan las 4 categorias de detectores.

-- Limpieza de seed previos (idempotente) y de alertas para evitar acumulacion en demos repetidas.
TRUNCATE TABLE alertas RESTART IDENTITY CASCADE;
DELETE FROM transacciones_staging
WHERE "DataJson"::text LIKE '%DEMO-SEED%';

-- Variables: usamos NOW() menos unas horas para que la "fecha de documento" caiga
-- dentro de la ventana del watermark inicial (seed pone watermark a NOW()-1h).
-- Insertamos como "Fecha original" hace 10 min, suficientemente reciente.

-- ===========================================
-- Compliance Violation: placa ZZZ999949 con 8 galones (umbral 5)
-- ===========================================
INSERT INTO transacciones_staging
    ("EstacionId", "TipoTransaccion", "DataJson", "FechaOriginal", "Procesada", "CreatedAt", "UpdatedAt")
VALUES (
    1, 'Factura',
    jsonb_build_object(
        'SecuenciaDocumento', 100001,
        'TipoDocumento', 'FAC',
        'NumeroDocumento', '001-001-000100001',
        'FechaDocumento', to_char(NOW() AT TIME ZONE 'UTC' - INTERVAL '10 minutes', 'YYYY-MM-DD"T"HH24:MI:SS.MS"Z"'),
        'CodigoCliente', 'CLI-0001',
        'TotalNeto', 28.40,
        'TotalSinIva', 25.36,
        'Descuento', 0,
        'Iva', 3.04,
        'CodigoVendedor', 'EMP-007',
        'CodigoPago', 'EF',
        'Placa', 'ZZZ999949',
        'RucCliente', '1700000000001',
        'NumeroTurno', 9001,
        'Subtotal', 25.36,
        'NumeroConsecutivo', 1,
        'CodigoChofer', '',
        'CodigoManguera', 'M-01',
        'DemoSeed', 'DEMO-SEED'
    ),
    NOW() AT TIME ZONE 'UTC' - INTERVAL '10 minutes',
    false, NOW(), NOW()
);

-- Detalle del despacho asociado: 8 galones de Extra (gasolina)
INSERT INTO transacciones_staging
    ("EstacionId", "TipoTransaccion", "DataJson", "FechaOriginal", "Procesada", "CreatedAt", "UpdatedAt")
VALUES (
    1, 'DetalleFactura',
    jsonb_build_object(
        'NumeroDespacho', 700001,
        'CodigoManguera', 'M-01',
        'FechaDespacho', to_char(NOW() AT TIME ZONE 'UTC' - INTERVAL '10 minutes', 'YYYY-MM-DD"T"HH24:MI:SS.MS"Z"'),
        'VolumenTotal', 8.0,
        'Cantidad', 8.0,
        'ValorUnitario', 3.55,
        'CodigoProducto', 'EXTRA',
        'NombreProducto', 'Gasolina Extra',
        'CodigoCliente', 'CLI-0001',
        'DemoSeed', 'DEMO-SEED'
    ),
    NOW() AT TIME ZONE 'UTC' - INTERVAL '10 minutes',
    false, NOW(), NOW()
);

-- ===========================================
-- Compliance Violation: mismo vehiculo, mismo dia, diesel + extra
-- ===========================================
INSERT INTO transacciones_staging
    ("EstacionId", "TipoTransaccion", "DataJson", "FechaOriginal", "Procesada", "CreatedAt", "UpdatedAt")
VALUES (
    1, 'Factura',
    jsonb_build_object(
        'SecuenciaDocumento', 100002,
        'TipoDocumento', 'FAC',
        'NumeroDocumento', '001-001-000100002',
        'FechaDocumento', to_char(NOW() AT TIME ZONE 'UTC' - INTERVAL '20 minutes', 'YYYY-MM-DD"T"HH24:MI:SS.MS"Z"'),
        'CodigoCliente', 'CLI-0002',
        'TotalNeto', 45.10,
        'TotalSinIva', 40.27,
        'Descuento', 0, 'Iva', 4.83,
        'CodigoVendedor', 'EMP-003',
        'CodigoPago', 'TC',
        'Placa', 'PCQ1234',
        'RucCliente', '1700000000002',
        'NumeroTurno', 9001,
        'Subtotal', 40.27,
        'NumeroConsecutivo', 2,
        'CodigoChofer', '',
        'CodigoManguera', 'M-02',
        'DemoSeed', 'DEMO-SEED'
    ),
    NOW() AT TIME ZONE 'UTC' - INTERVAL '20 minutes',
    false, NOW(), NOW()
);

INSERT INTO transacciones_staging
    ("EstacionId", "TipoTransaccion", "DataJson", "FechaOriginal", "Procesada", "CreatedAt", "UpdatedAt")
VALUES (
    1, 'Factura',
    jsonb_build_object(
        'SecuenciaDocumento', 100003,
        'TipoDocumento', 'FAC',
        'NumeroDocumento', '001-001-000100003',
        'FechaDocumento', to_char(NOW() AT TIME ZONE 'UTC' - INTERVAL '15 minutes', 'YYYY-MM-DD"T"HH24:MI:SS.MS"Z"'),
        'CodigoCliente', 'CLI-0002',
        'TotalNeto', 30.00, 'TotalSinIva', 26.79, 'Descuento', 0, 'Iva', 3.21,
        'CodigoVendedor', 'EMP-003',
        'CodigoPago', 'TC',
        'Placa', 'PCQ1234',
        'RucCliente', '1700000000002',
        'NumeroTurno', 9001,
        'Subtotal', 26.79, 'NumeroConsecutivo', 3,
        'CodigoChofer', '',
        'CodigoManguera', 'M-05',
        'DemoSeed', 'DEMO-SEED'
    ),
    NOW() AT TIME ZONE 'UTC' - INTERVAL '15 minutes',
    false, NOW(), NOW()
);

INSERT INTO transacciones_staging
    ("EstacionId", "TipoTransaccion", "DataJson", "FechaOriginal", "Procesada", "CreatedAt", "UpdatedAt")
VALUES (
    1, 'DetalleFactura',
    jsonb_build_object(
        'NumeroDespacho', 700002, 'CodigoManguera', 'M-02',
        'FechaDespacho', to_char(NOW() AT TIME ZONE 'UTC' - INTERVAL '20 minutes', 'YYYY-MM-DD"T"HH24:MI:SS.MS"Z"'),
        'VolumenTotal', 12.7, 'Cantidad', 12.7, 'ValorUnitario', 3.17,
        'CodigoProducto', 'DIESEL', 'NombreProducto', 'Diesel Premium',
        'CodigoCliente', 'CLI-0002', 'DemoSeed', 'DEMO-SEED'
    ),
    NOW() AT TIME ZONE 'UTC' - INTERVAL '20 minutes',
    false, NOW(), NOW()
);

INSERT INTO transacciones_staging
    ("EstacionId", "TipoTransaccion", "DataJson", "FechaOriginal", "Procesada", "CreatedAt", "UpdatedAt")
VALUES (
    1, 'DetalleFactura',
    jsonb_build_object(
        'NumeroDespacho', 700003, 'CodigoManguera', 'M-05',
        'FechaDespacho', to_char(NOW() AT TIME ZONE 'UTC' - INTERVAL '15 minutes', 'YYYY-MM-DD"T"HH24:MI:SS.MS"Z"'),
        'VolumenTotal', 8.5, 'Cantidad', 8.5, 'ValorUnitario', 3.53,
        'CodigoProducto', 'EXTRA', 'NombreProducto', 'Gasolina Extra',
        'CodigoCliente', 'CLI-0002', 'DemoSeed', 'DEMO-SEED'
    ),
    NOW() AT TIME ZONE 'UTC' - INTERVAL '15 minutes',
    false, NOW(), NOW()
);

-- ===========================================
-- Cash Fraud: cierre de turno con faltante de $85 (umbral $50)
-- ===========================================
INSERT INTO transacciones_staging
    ("EstacionId", "TipoTransaccion", "DataJson", "FechaOriginal", "Procesada", "CreatedAt", "UpdatedAt")
VALUES (
    1, 'CierreTurno',
    jsonb_build_object(
        'NumeroTurno', 9001,
        'CodigoVendedor', 'EMP-007',
        'FechaInicio', to_char(NOW() AT TIME ZONE 'UTC' - INTERVAL '8 hours', 'YYYY-MM-DD"T"HH24:MI:SS.MS"Z"'),
        'FechaFin', to_char(NOW() AT TIME ZONE 'UTC' - INTERVAL '5 minutes', 'YYYY-MM-DD"T"HH24:MI:SS.MS"Z"'),
        'SaldoInicial', 100.00,
        'Ingresos', 850.00,
        'Egresos', 35.00,
        'SaldoFinal', 830.00,
        'Faltante', 85.00,
        'Sobrante', 0.00,
        'Creditos', 0.00,
        'DemoSeed', 'DEMO-SEED'
    ),
    NOW() AT TIME ZONE 'UTC' - INTERVAL '5 minutes',
    false, NOW(), NOW()
);

-- ===========================================
-- Invoice Anomaly: factura con placa vacia + monto significativo
-- ===========================================
INSERT INTO transacciones_staging
    ("EstacionId", "TipoTransaccion", "DataJson", "FechaOriginal", "Procesada", "CreatedAt", "UpdatedAt")
VALUES (
    1, 'Factura',
    jsonb_build_object(
        'SecuenciaDocumento', 100004,
        'TipoDocumento', 'FAC',
        'NumeroDocumento', '001-001-000100004',
        'FechaDocumento', to_char(NOW() AT TIME ZONE 'UTC' - INTERVAL '30 minutes', 'YYYY-MM-DD"T"HH24:MI:SS.MS"Z"'),
        'CodigoCliente', 'CLI-0003',
        'TotalNeto', 120.00, 'TotalSinIva', 107.14, 'Descuento', 0, 'Iva', 12.86,
        'CodigoVendedor', 'EMP-011',
        'CodigoPago', 'EF',
        'Placa', '',
        'RucCliente', '1700000000003',
        'NumeroTurno', 9002,
        'Subtotal', 107.14, 'NumeroConsecutivo', 4,
        'CodigoChofer', '', 'CodigoManguera', 'M-03',
        'DemoSeed', 'DEMO-SEED'
    ),
    NOW() AT TIME ZONE 'UTC' - INTERVAL '30 minutes',
    false, NOW(), NOW()
);

-- ===========================================
-- Invoice Anomaly: 6 anulaciones de un mismo empleado (>5% diario)
-- ===========================================
INSERT INTO transacciones_staging
    ("EstacionId", "TipoTransaccion", "DataJson", "FechaOriginal", "Procesada", "CreatedAt", "UpdatedAt")
SELECT
    1, 'Anulacion',
    jsonb_build_object(
        'NumeroAnulacion', 5000 + n,
        'TipoComprobante', 'FAC',
        'FechaAnulacion', to_char(NOW() AT TIME ZONE 'UTC' - (n * INTERVAL '7 minutes'), 'YYYY-MM-DD"T"HH24:MI:SS.MS"Z"'),
        'Establecimiento', '001',
        'PuntoEmision', '001',
        'SecuencialInicio', 200000 + n,
        'SecuencialFin', 200000 + n,
        'Autorizacion', 'AUTH-DEMO-' || n,
        'DemoSeed', 'DEMO-SEED'
    ),
    NOW() AT TIME ZONE 'UTC' - (n * INTERVAL '7 minutes'),
    false, NOW(), NOW()
FROM generate_series(1, 6) AS n;

-- 10 facturas del mismo empleado EMP-011 hoy (para que 6 anulaciones = 60% > 5%)
INSERT INTO transacciones_staging
    ("EstacionId", "TipoTransaccion", "DataJson", "FechaOriginal", "Procesada", "CreatedAt", "UpdatedAt")
SELECT
    1, 'Factura',
    jsonb_build_object(
        'SecuenciaDocumento', 100100 + n,
        'TipoDocumento', 'FAC',
        'NumeroDocumento', '001-001-000100' || lpad((100 + n)::text, 3, '0'),
        'FechaDocumento', to_char(NOW() AT TIME ZONE 'UTC' - (n * INTERVAL '15 minutes'), 'YYYY-MM-DD"T"HH24:MI:SS.MS"Z"'),
        'CodigoCliente', 'CLI-LOOP-' || n,
        'TotalNeto', 30.0 + n,
        'TotalSinIva', 26.0 + n,
        'Descuento', 0, 'Iva', 4.0,
        'CodigoVendedor', 'EMP-011',
        'CodigoPago', 'EF',
        'Placa', 'PLA' || lpad(n::text, 4, '0'),
        'RucCliente', '17000000' || lpad(n::text, 5, '0'),
        'NumeroTurno', 9002,
        'Subtotal', 26.0 + n,
        'NumeroConsecutivo', 100 + n,
        'CodigoChofer', '',
        'CodigoManguera', 'M-04',
        'DemoSeed', 'DEMO-SEED'
    ),
    NOW() AT TIME ZONE 'UTC' - (n * INTERVAL '15 minutes'),
    false, NOW(), NOW()
FROM generate_series(1, 10) AS n;

-- ===========================================
-- Payment Fraud: una factura con tarjeta + una reversion 90 min despues
-- ===========================================
INSERT INTO transacciones_staging
    ("EstacionId", "TipoTransaccion", "DataJson", "FechaOriginal", "Procesada", "CreatedAt", "UpdatedAt")
VALUES (
    1, 'Factura',
    jsonb_build_object(
        'SecuenciaDocumento', 100200,
        'TipoDocumento', 'FAC',
        'NumeroDocumento', '001-001-000100200',
        'FechaDocumento', to_char(NOW() AT TIME ZONE 'UTC' - INTERVAL '2 hours', 'YYYY-MM-DD"T"HH24:MI:SS.MS"Z"'),
        'CodigoCliente', 'CLI-PF-1',
        'TotalNeto', 75.00, 'TotalSinIva', 66.96, 'Descuento', 0, 'Iva', 8.04,
        'CodigoVendedor', 'EMP-022',
        'CodigoPago', 'TC',
        'Placa', 'GAA9988',
        'RucCliente', '1791234567001',
        'NumeroTurno', 9003,
        'Subtotal', 66.96, 'NumeroConsecutivo', 200,
        'CodigoChofer', '', 'CodigoManguera', 'M-06',
        'DemoSeed', 'DEMO-SEED'
    ),
    NOW() AT TIME ZONE 'UTC' - INTERVAL '2 hours',
    false, NOW(), NOW()
);

INSERT INTO transacciones_staging
    ("EstacionId", "TipoTransaccion", "DataJson", "FechaOriginal", "Procesada", "CreatedAt", "UpdatedAt")
VALUES (
    1, 'TarjetaTurno',
    jsonb_build_object(
        'NumeroTarjetaTurno', 8001,
        'NumeroTurno', 9003,
        'CodigoBanco', 'BANCO-PICHINCHA',
        'Cantidad', 1,
        'Valor', -75.00,
        'DemoSeed', 'DEMO-SEED'
    ),
    NOW() AT TIME ZONE 'UTC' - INTERVAL '30 minutes',
    false, NOW(), NOW()
);

INSERT INTO transacciones_staging
    ("EstacionId", "TipoTransaccion", "DataJson", "FechaOriginal", "Procesada", "CreatedAt", "UpdatedAt")
VALUES (
    1, 'CierreTurno',
    jsonb_build_object(
        'NumeroTurno', 9003,
        'CodigoVendedor', 'EMP-022',
        'FechaInicio', to_char(NOW() AT TIME ZONE 'UTC' - INTERVAL '4 hours', 'YYYY-MM-DD"T"HH24:MI:SS.MS"Z"'),
        'FechaFin', to_char(NOW() AT TIME ZONE 'UTC' - INTERVAL '15 minutes', 'YYYY-MM-DD"T"HH24:MI:SS.MS"Z"'),
        'SaldoInicial', 50.00, 'Ingresos', 400.00, 'Egresos', 20.00,
        'SaldoFinal', 430.00, 'Faltante', 0.00, 'Sobrante', 0.00, 'Creditos', 0.00,
        'DemoSeed', 'DEMO-SEED'
    ),
    NOW() AT TIME ZONE 'UTC' - INTERVAL '15 minutes',
    false, NOW(), NOW()
);

SELECT 'Filas insertadas en staging:' AS info, COUNT(*) AS total
FROM transacciones_staging
WHERE "Procesada" = false;
