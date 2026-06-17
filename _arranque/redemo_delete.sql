/* Borra las ventas anomalas de la corrida anterior para poder re-insertarlas
   con fecha actual (CURRENT_TIMESTAMP) y que el Station Agent las levante. */
DELETE FROM DCTO WHERE SEC_DCTO BETWEEN 9900001 AND 9900012;
DELETE FROM DESP WHERE NUM_DESP = 9900001;
DELETE FROM TURN WHERE NUM_TURN = 990001;
COMMIT;
