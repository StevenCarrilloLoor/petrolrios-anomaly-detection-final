/* ============================================================
   Inserciones de prueba E2E contra la BD Firebird REAL (CONTAC.FDB).
   Simulan anulaciones nuevas del POS Contaplus que el Station Agent
   detectara por watermark (FECHAANULACION posterior a la ultima
   extraccion) y enviara a la central como fuente configurable "Anulaciones".

   Disparan la regla personalizada de prueba:
     "PRUEBA E2E - Exceso de anulaciones por punto de emision"
     (Anulaciones: NUMAN > 0, carril Auditoria)

   Claves primarias en rango 999xxxx para no chocar con datos reales.
   ============================================================ */

INSERT INTO ANUL (NUMAN, TIPOCOMPROBANTE, FECHAANULACION, ESTABLECIMIENTO, PUNTOEMISION, SECUENCIALINICIO, SECUENCIALFIN, AUTORIZACION)
VALUES (9990001, 'FV', CURRENT_TIMESTAMP, '001', '001', '0009990', '0009990', 'PRUEBA-E2E-ANUL-9990001');

INSERT INTO ANUL (NUMAN, TIPOCOMPROBANTE, FECHAANULACION, ESTABLECIMIENTO, PUNTOEMISION, SECUENCIALINICIO, SECUENCIALFIN, AUTORIZACION)
VALUES (9990002, 'FV', CURRENT_TIMESTAMP, '001', '001', '0009991', '0009991', 'PRUEBA-E2E-ANUL-9990002');

INSERT INTO ANUL (NUMAN, TIPOCOMPROBANTE, FECHAANULACION, ESTABLECIMIENTO, PUNTOEMISION, SECUENCIALINICIO, SECUENCIALFIN, AUTORIZACION)
VALUES (9990003, 'FV', CURRENT_TIMESTAMP, '001', '002', '0009992', '0009992', 'PRUEBA-E2E-ANUL-9990003');

COMMIT;
