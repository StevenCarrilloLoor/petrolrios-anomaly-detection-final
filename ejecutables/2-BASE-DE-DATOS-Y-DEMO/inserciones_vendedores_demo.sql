/* ============================================================
   Despachadores/vendedores demo en la BD Firebird REAL (CONTAC.FDB).
   Da de alta en VEND los codigos de despachador que usan las ventas
   anomalas de prueba, con nombres realistas, para que el Station Agent
   sincronice el catalogo (codigo -> nombre) y las alertas muestren el
   NOMBRE junto al codigo.

   Codigos de 3 caracteres: es el formato REAL de VEND.COD_VEND en
   Contaplus (CHAR(3)). Idempotente: UPDATE OR INSERT por COD_VEND, asi
   que correrlo varias veces no duplica.

   El codigo 003 (LUIS SOTOMAYOR) ya existe en la base restaurada; aqui
   solo se agregan 004..010 para tener un despachador distinto por
   escenario de prueba.
   ============================================================ */

UPDATE OR INSERT INTO VEND (COD_VEND, NOM_VEND) VALUES ('004', 'JORGE MENDOZA')    MATCHING (COD_VEND);
UPDATE OR INSERT INTO VEND (COD_VEND, NOM_VEND) VALUES ('005', 'MARIA QUINONEZ')   MATCHING (COD_VEND);
UPDATE OR INSERT INTO VEND (COD_VEND, NOM_VEND) VALUES ('006', 'ANGEL CASTRO')     MATCHING (COD_VEND);
UPDATE OR INSERT INTO VEND (COD_VEND, NOM_VEND) VALUES ('007', 'DIANA PARRAGA')    MATCHING (COD_VEND);
UPDATE OR INSERT INTO VEND (COD_VEND, NOM_VEND) VALUES ('008', 'WASHINGTON BRAVO') MATCHING (COD_VEND);
UPDATE OR INSERT INTO VEND (COD_VEND, NOM_VEND) VALUES ('009', 'NESTOR INTRIAGO')  MATCHING (COD_VEND);
UPDATE OR INSERT INTO VEND (COD_VEND, NOM_VEND) VALUES ('010', 'CARLA VALAREZO')   MATCHING (COD_VEND);

COMMIT;
