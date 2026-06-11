/********************* ROLES **********************/

CREATE ROLE RDB$ADMIN;
/********************* UDFS ***********************/

/****************** GENERATORS ********************/

CREATE GENERATOR CIE_MOLI_TANQ;
CREATE GENERATOR CON_MOVI_PGDE_ID;
CREATE GENERATOR GEN_ARCH_CON_ID;
CREATE GENERATOR GEN_ARCH_CON_REF;
CREATE GENERATOR GEN_ARCH_CUA_ID;
CREATE GENERATOR GEN_ARCH_MOV_ID;
CREATE GENERATOR GEN_BANC_PEND_ID;
CREATE GENERATOR GEN_CHEQ_ID;
CREATE GENERATOR GEN_CHEQ_MOV_ID;
CREATE GENERATOR GEN_CICA_ID;
CREATE GENERATOR GEN_CLIE_CC;
CREATE GENERATOR GEN_CLIE_CUMO_ID;
CREATE GENERATOR GEN_CLIE_CUPO_ID;
CREATE GENERATOR GEN_CLIE_PRESET_ID;
CREATE GENERATOR GEN_COBA_ID;
CREATE GENERATOR GEN_CONS1_ID;
CREATE GENERATOR GEN_CONS_ID;
CREATE GENERATOR GEN_CONS_MOV_ID;
CREATE GENERATOR GEN_CON_MOVIP_ID;
CREATE GENERATOR GEN_CON_MOVI_ID;
CREATE GENERATOR GEN_CST_CLIE;
CREATE GENERATOR GEN_DCTO_ID;
CREATE GENERATOR GEN_DESP_ID;
CREATE GENERATOR GEN_EMPL_ID;
CREATE GENERATOR GEN_ESTA_ID;
CREATE GENERATOR GEN_ESTR_MOV_ID;
CREATE GENERATOR GEN_ESTR_VEN_ID;
CREATE GENERATOR GEN_F104_ID;
CREATE GENERATOR GEN_FACT_ID;
CREATE GENERATOR GEN_IAIRV_ID;
CREATE GENERATOR GEN_IAIR_ID;
CREATE GENERATOR GEN_IMPR_ID;
CREATE GENERATOR GEN_KEY_LOG;
CREATE GENERATOR GEN_KILO_ID;
CREATE GENERATOR GEN_LICA_ID;
CREATE GENERATOR GEN_LIMO_ID;
CREATE GENERATOR GEN_LIQU_CRED_ID;
CREATE GENERATOR GEN_LIQU_DENO_ID;
CREATE GENERATOR GEN_LIQU_DEPO_DENO_ID;
CREATE GENERATOR GEN_LIQU_DEPO_ID;
CREATE GENERATOR GEN_LIQU_GAST_ID;
CREATE GENERATOR GEN_LIQU_ITEM_ID;
CREATE GENERATOR GEN_LIQU_LLLD;
CREATE GENERATOR GEN_LIQU_TANK_ID;
CREATE GENERATOR GEN_LIQU_TARJ_DET;
CREATE GENERATOR GEN_LIQU_TARJ_ID;
CREATE GENERATOR GEN_LOG_ID;
CREATE GENERATOR GEN_MOCB_ID;
CREATE GENERATOR GEN_MOCB_PEND_ID;
CREATE GENERATOR GEN_MOCB_REC_ID;
CREATE GENERATOR GEN_MOLI_ID;
CREATE GENERATOR GEN_MOLI_TANQ_ID;
CREATE GENERATOR GEN_MOPF_MOVI_ID;
CREATE GENERATOR GEN_MOVI_ID;
CREATE GENERATOR GEN_NUM_CLIE_DEST_ID;
CREATE GENERATOR GEN_NUM_COMP_ID;
CREATE GENERATOR GEN_NUM_ITEM_ID;
CREATE GENERATOR GEN_NUM_MOLI;
CREATE GENERATOR GEN_NUM_PREC_ID;
CREATE GENERATOR GEN_NUM_PROM_SORT_ID;
CREATE GENERATOR GEN_NUM_RUTA_ID;
CREATE GENERATOR GEN_NUM_TLVV_ID;
CREATE GENERATOR GEN_NUM_ZONA_DIST_ID;
CREATE GENERATOR GEN_PLACA_CUPO_ID;
CREATE GENERATOR GEN_PROM_ID;
CREATE GENERATOR GEN_PRUEBAS_ID;
CREATE GENERATOR GEN_R103_ID;
CREATE GENERATOR GEN_R103_MOV_ID;
CREATE GENERATOR GEN_R104_ID;
CREATE GENERATOR GEN_R104_MOV_ID;
CREATE GENERATOR GEN_REPESTCUEN_ID;
CREATE GENERATOR GEN_RFID_COMB_ID;
CREATE GENERATOR GEN_RFID_KEY_ID;
CREATE GENERATOR GEN_ROLP_ID;
CREATE GENERATOR GEN_ROL_MOV_ID;
CREATE GENERATOR GEN_ROL_RUB_ID;
CREATE GENERATOR GEN_SESS_ID;
CREATE GENERATOR GEN_SYSLOCKEXCE_ID;
CREATE GENERATOR GEN_SYSLOCKMOVI_ID;
CREATE GENERATOR GEN_SYSLOCK_ID;
CREATE GENERATOR GEN_SYSMODUTIPO_ID;
CREATE GENERATOR GEN_SYSMODU_ID;
CREATE GENERATOR GEN_TANQ_ING_ID;
CREATE GENERATOR GEN_TANQ_MOV_ID;
CREATE GENERATOR GEN_TANQ_NUM_REPO_ID;
CREATE GENERATOR GEN_TANQ_REPO_ID;
CREATE GENERATOR GEN_TANQ_RIND_ID;
CREATE GENERATOR GEN_TANQ_TAB_ID;
CREATE GENERATOR GEN_TICKET_ID;
CREATE GENERATOR GEN_TRAM_ID;
CREATE GENERATOR GEN_TRAM_MOV_ID;
CREATE GENERATOR GEN_TURN_DEPO_ID;
CREATE GENERATOR GEN_TURN_ID;
CREATE GENERATOR GEN_TURN_TARJ;
CREATE GENERATOR GEN_XBANC_ID;
CREATE GENERATOR GEN_XDCTO_ID;
CREATE GENERATOR GEN_XPGCA_ID;
CREATE GENERATOR GEN_XPGDE_ID;
CREATE GENERATOR ITEM_MAMO_ID;
CREATE GENERATOR NUM_PUNTOS_ID;
/******************** DOMAINS *********************/

/******************* PROCEDURES ******************/

SET TERM ^ ;
CREATE PROCEDURE CAL_COS_ITEM (
    ICOD_ITEM Char(100),
    IUNI_ITEMC Double precision,
    IFEC_DCTO Timestamp )
RETURNS (
    RCOD_ITEM Char(100),
    RSCA_ITEM Numeric(15,3),
    RSUN_ITEM Numeric(15,3),
    RCOS_ITEM Numeric(15,6) )
AS
BEGIN EXIT; END^
SET TERM ; ^

SET TERM ^ ;
CREATE PROCEDURE CONSULTAS_VENTAS (
    IPROCESO Integer,
    IFECHA_DESDE Char(24),
    IFECHA_HASTA Char(24) )
RETURNS (
    ONOM_LIQ Char(100),
    OTRA_LIQ Decimal(15,0),
    OCAN_LIQ Decimal(15,2),
    OVAL_LIQ Decimal(15,2) )
AS
BEGIN EXIT; END^
SET TERM ; ^

SET TERM ^ ;
CREATE PROCEDURE INTERVALOS_IMPRODUCTIVOS (
    FECHA_DESDE Char(19),
    FECHA_HASTA Char(19) )
RETURNS (
    SURTIDOR Char(4),
    MANGUERA Char(2),
    PRODUCTO Char(10),
    DIA Char(9),
    FECHA Char(10),
    HORA Char(8),
    INTERVALO Char(7),
    GALONES Numeric(15,4),
    MONTO Numeric(15,4),
    NDESPACHADOR Char(150) )
AS
BEGIN EXIT; END^
SET TERM ; ^

SET TERM ^ ;
CREATE PROCEDURE INTERVALOS_IMPRODUCTIVOS_MANUAL (
    FECHA_DESDE Char(19),
    FECHA_HASTA Char(19),
    VALOR Integer )
RETURNS (
    SURTIDOR Char(4),
    MANGUERA Char(2),
    PRODUCTO Char(10),
    DIA Char(9),
    FECHA Char(10),
    HORA Char(8),
    INTERVALO Char(7),
    GALONES Numeric(15,4),
    MONTO Numeric(15,4),
    NDESPACHADOR Char(150) )
AS
BEGIN EXIT; END^
SET TERM ; ^

SET TERM ^ ;
CREATE PROCEDURE KARDEX (
    IEB_CONS Integer DEFAULT 1,
    IFV_CONS Integer DEFAULT 1,
    ICOD_BODE Char(2) DEFAULT '01',
    ICOD_ITEM Char(20) DEFAULT '',
    ISAL_ANTE Numeric(12,3) DEFAULT 0,
    IFDE_DCTO Timestamp DEFAULT '01/01/2018',
    IFHA_DCTO Timestamp DEFAULT '12/31/2018 23:59:59' )
RETURNS (
    BODEGA Char(2),
    TIPO Char(2),
    NUMERO Char(20),
    FECHA Timestamp,
    BEM_MOVI Char(1),
    SECUENCIAL Integer,
    DETALLE Char(300),
    PLACA Char(50),
    CANTIDAD Numeric(12,3),
    SALDO Numeric(18,3),
    CODIGO Char(10),
    RUC Char(15),
    NOMBRE Char(300),
    DEBITO Numeric(18,3),
    CREDITO Numeric(18,3),
    CONSOLIDACION Integer,
    CANTIDAD_CONSOLIDACION Numeric(18,3),
    DEBITO_CONSOLIDACION Numeric(18,3) )
AS
BEGIN EXIT; END^
SET TERM ; ^

SET TERM ^ ;
CREATE PROCEDURE KERDEX (
    IEB_CONS Integer DEFAULT 1,
    IFV_CONS Integer DEFAULT 1,
    ICOD_BODE Char(2) DEFAULT '01',
    ICOD_ITEM Char(20) DEFAULT '',
    ISAL_ANTE Numeric(12,3) DEFAULT 0,
    IFDE_DCTO Timestamp DEFAULT '01/01/2018',
    IFHA_DCTO Timestamp DEFAULT '12/31/2018 23:59:59' )
RETURNS (
    BODEGA Char(2),
    TIPO Char(2),
    NUMERO Char(20),
    FECHA Timestamp,
    BEM_MOVI Char(1),
    SECUENCIAL Integer,
    DETALLE Char(300),
    PLACA Char(50),
    CANTIDAD Numeric(12,3),
    SALDO Numeric(18,3),
    CODIGO Char(10),
    RUC Char(15),
    NOMBRE Char(300),
    DEBITO Numeric(18,3),
    CREDITO Numeric(18,3) )
AS
BEGIN EXIT; END^
SET TERM ; ^

SET TERM ^ ;
CREATE PROCEDURE NEW_PROCEDURE (
    IFEC_DCTO Timestamp,
    ISEC_DCTO Double precision DEFAULT  )
RETURNS (
    RTOT_PAG Double precision )
AS
BEGIN EXIT; END^
SET TERM ; ^

SET TERM ^ ;
CREATE PROCEDURE PRODUCTIVIDAD_DESPACHADORES (
    FECHA_DESDE Char(19),
    FECHA_HASTA Char(19),
    VALOR Integer )
RETURNS (
    NDESPACHADOR Char(40),
    GALONES Numeric(15,2),
    GALONESE Numeric(15,2),
    PORCENTAJEG Numeric(15,2),
    DESPACHOS Integer,
    DESPACHOSE Integer,
    PORCENTAJED Numeric(15,2),
    PORCENTAJEP Numeric(15,2) )
AS
BEGIN EXIT; END^
SET TERM ; ^

SET TERM ^ ;
CREATE PROCEDURE PRODUCTIVIDAD_TRIMESTRAL (
    FECHA_DESDE Char(19),
    FECHA_DESDE2 Char(19),
    FECHA_HASTA Char(19),
    FECHA_HASTA2 Char(19),
    VALOR Integer )
RETURNS (
    NDESPACHADOR Char(40),
    GALONES Numeric(15,2),
    GALONESE Numeric(15,2),
    PORCENTAJEG Numeric(15,2),
    DESPACHOS Integer,
    DESPACHOSE Integer,
    PORCENTAJED Numeric(15,2),
    PORCENTAJEP Numeric(15,2) )
AS
BEGIN EXIT; END^
SET TERM ; ^

SET TERM ^ ;
CREATE PROCEDURE PROREP_VENT (
    ICDE_VEND Char(9) DEFAULT ,
    ICHA_VEND Char(9) DEFAULT ZZZZ',
    IFDE_DCTO Timestamp DEFAULT /2012',
    IFHA_DCTO Timestamp DEFAULT /2012 23:59:59',
    ICDE_CLIE Char(9) DEFAULT ,
    ICHA_CLIE Char(9) DEFAULT ZZZZ',
    ICDE_PAGO Char(3) DEFAULT ,
    ICHA_PAGO Char(3) DEFAULT ,
    ICDE_DCTO Char(2) DEFAULT ,
    ICHA_DCTO Char(2) DEFAULT  )
RETURNS (
    RSEC_DCTO Double precision,
    RTIP_DCTO Char(10),
    RFEC_DCTO Timestamp,
    RDET_DCTO Char(250),
    RTNI_DCTO Double precision,
    RTSI_DCTO Double precision,
    RIVA_DCTO Double precision,
    RCOD_VEND Char(9),
    RCOD_CLIE Char(9),
    RNOM_CLIE Char(60) )
AS
BEGIN EXIT; END^
SET TERM ; ^

SET TERM ^ ;
CREATE PROCEDURE PRO_DCTO_CLIE (
    ISEC_DCTO Double precision DEFAULT  )
RETURNS (
    RSEC_DCTO Double precision,
    RNUM_DCTO Char(20),
    RDET_DCTO Char(250),
    RCOL_DCTO Char(50),
    RFEC_DCTO Timestamp,
    RCOD_PAGO Char(3),
    RCOD_CLIE Char(9),
    RNOM_CLIE Char(60),
    RRUC_CLIE Char(13),
    RDCA_CLIE Char(50),
    RTE1_CLIE Char(9),
    RTNI_DCTO Double precision,
    RTSI_DCTO Double precision,
    RDSC_DCTO Double precision,
    RIVA_DCTO Double precision,
    RCOD_BODE Char(2),
    RCOD_ACTI Char(2),
    RCOD_VEND Char(9) )
AS
BEGIN EXIT; END^
SET TERM ; ^

SET TERM ^ ;
CREATE PROCEDURE PRO_MOVI_ITEM (
    ISEC_DCTO Double precision )
RETURNS (
    RCOD_ITEM Char(100),
    RNOM_ITEM Char(100),
    RCAN_MOVI Double precision,
    RVAL_MOVI Double precision,
    RDSC_MOVI Double precision,
    RTOT_MOVI Double precision,
    RIVA_MOVI Char(1),
    RBEM_MOVI Char(1),
    RTVE_MOVI Char(1),
    RSEC_DCTO Double precision )
AS
BEGIN EXIT; END^
SET TERM ; ^

SET TERM ^ ;
CREATE PROCEDURE PRO_REP_VENT (
    ICDE_VEND Char(9) DEFAULT ,
    ICHA_VEND Char(9) DEFAULT ZZZZ',
    IFDE_DCTO Timestamp DEFAULT /2012',
    IFHA_DCTO Timestamp DEFAULT /2012 23:59:59',
    ICDE_CLIE Char(9) DEFAULT ,
    ICHA_CLIE Char(9) DEFAULT ZZZZ',
    ICDE_PAGO Char(3) DEFAULT ,
    ICHA_PAGO Char(3) DEFAULT  )
RETURNS (
    RSEC_DCTO Double precision,
    RTIP_DCTO Char(10),
    RFEC_DCTO Timestamp,
    RDET_DCTO Char(250),
    RCAN_MOVI Double precision,
    RVAL_MOVI Double precision,
    RTNI_DCTO Double precision,
    RTSI_DCTO Double precision,
    RIVA_DCTO Double precision,
    RCOD_VEND Char(9),
    RCOD_CLIE Char(9),
    RNOM_CLIE Char(60),
    RCOD_ITEM Char(100) )
AS
BEGIN EXIT; END^
SET TERM ; ^

SET TERM ^ ;
CREATE PROCEDURE PRO_SAL_DCTO (
    IFEC_DCTO Timestamp,
    ISEC_DCTO Double precision DEFAULT  )
RETURNS (
    RTOT_PAG Double precision,
    RFUP_PAG Timestamp )
AS
BEGIN EXIT; END^
SET TERM ; ^

SET TERM ^ ;
CREATE PROCEDURE PRO_SAL_DCTOMENOR (
    IFEC_DCTO Timestamp,
    ISEC_DCTO Double precision DEFAULT  )
RETURNS (
    RTOT_PAG Double precision )
AS
BEGIN EXIT; END^
SET TERM ; ^

SET TERM ^ ;
CREATE PROCEDURE PRO_ULT_VENT (
    INUM_REGI Integer )
RETURNS (
    RSEC_DCTO Double precision,
    RFEC_DCTO Timestamp,
    RDET_DCTO Char(250),
    RCAN_MOVI Double precision,
    RVAL_MOVI Double precision,
    RTNI_DCTO Double precision,
    RTSI_DCTO Double precision,
    RIVA_DCTO Double precision,
    RCOD_VEND Char(9),
    RCOD_CLIE Char(9),
    RNOM_CLIE Char(60),
    RCOD_ITEM Char(100) )
AS
BEGIN EXIT; END^
SET TERM ; ^

SET TERM ^ ;
CREATE PROCEDURE XPERM (
    TCNOMBREUSUARIO Varchar(31) )
AS
BEGIN EXIT; END^
SET TERM ; ^

/******************** TABLES **********************/

CREATE TABLE ACCE
(
  CODACCE Char(25),
  CREACCE Char(1),
  MODACCE Char(1),
  ELIACCE Char(1),
  IMPACCE Char(1),
  VERACCE Char(1),
  AUDACCE Char(1) DEFAULT '0',
  MODUACCE Char(1) DEFAULT '0',
  PRIMARY KEY (CODACCE)
);
CREATE TABLE ACFI
(
  COD_ACFI Char(25),
  COD_ITEM Char(25),
  VAL_ACFI Double precision,
  DAC_ACFI Double precision,
  NME_GRUP Integer,
  FIN_ACFI Timestamp,
  FUD_ACFI Timestamp,
  FUM_ACFI Timestamp,
  COD_DEPA Char(25),
  TNI_ACFI Double precision,
  TSI_ACFI Double precision,
  IVA_ACFI Double precision,
  NSE_ACFI Char(50),
  SEC_DCTO Double precision,
  MIN_ACFI Double precision,
  PRIMARY KEY (COD_ACFI)
);
CREATE TABLE ACTI
(
  COD_ACTI Char(2),
  NOM_ACTI Char(50),
  COL_ACTI Double precision,
  IVG_ACTI Char(1) DEFAULT '1',
  COD_PAFA Char(10) DEFAULT '',
  COD_CUEN Char(25) DEFAULT '',
  COS_ACTI Char(1) DEFAULT 'N',
  COD_BANC Char(2) DEFAULT '',
  COD_EMPR Char(3) DEFAULT '001',
  PRIMARY KEY (COD_ACTI)
);
CREATE TABLE ADDE
(
  NUMMCOMP Double precision,
  NUM_ADDI Integer,
  COD_ADDI Char(20),
  DET_ADDE Char(200)
);
CREATE TABLE ADDI
(
  COD_TIPC Char(2),
  NUM_ADDI Integer,
  COD_ADDI Char(20),
  EST_ADDI Integer
);
CREATE TABLE AHOR
(
  COD_AHOR Char(2),
  NOM_AHOR Char(50),
  TAZ_AHOR Double precision,
  APE_AHOR Double precision,
  APA_AHOR Double precision,
  PRIMARY KEY (COD_AHOR)
);
CREATE TABLE AHOR_MOVI
(
  COD_TIPO Char(4),
  FEC_MOVI Timestamp,
  CAN_MOVI Double precision,
  APP_MOVI Char(1),
  NUMMCOMP Double precision,
  REF_MOVI Char(15),
  COD_AHOR Char(2),
  COD_BANC Char(2),
  COD_SOCI Char(9)
);
CREATE TABLE AHOR_TIPO
(
  COD_TIPO Char(4),
  NOM_TIPO Char(50),
  DCR_TIPO Char(1),
  CBA_BANC Char(1),
  COD_CUEN Char(25),
  CTA_TIPO Char(25),
  PRIMARY KEY (COD_TIPO)
);
CREATE TABLE ANUL
(
  NUMAN Double precision,
  TIPOCOMPROBANTE Char(2),
  FECHAANULACION Timestamp,
  ESTABLECIMIENTO Char(3),
  PUNTOEMISION Char(3),
  SECUENCIALINICIO Char(7),
  SECUENCIALFIN Char(7),
  AUTORIZACION Varchar(50),
  PRIMARY KEY (NUMAN)
);
CREATE TABLE ARCH
(
  COD_ARCH Char(3),
  NOM_ARCH Char(50) DEFAULT '',
  HTT_ARCH Char(100) DEFAULT '',
  USU_ARCH Char(20) DEFAULT '',
  CLA_ARCH Char(20) DEFAULT '',
  COD_ACTI Char(2) DEFAULT '00',
  COD_BODE Char(2) DEFAULT '00',
  WSC_ARCH Char(200) DEFAULT '',
  PRIMARY KEY (COD_ARCH)
);
CREATE TABLE ARCH_CON
(
  NUM_ARCH_CON Integer,
  REF_ARCH_CON Integer DEFAULT 0,
  FEC_ARCH_CON Timestamp DEFAULT current_timestamp ,
  AUTORIZACION Char(50) DEFAULT '',
  BENEFICIARIO Char(250) DEFAULT '',
  DIRECCION Char(250) DEFAULT '',
  ESTABLECIMIENTO Char(10) DEFAULT '',
  IDENTIFICACION Char(20) DEFAULT '',
  PRODUCTOS Char(250) DEFAULT '',
  PRIMARY KEY (NUM_ARCH_CON)
);
CREATE TABLE ARCH_CUA
(
  NUM_ARCH_CUA Integer,
  NUM_ARMO Integer DEFAULT 0,
  EST_ARCH_CUA Char(2) DEFAULT '00',
  FEC_ARCH_CUA Timestamp DEFAULT current_timestamp ,
  CIACTIVIDAD Char(250) DEFAULT '',
  CIAUTTRANSPORTE Char(50) DEFAULT '',
  CIBENEFICIARIO Char(250) DEFAULT '',
  CICODIGOERROR Char(10) DEFAULT '',
  CIDESCRIPCIONERROR Char(200) DEFAULT '00',
  CIDESTINO Char(250) DEFAULT '',
  CIESTACION Char(250) DEFAULT '',
  CIFECHAFIN Char(50) DEFAULT '',
  CIFECHAINICIO Char(50) DEFAULT '',
  CIORIGEN Char(250) DEFAULT '',
  CIPERSONASRETIRO Char(250) DEFAULT '',
  CIRUCBENEFICIARIO Char(20) DEFAULT '',
  CIVOLUMENP Char(20) DEFAULT '',
  COAUTCUANTIA Char(100) DEFAULT '',
  DIASSIGUIENTEPERIODO Char(10) DEFAULT '0',
  INICIOSIGUIENTEPERIODO Char(50) DEFAULT '',
  NOMBREPRODUCTO Char(200) DEFAULT '',
  PROCODIGO Char(20) DEFAULT '',
  PRIMARY KEY (NUM_ARCH_CUA)
);
CREATE TABLE ARCH_EST
(
  COD_ARCH_EST Char(3),
  MOD_ARCH_EST Char(2) DEFAULT '00',
  DET_ARCH_EST Char(250) DEFAULT '',
  SOL_ARCH_EST Char(250) DEFAULT '',
  PRIMARY KEY (COD_ARCH_EST,MOD_ARCH_EST)
);
CREATE TABLE ARCH_MOV
(
  NUM_ARMO Integer,
  SEC_DCTO Double precision DEFAULT 0,
  TIP_ARMO Char(2) DEFAULT '01',
  CON_ARMO Integer DEFAULT 0,
  FEC_ARMO Timestamp DEFAULT current_timestamp  ,
  COD_CLIE Char(9) DEFAULT '',
  COD_ITEM Char(100) DEFAULT '',
  TTR_ARMO Char(2) DEFAULT '02',
  CAN_ARMO Double precision DEFAULT 0,
  COD_CHOF Char(9) DEFAULT '000000000',
  RSN_ARMO Integer DEFAULT 0,
  RSD_ARMO Char(50) DEFAULT '',
  COD_ARCH Char(3) DEFAULT '',
  PLA_ARMO Char(20),
  GUI_ARMO Char(20),
  ANU_ARMO Char(1),
  FDE_ARMO Timestamp DEFAULT '01/01/2015',
  FHA_ARMO Timestamp DEFAULT '01/01/2015',
  RES_ARMO Char(200) DEFAULT '',
  EST_ARMO Char(3) DEFAULT '',
  FFA_ARMO Timestamp DEFAULT current_timestamp ,
  ND0_ARMO Char(20) DEFAULT '',
  ND1_ARMO Char(20) DEFAULT '',
  ND2_ARMO Char(20) DEFAULT '',
  ND3_ARMO Char(20) DEFAULT '',
  ND4_ARMO Char(20) DEFAULT '',
  ND5_ARMO Char(20) DEFAULT '',
  SD0_ARMO Double precision DEFAULT 0,
  SD1_ARMO Double precision DEFAULT 0,
  SD2_ARMO Double precision DEFAULT 0,
  SD3_ARMO Double precision DEFAULT 0,
  SD4_ARMO Double precision DEFAULT 0,
  SD5_ARMO Double precision DEFAULT 0,
  PRIMARY KEY (NUM_ARMO)
);
CREATE TABLE BANC
(
  COD_BANC Char(2),
  NOM_BANC Char(255),
  CTA_BANC Char(20),
  COD_CUEN Char(25),
  CBA_BANC Char(1),
  CPO_BANC Char(1) DEFAULT '0',
  SAL_BANC Double precision DEFAULT 0,
  NUM_TURN Integer DEFAULT 0,
  COD_CLIE Char(9) DEFAULT '',
  COD_ITEM Char(25) DEFAULT '',
  COD_CUEN_DIF Char(25) DEFAULT '',
  COD_CLIE_CXC Char(25) DEFAULT '',
  COD_CLIE_CXP Char(25) DEFAULT '',
  BAN_BANC_CXP Char(1) DEFAULT '0',
  MOD_BANC Char(1) DEFAULT '0',
  VER_BANC Char(1) DEFAULT '3',
  CHI_BANC Char(1) DEFAULT '0',
  REP_BANC Double precision DEFAULT 0,
  COD_PAGO Char(3) DEFAULT '',
  PRIMARY KEY (COD_BANC)
);
CREATE TABLE BANC_PEND
(
  NUM_BANC_PEND Integer,
  FEC_BANC_PEND Timestamp,
  COD_CUEN Char(25),
  DET_BANC_PEND Char(240),
  VAL_BANC_PEND Numeric(13,2),
  CHE_BANC_PEND Double precision DEFAULT 0,
  REF_BANC_PEND Char(15),
  NOM_CLIE Char(240) DEFAULT '',
  NUM_COBA Integer,
  PRIMARY KEY (NUM_BANC_PEND)
);
CREATE TABLE BODE
(
  COD_BODE Char(2),
  NOM_BODE Char(50),
  COM_BODE Integer,
  VEN_BODE Integer,
  ING_BODE Integer,
  EGR_BODE Integer,
  TRA_BODE Integer,
  STO_BODE Integer,
  COD_ACTI Char(2),
  FAC_BODE Char(20) DEFAULT '00',
  COD_DEPA Char(2) DEFAULT '00',
  DSC_BODE Char(1) DEFAULT '0',
  COS_BODE Char(1) DEFAULT 'N',
  VEB_BODE Char(1) DEFAULT '0',
  COD_BANC Char(2) DEFAULT '',
  COS_X_BODE Char(1) DEFAULT '0',
  ZAM_BODE Char(1) DEFAULT '0',
  LEY1_FAC_BODE Char(300) DEFAULT '',
  LEY2_FAC_BODE Char(300) DEFAULT '',
  PRIMARY KEY (COD_BODE)
);
CREATE TABLE BODE_MIGR
(
  COD_BODE_ORI Char(2),
  COD_ACTI_DES Char(2) DEFAULT '',
  COD_BODE_DES Char(2) DEFAULT '',
  COD_ITEM_DES Char(2) DEFAULT '',
  COD_VEND_DES Char(9) DEFAULT '',
  PRIMARY KEY (COD_BODE_ORI)
);
CREATE TABLE BORR
(
  NUM_BORR Double precision,
  COD_BORR Char(25),
  NOM_BORR Char(60),
  DET_BORR Char(200),
  FAP_BORR Timestamp,
  CUS_BORR Char(10),
  BMO_BORR Char(1),
  TDO_BORR Char(60),
  PRIMARY KEY (NUM_BORR)
);
CREATE TABLE CHEQ
(
  NUM_CHEQ Integer,
  CPR_CHEQ Char(1),
  COD_CLIE Char(9),
  COD_BANC Char(2),
  CTA_CHEQ Char(20),
  NCH_CHEQ Double precision,
  VAL_CHEQ Double precision,
  FEC_CHEQ Timestamp,
  FVE_CHEQ Timestamp,
  DET_CHEQ Char(254),
  FEC_CHMO Timestamp,
  ADD_CHEQ Char(2),
  NAD_CHEQ Double precision,
  COD_ACTI Char(2),
  CBA_CHEQ Char(2),
  NUM_PGCA Double precision DEFAULT 0,
  CONSTRAINT PK_CHEQ PRIMARY KEY (NUM_CHEQ)
);
CREATE TABLE CHEQ_MOV
(
  NUM_CHMO Integer,
  NUM_CHEQ Integer,
  FEC_CHMO Timestamp,
  DET_CHMO Char(254),
  NUMMCOMP Double precision,
  ORI_CHEQ Char(2),
  COD_BANC Char(2),
  NUM_PGCA Double precision,
  CONSTRAINT PK_CHEQ_MOV PRIMARY KEY (NUM_CHMO)
);
CREATE TABLE CICA
(
  NUM_CICA Integer,
  FEC_CICA Timestamp DEFAULT current_timestamp ,
  SINI_CICA Double precision DEFAULT 0,
  TRAN_CICA Double precision DEFAULT 0,
  SFIN_CICA Double precision DEFAULT 0,
  PRIMARY KEY (NUM_CICA)
);
CREATE TABLE CIUD
(
  COD_CIUD Char(4),
  COD_PROV Char(2),
  NOM_CIUD Char(50),
  ROL_CIUD Char(20) DEFAULT '',
  SUP_CIUD Char(20) DEFAULT '',
  CONSTRAINT PK_CIUD PRIMARY KEY (COD_CIUD)
);
CREATE TABLE CLAC
(
  COD_CLIE Char(9),
  COD_CUEN Char(25),
  BAC_CLAC Char(1),
  COD_MODU Char(2)
);
CREATE TABLE CLIE
(
  COD_CLIE Char(9),
  TID_CLIE Char(1),
  RUC_CLIE Char(13),
  RAZ_CLIE Char(50),
  APE_CLIE Char(40),
  NOM_CLIE Char(150),
  DCA_CLIE Char(50),
  DNU_CLIE Char(10),
  DCI_CLIE Char(20),
  DPR_CLIE Char(2),
  CPR_CLIE Char(1),
  TE1_CLIE Char(9),
  TE2_CLIE Char(9),
  FAX_CLIE Char(9),
  COR_CLIE Char(50),
  FIN_CLIE Timestamp,
  CRE_CLIE Double precision,
  EST_CLIE Char(2),
  COD_ZONA Char(2),
  CAN_CLIE Char(10),
  QUI_CLIE Char(2),
  CST_CLIE Varchar(9) DEFAULT '0000000',
  REL_CLIE Char(1) DEFAULT '0',
  CUP_CLIE Char(1) DEFAULT '0',
  MAI_CLIE Char(200) DEFAULT '',
  ARC_CLIE Char(200) DEFAULT '',
  NCR_CLIE Integer DEFAULT 0,
  NGA_CLIE Integer DEFAULT 0,
  CGA_CLIE Integer DEFAULT 0,
  KGA_CLIE Char(9) DEFAULT '',
  RIS_CLIE Char(1) DEFAULT '0',
  DPL_CLIE Integer DEFAULT 0,
  SOC_CLIE Char(1) DEFAULT '0',
  COD_VEND Char(9) DEFAULT '',
  BMX_CLIE Char(1) DEFAULT '0',
  VMX_CLIE Double precision DEFAULT 0,
  VAL_CADA_PUNTOS Double precision DEFAULT 0,
  SAL_PUNTOS Double precision DEFAULT 0,
  VEA_CLIE Integer DEFAULT 1,
  PLA_CLIE Char(1) DEFAULT '0',
  NCO_FV_CLIE Integer DEFAULT 0,
  NCO_FVC_CLIE Integer DEFAULT 0,
  NCO_EB_CLIE Integer DEFAULT 0,
  MAX_FV_CLIE Integer DEFAULT 0,
  MAX_EB_CLIE Integer DEFAULT 0,
  KIL_CLIE Char(1) DEFAULT '0',
  MAI_ENV_CLIE Char(1) DEFAULT '0',
  RFID_CLIE Char(30) DEFAULT '',
  NUM_EMPL Integer DEFAULT 0,
  REI_CUP_CLIE Char(1) DEFAULT '0',
  CRE_RFID_CLIE Char(1) DEFAULT '0',
  DEF_CUP_CLIE Double precision DEFAULT 0,
  VAL_CUP_CLIE Double precision DEFAULT 0,
  FEC_CUP_CLIE Timestamp DEFAULT current_timestamp ,
  CON_CUP_CLIE Double precision DEFAULT 0,
  CHO_FLO_CLIE Char(1) DEFAULT 0,
  NUM_CLCU Integer DEFAULT 0,
  CON_EB_CLIE Char(1) DEFAULT '0',
  CRE_RFID_CARD Char(1) DEFAULT '0',
  PRE_SET_CLIE Char(1) DEFAULT '0',
  BAN_TICK_CLIE Char(1) DEFAULT '0',
  GAL_PVP_CLIE Char(1) DEFAULT 'P',
  FFI_CUP_CLIE Timestamp DEFAULT current_TIMESTAMP  ,
  CON_ESP_CLIE Char(10) DEFAULT '',
  OBL_CON_CLIE Char(1) DEFAULT '0',
  RFID_PUN_CLIE Char(1) DEFAULT '0',
  PRIMARY KEY (COD_CLIE)
);
CREATE TABLE CLIE_CONTR
(
  COD_CLIE_CONTR Char(2),
  NOM_CLIE_CONTR Char(150) DEFAULT '',
  ARC_CLIE_CONTR Char(300) DEFAULT '',
  PRIMARY KEY (COD_CLIE_CONTR)
);
CREATE TABLE CLIE_CONTR_DET
(
  COD_CLIE_CONTR_DET Char(2),
  COD_CLIE_CONTR Char(2),
  TXT_CLIE_CONTR_DET Char(100) DEFAULT '',
  CAM_CLIE_CONTR_DET Char(100) DEFAULT '',
  PRIMARY KEY (COD_CLIE_CONTR,COD_CLIE_CONTR_DET)
);
CREATE TABLE CLIE_CONT_MENSUAL
(
  COD_ITEM Char(100),
  COD_CLIE Char(9),
  NUM_CONT Char(20),
  CAN_MOVI Integer DEFAULT 0,
  VAL_MOVI Decimal(12,6) DEFAULT 0,
  IVA_ITEM Char(1) DEFAULT 'S',
  ADD_MOVI Char(150) DEFAULT '',
  DIA_FACT Integer DEFAULT 1,
  CONSTRAINT PK_CLIE_CONT_MENSUAL_1 PRIMARY KEY (COD_ITEM,COD_CLIE,NUM_CONT)
);
CREATE TABLE CLIE_CUMO
(
  NUM_CUMO Integer,
  NUM_CLCU Integer,
  SEC_DCTO Integer,
  CODI_PLA Char(15),
  COD_CLIE Char(9),
  EST_CUMO Char(1) DEFAULT '0',
  FEC_CUMO Timestamp DEFAULT CURRENT_TIMESTAMP  ,
  VAL_CUMO Double precision DEFAULT 0,
  PRIMARY KEY (NUM_CUMO)
);
CREATE TABLE CLIE_CUPO
(
  NUM_CLCU Integer,
  COD_CLIE Char(9),
  EST_CLCU Char(1) DEFAULT '0',
  FIN_CLCU Timestamp DEFAULT CURRENT_TIMESTAMP  ,
  FFI_CLCU Timestamp DEFAULT CURRENT_TIMESTAMP  ,
  VAL_CLCU Numeric(12,2) DEFAULT 0,
  COD_USUA Char(13) DEFAULT '',
  SEC_DCTO Integer DEFAULT 0,
  PRIMARY KEY (NUM_CLCU)
);
CREATE TABLE CLIE_DEST
(
  NUM_CLIE_DEST Integer,
  COD_CLIE Char(9),
  DIR_CLIE_DEST Char(200),
  PRIMARY KEY (NUM_CLIE_DEST)
);
CREATE TABLE CLIE_MENSUAL
(
  COD_ITEM Char(100),
  COD_CLIE Char(9),
  CAN_MOVI Integer DEFAULT 0,
  VAL_MOVI Decimal(12,6) DEFAULT 0,
  IVA_ITEM Char(1) DEFAULT 'S',
  ADD_MOVI Char(150) DEFAULT '',
  DIA_FACT Integer DEFAULT 1,
  CONSTRAINT PK_CLIE_MENSUAL_1 PRIMARY KEY (COD_ITEM,COD_CLIE)
);
CREATE TABLE CLIE_PLAN
(
  COD_PLAN Char(2),
  NOM_PLAN Char(100),
  DET_PLAN Char(200),
  PRIMARY KEY (COD_PLAN)
);
CREATE TABLE CLIE_PLAN_CONT
(
  NUM_CONT Char(20),
  COD_CLIE Char(9),
  COD_PLAN Char(2),
  COD_ZONA Char(2),
  EST_CONT Char(2),
  DET_CONT Char(150) DEFAULT '',
  CIU_CONT Char(50) DEFAULT '',
  PIU_CONT Char(2) DEFAULT '01',
  CAL_CONT Char(100) DEFAULT '',
  NCA_CONT Char(20) DEFAULT '',
  CRU_CONT Char(100) DEFAULT '',
  FIN_CONT Timestamp,
  FFI_CONT Timestamp,
  LUZ_CONT Char(50) DEFAULT '',
  GNAP_CONT Char(50) DEFAULT '',
  PRIMARY KEY (NUM_CONT)
);
CREATE TABLE CLIE_PRESET
(
  NUM_PRESET Integer,
  COD_CLIE Char(9),
  COD_VEND Char(9),
  CUP_CLIE Decimal(10,2),
  ACU_PRESET Decimal(10,2),
  FEC_PRESET Timestamp,
  VAL_PRESET Decimal(10,2),
  DET_PRESET Char(50),
  SEC_DCTO Integer DEFAULT 0,
  EST_PRESET Char(1),
  PRO_PRESET Char(1),
  PRIMARY KEY (NUM_PRESET)
);
CREATE TABLE COBA
(
  NUM_COBA Integer,
  COD_BANC Char(2),
  FEC_COBA Timestamp,
  FDE_COBA Timestamp,
  FHA_COBA Timestamp,
  SIN_COBA Double precision,
  SFI_COBA Double precision,
  SES_COBA Double precision,
  TDB_COBA Double precision,
  TCR_COBA Double precision,
  CHN_COBA Double precision,
  CHC_COBA Double precision,
  TND_COBA Double precision,
  TNC_COBA Double precision,
  NDC_COBA Double precision DEFAULT 0,
  NDN_COBA Double precision DEFAULT 0,
  NCC_COBA Double precision DEFAULT 0,
  NCN_COBA Double precision DEFAULT 0,
  SIE_COBA Double precision DEFAULT 0,
  EST_COBA Char(1) DEFAULT 'A',
  DEC_COBA Double precision DEFAULT 0,
  DEN_COBA Double precision DEFAULT 0,
  NUMMCOMP Integer DEFAULT 0,
  CONSTRAINT PK_COBA PRIMARY KEY (NUM_COBA)
);
CREATE TABLE COMI
(
  COD_COMI Char(2),
  NOM_COMI Char(50) DEFAULT '',
  POR_COMI Double precision DEFAULT 0,
  TIP_COMI Char(1) DEFAULT '0',
  PRIMARY KEY (COD_COMI)
);
CREATE TABLE COMP
(
  NUMMCOMP Double precision,
  COD_TIPC Char(2),
  FAP_COMP Timestamp,
  VAL_COMP Double precision,
  DET_COMP Char(240),
  COD_USUA Char(10),
  AUT_COMP Char(60),
  DIG_COMP Char(60),
  NOM_CLIE Char(250),
  COD_CLIE Char(9),
  CHE_COMP Double precision,
  NUM_COMP Char(20),
  COD_PERI Char(3),
  COD_BANC Char(2),
  ADD_COMP Char(8),
  COD_ACTI Char(2),
  AMO_COMP Char(1) DEFAULT '0',
  AEL_COMP Char(1) DEFAULT '0',
  AUS_COMP Char(10) DEFAULT '',
  COD_XUSUA Char(10) DEFAULT current_user,
  FEC_XCOMP Timestamp DEFAULT current_timestamp ,
  NIP_XCOMP Char(20) DEFAULT '',
  DE2_COMP Varchar(240) DEFAULT '',
  NUM_TURN Integer DEFAULT 0,
  PRIMARY KEY (NUMMCOMP)
);
CREATE TABLE COMP1
(
  COD_COMP Char(2),
  NOM_COMP Char(250),
  CODTSECU Char(200),
  PRIMARY KEY (COD_COMP)
);
CREATE TABLE CONS
(
  NUM_CONS Integer,
  FEC_CONS Timestamp DEFAULT current_timestamp  ,
  FDE_CONS Timestamp DEFAULT current_timestamp  ,
  FHA_CONS Timestamp DEFAULT current_timestamp  ,
  FAC_CONS Timestamp DEFAULT current_timestamp  ,
  COR_CLIE Char(9) DEFAULT '',
  CDE_CLIE Char(9) DEFAULT '',
  TNI_CONS Double precision DEFAULT 0,
  TSI_CONS Double precision DEFAULT 0,
  IVA_CONS Double precision DEFAULT 0,
  DET_CONS Char(200) DEFAULT '',
  PAG_CONS Char(1) DEFAULT '3',
  SEC_DCTO Double precision,
  EST_CONS Char(1),
  MAX_CONS Double precision DEFAULT 0,
  SEC_DCTO2 Double precision DEFAULT 0,
  SEC_DCTO3 Double precision DEFAULT 0,
  SEC_DCTO4 Double precision DEFAULT 0,
  SEC_DCTO5 Double precision DEFAULT 0,
  SEC_DCTO6 Double precision DEFAULT 0,
  SEC_DCTO7 Double precision DEFAULT 0,
  SEC_DCTO8 Double precision DEFAULT 0,
  SEC_DCTO9 Double precision DEFAULT 0,
  TIP_CONS Char(2) DEFAULT 'EB',
  COD_ACTI Char(2) DEFAULT '01',
  COD_BODE Char(2) DEFAULT '01',
  COD_VEND Char(9) DEFAULT '',
  NUMMCOMP Integer DEFAULT 0,
  COD_BANC Char(2) DEFAULT '',
  REC_CONS_TARJ Integer DEFAULT 0,
  FAC_CONS_TARJ Double precision DEFAULT 0,
  BAU_CONS_TARJ Double precision DEFAULT 0,
  DIF_CONS_TARJ Double precision DEFAULT 0,
  TOT_PUNTOS Double precision DEFAULT 0,
  LOT_CONS Char(6) DEFAULT '',
  PUN_CONS Char(200) DEFAULT '',
  PLA_CONS Char(20) DEFAULT '',
  TIP_DCTO Char(2) DEFAULT 'FC',
  SEC_DCTO_DEV Integer DEFAULT 0,
  PRIMARY KEY (NUM_CONS)
);
CREATE TABLE CONS_MOV
(
  NUM_COMO Integer,
  NUM_CONS Integer DEFAULT 0,
  SEC_DCTO Double precision DEFAULT 0,
  TNI_DCTO Double precision DEFAULT 0,
  TSI_DCTO Double precision DEFAULT 0,
  IVA_DCTO Double precision DEFAULT 0,
  PRIMARY KEY (NUM_COMO)
);
CREATE TABLE CON_MOVI
(
  NUM_MOVI Integer,
  NUMMCOMP Double precision,
  COD_CUEN Char(25),
  DET_MOVI Char(240),
  VAL_MOVI Double precision,
  REF_MOVI Char(15),
  NUM_COBA Integer,
  CAN_ITEM Double precision DEFAULT 0,
  COS_ITEM Double precision DEFAULT 0,
  CONSTRAINT PK_CON_MOVI PRIMARY KEY (NUM_MOVI)
);
CREATE TABLE CON_MOVIP
(
  NUMMCOMP Double precision,
  COD_CUEN Char(25),
  DET_MOVI Char(240),
  VAL_MOVI Double precision,
  REF_MOVI Char(15),
  NUM_COBA Integer
);
CREATE TABLE CON_MOVI_PGDE
(
  NUM_CON_MOVI_PGDE Integer,
  NUM_PGDE Integer,
  NUM_MOVI Integer,
  NUM_PGCA Integer,
  NUMMCOMP Integer,
  PRIMARY KEY (NUM_CON_MOVI_PGDE)
);
CREATE TABLE CON_PEND
(
  NUM_PEND Double precision,
  COD_CUEN Char(25),
  DET_MOVI Char(240),
  VAL_MOVI Double precision,
  REF_MOVI Char(15)
);
CREATE TABLE COSM
(
  NUM_COSM Integer,
  COD_ITEM Char(100),
  MES_COSM Double precision,
  VAL_COSM Double precision,
  CONSTRAINT PK_COSM PRIMARY KEY (NUM_COSM)
);
CREATE TABLE COST
(
  COD_REPO Char(20),
  COD_ITEM Char(100),
  NOM_ITEM Char(100),
  STO_ITEM Double precision,
  SUN_ITEM Double precision,
  COS_ITEM Double precision,
  CUN_ITEM Double precision,
  COD_GRUP Char(4),
  NOM_GRUP Char(50),
  CODSGRUP Char(4),
  NOMSGRUP Char(50),
  CONSTRAINT PK_COST PRIMARY KEY (COD_REPO,COD_ITEM)
);
CREATE TABLE CRED
(
  COD_CRED Char(2),
  NOM_CRED Char(100),
  TAZ_CRED Double precision,
  CPR_CUEN Char(25),
  CIX_CUEN Char(25),
  CIC_CUEN Char(25),
  CIO_CUEN Char(25),
  CPO_CUEN Char(15),
  TPL_CRED Char(1) DEFAULT '1',
  PRIMARY KEY (COD_CRED)
);
CREATE TABLE CRED_ADDE
(
  NUM_CABE Double precision,
  COD_DECR Char(4),
  VAL_ADDE Double precision
);
CREATE TABLE CRED_CABE
(
  NUM_CABE Double precision,
  FEC_CABE Timestamp,
  COD_CRED Char(2),
  COD_SOCI Char(9),
  PLA_CABE Integer,
  TAZ_CRED Double precision,
  COD_GARA Char(9),
  FPA_CABE Char(2),
  TCR_CABE Double precision,
  TIN_CABE Double precision,
  PCR_CABE Double precision,
  PIN_CABE Double precision,
  COD_BANC Char(2),
  NUMMCOMP Double precision,
  TOO_CABE Double precision,
  POO_CABE Double precision,
  CHE_CABE Double precision,
  CUO_CABE Double precision,
  CIN_CABE Double precision,
  CPR_CABE Double precision,
  VBA_CABE Double precision,
  SCP_CABE Char(1) DEFAULT 'C',
  DIA_CABE Integer DEFAULT 0,
  FUL_CABE Timestamp DEFAULT current_timestamp ,
  CICA_CABE Integer DEFAULT 0,
  PRIMARY KEY (NUM_CABE)
);
CREATE TABLE CRED_DECR
(
  COD_DECR Char(4),
  NOM_DECR Char(100),
  COD_CRED Char(2),
  DCR_DECR Char(1),
  POR_DECR Double precision,
  COD_CUEN Char(25),
  VAL_DECR Double precision,
  FIN_DECR Timestamp,
  FFI_DECR Timestamp,
  PRIMARY KEY (COD_DECR)
);
CREATE TABLE CRED_MOVI
(
  NUM_CABE Double precision,
  NUM_MOVI Integer,
  TCR_MOVI Double precision,
  TPACR_MOVI Double precision,
  TIN_MOVI Double precision,
  TPAIN_MOVI Double precision,
  FVE_MOVI Timestamp,
  FPA_MOVI Timestamp,
  BPA_MOVI Char(1),
  NUMMCOMP Double precision,
  REF_MOVI Char(15),
  COD_TIPO Char(4),
  CIN_MOVI Double precision,
  COD_MOVI Char(7),
  FUL_MOVI Timestamp DEFAULT current_timestamp ,
  CICA_MOVI Integer DEFAULT 0
);
CREATE TABLE CRED_TIPO
(
  COD_TIPO Char(4),
  NOM_TIPO Char(50),
  DCR_TIPO Char(1),
  CBA_BANC Char(1),
  PRIMARY KEY (COD_TIPO)
);
CREATE TABLE CUEN
(
  COD_CUEN Char(25),
  NOM_CUEN Char(200),
  FAP_CUEN Timestamp,
  FUM_CUEN Timestamp,
  SAA_CUEN Double precision,
  COD_ACTI Char(2),
  BMO_CUEN Char(1),
  TIP_CUEN Char(2),
  INT_CUEN Char(9),
  BIN_CUEN Char(1),
  BPR_CUEN Char(1),
  F11_CUEN Char(10),
  COD_DEPA Char(2),
  DB_01 Double precision DEFAULT 0,
  CR_01 Double precision DEFAULT 0,
  DB_02 Double precision DEFAULT 0,
  CR_02 Double precision DEFAULT 0,
  DB_03 Double precision DEFAULT 0,
  CR_03 Double precision DEFAULT 0,
  DB_04 Double precision DEFAULT 0,
  CR_04 Double precision DEFAULT 0,
  DB_05 Double precision DEFAULT 0,
  CR_05 Double precision DEFAULT 0,
  DB_06 Double precision DEFAULT 0,
  CR_06 Double precision DEFAULT 0,
  DB_07 Double precision DEFAULT 0,
  CR_07 Double precision DEFAULT 0,
  DB_08 Double precision DEFAULT 0,
  CR_08 Double precision DEFAULT 0,
  DB_09 Double precision DEFAULT 0,
  CR_09 Double precision DEFAULT 0,
  DB_10 Double precision DEFAULT 0,
  CR_10 Double precision DEFAULT 0,
  DB_11 Double precision DEFAULT 0,
  CR_11 Double precision DEFAULT 0,
  DB_12 Double precision DEFAULT 0,
  CR_12 Double precision DEFAULT 0,
  DB_00 Double precision DEFAULT 0,
  CR_00 Double precision DEFAULT 0,
  NV_00 Char(25) DEFAULT '-',
  PRIMARY KEY (COD_CUEN)
);
CREATE TABLE CUOT
(
  NUM_CUOT Double precision,
  SEC_DCTO Double precision,
  ORD_CUOT Integer,
  FVE_CUOT Timestamp,
  VAL_CUOT Double precision,
  INT_CUOT Double precision DEFAULT 0,
  OTR_CUOT Double precision DEFAULT 0,
  PAG_CUOT Double precision DEFAULT 0,
  BPA_PAGO Char(1),
  NUMMCOMP Double precision,
  PRIMARY KEY (NUM_CUOT)
);
CREATE TABLE DCTO
(
  SEC_DCTO Double precision,
  TIP_DCTO Char(2),
  NUM_DCTO Char(20),
  FEC_DCTO Timestamp,
  COD_CLIE Char(9),
  TNI_DCTO Double precision,
  TSI_DCTO Double precision,
  DSC_DCTO Double precision,
  IVA_DCTO Double precision,
  COD_BODE Char(2),
  NCU_DCTO Integer,
  DCU_DCTO Integer,
  DET_DCTO Char(250),
  OBS_DCTO Char(250),
  RUC_DCTO Char(13),
  DIR_DCTO Char(250),
  COD_VEND Char(9),
  COD_PAGO Char(3),
  NDO_DCTO Char(20),
  BPA_DCTO Char(1),
  PAG_DCTO Double precision,
  COL_DCTO Char(50),
  NUMMCOMP Double precision,
  NUM_LIQV Double precision,
  GUI_DCTO Varchar(50),
  FGU_DCTO Timestamp,
  COD_ACTI Char(2),
  ANE_DCTO Char(1) DEFAULT '1',
  AUT_DCTO Char(50) DEFAULT '',
  ESE_DCTO Char(4) DEFAULT '',
  DEE_DCTO Char(200) DEFAULT '',
  CLE_DCTO Char(100) DEFAULT '',
  SER_DCTO Double precision DEFAULT 0,
  PLA_DCTO Char(20) DEFAULT '',
  ORD_DCTO Char(20) DEFAULT '',
  NUM_TURN Integer DEFAULT 0,
  VAC_DCTO Char(2) DEFAULT '01',
  COD_CHOF Char(9) DEFAULT '000000000',
  NUM_CONS Integer DEFAULT 0,
  SUB_DCTO Double precision DEFAULT 0,
  TER_DCTO Double precision DEFAULT 0,
  FUL_CABE Timestamp DEFAULT current_timestamp ,
  TAZ_CRED Timestamp DEFAULT current_timestamp ,
  PCM_DCTO Integer DEFAULT 0,
  VCM_DCTO Double precision DEFAULT 0,
  PCMR_DCTO Integer DEFAULT 0,
  VCMR_DCTO Double precision DEFAULT 0,
  DNI_DCTO Double precision DEFAULT 0,
  DSI_DCTO Double precision DEFAULT 0,
  IMP_DCTO Double precision DEFAULT 0,
  ESG_DCTO Char(4) DEFAULT '',
  DEG_DCTO Varchar(200) DEFAULT '',
  CLG_DCTO Varchar(100) DEFAULT '',
  AUG_DCTO Varchar(50) DEFAULT '',
  PIV_DCTO Integer DEFAULT 14,
  COD_GRUP Char(4) DEFAULT '',
  COD_XUSUA Char(10) DEFAULT current_user,
  FEC_XDCTO Timestamp DEFAULT current_timestamp ,
  NIP_XDCTO Char(20) DEFAULT '',
  COD_MANG Char(2) DEFAULT '00',
  NUM_TICK_MOV Integer DEFAULT 0,
  NUM_VAUC Integer DEFAULT 0,
  DSCA0_DCTO Double precision DEFAULT 0,
  DSCA_DCTO Double precision DEFAULT 0,
  PLG_DCTO Varchar(20) DEFAULT '',
  PRIMARY KEY (SEC_DCTO)
);
CREATE TABLE DCTO_DEV
(
  SEC_DCTO Integer,
  TIP_DCTO_DEV Char(2),
  SEC_DCTO_DEV Integer,
  NUM_DCTO_DEV Char(20),
  AUT_DCTO_DEV Char(50),
  FEC_DCTO_DEV Timestamp,
  PRIMARY KEY (SEC_DCTO)
);
CREATE TABLE DEPA
(
  COD_DEPA Char(25),
  NOM_DEPA Char(100),
  RES_DEPA Char(100),
  COD_ACTI Char(2),
  PRIMARY KEY (COD_DEPA)
);
CREATE TABLE DEPR
(
  NUM_DEPR Double precision,
  FEC_DEPR Timestamp,
  DES_DEPR Char(100),
  TAC_DEPR Double precision,
  TDA_DEPR Double precision,
  TDE_DEPR Double precision,
  NUMMCOMP Double precision,
  COD_ACTI Char(2),
  FUD_DEPR Timestamp,
  PRIMARY KEY (NUM_DEPR)
);
CREATE TABLE DEP_MOVI
(
  NUM_DEPR Double precision,
  COD_ACFI Char(25),
  FUD_ACFI Timestamp,
  DAC_ACFI Double precision,
  DEP_MOVI Double precision,
  FAD_ACFI Timestamp
);
CREATE TABLE DESP
(
  NUM_DESP Double precision,
  COD_MANG Char(2),
  FEC_DESP Char(50),
  VTO_DESP Double precision,
  CAN_DESP Double precision,
  VUN_DESP Double precision,
  COD_PROD Char(2),
  NOM_PROD Char(20),
  EST_DESP Char(1),
  FAC_DESP Char(1),
  SUR_DESP Char(4),
  VEN_PUNT Char(10),
  NUM_LIQU Double precision,
  VO1_PUNT Char(21),
  VO2_PUNT Char(21),
  VO3_PUNT Char(21),
  VO4_PUNT Char(21),
  IM1_PUNT Char(21),
  IM2_PUNT Char(21),
  IM3_PUNT Char(21),
  IM4_PUNT Char(21),
  COD_CLIE Char(9),
  FIN_DESP Timestamp DEFAULT current_timestamp ,
  COD_MAN1 Char(2) DEFAULT '00',
  LOT_PUNT Char(10) DEFAULT '',
  TTA_PUNT Char(1) DEFAULT '',
  CONSTRAINT PK_DESP PRIMARY KEY (NUM_DESP)
);
CREATE TABLE DET_INVFOTOS
(
  SEC_INVINFPRODUCTOS Double precision,
  SEC_MICROPLUS Double precision,
  FOTO_TOMAFISICA Blob sub_type 0,
  USUARIO Char(20),
  FECHA Timestamp DEFAULT current_timestamp,
  PRIMARY KEY (SEC_INVINFPRODUCTOS)
);
CREATE TABLE DET_INVINFPRODUCTOS
(
  SEC_INVINFPRODUCTOS Double precision,
  COD_ITEM Char(25),
  SEC_MICROPLUS Double precision,
  CAN_TOMAFISICA Double precision,
  CAN_COMPRASRECIBIDAS Double precision,
  CAN_VENTASDIA Double precision,
  CAN_DESPACHOSPREPAGOS Double precision,
  CAN_DEVOLUCIONTANQUES Double precision,
  USUARIO Char(20),
  FECHA Timestamp DEFAULT current_timestamp,
  PRECIO Double precision,
  GUIAREMISION Char(50),
  PRIMARY KEY (SEC_INVINFPRODUCTOS)
);
CREATE TABLE DET_INVINFTURNOS
(
  SEC_INVINFTURNOS Double precision,
  SEC_MOLI Double precision,
  SEC_MICROPLUS Double precision,
  NUM_LIQU Double precision,
  USUARIO Char(20),
  FECHA Timestamp DEFAULT current_timestamp,
  PRIMARY KEY (SEC_INVINFTURNOS)
);
CREATE TABLE DET_INVLIQUIDACION
(
  SEC_MICROPLUS Double precision,
  ARCHIVO_LIQUIDACION Blob sub_type 0,
  USUARIO Char(20),
  FECHA Timestamp DEFAULT current_timestamp,
  PRIMARY KEY (SEC_MICROPLUS)
);
CREATE TABLE DET_MICROINFTURNOS
(
  SEC_MICROINFTURNOS Double precision,
  SEC_MOLI Double precision,
  SEC_MICROPLUS Double precision,
  NUM_LIQU Double precision,
  USUARIO Char(20),
  FECHA Timestamp DEFAULT current_timestamp,
  PRIMARY KEY (SEC_MICROINFTURNOS)
);
CREATE TABLE ELES
(
  EST_ELES Char(3) DEFAULT '001',
  COD_EMPR Char(3) DEFAULT '001',
  NOM_ELES Char(300) DEFAULT '',
  COD_ACTI Char(2) DEFAULT '01',
  DIR_ELES Char(300) DEFAULT '',
  TIP_ELES Char(1) DEFAULT '1',
  CLA_ELES Char(8) DEFAULT '12345678',
  CONSTRAINT PK_ELES PRIMARY KEY (EST_ELES)
);
CREATE TABLE ELPU
(
  EST_ELES Char(3) DEFAULT '001',
  PUN_ELPU Char(3) DEFAULT '001',
  TIP_ELPU Char(2) DEFAULT '01',
  NOM_ELPU Char(100) DEFAULT '',
  SEC_ELPU Integer DEFAULT '1',
  PLA_ELPU Char(1) DEFAULT '1',
  ELE_ELPU Char(1) DEFAULT '1',
  CONSTRAINT PK_ELPU PRIMARY KEY (EST_ELES,PUN_ELPU,TIP_ELPU)
);
CREATE TABLE EMPL
(
  NUM_EMPL Integer,
  TID_EMPL Char(1),
  RUC_EMPL Char(13),
  NOM_EMPL Char(100),
  DCA_EMPL Char(100),
  DNU_EMPL Char(10),
  TE1_EMPL Char(10),
  TE2_EMPL Char(10),
  FAX_EMPL Char(10),
  COD_PROV Char(3),
  COD_CIUD Char(5),
  CAR_EMPL Char(100),
  SSN_EMPL Char(1),
  GVI_EMPL Double precision,
  GED_EMPL Double precision,
  GAL_EMPL Double precision,
  GVE_EMPL Double precision,
  GES_EMPL Double precision,
  IOT_EMPL Double precision,
  GOT_EMPL Double precision,
  ISB_EMPL Double precision,
  FIN_EMPL Timestamp,
  FFI_EMPL Timestamp,
  FNA_EMPL Timestamp,
  EST_EMPL Char(1),
  DIA_EMPL Double precision,
  RES_EMPL Char(1),
  MES_EMPL Timestamp,
  COD_DEPA Char(2) DEFAULT '01',
  BEN_EMPL Char(1) DEFAULT '0',
  DIB_EMPL Double precision DEFAULT 30,
  IES_EMPL Double precision DEFAULT 30,
  HOR_EMPL Integer DEFAULT 240,
  COD_CUEN Char(25) DEFAULT '',
  MAI_EMPL Char(100) DEFAULT '',
  TIP_FALT_EMPL Char(1) DEFAULT 'N',
  ORI_FALT_EMPL Char(1) DEFAULT 'M',
  ESP_EMPL Char(1) DEFAULT '0',
  CONSTRAINT PK_EMPL PRIMARY KEY (NUM_EMPL)
);
CREATE TABLE EMPL_PERS
(
  NUM_EMPL Integer,
  NUM_RUBR Integer,
  COD_CUEN Char(25),
  CONSTRAINT PK_EMPL_PERS_1 PRIMARY KEY (NUM_EMPL,NUM_RUBR,COD_CUEN)
);
CREATE TABLE EMPR
(
  COD_EMPR Char(3),
  NOM_EMPR Char(100),
  DIR_EMPR Char(100),
  RUC_EMPR Char(13),
  TE1_EMPR Char(10),
  TE2_EMPR Char(10),
  FAX_EMPR Char(9),
  EME_EMPR Char(100),
  RCA_EMPR Char(50),
  RNO_EMPR Char(50),
  JCA_EMPR Char(50),
  JNO_EMPR Char(50),
  CCA_EMPR Char(50),
  CNO_EMPR Char(50),
  ACA_EMPR Char(50),
  ANO_EMPR Char(50),
  CIN_EMPR Char(25),
  CEG_EMPR Char(25),
  DPL_EMPR Char(25),
  IVA_EMPR Double precision,
  RRU_EMPR Char(13),
  CRU_EMPR Char(13),
  RTP_EMPR Char(1),
  DEP_EMPR Char(25),
  REV_EMPR Char(25),
  DIMP_103_EMPR Char(25),
  SINT_103_EMPR Char(25),
  SMUL_103_EMPR Char(25),
  DNCX_103_EMPR Char(25),
  DIMP_104_EMPR Char(25),
  SINT_104_EMPR Char(25),
  SMUL_104_EMPR Char(25),
  DNCX_104_EMPR Char(25),
  DCOX_104_EMPR Char(25),
  CTRA_104_EMPR Char(25),
  CTRR_104_EMPR Char(25),
  GIVA_104_EMPR Char(25),
  EST_EMPR Char(3) DEFAULT '001',
  PLA_EMPR Char(25) DEFAULT '12233445566',
  CIU_EMPR Char(50) DEFAULT '',
  ESP_EMPR Char(10) DEFAULT '0101',
  CON_EMPR Char(1) DEFAULT '0',
  CBA_EMPR Char(1) DEFAULT '1',
  SUB_EMPR Char(1) DEFAULT '0',
  BTE_EMPR Char(1) DEFAULT '0',
  LRI_EMPR Double precision DEFAULT 0,
  FPA_EMPR Char(1) DEFAULT '0',
  COM_EMPR Char(1) DEFAULT '0',
  PCM_EMPR Integer DEFAULT 0,
  CUP_EMPR Char(1) DEFAULT '0',
  SRI_RETASUM_EMPR Char(10) DEFAULT '',
  SER_BAS_EMPR Char(1) DEFAULT '0',
  HUM_BAS_EMPR Char(1) DEFAULT '0',
  IMP_BAS_EMPR Char(1) DEFAULT '0',
  CHUM_BAS_EMPR Char(1) DEFAULT '0',
  RUT_FIR_EMPR Char(220) DEFAULT '',
  CLA_FIR_EMPR Char(50) DEFAULT '',
  BKL_EMPR Char(1) DEFAULT '0',
  CLA_EME_EMPR Varchar(200) DEFAULT '',
  LOG_EME_EMPR Char(220) DEFAULT '',
  BMA_EMPR Char(1) DEFAULT '0',
  DIF_LIQU_EMPR Char(1) DEFAULT '0',
  NOM_COM_EMPR Char(200) DEFAULT '',
  BKL_ELE_EMPR Char(1) DEFAULT '0',
  PLACA_EMPR Char(1) DEFAULT '0',
  COM_PAG_EMPR Char(1) DEFAULT '0',
  CAD_FIR_EMPR Timestamp,
  CAD_STR_FIR_EMPR Char(100) DEFAULT '',
  HOR_EXTR_EMPR Char(1) DEFAULT '0',
  NUM_RUBR_FAL_EMPR Integer DEFAULT 0,
  HTML_EME_EMPR Char(1) DEFAULT '0',
  TIP_EME_EMPR Char(1) DEFAULT '1',
  COR_HTML_EMPR Blob sub_type 0,
  BHO_PAV_EMPR Char(1) DEFAULT '0',
  HOR_EB_EMPR Char(1) DEFAULT '0',
  LEY_MIC_EMPR Char(50) DEFAULT '',
  RES_MIC_EMPR Char(20) DEFAULT '',
  CARD_EMPR Char(10) DEFAULT '',
  CON_LICA_EMPR Char(1) DEFAULT '0',
  MOD_CONT_EMPR Char(1) DEFAULT '1',
  MOD_INVE_EMPR Char(1) DEFAULT '1',
  MOD_FACT_EMPR Char(1) DEFAULT '1',
  MOD_CLIE_EMPR Char(1) DEFAULT '1',
  MOD_PROV_EMPR Char(1) DEFAULT '1',
  MOD_BANC_EMPR Char(1) DEFAULT '1',
  MOD_PERS_EMPR Char(1) DEFAULT '1',
  MOD_GASO_EMPR Char(1) DEFAULT '1',
  MOD_BASC_EMPR Char(1) DEFAULT '1',
  DES_PRIOS_EMPR Char(1) DEFAULT '0',
  COR_PRIOS_EMPR Char(100) DEFAULT '',
  CLA_PRIOS_EMPR Char(100) DEFAULT '',
  DES_DB_EMPR Char(1) DEFAULT '0',
  RIMPE_EMPR Char(100) DEFAULT '',
  DES_DEPO_EMPR Char(1) DEFAULT '0',
  REC_EME_EMPR Char(250) DEFAULT '',
  RENT_3X1_EMPR Char(10) DEFAULT '9002',
  IVA_PRES_EMPR Char(10) DEFAULT '9001',
  CIE_MAR_FAL_EMPR Char(1) DEFAULT '0',
  RES_ISL_CLA_EMPR Char(10) DEFAULT '',
  MODI_BAS_EMPR Char(1) DEFAULT '0',
  CLA_MAE_EMPR Char(20) DEFAULT 'aster01.',
  POPULAR_EMPR Char(1) DEFAULT '0',
  IR332_EMPR Char(1) DEFAULT '0',
  INV_SAL_PERI_EMPR Char(1) DEFAULT '0',
  BAL_SAL_PERI_EMPR Char(1) DEFAULT '0',
  CXC_SAL_PERI_EMPR Char(1) DEFAULT '0',
  CXP_SAL_PERI_EMPR Char(1) DEFAULT '0',
  ARTE_EMPR Char(100) DEFAULT '',
  VAL_CUA_BAS_EMPR Double precision DEFAULT 0,
  UPDATE_EMPR Char(20) DEFAULT '',
  COD_CLIE_PRUEBAS Char(9) DEFAULT '',
  PRIMARY KEY (COD_EMPR)
);
CREATE TABLE ESTA
(
  NUM_ESTA Integer,
  EST_ESTA Char(10),
  MES_ESTA Char(6),
  VAL_ESTA Double precision,
  NFA_ESTA Integer DEFAULT 0,
  VNC_ESTA Double precision DEFAULT 0,
  NNC_ESTA Integer DEFAULT 0,
  CONSTRAINT PK_ESTA PRIMARY KEY (NUM_ESTA)
);
CREATE TABLE ESTR
(
  COD_ESTR Char(10),
  NOM_ESTR Char(150) DEFAULT '',
  COD_ACTI Char(2) DEFAULT '',
  COD_BODE Char(2) DEFAULT '',
  COD_VEND Char(9) DEFAULT '',
  PAG_ESTR Integer DEFAULT 0,
  RUC_ESTR Integer DEFAULT 0,
  NMB_ESTR Integer DEFAULT 0,
  FEC_ESTR Integer DEFAULT 0,
  TNI_ESTR Integer DEFAULT 0,
  TSI_ESTR Integer DEFAULT 0,
  IVA_ESTR Integer DEFAULT 0,
  TOT_ESTR Integer DEFAULT 0,
  ITE_ESTR Integer DEFAULT 0,
  SEC_ESTR Integer DEFAULT 0,
  NUM_ESTR Integer DEFAULT 0,
  CON_ESTR Char(200) DEFAULT '',
  SQ1_ESTR Char(200) DEFAULT '',
  SQ2_ESTR Char(200) DEFAULT '',
  SQ3_ESTR Char(200) DEFAULT '',
  SQ4_ESTR Char(200) DEFAULT '',
  ESE_ESTR Integer DEFAULT 0,
  CLE_ESTR Integer DEFAULT 0,
  AUE_ESTR Integer DEFAULT 0,
  TIP_ESTR Integer DEFAULT 0,
  BAU_ESTR Char(1) DEFAULT '0',
  BSO_ESTR Char(1) DEFAULT '0',
  COD_GRUP Char(4) DEFAULT '',
  CON1_ESTR Char(200) DEFAULT '',
  CON2_ESTR Char(200) DEFAULT '',
  CON3_ESTR Char(200) DEFAULT '',
  CON4_ESTR Char(200) DEFAULT '',
  CON5_ESTR Char(200) DEFAULT '',
  PRP_ESTR Integer DEFAULT 0,
  CO2_ESTR Char(2) DEFAULT '00',
  SQ5_ESTR Varchar(200) DEFAULT '',
  SSD_ESTR Integer DEFAULT 0,
  SSI_ESTR Integer DEFAULT 0,
  SSTI_ESTR Integer DEFAULT 0,
  DCU_ESTR Integer DEFAULT 0,
  TID_ESTR Integer DEFAULT 0,
  VEN_ESTR Integer DEFAULT 0,
  PLACA_ESTR Integer DEFAULT 0,
  GRUP_ESTR Integer DEFAULT 0,
  LOTE_ESTR Integer DEFAULT 0,
  VAUC_ESTR Integer DEFAULT 0,
  PUN_ESTR Char(1) DEFAULT '0',
  KPL_ESTR Integer DEFAULT 0,
  KKL_ESTR Integer DEFAULT 0,
  KCH_ESTR Integer DEFAULT 0,
  KIL_CAMP_ESTR Char(200) DEFAULT '',
  KIL_JOIN_ESTR Char(200) DEFAULT '',
  CAN_UNI_ESTR Integer DEFAULT 0,
  VAL_UNI_ESTR Integer DEFAULT 0,
  SUB_UNI_ESTR Integer DEFAULT 0,
  IVA_UNI_ESTR Integer DEFAULT 0,
  GUI_ESTR Integer DEFAULT 0,
  CHO_ESTR Integer DEFAULT 0,
  CONS_ESTR Integer DEFAULT 0,
  CONSTRAINT PK_ESTR PRIMARY KEY (COD_ESTR)
);
CREATE TABLE ESTR_MOV
(
  NUM_ESMO Integer,
  COD_ESTR Char(2),
  COD_ORIG Char(25),
  COD_DESP Char(25),
  CONSTRAINT PK_ESTR_MOV PRIMARY KEY (NUM_ESMO)
);
CREATE TABLE ESTR_VEN
(
  NUM_ESVE Integer,
  COD_ESTR Char(2),
  COD_ORIG Char(9),
  COD_DESP Char(9),
  PRIMARY KEY (NUM_ESVE)
);
CREATE TABLE F104
(
  NUM_F104 Integer,
  COD_M104 Char(5),
  FOR_R104 Char(3),
  NOM_F104 Char(100),
  EST_F104 Char(1),
  SUM_F104 Char(1),
  SQ1_F104 Char(250),
  SQ2_F104 Char(250)
);
CREATE TABLE FACT
(
  NUM_FACT Integer,
  COD_CLIE Char(9),
  MES_FACT Char(6),
  CAN_FACT Double precision,
  PRIMARY KEY (NUM_FACT)
);
CREATE TABLE FOTO
(
  COD_CLIE Char(9) DEFAULT '',
  CLA_DCTO Char(50) DEFAULT '',
  FOTO Blob sub_type 0,
  BARRA1D Blob sub_type 0,
  BARRA2D Blob sub_type 0,
  NUM_DCTO Integer DEFAULT 0
);
CREATE TABLE GRUP
(
  COD_GRUP Char(4),
  NOM_GRUP Char(200),
  TIP_GRUP Char(1),
  DEV_GRUP Char(1),
  NME_GRUP Integer,
  COD_SUST Char(2),
  COD_RENT Char(5),
  COD_RIVA Char(5),
  COM_GRUP Char(1) DEFAULT '0',
  COD_BANC Char(2) DEFAULT '',
  PRIMARY KEY (COD_GRUP)
);
CREATE TABLE IAIR
(
  NUMTL Double precision,
  CODRETAIR Char(5),
  BASEIMPAIR Double precision,
  PORCENTAJEAIR Double precision,
  VALRETAIR Double precision,
  BASEIMPAIRGRAV Double precision,
  BASEIMPAIRNOGRAV Double precision,
  COD_CUEN Char(25),
  NUMIAIR Integer,
  ANIIAIR Char(4) DEFAULT '2020',
  FECDIVIAIR Timestamp DEFAULT current_timestamp ,
  VALDIVIAIR Double precision DEFAULT 0
);
CREATE TABLE IAIRV
(
  NUM_TLVV Double precision,
  CODRETAIR Char(5),
  BASEIMPAIR Double precision,
  PORCENTAJEAIR Double precision,
  VALRETAIR Double precision,
  COD_CUEN Char(25),
  NUMIAIRV Integer,
  PRIMARY KEY (NUM_TLVV)
);
CREATE TABLE IICE
(
  NOM_IICE Char(250),
  FAC_IICE Double precision,
  FIN_IICE Timestamp,
  FFI_IICE Timestamp,
  ORI_IICE Char(4),
  COD_IICE Integer
);
CREATE TABLE IMPR
(
  NUM_IMPR Integer,
  NOM_IMPR Char(250) DEFAULT '',
  EST_IMPR Char(3),
  PUN_IMPR Char(3),
  SEC_IMPR Char(20),
  MAX_IMPR Char(20),
  LOT_IMPR Integer DEFAULT 100,
  CONSTRAINT PK_IMPR PRIMARY KEY (NUM_IMPR)
);
CREATE TABLE IMPU
(
  COD_IMPU Char(2),
  NOM_IMPU Char(50) DEFAULT '',
  FAC_IMPU Double precision DEFAULT 0,
  REN_IMPU Char(1) DEFAULT '0',
  PRIMARY KEY (COD_IMPU)
);
CREATE TABLE ITEM
(
  NOM_ITEM Char(400),
  NCA_ITEM Char(30),
  UNI_ITEM Double precision,
  NUN_ITEM Char(30),
  VEA_ITEM Double precision,
  VEB_ITEM Double precision,
  VEC_ITEM Double precision,
  VUA_ITEM Double precision,
  VUB_ITEM Double precision,
  VUC_ITEM Double precision,
  IVA_ITEM Char(1),
  SER_ITEM Char(1),
  ICE_ITEM Char(1),
  COD_GRUP Char(4),
  UCO_ITEM Double precision,
  UIN_ITEM Timestamp,
  UEG_ITEM Timestamp,
  MIN_ITEM Double precision,
  MAX_ITEM Double precision,
  DEA_ITEM Double precision,
  DEB_ITEM Double precision,
  DEC_ITEM Double precision,
  COD_ITEM Char(25),
  CBA_ITEM Char(20) DEFAULT '',
  ARC_ITEM Char(20) DEFAULT '',
  SUB_ITEM Double precision DEFAULT 0,
  PIV_ITEM Double precision DEFAULT 0,
  COD_BODE Char(2) DEFAULT '00',
  ACT_ITEM Char(1) DEFAULT '1',
  COD_IMPU Char(2) DEFAULT '00',
  FTR_ITEM Double precision DEFAULT 1,
  COD_COMI Char(2) DEFAULT '00',
  HFI_ITEM Double precision DEFAULT 0,
  IFI_ITEM Double precision DEFAULT 0,
  TNQ_ITEM Char(1) DEFAULT 'N',
  PEV_ITEM Double precision DEFAULT 0,
  ADD_ITEM Char(1) DEFAULT 'N',
  CID_ITEM Integer DEFAULT 0,
  CUA_ITEM Char(10) DEFAULT '',
  ALI_ITEM Char(30) DEFAULT '',
  DIT_ITEM Char(1) DEFAULT '0',
  COM_ITEM Char(1) DEFAULT '0',
  BAR_INI_ITEM Char(1) DEFAULT '0',
  REF_ITEM Char(400) DEFAULT '',
  UBI_A_ITEM Char(20) DEFAULT '',
  UBI_B_ITEM Char(20) DEFAULT '',
  UBI_C_ITEM Char(20) DEFAULT '',
  COD_CUEN Char(20) DEFAULT '',
  PUN_ITEM Double precision DEFAULT 0,
  ISL_ITEM Char(1) DEFAULT '0',
  PRIMARY KEY (COD_ITEM)
);
CREATE TABLE ITEMM
(
  COD_ITEM Char(100),
  DB_01 Double precision DEFAULT 0,
  DB_02 Double precision DEFAULT 0,
  DB_03 Double precision DEFAULT 0,
  DB_04 Double precision DEFAULT 0,
  DB_05 Double precision DEFAULT 0,
  DB_06 Double precision DEFAULT 0,
  DB_07 Double precision DEFAULT 0,
  DB_08 Double precision DEFAULT 0,
  DB_09 Double precision DEFAULT 0,
  DB_10 Double precision DEFAULT 0,
  DB_11 Double precision DEFAULT 0,
  DB_12 Double precision DEFAULT 0,
  CR_01 Double precision DEFAULT 0,
  CR_02 Double precision DEFAULT 0,
  CR_03 Double precision DEFAULT 0,
  CR_04 Double precision DEFAULT 0,
  CR_05 Double precision DEFAULT 0,
  CR_06 Double precision DEFAULT 0,
  CR_07 Double precision DEFAULT 0,
  CR_08 Double precision DEFAULT 0,
  CR_09 Double precision DEFAULT 0,
  CR_10 Double precision DEFAULT 0,
  CR_11 Double precision DEFAULT 0,
  CR_12 Double precision DEFAULT 0,
  CS_01 Double precision DEFAULT 0,
  CS_02 Double precision DEFAULT 0,
  CS_03 Double precision DEFAULT 0,
  CS_04 Double precision DEFAULT 0,
  CS_05 Double precision DEFAULT 0,
  CS_06 Double precision DEFAULT 0,
  CS_07 Double precision DEFAULT 0,
  CS_08 Double precision DEFAULT 0,
  CS_09 Double precision DEFAULT 0,
  CS_10 Double precision DEFAULT 0,
  CS_11 Double precision DEFAULT 0,
  CS_12 Double precision DEFAULT 0,
  VEN_01 Double precision DEFAULT 0,
  COM_01 Double precision DEFAULT 0,
  VEN_02 Double precision DEFAULT 0,
  COM_02 Double precision DEFAULT 0,
  VEN_03 Double precision DEFAULT 0,
  COM_03 Double precision DEFAULT 0,
  VEN_04 Double precision DEFAULT 0,
  COM_04 Double precision DEFAULT 0,
  VEN_05 Double precision DEFAULT 0,
  COM_05 Double precision DEFAULT 0,
  VEN_06 Double precision DEFAULT 0,
  COM_06 Double precision DEFAULT 0,
  VEN_07 Double precision DEFAULT 0,
  COM_07 Double precision DEFAULT 0,
  VEN_08 Double precision DEFAULT 0,
  COM_08 Double precision DEFAULT 0,
  VEN_09 Double precision DEFAULT 0,
  COM_09 Double precision DEFAULT 0,
  VEN_10 Double precision DEFAULT 0,
  COM_10 Double precision DEFAULT 0,
  VEN_11 Double precision DEFAULT 0,
  COM_11 Double precision DEFAULT 0,
  VEN_12 Double precision DEFAULT 0,
  COM_12 Double precision DEFAULT 0,
  DB_00 Double precision DEFAULT 0,
  CR_00 Double precision DEFAULT 0,
  DB_99 Double precision DEFAULT 0,
  CR_99 Double precision DEFAULT 0,
  NUM_MOVI_INGR Integer DEFAULT 0,
  NUM_MOVI_EGRE Integer DEFAULT 0,
  CONSTRAINT PK_ITEMM PRIMARY KEY (COD_ITEM)
);
CREATE TABLE ITEM_BARRA
(
  COD_ITEM Char(25),
  NUM_ITEM_BARRA Integer DEFAULT 0
);
CREATE TABLE ITEM_COMBO
(
  COD_ITEM Char(25),
  COD_ITEM_COMBO Char(25) DEFAULT '',
  CAN_ITEM_COMBO Decimal(10,5) DEFAULT 0,
  CONSTRAINT PK_ITEM_COMBO_1 PRIMARY KEY (COD_ITEM,COD_ITEM_COMBO)
);
CREATE TABLE ITEM_COST
(
  COD_ITEM Char(100),
  FEC_ITEM_COST Timestamp,
  SCA_ITEM_COST Decimal(10,3) DEFAULT 0,
  SUN_ITEM_COST Decimal(10,3) DEFAULT 0,
  CCA_ITEM_COST Decimal(14,6) DEFAULT 0,
  CUN_ITEM_COST Decimal(14,6) DEFAULT 0,
  SEC_DCTO Integer,
  CONSTRAINT PK_ITEM_COST_1 PRIMARY KEY (COD_ITEM,SEC_DCTO)
);
CREATE TABLE ITEM_MAMO
(
  NUM_ITEM_MAMO Integer,
  COD_ITEM Char(100),
  COD_ITEM_MARCA Char(4),
  DES_ITEM_MAMO Char(4) DEFAULT '',
  HAS_ITEM_MAMO Char(4) DEFAULT '',
  PRIMARY KEY (NUM_ITEM_MAMO)
);
CREATE TABLE ITEM_MARCA
(
  COD_ITEM_MARCA Char(4),
  NOM_ITEM_MARCA Char(100) DEFAULT '',
  DET_ITEM_MARCA Char(300) DEFAULT '',
  PRIMARY KEY (COD_ITEM_MARCA)
);
CREATE TABLE IVA
(
  COD_IVA Char(2),
  COD_POR_IVA Char(2) DEFAULT '3',
  VAL_POR_IVA Decimal(12,3) DEFAULT 13,
  EST_IVA Char(1) DEFAULT '1',
  FDE_IVA Timestamp DEFAULT current_TIMESTAMP  ,
  FFI_IVA Timestamp DEFAULT current_TIMESTAMP  ,
  PRIMARY KEY (COD_IVA)
);
CREATE TABLE IXML
(
  COD_IXML Char(255) DEFAULT '',
  COD_PAFA Char(10),
  COD_CLIE Char(9) DEFAULT '',
  BSE_IXML Char(1) DEFAULT 'B',
  ATS_IXML Char(1) DEFAULT 'S',
  INV_IXML Char(1),
  NOM_IXML Char(245) DEFAULT '',
  COD_CUEN Char(25) DEFAULT '',
  UNI_ITEM Integer DEFAULT 1,
  COD_IVA Char(2) DEFAULT '00',
  COD_PIV_IXML Char(2) DEFAULT '00',
  PIV_IXML Decimal(12,3) DEFAULT 0,
  OBJ_IXML Char(1) DEFAULT '0',
  CONSTRAINT PK_IXML_1 PRIMARY KEY (COD_IXML,COD_CLIE)
);
CREATE TABLE KEY_LOG_202409
(
  NUM_KEY Integer,
  FEC_KEY Timestamp,
  ISL_KEY Char(2) DEFAULT '',
  EST_KEY Char(1) DEFAULT '0',
  COD_CLIE Char(9) DEFAULT '',
  COD_DESP Char(9) DEFAULT '',
  COD_PUNT Char(2) DEFAULT '',
  COD_PAGO Char(1) DEFAULT '',
  CODI_PLA Char(20) DEFAULT '',
  NUM_LOTE Char(10) DEFAULT '',
  TIP_TARJ Char(1) DEFAULT '',
  CUP_CLIE Double precision DEFAULT 0,
  SAL_PROM Char(10) DEFAULT '',
  KIL_KILO Char(20) DEFAULT '',
  TXT_RFID Char(300) DEFAULT '',
  NUM_DESP Integer DEFAULT 0,
  SEC_DCTO Integer DEFAULT 0,
  PRIMARY KEY (NUM_KEY)
);
CREATE TABLE KEY_LOG_202410
(
  NUM_KEY Integer,
  FEC_KEY Timestamp,
  ISL_KEY Char(2) DEFAULT '',
  EST_KEY Char(1) DEFAULT '0',
  COD_CLIE Char(9) DEFAULT '',
  COD_DESP Char(9) DEFAULT '',
  COD_PUNT Char(2) DEFAULT '',
  COD_PAGO Char(1) DEFAULT '',
  CODI_PLA Char(20) DEFAULT '',
  NUM_LOTE Char(10) DEFAULT '',
  TIP_TARJ Char(1) DEFAULT '',
  CUP_CLIE Double precision DEFAULT 0,
  SAL_PROM Char(10) DEFAULT '',
  KIL_KILO Char(20) DEFAULT '',
  TXT_RFID Char(300) DEFAULT '',
  NUM_DESP Integer DEFAULT 0,
  SEC_DCTO Integer DEFAULT 0,
  NUM_TICK_MOV Integer DEFAULT 0,
  PRIMARY KEY (NUM_KEY)
);
CREATE TABLE KEY_LOG_202411
(
  NUM_KEY Integer,
  FEC_KEY Timestamp,
  ISL_KEY Char(2) DEFAULT '',
  EST_KEY Char(1) DEFAULT '0',
  COD_CLIE Char(9) DEFAULT '',
  COD_DESP Char(9) DEFAULT '',
  COD_PUNT Char(2) DEFAULT '',
  COD_PAGO Char(1) DEFAULT '',
  CODI_PLA Char(20) DEFAULT '',
  NUM_LOTE Char(10) DEFAULT '',
  TIP_TARJ Char(1) DEFAULT '',
  CUP_CLIE Double precision DEFAULT 0,
  SAL_PROM Char(10) DEFAULT '',
  KIL_KILO Char(20) DEFAULT '',
  TXT_RFID Char(300) DEFAULT '',
  NUM_DESP Integer DEFAULT 0,
  SEC_DCTO Integer DEFAULT 0,
  NUM_TICK_MOV Integer DEFAULT 0,
  PRIMARY KEY (NUM_KEY)
);
CREATE TABLE KEY_LOG_202412
(
  NUM_KEY Integer,
  FEC_KEY Timestamp,
  ISL_KEY Char(2) DEFAULT '',
  EST_KEY Char(1) DEFAULT '0',
  COD_CLIE Char(9) DEFAULT '',
  COD_DESP Char(9) DEFAULT '',
  COD_PUNT Char(2) DEFAULT '',
  COD_PAGO Char(1) DEFAULT '',
  CODI_PLA Char(20) DEFAULT '',
  NUM_LOTE Char(10) DEFAULT '',
  TIP_TARJ Char(1) DEFAULT '',
  CUP_CLIE Double precision DEFAULT 0,
  SAL_PROM Char(10) DEFAULT '',
  KIL_KILO Char(20) DEFAULT '',
  TXT_RFID Char(300) DEFAULT '',
  NUM_DESP Integer DEFAULT 0,
  SEC_DCTO Integer DEFAULT 0,
  NUM_TICK_MOV Integer DEFAULT 0,
  PRIMARY KEY (NUM_KEY)
);
CREATE TABLE KEY_LOG_202501
(
  NUM_KEY Integer,
  FEC_KEY Timestamp,
  ISL_KEY Char(2) DEFAULT '',
  EST_KEY Char(1) DEFAULT '0',
  COD_CLIE Char(9) DEFAULT '',
  COD_DESP Char(9) DEFAULT '',
  COD_PUNT Char(2) DEFAULT '',
  COD_PAGO Char(1) DEFAULT '',
  CODI_PLA Char(20) DEFAULT '',
  NUM_LOTE Char(10) DEFAULT '',
  TIP_TARJ Char(1) DEFAULT '',
  CUP_CLIE Double precision DEFAULT 0,
  SAL_PROM Char(10) DEFAULT '',
  KIL_KILO Char(20) DEFAULT '',
  TXT_RFID Char(300) DEFAULT '',
  NUM_DESP Integer DEFAULT 0,
  SEC_DCTO Integer DEFAULT 0,
  NUM_TICK_MOV Integer DEFAULT 0,
  PRIMARY KEY (NUM_KEY)
);
CREATE TABLE KEY_LOG_202502
(
  NUM_KEY Integer,
  FEC_KEY Timestamp,
  ISL_KEY Char(2) DEFAULT '',
  EST_KEY Char(1) DEFAULT '0',
  COD_CLIE Char(9) DEFAULT '',
  COD_DESP Char(9) DEFAULT '',
  COD_PUNT Char(2) DEFAULT '',
  COD_PAGO Char(1) DEFAULT '',
  CODI_PLA Char(20) DEFAULT '',
  NUM_LOTE Char(10) DEFAULT '',
  TIP_TARJ Char(1) DEFAULT '',
  CUP_CLIE Double precision DEFAULT 0,
  SAL_PROM Char(10) DEFAULT '',
  KIL_KILO Char(20) DEFAULT '',
  TXT_RFID Char(300) DEFAULT '',
  NUM_DESP Integer DEFAULT 0,
  SEC_DCTO Integer DEFAULT 0,
  NUM_TICK_MOV Integer DEFAULT 0,
  PRIMARY KEY (NUM_KEY)
);
CREATE TABLE KEY_LOG_202503
(
  NUM_KEY Integer,
  FEC_KEY Timestamp,
  ISL_KEY Char(2) DEFAULT '',
  EST_KEY Char(1) DEFAULT '0',
  COD_CLIE Char(9) DEFAULT '',
  COD_DESP Char(9) DEFAULT '',
  COD_PUNT Char(2) DEFAULT '',
  COD_PAGO Char(1) DEFAULT '',
  CODI_PLA Char(20) DEFAULT '',
  NUM_LOTE Char(10) DEFAULT '',
  TIP_TARJ Char(1) DEFAULT '',
  CUP_CLIE Double precision DEFAULT 0,
  SAL_PROM Char(10) DEFAULT '',
  KIL_KILO Char(20) DEFAULT '',
  TXT_RFID Char(300) DEFAULT '',
  NUM_DESP Integer DEFAULT 0,
  SEC_DCTO Integer DEFAULT 0,
  NUM_TICK_MOV Integer DEFAULT 0,
  PRIMARY KEY (NUM_KEY)
);
CREATE TABLE KEY_LOG_202504
(
  NUM_KEY Integer,
  FEC_KEY Timestamp,
  ISL_KEY Char(2) DEFAULT '',
  EST_KEY Char(1) DEFAULT '0',
  COD_CLIE Char(9) DEFAULT '',
  COD_DESP Char(9) DEFAULT '',
  COD_PUNT Char(2) DEFAULT '',
  COD_PAGO Char(1) DEFAULT '',
  CODI_PLA Char(20) DEFAULT '',
  NUM_LOTE Char(10) DEFAULT '',
  TIP_TARJ Char(1) DEFAULT '',
  CUP_CLIE Double precision DEFAULT 0,
  SAL_PROM Char(10) DEFAULT '',
  KIL_KILO Char(20) DEFAULT '',
  TXT_RFID Char(300) DEFAULT '',
  NUM_DESP Integer DEFAULT 0,
  SEC_DCTO Integer DEFAULT 0,
  NUM_TICK_MOV Integer DEFAULT 0,
  PRIMARY KEY (NUM_KEY)
);
CREATE TABLE KEY_LOG_202505
(
  NUM_KEY Integer,
  FEC_KEY Timestamp,
  ISL_KEY Char(2) DEFAULT '',
  EST_KEY Char(1) DEFAULT '0',
  COD_CLIE Char(9) DEFAULT '',
  COD_DESP Char(9) DEFAULT '',
  COD_PUNT Char(2) DEFAULT '',
  COD_PAGO Char(1) DEFAULT '',
  CODI_PLA Char(20) DEFAULT '',
  NUM_LOTE Char(10) DEFAULT '',
  TIP_TARJ Char(1) DEFAULT '',
  CUP_CLIE Double precision DEFAULT 0,
  SAL_PROM Char(10) DEFAULT '',
  KIL_KILO Char(20) DEFAULT '',
  TXT_RFID Char(300) DEFAULT '',
  NUM_DESP Integer DEFAULT 0,
  SEC_DCTO Integer DEFAULT 0,
  NUM_TICK_MOV Integer DEFAULT 0,
  PRIMARY KEY (NUM_KEY)
);
CREATE TABLE KEY_LOG_202506
(
  NUM_KEY Integer,
  FEC_KEY Timestamp,
  ISL_KEY Char(2) DEFAULT '',
  EST_KEY Char(1) DEFAULT '0',
  COD_CLIE Char(9) DEFAULT '',
  COD_DESP Char(9) DEFAULT '',
  COD_PUNT Char(2) DEFAULT '',
  COD_PAGO Char(1) DEFAULT '',
  CODI_PLA Char(20) DEFAULT '',
  NUM_LOTE Char(10) DEFAULT '',
  TIP_TARJ Char(1) DEFAULT '',
  CUP_CLIE Double precision DEFAULT 0,
  SAL_PROM Char(10) DEFAULT '',
  KIL_KILO Char(20) DEFAULT '',
  TXT_RFID Char(300) DEFAULT '',
  NUM_DESP Integer DEFAULT 0,
  SEC_DCTO Integer DEFAULT 0,
  NUM_TICK_MOV Integer DEFAULT 0,
  PRIMARY KEY (NUM_KEY)
);
CREATE TABLE KEY_LOG_202507
(
  NUM_KEY Integer,
  FEC_KEY Timestamp,
  ISL_KEY Char(2) DEFAULT '',
  EST_KEY Char(1) DEFAULT '0',
  COD_CLIE Char(9) DEFAULT '',
  COD_DESP Char(9) DEFAULT '',
  COD_PUNT Char(2) DEFAULT '',
  COD_PAGO Char(1) DEFAULT '',
  CODI_PLA Char(20) DEFAULT '',
  NUM_LOTE Char(10) DEFAULT '',
  TIP_TARJ Char(1) DEFAULT '',
  CUP_CLIE Double precision DEFAULT 0,
  SAL_PROM Char(10) DEFAULT '',
  KIL_KILO Char(20) DEFAULT '',
  TXT_RFID Char(300) DEFAULT '',
  NUM_DESP Integer DEFAULT 0,
  SEC_DCTO Integer DEFAULT 0,
  NUM_TICK_MOV Integer DEFAULT 0,
  PRIMARY KEY (NUM_KEY)
);
CREATE TABLE KEY_LOG_202508
(
  NUM_KEY Integer,
  FEC_KEY Timestamp,
  ISL_KEY Char(2) DEFAULT '',
  EST_KEY Char(1) DEFAULT '0',
  COD_CLIE Char(9) DEFAULT '',
  COD_DESP Char(9) DEFAULT '',
  COD_PUNT Char(2) DEFAULT '',
  COD_PAGO Char(1) DEFAULT '',
  CODI_PLA Char(20) DEFAULT '',
  NUM_LOTE Char(10) DEFAULT '',
  TIP_TARJ Char(1) DEFAULT '',
  CUP_CLIE Double precision DEFAULT 0,
  SAL_PROM Char(10) DEFAULT '',
  KIL_KILO Char(20) DEFAULT '',
  TXT_RFID Char(300) DEFAULT '',
  NUM_DESP Integer DEFAULT 0,
  SEC_DCTO Integer DEFAULT 0,
  NUM_TICK_MOV Integer DEFAULT 0,
  PRIMARY KEY (NUM_KEY)
);
CREATE TABLE KEY_LOG_202509
(
  NUM_KEY Integer,
  FEC_KEY Timestamp,
  ISL_KEY Char(2) DEFAULT '',
  EST_KEY Char(1) DEFAULT '0',
  COD_CLIE Char(9) DEFAULT '',
  COD_DESP Char(9) DEFAULT '',
  COD_PUNT Char(2) DEFAULT '',
  COD_PAGO Char(1) DEFAULT '',
  CODI_PLA Char(20) DEFAULT '',
  NUM_LOTE Char(10) DEFAULT '',
  TIP_TARJ Char(1) DEFAULT '',
  CUP_CLIE Double precision DEFAULT 0,
  SAL_PROM Char(10) DEFAULT '',
  KIL_KILO Char(20) DEFAULT '',
  TXT_RFID Char(300) DEFAULT '',
  NUM_DESP Integer DEFAULT 0,
  SEC_DCTO Integer DEFAULT 0,
  NUM_TICK_MOV Integer DEFAULT 0,
  PRIMARY KEY (NUM_KEY)
);
CREATE TABLE KEY_LOG_202510
(
  NUM_KEY Integer,
  FEC_KEY Timestamp,
  ISL_KEY Char(2) DEFAULT '',
  EST_KEY Char(1) DEFAULT '0',
  COD_CLIE Char(9) DEFAULT '',
  COD_DESP Char(9) DEFAULT '',
  COD_PUNT Char(2) DEFAULT '',
  COD_PAGO Char(1) DEFAULT '',
  CODI_PLA Char(20) DEFAULT '',
  NUM_LOTE Char(10) DEFAULT '',
  TIP_TARJ Char(1) DEFAULT '',
  CUP_CLIE Double precision DEFAULT 0,
  SAL_PROM Char(10) DEFAULT '',
  KIL_KILO Char(20) DEFAULT '',
  TXT_RFID Char(300) DEFAULT '',
  NUM_DESP Integer DEFAULT 0,
  SEC_DCTO Integer DEFAULT 0,
  NUM_TICK_MOV Integer DEFAULT 0,
  PRIMARY KEY (NUM_KEY)
);
CREATE TABLE KEY_LOG_202511
(
  NUM_KEY Integer,
  FEC_KEY Timestamp,
  ISL_KEY Char(2) DEFAULT '',
  EST_KEY Char(1) DEFAULT '0',
  COD_CLIE Char(9) DEFAULT '',
  COD_DESP Char(9) DEFAULT '',
  COD_PUNT Char(2) DEFAULT '',
  COD_PAGO Char(1) DEFAULT '',
  CODI_PLA Char(20) DEFAULT '',
  NUM_LOTE Char(10) DEFAULT '',
  TIP_TARJ Char(1) DEFAULT '',
  CUP_CLIE Double precision DEFAULT 0,
  SAL_PROM Char(10) DEFAULT '',
  KIL_KILO Char(20) DEFAULT '',
  TXT_RFID Char(300) DEFAULT '',
  NUM_DESP Integer DEFAULT 0,
  SEC_DCTO Integer DEFAULT 0,
  NUM_TICK_MOV Integer DEFAULT 0,
  PRIMARY KEY (NUM_KEY)
);
CREATE TABLE KEY_LOG_202512
(
  NUM_KEY Integer,
  FEC_KEY Timestamp,
  ISL_KEY Char(2) DEFAULT '',
  EST_KEY Char(1) DEFAULT '0',
  COD_CLIE Char(9) DEFAULT '',
  COD_DESP Char(9) DEFAULT '',
  COD_PUNT Char(2) DEFAULT '',
  COD_PAGO Char(1) DEFAULT '',
  CODI_PLA Char(20) DEFAULT '',
  NUM_LOTE Char(10) DEFAULT '',
  TIP_TARJ Char(1) DEFAULT '',
  CUP_CLIE Double precision DEFAULT 0,
  SAL_PROM Char(10) DEFAULT '',
  KIL_KILO Char(20) DEFAULT '',
  TXT_RFID Char(300) DEFAULT '',
  NUM_DESP Integer DEFAULT 0,
  SEC_DCTO Integer DEFAULT 0,
  NUM_TICK_MOV Integer DEFAULT 0,
  PRIMARY KEY (NUM_KEY)
);
CREATE TABLE KEY_LOG_202601
(
  NUM_KEY Integer,
  FEC_KEY Timestamp,
  ISL_KEY Char(2) DEFAULT '',
  EST_KEY Char(1) DEFAULT '0',
  COD_CLIE Char(9) DEFAULT '',
  COD_DESP Char(9) DEFAULT '',
  COD_PUNT Char(2) DEFAULT '',
  COD_PAGO Char(1) DEFAULT '',
  CODI_PLA Char(20) DEFAULT '',
  NUM_LOTE Char(10) DEFAULT '',
  TIP_TARJ Char(1) DEFAULT '',
  CUP_CLIE Double precision DEFAULT 0,
  SAL_PROM Char(10) DEFAULT '',
  KIL_KILO Char(20) DEFAULT '',
  TXT_RFID Char(300) DEFAULT '',
  NUM_DESP Integer DEFAULT 0,
  SEC_DCTO Integer DEFAULT 0,
  NUM_TICK_MOV Integer DEFAULT 0,
  PRIMARY KEY (NUM_KEY)
);
CREATE TABLE KEY_LOG_202602
(
  NUM_KEY Integer,
  FEC_KEY Timestamp,
  ISL_KEY Char(2) DEFAULT '',
  EST_KEY Char(1) DEFAULT '0',
  COD_CLIE Char(9) DEFAULT '',
  COD_DESP Char(9) DEFAULT '',
  COD_PUNT Char(2) DEFAULT '',
  COD_PAGO Char(1) DEFAULT '',
  CODI_PLA Char(20) DEFAULT '',
  NUM_LOTE Char(10) DEFAULT '',
  TIP_TARJ Char(1) DEFAULT '',
  CUP_CLIE Double precision DEFAULT 0,
  SAL_PROM Char(10) DEFAULT '',
  KIL_KILO Char(20) DEFAULT '',
  TXT_RFID Char(300) DEFAULT '',
  NUM_DESP Integer DEFAULT 0,
  SEC_DCTO Integer DEFAULT 0,
  NUM_TICK_MOV Integer DEFAULT 0,
  PRIMARY KEY (NUM_KEY)
);
CREATE TABLE KILO
(
  SEC_KILO Integer,
  SEC_DCTO Double precision DEFAULT 0,
  FEC_KILO Timestamp DEFAULT CURRENT_TIMESTAMP,
  PLA_KILO Char(10) DEFAULT '',
  KIL_KILO Char(20) DEFAULT '0',
  COD_CLIE Char(9) DEFAULT ''
);
CREATE TABLE LICA
(
  NUM_LICA Integer,
  DET_LICA Char(240),
  FEC_LICA Timestamp,
  FDE_LICA Timestamp,
  FHA_LICA Timestamp,
  TDE_LICA Double precision,
  TCR_LICA Double precision,
  CAJ_LICA Double precision,
  CONSTRAINT PK_LICA PRIMARY KEY (NUM_LICA)
);
CREATE TABLE LIMO
(
  NUM_LIMO Integer,
  NUM_LICA Integer,
  COD_LIMO Char(2),
  DET_LIMO Char(240),
  UNI_LIMO Double precision,
  LIN_LIMO Double precision,
  LFI_LIMO Double precision,
  CAN_LIMO Double precision,
  DEB_LIMO Double precision,
  CRE_LIMO Double precision,
  TIP_LIMO Char(1),
  CONSTRAINT PK_LIMO PRIMARY KEY (NUM_LIMO)
);
CREATE TABLE LIQU
(
  NUM_LIQU Double precision,
  COD_CLIE Char(9),
  DET_LIQU Char(200),
  FEC_LIQU Timestamp,
  TDE_LIQU Double precision,
  TCR_LIQU Double precision,
  EDE_LIQU Double precision,
  ECR_LIQU Double precision,
  GAS_LIQU Double precision,
  SAN_LIQU Double precision,
  SFI_LIQU Double precision,
  COD_ITEM Char(100),
  COD_PUNT Char(2),
  NUM_TURN Integer DEFAULT 0,
  OIN_LIQU Double precision DEFAULT 0,
  OEG_LIQU Double precision DEFAULT 0,
  FAL_LIQU Double precision DEFAULT 0,
  SOB_LIQU Double precision DEFAULT 0,
  DIN_LIQU Char(250) DEFAULT '',
  DEG_LIQU Char(250) DEFAULT '',
  DSO_LIQU Char(250) DEFAULT '',
  DFA_LIQU Char(250) DEFAULT '',
  TAR_LIQU Double precision DEFAULT 0,
  NUMMCOMP Integer DEFAULT 0,
  DIF_GAL_MEC_LIQU Double precision DEFAULT 0,
  DIF_DOL_MEC_LIQU Double precision DEFAULT 0,
  CON_LIQU Double precision DEFAULT 0,
  CRE_LIQU Double precision DEFAULT 0,
  DIF_LIQU Double precision DEFAULT 0,
  CONSTRAINT PK_LIQU PRIMARY KEY (NUM_LIQU)
);
CREATE TABLE LIQU_CRED
(
  NUM_LICR Integer,
  NUM_LIQU Integer,
  TIP_DCTO Char(2) DEFAULT '',
  SEC_DCTO Integer,
  DET_DCTO Char(50),
  TOT_LICR Numeric(12,2) DEFAULT 0,
  RUC_CLIE Char(13) DEFAULT '',
  COD_CLIE Char(9) DEFAULT '',
  NOM_CLIE Char(200) DEFAULT '',
  PRIMARY KEY (NUM_LICR)
);
CREATE TABLE LIQU_DENO
(
  NUM_LIDN Integer,
  NUM_LIQU Integer,
  TIP_LIDN Char(1) DEFAULT 'B',
  DEN_LIDN Numeric(10,2),
  DET_LIDN Char(50),
  CAN_LIDN Integer DEFAULT 0,
  TOT_LIDN Numeric(10,2) DEFAULT 0,
  PRIMARY KEY (NUM_LIDN)
);
CREATE TABLE LIQU_DEPO
(
  NUM_LIDP Integer,
  COD_CLIE Char(9) DEFAULT '',
  NUM_LIQU Integer DEFAULT 0,
  FEC_LIDP Timestamp DEFAULT current_timestamp ,
  TIP_LIDP Char(4) DEFAULT 'DEPO',
  DET_LIDP Char(50),
  CAN_LIDP Integer DEFAULT 0,
  VAL_LIDP Numeric(10,2) DEFAULT 0,
  TOT_LIDP Numeric(10,2) DEFAULT 0,
  COD_BANC Char(2) DEFAULT '',
  NUM_COMP Char(20) DEFAULT '',
  PRIMARY KEY (NUM_LIDP)
);
CREATE TABLE LIQU_DEPO_DENO
(
  NUM_LIDN Integer,
  NUM_LIDP Integer,
  TIP_LIDN Char(1) DEFAULT 'B',
  DEN_LIDN Numeric(10,2),
  DET_LIDN Char(50),
  CAN_LIDN Integer DEFAULT 0,
  TOT_LIDN Numeric(10,2) DEFAULT 0,
  PRIMARY KEY (NUM_LIDN)
);
CREATE TABLE LIQU_GAST
(
  NUM_LQGS Integer,
  NUM_LIQU Integer DEFAULT 0,
  NOM_LQGS Char(150) DEFAULT '',
  DET_LQGS Char(240) DEFAULT '',
  VAL_LQGS Numeric(12,2) DEFAULT 0,
  RET_LQGS Numeric(12,2) DEFAULT 0,
  NET_LQGS Numeric(12,2) DEFAULT 0,
  FEC_LQGS Timestamp,
  ANU_LQGS Char(1) DEFAULT '0',
  COD_VEND Char(9) DEFAULT '',
  PRIMARY KEY (NUM_LQGS)
);
CREATE TABLE LIQU_ITEM
(
  NUM_LQIT Integer,
  NUM_LIQU Integer DEFAULT 0,
  COD_ITEM Char(25) DEFAULT '',
  NOM_ITEM Char(100) DEFAULT '',
  SAN_LQIT Numeric(12,3) DEFAULT 0,
  ING_LQIT Numeric(12,3) DEFAULT 0,
  EGR_LQIT Numeric(12,3) DEFAULT 0,
  SAC_LQIT Numeric(12,3) DEFAULT 0,
  PRIMARY KEY (NUM_LQIT)
);
CREATE TABLE LIQU_TANK
(
  NUM_LQTK Integer,
  NUM_LIQU Integer DEFAULT 0,
  NUM_TQMV Integer DEFAULT 0,
  COD_TANQ Char(2) DEFAULT '',
  SCM_LQTK Numeric(13,3) DEFAULT 0,
  SGL_LQTK Numeric(13,3) DEFAULT 0,
  FEC_LQTK Timestamp DEFAULT current_timestamp  ,
  PRIMARY KEY (NUM_LQTK)
);
CREATE TABLE LIQU_TARJ
(
  NUM_LIQU_TARJ Integer,
  NUM_LIQU Integer DEFAULT 0,
  COD_BANC Char(2) DEFAULT '',
  CAN_LIQU_TARJ Integer DEFAULT 0,
  VAL_LIQU_TARJ Decimal(12,2) DEFAULT 0,
  PRIMARY KEY (NUM_LIQU_TARJ)
);
CREATE TABLE LIQU_TARJ_DET
(
  NUM_LICR Integer,
  NUM_LIQU Integer,
  COD_PAGO Char(3) DEFAULT '',
  SEC_DCTO Integer,
  DET_DCTO Char(50),
  TOT_LICR Numeric(12,2) DEFAULT 0,
  RUC_CLIE Char(13) DEFAULT '',
  COD_CLIE Char(9) DEFAULT '',
  NOM_CLIE Char(200) DEFAULT '',
  COD_GRUP Char(4) DEFAULT '',
  NUM_LOTE Char(10) DEFAULT 0,
  NUM_VAUC Integer DEFAULT 0,
  PRIMARY KEY (NUM_LICR)
);
CREATE TABLE LOG
(
  NUM_LOG Numeric(18,0),
  USU_LOG Char(50),
  TAB_LOG Char(20),
  DIR_LOG Char(20),
  FEC_LOG Timestamp,
  ACC_LOG Char(1),
  CONSTRAINT PK_LOG PRIMARY KEY (NUM_LOG)
);
CREATE TABLE MANG
(
  COD_MANG Char(10),
  NOM_MANG Char(50),
  SER_MANG Char(20),
  COD_TANQ Char(10),
  SIN_MANG Double precision,
  EGR_MANG Double precision,
  COD_PUNT Char(10),
  NUM_MANG Integer,
  ELI_MANG Char(1) DEFAULT '0',
  PRIMARY KEY (COD_MANG)
);
CREATE TABLE MOCB
(
  NUM_MOCB Integer,
  NUM_COBA Integer,
  NUM_MOVI Integer,
  EST_MOCB Char(1) DEFAULT '0',
  MOD_MOCB Char(2) DEFAULT 'MO',
  CONSTRAINT PK_MOCB PRIMARY KEY (NUM_MOCB)
);
CREATE TABLE MOCB_PEND
(
  NUM_MOCB Integer,
  NUM_COBA Integer,
  NUM_MOVI Integer,
  EST_MOCB Char(1) DEFAULT '0',
  MOD_MOCB Char(2) DEFAULT 'MO',
  PRIMARY KEY (NUM_MOCB)
);
CREATE TABLE MOCB_REC
(
  NUM_MOCB_REC Integer,
  NUM_MOCB Integer,
  NUM_COBA Integer,
  NUM_MOVI Integer,
  EST_MOCB Char(1) DEFAULT '0',
  NUMMCOMP Integer,
  COD_BANC Char(2) DEFAULT '00',
  VAL_MOVI Double precision DEFAULT 0,
  EST_MOCB_REC Char(1) DEFAULT '0',
  MOD_MOCB Char(2) DEFAULT 'MO',
  CONSTRAINT PK_MOCB_REC PRIMARY KEY (NUM_MOCB_REC)
);
CREATE TABLE MODU
(
  CODMODU Char(15),
  NOMMODU Char(100),
  CODJSMC Char(10),
  MOD_MODU Char(30) DEFAULT '',
  EPH_MODU Char(30) DEFAULT '',
  NOM_MODU Char(30) DEFAULT '',
  PRIMARY KEY (CODMODU)
);
CREATE TABLE MOLI
(
  NUM_MOLI Double precision,
  COD_PUNT Char(2),
  VO1_PUNT Char(21),
  VO2_PUNT Char(21),
  VO3_PUNT Char(21),
  VO4_PUNT Char(21),
  IM1_PUNT Char(21),
  IM2_PUNT Char(21),
  IM3_PUNT Char(21),
  IM4_PUNT Char(21),
  VI1_PUNT Char(21),
  VI2_PUNT Char(21),
  VI3_PUNT Char(21),
  VI4_PUNT Char(21),
  II1_PUNT Char(21),
  II2_PUNT Char(21),
  II3_PUNT Char(21),
  II4_PUNT Char(21),
  NUM_LIQU Double precision,
  FEC_MOLI Timestamp,
  VEN_PUNT Char(9),
  FCI_PUNT Timestamp,
  DET_MOLI Char(150) DEFAULT '',
  LOT_PUNT Char(10) DEFAULT '',
  COD_PAGO Char(3) DEFAULT '',
  IVA_MOLI Double precision DEFAULT 0,
  COD_MANG Char(2) DEFAULT '',
  FAB_PUNT Timestamp DEFAULT current_TIMESTAMP ,
  CO1_ITEM Char(25) DEFAULT '',
  CO2_ITEM Char(25) DEFAULT '',
  CO3_ITEM Char(25) DEFAULT '',
  CO4_ITEM Char(25) DEFAULT '',
  NO1_ITEM Char(100) DEFAULT '',
  NO2_ITEM Char(100) DEFAULT '',
  NO3_ITEM Char(100) DEFAULT '',
  NO4_ITEM Char(100) DEFAULT '',
  VE1_ITEM Double precision DEFAULT 0,
  VE2_ITEM Double precision DEFAULT 0,
  VE3_ITEM Double precision DEFAULT 0,
  VE4_ITEM Double precision DEFAULT 0,
  TOT_FAC_MOLI Double precision DEFAULT 0,
  DIF_FAC_MOLI Double precision DEFAULT 0,
  VO1_MEC_PUNT Char(20) DEFAULT '0',
  VO2_MEC_PUNT Char(20) DEFAULT '0',
  VO3_MEC_PUNT Char(20) DEFAULT '0',
  VO4_MEC_PUNT Char(20) DEFAULT '0',
  VI1_MEC_PUNT Char(20) DEFAULT '0',
  VI2_MEC_PUNT Char(20) DEFAULT '0',
  VI3_MEC_PUNT Char(20) DEFAULT '0',
  VI4_MEC_PUNT Char(20) DEFAULT '0',
  CONSTRAINT PK_MOLI PRIMARY KEY (NUM_MOLI)
);
CREATE TABLE MOLI_TANQ
(
  NUM_MOLI_TANQ Integer,
  COD_TANQ Char(10),
  SCO_INI_TANQ Double precision DEFAULT 0,
  SCO_INI_GAL_TANQ Double precision DEFAULT 0,
  SAG_INI_TANQ Double precision DEFAULT 0,
  SAG_INI_GAL_TANQ Double precision DEFAULT 0,
  COD_VEND Char(9) DEFAULT '000',
  FEC_INI_TANQ Timestamp DEFAULT current_timestamp   ,
  FEC_FIN_TANQ Timestamp DEFAULT current_timestamp   ,
  TIP_MOLI_TANQ Char(2) DEFAULT 'TQ',
  TOT_VEN_TANQ Double precision DEFAULT 0,
  SCO_FIN_TANQ Double precision DEFAULT 0,
  SCO_FIN_GAL_TANQ Double precision DEFAULT 0,
  SAG_FIN_TANQ Double precision DEFAULT 0,
  SAG_FIN_GAL_TANQ Double precision DEFAULT 0,
  CIE_MOLI_TANQ Integer DEFAULT 0,
  SEC_DCTO Double precision DEFAULT 0,
  TOT_GAL_TANQ Double precision DEFAULT 0,
  PLA_MOLI_TANQ Char(20) DEFAULT '',
  REN_MOLI_TANQ Double precision DEFAULT 0,
  TIP_CIE_TANQ Char(1) DEFAULT 'C',
  CONSTRAINT PK_LIMO_TANQ PRIMARY KEY (NUM_MOLI_TANQ)
);
CREATE TABLE MOPF
(
  NUM_TLMM Double precision,
  COD_PAFA Char(10),
  VAL_MOPF Double precision,
  IVA_MOPF Char(1),
  ATS_MOPF Char(1),
  COD_CUEN Char(25),
  NUM_MOPF Double precision,
  BSE_MOPF Char(1),
  NUMMCOMP Double precision DEFAULT 0,
  COD_IVA Char(2) DEFAULT '00',
  COD_PIV_MOPF Char(2) DEFAULT '00',
  PIV_MOPF Double precision DEFAULT 0,
  VIV_MOPF Double precision DEFAULT 0,
  TOT_MOPF Double precision DEFAULT 0,
  NUM_MOVI Integer DEFAULT 0,
  OBJ_IVA_MOPF Char(1) DEFAULT '0',
  PRIMARY KEY (NUM_MOPF)
);
CREATE TABLE MOPF_MOVI
(
  NUM_MOPF_MOVI Integer,
  NUM_MOPF Integer DEFAULT 0,
  COD_MOPF_MOVI Char(100) DEFAULT '',
  NOM_MOPF_MOVI Char(200) DEFAULT '',
  CAN_MOPF_MOVI Decimal(12,6) DEFAULT 0,
  UNI_MOPF_MOVI Decimal(12,6) DEFAULT 0,
  TOT_MOPF_MOVI Decimal(12,6) DEFAULT 0,
  COD_CUEN Char(25) DEFAULT '',
  COD_PAFA Char(10) DEFAULT '',
  PRIMARY KEY (NUM_MOPF_MOVI)
);
CREATE TABLE MOVI
(
  COD_ITEM Char(100),
  CAN_MOVI Double precision,
  VAL_MOVI Double precision,
  DSC_MOVI Double precision,
  TOT_MOVI Double precision,
  IVA_MOVI Char(1),
  TVE_MOVI Char(1),
  SEC_DCTO Double precision,
  BEM_MOVI Char(1),
  NUM_MOVI Integer,
  COS_ITEM Double precision DEFAULT 0,
  TSU_MOVI Double precision DEFAULT 0,
  SUB_MOVI Double precision DEFAULT 0,
  PIV_MOVI Double precision DEFAULT 0,
  VIV_MOVI Double precision DEFAULT 0,
  VDS_MOVI Double precision DEFAULT 0,
  COD_IMPU Char(2) DEFAULT '00',
  VEV_MOVI Double precision DEFAULT 0,
  ADD_MOVI Char(150) DEFAULT '',
  SAL_TOT_TICK Double precision DEFAULT 0,
  VAL_TOT_TICK Double precision DEFAULT 0,
  SAL_MOVI Double precision DEFAULT 0,
  SAL_ANT_MOVI Double precision DEFAULT 0,
  CONSTRAINT PK_MOVI PRIMARY KEY (NUM_MOVI)
);
CREATE TABLE PAFA
(
  COD_PAFA Char(10),
  NOM_PAFA Char(50),
  DCT_PAFA Char(2),
  DCR_PAFA Char(1),
  FIN_PAFA Char(1) DEFAULT '0',
  PRIMARY KEY (COD_PAFA)
);
CREATE TABLE PAGO
(
  NOM_PAGO Char(50),
  COD_PAGO Char(3),
  TIP_PAGO Char(2),
  DCR_PAGO Char(1),
  TIP_BANC Char(1),
  COD_CUEN Char(13),
  ETC_PAGO Char(1) DEFAULT 'E',
  ATS_PAGO Char(2) DEFAULT '01',
  CRE_PAGO Char(1) DEFAULT '0',
  RCP_PAGO Char(1) DEFAULT '0',
  RIV_PAGO Char(1) DEFAULT '0',
  RRE_PAGO Char(1) DEFAULT '0',
  COD_BANC Char(2) DEFAULT '00',
  PRIMARY KEY (COD_PAGO)
);
CREATE TABLE PART
(
  COD_PART Char(15),
  COD_SOCI Char(9),
  SUE_PART Double precision,
  OTR_PART Double precision,
  VAC_PART Char(1),
  PRIMARY KEY (COD_PART)
);
CREATE TABLE PEDI
(
  COD_ITEM Char(100),
  BEM_PEDI Char(1),
  CAN_PEDI Double precision,
  VAL_PEDI Double precision,
  DSC_PEDI Double precision,
  TOT_PEDI Double precision,
  IVA_PEDI Char(1),
  TVE_PEDI Char(1),
  SEC_DCTO Double precision
);
CREATE TABLE PEND
(
  NUM_PEND Double precision,
  COD_TIPC Char(2),
  FAP_COMP Timestamp,
  VAL_COMP Double precision,
  DET_COMP Char(240),
  COD_USUA Char(10),
  AUT_COMP Char(60),
  DIG_COMP Char(60),
  NOM_CLIE Char(60),
  COD_CLIE Char(9),
  CHE_COMP Double precision,
  NUM_COMP Char(15),
  COD_PERI Char(3),
  COD_BANC Char(2),
  NUMMCOMP Double precision,
  PRIMARY KEY (NUM_PEND)
);
CREATE TABLE PERI
(
  COD_PERI Char(3),
  NOM_PERI Char(20),
  FIN_PERI Timestamp,
  FFI_PERI Timestamp,
  COD_EMPR Char(3),
  PRIMARY KEY (COD_PERI)
);
CREATE TABLE PGAT
(
  COD_PGAT Char(2),
  NOM_PGAT Char(150),
  BAN_PGAT Char(1) DEFAULT '1',
  POR_PGAT Integer,
  FIN_PGAT Timestamp,
  FFI_PGAT Timestamp,
  COD_PGDV Char(2),
  ALI_PGAT Char(150) DEFAULT '',
  CONSTRAINT PK_PGAT PRIMARY KEY (COD_PGAT)
);
CREATE TABLE PGCA
(
  NUM_PGCA Double precision,
  FPA_PGCA Timestamp,
  FVE_PGCA Timestamp,
  VAL_PGCA Double precision,
  INT_PGCA Double precision,
  OTR_PGCA Double precision,
  COD_PAGO Char(3),
  COD_BANC Char(2),
  CTA_PGCA Char(20),
  CHE_PGCA Char(30),
  NUMMCOMP Double precision,
  COR_BANC Char(3) DEFAULT '00',
  NUM_CONS Integer DEFAULT 0,
  MOD_PGCA Char(1) DEFAULT 'P',
  PRIMARY KEY (NUM_PGCA)
);
CREATE TABLE PGDE
(
  NUM_PGCA Double precision,
  VAL_PGDE Double precision,
  INT_PGDE Double precision,
  OTR_PGDE Double precision,
  NUM_CUOT Double precision,
  NUM_PGDE Double precision,
  DET_PGDE Char(255),
  SAL_PGDE Double precision DEFAULT 0,
  SEC_DCTO Double precision DEFAULT 0,
  PRIMARY KEY (NUM_PGDE)
);
CREATE TABLE PGDV
(
  COD_PGDV Char(2),
  NOM_PGDV Char(150),
  CONSTRAINT PK_PGDV PRIMARY KEY (COD_PGDV)
);
CREATE TABLE PLAC
(
  NUM_PLAC Char(10),
  DET_PLAC Char(100) DEFAULT '',
  CONSTRAINT PK_PLAC PRIMARY KEY (NUM_PLAC)
);
CREATE TABLE PLACA
(
  CODI_PLA Char(15),
  RUC_PLA Char(15) DEFAULT '',
  SEC_PLA Integer DEFAULT 0,
  BAR_PLA Char(25) DEFAULT 0,
  CRE_PLA Char(1) DEFAULT '0',
  DET_PLA Char(100) DEFAULT '',
  CUP_PLA Char(1) DEFAULT '0',
  RFID_PLA Char(1) DEFAULT '0',
  DEF_CUP_PLA Double precision DEFAULT 0,
  VAL_CUP_PLA Double precision DEFAULT 0,
  FEC_CUP_PLA Timestamp DEFAULT current_timestamp ,
  CON_CUP_PLA Double precision DEFAULT 0,
  NUM_PLCU Integer DEFAULT 0,
  CUP_CLIE Char(1) DEFAULT '0',
  DEP_PLA Char(100) DEFAULT '',
  MAR_PLA Char(20) DEFAULT '',
  CLA_PLA Char(30) DEFAULT '',
  DIS_PLA Char(30) DEFAULT '',
  CONSTRAINT PK_PLACA_1 PRIMARY KEY (CODI_PLA)
);
CREATE TABLE PLACA_BLOQ
(
  CODI_PLA Char(15),
  NOM_BLOQ Char(200) DEFAULT '',
  DET_BLOQ Char(200) DEFAULT '',
  PRIMARY KEY (CODI_PLA)
);
CREATE TABLE PLACA_CUPO
(
  NUM_PLCU Integer,
  CODI_PLA Char(15),
  EST_PLCU Char(1) DEFAULT '0',
  FIN_PLCU Timestamp DEFAULT CURRENT_TIMESTAMP  ,
  FFI_PLCU Timestamp DEFAULT CURRENT_TIMESTAMP  ,
  VAL_PLCU Numeric(12,2) DEFAULT 0,
  COD_USUA Char(13) DEFAULT '',
  PRIMARY KEY (NUM_PLCU)
);
CREATE TABLE PREC
(
  NUM_PREC Integer,
  NOM_PREC Char(100) DEFAULT '',
  PRIMARY KEY (NUM_PREC)
);
CREATE TABLE PRES
(
  COD_CUEN Char(25),
  COD_PERI Char(3),
  VAL_PRES Double precision,
  MOD_PRES Double precision,
  FUM_PRES Timestamp
);
CREATE TABLE PRODUCTIVIDAD_DESP
(
  PD_NDESPACHADOR Char(40),
  PD_GALONES Numeric(15,2),
  PD_GALONESE Numeric(15,2),
  PD_PORCENTAJEG Numeric(15,2),
  PD_DESPACHOS Integer,
  PD_DESPACHOSE Integer,
  PD_PORCENTAJED Numeric(15,2),
  PD_PORCENTAJEP Numeric(15,2),
  PD_FECHA Char(40)
);
CREATE TABLE PROM
(
  NUM_PROM Integer,
  FDE_PROM Timestamp DEFAULT current_timestamp  ,
  FHA_PROM Timestamp DEFAULT current_timestamp  ,
  LEY_PROM Char(500) DEFAULT '',
  VAL_TICK_PROM Char(50) DEFAULT '0;0;0',
  CAN_TICK_PROM Char(50) DEFAULT '',
  NOM_PROM Char(1) DEFAULT 'S',
  FVC_PROM Char(1) DEFAULT 'S',
  EBC_PROM Char(1) DEFAULT 'N',
  ALE_PROM Char(1) DEFAULT 'N',
  INF_PROM Char(1) DEFAULT 'N',
  INF_LEY_PROM Char(500) DEFAULT '',
  INF_VAL_PROM Char(50) DEFAULT '0;0;0',
  INF_NOM_PROM Char(1) DEFAULT 'S',
  NUM_BOL_PROM Integer DEFAULT 0,
  VAL_BOL_PROM Decimal(10,2) DEFAULT 0,
  PRIMARY KEY (NUM_PROM)
);
CREATE TABLE PROM_SORT
(
  NUM_SORT Integer,
  NUM_PROM Integer,
  NUM_LIQU Integer DEFAULT 0,
  VAL_SORT Decimal(10,2),
  NBO_SORT Integer,
  SEC_DCTO Integer,
  COD_CLIE Char(15),
  TOT_DCTO Decimal(10,2),
  PLA_DCTO Char(15),
  COD_PUNT Char(2) DEFAULT '',
  VEN_PUNT Char(10) DEFAULT '',
  FEC_SORT Timestamp DEFAULT current_timestamp   ,
  PRIMARY KEY (NUM_SORT)
);
CREATE TABLE PROT
(
  COD_PROT Char(10),
  NOM_PROT Char(50),
  OPC_PROT Char(50),
  EST_PROT Char(50),
  COM_PROT Char(10),
  PRIMARY KEY (COD_PROT)
);
CREATE TABLE PROV
(
  COD_PROV Char(2),
  NOM_PROV Char(50),
  ROL_PROV Char(10) DEFAULT '',
  SUP_PROV Char(10) DEFAULT '',
  PRIMARY KEY (COD_PROV)
);
CREATE TABLE PRUEBAS
(
  ID Integer,
  NOMBRE Char(50),
  CONSTRAINT PK_PRUEBAS PRIMARY KEY (ID)
);
CREATE TABLE PUNT
(
  COD_PUNT Char(2),
  NOM_PUNT Char(50),
  PIC_PUNT Integer,
  EST_PUNT Char(1),
  COD_PROT Char(10),
  COD_CLIE Char(9),
  NUM_DESP Double precision,
  LSU_PUNT Char(1),
  LCL_PUNT Char(1),
  VEN_PUNT Char(10),
  FPA_PUNT Char(1),
  CAN_ARTI Integer,
  COD_ARTI Char(21),
  VO1_PUNT Char(21),
  VO2_PUNT Char(21),
  VO3_PUNT Char(21),
  VO4_PUNT Char(21),
  IM1_PUNT Char(21),
  IM2_PUNT Char(21),
  IM3_PUNT Char(21),
  IM4_PUNT Char(21),
  VI1_PUNT Char(21),
  VI2_PUNT Char(21),
  VI3_PUNT Char(21),
  VI4_PUNT Char(21),
  II1_PUNT Char(21),
  II2_PUNT Char(21),
  II3_PUNT Char(21),
  II4_PUNT Char(21),
  VU1_PUNT Char(21),
  VU2_PUNT Char(21),
  VU3_PUNT Char(21),
  VU4_PUNT Char(21),
  IU1_PUNT Char(21),
  IU2_PUNT Char(21),
  IU3_PUNT Char(21),
  IU4_PUNT Char(21),
  FCI_PUNT Timestamp,
  FUC_PUNT Timestamp,
  IRE_PUNT Char(1) DEFAULT '1',
  LOT_PUNT Char(10) DEFAULT '',
  ELE_PUNT Char(1) DEFAULT '1',
  TTA_PUNT Char(1) DEFAULT '',
  CAN_BARRA Integer DEFAULT 0,
  NIP_PUNT Char(20) DEFAULT '',
  BCL_PUNT Char(1) DEFAULT '0',
  CLA_PUNT Char(10) DEFAULT '',
  PLA_PUNT Char(20) DEFAULT '',
  COD_CHOF Char(20) DEFAULT '',
  KIL_PUNT Char(20) DEFAULT '',
  FAB_PUNT Timestamp DEFAULT current_TIMESTAMP ,
  KEY_CARD_PUNT Char(50) DEFAULT '',
  VO1_MEC_PUNT Char(20) DEFAULT '0',
  VO2_MEC_PUNT Char(20) DEFAULT '0',
  VO3_MEC_PUNT Char(20) DEFAULT '0',
  VO4_MEC_PUNT Char(20) DEFAULT '0',
  VI1_MEC_PUNT Char(20) DEFAULT '0',
  VI2_MEC_PUNT Char(20) DEFAULT '0',
  VI3_MEC_PUNT Char(20) DEFAULT '0',
  VI4_MEC_PUNT Char(20) DEFAULT '0',
  CONSTRAINT PK_PUNT PRIMARY KEY (COD_PUNT)
);
CREATE TABLE PUNTOS
(
  NUM_PUNTOS Integer,
  SEC_DCTO Integer,
  NUM_LIQU Integer,
  COD_CLIE Char(9) DEFAULT '',
  ELI_PUNTOS Char(1) DEFAULT '0',
  CAN_PUNTOS Double precision DEFAULT 0,
  TOT_DCTO Double precision DEFAULT 0,
  NUM_RFID Char(20) DEFAULT '',
  PLA_PUNT Char(15) DEFAULT '',
  PRIMARY KEY (NUM_PUNTOS)
);
CREATE TABLE R103
(
  NUM_R103 Integer,
  COD_BANC Char(2),
  CHE_R103 Char(15),
  MES_R103 Timestamp,
  FEC_R103 Timestamp,
  NUMMCOMP Double precision,
  DET_R103 Char(250),
  ORI_R103 Char(1),
  NRE_R103 Char(15),
  PPR_R103 Double precision,
  AIN_R103 Double precision,
  AIM_R103 Double precision,
  AMU_R103 Double precision,
  BIN_R103 Double precision,
  BMU_R103 Double precision,
  NN1_R103 Char(15),
  NN2_R103 Char(15),
  NN3_R103 Char(15),
  VN1_R103 Double precision,
  VN2_R103 Double precision,
  VN3_R103 Double precision,
  VN4_R103 Double precision,
  COD_ACTI Char(2) DEFAULT '01',
  CONSTRAINT PK_R103 PRIMARY KEY (NUM_R103)
);
CREATE TABLE R103_MOV
(
  NUM_M103 Integer,
  NUM_R103 Integer,
  COD_RENT Char(5),
  BAS_M103 Double precision,
  RET_M103 Double precision,
  CONSTRAINT PK_R103_MOV PRIMARY KEY (NUM_M103)
);
CREATE TABLE R104
(
  NUM_R104 Integer,
  COD_BANC Char(2),
  CHE_R104 Char(15),
  MES_R104 Timestamp,
  FEC_R104 Timestamp,
  NUMMCOMP Double precision,
  DET_R104 Char(250),
  ORI_R104 Char(1),
  NRE_R104 Char(15),
  VBA_NC0_R104 Double precision,
  VBA_N12_R104 Double precision,
  VIM_N12_R104 Double precision,
  VBA_REE_R104 Double precision,
  VIM_REE_R104 Double precision,
  VBA_CRE_R104 Double precision,
  CBA_NC0_R104 Double precision,
  CBA_N12_R104 Double precision,
  CIM_N12_R104 Double precision,
  CBA_REE_R104 Double precision,
  CIM_REE_R104 Double precision,
  POR_FAC_R104 Double precision,
  VAL_FAC_R104 Double precision,
  IMP_R104 Double precision,
  CRE_AAM_R104 Double precision,
  CRE_CAM_R104 Double precision,
  CRE_RMA_R104 Double precision,
  R30_R104 Double precision,
  R70_R104 Double precision,
  R10_R104 Double precision,
  AIN_R104 Double precision,
  AIM_R104 Double precision,
  AMU_R104 Double precision,
  BIN_R104 Double precision,
  BMU_R104 Double precision,
  NN1_R104 Char(15),
  NN2_R104 Char(15),
  NN3_R104 Char(15),
  VN1_R104 Double precision,
  VN2_R104 Double precision,
  VN3_R104 Double precision,
  VN4_R104 Double precision,
  NC1_R104 Char(15),
  NC2_R104 Char(15),
  VC1_R104 Double precision,
  VC2_R104 Double precision,
  CTA_R104 Double precision,
  CTR_R104 Double precision,
  COD_ACTI Char(2) DEFAULT '01',
  R100_R104 Double precision DEFAULT 0,
  R20_R104 Double precision DEFAULT 0,
  R50_R104 Double precision DEFAULT 0,
  CONSTRAINT PK_R104 PRIMARY KEY (NUM_R104)
);
CREATE TABLE R104_MOV
(
  NUM_M104 Integer,
  NUM_R104 Integer,
  COD_M104 Char(5),
  BRU_M104 Double precision,
  NET_M104 Double precision,
  IMP_M104 Double precision,
  CONSTRAINT PK_R104_MOV PRIMARY KEY (NUM_M104)
);
CREATE TABLE RENT
(
  NOM_RENT Char(250),
  FAC_RENT Double precision,
  FIN_RENT Timestamp,
  FFI_RENT Timestamp,
  IPO_RENT Char(1),
  COD_RENT Char(5),
  QUI_RENT Char(1) DEFAULT '0',
  BAL_RENT Char(1) DEFAULT '0'
);
CREATE TABLE REPBACO
(
  COD_REPO Char(20),
  NUM_MOVI Integer,
  COD_TIPO Char(2),
  NUM_BACO Integer
);
CREATE TABLE REPBALCOM
(
  COD_REPO Char(20),
  COD_CUEN Char(25),
  NOM_CUEN Char(100),
  SAN_MOVI Double precision,
  DEB_MOVI Double precision,
  CRE_MOVI Double precision,
  SAC_MOVI Double precision,
  COD_ACPA Char(1),
  NOM_ACPA Char(30),
  BMO_CUEN Char(1),
  CONSTRAINT PK_REPBALCOM PRIMARY KEY (COD_REPO,COD_CUEN)
);
CREATE TABLE REPBALGEN
(
  COD_REPO Char(20),
  COD_CUEN Char(25),
  NOM_CUEN Char(100),
  VAL_MOVI Double precision,
  COD_ACPA Char(1),
  NOM_ACPA Char(30),
  VTOT_CUEN Double precision,
  CONSTRAINT PK_REPBALGEN PRIMARY KEY (COD_REPO,COD_CUEN)
);
CREATE TABLE REPCART
(
  SEC_DCTO Double precision,
  FEC_DCTO Timestamp,
  DET_DCTO Char(250),
  COD_CLIE Char(9),
  NOM_CLIE Char(50),
  TNI_DCTO Double precision,
  TSI_DCTO Double precision,
  IVA_DCTO Double precision,
  PAG_DCTO Double precision,
  COD_REPO Char(20),
  FUP_REPO Timestamp DEFAULT '01/01/2000'
);
CREATE TABLE REPCOMPC
(
  COD_REPO Char(20),
  COD_CUEN Char(25),
  NOM_CUEN Char(100),
  DET_MOVI Varchar(240),
  REF_MOVI Char(15),
  DEB_MOVI Double precision,
  CRE_MOVI Double precision,
  NUMMCOMP Double precision,
  NUM_LINE Integer
);
CREATE TABLE REPDESCRED
(
  COD_REPO Char(20),
  COD_PART Char(15),
  COD_SOCI Char(9),
  NOM_SOCI Char(60),
  TCR_CABE Double precision,
  TIN_CABE Double precision,
  NCU_MOVI Char(2),
  FVE_MOVI Timestamp,
  TCR_MOVI Double precision,
  TIN_MOVI Double precision
);
CREATE TABLE REPESTCUEN
(
  NUMREP Integer,
  CODREP Char(100) DEFAULT '',
  DOCANT Char(2),
  COD_CLIE Char(10),
  CODGRUP Char(2) DEFAULT '',
  TIPO Char(3) DEFAULT '',
  FECHA Timestamp DEFAULT current_timestamp  ,
  SECUENCIAL Integer DEFAULT 0,
  NUMERO Char(15) DEFAULT '',
  CONTABLE Integer DEFAULT 0,
  DETALLE Char(240) DEFAULT '',
  DEBITO Decimal(13,2) DEFAULT 0,
  CREDITO Decimal(13,2) DEFAULT 0,
  PLACA Char(20) DEFAULT '',
  PRIMARY KEY (NUMREP)
);
CREATE TABLE REPKARCOS23
(
  COD_REPO Char(40),
  COD_BODE Char(2),
  TIP_DCTO Char(2),
  SEC_DCTO Integer,
  NUM_DCTO Char(15),
  FEC_DCTO Timestamp,
  RUC_CLIE Char(15),
  NOM_CLIE Char(300),
  PLA_DCTO Char(20),
  ING_MOVI Decimal(12,3),
  EGR_MOVI Decimal(12,3),
  VAL_MOVI Decimal(12,6),
  COS_MOVI Decimal(12,6),
  TOT_MOVI Decimal(12,6),
  SAL_TOTA Decimal(12,3),
  COS_TOTA Decimal(12,6),
  TOT_TOTA Decimal(12,6),
  GUI_DCTO Char(20),
  RUC_CHOF Char(15),
  NOM_CHOF Char(300),
  NUM_CONS Integer,
  HOR_DCTO Time
);
CREATE TABLE REPMAYGEN
(
  COD_REPO Char(20),
  COD_CUEN Char(25),
  NOM_CUEN Char(100),
  NUMMCOMP Integer DEFAULT 0,
  NUM_MOVI Integer,
  CHE_COMP Double precision,
  NUM_COMP Char(20),
  FAP_COMP Timestamp,
  DET_COMP Char(240),
  REF_MOVI Char(20),
  SAN_MOVI Double precision,
  DEB_MOVI Double precision,
  CRE_MOVI Double precision,
  SAC_MOVI Double precision,
  COD_ACPA Char(1),
  NOM_ACPA Char(30),
  BMO_CUEN Char(1),
  CONSTRAINT PK_REPMAYGEN PRIMARY KEY (COD_REPO,COD_CUEN,NUM_MOVI)
);
CREATE TABLE REQUE
(
  ID_REQUE Integer,
  COD_CLIE Char(9),
  REQUERIMIENTOS Varchar(8000),
  OBSERVACIONES Varchar(8000),
  FECHA_CREATE Timestamp,
  FECHA_UPDATE Timestamp,
  ESTADO_REQUE Char(1),
  COD_USUA Char(10),
  PRIMARY KEY (ID_REQUE)
);
CREATE TABLE RETE
(
  NUM_RETE Double precision,
  FEC_RETE Timestamp,
  DES_RETE Char(13),
  HAS_RETE Char(13),
  ACT_RETE Char(13),
  AUT_RETE Char(10),
  CAD_RETE Char(7),
  PRIMARY KEY (NUM_RETE)
);
CREATE TABLE REUS
(
  COD_USUA Char(10),
  NUM_RETE Double precision,
  FEC_REUS Timestamp
);
CREATE TABLE RFID
(
  COD_RFID Char(2),
  NOM_RFID Char(50) DEFAULT '',
  NIP_RFID Char(20) DEFAULT '',
  EST_RFID Char(1) DEFAULT '0',
  PRIMARY KEY (COD_RFID)
);
CREATE TABLE RFID_COMB
(
  SEC_RFID Integer,
  NUM_RFID Char(10) DEFAULT '',
  COD_RFID Char(20) DEFAULT '',
  EST_RFID Char(1) DEFAULT '0',
  SAL_RFID Decimal(10,2) DEFAULT 0,
  CUP_RFID Decimal(10,2) DEFAULT 0,
  CCU_RFID Decimal(10,2) DEFAULT 0,
  KLM_RFID Char(20) DEFAULT '',
  PRO_RFID Char(20) DEFAULT '',
  COD_ITEM Char(20) DEFAULT '',
  SAP_RFID Decimal(10,2) DEFAULT 0,
  REP_RFID Decimal(10,2) DEFAULT 0,
  KEY_RFID Char(50) DEFAULT '',
  DET_RFID Char(100) DEFAULT '',
  FEC_RFID Timestamp DEFAULT current_timestamp ,
  SEC_DCTO Integer DEFAULT 0,
  ESE_RFID Char(1) DEFAULT '0',
  IDE_RFID Char(20) DEFAULT '',
  CAN_RFID Decimal(10,2) DEFAULT 0,
  VAL_RFID Decimal(10,2) DEFAULT 0,
  PRIMARY KEY (SEC_RFID)
);
CREATE TABLE RFID_KEY
(
  NUM_RFID Integer,
  NID_RFID Char(10) DEFAULT '',
  KEY_RFID Char(50) DEFAULT '',
  DET_RFID Char(100) DEFAULT '',
  FEC_RFID Timestamp DEFAULT current_timestamp ,
  EST_RFID Char(1) DEFAULT '',
  PRIMARY KEY (NUM_RFID)
);
CREATE TABLE RIVA
(
  COD_RIVA Char(5),
  NOM_RIVA Char(250),
  FIN_RIVA Timestamp,
  FFI_RIVA Timestamp,
  IPO_RIVA Char(1),
  POR_RIVA Integer,
  BSE_RIVA Char(1),
  VAL_RIVA Decimal(10,2) DEFAULT 0,
  PRIMARY KEY (COD_RIVA)
);
CREATE TABLE RMAN
(
  NUM_RMAN Integer,
  DET_MANG Char(244),
  FDE_MANG Timestamp,
  FHA_MANG Timestamp,
  CONSTRAINT PK_RMAN PRIMARY KEY (NUM_RMAN,DET_MANG)
);
CREATE TABLE ROLP
(
  NUM_ROLP Integer,
  MES_ROLP Timestamp,
  FEC_ROLP Timestamp,
  TSU_ROLP Double precision,
  TDE_ROLP Double precision,
  TCR_ROLP Double precision,
  TOC_ROLP Double precision,
  TPR_ROLP Double precision,
  DET_ROLP Char(254),
  NUM_RUBR Integer DEFAULT '0',
  COD_DEPA Char(2) DEFAULT '00',
  NUMMCOMP Double precision DEFAULT 0,
  CONSTRAINT PK_ROLP PRIMARY KEY (NUM_ROLP)
);
CREATE TABLE ROL_MOV
(
  NUM_ROMO Integer,
  NUM_EMPL Integer,
  NUM_RUBR Integer,
  FEC_ROMO Timestamp,
  DET_ROMO Char(100),
  VAL_ROMO Double precision,
  NUM_ROLP Integer,
  MES_ROMO Timestamp,
  NUMMCOMP Double precision,
  DIA_ROMO Integer,
  DST_RUBR Char(1),
  TCU_ROMO Integer,
  NCU_ROMO Integer,
  COD_BANC Char(2),
  FPA_ROMO Char(1),
  REC_ROMO Char(15),
  NUM_ROLL Integer,
  NUMMCOML Double precision,
  NUM_MOVI_CON Integer DEFAULT 0,
  NUMMCOMP_CON Integer DEFAULT 0,
  BEN_RUBR Char(1) DEFAULT '0'
);
CREATE TABLE ROL_RUB
(
  NUM_RUBR Integer,
  NOM_RUBR Char(100),
  TIP_RUBR Char(1),
  FIC_RUBR Timestamp,
  SUM_RUBR Char(1),
  FFC_RUBR Timestamp,
  FPA_RUBR Timestamp,
  CAD_RUBR Integer,
  PSU_RUBR Double precision,
  DCR_RUBR Char(1),
  DST_RUBR Char(1),
  ORD_RUBR Char(2),
  RES_RUBR Char(1),
  CDI_RUBR Char(1) DEFAULT '1',
  EST_RUBR Char(1) DEFAULT '1',
  IES_RUBR Char(1) DEFAULT '1',
  BEN_RUBR Char(1) DEFAULT '0',
  ESP_RUBR Char(1) DEFAULT '0',
  CONSTRAINT PK_ROL_RUB PRIMARY KEY (NUM_RUBR)
);
CREATE TABLE RUTA
(
  NUM_RUTA Integer,
  COD_ZONA Char(2) DEFAULT '01',
  COD_VEND Char(2) DEFAULT '01',
  COD_CLIE Char(9) DEFAULT '',
  DIA_RUTA Integer DEFAULT 0,
  ORD_RUTA Integer DEFAULT 0,
  PRIMARY KEY (NUM_RUTA)
);
CREATE TABLE SECU
(
  COD_SECU Char(2),
  COD_TRAN Char(1),
  IDE_SECU Char(1),
  NOM_SECU Char(20),
  CODTCOMP Char(200),
  PRIMARY KEY (COD_SECU)
);
CREATE TABLE SERI
(
  COD_ITEM Char(100),
  NUM_SERI Char(20),
  EGR_SERI Char(1),
  SEC_DCTO Double precision
);
CREATE TABLE SESS
(
  NUM_SESS Integer,
  NIP_SESS Char(20) DEFAULT '',
  SES_SESS Integer DEFAULT 0,
  COD_USUA Char(10) DEFAULT '',
  FIN_SESS Timestamp DEFAULT current_timestamp ,
  FFI_SESS Timestamp DEFAULT current_timestamp ,
  EST_SESS Char(1) DEFAULT '0',
  PRIMARY KEY (NUM_SESS)
);
CREATE TABLE SIVA
(
  COD_SIVA Char(5),
  NOM_SIVA Char(250),
  FIN_SIVA Timestamp,
  FFI_SIVA Timestamp,
  IPO_SIVA Char(1),
  COD_SUST Char(2),
  BSE_SIVA Char(1),
  PRIMARY KEY (COD_SIVA)
);
CREATE TABLE SOCI
(
  COD_SOCI Char(9),
  TID_SOCI Char(1),
  RUC_SOCI Char(13),
  RAZ_SOCI Char(50),
  APE_SOCI Char(40),
  NOM_SOCI Char(60),
  DCA_SOCI Char(50),
  DNU_SOCI Char(10),
  DCI_SOCI Char(20),
  DPR_SOCI Char(2),
  EST_SOCI Char(1),
  TE1_SOCI Char(9),
  TE2_SOCI Char(9),
  FAX_SOCI Char(9),
  COR_SOCI Char(50),
  FIN_SOCI Timestamp,
  FFI_SOCI Timestamp,
  ADD_SOCI Double precision,
  PRIMARY KEY (COD_SOCI)
);
CREATE TABLE SUST
(
  COD_SUST Char(2),
  NOM_SUST Char(250),
  CODTCOMP Char(200),
  TIP_GRUP Char(1),
  DIV_SUST Char(1) DEFAULT '0',
  PRIMARY KEY (COD_SUST)
);
CREATE TABLE SYSLOCK
(
  NUM_SYSLOCK Integer,
  FDE_SYSLOCK Timestamp,
  FHA_SYSLOCK Timestamp,
  COD_USUASYSLOCK Char(10) DEFAULT '',
  ADM_SYSLOCK Char(1) DEFAULT '0',
  PRIMARY KEY (NUM_SYSLOCK)
);
CREATE TABLE SYSLOCKEXCE
(
  NUM_SYSLOCKEXCE Integer DEFAULT 0,
  COD_USUA Char(10) DEFAULT '',
  NIP_TRAM Char(20) DEFAULT '',
  NUM_TRAM Integer DEFAULT 0,
  SES_SESS Integer DEFAULT 0,
  TAB_SYSLOCKEXCE Char(30) DEFAULT '',
  EST_SYSLOCKEXCE Char(1) DEFAULT '0',
  NUM_XXXX_SYSLOCKEXCE Integer DEFAULT 0,
  PRIMARY KEY (NUM_SYSLOCKEXCE)
);
CREATE TABLE SYSLOCKMOVI
(
  NUM_SYSLOCKMOVI Integer,
  NUM_SYSLOCK Integer,
  NUM_SYSMODUTIPO Integer,
  AGR_SYSLOCKMOVI Char(1) DEFAULT '0',
  MOD_SYSLOCKMOVI Char(1) DEFAULT '0',
  ELI_SYSLOCKMOVI Char(1) DEFAULT '0',
  PRO_SYSLOCKMOVI Char(1) DEFAULT '0',
  PRIMARY KEY (NUM_SYSLOCKMOVI)
);
CREATE TABLE SYSMODU
(
  NUM_SYSMODU Integer,
  NOM_SYSMODU Char(100),
  PRIMARY KEY (NUM_SYSMODU)
);
CREATE TABLE SYSMODUTIPO
(
  NUM_SYSMODUTIPO Integer,
  DOC_SYSMODUTIPO Char(2),
  NOM_SYSMODUTIPO Char(100),
  NUM_SYSMODU Integer,
  PRIMARY KEY (NUM_SYSMODUTIPO)
);
CREATE TABLE TANQ
(
  COD_TANQ Char(10),
  NOM_TANQ Char(50),
  TOT_TANQ Double precision,
  MIN_TANQ Double precision,
  COD_ITEM Char(25),
  COD_BODE Char(2) DEFAULT '00',
  SNR_TANQ Char(10) DEFAULT '',
  FUL_TANQ Timestamp DEFAULT current_timestamp ,
  SCO_TANQ Double precision DEFAULT 0,
  SAG_TANQ Double precision DEFAULT 0,
  TEM_TANQ Double precision DEFAULT 0,
  DEN_TANQ Double precision DEFAULT 0,
  SUM_TANQ Double precision DEFAULT 0,
  SCO_GAL_TANQ Double precision DEFAULT 0,
  SAG_GAL_TANQ Double precision DEFAULT 0,
  FMX_TANQ Char(11) DEFAULT '',
  HAB_TANQ Char(1) DEFAULT '1',
  LUB_TANQ Char(1) DEFAULT '0',
  SCO_INI_TANQ Double precision DEFAULT 0,
  SAG_INI_TANQ Double precision DEFAULT 0,
  SCO_FIN_TANQ Double precision DEFAULT 0,
  SAG_FIN_TANQ Double precision DEFAULT 0,
  SCO_INI_GAL_TANQ Double precision DEFAULT 0,
  SAG_INI_GAL_TANQ Double precision DEFAULT 0,
  SCO_FIN_GAL_TANQ Double precision DEFAULT 0,
  SAG_FIN_GAL_TANQ Double precision DEFAULT 0,
  FEC_INI_TANQ Timestamp DEFAULT current_timestamp ,
  FEC_FIN_TANQ Timestamp DEFAULT current_timestamp ,
  TOT_VEN_TANQ Double precision DEFAULT 0,
  COD_VEND Char(9) DEFAULT '000',
  TOT_GAL_TANQ Double precision DEFAULT 0,
  PLA_TANQ Char(20) DEFAULT '',
  TIP_CIE_TANQ Char(1) DEFAULT 'C',
  ALI_TANQ Char(2) DEFAULT '',
  COPY_TANQ Char(2) DEFAULT '00',
  CONSTRAINT PK_TANQ PRIMARY KEY (COD_TANQ)
);
CREATE TABLE TANQ_ING
(
  NUM_TQIN Integer,
  NUM_MOVI Integer,
  COD_TANQ Char(2),
  CAN_TQIN Numeric(13,3) DEFAULT '0.000',
  PRIMARY KEY (NUM_TQIN)
);
CREATE TABLE TANQ_MOV
(
  NUM_TQMV Integer,
  COD_TANQ Char(2) DEFAULT '00',
  FEC_TQMV Timestamp DEFAULT current_timestamp ,
  SCO_TQMV Numeric(13,3) DEFAULT '0.000',
  SAG_TQMV Numeric(13,3) DEFAULT '0.000',
  TEM_TQMV Numeric(13,3) DEFAULT '0.000',
  DEN_TQMV Numeric(13,3) DEFAULT '0.000',
  SUM_TQMV Numeric(13,3) DEFAULT '0.000',
  SCO_GAL_TQMV Numeric(13,3) DEFAULT '0.000',
  SAG_GAL_TQMV Numeric(13,3) DEFAULT '0.000',
  PRIMARY KEY (NUM_TQMV)
);
CREATE TABLE TANQ_REPO
(
  NUM_TANQ_REPO Integer,
  NUM_REPO Integer DEFAULT 0,
  FEC_INI_REPO Timestamp,
  FEC_FIN_REPO Timestamp,
  COD_TANQ Char(2) DEFAULT '',
  FEC_INI_TANQ Timestamp,
  FEC_FIN_TANQ Timestamp,
  SAL_INI_CEN_TANQ Decimal(12,3) DEFAULT 0,
  SAL_INI_GAL_TANQ Decimal(12,3) DEFAULT 0,
  SAL_FIN_CEN_TANQ Decimal(12,3) DEFAULT 0,
  SAL_FIN_GAL_TANQ Decimal(12,3) DEFAULT 0,
  DESCARGAS_TANQ Decimal(12,3) DEFAULT 0,
  VENTAS_TANQ Decimal(12,3) DEFAULT 0,
  FINAL_SISTEMA Decimal(12,3) DEFAULT 0,
  DIFERENCIA Decimal(12,3) DEFAULT 0,
  VENTAS_TANQ_DOL Decimal(12,3) DEFAULT 0,
  PRIMARY KEY (NUM_TANQ_REPO)
);
CREATE TABLE TANQ_RIND
(
  NUM_TANQ_RIND Integer,
  NUM_MOLI_TANQ Integer,
  COD_TANQ Char(10),
  SCO_INI_TANQ Double precision DEFAULT 0,
  SCO_INI_GAL_TANQ Double precision DEFAULT 0,
  SAG_INI_TANQ Double precision DEFAULT 0,
  SAG_INI_GAL_TANQ Double precision DEFAULT 0,
  COD_VEND Char(9) DEFAULT '000',
  FEC_INI_TANQ Timestamp DEFAULT current_timestamp   ,
  FEC_FIN_TANQ Timestamp DEFAULT current_timestamp   ,
  TIP_MOLI_TANQ Char(2) DEFAULT 'TQ',
  TOT_VEN_TANQ Double precision DEFAULT 0,
  SCO_FIN_TANQ Double precision DEFAULT 0,
  SCO_FIN_GAL_TANQ Double precision DEFAULT 0,
  SAG_FIN_TANQ Double precision DEFAULT 0,
  SAG_FIN_GAL_TANQ Double precision DEFAULT 0,
  CIE_MOLI_TANQ Integer DEFAULT 0,
  SEC_DCTO Double precision DEFAULT 0,
  TOT_GAL_TANQ Double precision DEFAULT 0,
  PLA_MOLI_TANQ Char(20) DEFAULT '',
  REN_MOLI_TANQ Double precision DEFAULT 0,
  NUM_TQMV Integer DEFAULT 0,
  FEC_TQMV Timestamp DEFAULT current_timestamp   ,
  SCO_TQMV Double precision DEFAULT 0,
  SAG_TQMV Double precision DEFAULT 0,
  SCO_GAL_TQMV Double precision DEFAULT 0,
  SAG_GAL_TQMV Double precision DEFAULT 0,
  TOT_VEN_RIND Double precision DEFAULT 0,
  DIF_VEN_RIND Double precision DEFAULT 0,
  PRIMARY KEY (NUM_TANQ_RIND)
);
CREATE TABLE TANQ_TAB
(
  NUM_TQTB Integer,
  COD_TANQ Char(2) DEFAULT '00',
  SCO_TQTB Numeric(13,3) DEFAULT '0.000',
  SCO_GAL_TQTB Numeric(13,3) DEFAULT '0.000',
  PRIMARY KEY (NUM_TQTB)
);
CREATE TABLE TICKET
(
  NUM_TICK Integer,
  COD_CLIE Char(9) DEFAULT '',
  DET_TICK Char(200) DEFAULT '',
  FEC_TICK Timestamp DEFAULT current_timestamp  ,
  FUM_TICK Timestamp DEFAULT current_timestamp  ,
  TOT_TICK Numeric(13,2) DEFAULT 0,
  NCU_TICK Integer DEFAULT 0,
  VCU_TICK Numeric(13,2) DEFAULT 0,
  VEN_TICK Numeric(13,2) DEFAULT 0,
  SAL_TICK Numeric(13,2) DEFAULT 0,
  EST_TICK Char(1) DEFAULT 'A',
  COD_ITEM Char(20) DEFAULT '',
  DES_TICK Integer DEFAULT 0,
  HAS_TICK Integer DEFAULT 0,
  BAN_ORI_TICK Char(1) DEFAULT '0',
  NUM_MOVI Integer DEFAULT 0,
  SEC_DCTO Integer DEFAULT 0,
  NUM_DCTO Char(20) DEFAULT '',
  VAL_TOT_TICK Numeric(13,2) DEFAULT 0,
  PRIMARY KEY (NUM_TICK)
);
CREATE TABLE TICKET_MOV
(
  NUM_TICK_MOV Integer,
  NUM_TICK Integer DEFAULT 0,
  CBA_ITEM Char(20) DEFAULT '',
  VAL_TICK_MOV Numeric(13,2) DEFAULT 0,
  VEN_TICK_MOV Numeric(13,2) DEFAULT 0,
  SAL_TICK_MOV Numeric(13,2) DEFAULT 0,
  EST_TICK_MOV Char(1) DEFAULT 'A',
  SEC_DCTO Integer DEFAULT 0,
  PRIMARY KEY (NUM_TICK_MOV)
);
CREATE TABLE TIPC
(
  COD_TIPC Char(2),
  NOM_TIPC Char(50),
  FAP_TIPC Timestamp,
  PRIMARY KEY (COD_TIPC)
);
CREATE TABLE TLMM
(
  NUM_TLMM Double precision,
  COD_SUST Char(2),
  DEVIVA Char(1),
  COD_CLIE Char(9),
  COD_COMP Char(2),
  FECHAREGISTRO Timestamp,
  ESTABLECIMIENTO Char(3),
  PUNTOEMISION Char(3),
  SECUENCIAL Char(9),
  FECHAEMISION Timestamp,
  AUTORIZACION Varchar(50),
  FECHACADUCIDAD Timestamp,
  BASEIMPONIBLE Double precision,
  BASEIMPGRAV Double precision,
  PORCENTAJEIVA Integer,
  MONTOIVA Double precision,
  BASEIMPICE Double precision,
  PORCENTAJEICE Integer,
  MONTOICE Double precision,
  MONTOIVABIENES Double precision,
  PORRETBIENES Integer,
  VALORRETBIENES Double precision,
  MONTOIVASERVICIOS Double precision,
  PORRETSERVICIOS Integer,
  VALORRETSERVICIOS Double precision,
  ESTABRETENCION1 Char(3),
  PTOEMIRETENCION1 Char(3),
  SECRETENCION1 Char(9),
  AUTRETENCION1 Varchar(100),
  FECHAEMIRET1 Timestamp,
  DOCMODIFICADO Char(2),
  FECHAEMIMODIFICADO Timestamp,
  ESTABMODIFICADO Char(3),
  PTOEMIMODIFICADO Char(3),
  SECMODIFICADO Char(9),
  AUTMODIFICADO Varchar(100),
  CONTRATOPARTIDOPOLITICO Char(10),
  MONTOTITULOONEROSO Double precision,
  MONTOTITULOGRATUITO Double precision,
  ESTABRETENCION2 Char(3),
  PTOEMIRETENCION2 Char(3),
  SECRETENCION2 Char(9),
  AUTRETENCION2 Varchar(100),
  FECHAEMIRET2 Timestamp,
  TOTRETAIR Double precision,
  OBS_TLMM Char(15),
  BASEIMPGRAVBIEN Double precision,
  BASEIMPGRAVSERV Double precision,
  BASEIMPONIBLEBIEN Double precision,
  BASEIMPONIBLESERV Double precision,
  NUMMCOMP Double precision,
  COD_ACTI Char(2),
  SEC_DCTO Double precision,
  NUM_PGCA Double precision,
  PBA_TLMM Double precision,
  PCA_TLMM Double precision,
  PPR_TLMM Double precision,
  RCA_TLMM Char(15),
  RBA_TLMM Char(15),
  RPR_TLMM Char(15),
  CCA_TLMM Char(2),
  CBA_TLMM Char(2),
  DET_TLMM Char(500),
  BASEIMPNOGRAV Double precision,
  BASEIMPNOGRAVBIEN Double precision,
  BASEIMPNOGRAVSERV Double precision,
  ESE_TLMM Char(4) DEFAULT '',
  DEE_TLMM Char(200) DEFAULT '',
  CLE_TLMM Char(100) DEFAULT '',
  PAN_TLMM Double precision DEFAULT 0,
  PBANUM_TLMM Integer DEFAULT 0,
  PCANUM_TLMM Integer DEFAULT 0,
  PPRNUM_TLMM Integer DEFAULT 0,
  PANNUM_TLMM Integer DEFAULT 0,
  PBACOD_TLMM Char(3) DEFAULT '',
  PCACOD_TLMM Char(3) DEFAULT '',
  PPRCOD_TLMM Char(3) DEFAULT '',
  PANCOD_TLMM Char(3) DEFAULT '',
  PNCNUM_TLMM Integer DEFAULT 0,
  RI0_TLMM Char(1) DEFAULT '0',
  ENVIAIVA_TLMM Char(1) DEFAULT '1',
  ENVIAIR_TLMM Char(1) DEFAULT '1',
  PRIMARY KEY (NUM_TLMM)
);
CREATE TABLE TLVV
(
  NUM_TLVV Double precision,
  TIPOCOMPROBANTE Char(2),
  FECHAREGISTRO Timestamp,
  NUMEROCOMPROBANTES Char(12),
  FECHAEMISION Timestamp,
  IVAPRESUNTIVO Char(1),
  PORCENTAJEIVA Integer,
  MONTOIVA Double precision,
  PORCENTAJEICE Double precision,
  MONTOICE Double precision,
  MONTOIVABIENES Double precision,
  PORRETBIENES Integer,
  VALORRETBIENES Double precision,
  MONTOIVASERVICIOS Double precision,
  PORRETSERVICIOS Integer,
  VALORRETSERVICIOS Double precision,
  RETPRESUNTIVA Char(1),
  TOTRETAIR Double precision,
  SEC_DCTO Double precision,
  NUMMCOMP Double precision,
  COD_ACTI Char(2),
  NUM_PGCA Double precision,
  PBA_TLVV Double precision DEFAULT 0,
  PCA_TLVV Double precision DEFAULT 0,
  PPR_TLVV Double precision DEFAULT 0,
  RCA_TLVV Char(15) DEFAULT '',
  RBA_TLVV Char(15) DEFAULT '',
  RPR_TLVV Char(15) DEFAULT '',
  CCA_TLVV Char(2) DEFAULT '',
  CBA_TLVV Char(2) DEFAULT '',
  DET_TLVV Char(240) DEFAULT '',
  NUMERORETENCION Char(20) DEFAULT '',
  AUTORIZACIONRETENCION Char(50) DEFAULT '',
  CONSTRAINT PK_TLVV PRIMARY KEY (NUM_TLVV)
);
CREATE TABLE TRAM
(
  NUM_TRAM Integer,
  COD_USUA Char(10) DEFAULT '',
  NIP_TRAM Char(20) DEFAULT '',
  TAB_TRAM Char(20) DEFAULT '',
  NDO_TRAM Integer DEFAULT 0,
  EST_TRAM Char(1) DEFAULT '1',
  FEC_TRAM Timestamp DEFAULT current_timestamp ,
  NTL_TRAM Integer DEFAULT 1,
  TIP_TRAM Char(1) DEFAULT 'A',
  CONSTRAINT PK_TRAM PRIMARY KEY (NUM_TRAM)
);
CREATE TABLE TRAM_MOV
(
  NUM_TRMO Integer,
  NUM_TRAM Integer,
  TAB_TRMO Char(20) DEFAULT '',
  NTL_TRAM Integer DEFAULT 1,
  CONSTRAINT PK_TRAM_MOV PRIMARY KEY (NUM_TRMO)
);
CREATE TABLE TURN
(
  NUM_TURN Integer,
  COD_VEND Char(9),
  FIN_TURN Timestamp,
  FFI_TURN Timestamp,
  SIN_TURN Double precision,
  ING_TURN Double precision,
  EGR_TURN Double precision,
  SFI_TURN Double precision,
  EST_TURN Char(1) DEFAULT '0',
  DET_TURN Char(200) DEFAULT '',
  IN2_TURN Double precision,
  FAL_TURN Double precision DEFAULT 0,
  SOB_TURN Double precision DEFAULT 0,
  CRE_TURN Double precision DEFAULT 0,
  CONSTRAINT PK_TURN PRIMARY KEY (NUM_TURN)
);
CREATE TABLE TURN_DEPO
(
  NUM_TUDP Integer,
  COD_VEND Char(9) DEFAULT '',
  NUM_TURN Integer DEFAULT 0,
  FEC_TUDP Timestamp DEFAULT current_timestamp   ,
  TIP_TUDP Char(4) DEFAULT 'DEPO',
  DET_TUDP Char(50),
  CAN_TUDP Integer DEFAULT 0,
  VAL_TUDP Numeric(10,2) DEFAULT 0,
  TOT_TUDP Numeric(10,2) DEFAULT 0,
  PRIMARY KEY (NUM_TUDP)
);
CREATE TABLE TURN_TARJ
(
  NUM_TURN_TARJ Integer,
  NUM_TURN Integer DEFAULT 0,
  COD_BANC Char(2) DEFAULT '',
  CAN_TURN_TARJ Integer DEFAULT 0,
  VAL_TURN_TARJ Decimal(12,2) DEFAULT 0,
  PRIMARY KEY (NUM_TURN_TARJ)
);
CREATE TABLE USUA
(
  COD_USUA Char(10),
  NOM_USUA Char(60),
  CLA_USUA Char(20),
  ICO_USUA Integer,
  COD_BANC Char(2),
  COD_BODE Char(2),
  COD_VEND Char(3) DEFAULT '000',
  PUN_VEND Char(1) DEFAULT '0',
  COD_XUSUA Char(31) DEFAULT '',
  EST_ELES Char(3) DEFAULT '000',
  PUN_ELPU Char(3) DEFAULT '000',
  MAC_USUA Char(50) DEFAULT '',
  WSER_USUA Char(1) DEFAULT '0',
  IDET_USUA Char(1) DEFAULT '1',
  COD_BODE_USUA Char(2) DEFAULT '00',
  STOCK_USUA Char(1) DEFAULT '1',
  USUA_VALIDA Char(1) DEFAULT '1',
  ELEC_USUA Char(1) DEFAULT '0',
  ADMIN_USUA Char(1) DEFAULT '0',
  PUERTO_USUA Char(4) DEFAULT '5101',
  COR_USUA Char(1) DEFAULT '0',
  PRIMARY KEY (COD_USUA)
);
CREATE TABLE VEND
(
  NOM_VEND Char(50),
  COD_VEND Char(3),
  DIA_VEND Integer,
  POR_VEND Double precision,
  COD_ZONA Char(2) DEFAULT '01',
  NUM_EMPL Integer DEFAULT 0,
  PRIMARY KEY (COD_VEND)
);
CREATE TABLE VENT
(
  COD_VENT Char(5),
  NOM_VENT Char(60),
  PRIMARY KEY (COD_VENT)
);
CREATE TABLE XBANC
(
  NUM_XBANC Integer,
  COD_BANC Char(2),
  NOM_BANC Char(255),
  CTA_BANC Char(20),
  COD_CUEN Char(25),
  CBA_BANC Char(1),
  CPO_BANC Char(1) DEFAULT '0',
  NUM_LOG Numeric(18,0),
  CONSTRAINT PK_XBANC PRIMARY KEY (NUM_XBANC)
);
CREATE TABLE XDCTO
(
  NUM_XDCTO Integer,
  SEC_DCTO Double precision,
  TIP_DCTO Char(2),
  NUM_DCTO Char(20),
  FEC_DCTO Timestamp,
  COD_CLIE Char(9),
  TNI_DCTO Double precision,
  TSI_DCTO Double precision,
  DSC_DCTO Double precision,
  IVA_DCTO Double precision,
  COD_BODE Char(2),
  NCU_DCTO Integer,
  DCU_DCTO Integer,
  DET_DCTO Char(250),
  OBS_DCTO Char(250),
  RUC_DCTO Char(13),
  DIR_DCTO Char(250),
  COD_VEND Char(9),
  COD_PAGO Char(3),
  NDO_DCTO Char(20),
  BPA_DCTO Char(1),
  PAG_DCTO Double precision,
  COL_DCTO Char(50),
  NUMMCOMP Double precision,
  NUM_LIQV Double precision,
  GUI_DCTO Varchar(50),
  FGU_DCTO Timestamp,
  COD_ACTI Char(2),
  ANE_DCTO Char(1),
  AUT_DCTO Char(50),
  ESE_DCTO Char(4),
  DEE_DCTO Char(200),
  CLE_DCTO Char(100),
  SER_DCTO Double precision,
  PLA_DCTO Char(20),
  ORD_DCTO Char(20),
  NUM_TURN Integer,
  VAC_DCTO Char(2),
  COD_CHOF Char(9),
  NUM_CONS Integer,
  SUB_DCTO Double precision,
  TER_DCTO Double precision,
  FUL_CABE Timestamp,
  TAZ_CRED Timestamp,
  PCM_DCTO Integer,
  VCM_DCTO Double precision,
  PCMR_DCTO Integer,
  VCMR_DCTO Double precision,
  DNI_DCTO Double precision,
  DSI_DCTO Double precision,
  IMP_DCTO Double precision,
  ESG_DCTO Char(4),
  DEG_DCTO Varchar(200),
  CLG_DCTO Varchar(100),
  AUG_DCTO Varchar(50),
  PIV_DCTO Integer,
  COD_GRUP Char(4),
  COD_XUSUA Char(10),
  FEC_XDCTO Timestamp,
  NIP_XDCTO Char(20),
  MEL_XDCTO Char(1) DEFAULT 'M',
  NUM_TRAM Integer DEFAULT 0,
  COD_MANG Char(2),
  NUM_TICK_MOV Integer,
  NUM_VAUC Integer,
  DSCA0_DCTO Double precision,
  DSCA_DCTO Double precision,
  PLG_DCTO Varchar(20),
  CONSTRAINT PK_XDCTO PRIMARY KEY (NUM_XDCTO)
);
CREATE TABLE XPGCA
(
  NUM_XPGCA Integer,
  NUM_PGCA Double precision,
  FPA_PGCA Timestamp,
  FVE_PGCA Timestamp,
  VAL_PGCA Double precision,
  INT_PGCA Double precision,
  OTR_PGCA Double precision,
  COD_PAGO Char(3),
  COD_BANC Char(2),
  CTA_PGCA Char(20),
  CHE_PGCA Char(30),
  NUMMCOMP Double precision,
  FEC_XOGCA Timestamp,
  NIP_XPGCA Char(50),
  USR_XPGCA Char(50),
  PRIMARY KEY (NUM_XPGCA)
);
CREATE TABLE XPGDE
(
  NUM_XPGDE Integer,
  NUM_PGCA Double precision,
  VAL_PGDE Double precision,
  INT_PGDE Double precision,
  OTR_PGDE Double precision,
  NUM_CUOT Double precision,
  NUM_PGDE Double precision,
  DET_PGDE Char(255),
  FEC_XOGDE Timestamp,
  NIP_PGDE Char(50),
  USR_XPGDE Char(50),
  CONSTRAINT PK_XPGDE PRIMARY KEY (NUM_XPGDE)
);
CREATE TABLE ZONA
(
  COD_ZONA Char(2),
  NOM_ZONA Char(50),
  COD_PROV Char(2),
  CIU_ZONA Char(50),
  PRIMARY KEY (COD_ZONA)
);
CREATE TABLE ZONA_DIST
(
  NUM_ZONA_DIST Integer,
  COD_VEND Char(2) DEFAULT '01',
  COD_ZONA Char(2) DEFAULT '01',
  PRIMARY KEY (NUM_ZONA_DIST)
);
/********************* VIEWS **********************/

CREATE VIEW CARTERA (TIPO, SECUENCIAL, FECHA, TOTAL, CLIENTE, CONTABLE, NUMERO, DETALLE, NUMPAGO, CODACTI, VENDEDOR, TIPOFV)
AS 
select tip_dcto, dcto.sec_dcto,fec_dcto,(tni_dcto+tsi_dcto+iva_dcto+num_liqv-dsc_dcto+ser_dcto-dscA0_dcto-dsca_dcto)*iif(dcto.tip_dcto='DV' or dcto.tip_dcto='DC',-1,1) as tot_dcto,cod_clie,
Dcto.nummcomp , Dcto.NUM_dcto, Trim(Dcto.det_dcto), 0, Dcto.cod_acti, Dcto.cod_vend, Dcto.tip_dcto
From Dcto, Pago
where dcto.COD_PAGO=pago.COD_PAGO AND PAGO.CRE_PAGO='1'
 and ( dcto.tip_dcto='DC' OR dcto.tip_dcto='DV' OR dcto.tip_dcto='FC' or dcto.tip_dcto='FV'  or dcto.tip_dcto='TT' or dcto.tip_dcto='LC' or (dcto.tip_dcto='EB' and dcto.ane_dcto='1'))
Union All
select pgca.cod_pago,cuot.sec_dcto,pgca.fpa_pgca, (pgde.val_pgde+pgde.int_pgde +pgde.otr_pgde )* iif((dcto.tip_dcto='DV' or dcto.tip_dcto='DC') and pgde.val_pgde>0,1,-1) as tot_dcto
 ,dcto.cod_clie,pgca.nummcomp,dcto.num_dcto,trim(pgde.det_pgde)  ,pgde.num_pgca,dcto.COD_ACTI AS COD_ACTI,dcto.COD_VEND,dcto.tip_dcto
 From pgde, pgca, cuot, Dcto, Pago
 where pgde.num_pgca = pgca.num_pgca and
 pgde.num_cuot = cuot.num_cuot and cuot.sec_dcto =dcto.sec_dcto and dcto.COD_PAGO=PAGO.COD_PAGO and PAGO.CRE_PAGO='1'
 and ( dcto.tip_dcto='DC' OR dcto.tip_dcto='DV' OR  dcto.tip_dcto='FC' or dcto.tip_dcto='FV' or dcto.tip_dcto='TT' or dcto.tip_dcto='LC' or (dcto.tip_dcto='EB' and dcto.ane_dcto='1'))
 order by 2,5,3;
CREATE VIEW CARTERAA (TIPO, SECUENCIAL, FECHA, TOTAL, CLIENTE, CONTABLE, NUMERO, DETALLE, NUMPAGO, CODACTI, VENDEDOR, TIPOFV)
AS   select tip_dcto, dcto.sec_dcto,fec_dcto,(tni_dcto+tsi_dcto+iva_dcto+num_liqv-dsc_dcto+ser_dcto)*iif(dcto.tip_dcto='1V' or dcto.tip_dcto='1C',-1,1) as tot_dcto,cod_clie, Dcto.nummcomp , Dcto.NUM_dcto, Trim(Dcto.det_dcto), 0, Dcto.cod_acti, Dcto.cod_vend, Dcto.tip_dcto From Dcto, Pago where dcto.COD_PAGO=pago.COD_PAGO AND PAGO.CRE_PAGO='1' and ( dcto.tip_dcto='1C' OR dcto.tip_dcto='1V') Union All select pgca.cod_pago,cuot.sec_dcto,pgca.fpa_pgca, (pgde.val_pgde+pgde.int_pgde +pgde.otr_pgde )* iif((dcto.tip_dcto='1V' or dcto.tip_dcto='1C') and pgde.val_pgde>0,1,-1) as tot_dcto ,dcto.cod_clie,pgca.nummcomp,dcto.num_dcto,trim(pgde.det_pgde)  ,pgde.num_pgca,dcto.COD_ACTI AS COD_ACTI,dcto.COD_VEND,dcto.tip_dcto From pgde, pgca, cuot, Dcto, Pago where pgde.num_pgca = pgca.num_pgca and pgde.num_cuot = cuot.num_cuot and cuot.sec_dcto =dcto.sec_dcto and dcto.COD_PAGO=PAGO.COD_PAGO and PAGO.CRE_PAGO='1' and ( dcto.tip_dcto='1C' OR dcto.tip_dcto='1V') order by 2,5,3;
CREATE VIEW CARTERAEB (TIPO, SECUENCIAL, FECHA, CONSOLIDACION, FECHACONS, TOTAL, CLIENTE, CONTABLE, NUMERO, DETALLE, NUMPAGO, CODACTI, VENDEDOR, TIPOFV)
AS  
 select tip_dcto, dcto.sec_dcto,fec_dcto,num_cons,taz_CRED,(tni_dcto+tsi_dcto+iva_dcto+num_liqv-dsc_dcto+ser_dcto-dscA0_dcto-dsca_dcto)*iif(dcto.tip_dcto='DV'  
 or dcto.tip_dcto='DC',-1,1) as tot_dcto,cod_clie,  
 Dcto.nummcomp , Dcto.num_dcto, Trim(Dcto.det_dcto), 0, Dcto.cod_acti, Dcto.cod_vend, Dcto.tip_dcto  
 From Dcto, Pago  
 where dcto.COD_PAGO=pago.COD_PAGO AND PAGO.CRE_PAGO='1'  
  and (dcto.tip_dcto='DC' OR dcto.tip_dcto='DV' OR dcto.tip_dcto='FC' or dcto.tip_dcto='FV' or dcto.tip_dcto='BC' or dcto.TIP_DCTO='BV'  or dcto.tip_dcto='TT'  
  or dcto.tip_dcto='LC' OR dcto.tip_dcto='BC' OR dcto.tip_dcto='BV' OR dcto.tip_dcto='EB')  
 Union All 
 select pgca.cod_pago,cuot.sec_dcto,pgca.fpa_pgca,0,pgca.fpa_pgca, (pgde.val_pgde+pgde.int_pgde +pgde.otr_pgde )* iif((dcto.tip_dcto='DV' or dcto.tip_dcto='DC') and pgde.val_pgde>0,1,-1) as tot_dcto 
  ,dcto.cod_clie,pgca.nummcomp,dcto.num_dcto,trim(pgde.det_pgde)  ,pgde.num_pgca,dcto.COD_ACTI AS COD_ACTI,dcto.COD_VEND,dcto.tip_dcto 
  From pgde, pgca, cuot, Dcto, Pago 
  where pgde.num_pgca = pgca.num_pgca and 
  pgde.num_cuot = cuot.num_cuot and cuot.sec_dcto =dcto.sec_dcto and dcto.COD_PAGO=PAGO.COD_PAGO and PAGO.CRE_PAGO='1' 
  and ( dcto.tip_dcto='DC' OR dcto.tip_dcto='DV' OR  dcto.tip_dcto='FC' or dcto.tip_dcto='FV' or dcto.tip_dcto='BC' or dcto.TIP_DCTO='BV'   or dcto.tip_dcto='TT' or dcto.tip_dcto='LC' OR dcto.tip_dcto='BC' OR dcto.tip_dcto='BV' 
  or dcto.tip_dcto='EB') 
  order by 2,5,3;
CREATE VIEW CARTERAR (TIPO, SECUENCIAL, FECHA, TOTAL, CLIENTE, CONTABLE, NUMERO, DETALLE, NUMPAGO, CODACTI)
AS 
select tip_dcto, dcto.sec_dcto AS SEC_DCTO,fec_dcto as fec_dcto,(tni_dcto+tsi_dcto+iva_dcto+num_liqv-dsc_dcto) as tot_dcto,cod_clie,
dcto.nummcomp,dcto.num_dcto,trim(dcto.det_dcto) AS DET_DCTO,0 as num_pgca,DCTO.cod_acti from dcto,cuot
where dcto.sec_dcto=cuot.sec_dcto 
union all
select pgca.cod_pago,pgca.num_pgca AS SEC_DCTO,pgca.fpa_pgca as fec_dcto, sum(pgde.val_pgde+pgde.int_pgde +pgde.otr_pgde )*-1 as tot_dcto
 ,dcto.cod_clie,pgca.nummcomp,'' as num_dcto,trim('') AS DET_DCTO,pgde.num_pgca as num_pgca,dcto.cod_acti AS COD_ACTI
 from pgde, pgca,cuot, dcto where pgde.num_pgca = pgca.num_pgca and
 pgde.num_cuot = cuot.num_cuot and cuot.sec_dcto =dcto.sec_dcto
 group by cod_pago,num_pgca,sec_dcto,fec_dcto,cod_clie,nummcomp,num_dcto,det_dcto,num_pgca,cod_acti
 order by 2,5,3
;
CREATE VIEW CHOFER (COD_CLIE, TID_CLIE, RUC_CLIE, RAZ_CLIE, APE_CLIE, NOM_CLIE, DCA_CLIE, DNU_CLIE, DCI_CLIE, DPR_CLIE, CPR_CLIE, TE1_CLIE, TE2_CLIE, FAX_CLIE, COR_CLIE, FIN_CLIE, CRE_CLIE, EST_CLIE, COD_ZONA, CAN_CLIE, QUI_CLIE, CST_CLIE, REL_CLIE, CUP_CLIE, MAI_CLIE, ARC_CLIE, NCR_CLIE, NGA_CLIE, CGA_CLIE, KGA_CLIE, RIS_CLIE, DPL_CLIE, SOC_CLIE, COD_VEND, BMX_CLIE, VMX_CLIE, VAL_CADA_PUNTOS, SAL_PUNTOS, VEA_CLIE, PLA_CLIE, NCO_FV_CLIE, NCO_FVC_CLIE, NCO_EB_CLIE, MAX_FV_CLIE, MAX_EB_CLIE, KIL_CLIE, MAI_ENV_CLIE, RFID_CLIE)
AS  select * from clie where cpr_clie='F' order by ruc_clie;
CREATE VIEW CLIEBODE (CODIGO, NOMBRE, RUC)
AS 
select COD_CLIE,NOM_CLIE,RUC_CLIE from clie 
UNION
SELECT COD_BODE,NOM_BODE,'' AS RUC FROM BODE;
CREATE VIEW CLIEREPE (RUC_CLIE, COD_CLIE, NOM_CLIE)
AS 
SELECT DISTINCT 
clie.ruc_clie, clie.cod_clie ,clie.nom_clie
FROM 
clie
WHERE cpr_clie='C' and
clie.ruc_clie
In ( 
SELECT ruc_clie FROM clie As Tmp GROUP BY ruc_clie HAVING Count(*) > 1)
ORDER BY 
clie.ruc_clie
;
CREATE VIEW COMPRASBASEIVA (SEC_DCTO, NUM_MOVI, CODIGO, TOT_MOVI, PIV_MOVI, VIV_MOVI, TIPO, ATS)
AS   SELECT T.SEC_DCTO,P.NUM_MOPF,P.COD_PAFA, P.VAL_MOPF,P.PIV_MOPF,P.VIV_MOPF,'TL',P.ATS_MOPF FROM MOPF P INNER JOIN TLMM T ON T.NUM_TLMM=P.NUM_TLMM  Union  SELECT D.SEC_DCTO,M.NUM_MOVI,M.COD_ITEM, M.TOT_MOVI,M.PIV_MOVI,M.VIV_MOVI,D.TIP_DCTO,M.COD_IMPU FROM MOVI M INNER JOIN DCTO D ON M.SEC_DCTO=D.SEC_DCTO  WHERE  D.TIP_DCTO='FC' OR D.TIP_DCTO='LC';
CREATE VIEW CONCILIA5 (NUM_COMP, NUM_MOVI, NUMMCOMP, COD_CUEN, DET_COMP, DET_MOVI, VAL_MOVI, REF_MOVI, NUM_COBA, CHE_COMP, FEC_COMP, TIPO)
AS  
select C.NUM_COMP,M.NUM_MOVI,C.NUMMCOMP,M.COD_CUEN,C.DET_COMP,M.DET_MOVI,M.VAL_MOVI,M.REF_MOVI,M.NUM_COBA,c.che_comp,c.fap_comp, 'MO' from COMP C,CON_MOVI M 
 Where c.nummcomp = m.nummcomp 
 Union 
 select '',b.NUM_BANC_PEND,0,b.COD_CUEN,b.DET_BANC_PEND,b.DET_BANC_PEND,b.VAL_BANC_PEND,b.REF_BANC_PEND,b.NUM_COBA,b.che_banc_pend,b.fec_banc_pend,'PE' from banc_pend b 
ORDER BY 1;
CREATE VIEW DCTOV (SEC_DCTO, TIP_DCTO, NUM_DCTO, FEC_DCTO, COD_CLIE, TNI_DCTO, TSI_DCTO, DSC_DCTO, IVA_DCTO, COD_BODE, NCU_DCTO, DCU_DCTO, DET_DCTO, OBS_DCTO, RUC_DCTO, DIR_DCTO, COD_VEND, COD_PAGO, NDO_DCTO, BPA_DCTO, PAG_DCTO, COL_DCTO, NUM_LIQV)
AS 
select SEC_DCTO,
    TIP_DCTO,
    NUM_DCTO,
    FEC_DCTO,
    COD_CLIE,
    TNI_DCTO,
    TSI_DCTO,
    DSC_DCTO,
    IVA_DCTO,
    COD_BODE,
    NCU_DCTO,
    DCU_DCTO,
    DET_DCTO,
    OBS_DCTO,
    RUC_DCTO,
    DIR_DCTO,
    COD_VEND,
    COD_PAGO,
    NDO_DCTO,
    BPA_DCTO,
    PAG_DCTO,
    COL_DCTO,
    NUM_LIQV from dcto
where (trim(col_dcto)='' or col_dcto=' 1') and num_liqv=0
;;
CREATE VIEW MANGV (COD_MANG, NOM_MANG, NUM_MANG, COD_PUNT, NOM_PUNT, VEN_PUNT, COD_ITEM, NOM_ITEM, COD_TANQ, PRE_ITEM, ELI_MANG, PIV_ITEM)
AS   select cod_mang,nom_mang,num_mang,punt.cod_punt,nom_punt,ven_punt,item.cod_item,nom_item,tanq.cod_tanq,item.vea_item,mang.eli_mang,item.piv_item From mang, punt, tanq, Item  Where Left(mang.cod_punt, 2) = punt.cod_punt And mang.cod_tanq = tanq.cod_tanq And tanq.cod_item = Item.cod_item ;
CREATE VIEW MOLI_VISTA (NUMERO, CODPUNT, CODPAGO, CODGRUP, INGRESO, EGRESO, IVA, TIPO)
AS  select num_LIQU,cod_punt,cod_pago,'',sum(iif(cod_punt='FV' OR cod_punt='EB',cast(im1_punt as numeric(13,2))+cast(im2_punt as numeric(13,2))+cast(im3_punt as numeric(13,2))+cast(im4_punt as numeric(13,2)),0)),sum(iif(cod_punt='DV' OR cod_punt='IB',cast(im1_punt as numeric(13,2))+cast(im2_punt as numeric(13,2))+cast(im3_punt as numeric(13,2))+cast(im4_punt as numeric(13,2)),0)),0 as IVA,'1' as tipo  from moli where  cod_punt>'99' and cod_mang<>'' group by 1,2,3,4 Union  select num_LIQU,'','',substring(MANGV.cod_item from 1 for 4), sum(iif(MOLI.cod_punt='DV' OR MOLI.cod_punt='IB',cast(im1_punt as numeric(13,2))+cast(im2_punt as numeric(13,2))+cast(im3_punt as numeric(13,2))+cast(im4_punt as numeric(13,2)),0)), sum(iif(MOLI.cod_punt='FV' OR MOLI.cod_punt='EB',cast(im1_punt as numeric(13,2))+cast(im2_punt as numeric(13,2))+cast(im3_punt as numeric(13,2))+cast(im4_punt as numeric(13,2)),0)), SUM(iva_moli) as IVA,'2' as Tipo from moli,mangV where moli.cod_mang=mangV.COD_MANG and MOLI.cod_punt>'99' and moli.cod_mang<>'' group by 1,2,3,4  ORDER BY 5,2;
CREATE VIEW STOCK (COD_ITEM, ING_ITEM, EGR_ITEM, FEC_ITEM, COD_GRUP)
AS 
select cod_item,sum(iif(can_movi>0,can_movi,0)) as Ingreso,sum(iif(can_movi<0,(can_movi*-1),0)) as Egreso,
cast(extract(month from a.fec_dcto) || '/' || extract(day from a.fec_dcto) || '/' || extract(year from a.fec_dcto) as date) as fec_item,
substring(cod_ITEM from 1 for 2) as Cod_Grup from movi
inner join dcto a on a.sec_dcto =movi.sec_dcto group  by cod_item,cast(extract(month from a.fec_dcto) || '/' || extract(day from a.fec_dcto) || '/' || extract(year from a.fec_dcto) as date),COD_GRUP
order by 1,4
;
CREATE VIEW VDESP (COD_CLIE, TID_CLIE, RUC_CLIE, RAZ_CLIE, APE_CLIE, NOM_CLIE, DCA_CLIE, DNU_CLIE, DCI_CLIE, DPR_CLIE, CPR_CLIE, TE1_CLIE, TE2_CLIE, FAX_CLIE, COR_CLIE, FIN_CLIE, CRE_CLIE, EST_CLIE, COD_ZONA, CAN_CLIE, QUI_CLIE, CST_CLIE, REL_CLIE, CUP_CLIE, MAI_CLIE, ARC_CLIE, NCR_CLIE, NGA_CLIE, CGA_CLIE, KGA_CLIE, RIS_CLIE, DPL_CLIE, SOC_CLIE, COD_VEND, BMX_CLIE, VMX_CLIE, VAL_CADA_PUNTOS, SAL_PUNTOS, VEA_CLIE, PLA_CLIE, NCO_FV_CLIE, NCO_FVC_CLIE, NCO_EB_CLIE, MAX_FV_CLIE, MAX_EB_CLIE, KIL_CLIE, MAI_ENV_CLIE, RFID_CLIE)
AS  select * from clie where cpr_clie='D' order by cod_clie;
CREATE VIEW VENDESP (CODIGO, NOMBRE)
AS      
select COD_VEND,NOM_VEND FROM VEND 
UNION 
select COD_CLIE,NOM_CLIE FROM CLIE where cpr_clie='D' order by 1;
/******************* EXCEPTIONS *******************/

CREATE EXCEPTION BAN_CART_ELI
'REGISTRO QUE INTENTA ELIMINAR ESTA CONCILIADO EN MODULO';
CREATE EXCEPTION BAN_CART_MOD
'REGISTRO QUE INTENTA MODIFICAR ESTA CONCILIADO EN MODULO';
CREATE EXCEPTION BAN_CONS_ELI
'REGISTRO QUE INTENTA ELIMINAR ESTA CONCILIADO';
CREATE EXCEPTION BAN_CONS_MOD
'REGISTRO QUE INTENTA MODIFICAR ESTA CONCILIADO';
CREATE EXCEPTION REG_BLOQ_AGR
'REGISTRO QUE INTENTA AGREGAR ESTA BLOQUEADO';
CREATE EXCEPTION REG_BLOQ_ELI
'REGISTRO QUE INTENTA ELIMINAR ESTA BLOQUEADO';
CREATE EXCEPTION REG_BLOQ_MOD
'REGISTRO QUE INTENTA MODIFICAR ESTA BLOQUEADO';
/******************** TRIGGERS ********************/

SET TERM ^ ;
CREATE TRIGGER ACT_ACFI_C FOR DEP_MOVI ACTIVE
AFTER INSERT POSITION 0
As begin update acfi set dac_acfi = dac_acfi+new.dep_movi,fud_acfi=new.fud_acfi where acfi.cod_acfi = new.cod_acfi ; end^
SET TERM ; ^
SET TERM ^ ;
CREATE TRIGGER ACT_CRED_C FOR CRED_ADDE ACTIVE
AFTER INSERT POSITION 0
As begin update cred_cabe set too_cabe = too_cabe+new.val_adde where cred_cabe.num_cabe = new.num_cabe ; end^
SET TERM ; ^
SET TERM ^ ;
CREATE TRIGGER ACT_CUOT_C FOR CUOT ACTIVE
AFTER UPDATE POSITION 0
As  begin  update dcto set pag_dcto = (pag_dcto-old.pag_cuot+new.pag_cuot),bpa_dcto='0' where dcto.sec_dcto = new.sec_dcto  and dcto.tip_dcto<>'DV' and dcto.tip_dcto<>'DC' ;  update dcto set pag_dcto = (pag_dcto-(old.pag_cuot*-1)+(new.pag_cuot*-1)),bpa_dcto='0' where dcto.sec_dcto = new.sec_dcto  and (dcto.tip_dcto='DV' or dcto.tip_dcto='DC') ;  update dcto set bpa_dcto='1' where ((dcto.sec_dcto = new.sec_dcto) and (dcto.pag_dcto = (dcto.tni_dcto+dcto.tsi_dcto-dcto.dsc_dcto+dcto.num_liqv +dcto.iva_dcto))) ; end^
SET TERM ; ^
SET TERM ^ ;
CREATE TRIGGER ACT_PGDE_C FOR PGDE ACTIVE
BEFORE INSERT POSITION 0
As begin update cuot set pag_cuot = pag_cuot+new.val_pgde,bpa_pago='0' where cuot.num_cuot = new.num_cuot ; update cuot set bpa_pago='1' where ((cuot.num_cuot = new.num_cuot) and (cast(Cuot.pag_cuot as decimal(14,2)) = cast(Cuot.val_cuot as decimal(14,2))))  ; End^
SET TERM ; ^
SET TERM ^ ;
CREATE TRIGGER ARCH_CON_BI FOR ARCH_CON ACTIVE
BEFORE INSERT POSITION 0
as  begin  if (new.NUM_ARCH_CON is null) then      new.NUM_ARCH_CON = gen_id(GEN_ARCH_CON_ID,1); End^
SET TERM ; ^
SET TERM ^ ;
CREATE TRIGGER ARCH_CUA_BI FOR ARCH_CUA ACTIVE
BEFORE INSERT POSITION 0
as  begin  if (new.NUM_ARCH_CUA is null) then      new.NUM_ARCH_CUA = gen_id(GEN_ARCH_CUA_ID,1); End^
SET TERM ; ^
SET TERM ^ ;
CREATE TRIGGER ARCH_MOV_BI FOR ARCH_MOV ACTIVE
BEFORE INSERT POSITION 0
as  begin  if (new.NUM_ARMO is null) then  new.NUM_ARMO = gen_id(GEN_ARCH_MOV_ID,1); End^
SET TERM ; ^
SET TERM ^ ;
CREATE TRIGGER BANC_BU0 FOR BANC ACTIVE
BEFORE UPDATE POSITION 0
AS
begin
    insert INTO log (usu_log,tab_log,dir_log,fec_log,acc_log) values
    (CURRENT_USER,'BANC',(select A.MON$REMOTE_ADDRESS FROM MON$ATTACHMENTS A WHERE A.MON$ATTACHMENT_ID = CURRENT_CONNECTION),
    CURRENT_TIMESTAMP,'2');
      /* Trigger text */
end^
SET TERM ; ^
SET TERM ^ ;
CREATE TRIGGER BANC_PEND_BD7 FOR BANC_PEND ACTIVE
BEFORE DELETE POSITION 7
AS 
  declare variable vnummocb integer; 
     begin 
         select NUM_MOCB from mocb WHERE MOCB.MOD_MOCB='PE' and OLD.num_banc_pend=MOCB.num_movi INTO :vnummocb; 
         if (:vnummocb is not null) then 
         begin 
             exception BAN_CONS_ELI; 
         End 
     End^
SET TERM ; ^
SET TERM ^ ;
CREATE TRIGGER BANC_PEND_BI0 FOR BANC_PEND ACTIVE
BEFORE INSERT POSITION 0
as begin if (new.num_BANC_PEND is null or new.num_BANC_PEND=0) then new.num_BANC_PEND = gen_id(gen_banc_pend_id,1); End^
SET TERM ; ^
SET TERM ^ ;
CREATE TRIGGER BANC_PEND_BU7 FOR BANC_PEND ACTIVE
BEFORE UPDATE POSITION 7
AS 
  declare variable vnummocb integer; 
     begin 
         select NUM_MOCB from mocb WHERE MOCB.MOD_MOCB='PE' and MOCB.EST_MOCB>0 and OLD.num_banc_pend=MOCB.num_movi INTO :vnummocb; 
         if (:vnummocb is not null) then 
         begin 
             exception BAN_CONS_MOD; 
         End 
     End^
SET TERM ; ^
SET TERM ^ ;
CREATE TRIGGER CHEQ_BI FOR CHEQ ACTIVE
BEFORE INSERT POSITION 0
as
begin
  if (new.num_cheq is null) then
    new.num_cheq = gen_id(gen_cheq_id,1);
end^
SET TERM ; ^
SET TERM ^ ;
CREATE TRIGGER CHEQ_MOV_BD0 FOR CHEQ_MOV ACTIVE
BEFORE DELETE POSITION 0
AS
begin
  /* Trigger text */
  update cheq set cod_banc = old.ori_cheq where old.num_cheq = cheq.num_cheq;
end^
SET TERM ; ^
SET TERM ^ ;
CREATE TRIGGER CHEQ_MOV_BI FOR CHEQ_MOV ACTIVE
BEFORE INSERT POSITION 0
as
begin
  if (new.num_chmo is null) then
    new.num_chmo = gen_id(gen_cheq_mov_id,1);
    update cheq set cod_banc = new.cod_banc , fec_chmo = new.fec_chmo where new.num_cheq = cheq.num_cheq;
end^
SET TERM ; ^
SET TERM ^ ;
CREATE TRIGGER CICA_BI0 FOR CICA ACTIVE
BEFORE INSERT POSITION 0
as  begin  if (new.num_cica is null) then  new.num_cica = gen_id(gen_cica_id,1); End^
SET TERM ; ^
SET TERM ^ ;
CREATE TRIGGER CLIE_BI0 FOR CLIE ACTIVE
BEFORE INSERT POSITION 0
AS
declare variable Acum Char(6);
declare variable AcumL Char(6);
declare variable vNum Char(1);
declare variable vNumA Char(7);

declare variable AcumN Char(7);
declare variable AcumLN Char(7);
declare variable vNumAN Char(9);

begin
   if ((new.cst_clie is null or new.cst_clie='0000000' or trim(new.cst_clie)=''  ) and (new.cpr_clie='C')) then
   begin
      Acum= lpad ((select Gen_Id(gen_cst_clie,1) from rdb$database),6,'0');
      AcumL = cast(Acum as char(6));
      vNumA= '7' || AcumL;
      new.cst_clie=vNumA;
   end
   if ((new.cod_clie is null or new.cod_clie='000000000' or trim(new.cod_clie)=''  ) and (new.cpr_clie='C')) then
   begin
      AcumN= lpad ((select Gen_Id(gen_clie_cc,1) from rdb$database),7,'0');
      AcumLN = cast(AcumN as char(7));
      vNumAN= 'CC' || AcumLN;
      new.cod_clie=vNumAN;
   end
  /* Trigger text */
end^
SET TERM ; ^
SET TERM ^ ;
CREATE TRIGGER CLIE_BU0 FOR CLIE ACTIVE
BEFORE UPDATE POSITION 0
AS
declare variable Acum Char(6);
declare variable AcumL Char(6);
declare variable vNum Char(1);
declare variable vNumA Char(7);
begin
   if ((new.cst_clie is null or new.cst_clie='0000000' or trim(new.cst_clie)=''  ) and (new.cpr_clie='C')) then
   begin
      Acum= lpad ((select Gen_Id(gen_cst_clie,1) from rdb$database),6,'0');
      AcumL = cast(Acum as char(6));
      vNumA= '7' || AcumL;
      new.cst_clie=vNumA;
   end
  /* Trigger text */
end^
SET TERM ; ^
SET TERM ^ ;
CREATE TRIGGER CLIE_CUMO_BI FOR CLIE_CUMO ACTIVE
BEFORE INSERT POSITION 0
as  begin  if (new.num_CUMO is null) then  new.num_CUMO = gen_id(gen_CLIE_CUMO_id,1); End^
SET TERM ; ^
SET TERM ^ ;
CREATE TRIGGER CLIE_CUPO_BI FOR CLIE_CUPO ACTIVE
BEFORE INSERT POSITION 0
as  begin  if (new.num_CLCU is null) then  new.num_CLCU = gen_id(gen_CLIE_CUPO_id,1); End^
SET TERM ; ^
SET TERM ^ ;
CREATE TRIGGER CLIE_DEST_BI FOR CLIE_DEST ACTIVE
BEFORE INSERT POSITION 0
as  begin  if (new.num_clie_dest is null or new.num_clie_dest=0 ) then  new.num_clie_dest = gen_id(gen_num_clie_dest_id,1); End^
SET TERM ; ^
SET TERM ^ ;
CREATE TRIGGER CLIE_PRESET_BI FOR CLIE_PRESET ACTIVE
BEFORE INSERT POSITION 0
as  begin  if (new.num_preset is null) then  new.num_preset = gen_id(gen_CLIE_PRESET_id,1); End^
SET TERM ; ^
SET TERM ^ ;
CREATE TRIGGER COBA_BI FOR COBA ACTIVE
BEFORE INSERT POSITION 0
as
begin
  if (new.num_coba is null) then
    new.num_coba = gen_id(gen_coba_id,1);
end^
SET TERM ; ^
SET TERM ^ ;
CREATE TRIGGER CONS_BI FOR CONS ACTIVE
BEFORE INSERT POSITION 0
as  
begin   
if (new.num_cons is null) then  
begin   
new.num_cons = gen_id(gen_cons_id,1) ;
end
end^
SET TERM ; ^
SET TERM ^ ;
CREATE TRIGGER CONS_MOV_BI FOR CONS_MOV ACTIVE
BEFORE INSERT POSITION 0
as  begin   if (new.num_como is null) then     new.num_como = gen_id(gen_cons_mov_id,1); end^
SET TERM ; ^
SET TERM ^ ;
CREATE TRIGGER CON_MOVI_BD0 FOR CON_MOVI ACTIVE
BEFORE DELETE POSITION 0
AS
declare variable vCodPlan char(25);
declare variable i integer;
declare variable vMes integer;
declare variable vCodCuen char(25);
declare variable iMax integer;
declare variable vNumNive char(1);
begin
  select pla_empr from empr where cod_empr='001' into :vCodPlan;
  select extract(month from fap_comp) from comp where nummcomp=old.nummcomp into :vMes;
  vNumNive = '1';
  iMax = char_length(trim(old.cod_cuen));
  i = 1;
  while (i < (iMax+1)) do
    begin
        if ((vNumNive<>substring(vCodPlan from i for 1)) or (i=iMax)) then
        begin
          if (i=iMax) then
          begin
            vCodCuen=old.cod_cuen;
          end
          else
          begin
           vCodCuen=substring(old.cod_cuen from 1 for (i-1));
          end
          if (vMes=1) then update cuen set db_01=db_01-old.val_movi where old.val_movi>0 and cod_cuen=:vCodCuen;
          if (vMes=2) then update cuen set db_02=db_02-old.val_movi where old.val_movi>0 and cod_cuen=:vCodCuen;
          if (vMes=3) then update cuen set db_03=db_03-old.val_movi where old.val_movi>0 and cod_cuen=:vCodCuen;
          if (vMes=4) then update cuen set db_04=db_04-old.val_movi where old.val_movi>0 and cod_cuen=:vCodCuen;
          if (vMes=5) then update cuen set db_05=db_05-old.val_movi where old.val_movi>0 and cod_cuen=:vCodCuen;
          if (vMes=6) then update cuen set db_06=db_06-old.val_movi where old.val_movi>0 and cod_cuen=:vCodCuen;
          if (vMes=7) then update cuen set db_07=db_07-old.val_movi where old.val_movi>0 and cod_cuen=:vCodCuen;
          if (vMes=8) then update cuen set db_08=db_08-old.val_movi where old.val_movi>0 and cod_cuen=:vCodCuen;
          if (vMes=9) then update cuen set db_09=db_09-old.val_movi where old.val_movi>0 and cod_cuen=:vCodCuen;
          if (vMes=10) then update cuen set db_10=db_10-old.val_movi where old.val_movi>0 and cod_cuen=:vCodCuen;
          if (vMes=11) then update cuen set db_11=db_11-old.val_movi where old.val_movi>0 and cod_cuen=:vCodCuen;
          if (vMes=12) then update cuen set db_12=db_12-old.val_movi where old.val_movi>0 and cod_cuen=:vCodCuen;
          if (vMes=1) then update cuen set cr_01=cr_01-(old.val_movi*-1) where old.val_movi<0 and cod_cuen=:vCodCuen;
          if (vMes=2) then update cuen set cr_02=cr_02-(old.val_movi*-1) where old.val_movi<0 and cod_cuen=:vCodCuen;
          if (vMes=3) then update cuen set cr_03=cr_03-(old.val_movi*-1) where old.val_movi<0 and cod_cuen=:vCodCuen;
          if (vMes=4) then update cuen set cr_04=cr_04-(old.val_movi*-1) where old.val_movi<0 and cod_cuen=:vCodCuen;
          if (vMes=5) then update cuen set cr_05=cr_05-(old.val_movi*-1) where old.val_movi<0 and cod_cuen=:vCodCuen;
          if (vMes=6) then update cuen set cr_06=cr_06-(old.val_movi*-1) where old.val_movi<0 and cod_cuen=:vCodCuen;
          if (vMes=7) then update cuen set cr_07=cr_07-(old.val_movi*-1) where old.val_movi<0 and cod_cuen=:vCodCuen;
          if (vMes=8) then update cuen set cr_08=cr_08-(old.val_movi*-1) where old.val_movi<0 and cod_cuen=:vCodCuen;
          if (vMes=9) then update cuen set cr_09=cr_09-(old.val_movi*-1) where old.val_movi<0 and cod_cuen=:vCodCuen;
          if (vMes=10) then update cuen set cr_10=cr_10-(old.val_movi*-1) where old.val_movi<0 and cod_cuen=:vCodCuen;
          if (vMes=11) then update cuen set cr_11=cr_11-(old.val_movi*-1) where old.val_movi<0 and cod_cuen=:vCodCuen;
          if (vMes=12) then update cuen set cr_12=cr_12-(old.val_movi*-1) where old.val_movi<0 and cod_cuen=:vCodCuen;
          vNumNive=substring(vCodPlan from i for 1);
        end
        i = i + 1;
  end

  /* Trigger text */
end^
SET TERM ; ^
SET TERM ^ ;
CREATE TRIGGER CON_MOVI_BD7 FOR CON_MOVI ACTIVE
BEFORE DELETE POSITION 7
AS 
  declare variable vnummocb integer; 
     begin 
         select NUM_MOCB from mocb WHERE MOCB.MOD_MOCB='MO' and MOCB.EST_MOCB>0 and OLD.num_movi=MOCB.num_movi INTO :vnummocb; 
         if (:vnummocb is not null) then 
         begin 
             exception BAN_CONS_ELI; 
         End 
     End^
SET TERM ; ^
SET TERM ^ ;
CREATE TRIGGER CON_MOVI_BI FOR CON_MOVI ACTIVE
BEFORE INSERT POSITION 0
AS
declare variable vCodPlan char(25);
declare variable i integer;
declare variable vMes integer;
declare variable vCodCuen char(25);
declare variable iMax integer;
declare variable vNumNive char(1);
begin
  select pla_empr from empr where cod_empr='001' into :vCodPlan;
  select extract(month from fap_comp) from comp where nummcomp=new.nummcomp into :vMes;
  vNumNive = '1';
  iMax = char_length(trim(new.cod_cuen));
  i = 1;
  while (i < (iMax+1)) do
    begin
        if ((vNumNive<>substring(vCodPlan from i for 1)) or (i=iMax)) then
        begin
          if (i=iMax) then
          begin
            vCodCuen=new.cod_cuen;
          end
          else
          begin
           vCodCuen=substring(new.cod_cuen from 1 for (i-1));
          end
          if (vMes=1) then update cuen set db_01=db_01+new.val_movi where new.val_movi>0 and cod_cuen=:vCodCuen;
          if (vMes=2) then update cuen set db_02=db_02+new.val_movi where new.val_movi>0 and cod_cuen=:vCodCuen;
          if (vMes=3) then update cuen set db_03=db_03+new.val_movi where new.val_movi>0 and cod_cuen=:vCodCuen;
          if (vMes=4) then update cuen set db_04=db_04+new.val_movi where new.val_movi>0 and cod_cuen=:vCodCuen;
          if (vMes=5) then update cuen set db_05=db_05+new.val_movi where new.val_movi>0 and cod_cuen=:vCodCuen;
          if (vMes=6) then update cuen set db_06=db_06+new.val_movi where new.val_movi>0 and cod_cuen=:vCodCuen;
          if (vMes=7) then update cuen set db_07=db_07+new.val_movi where new.val_movi>0 and cod_cuen=:vCodCuen;
          if (vMes=8) then update cuen set db_08=db_08+new.val_movi where new.val_movi>0 and cod_cuen=:vCodCuen;
          if (vMes=9) then update cuen set db_09=db_09+new.val_movi where new.val_movi>0 and cod_cuen=:vCodCuen;
          if (vMes=10) then update cuen set db_10=db_10+new.val_movi where new.val_movi>0 and cod_cuen=:vCodCuen;
          if (vMes=11) then update cuen set db_11=db_11+new.val_movi where new.val_movi>0 and cod_cuen=:vCodCuen;
          if (vMes=12) then update cuen set db_12=db_12+new.val_movi where new.val_movi>0 and cod_cuen=:vCodCuen;
          if (vMes=1) then update cuen set cr_01=cr_01+(new.val_movi*-1) where new.val_movi<0 and cod_cuen=:vCodCuen;
          if (vMes=2) then update cuen set cr_02=cr_02+(new.val_movi*-1) where new.val_movi<0 and cod_cuen=:vCodCuen;
          if (vMes=3) then update cuen set cr_03=cr_03+(new.val_movi*-1) where new.val_movi<0 and cod_cuen=:vCodCuen;
          if (vMes=4) then update cuen set cr_04=cr_04+(new.val_movi*-1) where new.val_movi<0 and cod_cuen=:vCodCuen;
          if (vMes=5) then update cuen set cr_05=cr_05+(new.val_movi*-1) where new.val_movi<0 and cod_cuen=:vCodCuen;
          if (vMes=6) then update cuen set cr_06=cr_06+(new.val_movi*-1) where new.val_movi<0 and cod_cuen=:vCodCuen;
          if (vMes=7) then update cuen set cr_07=cr_07+(new.val_movi*-1) where new.val_movi<0 and cod_cuen=:vCodCuen;
          if (vMes=8) then update cuen set cr_08=cr_08+(new.val_movi*-1) where new.val_movi<0 and cod_cuen=:vCodCuen;
          if (vMes=9) then update cuen set cr_09=cr_09+(new.val_movi*-1) where new.val_movi<0 and cod_cuen=:vCodCuen;
          if (vMes=10) then update cuen set cr_10=cr_10+(new.val_movi*-1) where new.val_movi<0 and cod_cuen=:vCodCuen;
          if (vMes=11) then update cuen set cr_11=cr_11+(new.val_movi*-1) where new.val_movi<0 and cod_cuen=:vCodCuen;
          if (vMes=12) then update cuen set cr_12=cr_12+(new.val_movi*-1) where new.val_movi<0 and cod_cuen=:vCodCuen;
          vNumNive=substring(vCodPlan from i for 1);
        end
        i = i + 1;
  end
  /* Trigger text */
  if (new.num_movi is null) then
    new.num_movi = gen_id(gen_con_movi_id,1);
end^
SET TERM ; ^
SET TERM ^ ;
CREATE TRIGGER CON_MOVI_BU7 FOR CON_MOVI ACTIVE
BEFORE UPDATE POSITION 7
AS 
  declare variable vnummocb integer; 
     begin 
         select NUM_MOCB from mocb WHERE MOCB.MOD_MOCB='MO' and MOCB.EST_MOCB>0 and OLD.num_movi=MOCB.num_movi INTO :vnummocb; 
         if (:vnummocb is not null) then 
         begin 
             exception BAN_CONS_MOD; 
         End 
     End^
SET TERM ; ^
SET TERM ^ ;
CREATE TRIGGER CON_MOVI_PGDE_BI FOR CON_MOVI_PGDE ACTIVE
BEFORE INSERT POSITION 0
as  begin   if (new.NUM_CON_MOVI_PGDE is null or new.NUM_CON_MOVI_PGDE=0 ) then     new.NUM_CON_MOVI_PGDE = gen_id(CON_MOVI_PGDE_ID,1); end^
SET TERM ; ^
SET TERM ^ ;
CREATE TRIGGER CUOT_BIU2 FOR CUOT ACTIVE
BEFORE INSERT OR UPDATE POSITION 1
AS      BEGIN      if (new.INT_CUOT is null) then      new.INT_CUOT =0;      if (new.OTR_CUOT is null) then      new.OTR_CUOT =0;     if (new.PAG_CUOT is null) then     new.PAG_CUOT =0;     END^
SET TERM ; ^
SET TERM ^ ;
CREATE TRIGGER DCTO_BD0 FOR DCTO ACTIVE
BEFORE DELETE POSITION 0
AS
begin
  /* Trigger text */
  DELETE FROM MOVI WHERE OLD.sec_dcto=MOVI.sec_dcto;
  DELETE FROM cuot WHERE OLD.SEC_DCTO=CUOT.SEC_DCTO;
end^
SET TERM ; ^
SET TERM ^ ;
CREATE TRIGGER DCTO_BD4 FOR DCTO ACTIVE
BEFORE DELETE POSITION 4
AS 
 declare variable vNumLock integer; 
 declare variable vCodUsua char(10); 
 begin 
  select first 1 cod_usua from sess where est_sess='1' and ses_sess=CURRENT_CONNECTION into :vcodusua;
  if (:vcodusua is null) then 
   vcodusua=current_user; 
   for select b.ELI_SYSLOCKMOVI from SYSLOCK a,SYSLOCKMOVI b,SYSMODUTIPO c 
 Where a.NUM_SysLock = b.NUM_SysLock 
 and b.ELI_SYSLOCKMOVI='1' and b.NUM_SYSMODUTIPO=c.NUM_SYSMODUTIPO 
  and  c.DOC_SYSMODUTIPO= old.TIP_DCTO
  and  C.NUM_SYSMODU= 2
  and old.FEC_DCTO >=a.FDE_SYSLOCK 
  and old.FEC_DCTO <=a.FHA_SYSLOCK 
  and ((a.COD_USUASYSLOCK='0000000000' and a.ADM_SYSLOCK='0') 
  or (trim(:vCodUsua)<>'Admin' and a.ADM_SYSLOCK='1') 
  or (trim(a.COD_USUASYSLOCK)=trim(:vCodUsua)) 
  ) into :vNumLock do 
     begin 
         if (not(:vNumLock is null) or :vNumLock>0) then 
         begin 
             exception REG_BLOQ_ELI; 
         End 
     End 
 End^
SET TERM ; ^
SET TERM ^ ;
CREATE TRIGGER DCTO_BDA0 FOR DCTO ACTIVE
BEFORE DELETE POSITION 5
AS 
 declare variable vnumtram integer; 
 declare variable vcodusua char(10); 
 begin 
 select first 1 cod_usua from sess where est_sess='1' and ses_sess=CURRENT_CONNECTION into :vcodusua;
 if (:vcodusua is null) then 
   vcodusua=current_user;  

 select first 1 num_tram from tram where (trim(tram.cod_usua)=trim(:vcodusua) and trim(tram.nip_tram)=trim(RDB$GET_CONTEXT('SYSTEM' , 'CLIENT_ADDRESS')) and tram.est_tram='0')  into :vnumtram; 
 if (:vnumtram is null or :vnumtram<=0 ) then 
 begin 
    insert into tram (cod_usua,nip_tram,est_tram,fec_tram,tip_tram) values (:vcodusua, RDB$GET_CONTEXT('SYSTEM' , 'CLIENT_ADDRESS'),'0',current_timestamp,'E');
 end 
 INSERT INTO XDCTO( SEC_DCTO , TIP_DCTO , NUM_DCTO , FEC_DCTO , COD_CLIE , TNI_DCTO , 
TSI_DCTO , DSC_DCTO , IVA_DCTO , COD_BODE , NCU_DCTO , DCU_DCTO , 
DET_DCTO , OBS_DCTO , RUC_DCTO , DIR_DCTO , COD_VEND , COD_PAGO , 
NDO_DCTO , BPA_DCTO , PAG_DCTO , COL_DCTO , NUMMCOMP , NUM_LIQV , 
GUI_DCTO , FGU_DCTO , COD_ACTI , ANE_DCTO , AUT_DCTO , ESE_DCTO , 
DEE_DCTO , CLE_DCTO , SER_DCTO , PLA_DCTO , ORD_DCTO , NUM_TURN , 
VAC_DCTO , COD_CHOF , NUM_CONS , SUB_DCTO , TER_DCTO , FUL_CABE , 
TAZ_CRED , PCM_DCTO , VCM_DCTO , PCMR_DCTO , VCMR_DCTO , DNI_DCTO , 
DSI_DCTO , IMP_DCTO , ESG_DCTO , DEG_DCTO , CLG_DCTO , AUG_DCTO , 
PIV_DCTO , COD_GRUP , COD_XUSUA , FEC_XDCTO , NIP_XDCTO , COD_MANG , 
NUM_TICK_MOV , NUM_VAUC , DSCA0_DCTO , DSCA_DCTO , PLG_DCTO ,  MEL_XDCTO ) values ( OLD.SEC_DCTO , OLD.TIP_DCTO , OLD.NUM_DCTO , OLD.FEC_DCTO , OLD.COD_CLIE , OLD.TNI_DCTO , 
OLD.TSI_DCTO , OLD.DSC_DCTO , OLD.IVA_DCTO , OLD.COD_BODE , OLD.NCU_DCTO , OLD.DCU_DCTO , 
OLD.DET_DCTO , OLD.OBS_DCTO , OLD.RUC_DCTO , OLD.DIR_DCTO , OLD.COD_VEND , OLD.COD_PAGO , 
OLD.NDO_DCTO , OLD.BPA_DCTO , OLD.PAG_DCTO , OLD.COL_DCTO , OLD.NUMMCOMP , OLD.NUM_LIQV , 
OLD.GUI_DCTO , OLD.FGU_DCTO , OLD.COD_ACTI , OLD.ANE_DCTO , OLD.AUT_DCTO , OLD.ESE_DCTO , 
OLD.DEE_DCTO , OLD.CLE_DCTO , OLD.SER_DCTO , OLD.PLA_DCTO , OLD.ORD_DCTO , OLD.NUM_TURN , 
OLD.VAC_DCTO , OLD.COD_CHOF , OLD.NUM_CONS , OLD.SUB_DCTO , OLD.TER_DCTO , OLD.FUL_CABE , 
OLD.TAZ_CRED , OLD.PCM_DCTO , OLD.VCM_DCTO , OLD.PCMR_DCTO , OLD.VCMR_DCTO , OLD.DNI_DCTO , 
OLD.DSI_DCTO , OLD.IMP_DCTO , OLD.ESG_DCTO , OLD.DEG_DCTO , OLD.CLG_DCTO , OLD.AUG_DCTO , 
OLD.PIV_DCTO , OLD.COD_GRUP ,  :vcodusua , current_timestamp , RDB$GET_CONTEXT('SYSTEM' , 'CLIENT_ADDRESS') , OLD.COD_MANG , 
OLD.NUM_TICK_MOV , OLD.NUM_VAUC , OLD.DSCA0_DCTO , OLD.DSCA_DCTO , OLD.PLG_DCTO , 'E');

 end^
SET TERM ; ^
SET TERM ^ ;
CREATE TRIGGER DCTO_BI4 FOR DCTO ACTIVE
BEFORE INSERT POSITION 4
AS 
 declare variable vNumLock integer; 
 declare variable vCodUsua char(10); 
 begin 
  select first 1 cod_usua from sess where est_sess='1' and ses_sess=CURRENT_CONNECTION into :vcodusua;
  if (:vcodusua is null) then 
   vcodusua=current_user; 
   for select b.AGR_SYSLOCKMOVI from SYSLOCK a,SYSLOCKMOVI b,SYSMODUTIPO c 
 Where a.NUM_SysLock = b.NUM_SysLock 
 and b.ELI_SYSLOCKMOVI='1' and b.NUM_SYSMODUTIPO=c.NUM_SYSMODUTIPO 
  and  C.NUM_SYSMODU= 2
  and  c.DOC_SYSMODUTIPO= new.TIP_DCTO
  and new.FEC_DCTO >=a.FDE_SYSLOCK 
  and new.FEC_DCTO <=a.FHA_SYSLOCK 
  and ((a.COD_USUASYSLOCK='0000000000' and a.ADM_SYSLOCK='0') 
  or (trim(:vCodUsua)<>'Admin' and a.ADM_SYSLOCK='1') 
  or (trim(a.COD_USUASYSLOCK)=trim(:vCodUsua)) 
  ) into :vNumLock do 
     begin 
         if (not(:vNumLock is null) or :vNumLock>0) then 
         begin 
             exception REG_BLOQ_AGR; 
         End 
     End 
 End^
SET TERM ; ^
SET TERM ^ ;
CREATE TRIGGER DCTO_BIA0 FOR DCTO ACTIVE
BEFORE INSERT POSITION 5
AS begin 
 NEW.NIP_XDCTO = RDB$GET_CONTEXT('SYSTEM', 'CLIENT_ADDRESS');
 NEW.COD_XUSUA = (select first 1 cod_usua from sess where est_sess='1' and ses_sess=CURRENT_CONNECTION); 
 NEW.FEC_XDCTO = current_timestamp; 
 if (new.COD_XUSUA is null) then
   new.COD_XUSUA= current_user;
 end^
SET TERM ; ^
SET TERM ^ ;
CREATE TRIGGER DCTO_BIA6 FOR DCTO ACTIVE
BEFORE INSERT POSITION 6
AS begin 
if (new.tip_dcto='FV' or new.tip_dcto='EB') then
    new.num_liqv=0;
end^
SET TERM ; ^
SET TERM ^ ;
CREATE TRIGGER DCTO_BU4 FOR DCTO ACTIVE
BEFORE UPDATE POSITION 4
AS 
 declare variable vNumLock integer; 
 declare variable vCodUsua char(10); 
 begin 
  select first 1 cod_usua from sess where est_sess='1' and ses_sess=CURRENT_CONNECTION into :vcodusua;
  if (:vcodusua is null) then 
   vcodusua=current_user; 
   for select b.AGR_SYSLOCKMOVI from SYSLOCK a,SYSLOCKMOVI b,SYSMODUTIPO c 
 Where a.NUM_SysLock = b.NUM_SysLock 
 and b.MOD_SYSLOCKMOVI='1' and b.NUM_SYSMODUTIPO=c.NUM_SYSMODUTIPO 
  and  c.DOC_SYSMODUTIPO= new.TIP_DCTO
  and  C.NUM_SYSMODU= 2
  and new.FEC_DCTO >=a.FDE_SYSLOCK 
  and new.FEC_DCTO <=a.FHA_SYSLOCK 
  and OLD.FEC_DCTO >=a.FDE_SYSLOCK 
  and OLD.FEC_DCTO <=a.FHA_SYSLOCK 
  and ((a.COD_USUASYSLOCK='0000000000' and a.ADM_SYSLOCK='0') 
  or (trim(:vCodUsua)<>'Admin' and a.ADM_SYSLOCK='1') 
  or (trim(a.COD_USUASYSLOCK)=trim(:vCodUsua)) 
  ) into :vNumLock do 
     begin 
         if (not(:vNumLock is null) or :vNumLock>0) then 
         begin 
             exception REG_BLOQ_MOD; 
         End 
     End 
 End^
SET TERM ; ^
SET TERM ^ ;
CREATE TRIGGER ELI_ACFI_C FOR DEP_MOVI ACTIVE
BEFORE DELETE POSITION 0
As begin update acfi set dac_acfi = dac_acfi-old.dep_movi,fud_acfi=old.fad_acfi where acfi.cod_acfi = old.cod_acfi ;end^
SET TERM ; ^
SET TERM ^ ;
CREATE TRIGGER ELI_COMP_C FOR COMP ACTIVE
BEFORE DELETE POSITION 0
As
 begin
 delete from con_movi where con_movi.nummcomp = old.nummcomp ;
 end^
SET TERM ; ^
SET TERM ^ ;
CREATE TRIGGER ELI_CRED_C FOR CRED_ADDE ACTIVE
BEFORE DELETE POSITION 0
As begin update CRED_CABE set too_cabe = too_cabe-old.val_adde where cred_cabe.num_cabe = old.num_cabe ; end^
SET TERM ; ^
SET TERM ^ ;
CREATE TRIGGER ELI_CUOT_C FOR CUOT ACTIVE
BEFORE DELETE POSITION 0
As
begin
update dcto set pag_dcto = pag_dcto-old.pag_cuot,bpa_dcto='0' where dcto.sec_dcto = old.sec_dcto ;
end^
SET TERM ; ^
SET TERM ^ ;
CREATE TRIGGER ELI_DEPR_C FOR DEPR ACTIVE
BEFORE DELETE POSITION 0
As begin delete from dep_movi where dep_movi.num_depr = old.num_depr ;delete from comp where comp.nummcomp = old.nummcomp ;end^
SET TERM ; ^
SET TERM ^ ;
CREATE TRIGGER ELI_PEND FOR PEND ACTIVE
BEFORE DELETE POSITION 0
As begin delete from CON_PEND where pend.num_pend = old.num_pend ; end^
SET TERM ; ^
SET TERM ^ ;
CREATE TRIGGER ELI_PGDE FOR PGCA ACTIVE
BEFORE DELETE POSITION 0
As  begin  delete from pgde where pgde.num_pgca = old.num_pgca ; End^
SET TERM ; ^
SET TERM ^ ;
CREATE TRIGGER ELI_PGDE_C FOR PGDE ACTIVE
BEFORE DELETE POSITION 0
As
begin
    insert into xpgde
    (num_pgca,val_pgde,int_pgde,otr_pgde,num_cuot,num_pgde,det_pgde,fec_xogde,nip_pgde,usr_xpgde)
    VALUES (old.num_pgca,old.val_pgde,old.int_pgde,old.otr_pgde,old.num_cuot,old.num_pgde,old.det_pgde,CURRENT_TIMESTAMP,'',current_user );
    update cuot set pag_cuot = pag_cuot-old.val_pgde,int_cuot=int_cuot-old.int_pgde,bpa_pago='0',otr_cuot=otr_cuot-old.otr_pgde
    where cuot.num_cuot = old.num_cuot;
end^
SET TERM ; ^
SET TERM ^ ;
CREATE TRIGGER EMPL_BI FOR EMPL ACTIVE
BEFORE INSERT POSITION 0
as
begin
  if (new.num_empl is null) then
    new.num_empl = gen_id(gen_empl_id,1);
end^
SET TERM ; ^
SET TERM ^ ;
CREATE TRIGGER ESTA_BI FOR ESTA ACTIVE
BEFORE INSERT POSITION 0
as  begin  if (new.num_esta is null) then  new.num_esta = gen_id(gen_esta_id,1); End^
SET TERM ; ^
SET TERM ^ ;
CREATE TRIGGER ESTR_MOV_BI FOR ESTR_MOV ACTIVE
BEFORE INSERT POSITION 0
as  begin   if (new.num_esmo is null) then     new.num_esmo = gen_id(gen_estr_mov_id,1); end^
SET TERM ; ^
SET TERM ^ ;
CREATE TRIGGER ESTR_VEN_BI FOR ESTR_VEN ACTIVE
BEFORE INSERT POSITION 0
as  begin   if (new.num_esve is null) then     new.num_esve = gen_id(gen_estr_ven_id,1); end^
SET TERM ; ^
SET TERM ^ ;
CREATE TRIGGER F104_BI FOR F104 ACTIVE
BEFORE INSERT POSITION 0
as
begin
  if (new.num_f104 is null) then
    new.num_f104 = gen_id(gen_f104_id,1);
end^
SET TERM ; ^
SET TERM ^ ;
CREATE TRIGGER FACT_BI FOR FACT ACTIVE
BEFORE INSERT POSITION 0
as  begin  if (new.num_FACT is null) then  new.num_FACT = gen_id(gen_FACT_id,1); End^
SET TERM ; ^
SET TERM ^ ;
CREATE TRIGGER IAIRV_BI FOR IAIRV ACTIVE
BEFORE INSERT POSITION 0
as
begin
  if (new.numiairv is null) then
    new.numiairv = gen_id(gen_iairv_id,1);
end^
SET TERM ; ^
SET TERM ^ ;
CREATE TRIGGER IAIR_BI FOR IAIR ACTIVE
BEFORE INSERT POSITION 0
as
begin
  if (new.numiair is null) then
    new.numiair = gen_id(gen_iair_id,1);
end^
SET TERM ; ^
SET TERM ^ ;
CREATE TRIGGER IMPR_BI FOR IMPR ACTIVE
BEFORE INSERT POSITION 0
as
begin
  if (new.num_impr is null) then
    new.num_impr = gen_id(gen_impr_id,1);
end^
SET TERM ; ^
SET TERM ^ ;
CREATE TRIGGER ITEM_BI0 FOR ITEM ACTIVE
BEFORE INSERT POSITION 0
AS
begin
    INSERT INTO ITEMM (COD_ITEM) VALUES (NEW.cod_item);
  /* Trigger text */
end^
SET TERM ; ^
SET TERM ^ ;
CREATE TRIGGER ITEM_BI1 FOR ITEM ACTIVE
BEFORE INSERT OR UPDATE POSITION 1
as begin if (new.CID_ITEM is null or new.CID_ITEM=0) then new.CID_ITEM = gen_id(gen_NUM_ITEM_id,1); End^
SET TERM ; ^
SET TERM ^ ;
CREATE TRIGGER ITEM_MAMO_BI FOR ITEM_MAMO ACTIVE
BEFORE INSERT POSITION 0
as  begin  if (new.num_ITEM_MAMO is null OR new.num_ITEM_MAMO=0 ) then  new.num_ITEM_MAMO = gen_id(ITEM_MAMO_ID,1); End^
SET TERM ; ^
SET TERM ^ ;
CREATE TRIGGER KEY_LOG_202409_BI FOR KEY_LOG_202409 ACTIVE
BEFORE INSERT POSITION 0
as  begin   if (new.num_key is null) then     new.num_key = gen_id(gen_KEY_LOG,1); end^
SET TERM ; ^
SET TERM ^ ;
CREATE TRIGGER KEY_LOG_202410_BI FOR KEY_LOG_202410 ACTIVE
BEFORE INSERT POSITION 0
as  begin   if (new.num_key is null) then     new.num_key = gen_id(gen_KEY_LOG,1); end^
SET TERM ; ^
SET TERM ^ ;
CREATE TRIGGER KEY_LOG_202411_BI FOR KEY_LOG_202411 ACTIVE
BEFORE INSERT POSITION 0
as  begin   if (new.num_key is null) then     new.num_key = gen_id(gen_KEY_LOG,1); end^
SET TERM ; ^
SET TERM ^ ;
CREATE TRIGGER KEY_LOG_202412_BI FOR KEY_LOG_202412 ACTIVE
BEFORE INSERT POSITION 0
as  begin   if (new.num_key is null) then     new.num_key = gen_id(gen_KEY_LOG,1); end^
SET TERM ; ^
SET TERM ^ ;
CREATE TRIGGER KEY_LOG_202501_BI FOR KEY_LOG_202501 ACTIVE
BEFORE INSERT POSITION 0
as  begin   if (new.num_key is null) then     new.num_key = gen_id(gen_KEY_LOG,1); end^
SET TERM ; ^
SET TERM ^ ;
CREATE TRIGGER KEY_LOG_202502_BI FOR KEY_LOG_202502 ACTIVE
BEFORE INSERT POSITION 0
as  begin   if (new.num_key is null) then     new.num_key = gen_id(gen_KEY_LOG,1); end^
SET TERM ; ^
SET TERM ^ ;
CREATE TRIGGER KEY_LOG_202503_BI FOR KEY_LOG_202503 ACTIVE
BEFORE INSERT POSITION 0
as  begin   if (new.num_key is null) then     new.num_key = gen_id(gen_KEY_LOG,1); end^
SET TERM ; ^
SET TERM ^ ;
CREATE TRIGGER KEY_LOG_202504_BI FOR KEY_LOG_202504 ACTIVE
BEFORE INSERT POSITION 0
as  begin   if (new.num_key is null) then     new.num_key = gen_id(gen_KEY_LOG,1); end^
SET TERM ; ^
SET TERM ^ ;
CREATE TRIGGER KEY_LOG_202505_BI FOR KEY_LOG_202505 ACTIVE
BEFORE INSERT POSITION 0
as  begin   if (new.num_key is null) then     new.num_key = gen_id(gen_KEY_LOG,1); end^
SET TERM ; ^
SET TERM ^ ;
CREATE TRIGGER KEY_LOG_202506_BI FOR KEY_LOG_202506 ACTIVE
BEFORE INSERT POSITION 0
as  begin   if (new.num_key is null) then     new.num_key = gen_id(gen_KEY_LOG,1); end^
SET TERM ; ^
SET TERM ^ ;
CREATE TRIGGER KEY_LOG_202507_BI FOR KEY_LOG_202507 ACTIVE
BEFORE INSERT POSITION 0
as  begin   if (new.num_key is null) then     new.num_key = gen_id(gen_KEY_LOG,1); end^
SET TERM ; ^
SET TERM ^ ;
CREATE TRIGGER KEY_LOG_202508_BI FOR KEY_LOG_202508 ACTIVE
BEFORE INSERT POSITION 0
as  begin   if (new.num_key is null) then     new.num_key = gen_id(gen_KEY_LOG,1); end^
SET TERM ; ^
SET TERM ^ ;
CREATE TRIGGER KEY_LOG_202509_BI FOR KEY_LOG_202509 ACTIVE
BEFORE INSERT POSITION 0
as  begin   if (new.num_key is null) then     new.num_key = gen_id(gen_KEY_LOG,1); end^
SET TERM ; ^
SET TERM ^ ;
CREATE TRIGGER KEY_LOG_202510_BI FOR KEY_LOG_202510 ACTIVE
BEFORE INSERT POSITION 0
as  begin   if (new.num_key is null) then     new.num_key = gen_id(gen_KEY_LOG,1); end^
SET TERM ; ^
SET TERM ^ ;
CREATE TRIGGER KEY_LOG_202511_BI FOR KEY_LOG_202511 ACTIVE
BEFORE INSERT POSITION 0
as  begin   if (new.num_key is null) then     new.num_key = gen_id(gen_KEY_LOG,1); end^
SET TERM ; ^
SET TERM ^ ;
CREATE TRIGGER KEY_LOG_202512_BI FOR KEY_LOG_202512 ACTIVE
BEFORE INSERT POSITION 0
as  begin   if (new.num_key is null) then     new.num_key = gen_id(gen_KEY_LOG,1); end^
SET TERM ; ^
SET TERM ^ ;
CREATE TRIGGER KEY_LOG_202601_BI FOR KEY_LOG_202601 ACTIVE
BEFORE INSERT POSITION 0
as  begin   if (new.num_key is null) then     new.num_key = gen_id(gen_KEY_LOG,1); end^
SET TERM ; ^
SET TERM ^ ;
CREATE TRIGGER KEY_LOG_202602_BI FOR KEY_LOG_202602 ACTIVE
BEFORE INSERT POSITION 0
as  begin   if (new.num_key is null) then     new.num_key = gen_id(gen_KEY_LOG,1); end^
SET TERM ; ^
SET TERM ^ ;
CREATE TRIGGER KILO_BI FOR KILO ACTIVE
BEFORE INSERT POSITION 0
AS
BEGIN
  IF (NEW.SEC_KILO IS NULL) THEN
    NEW.SEC_KILO = GEN_ID(GEN_KILO_ID, 1);
END^
SET TERM ; ^
SET TERM ^ ;
CREATE TRIGGER LICA_BD0 FOR LICA ACTIVE
BEFORE DELETE POSITION 0
AS
begin
  /* Trigger text */
  delete from limo where old.num_lica=num_lica;
end^
SET TERM ; ^
SET TERM ^ ;
CREATE TRIGGER LICA_BI FOR LICA ACTIVE
BEFORE INSERT POSITION 0
as
begin
  if (new.num_lica is null) then
    new.num_lica = gen_id(gen_lica_id,1);
end^
SET TERM ; ^
SET TERM ^ ;
CREATE TRIGGER LIMO_BI FOR LIMO ACTIVE
BEFORE INSERT POSITION 0
as
begin
  if (new.num_limo is null) then
    new.num_limo = gen_id(gen_limo_id,1);
end^
SET TERM ; ^
SET TERM ^ ;
CREATE TRIGGER LIQU_CRED_BI FOR LIQU_CRED ACTIVE
BEFORE INSERT POSITION 0
as  begin  if (new.num_LICR is null) then  new.num_LICR = gen_id(gen_LIQU_CRED_id,1); End^
SET TERM ; ^
SET TERM ^ ;
CREATE TRIGGER LIQU_DENO_BI FOR LIQU_DENO ACTIVE
BEFORE INSERT POSITION 0
as  begin  if (new.num_LIDN is null) then  new.num_LIDN = gen_id(gen_LIQU_DENO_id,1); End^
SET TERM ; ^
SET TERM ^ ;
CREATE TRIGGER LIQU_DEPO_BI FOR LIQU_DEPO ACTIVE
BEFORE INSERT POSITION 0
as  begin   new.num_LIDP = gen_id(gen_LIQU_DEPO_id,1); End^
SET TERM ; ^
SET TERM ^ ;
CREATE TRIGGER LIQU_DEPO_DENO_BI FOR LIQU_DEPO_DENO ACTIVE
BEFORE INSERT POSITION 0
as  begin   new.num_LIDN = gen_id(gen_LIQU_DEPO_DENO_id,1); End^
SET TERM ; ^
SET TERM ^ ;
CREATE TRIGGER LIQU_GAST_BI FOR LIQU_GAST ACTIVE
BEFORE INSERT POSITION 0
as  begin  if (new.num_LQGS is null) then  new.num_LQGS = gen_id(gen_LIQU_GAST_id,1); End^
SET TERM ; ^
SET TERM ^ ;
CREATE TRIGGER LIQU_ITEM_BI FOR LIQU_ITEM ACTIVE
BEFORE INSERT POSITION 0
as  begin  if (new.num_LQIT is null) then  new.num_LQIT = gen_id(gen_LIQU_ITEM_id,1); End^
SET TERM ; ^
SET TERM ^ ;
CREATE TRIGGER LIQU_PLACA_CUPO_BI FOR PLACA_CUPO ACTIVE
BEFORE INSERT POSITION 0
as  begin  if (new.num_PLCU is null) then  new.num_PLCU = gen_id(gen_PLACA_CUPO_id,1); End^
SET TERM ; ^
SET TERM ^ ;
CREATE TRIGGER LIQU_TANK_BI FOR LIQU_TANK ACTIVE
BEFORE INSERT POSITION 0
as  begin  if (new.num_LQTK is null) then  new.num_LQTK = gen_id(gen_LIQU_TANK_id,1); End^
SET TERM ; ^
SET TERM ^ ;
CREATE TRIGGER LIQU_TARJ_BI FOR LIQU_TARJ ACTIVE
BEFORE INSERT POSITION 0
as  begin  if (new.num_liqu_tarj is null) then  new.num_liqu_tarj = gen_id(GEN_LIQU_TARJ_ID,1); End^
SET TERM ; ^
SET TERM ^ ;
CREATE TRIGGER LIQU_TARJ_DET_BI FOR LIQU_TARJ_DET ACTIVE
BEFORE INSERT POSITION 0
as  begin   if (new.num_licr is null) then     new.num_licr = gen_id(gen_LIQU_TARJ_DET,1); end^
SET TERM ; ^
SET TERM ^ ;
CREATE TRIGGER LOG_BI FOR LOG ACTIVE
BEFORE INSERT POSITION 0
as
begin
  if (new.num_log is null) then
    new.num_log = gen_id(gen_log_id,1);
end^
SET TERM ; ^
SET TERM ^ ;
CREATE TRIGGER MOCB_B2 FOR MOCB_PEND ACTIVE
BEFORE INSERT POSITION 0
as begin if (new.num_mocb is null or new.num_mocb=0) then new.num_mocb = gen_id(gen_mocb_PEND_id,1); End^
SET TERM ; ^
SET TERM ^ ;
CREATE TRIGGER MOCB_BI FOR MOCB ACTIVE
BEFORE INSERT POSITION 0
as begin if (new.num_mocb is null or new.num_mocb=0) then new.num_mocb = gen_id(gen_mocb_id,1); End^
SET TERM ; ^
SET TERM ^ ;
CREATE TRIGGER MOCB_BI_REC FOR MOCB_REC ACTIVE
BEFORE INSERT POSITION 0
as begin if (new.num_mocb_rec is null or new.num_mocb_rec=0) then new.num_mocb_rec = gen_id(gen_mocb_rec_id,1); End^
SET TERM ; ^
SET TERM ^ ;
CREATE TRIGGER MOLI_BI FOR MOLI ACTIVE
BEFORE INSERT POSITION 0
AS
BEGIN
  IF (NEW.NUM_MOLI IS NULL) THEN
    NEW.NUM_MOLI = GEN_ID(GEN_MOLI_ID, 1);
END^
SET TERM ; ^
SET TERM ^ ;
CREATE TRIGGER MOLI_TANQ_BI FOR MOLI_TANQ ACTIVE
BEFORE INSERT POSITION 0
as  begin  if (new.NUM_MOLI_TANQ is null) then  new.NUM_MOLI_TANQ = gen_id(GEN_MOLI_TANQ_ID,1); End^
SET TERM ; ^
SET TERM ^ ;
CREATE TRIGGER MOPF_MOVI_BI FOR MOPF_MOVI ACTIVE
BEFORE INSERT POSITION 0
as  begin  if (new.num_mopf_movi is null) then  new.num_mopf_movi = gen_id(GEN_MOPF_MOVI_ID,1); End^
SET TERM ; ^
SET TERM ^ ;
CREATE TRIGGER MOVI_BI FOR MOVI ACTIVE
BEFORE INSERT POSITION 0
AS
begin
if (new.cos_item=0) then
    new.cos_item = 0;
  if (new.num_movi is null or new.num_movi=0) then
    new.num_movi = gen_id(gen_movi_id,1);
end^
SET TERM ; ^
SET TERM ^ ;
CREATE TRIGGER PREC_BI0 FOR PREC ACTIVE
BEFORE INSERT POSITION 0
as begin if (new.num_prec is null or new.num_prec=0) then new.num_prec = gen_id(gen_num_prec_id,1); End^
SET TERM ; ^
SET TERM ^ ;
CREATE TRIGGER PROM_BI FOR PROM ACTIVE
BEFORE INSERT POSITION 0
as  begin  new.num_prom = gen_id(GEN_PROM_ID,1); End^
SET TERM ; ^
SET TERM ^ ;
CREATE TRIGGER PROM_SORT_BI FOR PROM_SORT ACTIVE
BEFORE INSERT POSITION 0
as  begin   if (new.NUM_SORT is null or new.NUM_SORT=0 ) then     new.NUM_SORT = gen_id(GEN_NUM_PROM_SORT_ID,1); end^
SET TERM ; ^
SET TERM ^ ;
CREATE TRIGGER PRUEBAS_BI FOR PRUEBAS ACTIVE
BEFORE INSERT POSITION 0
as
begin
  if (new.id is null) then
    new.id = gen_id(gen_pruebas_id,1);
end^
SET TERM ; ^
SET TERM ^ ;
CREATE TRIGGER PUNTOS_BDP FOR PUNTOS ACTIVE
BEFORE DELETE POSITION 0
AS  BEGIN  update clie c set c.sal_puntos=c.sal_puntos-OLD.CAN_PUNTOS where c.cod_clie=OLD.COD_CLIE AND OLD.ELI_PUNTOS='0'; END^
SET TERM ; ^
SET TERM ^ ;
CREATE TRIGGER PUNTOS_BI FOR PUNTOS ACTIVE
BEFORE INSERT POSITION 0
as  begin   if (new.NUM_PUNTOS is null or new.NUM_PUNTOS=0 ) then     new.NUM_PUNTOS = gen_id(NUM_PUNTOS_ID,1); end^
SET TERM ; ^
SET TERM ^ ;
CREATE TRIGGER PUNTOS_BIP FOR PUNTOS ACTIVE
BEFORE INSERT POSITION 1
as  begin  update clie c set c.sal_puntos=c.sal_puntos+new.CAN_PUNTOS  WHERE  C.COD_CLIE=new.COD_CLIE AND new.ELI_PUNTOS='0' ;   end^
SET TERM ; ^
SET TERM ^ ;
CREATE TRIGGER PUNTOS_BUP FOR PUNTOS ACTIVE
BEFORE UPDATE POSITION 0
AS   BEGIN   update clie c set c.sal_puntos=c.sal_puntos-old.CAN_PUNTOS  WHERE  C.COD_CLIE=OLD.COD_CLIE AND old.ELI_PUNTOS='0' and new.ELI_PUNTOS='1' ;   update clie c set c.sal_puntos=c.sal_puntos+old.CAN_PUNTOS   WHERE  C.COD_CLIE=OLD.COD_CLIE AND old.ELI_PUNTOS='1' and new.ELI_PUNTOS='0' ;   END^
SET TERM ; ^
SET TERM ^ ;
CREATE TRIGGER R103_BD0 FOR R103 ACTIVE
BEFORE DELETE POSITION 0
AS
begin
  /* Trigger text */
  delete from r103_mov where num_r103 = old.num_r103 ;
  delete from comp where nummcomp = old.nummcomp ;
end^
SET TERM ; ^
SET TERM ^ ;
CREATE TRIGGER R103_BI FOR R103 ACTIVE
BEFORE INSERT POSITION 0
as
begin
  if (new.num_r103 is null) then
    new.num_r103 = gen_id(gen_r103_id,1);
end^
SET TERM ; ^
SET TERM ^ ;
CREATE TRIGGER R103_MOV_BI FOR R103_MOV ACTIVE
BEFORE INSERT POSITION 0
as
begin
  if (new.num_m103 is null) then
    new.num_m103 = gen_id(gen_r103_mov_id,1);
end^
SET TERM ; ^
SET TERM ^ ;
CREATE TRIGGER R104_BD0 FOR R104 ACTIVE
BEFORE DELETE POSITION 0
AS
begin
  /* Trigger text */
    delete from r104_mov where num_r104 = old.num_r104 ;
  delete from comp where nummcomp = old.nummcomp ;
end^
SET TERM ; ^
SET TERM ^ ;
CREATE TRIGGER R104_BI FOR R104 ACTIVE
BEFORE INSERT POSITION 0
as
begin
  if (new.num_r104 is null) then
    new.num_r104 = gen_id(gen_r104_id,1);
end^
SET TERM ; ^
SET TERM ^ ;
CREATE TRIGGER R104_MOV_BI FOR R104_MOV ACTIVE
BEFORE INSERT POSITION 0
as
begin
  if (new.num_m104 is null) then
    new.num_m104 = gen_id(gen_r104_mov_id,1);
end^
SET TERM ; ^
SET TERM ^ ;
CREATE TRIGGER REPESTCUEN_BI FOR REPESTCUEN ACTIVE
BEFORE INSERT POSITION 0
as begin if (new.NUMREP is null or new.NUMREP=0) then new.NUMREP = gen_id(GEN_REPESTCUEN_ID,1); End^
SET TERM ; ^
SET TERM ^ ;
CREATE TRIGGER RFID_COMB_BI FOR RFID_COMB ACTIVE
BEFORE INSERT POSITION 0
as begin if (new.sec_RFID is null or new.sec_RFID=0) then new.sec_RFID = gen_id(GEN_RFID_COMB_ID,1); End^
SET TERM ; ^
SET TERM ^ ;
CREATE TRIGGER RFID_KEY_BI FOR RFID_KEY ACTIVE
BEFORE INSERT POSITION 0
as begin if (new.NUM_RFID is null or new.NUM_RFID=0) then new.NUM_RFID = gen_id(GEN_RFID_KEY_ID,1); End^
SET TERM ; ^
SET TERM ^ ;
CREATE TRIGGER ROLP_BD0 FOR ROLP ACTIVE
BEFORE DELETE POSITION 0
AS
begin
  /* Trigger text */
  delete from rol_mov where old.num_rolp = num_rolp;
end^
SET TERM ; ^
SET TERM ^ ;
CREATE TRIGGER ROLP_BI FOR ROLP ACTIVE
BEFORE INSERT POSITION 0
as
begin
  if (new.num_rolp is null) then
    new.num_rolp = gen_id(gen_rolp_id,1);
end^
SET TERM ; ^
SET TERM ^ ;
CREATE TRIGGER ROL_MOV_BD0 FOR ROL_MOV ACTIVE
BEFORE DELETE POSITION 0
AS  begin  /* Trigger text */  /* delete from comp where comp.nummcomp=old.nummcomp ; */  end^
SET TERM ; ^
SET TERM ^ ;
CREATE TRIGGER ROL_MOV_BI FOR ROL_MOV ACTIVE
BEFORE INSERT POSITION 0
as
begin
  if (new.num_romo is null) then
    new.num_romo = gen_id(gen_rol_mov_id,1);
end^
SET TERM ; ^
SET TERM ^ ;
CREATE TRIGGER ROL_RUB_BI FOR ROL_RUB ACTIVE
BEFORE INSERT POSITION 0
as
begin
  if (new.num_rubr is null) then
    new.num_rubr = gen_id(gen_rol_rub_id,1);
end^
SET TERM ; ^
SET TERM ^ ;
CREATE TRIGGER RUTA_BI0 FOR RUTA ACTIVE
BEFORE INSERT POSITION 0
as begin if (new.num_ruta is null or new.num_ruta=0) then new.num_ruta = gen_id(gen_num_ruta_id,1); End^
SET TERM ; ^
SET TERM ^ ;
CREATE TRIGGER SESS_BI FOR SESS ACTIVE
BEFORE INSERT POSITION 0
AS BEGIN IF (NEW.NUM_SESS IS NULL or NEW.NUM_SESS=0  ) THEN NEW.NUM_SESS = GEN_ID(GEN_SESS_ID, 1); END^
SET TERM ; ^
SET TERM ^ ;
CREATE TRIGGER SYSLOCKEXCE_BI0 FOR SYSLOCKEXCE ACTIVE
BEFORE INSERT POSITION 0
as  begin  NEW.NUM_SYSLOCKEXCE = GEN_ID(GEN_SYSLOCKEXCE_ID, 1); end^
SET TERM ; ^
SET TERM ^ ;
CREATE TRIGGER SYSLOCKMOVI_BI0 FOR SYSLOCKMOVI ACTIVE
BEFORE INSERT POSITION 0
as  begin  if (new.num_SYSLOCKMOVI is null OR new.num_SYSLOCKMOVI=0) then  new.num_SYSLOCKMOVI = gen_id(gen_SYSLOCKMOVI_id,1); End^
SET TERM ; ^
SET TERM ^ ;
CREATE TRIGGER SYSLOCK_BI0 FOR SYSLOCK ACTIVE
BEFORE INSERT POSITION 0
as  begin  if (new.num_SYSLOCK is null OR new.num_SYSLOCK=0) then  new.num_SYSLOCK = gen_id(gen_SYSLOCK_id,1); End^
SET TERM ; ^
SET TERM ^ ;
CREATE TRIGGER SYSMODUTIPO_BI0 FOR SYSMODUTIPO ACTIVE
BEFORE INSERT POSITION 0
as  begin  if (new.num_SYSMODUTIPO is null) then  new.num_SYSMODUTIPO = gen_id(gen_SYSMODUTIPO_id,1); End^
SET TERM ; ^
SET TERM ^ ;
CREATE TRIGGER SYSMODU_BI0 FOR SYSMODU ACTIVE
BEFORE INSERT POSITION 0
as  begin  if (new.num_SYSMODU is null) then  new.num_SYSMODU = gen_id(gen_SYSMODU_id,1); End^
SET TERM ; ^
SET TERM ^ ;
CREATE TRIGGER TANQ_ING_BI FOR TANQ_ING ACTIVE
BEFORE INSERT POSITION 0
as  begin  if (new.num_tqin is null) then  new.num_tqin = gen_id(GEN_TANQ_ING_ID,1); End^
SET TERM ; ^
SET TERM ^ ;
CREATE TRIGGER TANQ_MOV_BI FOR TANQ_MOV ACTIVE
BEFORE INSERT POSITION 0
as  begin  if (new.num_tqmv is null) then  new.num_tqmv = gen_id(GEN_TANQ_MOV_ID,1); End^
SET TERM ; ^
SET TERM ^ ;
CREATE TRIGGER TANQ_REPO_BI FOR TANQ_REPO ACTIVE
BEFORE INSERT POSITION 0
as  begin   if (new.NUM_TANQ_REPO is null or new.NUM_TANQ_REPO=0 ) then     new.NUM_TANQ_REPO = gen_id(GEN_TANQ_REPO_ID,1); end^
SET TERM ; ^
SET TERM ^ ;
CREATE TRIGGER TANQ_RIND_BI FOR TANQ_RIND ACTIVE
BEFORE INSERT POSITION 0
as  begin  if (new.num_TANQ_RIND is null) then  new.num_TANQ_RIND = gen_id(gen_TANQ_RIND_ID,1); End^
SET TERM ; ^
SET TERM ^ ;
CREATE TRIGGER TANQ_TAB_BI FOR TANQ_TAB ACTIVE
BEFORE INSERT POSITION 0
as  begin  if (new.num_tqtb is null) then  new.num_tqtb = gen_id(GEN_TANQ_TAB_ID,1); End^
SET TERM ; ^
SET TERM ^ ;
CREATE TRIGGER TICKET_BI FOR TICKET ACTIVE
BEFORE INSERT POSITION 0
as  begin  if (new.num_tick is null) then  new.num_tick = gen_id(GEN_TICKET_ID,1); End^
SET TERM ; ^
SET TERM ^ ;
CREATE TRIGGER TICKET_MOV_BU0 FOR TICKET_MOV ACTIVE
BEFORE UPDATE POSITION 0
AS  
BEGIN  
    UPDATE TICKET T SET T.VEN_TICK = T.VEN_TICK - OLD.VEN_TICK_MOV + NEW.VEN_TICK_MOV,  
    T.SAL_TICK = T.SAL_TICK + OLD.VEN_TICK_MOV - NEW.VEN_TICK_MOV  
    where T.num_tick=new.num_tick;  
End^
SET TERM ; ^
SET TERM ^ ;
CREATE TRIGGER TLMM_BD0 FOR TLMM ACTIVE
BEFORE DELETE POSITION 0
AS
begin
  /* Trigger text */
  delete from iair where IAIR.numtl =OLD.num_tlmm;
  DELETE FROM mopf where mopf.num_tlmm = OLD.num_tlmm;
end^
SET TERM ; ^
SET TERM ^ ;
CREATE TRIGGER TRAM_BI FOR TRAM ACTIVE
BEFORE INSERT POSITION 0
as  begin  if (new.num_tram is null or new.num_tram=0) then  new.num_tram = gen_id(gen_tram_id,1);  end^
SET TERM ; ^
SET TERM ^ ;
CREATE TRIGGER TRAM_MOV_BI FOR TRAM_MOV ACTIVE
BEFORE INSERT POSITION 0
as  begin    if (new.num_trmo is null) then      new.num_trmo = gen_id(gen_tram_mov_id,1);  end^
SET TERM ; ^
SET TERM ^ ;
CREATE TRIGGER TURN_BI FOR TURN ACTIVE
BEFORE INSERT POSITION 0
as begin if (new.num_turn is null or new.num_turn=0 ) then new.num_turn = gen_id(gen_turn_id,1);end^
SET TERM ; ^
SET TERM ^ ;
CREATE TRIGGER TURN_DEPO_BI FOR TURN_DEPO ACTIVE
BEFORE INSERT POSITION 0
as  begin   if (new.num_tudp is null or new.num_tudp=0 ) then     new.num_tudp = gen_id(GEN_TURN_DEPO_ID,1); end^
SET TERM ; ^
SET TERM ^ ;
CREATE TRIGGER TURN_TARJ_BI FOR TURN_TARJ ACTIVE
BEFORE INSERT POSITION 0
as  begin   if (new.num_turn_tarj is null) then     new.num_turn_tarj = gen_id(GEN_TURN_TARJ,1); end^
SET TERM ; ^
SET TERM ^ ;
CREATE TRIGGER XDCTO_BIA0 FOR XDCTO ACTIVE
BEFORE INSERT POSITION 2
as 
 declare variable vNumTram integer;
 declare variable vNtlTram integer;
 declare variable vcodusua char(10);
 begin  select first 1 cod_usua from sess where est_sess='1' and ses_sess=CURRENT_CONNECTION into :vcodusua; if (:vcodusua is null) then    vcodusua=current_user;  
 if (new.num_xDCTO is null) then
 begin
 new.num_xDCTO = gen_id(gen_xDCTO_id,1); 
 end 
 select first 1 num_tram,ntl_tram from tram where (trim(tram.cod_usua)=trim(:vcodusua) and trim(tram.nip_tram)=trim(RDB$GET_CONTEXT('SYSTEM' , 'CLIENT_ADDRESS')) and tram.est_tram='0')  into :vnumtram,:vntltram ; 
    new.num_tram=:vnumtram; 
    update or insert into tram_mov (num_tram,tab_trmo,ntl_tram) values (:vnumtram, 'DCTO',:vntltram)
 MATCHING (num_tram,tab_trmo); 
 end ^
SET TERM ; ^
SET TERM ^ ;
CREATE TRIGGER XPGCA_BI FOR XPGCA ACTIVE
BEFORE INSERT POSITION 0
as
begin
  if (new.num_xpgca is null) then
    new.num_xpgca = gen_id(gen_xpgca_id,1);
    new.nip_XPGCA = (SELECT M.MON$REMOTE_ADDRESS FROM MON$ATTACHMENTS M WHERE M.MON$ATTACHMENT_ID = CURRENT_CONNECTION);
end^
SET TERM ; ^
SET TERM ^ ;
CREATE TRIGGER XPGDE_BI FOR XPGDE ACTIVE
BEFORE INSERT POSITION 0
as
begin
  if (new.num_xpgde is null) then
    new.num_xpgde = gen_id(gen_xpgde_id,1);
    new.nip_pgde = (SELECT M.MON$REMOTE_ADDRESS FROM MON$ATTACHMENTS M WHERE M.MON$ATTACHMENT_ID = CURRENT_CONNECTION);
end^
SET TERM ; ^
SET TERM ^ ;
CREATE TRIGGER ZONA_DIST_BI0 FOR ZONA_DIST ACTIVE
BEFORE INSERT POSITION 0
as begin if (new.num_zona_dist is null or new.num_zona_dist=0) then new.num_zona_dist = gen_id(gen_num_zona_dist_id,1); End^
SET TERM ; ^

GRANT RDB$ADMIN TO ADMINI;
GRANT RDB$ADMIN TO ELIASI;
GRANT RDB$ADMIN TO IRISI;
GRANT RDB$ADMIN TO KEVINI;
SET TERM ^ ;
ALTER PROCEDURE CAL_COS_ITEM (
    ICOD_ITEM Char(100),
    IUNI_ITEMC Double precision,
    IFEC_DCTO Timestamp )
RETURNS (
    RCOD_ITEM Char(100),
    RSCA_ITEM Numeric(15,3),
    RSUN_ITEM Numeric(15,3),
    RCOS_ITEM Numeric(15,6) )
AS
declare variable pcan_movi numeric(15,3);
declare variable ptot_movi numeric(15,6);
declare variable psum_item numeric(15,3);
declare variable pcod_item char(100);
declare variable pbem_item char(1);
declare variable ptip_dcto char(2);
declare variable pfec_dcto timestamp;
declare variable iuni_item double precision;
begin
  rsca_item=0;
  rsun_item=0;
  rcos_item=0;
  psum_item=0;
  iuni_item=:iuni_itemc;
  FOR SELECT cod_item, CAN_MOVI,TOT_MOVI,BEM_MOVI,TIP_DCTO,FEC_DCTO FROM MOVI,DCTO
  where MOVI.sec_dcto = DCTO.sec_dcto AND COD_ITEM =:ICOD_ITEM AND FEC_DCTO <= :IFEC_DCTO
  order by FEC_DCTO,ABS(CAN_MOVI)
  INTO :pcod_item, :PCAN_MOVI, :PTOT_MOVI, :pbem_item, :ptip_dcto, :pfec_dcto  DO
  BEGIN
    if (:PCAN_MOVI>0 AND :pbem_item ='C' AND :ptip_dcto <>'TR' and :ptip_dcto <>'DV' ) then
    BEGIN
        rcos_item = ((:rcos_item * :psum_item)+:PTOT_MOVI)/(:psum_item +:PCAN_MOVI);
    END
    rcod_item =:pcod_item;
    if (:pbem_item ='C') then
        begin 
            rsca_item= :rsca_item+:pcan_movi ;
        end
    if (:pbem_item <>'C') then
        begin 
            rsUN_item= :rsUN_item+:pcan_movi ;
        end
    if (:rsun_item <0 ) then
        begin
           if (mod(:rsun_item,:iuni_item )>0 ) then
           begin
            rsca_item  = :rsca_item  - (cast((Abs(:rsun_item ) / :iuni_item) as integer) + 1);
            rsun_item  = :rsun_item  + (:iuni_item  * (cast((Abs(:rsun_item ) / :iuni_item ) as integer) + 1));
           end
           if (mod(:rsun_item,:iuni_item )=0 ) then
           begin
            rsca_item  = :rsca_item  - (cast((Abs(:rsun_item ) / :iuni_item ) as integer));
            rsun_item  = :rsun_item  + (:iuni_item  * (cast((Abs(:rsun_item ) / :iuni_item ) as integer )));
           end
        end
    if (:iuni_item >0 and :rsun_item >:iuni_item) then
        begin
            rsca_item  = :rsca_item  + (cast((Abs(:rsun_item ) / :iuni_item ) as integer));
            rsun_item  = :rsun_item  - (:iuni_item  * (cast((Abs(:rsun_item ) / :iuni_item ) as integer )));
        end
  END
  /* Procedure Text */
  suspend;
end^
SET TERM ; ^

GRANT EXECUTE
 ON PROCEDURE CAL_COS_ITEM TO ADMINI;

GRANT EXECUTE
 ON PROCEDURE CAL_COS_ITEM TO ELIASI;

GRANT EXECUTE
 ON PROCEDURE CAL_COS_ITEM TO IRISI;

GRANT EXECUTE
 ON PROCEDURE CAL_COS_ITEM TO KEVINI;

GRANT EXECUTE
 ON PROCEDURE CAL_COS_ITEM TO SYSDBA;


SET TERM ^ ;
ALTER PROCEDURE CONSULTAS_VENTAS (
    IPROCESO Integer,
    IFECHA_DESDE Char(24),
    IFECHA_HASTA Char(24) )
RETURNS (
    ONOM_LIQ Char(100),
    OTRA_LIQ Decimal(15,0),
    OCAN_LIQ Decimal(15,2),
    OVAL_LIQ Decimal(15,2) )
AS
declare XTIP_LIQ CHAR(1);
declare XNOM_LIQ CHAR(100);
declare XTRA_LIQ DECIMAL(15,0);
declare XCAN_LIQ DECIMAL(15,2);            
declare XVAL_LIQ DECIMAL(15,2);

declare XNOM_PUNT char(50);
declare XVEN_PUNT char(9);
declare XFEC_MOLI timestamp;
declare XFCI_PUNT timestamp;

declare XFECHA_DESDE Char(24);
declare XFECHA_HASTA Char(24);

BEGIN

   
    ---- SE CARGAN FECHA Y HORA INICIAL Y FINAL EN EL FORMATO QUE TIENE LA FECHA EN DESPACHOS (TABLA DESP)
    
    XFECHA_DESDE = SUBSTRING(:IFECHA_DESDE from 1 for 4) || '/' || SUBSTRING(:IFECHA_DESDE from 6 for 2) || '/' || SUBSTRING(:IFECHA_DESDE from 9 for 2) || 
                    ' ' || substring(:IFECHA_DESDE from 13 for 8);

    XFECHA_HASTA = SUBSTRING(:IFECHA_HASTA from 1 for 4) || '/' || SUBSTRING(:IFECHA_HASTA from 6 for 2) || '/' || SUBSTRING(:IFECHA_HASTA from 9 for 2) ||
                    ' ' || substring(:IFECHA_HASTA from 13 for 8);


    ---- SI IPROCESO = 1 ES CONSULTA DE DESPACHOS AGRUPADOS POR FORMA DE PAGO
    ---- SE ACUMULAN LAS TRANSACCIONES Y VALORES VENDIDOS AGRUPADOS POR FORMA DE PAGO                        

    IF (:IPROCESO = 1) THEN
    BEGIN
        FOR SELECT (case when A.FAC_DESP in('0','1','2','5','6') then 'CONTADO'
            when a.fac_desp in('4','7','8','9') then 'TARJETAS'
            when a.fac_desp in('3') then 'CREDITO' 
            Else 'OTRA FORMA PAGO' end) as forma, count(*), sum(a.can_desp), sum(a.vto_desp)
            FROM DESP a
            where
            ((SUBSTRING(a.fec_desp from 7 for 4) || '/' || SUBSTRING(a.fec_desp from 4 for 2) || '/' ||
            SUBSTRING(a.fec_desp from 1 for 2) || ' ' || 	
                (case
                    when (substring(a.fec_desp from 12 for 2) in('0:','1:','2:','3:','4:','5:','6:','7:','8:','9:')) then '0' || substring(a.fec_desp from 12 for 7)
                    when (substring(a.fec_desp from 12 for 2) in('  ')) then '00:00:00'
                        else substring(a.fec_desp from 12 for 8) 
                 end)) >= SUBSTRING(:XFECHA_DESDE FROM 1 FOR 19) AND
            (SUBSTRING(a.fec_desp from 7 for 4) || '/' ||
            SUBSTRING(a.fec_desp from 4 for 2) || '/' ||
            SUBSTRING(a.fec_desp from 1 for 2) || ' ' || 	
                (case
                    when (substring(a.fec_desp from 12 for 2) in('0:','1:','2:','3:','4:','5:','6:','7:','8:','9:')) then '0' || substring(a.fec_desp from 12 for 7)
                    when (substring(a.fec_desp from 12 for 2) in('  ')) then '00:00:00'
                        else substring(a.fec_desp from 12 for 8) 
                 end) <= SUBSTRING(:XFECHA_HASTA FROM 1 FOR 19)))
            GROUP BY FORMA         
            INTO :ONOM_LIQ, :OTRA_LIQ, :OCAN_LIQ, :OVAL_LIQ

            DO BEGIN
                SUSPEND;
            END
    END

    ---- SI IPROCESO = 2 ES CONSULTA DE DESPACHOS AGRUPADOS POR TIPO DE PRODUCTO
    ---- SE ACUMULAN LAS TRANSACCIONES, CANTIDADES Y VALORES VENDIDOS AGRUPADOS POR TIPO DE PRODUCTO                        

    IF (:IPROCESO = 2) THEN
    BEGIN

        FOR SELECT 
            (case   when a.cod_prod = 1 then 'Super'
                    when a.cod_prod = 2 then 'Extra' 
                    when a.cod_prod = 3 then 'Diesel Premium' 
                    when a.cod_prod = 4 then 'Eco Plus 89' 
                    else 'Super Premium 95' end), count(*), sum(a.can_desp), sum(a.vto_desp)
            FROM DESP a
            where
            ((SUBSTRING(a.fec_desp from 7 for 4) || '/' || SUBSTRING(a.fec_desp from 4 for 2) || '/' ||
            SUBSTRING(a.fec_desp from 1 for 2) || ' ' || 	
                (case
                    when (substring(a.fec_desp from 12 for 2) in('0:','1:','2:','3:','4:','5:','6:','7:','8:','9:')) then '0' || substring(a.fec_desp from 12 for 7)
                    when (substring(a.fec_desp from 12 for 2) in('  ')) then '00:00:00'
                        else substring(a.fec_desp from 12 for 8) 
                 end) >= SUBSTRING(:XFECHA_DESDE FROM 1 FOR 19)) AND
            (SUBSTRING(a.fec_desp from 7 for 4) || '/' ||
            SUBSTRING(a.fec_desp from 4 for 2) || '/' ||
            SUBSTRING(a.fec_desp from 1 for 2) || ' ' || 	
                (case
                    when (substring(a.fec_desp from 12 for 2) in('0:','1:','2:','3:','4:','5:','6:','7:','8:','9:')) then '0' || substring(a.fec_desp from 12 for 7)
                    when (substring(a.fec_desp from 12 for 2) in('  ')) then '00:00:00'
                        else substring(a.fec_desp from 12 for 8) 
                 end) <= SUBSTRING(:XFECHA_HASTA FROM 1 FOR 19)))
            GROUP BY 1         
            INTO :ONOM_LIQ, :OTRA_LIQ, :OCAN_LIQ, :OVAL_LIQ
            
            DO BEGIN
                SUSPEND;
            END

    END

    ---- SI IPROCESO = 3 ES CONSULTA DE DOCUMENTOS (TABLA DCTO) AGRUPADOS POR FORMA DE PAGO
    ---- SE ACUMULAN LAS TRANSACCIONES Y VALORES VENDIDOS AGRUPADOS POR FORMA DE PAGO                        

    IF (:IPROCESO = 3) THEN
    BEGIN
        -- LAS DEVOLUCIONES EN LA TABLA MOVI TIENEN VALORES POSITIVOS EN GALONES Y NEGATIVOS EN FACTURAS DE VENTA, POR ESO SE COMPENSAN EN LA SUMATORIA TOTAL DE
        -- GALONES. PARA COMPENSAR LAS VALORES DE DEVOLUCIONES EN LA SUMATORIA DE TODOS LOS DOCUMENTOS DE MULTIPLICA POR -1, PARA QUE SE NETEE CON EL VALOR DE LA 
        -- FACTURA DE VENTA DEVUELTA
        -- ADICIONALMENTE SE CONSIDERAN SOLO LOS EGRESOS DE BODEGA (EB) CON (a.ane_dcto = '1' or a.ndo_dcto = ' ') PARA TOMAR SOLO LOS EGRESOS QUE AUN NO HAN
        -- SIDO CONVERTIDOS EN FACTURAS (CONSOLIDADOS) Y LO EGRESOS CORRESPONDIENTES A VENTAS DE LUBRICANTES

        FOR SELECT (case when a.COD_PAGO in('001','004','CON','EDV') then 'CONTADO'
            when a.COD_PAGO in('002','003') then 'TARJETAS'
            when a.COD_PAGO in('CRE','CDV','CEB') then 'CREDITO' 
            Else 'OTRA FORMA PAGO' end) as forma, count(*), sum((-1) * b.can_MOVI), sum(IIF(a.tip_dcto = 'DV', -1, 1) * (b.TOT_MOVI  + b.viv_movi))
            FROM DCTO a, MOVI b
            where a.SEC_DCTO = b.SEC_DCTO AND
            (a.fec_dcto >= :IFECHA_DESDE AND
            a.fec_dcto <= :IFECHA_HASTA) and
            (
            (a.tip_dcto = 'FV') or 
            (a.tip_dcto = 'EB' and (a.ane_dcto = '1' or a.ndo_dcto = ' ')) or   /* sea egreso no consolidado '1' o ' ' egreso de lubricantes */
            (a.tip_dcto = 'DV')            
            )   
            GROUP BY FORMA         
            INTO :ONOM_LIQ, :OTRA_LIQ, :OCAN_LIQ, :OVAL_LIQ

            DO BEGIN
                SUSPEND;
            END

    END

    ---- SI IPROCESO = 4 ES CONSULTA DE DOCUMENTOS (TABLA DCTO) AGRUPADOS POR TIPO DE PRODUCTO
    ---- SE ACUMULAN LAS TRANSACCIONES, CANTIDADES Y VALORES VENDIDOS AGRUPADOS POR TIPO DE PRODUCTO                        

    IF (:IPROCESO = 4) THEN
    BEGIN
        -- LAS DEVOLUCIONES EN LA TABLA MOVI TIENEN VALORES POSITIVOS EN GALONES Y NEGATIVOS EN FACTURAS DE VENTA, POR ESO SE COMPENSAN EN LA SUMATORIA TOTAL DE
        -- GALONES. PARA COMPENSAR LAS VALORES DE DEVOLUCIONES EN LA SUMATORIA DE TODOS LOS DOCUMENTOS DE MULTIPLICA POR -1, PARA QUE SE NETEE CON EL VALOR DE LA 
        -- FACTURA DE VENTA DEVUELTA
        -- ADICIONALMENTE SE CONSIDERAN SOLO LOS EGRESOS DE BODEGA (EB) CON (a.ane_dcto = '1' or a.ndo_dcto = ' ') PARA TOMAR SOLO LOS EGRESOS QUE AUN NO HAN
        -- SIDO CONVERTIDOS EN FACTURAS (CONSOLIDADOS) Y LO EGRESOS CORRESPONDIENTES A VENTAS DE LUBRICANTES

        FOR SELECT c.nom_item as item, count(*), sum((-1) * b.can_MOVI), sum(IIF(a.tip_dcto = 'DV', -1, 1) * (b.TOT_MOVI  + b.viv_movi))
            FROM DCTO a, MOVI b, ITEM c 
            where a.SEC_DCTO = b.SEC_DCTO AND
            b.cod_ITEM = c.COD_item and
            (a.fec_dcto >= :IFECHA_DESDE AND
            a.fec_dcto <= :IFECHA_HASTA) and
            (
            (a.tip_dcto = 'FV') or 
            (a.tip_dcto = 'EB' and (a.ane_dcto = '1' or a.ndo_dcto = ' ')) or   /* sea egreso no consolidado '1' o ' ' egreso de lubricantes */
            (a.tip_dcto = 'DV')            
            )   
            GROUP BY item         
            INTO :ONOM_LIQ, :OTRA_LIQ, :OCAN_LIQ, :OVAL_LIQ

            DO BEGIN
                SUSPEND;
            END
    END

    ---- SI IPROCESO = 5 ES CONSULTA DE NUMERO DE DEVOLUCIONES DE DOCUMENTOS (TABLA DCTO) AGRUPADOS POR FORMA DE PAGO
    
    IF (:IPROCESO = 5) THEN
    BEGIN

        FOR SELECT (case when a.COD_PAGO in('001','004','CON','EDV') then 'CONTADO'
            when a.COD_PAGO in('002','003') then 'TARJETAS'
            when a.COD_PAGO in('CRE','CDV','CEB') then 'CREDITO' 
            Else 'OTRA FORMA PAGO' end) as forma, count(*), 0, 0
            FROM DCTO a
            where (a.fec_dcto >= :IFECHA_DESDE AND
            a.fec_dcto <= :IFECHA_HASTA) and
            (a.tip_dcto = 'DV' and exists (select d.sec_dcto from dcto d where (IIF(a.ndo_dcto = ' ', 0, a.ndo_dcto) = d.sec_dcto) and d.num_cons = 0))                        GROUP BY FORMA         
            INTO :ONOM_LIQ, :OTRA_LIQ, :OCAN_LIQ, :OVAL_LIQ

            DO BEGIN
                SUSPEND;
            END

    END

    ---- SI IPROCESO = 6 ES CONSULTA DE NUMERO DE DEVOLUCIONES DE DOCUMENTOS (TABLA DCTO) AGRUPADOS POR TIPO DE PRODUCTO

    IF (:IPROCESO = 6) THEN
    BEGIN

        FOR SELECT c.nom_item, count(*), 0, 0
            FROM DCTO a, MOVI b, ITEM c 
            where a.SEC_DCTO = b.SEC_DCTO AND
            b.cod_ITEM = c.COD_item and
            (a.fec_dcto >= :IFECHA_DESDE AND
            a.fec_dcto <= :IFECHA_HASTA) and
            (a.tip_dcto = 'DV' and exists (select d.sec_dcto from dcto d where (IIF(a.ndo_dcto = ' ', 0, a.ndo_dcto) = d.sec_dcto) and d.num_cons = 0))                        GROUP BY C.nom_item         
            INTO :ONOM_LIQ, :OTRA_LIQ, :OCAN_LIQ, :OVAL_LIQ

            DO BEGIN
                SUSPEND;
            END

    END


END^
SET TERM ; ^

GRANT EXECUTE
 ON PROCEDURE CONSULTAS_VENTAS TO SYSDBA;


SET TERM ^ ;
ALTER PROCEDURE INTERVALOS_IMPRODUCTIVOS (
    FECHA_DESDE Char(19),
    FECHA_HASTA Char(19) )
RETURNS (
    SURTIDOR Char(4),
    MANGUERA Char(2),
    PRODUCTO Char(10),
    DIA Char(9),
    FECHA Char(10),
    HORA Char(8),
    INTERVALO Char(7),
    GALONES Numeric(15,4),
    MONTO Numeric(15,4),
    NDESPACHADOR Char(150) )
AS
declare variable primer_registro char(1);
declare variable aSURTIDOR Char(4);
declare variable aMANGUERA Char(2);
declare variable aPRODUCTO Char(10);
declare Khoras numeric(15,4);
declare thoras integer;
declare tminutos integer;
declare choras char(2);
declare cminutos char(2);

declare variable xSURTIDOR Char(4);
declare variable xMANGUERA Char(2);
declare variable xPRODUCTO Char(10);
declare xDIA Char(9);
declare xFECHA Char(10);
declare xHORA Char(8);
declare xintervalo char(7);
declare xGALONES Numeric(15,4);
declare xMONTO Numeric(15,4);
declare xNDESPACHADOR Char(150);
declare variable ySURTIDOR Char(4);
declare variable yMANGUERA Char(2);
declare variable yPRODUCTO Char(10);
declare yDIA Char(9);
declare yFECHA Char(10);
declare yHORA Char(8);
declare yintervalo char(7);
declare yGALONES Numeric(15,4);
declare yMONTO Numeric(15,4);
declare yNDESPACHADOR Char(150);

declare zHORA Char(8);

BEGIN

----

---- OBTIENE HORA DE CIERRE DEL TURNO DEL DOMINGO, PARA INICIAR LA SEMANA
/*
    SELECT substring(max(a.FCI_PUNT) from 12 for 8) from moli a
    where 
    a.Fci_punt >= (substring(:FECHA_DESDE from 1 for 10) || ', 00:00:00.000') and
    a.Fci_punt <= (substring(:FECHA_DESDE from 1 for 10) || ', 08:59:59.999') and
    a.det_moli = 'd'
    into :zhora;
    
    fecha_desde = substring(FECHA_DESDE from 1 for 10) || ' ' || zhora; 

    select  :xsurtidor, :xmanguera, :xproducto, :xdia, :xfecha, :zhora, :choras || ':' || :cminutos, :xgalones, :xmonto, :FECHA_DESDE FROM RDB$DATABASE into :surtidor, :manguera, :producto, :dia, :fecha, :hora, :intervalo, :galones, :monto, :ndespachador;
    suspend;

--- OBTIENE HORA DE CIERRE DEL TURNO DEL DOMINGO, PARA TERMINAR LA SEMANA
 
    SELECT substring(max(a.FCI_PUNT) from 12 for 8) from moli a
    where 
    a.Fci_punt >= (substring(:FECHA_HASTA from 1 for 10) || ', 00:00:00.000') and
    a.Fci_punt <= (substring(:FECHA_HASTA from 1 for 10) || ', 08:59:59.999') and
    a.det_moli = 'd'
    into :zhora;
    
    fecha_hasta = substring(FECHA_HASTA from 1 for 10) || ' ' || zhora; 

    select  :xsurtidor, :xmanguera, :xproducto, :xdia, :xfecha, :zhora, :choras || ':' || :cminutos, :xgalones, :xmonto, :FECHA_HASTA FROM RDB$DATABASE into :surtidor, :manguera, :producto, :dia, :fecha, :hora, :intervalo, :galones, :monto, :ndespachador;
    suspend;
*/
----


	primer_registro = ' ';
	khoras = 0;


  FOR SELECT a.SUR_DESP, a.COD_MANG, c.nom_tanq,
     DECODE(
      EXTRACT(
         WEEKDAY FROM CAST(SUBSTRING(a.fec_desp from 7 for 4) || '/' ||
            SUBSTRING(a.fec_desp from 4 for 2) || '/' ||
            SUBSTRING(a.fec_desp from 1 for 2) AS DATE)), 
            0, 'DOMINGO', 
            1, 'LUNES', 
            2, 'MARTES', 
            3, 'MIERCOLES', 
            4, 'JUEVES', 
            5, 'VIERNES', 
            6, 'SABADO'),
 	SUBSTRING(a.fec_desp from 7 for 4) || '/' ||
	SUBSTRING(a.fec_desp from 4 for 2) || '/' ||
	SUBSTRING(a.fec_desp from 1 for 2),
	(case
      		when (substring(a.fec_desp from 12 for 2) in('0:','1:','2:','3:','4:','5:','6:','7:','8:','9:')) then '0' || substring(a.fec_desp from 12 for 7)
      		when (substring(a.fec_desp from 12 for 2) in('  ')) then '00:00:00'
            	else substring(a.fec_desp from 12 for 8) 
	end), a.CAN_DESP, a.VTO_DESP, b.nom_clie
	FROM DESP a, clie b, tanq c
	where (a.ven_punt = b.cod_clie and
    b.cpr_clie = 'D') and
    '0' || a.cod_prod = c.cod_tanq and 
    ((SUBSTRING(a.fec_desp from 7 for 4) || '/' ||
	SUBSTRING(a.fec_desp from 4 for 2) || '/' ||
	SUBSTRING(a.fec_desp from 1 for 2) || ' ' || 	
        (case
      		when (substring(a.fec_desp from 12 for 2) in('0:','1:','2:','3:','4:','5:','6:','7:','8:','9:')) then '0' || substring(a.fec_desp from 12 for 7)
      		when (substring(a.fec_desp from 12 for 2) in('  ')) then '00:00:00'
            	else substring(a.fec_desp from 12 for 8) 
         end)) >= :fecha_desde AND
	(SUBSTRING(a.fec_desp from 7 for 4) || '/' ||
	SUBSTRING(a.fec_desp from 4 for 2) || '/' ||
	SUBSTRING(a.fec_desp from 1 for 2) || ' ' || 	
        (case
      		when (substring(a.fec_desp from 12 for 2) in('0:','1:','2:','3:','4:','5:','6:','7:','8:','9:')) then '0' || substring(a.fec_desp from 12 for 7)
      		when (substring(a.fec_desp from 12 for 2) in('  ')) then '00:00:00'
            	else substring(a.fec_desp from 12 for 8) 
         end)) <= :fecha_hasta) order by 1,2,5,6 
  INTO :xsurtidor, :xmanguera, :xproducto, :xdia, :xfecha, :xhora, :xgalones, :xmonto, :xndespachador  DO
  BEGIN

	if (primer_registro <> ' ') then 
	begin
            if (xsurtidor <> asurtidor or xmanguera <> amanguera) then 
            begin

                yhora = substring(FECHA_DESDE from 12 for 8);
                if ((CAST(xhora AS time) - cast(yhora as time))< 0) then
                begin
                    khoras = (CAST('23:59:59' AS time) - cast(yhora as time)) + (CAST(xhora AS time) - cast('00:00:00' as time));
                    thoras = trunc(khoras / 3600);
                    tminutos = trunc((khoras - (thoras * 3600)) / 60);  
                end
                else
                begin 
                    khoras = CAST(xhora AS time) - cast(yhora as time);                    
                    thoras = trunc(khoras / 3600);
                    tminutos = trunc((khoras - (thoras * 3600)) / 60);  
                end
                choras = thoras;
                cminutos = tminutos;
                if (thoras < 10) then
                begin
                    choras = '0' || thoras;
                end    
                if (tminutos < 10) then
                begin
                    cminutos = '0' || tminutos;
                end    
            
                select  :xsurtidor, :xmanguera, :xproducto, :xdia, :xfecha, :xhora, :choras || ':' || :cminutos, :xgalones, :xmonto, :xndespachador FROM RDB$DATABASE into :surtidor, :manguera, :producto, :dia, :fecha, :hora, :intervalo, :galones, :monto, :ndespachador;
                suspend;

                asurtidor = xsurtidor;
                amanguera = xmanguera;
                
                ysurtidor = xsurtidor;
                ymanguera = xmanguera;
                yproducto = xproducto;                
                ydia = xdia;
                yfecha = xfecha;
                yhora = xhora;
                ygalones= xgalones;
                ymonto = xmonto;
                yndespachador = xndespachador;

            end 
            else
            begin

                if ((CAST(xhora AS time) - cast(yhora as time))< 0) then
                begin
                    khoras = (CAST('23:59:59' AS time) - cast(yhora as time)) + (CAST(xhora AS time) - cast('00:00:00' as time));
                    thoras = trunc(khoras / 3600);
                    tminutos = trunc((khoras - (thoras * 3600)) / 60);  
                end
                else
                begin 
                    khoras = CAST(xhora AS time) - cast(yhora as time);                    
                    thoras = trunc(khoras / 3600);
                    tminutos = trunc((khoras - (thoras * 3600)) / 60);  
                end
                choras = thoras;
                cminutos = tminutos;
                if (thoras < 10) then
                begin
                    choras = '0' || thoras;
                end    
                if (tminutos < 10) then
                begin
                    cminutos = '0' || tminutos;
                end    

                select  :xsurtidor, :xmanguera, :xproducto, :xdia, :xfecha, :xhora, :choras || ':' || :cminutos, :xgalones, :xmonto, :xndespachador FROM RDB$DATABASE into :surtidor, :manguera, :producto, :dia, :fecha, :hora, :intervalo, :galones, :monto, :ndespachador;
                suspend;

                ysurtidor = xsurtidor;
                ymanguera = xmanguera;
                yproducto = xproducto;                                
                ydia = xdia;
                yfecha = xfecha;
                yhora = xhora;
                ygalones= xgalones;
                ymonto = xmonto;
                yndespachador = xndespachador;

            end 
	end 
	else
	begin
	
        yhora = substring(FECHA_DESDE from 12 for 8);
        if ((CAST(xhora AS time) - cast(yhora as time))< 0) then
        begin
            khoras = (CAST('23:59:59' AS time) - cast(yhora as time)) + (CAST(xhora AS time) - cast('00:00:00' as time));
            thoras = trunc(khoras / 3600);
            tminutos = trunc((khoras - (thoras * 3600)) / 60);  
        end
        else
        begin 
            khoras = CAST(xhora AS time) - cast(yhora as time);                    
            thoras = trunc(khoras / 3600);
            tminutos = trunc((khoras - (thoras * 3600)) / 60);  
        end
        choras = thoras;
        cminutos = tminutos;
        if (thoras < 10) then
        begin
            choras = '0' || thoras;
        end    
        if (tminutos < 10) then
        begin
            cminutos = '0' || tminutos;
        end    
    
        select  :xsurtidor, :xmanguera, :xproducto, :xdia, :xfecha, :xhora, :choras || ':' || :cminutos, :xgalones, :xmonto, :xndespachador FROM RDB$DATABASE into :surtidor, :manguera, :producto, :dia, :fecha, :hora, :intervalo, :galones, :monto, :ndespachador;
        suspend;

        primer_registro = '1';	
        asurtidor = xsurtidor;
        amanguera = xmanguera;
        
        ysurtidor = xsurtidor;
        ymanguera = xmanguera;
        yproducto = xproducto;                
        ydia = xdia;
        yfecha = xfecha;
        yhora = xhora;
        ygalones= xgalones;
        ymonto = xmonto;
        yndespachador = xndespachador;
        
	end 
  END
--  select  :asurtidor, :amanguera, :aproducto, :afecha, :ahora, :khoras FROM RDB$DATABASE into :surtidor, :manguera, :fecha, :hora, :galones;
--  suspend;

--SELECT 'PASA A FOR SELECT TEMP' FROM RDB$DATABASE into :ndespachador;
--suspend;



END^
SET TERM ; ^

GRANT EXECUTE
 ON PROCEDURE INTERVALOS_IMPRODUCTIVOS TO SYSDBA;


SET TERM ^ ;
ALTER PROCEDURE INTERVALOS_IMPRODUCTIVOS_MANUAL (
    FECHA_DESDE Char(19),
    FECHA_HASTA Char(19),
    VALOR Integer )
RETURNS (
    SURTIDOR Char(4),
    MANGUERA Char(2),
    PRODUCTO Char(10),
    DIA Char(9),
    FECHA Char(10),
    HORA Char(8),
    INTERVALO Char(7),
    GALONES Numeric(15,4),
    MONTO Numeric(15,4),
    NDESPACHADOR Char(150) )
AS
declare variable primer_registro char(1);
declare variable aSURTIDOR Char(4);
declare variable aMANGUERA Char(2);
declare variable aPRODUCTO Char(10);
declare Khoras numeric(15,4);
declare thoras integer;
declare tminutos integer;
declare choras char(2);
declare cminutos char(2);

declare variable xSURTIDOR Char(4);
declare variable xMANGUERA Char(2);
declare variable xPRODUCTO Char(10);
declare xDIA Char(9);
declare xFECHA Char(10);
declare xHORA Char(8);
declare xintervalo char(7);
declare xGALONES Numeric(15,4);
declare xMONTO Numeric(15,4);
declare xNDESPACHADOR Char(150);
declare variable ySURTIDOR Char(4);
declare variable yMANGUERA Char(2);
declare variable yPRODUCTO Char(10);
declare yDIA Char(9);
declare yFECHA Char(10);
declare yHORA Char(8);
declare yintervalo char(7);
declare yGALONES Numeric(15,4);
declare yMONTO Numeric(15,4);
declare yNDESPACHADOR Char(150);

declare zHORA Char(8);

BEGIN

----

---- OBTIENE HORA DE CIERRE DEL TURNO DEL DOMINGO, PARA INICIAR LA SEMANA

    SELECT substring(max(a.FCI_PUNT) from 12 for 8) from moli a
    where 
    a.Fci_punt >= (substring(:FECHA_DESDE from 1 for 10) || ', 00:00:00.000') and
    a.Fci_punt <= (substring(:FECHA_DESDE from 1 for 10) || ', 06:59:59.999') and
    a.det_moli = 'd'
    into :zhora;
    
    fecha_desde = substring(FECHA_DESDE from 1 for 10) || ' ' || zhora; 

    select  :xsurtidor, :xmanguera, :xproducto, :xdia, :xfecha, :zhora, :choras || ':' || :cminutos, :xgalones, :xmonto, :FECHA_DESDE FROM RDB$DATABASE into :surtidor, :manguera, :producto, :dia, :fecha, :hora, :intervalo, :galones, :monto, :ndespachador;
    suspend;

--- OBTIENE HORA DE CIERRE DEL TURNO DEL DOMINGO, PARA TERMINAR LA SEMANA
 
    SELECT substring(max(a.FCI_PUNT) from 12 for 8) from moli a
    where 
    a.Fci_punt >= (substring(:FECHA_HASTA from 1 for 10) || ', 00:00:00.000') and
    a.Fci_punt <= (substring(:FECHA_HASTA from 1 for 10) || ', 06:59:59.999') and
    a.det_moli = 'd'
    into :zhora;
    
    fecha_hasta = substring(FECHA_HASTA from 1 for 10) || ' ' || zhora; 

    select  :xsurtidor, :xmanguera, :xproducto, :xdia, :xfecha, :zhora, :choras || ':' || :cminutos, :xgalones, :xmonto, :FECHA_HASTA FROM RDB$DATABASE into :surtidor, :manguera, :producto, :dia, :fecha, :hora, :intervalo, :galones, :monto, :ndespachador;
    suspend;

----


	primer_registro = ' ';
	khoras = 0;


  FOR SELECT a.SUR_DESP, a.COD_MANG, c.nom_tanq,
     DECODE(
      EXTRACT(
         WEEKDAY FROM CAST(SUBSTRING(a.fec_desp from 7 for 4) || '/' ||
            SUBSTRING(a.fec_desp from 4 for 2) || '/' ||
            SUBSTRING(a.fec_desp from 1 for 2) AS DATE)), 
            0, 'DOMINGO', 
            1, 'LUNES', 
            2, 'MARTES', 
            3, 'MIERCOLES', 
            4, 'JUEVES', 
            5, 'VIERNES', 
            6, 'SABADO'),
 	SUBSTRING(a.fec_desp from 7 for 4) || '/' ||
	SUBSTRING(a.fec_desp from 4 for 2) || '/' ||
	SUBSTRING(a.fec_desp from 1 for 2),
	(case
      		when (substring(a.fec_desp from 12 for 2) in('0:','1:','2:','3:','4:','5:','6:','7:','8:','9:')) then '0' || substring(a.fec_desp from 12 for 7)
      		when (substring(a.fec_desp from 12 for 2) in('  ')) then '00:00:00'
            	else substring(a.fec_desp from 12 for 8) 
	end), a.CAN_DESP, a.VTO_DESP, b.nom_clie
	FROM DESP a, clie b, tanq c
	where (a.ven_punt = b.cod_clie and
    b.cpr_clie = 'D') and
    '0' || a.cod_prod = c.cod_tanq and 
    ((SUBSTRING(a.fec_desp from 7 for 4) || '/' ||
	SUBSTRING(a.fec_desp from 4 for 2) || '/' ||
	SUBSTRING(a.fec_desp from 1 for 2) || ' ' || 	
        (case
      		when (substring(a.fec_desp from 12 for 2) in('0:','1:','2:','3:','4:','5:','6:','7:','8:','9:')) then '0' || substring(a.fec_desp from 12 for 7)
      		when (substring(a.fec_desp from 12 for 2) in('  ')) then '00:00:00'
            	else substring(a.fec_desp from 12 for 8) 
         end)) >= :fecha_desde AND
	(SUBSTRING(a.fec_desp from 7 for 4) || '/' ||
	SUBSTRING(a.fec_desp from 4 for 2) || '/' ||
	SUBSTRING(a.fec_desp from 1 for 2) || ' ' || 	
        (case
      		when (substring(a.fec_desp from 12 for 2) in('0:','1:','2:','3:','4:','5:','6:','7:','8:','9:')) then '0' || substring(a.fec_desp from 12 for 7)
      		when (substring(a.fec_desp from 12 for 2) in('  ')) then '00:00:00'
            	else substring(a.fec_desp from 12 for 8) 
         end)) <= :fecha_hasta) order by 1,2,5,6 
  INTO :xsurtidor, :xmanguera, :xproducto, :xdia, :xfecha, :xhora, :xgalones, :xmonto, :xndespachador  DO
  BEGIN

	if (primer_registro <> ' ') then 
	begin
            if (xsurtidor <> asurtidor or xmanguera <> amanguera) then 
            begin

                yhora = substring(FECHA_DESDE from 12 for 8);
                if ((CAST(xhora AS time) - cast(yhora as time))< 0) then
                begin
                    khoras = (CAST('23:59:59' AS time) - cast(yhora as time)) + (CAST(xhora AS time) - cast('00:00:00' as time));
                    thoras = trunc(khoras / 3600);
                    tminutos = trunc((khoras - (thoras * 3600)) / 60);  
                end
                else
                begin 
                    khoras = CAST(xhora AS time) - cast(yhora as time);                    
                    thoras = trunc(khoras / 3600);
                    tminutos = trunc((khoras - (thoras * 3600)) / 60);  
                end
                choras = thoras;
                cminutos = tminutos;
                if (thoras < 10) then
                begin
                    choras = '0' || thoras;
                end    
                if (tminutos < 10) then
                begin
                    cminutos = '0' || tminutos;
                end    
            
                select  :xsurtidor, :xmanguera, :xproducto, :xdia, :xfecha, :xhora, :choras || ':' || :cminutos, :xgalones, :xmonto, :xndespachador FROM RDB$DATABASE into :surtidor, :manguera, :producto, :dia, :fecha, :hora, :intervalo, :galones, :monto, :ndespachador;
                suspend;

                asurtidor = xsurtidor;
                amanguera = xmanguera;
                
                ysurtidor = xsurtidor;
                ymanguera = xmanguera;
                yproducto = xproducto;                
                ydia = xdia;
                yfecha = xfecha;
                yhora = xhora;
                ygalones= xgalones;
                ymonto = xmonto;
                yndespachador = xndespachador;

            end 
            else
            begin

                if ((CAST(xhora AS time) - cast(yhora as time))< 0) then
                begin
                    khoras = (CAST('23:59:59' AS time) - cast(yhora as time)) + (CAST(xhora AS time) - cast('00:00:00' as time));
                    thoras = trunc(khoras / 3600);
                    tminutos = trunc((khoras - (thoras * 3600)) / 60);  
                end
                else
                begin 
                    khoras = CAST(xhora AS time) - cast(yhora as time);                    
                    thoras = trunc(khoras / 3600);
                    tminutos = trunc((khoras - (thoras * 3600)) / 60);  
                end
                choras = thoras;
                cminutos = tminutos;
                if (thoras < 10) then
                begin
                    choras = '0' || thoras;
                end    
                if (tminutos < 10) then
                begin
                    cminutos = '0' || tminutos;
                end    

                select  :xsurtidor, :xmanguera, :xproducto, :xdia, :xfecha, :xhora, :choras || ':' || :cminutos, :xgalones, :xmonto, :xndespachador FROM RDB$DATABASE into :surtidor, :manguera, :producto, :dia, :fecha, :hora, :intervalo, :galones, :monto, :ndespachador;
                suspend;

                ysurtidor = xsurtidor;
                ymanguera = xmanguera;
                yproducto = xproducto;                                
                ydia = xdia;
                yfecha = xfecha;
                yhora = xhora;
                ygalones= xgalones;
                ymonto = xmonto;
                yndespachador = xndespachador;

            end 
	end 
	else
	begin
	
        yhora = substring(FECHA_DESDE from 12 for 8);
        if ((CAST(xhora AS time) - cast(yhora as time))< 0) then
        begin
            khoras = (CAST('23:59:59' AS time) - cast(yhora as time)) + (CAST(xhora AS time) - cast('00:00:00' as time));
            thoras = trunc(khoras / 3600);
            tminutos = trunc((khoras - (thoras * 3600)) / 60);  
        end
        else
        begin 
            khoras = CAST(xhora AS time) - cast(yhora as time);                    
            thoras = trunc(khoras / 3600);
            tminutos = trunc((khoras - (thoras * 3600)) / 60);  
        end
        choras = thoras;
        cminutos = tminutos;
        if (thoras < 10) then
        begin
            choras = '0' || thoras;
        end    
        if (tminutos < 10) then
        begin
            cminutos = '0' || tminutos;
        end    
    
        select  :xsurtidor, :xmanguera, :xproducto, :xdia, :xfecha, :xhora, :choras || ':' || :cminutos, :xgalones, :xmonto, :xndespachador FROM RDB$DATABASE into :surtidor, :manguera, :producto, :dia, :fecha, :hora, :intervalo, :galones, :monto, :ndespachador;
        suspend;

        primer_registro = '1';	
        asurtidor = xsurtidor;
        amanguera = xmanguera;
        
        ysurtidor = xsurtidor;
        ymanguera = xmanguera;
        yproducto = xproducto;                
        ydia = xdia;
        yfecha = xfecha;
        yhora = xhora;
        ygalones= xgalones;
        ymonto = xmonto;
        yndespachador = xndespachador;
        
	end 
  END
--  select  :asurtidor, :amanguera, :aproducto, :afecha, :ahora, :khoras FROM RDB$DATABASE into :surtidor, :manguera, :fecha, :hora, :galones;
--  suspend;

--SELECT 'PASA A FOR SELECT TEMP' FROM RDB$DATABASE into :ndespachador;
--suspend;



END^
SET TERM ; ^

GRANT EXECUTE
 ON PROCEDURE INTERVALOS_IMPRODUCTIVOS_MANUAL TO SYSDBA;


SET TERM ^ ;
ALTER PROCEDURE KARDEX (
    IEB_CONS Integer DEFAULT 1,
    IFV_CONS Integer DEFAULT 1,
    ICOD_BODE Char(2) DEFAULT '01',
    ICOD_ITEM Char(20) DEFAULT '',
    ISAL_ANTE Numeric(12,3) DEFAULT 0,
    IFDE_DCTO Timestamp DEFAULT '01/01/2018',
    IFHA_DCTO Timestamp DEFAULT '12/31/2018 23:59:59' )
RETURNS (
    BODEGA Char(2),
    TIPO Char(2),
    NUMERO Char(20),
    FECHA Timestamp,
    BEM_MOVI Char(1),
    SECUENCIAL Integer,
    DETALLE Char(300),
    PLACA Char(50),
    CANTIDAD Numeric(12,3),
    SALDO Numeric(18,3),
    CODIGO Char(10),
    RUC Char(15),
    NOMBRE Char(300),
    DEBITO Numeric(18,3),
    CREDITO Numeric(18,3),
    CONSOLIDACION Integer,
    CANTIDAD_CONSOLIDACION Numeric(18,3),
    DEBITO_CONSOLIDACION Numeric(18,3) )
AS
declare variable saldo_acumulado numeric(18,3); BEGIN saldo_acumulado = ISAL_ANTE;DEBITO = 0;CREDITO = 0;CANTIDAD_CONSOLIDACION =0;DEBITO_CONSOLIDACION =0; for select d.cod_bode,d.tip_dcto, d.num_dcto,d.fec_dcto,m.bem_movi,M.sec_dcto,trim(d.det_dcto),Trim (d.pla_dcto), m.can_movi, c.cod_clie, c.ruc_clie, Trim(c.nom_clie) ,D.NUM_CONS  from movi m  inner join dcto d on m.sec_dcto = d.sec_dcto LEFT OUTER JOIN clie c on c.cod_clie=d.cod_clie  where  cod_bode like iif(:ICOD_BODE='00','%',:ICOD_BODE || '%')  and d.fec_dcto> cast(:IFDE_DCTO as timestamp)  and d.fec_dcto<= cast(:IFHA_DCTO as timestamp) and  m.cod_item = :ICOD_ITEM order by d.fec_dcto,m.bem_movi,m.can_movi  descending  INTO :Bodega,:Tipo,:Numero,:Fecha,:bem_movi,:Secuencial,:Detalle,:Placa,:Cantidad,:Codigo,  :Ruc,:Nombre,:consolidacion Do BEGIN Saldo = :saldo_acumulado + :Cantidad; Saldo_acumulado = :Saldo; CANTIDAD_CONSOLIDACION = 0;  if (:Tipo='FV' and :consolidacion>0) then    BEGIN     CANTIDAD_CONSOLIDACION = (:cantidad*-1);         DEBITO_CONSOLIDACION = :DEBITO_CONSOLIDACION + (:Cantidad*-1);         Saldo = :saldo_acumulado + :CANTIDAD_CONSOLIDACION;         Saldo_acumulado = :Saldo ;     End  if (:Cantidad > 0) then     DEBITO = :DEBITO + :Cantidad  ;  Else     CREDITO = :CREDITO + (:Cantidad*-1);  suspend;   End  end^
SET TERM ; ^

GRANT EXECUTE
 ON PROCEDURE KARDEX TO SYSDBA;


SET TERM ^ ;
ALTER PROCEDURE KERDEX (
    IEB_CONS Integer DEFAULT 1,
    IFV_CONS Integer DEFAULT 1,
    ICOD_BODE Char(2) DEFAULT '01',
    ICOD_ITEM Char(20) DEFAULT '',
    ISAL_ANTE Numeric(12,3) DEFAULT 0,
    IFDE_DCTO Timestamp DEFAULT '01/01/2018',
    IFHA_DCTO Timestamp DEFAULT '12/31/2018 23:59:59' )
RETURNS (
    BODEGA Char(2),
    TIPO Char(2),
    NUMERO Char(20),
    FECHA Timestamp,
    BEM_MOVI Char(1),
    SECUENCIAL Integer,
    DETALLE Char(300),
    PLACA Char(50),
    CANTIDAD Numeric(12,3),
    SALDO Numeric(18,3),
    CODIGO Char(10),
    RUC Char(15),
    NOMBRE Char(300),
    DEBITO Numeric(18,3),
    CREDITO Numeric(18,3) )
AS
declare variable saldo_acumulado numeric(18,3); begin  saldo_acumulado = ISAL_ANTE;DEBITO = 0;CREDITO = 0;  for select d.cod_bode,d.tip_dcto, d.num_dcto,d.fec_dcto,m.bem_movi,M.sec_dcto,trim(d.det_dcto),  Trim (d.pla_dcto), m.can_movi, c.cod_clie, c.ruc_clie, Trim(c.nom_clie)  from movi m inner join dcto d on m.sec_dcto = d.sec_dcto LEFT OUTER JOIN clie c on c.cod_clie=d.cod_clie  where  cod_bode like iif(:ICOD_BODE='00','%',:ICOD_BODE || '%')  AND  ((tip_dcto='EB' and num_cons<:IEB_CONS) or tip_dcto<>'EB')  AND  ((tip_dcto='FV' and num_cons<:IFV_CONS) or tip_dcto<>'FV')  and d.fec_dcto> cast(:IFDE_DCTO as timestamp) and d.fec_dcto<= cast(:IFHA_DCTO as timestamp) and  m.cod_item = :ICOD_ITEM order by d.fec_dcto,m.bem_movi,m.can_movi descending  INTO :Bodega,:Tipo,:Numero,:Fecha,:bem_movi,:Secuencial,:Detalle,:Placa,:Cantidad,:Codigo,:Ruc,:Nombre  Do  begin      Saldo = saldo_acumulado + :Cantidad; Saldo_acumulado = :Saldo ; if (:Cantidad > 0) then DEBITO = :DEBITO + :Cantidad; Else CREDITO = :CREDITO + (:Cantidad*-1);      suspend;  End  end^
SET TERM ; ^

GRANT EXECUTE
 ON PROCEDURE KERDEX TO SYSDBA;


SET TERM ^ ;
ALTER PROCEDURE NEW_PROCEDURE (
    IFEC_DCTO Timestamp,
    ISEC_DCTO Double precision DEFAULT  )
RETURNS (
    RTOT_PAG Double precision )
AS
begin
  /* Procedure Text */
  SELECT SUM(VAL_PGDE) FROM pgde, PGCA,cuot
  WHERE PGDE.NUM_CUOT=CUOT.num_cuot
  AND PGDE.num_pgca = PGCA.num_pgca 
  AND CUOT.SEC_DCTO= :isec_dcto
  and PGCA.fpa_pgca <= :ifec_dcto 
  INTO :rtot_pag ;
  suspend;
end^
SET TERM ; ^

GRANT EXECUTE
 ON PROCEDURE NEW_PROCEDURE TO ADMINI;

GRANT EXECUTE
 ON PROCEDURE NEW_PROCEDURE TO ELIASI;

GRANT EXECUTE
 ON PROCEDURE NEW_PROCEDURE TO IRISI;

GRANT EXECUTE
 ON PROCEDURE NEW_PROCEDURE TO KEVINI;

GRANT EXECUTE
 ON PROCEDURE NEW_PROCEDURE TO SYSDBA;


SET TERM ^ ;
ALTER PROCEDURE PRODUCTIVIDAD_DESPACHADORES (
    FECHA_DESDE Char(19),
    FECHA_HASTA Char(19),
    VALOR Integer )
RETURNS (
    NDESPACHADOR Char(40),
    GALONES Numeric(15,2),
    GALONESE Numeric(15,2),
    PORCENTAJEG Numeric(15,2),
    DESPACHOS Integer,
    DESPACHOSE Integer,
    PORCENTAJED Numeric(15,2),
    PORCENTAJEP Numeric(15,2) )
AS
declare XNDESPACHADOR Char(40);
declare XGALONES Numeric(15,4);
declare XGALONESE Numeric(15,4);
declare XPORCENTAJEG Numeric(15,2);
declare XDESPACHOS integer;
declare XDESPACHOSE integer;
declare XPORCENTAJED numeric(15,2);
declare XPORCENTAJEP numeric(15,2);
declare zHORA Char(8);
BEGIN
DELETE FROM PRODUCTIVIDAD_DESP;

SELECT substring(max(a.FCI_PUNT) from 12 for 8) from moli a
Where
a.Fci_punt >= (substring(:FECHA_DESDE from 1 for 10) || ', 00:00:00.000') and
a.Fci_punt <= (substring(:FECHA_DESDE from 1 for 10) || ', 08:59:59.999') and
a.det_moli = 'd'
into :zhora;
fecha_desde = substring(FECHA_DESDE from 1 for 10) || ' ' || zhora;
INSERT INTO PRODUCTIVIDAD_DESP (PD_NDESPACHADOR, PD_GALONES, PD_GALONESE, PD_PORCENTAJEG, PD_DESPACHOS, PD_DESPACHOSE, PD_PORCENTAJED, PD_PORCENTAJEP)
VALUES (:FECHA_DESDE, 0, 0, 0, 0, 0, 0, 100);

SELECT substring(max(a.FCI_PUNT) from 12 for 8) from moli a
Where
a.Fci_punt >= (substring(:FECHA_HASTA from 1 for 10) || ', 00:00:00.000') and
a.Fci_punt <= (substring(:FECHA_HASTA from 1 for 10) || ', 08:59:59.999') and
a.det_moli = 'd'
into :zhora;
fecha_hasta = substring(FECHA_HASTA from 1 for 10) || ' ' || zhora;
INSERT INTO PRODUCTIVIDAD_DESP (PD_NDESPACHADOR, PD_GALONES, PD_GALONESE, PD_PORCENTAJEG, PD_DESPACHOS, PD_DESPACHOSE, PD_PORCENTAJED, PD_PORCENTAJEP)
VALUES (:FECHA_HASTA, 0, 0, 0, 0, 0, 0, 99);

select sum(a.can_desp) from desp a
where ((SUBSTRING(a.fec_desp from 7 for 4) || '/' ||
SUBSTRING(a.fec_desp from 4 for 2) || '/' ||
SUBSTRING(a.fec_desp from 1 for 2) || ' ' ||
    (case
        when (substring(a.fec_desp from 12 for 2) in('0:','1:','2:','3:','4:','5:','6:','7:','8:','9:')) then '0' || substring(a.fec_desp from 12 for 7)
        when (substring(a.fec_desp from 12 for 2) in('  ')) then '00:00:00:'
            else substring(a.fec_desp from 12 for 8)
     end) >= :fecha_desde) AND
(SUBSTRING(a.fec_desp from 7 for 4) || '/' ||
SUBSTRING(a.fec_desp from 4 for 2) || '/' ||
SUBSTRING(a.fec_desp from 1 for 2) || ' ' ||
    (case
        when (substring(a.fec_desp from 12 for 2) in('0:','1:','2:','3:','4:','5:','6:','7:','8:','9:')) then '0' || substring(a.fec_desp from 12 for 7)
        when (substring(a.fec_desp from 12 for 2) in('  ')) then '00:00:00:'
            else substring(a.fec_desp from 12 for 8)
     end) <= :fecha_hasta))
into :XGALONESE;
select COUNT(*) from desp a
where ((SUBSTRING(a.fec_desp from 7 for 4) || '/' ||
SUBSTRING(a.fec_desp from 4 for 2) || '/' ||
SUBSTRING(a.fec_desp from 1 for 2) || ' ' ||
    (case
        when (substring(a.fec_desp from 12 for 2) in('0:','1:','2:','3:','4:','5:','6:','7:','8:','9:')) then '0' || substring(a.fec_desp from 12 for 7)
        when (substring(a.fec_desp from 12 for 2) in('  ')) then '00:00:00:'
            else substring(a.fec_desp from 12 for 8)
     end) >= :fecha_desde) AND
(SUBSTRING(a.fec_desp from 7 for 4) || '/' ||
SUBSTRING(a.fec_desp from 4 for 2) || '/' ||
SUBSTRING(a.fec_desp from 1 for 2) || ' ' ||
    (case
        when (substring(a.fec_desp from 12 for 2) in('0:','1:','2:','3:','4:','5:','6:','7:','8:','9:')) then '0' || substring(a.fec_desp from 12 for 7)
        when (substring(a.fec_desp from 12 for 2) in('  ')) then '00:00:00:'
            else substring(a.fec_desp from 12 for 8)
     end) <= :fecha_hasta))
into :XDESPACHOSE;
FOR select b.nom_clie, sum(a.can_desp), count(*)
FROM DESP a left join clie b on a.ven_punt = b.cod_clie and b.cpr_clie = 'D'
where ((SUBSTRING(a.fec_desp from 7 for 4) || '/' ||
SUBSTRING(a.fec_desp from 4 for 2) || '/' ||
SUBSTRING(a.fec_desp from 1 for 2) || ' ' ||
    (case
        when (substring(a.fec_desp from 12 for 2) in('0:','1:','2:','3:','4:','5:','6:','7:','8:','9:')) then '0' || substring(a.fec_desp from 12 for 7)
        when (substring(a.fec_desp from 12 for 2) in('  ')) then '00:00:00:'
            else substring(a.fec_desp from 12 for 8)
     end) >= :fecha_desde) AND
(SUBSTRING(a.fec_desp from 7 for 4) || '/' ||
SUBSTRING(a.fec_desp from 4 for 2) || '/' ||
SUBSTRING(a.fec_desp from 1 for 2) || ' ' ||
    (case
        when (substring(a.fec_desp from 12 for 2) in('0:','1:','2:','3:','4:','5:','6:','7:','8:','9:')) then '0' || substring(a.fec_desp from 12 for 7)
        when (substring(a.fec_desp from 12 for 2) in('  ')) then '00:00:00:'
            else substring(a.fec_desp from 12 for 8)
     end) <= :fecha_hasta))
group by b.nom_clie
order by b.nom_clie
INTO :XNDESPACHADOR, :XGALONES, :XDESPACHOS DO
BEGIN
      XPORCENTAJEG = round((XGALONES / XGALONESE) * 100,2);
      XPORCENTAJED = round((CAST(XDESPACHOS as numeric(15,4)) / CAST(XDESPACHOSE as numeric(15,4))) * 100,2);
      INSERT INTO PRODUCTIVIDAD_DESP (PD_NDESPACHADOR, PD_GALONES, PD_GALONESE, PD_PORCENTAJEG, PD_DESPACHOS, PD_DESPACHOSE, PD_PORCENTAJED, PD_PORCENTAJEP)
      VALUES (:XNDESPACHADOR, :XGALONES, :XGALONESE, :XPORCENTAJEG,:XDESPACHOS, :XDESPACHOSE, :XPORCENTAJED, round(((:XPORCENTAJEG + :XPORCENTAJED) / 2),2));
  End
  FOR select * FROM PRODUCTIVIDAD_DESP ORDER BY PD_PORCENTAJEP  DESC
  into :NDESPACHADOR, :GALONES, :GALONESE, :PORCENTAJEG, :DESPACHOS, :DESPACHOSE, :PORCENTAJED, :PORCENTAJEP DO
  BEGIN
    suspend;
  End
END^
SET TERM ; ^

GRANT EXECUTE
 ON PROCEDURE PRODUCTIVIDAD_DESPACHADORES TO SYSDBA;


SET TERM ^ ;
ALTER PROCEDURE PRODUCTIVIDAD_TRIMESTRAL (
    FECHA_DESDE Char(19),
    FECHA_DESDE2 Char(19),
    FECHA_HASTA Char(19),
    FECHA_HASTA2 Char(19),
    VALOR Integer )
RETURNS (
    NDESPACHADOR Char(40),
    GALONES Numeric(15,2),
    GALONESE Numeric(15,2),
    PORCENTAJEG Numeric(15,2),
    DESPACHOS Integer,
    DESPACHOSE Integer,
    PORCENTAJED Numeric(15,2),
    PORCENTAJEP Numeric(15,2) )
AS
declare XNDESPACHADOR Char(40);
declare XGALONES Numeric(15,4);
declare XGALONESE Numeric(15,4);
declare XPORCENTAJEG Numeric(15,2);            
declare XDESPACHOS integer;
declare XDESPACHOSE integer;
declare XPORCENTAJED numeric(15,2);
declare XPORCENTAJEP numeric(15,2);

declare zFECHA Char(10);
declare zHORA Char(8);

BEGIN

    DELETE FROM PRODUCTIVIDAD_DESP;
    
----

---- OBTIENE HORA DE CIERRE DEL ULTIMO DIA DEL MES ANTERIOR AL INICIO DEL SEMESTRE

    SELECT substring(max(a.FCI_PUNT) from 1 for 10), substring(max(a.FCI_PUNT) from 12 for 8) from moli a
    where 
    a.Fci_punt >= (substring(:FECHA_DESDE from 1 for 10) || ', 20:00:00.000') and
    a.Fci_punt <= (substring(:FECHA_DESDE2 from 1 for 10) || ', 00:59:59.999') and
    a.det_moli = 'd'
    into :zfecha, :zhora;
    
    fecha_desde = ZFECHA || ' ' || zhora; 

    INSERT INTO PRODUCTIVIDAD_DESP (PD_NDESPACHADOR, PD_GALONES, PD_GALONESE, PD_PORCENTAJEG, PD_DESPACHOS, PD_DESPACHOSE, PD_PORCENTAJED, PD_PORCENTAJEP)
    VALUES (:FECHA_DESDE, 0, 0, 0, 0, 0, 0, 100);

--    fecha_desde2 = substring(FECHA_DESDE2 from 1 for 10) || ' ' || zhora; 

--    INSERT INTO PRODUCTIVIDAD_DESP (PD_NDESPACHADOR, PD_GALONES, PD_GALONESE, PD_PORCENTAJEG, PD_DESPACHOS, PD_DESPACHOSE, PD_PORCENTAJED, PD_PORCENTAJEP)
--    VALUES (:FECHA_DESDE2, 0, 0, 0, 0, 0, 0, 99);

--- OBTIENE HORA DE CIERRE DEL ULTIMO DIA DEL MES FINAL DEL TRIMESTRE
 
    SELECT substring(max(a.FCI_PUNT) from 1 for 10), substring(max(a.FCI_PUNT) from 12 for 8) from moli a
    where 
    a.Fci_punt >= (substring(:FECHA_HASTA from 1 for 10) || ', 20:00:00.000') and
    a.Fci_punt <= (substring(:FECHA_HASTA2 from 1 for 10) || ', 23:59:59.999') and
    a.det_moli = 'd'
    into :zfecha, :zhora;
    
    fecha_hasta = ZFECHA || ' ' || zhora; 

    INSERT INTO PRODUCTIVIDAD_DESP (PD_NDESPACHADOR, PD_GALONES, PD_GALONESE, PD_PORCENTAJEG, PD_DESPACHOS, PD_DESPACHOSE, PD_PORCENTAJED, PD_PORCENTAJEP)
    VALUES (:FECHA_HASTA, 0, 0, 0, 0, 0, 0, 99);

--    fecha_hasta2 = substring(FECHA_HASTA2 from 1 for 10) || ' ' || zhora; 

--    INSERT INTO PRODUCTIVIDAD_DESP (PD_NDESPACHADOR, PD_GALONES, PD_GALONESE, PD_PORCENTAJEG, PD_DESPACHOS, PD_DESPACHOSE, PD_PORCENTAJED, PD_PORCENTAJEP)
--    VALUES (:FECHA_HASTA2, 0, 0, 0, 0, 0, 0, 98);

----

  
    select sum(a.can_desp) from desp a 
    where ((SUBSTRING(a.fec_desp from 7 for 4) || '-' ||
    SUBSTRING(a.fec_desp from 4 for 2) || '-' ||
    SUBSTRING(a.fec_desp from 1 for 2) || ' ' || 	
        (case
      		when (substring(a.fec_desp from 12 for 2) in('0:','1:','2:','3:','4:','5:','6:','7:','8:','9:')) then '0' || substring(a.fec_desp from 12 for 7)
      		when (substring(a.fec_desp from 12 for 2) in('  ')) then '00:00:00:'
            	else substring(a.fec_desp from 12 for 8) 
         end) >= :fecha_desde) AND
    (SUBSTRING(a.fec_desp from 7 for 4) || '-' ||
    SUBSTRING(a.fec_desp from 4 for 2) || '-' ||
    SUBSTRING(a.fec_desp from 1 for 2) || ' ' || 	
        (case
      		when (substring(a.fec_desp from 12 for 2) in('0:','1:','2:','3:','4:','5:','6:','7:','8:','9:')) then '0' || substring(a.fec_desp from 12 for 7)
      		when (substring(a.fec_desp from 12 for 2) in('  ')) then '00:00:00:'
            	else substring(a.fec_desp from 12 for 8) 
         end) <= :fecha_hasta))
    into :XGALONESE; 

    select COUNT(*) from desp a 
    where ((SUBSTRING(a.fec_desp from 7 for 4) || '-' ||
    SUBSTRING(a.fec_desp from 4 for 2) || '-' ||
    SUBSTRING(a.fec_desp from 1 for 2) || ' ' || 	
        (case
      		when (substring(a.fec_desp from 12 for 2) in('0:','1:','2:','3:','4:','5:','6:','7:','8:','9:')) then '0' || substring(a.fec_desp from 12 for 7)
      		when (substring(a.fec_desp from 12 for 2) in('  ')) then '00:00:00:'
            	else substring(a.fec_desp from 12 for 8) 
         end) >= :fecha_desde) AND
    (SUBSTRING(a.fec_desp from 7 for 4) || '-' ||
    SUBSTRING(a.fec_desp from 4 for 2) || '-' ||
    SUBSTRING(a.fec_desp from 1 for 2) || ' ' || 	
        (case
      		when (substring(a.fec_desp from 12 for 2) in('0:','1:','2:','3:','4:','5:','6:','7:','8:','9:')) then '0' || substring(a.fec_desp from 12 for 7)
      		when (substring(a.fec_desp from 12 for 2) in('  ')) then '00:00:00:'
            	else substring(a.fec_desp from 12 for 8) 
         end) <= :fecha_hasta))
    into :XDESPACHOSE; 


  FOR select b.nom_clie, sum(a.can_desp), count(*) 
    FROM DESP a left join clie b on a.ven_punt = b.cod_clie and b.cpr_clie = 'D'
    where ((SUBSTRING(a.fec_desp from 7 for 4) || '-' ||
    SUBSTRING(a.fec_desp from 4 for 2) || '-' ||
    SUBSTRING(a.fec_desp from 1 for 2) || ' ' || 	
        (case
      		when (substring(a.fec_desp from 12 for 2) in('0:','1:','2:','3:','4:','5:','6:','7:','8:','9:')) then '0' || substring(a.fec_desp from 12 for 7)
      		when (substring(a.fec_desp from 12 for 2) in('  ')) then '00:00:00:'
            	else substring(a.fec_desp from 12 for 8) 
         end) >= :fecha_desde) AND
    (SUBSTRING(a.fec_desp from 7 for 4) || '-' ||
    SUBSTRING(a.fec_desp from 4 for 2) || '-' ||
    SUBSTRING(a.fec_desp from 1 for 2) || ' ' || 	
        (case
      		when (substring(a.fec_desp from 12 for 2) in('0:','1:','2:','3:','4:','5:','6:','7:','8:','9:')) then '0' || substring(a.fec_desp from 12 for 7)
      		when (substring(a.fec_desp from 12 for 2) in('  ')) then '00:00:00:'
            	else substring(a.fec_desp from 12 for 8) 
         end) <= :fecha_hasta))
    group by b.nom_clie
    order by b.nom_clie 
   INTO :XNDESPACHADOR, :XGALONES, :XDESPACHOS DO

  BEGIN

        XPORCENTAJEG = round((XGALONES / XGALONESE) * 100,2);
        XPORCENTAJED = round((CAST(XDESPACHOS as numeric(15,4)) / CAST(XDESPACHOSE as numeric(15,4))) * 100,2);

        INSERT INTO PRODUCTIVIDAD_DESP (PD_NDESPACHADOR, PD_GALONES, PD_GALONESE, PD_PORCENTAJEG, PD_DESPACHOS, PD_DESPACHOSE, PD_PORCENTAJED, PD_PORCENTAJEP)
        VALUES (:XNDESPACHADOR, :XGALONES, :XGALONESE, :XPORCENTAJEG,:XDESPACHOS, :XDESPACHOSE, :XPORCENTAJED, round(((:XPORCENTAJEG + :XPORCENTAJED) / 2),2));

  END
  FOR select * FROM PRODUCTIVIDAD_DESP ORDER BY PD_PORCENTAJEP  DESC
  into :NDESPACHADOR, :GALONES, :GALONESE, :PORCENTAJEG, :DESPACHOS, :DESPACHOSE, :PORCENTAJED, :PORCENTAJEP DO
  BEGIN 
    suspend;
  END 


END^
SET TERM ; ^

GRANT EXECUTE
 ON PROCEDURE PRODUCTIVIDAD_TRIMESTRAL TO SYSDBA;


SET TERM ^ ;
ALTER PROCEDURE PROREP_VENT (
    ICDE_VEND Char(9) DEFAULT ,
    ICHA_VEND Char(9) DEFAULT ZZZZ',
    IFDE_DCTO Timestamp DEFAULT /2012',
    IFHA_DCTO Timestamp DEFAULT /2012 23:59:59',
    ICDE_CLIE Char(9) DEFAULT ,
    ICHA_CLIE Char(9) DEFAULT ZZZZ',
    ICDE_PAGO Char(3) DEFAULT ,
    ICHA_PAGO Char(3) DEFAULT ,
    ICDE_DCTO Char(2) DEFAULT ,
    ICHA_DCTO Char(2) DEFAULT  )
RETURNS (
    RSEC_DCTO Double precision,
    RTIP_DCTO Char(10),
    RFEC_DCTO Timestamp,
    RDET_DCTO Char(250),
    RTNI_DCTO Double precision,
    RTSI_DCTO Double precision,
    RIVA_DCTO Double precision,
    RCOD_VEND Char(9),
    RCOD_CLIE Char(9),
    RNOM_CLIE Char(60) )
AS
begin
  for select
     DCTO.sec_dcto,tip_dcto,fec_dcto,det_dcto,tni_dcto,tsi_dcto,iva_dcto,
     cod_vend,CLIE.cod_clie,nom_clie
     from dcto,clie where dcto.cod_clie=clie.cod_clie
     and fec_dcto >= :ifde_dcto and fec_dcto <= :ifha_dcto
     AND cod_vend>= :icde_vend and cod_vend<=:icha_vend
     AND dcto.cod_clie>= :icde_clie and dcto.cod_clie<=:icha_clie
     AND cod_pago>= :icde_pago and cod_pago<=:icha_pago
     AND TRIM(col_dcto)>= :icde_dcto and trim(col_dcto)<=:icha_dcto 
     order by tip_dcto,dcto.cod_clie,sec_dcto
     INTO :rSEC_DCTO,:rtip_dcto, :rFEC_DCTO,:rDET_DCTO,:rTNI_DCTO,:rTSI_DCTO,:rIVA_DCTO,
     :rCOD_VEND,:rCOD_CLIE,:rNOM_CLIE do
    begin
        suspend;
    end

  /* Procedure Text */
end^
SET TERM ; ^

GRANT EXECUTE
 ON PROCEDURE PROREP_VENT TO ADMINI;

GRANT EXECUTE
 ON PROCEDURE PROREP_VENT TO ELIASI;

GRANT EXECUTE
 ON PROCEDURE PROREP_VENT TO IRISI;

GRANT EXECUTE
 ON PROCEDURE PROREP_VENT TO KEVINI;

GRANT EXECUTE
 ON PROCEDURE PROREP_VENT TO SYSDBA;


SET TERM ^ ;
ALTER PROCEDURE PRO_DCTO_CLIE (
    ISEC_DCTO Double precision DEFAULT  )
RETURNS (
    RSEC_DCTO Double precision,
    RNUM_DCTO Char(20),
    RDET_DCTO Char(250),
    RCOL_DCTO Char(50),
    RFEC_DCTO Timestamp,
    RCOD_PAGO Char(3),
    RCOD_CLIE Char(9),
    RNOM_CLIE Char(60),
    RRUC_CLIE Char(13),
    RDCA_CLIE Char(50),
    RTE1_CLIE Char(9),
    RTNI_DCTO Double precision,
    RTSI_DCTO Double precision,
    RDSC_DCTO Double precision,
    RIVA_DCTO Double precision,
    RCOD_BODE Char(2),
    RCOD_ACTI Char(2),
    RCOD_VEND Char(9) )
AS
begin
  /* Procedure Text */
  FOR SELECT
  SEC_DCTO,NUM_DCTO,DET_DCTO,COL_DCTO,FEC_DCTO,COD_PAGO,
  CLIE.COD_CLIE,NOM_CLIE,RUC_CLIE,DCA_CLIE,TE1_CLIE,
  TNI_DCTO,TSI_DCTO,DSC_DCTO,IVA_DCTO,COD_BODE,COD_ACTI,
  COD_VEND FROM dcto, clie WHERE DCTO.COD_CLIE=CLIE.cod_clie AND SEC_DCTO=:isec_dcto 
  INTO
  :RSEC_DCTO, :RNUM_DCTO, :RDET_DCTO, :RCOL_DCTO, :RFEC_DCTO, :RCOD_PAGO,
  :RCOD_CLIE, :RNOM_CLIE, :RRUC_CLIE, :RDCA_CLIE, :RTE1_CLIE,
  :RTNI_DCTO, :RTSI_DCTO, :RDSC_DCTO, :RIVA_DCTO, :RCOD_BODE, :RCOD_ACTI,
  :rcod_vend DO
  BEGIN
  suspend;
  end 
end^
SET TERM ; ^

GRANT EXECUTE
 ON PROCEDURE PRO_DCTO_CLIE TO ADMINI;

GRANT EXECUTE
 ON PROCEDURE PRO_DCTO_CLIE TO ELIASI;

GRANT EXECUTE
 ON PROCEDURE PRO_DCTO_CLIE TO IRISI;

GRANT EXECUTE
 ON PROCEDURE PRO_DCTO_CLIE TO KEVINI;

GRANT EXECUTE
 ON PROCEDURE PRO_DCTO_CLIE TO SYSDBA;


SET TERM ^ ;
ALTER PROCEDURE PRO_MOVI_ITEM (
    ISEC_DCTO Double precision )
RETURNS (
    RCOD_ITEM Char(100),
    RNOM_ITEM Char(100),
    RCAN_MOVI Double precision,
    RVAL_MOVI Double precision,
    RDSC_MOVI Double precision,
    RTOT_MOVI Double precision,
    RIVA_MOVI Char(1),
    RBEM_MOVI Char(1),
    RTVE_MOVI Char(1),
    RSEC_DCTO Double precision )
AS
begin
  /* Procedure Text */
  FOR SELECT MOVI.COD_ITEM,ITEM.NOM_ITEM,CAN_MOVI,VAL_MOVI,DSC_MOVI,TOT_MOVI,IVA_MOVI,TVE_MOVI, BEM_MOVI,SEC_DCTO
  FROM
  movi, item WHERE MOVI.SEC_DCTO=:isec_dcto AND TRIM(MOVI.cod_item) = TRIM(ITEM.cod_item)
  INTO
  :rcod_item, :rnom_item, :rcan_movi, :rval_movi, :rdsc_movi, :rtot_movi, :riva_movi, :rtve_movi, :rbem_movi ,:rsec_dcto
  DO
  begin
  suspend;
  end 
end^
SET TERM ; ^

GRANT EXECUTE
 ON PROCEDURE PRO_MOVI_ITEM TO ADMINI;

GRANT EXECUTE
 ON PROCEDURE PRO_MOVI_ITEM TO ELIASI;

GRANT EXECUTE
 ON PROCEDURE PRO_MOVI_ITEM TO IRISI;

GRANT EXECUTE
 ON PROCEDURE PRO_MOVI_ITEM TO KEVINI;

GRANT EXECUTE
 ON PROCEDURE PRO_MOVI_ITEM TO SYSDBA;


SET TERM ^ ;
ALTER PROCEDURE PRO_REP_VENT (
    ICDE_VEND Char(9) DEFAULT ,
    ICHA_VEND Char(9) DEFAULT ZZZZ',
    IFDE_DCTO Timestamp DEFAULT /2012',
    IFHA_DCTO Timestamp DEFAULT /2012 23:59:59',
    ICDE_CLIE Char(9) DEFAULT ,
    ICHA_CLIE Char(9) DEFAULT ZZZZ',
    ICDE_PAGO Char(3) DEFAULT ,
    ICHA_PAGO Char(3) DEFAULT  )
RETURNS (
    RSEC_DCTO Double precision,
    RTIP_DCTO Char(10),
    RFEC_DCTO Timestamp,
    RDET_DCTO Char(250),
    RCAN_MOVI Double precision,
    RVAL_MOVI Double precision,
    RTNI_DCTO Double precision,
    RTSI_DCTO Double precision,
    RIVA_DCTO Double precision,
    RCOD_VEND Char(9),
    RCOD_CLIE Char(9),
    RNOM_CLIE Char(60),
    RCOD_ITEM Char(100) )
AS
declare variable asec_dcto double precision;
declare variable afec_dcto timestamp;
declare variable adet_dcto char(250);
declare variable acan_movi double precision;
declare variable aval_movi double precision;
declare variable atni_dcto double precision;
declare variable atsi_dcto double precision;
declare variable aiva_dcto double precision;
declare variable acod_vend char(9);
declare variable acod_clie char(9);
declare variable anom_clie char(60);
declare variable acod_item char(100);
declare variable afil_cod_vend char(100) = '';
begin
  for select
     DCTO.sec_dcto,tip_dcto,fec_dcto,det_dcto,tni_dcto,tsi_dcto,iva_dcto,
     cod_vend,CLIE.cod_clie,nom_clie
     from dcto,clie where dcto.cod_clie=clie.cod_clie
     and fec_dcto >= :ifde_dcto and fec_dcto <= :ifha_dcto
     AND cod_vend>= :icde_vend and cod_vend<=:icha_vend
     AND dcto.cod_clie>= :icde_clie and dcto.cod_clie<=:icha_clie
     AND cod_pago>= :icde_pago and cod_pago<=:icha_pago
     order by tip_dcto,dcto.cod_clie,sec_dcto
     INTO :rSEC_DCTO,:rtip_dcto, :rFEC_DCTO,:rDET_DCTO,:rTNI_DCTO,:rTSI_DCTO,:rIVA_DCTO,
     :rCOD_VEND,:rCOD_CLIE,:rNOM_CLIE do
    begin
        suspend;
    end

  /* Procedure Text */
end^
SET TERM ; ^

GRANT EXECUTE
 ON PROCEDURE PRO_REP_VENT TO ADMINI;

GRANT EXECUTE
 ON PROCEDURE PRO_REP_VENT TO ELIASI;

GRANT EXECUTE
 ON PROCEDURE PRO_REP_VENT TO IRISI;

GRANT EXECUTE
 ON PROCEDURE PRO_REP_VENT TO KEVINI;

GRANT EXECUTE
 ON PROCEDURE PRO_REP_VENT TO SYSDBA;


SET TERM ^ ;
ALTER PROCEDURE PRO_SAL_DCTO (
    IFEC_DCTO Timestamp,
    ISEC_DCTO Double precision DEFAULT  )
RETURNS (
    RTOT_PAG Double precision,
    RFUP_PAG Timestamp )
AS
begin
  /* Procedure Text */
  SELECT SUM(VAL_PGDE),MAX(PGCA.fpa_pgca) FROM pgde, PGCA,cuot
  WHERE PGDE.NUM_CUOT=CUOT.num_cuot
  AND PGDE.num_pgca = PGCA.num_pgca 
  AND CUOT.SEC_DCTO= :isec_dcto
  and PGCA.fpa_pgca <= :ifec_dcto 
  INTO :rtot_pag, :rfup_pag  ;
  suspend;
end^
SET TERM ; ^

GRANT EXECUTE
 ON PROCEDURE PRO_SAL_DCTO TO ADMINI;

GRANT EXECUTE
 ON PROCEDURE PRO_SAL_DCTO TO ELIASI;

GRANT EXECUTE
 ON PROCEDURE PRO_SAL_DCTO TO IRISI;

GRANT EXECUTE
 ON PROCEDURE PRO_SAL_DCTO TO KEVINI;

GRANT EXECUTE
 ON PROCEDURE PRO_SAL_DCTO TO SYSDBA;


SET TERM ^ ;
ALTER PROCEDURE PRO_SAL_DCTOMENOR (
    IFEC_DCTO Timestamp,
    ISEC_DCTO Double precision DEFAULT  )
RETURNS (
    RTOT_PAG Double precision )
AS
begin
  /* Procedure Text */
  SELECT SUM(VAL_PGDE) FROM pgde, PGCA,cuot
  WHERE PGDE.NUM_CUOT=CUOT.num_cuot
  AND PGDE.num_pgca = PGCA.num_pgca 
  AND CUOT.SEC_DCTO= :isec_dcto
  and PGCA.fpa_pgca < :ifec_dcto
  INTO :rtot_pag ;
  suspend;
end^
SET TERM ; ^

GRANT EXECUTE
 ON PROCEDURE PRO_SAL_DCTOMENOR TO ADMINI;

GRANT EXECUTE
 ON PROCEDURE PRO_SAL_DCTOMENOR TO ELIASI;

GRANT EXECUTE
 ON PROCEDURE PRO_SAL_DCTOMENOR TO IRISI;

GRANT EXECUTE
 ON PROCEDURE PRO_SAL_DCTOMENOR TO KEVINI;

GRANT EXECUTE
 ON PROCEDURE PRO_SAL_DCTOMENOR TO SYSDBA;


SET TERM ^ ;
ALTER PROCEDURE PRO_ULT_VENT (
    INUM_REGI Integer )
RETURNS (
    RSEC_DCTO Double precision,
    RFEC_DCTO Timestamp,
    RDET_DCTO Char(250),
    RCAN_MOVI Double precision,
    RVAL_MOVI Double precision,
    RTNI_DCTO Double precision,
    RTSI_DCTO Double precision,
    RIVA_DCTO Double precision,
    RCOD_VEND Char(9),
    RCOD_CLIE Char(9),
    RNOM_CLIE Char(60),
    RCOD_ITEM Char(100) )
AS
declare variable asec_dcto double precision;
declare variable afec_dcto timestamp;
declare variable adet_dcto char(250);
declare variable acan_movi double precision;
declare variable aval_movi double precision;
declare variable atni_dcto double precision;
declare variable atsi_dcto double precision;
declare variable aiva_dcto double precision;
declare variable acod_vend char(9);
declare variable acod_clie char(9);
declare variable anom_clie char(60);
declare variable acod_item char(100);
begin
  for select
     DCTO.sec_dcto,fec_dcto,det_dcto,can_movi,val_movi,tni_dcto,tsi_dcto,iva_dcto,
     cod_vend,CLIE.cod_clie,nom_clie,cod_item
     from movi,dcto,clie where movi.sec_dcto=dcto.sec_dcto and dcto.cod_clie=clie.cod_clie
     order by fec_dcto descending
     INTO :aSEC_DCTO,:aFEC_DCTO,:aDET_DCTO,:aCAN_MOVI,:aVAL_MOVI,:aTNI_DCTO,:aTSI_DCTO,:aIVA_DCTO,
     :aCOD_VEND,:aCOD_CLIE,:aNOM_CLIE,:aCOD_ITEM do
    begin
        suspend;
    end

  /* Procedure Text */
end^
SET TERM ; ^

GRANT EXECUTE
 ON PROCEDURE PRO_ULT_VENT TO ADMINI;

GRANT EXECUTE
 ON PROCEDURE PRO_ULT_VENT TO ELIASI;

GRANT EXECUTE
 ON PROCEDURE PRO_ULT_VENT TO IRISI;

GRANT EXECUTE
 ON PROCEDURE PRO_ULT_VENT TO KEVINI;

GRANT EXECUTE
 ON PROCEDURE PRO_ULT_VENT TO SYSDBA;


SET TERM ^ ;
ALTER PROCEDURE XPERM (
    TCNOMBREUSUARIO Varchar(31) )
AS
declare variable tcobjeto varchar(31); BEGIN FOR SELECT RDB$RELATION_NAME From RDB$RELATIONS where RDB$SYSTEM_FLAG = 0    Into  :tcObjeto    Do       Execute STATEMENT 'GRANT ALL ON ' || tcObjeto || ' TO ' || tcNombreUsuario;    FOR SELECT       RDB$PROCEDURE_NAME    From       RDB$Procedures    Where       RDB$SYSTEM_FLAG = 0    Into  :tcObjeto    Do       Execute STATEMENT 'GRANT EXECUTE ON PROCEDURE ' || tcObjeto || ' TO ' || tcNombreUsuario; End^
SET TERM ; ^

GRANT EXECUTE
 ON PROCEDURE XPERM TO ADMINI;

GRANT EXECUTE
 ON PROCEDURE XPERM TO ELIASI;

GRANT EXECUTE
 ON PROCEDURE XPERM TO IRISI;

GRANT EXECUTE
 ON PROCEDURE XPERM TO KEVINI;

GRANT EXECUTE
 ON PROCEDURE XPERM TO SYSDBA;


GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON ACCE TO ADMINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON ACCE TO ELIASI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON ACCE TO IRISI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON ACCE TO KEVINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON ACCE TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON ACFI TO ADMINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON ACFI TO ELIASI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON ACFI TO IRISI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON ACFI TO KEVINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON ACFI TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON ACTI TO ADMINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON ACTI TO ELIASI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON ACTI TO IRISI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON ACTI TO KEVINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON ACTI TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON ADDE TO ADMINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON ADDE TO ELIASI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON ADDE TO IRISI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON ADDE TO KEVINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON ADDE TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON ADDI TO ADMINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON ADDI TO ELIASI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON ADDI TO IRISI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON ADDI TO KEVINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON ADDI TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON AHOR TO ADMINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON AHOR TO ELIASI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON AHOR TO IRISI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON AHOR TO KEVINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON AHOR TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON AHOR_MOVI TO ADMINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON AHOR_MOVI TO ELIASI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON AHOR_MOVI TO IRISI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON AHOR_MOVI TO KEVINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON AHOR_MOVI TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON AHOR_TIPO TO ADMINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON AHOR_TIPO TO ELIASI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON AHOR_TIPO TO IRISI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON AHOR_TIPO TO KEVINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON AHOR_TIPO TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON ANUL TO ADMINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON ANUL TO ELIASI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON ANUL TO IRISI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON ANUL TO KEVINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON ANUL TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON ARCH TO ADMINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON ARCH TO ELIASI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON ARCH TO IRISI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON ARCH TO KEVINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON ARCH TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON ARCH_CON TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON ARCH_CUA TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON ARCH_EST TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON ARCH_MOV TO ADMINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON ARCH_MOV TO ELIASI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON ARCH_MOV TO IRISI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON ARCH_MOV TO KEVINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON ARCH_MOV TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON BANC TO ADMINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON BANC TO ELIASI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON BANC TO IRISI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON BANC TO KEVINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON BANC TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON BANC_PEND TO SYSDBA WITH GRANT OPTION;

ALTER TABLE BODE ADD CONSTRAINT FK_BODE_1
  FOREIGN KEY (COD_ACTI) REFERENCES ACTI (COD_ACTI) ON UPDATE NO ACTION ON DELETE NO ACTION;
GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON BODE TO ADMINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON BODE TO ELIASI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON BODE TO IRISI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON BODE TO KEVINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON BODE TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON BODE_MIGR TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON BORR TO ADMINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON BORR TO ELIASI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON BORR TO IRISI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON BORR TO KEVINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON BORR TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON CHEQ TO ADMINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON CHEQ TO ELIASI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON CHEQ TO IRISI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON CHEQ TO KEVINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON CHEQ TO SYSDBA WITH GRANT OPTION;

ALTER TABLE CHEQ_MOV ADD CONSTRAINT FK_CHEQ_MOV_1
  FOREIGN KEY (NUM_CHEQ) REFERENCES CHEQ (NUM_CHEQ) ON UPDATE NO ACTION ON DELETE NO ACTION;
GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON CHEQ_MOV TO ADMINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON CHEQ_MOV TO ELIASI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON CHEQ_MOV TO IRISI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON CHEQ_MOV TO KEVINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON CHEQ_MOV TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON CICA TO ADMINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON CICA TO ELIASI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON CICA TO IRISI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON CICA TO KEVINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON CICA TO SYSDBA WITH GRANT OPTION;

ALTER TABLE CIUD ADD CONSTRAINT FK_CIUD_1
  FOREIGN KEY (COD_PROV) REFERENCES PROV (COD_PROV);
GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON CIUD TO ADMINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON CIUD TO ELIASI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON CIUD TO IRISI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON CIUD TO KEVINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON CIUD TO SYSDBA WITH GRANT OPTION;

CREATE INDEX IDX_CLAC1 ON CLAC (COD_CLIE);
CREATE INDEX IDX_CLAC2 ON CLAC (COD_CUEN);
CREATE INDEX IDX_CLAC3 ON CLAC (COD_MODU);
GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON CLAC TO ADMINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON CLAC TO ELIASI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON CLAC TO IRISI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON CLAC TO KEVINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON CLAC TO SYSDBA WITH GRANT OPTION;

CREATE INDEX IDX_CLIE1 ON CLIE (TID_CLIE);
CREATE INDEX IDX_CLIE2 ON CLIE (RUC_CLIE);
CREATE INDEX IDX_CLIE3 ON CLIE (NOM_CLIE);
CREATE INDEX IDX_CLIE4 ON CLIE (CPR_CLIE);
CREATE INDEX IDX_CLIE5 ON CLIE (APE_CLIE);
GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON CLIE TO ADMINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON CLIE TO ELIASI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON CLIE TO IRISI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON CLIE TO KEVINI;

GRANT SELECT
 ON CLIE TO PROCEDUREPROREP_VENT;

GRANT SELECT
 ON CLIE TO PROCEDUREPRO_REP_VENT;

GRANT SELECT
 ON CLIE TO PROCEDUREPRO_ULT_VENT;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON CLIE TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON CLIE_CONTR TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON CLIE_CONTR_DET TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON CLIE_CONT_MENSUAL TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON CLIE_CUMO TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON CLIE_CUPO TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON CLIE_DEST TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON CLIE_MENSUAL TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON CLIE_PLAN TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON CLIE_PLAN_CONT TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON CLIE_PRESET TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON COBA TO ADMINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON COBA TO ELIASI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON COBA TO IRISI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON COBA TO KEVINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON COBA TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON COMI TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON COMP TO ADMINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON COMP TO ELIASI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON COMP TO IRISI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON COMP TO KEVINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON COMP TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON COMP1 TO ADMINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON COMP1 TO ELIASI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON COMP1 TO IRISI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON COMP1 TO KEVINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON COMP1 TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON CONS TO ADMINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON CONS TO ELIASI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON CONS TO IRISI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON CONS TO KEVINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON CONS TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON CONS_MOV TO ADMINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON CONS_MOV TO ELIASI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON CONS_MOV TO IRISI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON CONS_MOV TO KEVINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON CONS_MOV TO SYSDBA WITH GRANT OPTION;

ALTER TABLE CON_MOVI ADD CONSTRAINT FK_CON_MOVI_1
  FOREIGN KEY (NUMMCOMP) REFERENCES COMP (NUMMCOMP);
GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON CON_MOVI TO ADMINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON CON_MOVI TO ELIASI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON CON_MOVI TO IRISI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON CON_MOVI TO KEVINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON CON_MOVI TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON CON_MOVIP TO ADMINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON CON_MOVIP TO ELIASI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON CON_MOVIP TO IRISI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON CON_MOVIP TO KEVINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON CON_MOVIP TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON CON_MOVI_PGDE TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON CON_PEND TO ADMINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON CON_PEND TO ELIASI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON CON_PEND TO IRISI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON CON_PEND TO KEVINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON CON_PEND TO SYSDBA WITH GRANT OPTION;

CREATE INDEX COSM_IDX1 ON COSM (COD_ITEM,MES_COSM);
GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON COSM TO ADMINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON COSM TO ELIASI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON COSM TO IRISI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON COSM TO KEVINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON COSM TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON COST TO ADMINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON COST TO ELIASI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON COST TO IRISI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON COST TO KEVINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON COST TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON CRED TO ADMINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON CRED TO ELIASI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON CRED TO IRISI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON CRED TO KEVINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON CRED TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON CRED_ADDE TO ADMINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON CRED_ADDE TO ELIASI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON CRED_ADDE TO IRISI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON CRED_ADDE TO KEVINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON CRED_ADDE TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON CRED_CABE TO ADMINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON CRED_CABE TO ELIASI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON CRED_CABE TO IRISI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON CRED_CABE TO KEVINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON CRED_CABE TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON CRED_DECR TO ADMINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON CRED_DECR TO ELIASI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON CRED_DECR TO IRISI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON CRED_DECR TO KEVINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON CRED_DECR TO SYSDBA WITH GRANT OPTION;

CREATE UNIQUE INDEX COD_CRED ON CRED_MOVI (COD_MOVI);
CREATE UNIQUE INDEX COD_MOVI ON CRED_MOVI (COD_MOVI);
GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON CRED_MOVI TO ADMINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON CRED_MOVI TO ELIASI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON CRED_MOVI TO IRISI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON CRED_MOVI TO KEVINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON CRED_MOVI TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON CRED_TIPO TO ADMINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON CRED_TIPO TO ELIASI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON CRED_TIPO TO IRISI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON CRED_TIPO TO KEVINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON CRED_TIPO TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON CUEN TO ADMINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON CUEN TO ELIASI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON CUEN TO IRISI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON CUEN TO KEVINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON CUEN TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON CUOT TO ADMINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON CUOT TO ELIASI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON CUOT TO IRISI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON CUOT TO KEVINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON CUOT TO SYSDBA WITH GRANT OPTION;

ALTER TABLE DCTO ADD CONSTRAINT FK_DCTO_1
  FOREIGN KEY (COD_ACTI) REFERENCES ACTI (COD_ACTI) ON UPDATE NO ACTION ON DELETE NO ACTION;
CREATE INDEX DCTO_IDX1 ON DCTO (FEC_DCTO);
CREATE INDEX DCTO_IDX2 ON DCTO (COD_CLIE);
CREATE INDEX IDX_DCTO1 ON DCTO (ESE_DCTO);
CREATE INDEX IDX_DCTO10 ON DCTO (CLE_DCTO);
CREATE DESCENDING INDEX IDX_DCTO11 ON DCTO (FEC_DCTO);
CREATE INDEX IDX_DCTO2 ON DCTO (TIP_DCTO);
CREATE INDEX IDX_DCTO3 ON DCTO (NUM_DCTO);
CREATE INDEX IDX_DCTO4 ON DCTO (NUMMCOMP);
CREATE INDEX IDX_DCTO5 ON DCTO (COD_BODE);
CREATE INDEX IDX_DCTO6 ON DCTO (DET_DCTO);
CREATE INDEX IDX_DCTO7 ON DCTO (COD_VEND);
CREATE INDEX IDX_DCTO8 ON DCTO (COD_PAGO);
CREATE INDEX IDX_DCTO9 ON DCTO (ESE_DCTO);
GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON DCTO TO ADMINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON DCTO TO ELIASI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON DCTO TO IRISI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON DCTO TO KEVINI;

GRANT SELECT
 ON DCTO TO PROCEDUREPROREP_VENT;

GRANT SELECT
 ON DCTO TO PROCEDUREPRO_REP_VENT;

GRANT SELECT
 ON DCTO TO PROCEDUREPRO_ULT_VENT;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON DCTO TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON DCTO_DEV TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON DEPA TO ADMINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON DEPA TO ELIASI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON DEPA TO IRISI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON DEPA TO KEVINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON DEPA TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON DEPR TO ADMINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON DEPR TO ELIASI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON DEPR TO IRISI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON DEPR TO KEVINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON DEPR TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON DEP_MOVI TO ADMINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON DEP_MOVI TO ELIASI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON DEP_MOVI TO IRISI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON DEP_MOVI TO KEVINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON DEP_MOVI TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON DESP TO ADMINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON DESP TO ELIASI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON DESP TO IRISI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON DESP TO KEVINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON DESP TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON DET_INVFOTOS TO ADMINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON DET_INVFOTOS TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON DET_INVINFPRODUCTOS TO ADMINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON DET_INVINFPRODUCTOS TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON DET_INVINFTURNOS TO ADMINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON DET_INVINFTURNOS TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON DET_INVLIQUIDACION TO ADMINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON DET_INVLIQUIDACION TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON DET_MICROINFTURNOS TO ADMINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON DET_MICROINFTURNOS TO SYSDBA WITH GRANT OPTION;

UPDATE RDB$RELATION_FIELDS set RDB$DESCRIPTION = '1 PRUEBA 2 PRODUCCION'  where RDB$FIELD_NAME = 'TIP_ELES' and RDB$RELATION_NAME = 'ELES';
ALTER TABLE ELES ADD CONSTRAINT FK_ELES_1
  FOREIGN KEY (COD_EMPR) REFERENCES EMPR (COD_EMPR);
GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON ELES TO ADMINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON ELES TO ELIASI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON ELES TO IRISI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON ELES TO KEVINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON ELES TO SYSDBA WITH GRANT OPTION;

ALTER TABLE ELPU ADD CONSTRAINT FK_ELPU_1
  FOREIGN KEY (EST_ELES) REFERENCES ELES (EST_ELES);
GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON ELPU TO ADMINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON ELPU TO ELIASI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON ELPU TO IRISI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON ELPU TO KEVINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON ELPU TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON EMPL TO ADMINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON EMPL TO ELIASI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON EMPL TO IRISI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON EMPL TO KEVINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON EMPL TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON EMPL_PERS TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON EMPR TO ADMINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON EMPR TO ELIASI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON EMPR TO IRISI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON EMPR TO KEVINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON EMPR TO SYSDBA WITH GRANT OPTION;

CREATE UNIQUE INDEX ESTA_IDX1 ON ESTA (EST_ESTA,MES_ESTA);
GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON ESTA TO ADMINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON ESTA TO ELIASI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON ESTA TO IRISI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON ESTA TO KEVINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON ESTA TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON ESTR TO ADMINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON ESTR TO ELIASI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON ESTR TO IRISI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON ESTR TO KEVINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON ESTR TO SYSDBA WITH GRANT OPTION;

ALTER TABLE ESTR_MOV ADD CONSTRAINT FK_ESTR_MOV_1
  FOREIGN KEY (COD_ESTR) REFERENCES ESTR (COD_ESTR);
GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON ESTR_MOV TO ADMINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON ESTR_MOV TO ELIASI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON ESTR_MOV TO IRISI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON ESTR_MOV TO KEVINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON ESTR_MOV TO SYSDBA WITH GRANT OPTION;

ALTER TABLE ESTR_VEN ADD CONSTRAINT FK_ESTR_VEN_1
  FOREIGN KEY (COD_ESTR) REFERENCES ESTR (COD_ESTR);
GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON ESTR_VEN TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON F104 TO ADMINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON F104 TO ELIASI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON F104 TO IRISI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON F104 TO KEVINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON F104 TO SYSDBA WITH GRANT OPTION;

ALTER TABLE FACT ADD CONSTRAINT FK_FACT_1
  FOREIGN KEY (COD_CLIE) REFERENCES CLIE (COD_CLIE);
CREATE UNIQUE INDEX FACT_IDX1 ON FACT (COD_CLIE,MES_FACT);
GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON FACT TO ADMINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON FACT TO ELIASI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON FACT TO IRISI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON FACT TO KEVINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON FACT TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON FOTO TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON GRUP TO ADMINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON GRUP TO ELIASI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON GRUP TO IRISI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON GRUP TO KEVINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON GRUP TO SYSDBA WITH GRANT OPTION;

ALTER TABLE IAIR ADD CONSTRAINT FK_IAIR_1
  FOREIGN KEY (NUMTL) REFERENCES TLMM (NUM_TLMM) ON DELETE CASCADE;
GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON IAIR TO ADMINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON IAIR TO ELIASI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON IAIR TO IRISI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON IAIR TO KEVINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON IAIR TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON IAIRV TO ADMINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON IAIRV TO ELIASI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON IAIRV TO IRISI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON IAIRV TO KEVINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON IAIRV TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON IICE TO ADMINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON IICE TO ELIASI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON IICE TO IRISI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON IICE TO KEVINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON IICE TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON IMPR TO ADMINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON IMPR TO ELIASI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON IMPR TO IRISI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON IMPR TO KEVINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON IMPR TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON IMPU TO ADMINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON IMPU TO ELIASI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON IMPU TO IRISI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON IMPU TO KEVINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON IMPU TO SYSDBA WITH GRANT OPTION;

CREATE INDEX ITEM_IDX1 ON ITEM (COD_GRUP);
GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON ITEM TO ADMINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON ITEM TO ELIASI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON ITEM TO IRISI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON ITEM TO KEVINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON ITEM TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON ITEMM TO ADMINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON ITEMM TO ELIASI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON ITEMM TO IRISI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON ITEMM TO KEVINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON ITEMM TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON ITEM_BARRA TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON ITEM_COMBO TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON ITEM_COST TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON ITEM_MAMO TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON ITEM_MARCA TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON IVA TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON IXML TO ADMINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON IXML TO ELIASI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON IXML TO IRISI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON IXML TO KEVINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON IXML TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON KEY_LOG_202409 TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON KEY_LOG_202410 TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON KEY_LOG_202411 TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON KEY_LOG_202412 TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON KEY_LOG_202501 TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON KEY_LOG_202502 TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON KEY_LOG_202503 TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON KEY_LOG_202504 TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON KEY_LOG_202505 TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON KEY_LOG_202506 TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON KEY_LOG_202507 TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON KEY_LOG_202508 TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON KEY_LOG_202509 TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON KEY_LOG_202510 TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON KEY_LOG_202511 TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON KEY_LOG_202512 TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON KEY_LOG_202601 TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON KEY_LOG_202602 TO SYSDBA WITH GRANT OPTION;

CREATE INDEX IDX_KILO1 ON KILO (SEC_DCTO);
CREATE INDEX IDX_KILO2 ON KILO (PLA_KILO);
CREATE INDEX IDX_KILO3 ON KILO (COD_CLIE);
GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON KILO TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON LICA TO ADMINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON LICA TO ELIASI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON LICA TO IRISI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON LICA TO KEVINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON LICA TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON LIMO TO ADMINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON LIMO TO ELIASI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON LIMO TO IRISI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON LIMO TO KEVINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON LIMO TO SYSDBA WITH GRANT OPTION;

CREATE INDEX IDX_LIQU1 ON LIQU (COD_CLIE);
CREATE INDEX IDX_LIQU2 ON LIQU (FEC_LIQU);
CREATE INDEX IDX_LIQU3 ON LIQU (NUMMCOMP);
CREATE INDEX IDX_LIQU4 ON LIQU (COD_PUNT);
GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON LIQU TO ADMINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON LIQU TO ELIASI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON LIQU TO IRISI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON LIQU TO KEVINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON LIQU TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON LIQU_CRED TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON LIQU_DENO TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON LIQU_DEPO TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON LIQU_DEPO_DENO TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON LIQU_GAST TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON LIQU_ITEM TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON LIQU_TANK TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON LIQU_TARJ TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON LIQU_TARJ_DET TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON LOG TO ADMINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON LOG TO ELIASI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON LOG TO IRISI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON LOG TO KEVINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON LOG TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON MANG TO ADMINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON MANG TO ELIASI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON MANG TO IRISI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON MANG TO KEVINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON MANG TO SYSDBA WITH GRANT OPTION;

ALTER TABLE MOCB ADD CONSTRAINT FK_MOCB_1
  FOREIGN KEY (NUM_COBA) REFERENCES COBA (NUM_COBA) ON DELETE CASCADE;
ALTER TABLE MOCB ADD CONSTRAINT FK_MOCB_2
  FOREIGN KEY (NUM_MOVI) REFERENCES CON_MOVI (NUM_MOVI);
GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON MOCB TO ADMINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON MOCB TO ELIASI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON MOCB TO IRISI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON MOCB TO KEVINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON MOCB TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON MOCB_PEND TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON MOCB_REC TO ADMINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON MOCB_REC TO ELIASI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON MOCB_REC TO IRISI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON MOCB_REC TO KEVINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON MOCB_REC TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON MODU TO ADMINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON MODU TO ELIASI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON MODU TO IRISI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON MODU TO KEVINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON MODU TO SYSDBA WITH GRANT OPTION;

CREATE INDEX IDX_MOLI1 ON MOLI (COD_PUNT);
CREATE INDEX IDX_MOLI2 ON MOLI (VEN_PUNT);
CREATE INDEX IDX_MOLI3 ON MOLI (NUM_LIQU);
CREATE INDEX IDX_MOLI4 ON MOLI (FEC_MOLI);
GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON MOLI TO ADMINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON MOLI TO ELIASI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON MOLI TO IRISI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON MOLI TO KEVINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON MOLI TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON MOLI_TANQ TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON MOPF TO ADMINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON MOPF TO ELIASI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON MOPF TO IRISI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON MOPF TO KEVINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON MOPF TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON MOPF_MOVI TO SYSDBA WITH GRANT OPTION;

ALTER TABLE MOVI ADD CONSTRAINT FK_MOVI_1
  FOREIGN KEY (SEC_DCTO) REFERENCES DCTO (SEC_DCTO);
CREATE INDEX MOVI_IDX2 ON MOVI (COD_ITEM);
GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON MOVI TO ADMINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON MOVI TO ELIASI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON MOVI TO IRISI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON MOVI TO KEVINI;

GRANT SELECT
 ON MOVI TO PROCEDUREPRO_ULT_VENT;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON MOVI TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON PAFA TO ADMINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON PAFA TO ELIASI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON PAFA TO IRISI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON PAFA TO KEVINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON PAFA TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON PAGO TO ADMINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON PAGO TO ELIASI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON PAGO TO IRISI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON PAGO TO KEVINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON PAGO TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON PART TO ADMINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON PART TO ELIASI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON PART TO IRISI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON PART TO KEVINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON PART TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON PEDI TO ADMINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON PEDI TO ELIASI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON PEDI TO IRISI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON PEDI TO KEVINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON PEDI TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON PEND TO ADMINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON PEND TO ELIASI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON PEND TO IRISI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON PEND TO KEVINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON PEND TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON PERI TO ADMINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON PERI TO ELIASI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON PERI TO IRISI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON PERI TO KEVINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON PERI TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON PGAT TO ADMINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON PGAT TO ELIASI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON PGAT TO IRISI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON PGAT TO KEVINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON PGAT TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON PGCA TO ADMINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON PGCA TO ELIASI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON PGCA TO IRISI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON PGCA TO KEVINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON PGCA TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON PGDE TO ADMINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON PGDE TO ELIASI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON PGDE TO IRISI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON PGDE TO KEVINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON PGDE TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON PGDV TO ADMINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON PGDV TO ELIASI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON PGDV TO IRISI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON PGDV TO KEVINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON PGDV TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON PLAC TO ADMINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON PLAC TO ELIASI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON PLAC TO IRISI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON PLAC TO KEVINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON PLAC TO SYSDBA WITH GRANT OPTION;

CREATE INDEX IDX_PLACA1 ON PLACA (RUC_PLA);
CREATE INDEX IDX_PLACA2 ON PLACA (SEC_PLA);
CREATE INDEX IDX_PLACA3 ON PLACA (BAR_PLA);
GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON PLACA TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON PLACA_BLOQ TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON PLACA_CUPO TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON PREC TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON PRES TO ADMINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON PRES TO ELIASI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON PRES TO IRISI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON PRES TO KEVINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON PRES TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON PRODUCTIVIDAD_DESP TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON PROM TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON PROM_SORT TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON PROT TO ADMINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON PROT TO ELIASI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON PROT TO IRISI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON PROT TO KEVINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON PROT TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON PROV TO ADMINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON PROV TO ELIASI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON PROV TO IRISI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON PROV TO KEVINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON PROV TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON PRUEBAS TO ADMINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON PRUEBAS TO ELIASI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON PRUEBAS TO IRISI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON PRUEBAS TO KEVINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON PRUEBAS TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON PUNT TO ADMINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON PUNT TO ELIASI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON PUNT TO IRISI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON PUNT TO KEVINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON PUNT TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON PUNTOS TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON R103 TO ADMINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON R103 TO ELIASI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON R103 TO IRISI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON R103 TO KEVINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON R103 TO SYSDBA WITH GRANT OPTION;

ALTER TABLE R103_MOV ADD CONSTRAINT FK_R103_MOV_1
  FOREIGN KEY (NUM_R103) REFERENCES R103 (NUM_R103) ON UPDATE NO ACTION ON DELETE NO ACTION;
GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON R103_MOV TO ADMINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON R103_MOV TO ELIASI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON R103_MOV TO IRISI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON R103_MOV TO KEVINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON R103_MOV TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON R104 TO ADMINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON R104 TO ELIASI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON R104 TO IRISI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON R104 TO KEVINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON R104 TO SYSDBA WITH GRANT OPTION;

ALTER TABLE R104_MOV ADD CONSTRAINT FK_R104_MOV_1
  FOREIGN KEY (NUM_R104) REFERENCES R104 (NUM_R104) ON UPDATE NO ACTION ON DELETE NO ACTION;
GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON R104_MOV TO ADMINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON R104_MOV TO ELIASI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON R104_MOV TO IRISI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON R104_MOV TO KEVINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON R104_MOV TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON RENT TO ADMINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON RENT TO ELIASI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON RENT TO IRISI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON RENT TO KEVINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON RENT TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON REPBACO TO ADMINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON REPBACO TO ELIASI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON REPBACO TO IRISI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON REPBACO TO KEVINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON REPBACO TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON REPBALCOM TO ADMINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON REPBALCOM TO ELIASI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON REPBALCOM TO IRISI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON REPBALCOM TO KEVINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON REPBALCOM TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON REPBALGEN TO ADMINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON REPBALGEN TO ELIASI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON REPBALGEN TO IRISI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON REPBALGEN TO KEVINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON REPBALGEN TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON REPCART TO ADMINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON REPCART TO ELIASI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON REPCART TO IRISI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON REPCART TO KEVINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON REPCART TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON REPCOMPC TO ADMINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON REPCOMPC TO ELIASI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON REPCOMPC TO IRISI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON REPCOMPC TO KEVINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON REPCOMPC TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON REPDESCRED TO ADMINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON REPDESCRED TO ELIASI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON REPDESCRED TO IRISI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON REPDESCRED TO KEVINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON REPDESCRED TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON REPESTCUEN TO SYSDBA WITH GRANT OPTION;

CREATE INDEX IDX_REPKARCOS231 ON REPKARCOS23 (COD_REPO);
CREATE INDEX IDX_REPKARCOS232 ON REPKARCOS23 (FEC_DCTO);
GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON REPKARCOS23 TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON REPMAYGEN TO SYSDBA WITH GRANT OPTION;

ALTER TABLE REQUE ADD CONSTRAINT FK_REQUE_1
  FOREIGN KEY (COD_CLIE) REFERENCES CLIE (COD_CLIE);
ALTER TABLE REQUE ADD CONSTRAINT FK_REQUE_4
  FOREIGN KEY (COD_USUA) REFERENCES USUA (COD_USUA);
GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON REQUE TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON RETE TO ADMINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON RETE TO ELIASI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON RETE TO IRISI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON RETE TO KEVINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON RETE TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON REUS TO ADMINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON REUS TO ELIASI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON REUS TO IRISI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON REUS TO KEVINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON REUS TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON RFID TO SYSDBA WITH GRANT OPTION;

CREATE INDEX IDX_RFID_COMB2 ON RFID_COMB (SEC_DCTO);
GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON RFID_COMB TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON RFID_KEY TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON RIVA TO ADMINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON RIVA TO ELIASI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON RIVA TO IRISI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON RIVA TO KEVINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON RIVA TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON RMAN TO ADMINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON RMAN TO ELIASI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON RMAN TO IRISI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON RMAN TO KEVINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON RMAN TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON ROLP TO ADMINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON ROLP TO ELIASI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON ROLP TO IRISI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON ROLP TO KEVINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON ROLP TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON ROL_MOV TO ADMINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON ROL_MOV TO ELIASI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON ROL_MOV TO IRISI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON ROL_MOV TO KEVINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON ROL_MOV TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON ROL_RUB TO ADMINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON ROL_RUB TO ELIASI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON ROL_RUB TO IRISI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON ROL_RUB TO KEVINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON ROL_RUB TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON RUTA TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON SECU TO ADMINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON SECU TO ELIASI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON SECU TO IRISI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON SECU TO KEVINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON SECU TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON SERI TO ADMINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON SERI TO ELIASI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON SERI TO IRISI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON SERI TO KEVINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON SERI TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON SESS TO ADMINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON SESS TO ELIASI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON SESS TO IRISI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON SESS TO KEVINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON SESS TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON SIVA TO ADMINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON SIVA TO ELIASI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON SIVA TO IRISI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON SIVA TO KEVINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON SIVA TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON SOCI TO ADMINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON SOCI TO ELIASI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON SOCI TO IRISI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON SOCI TO KEVINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON SOCI TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON SUST TO ADMINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON SUST TO ELIASI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON SUST TO IRISI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON SUST TO KEVINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON SUST TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON SYSLOCK TO ADMINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON SYSLOCK TO ELIASI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON SYSLOCK TO IRISI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON SYSLOCK TO KEVINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON SYSLOCK TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON SYSLOCKEXCE TO SYSDBA WITH GRANT OPTION;

ALTER TABLE SYSLOCKMOVI ADD CONSTRAINT FK_SYSLOCKMOVI_1
  FOREIGN KEY (NUM_SYSLOCK) REFERENCES SYSLOCK (NUM_SYSLOCK);
ALTER TABLE SYSLOCKMOVI ADD CONSTRAINT FK_SYSLOCKMOVI_2
  FOREIGN KEY (NUM_SYSMODUTIPO) REFERENCES SYSMODUTIPO (NUM_SYSMODUTIPO);
GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON SYSLOCKMOVI TO ADMINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON SYSLOCKMOVI TO ELIASI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON SYSLOCKMOVI TO IRISI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON SYSLOCKMOVI TO KEVINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON SYSLOCKMOVI TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON SYSMODU TO ADMINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON SYSMODU TO ELIASI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON SYSMODU TO IRISI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON SYSMODU TO KEVINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON SYSMODU TO SYSDBA WITH GRANT OPTION;

ALTER TABLE SYSMODUTIPO ADD CONSTRAINT FK_SYSMODUTIPO_1
  FOREIGN KEY (NUM_SYSMODU) REFERENCES SYSMODU (NUM_SYSMODU);
GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON SYSMODUTIPO TO ADMINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON SYSMODUTIPO TO ELIASI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON SYSMODUTIPO TO IRISI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON SYSMODUTIPO TO KEVINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON SYSMODUTIPO TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON TANQ TO ADMINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON TANQ TO ELIASI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON TANQ TO IRISI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON TANQ TO KEVINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON TANQ TO SYSDBA WITH GRANT OPTION;

ALTER TABLE TANQ_ING ADD CONSTRAINT FK_TANQINGTANQFK
  FOREIGN KEY (COD_TANQ) REFERENCES TANQ (COD_TANQ);
GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON TANQ_ING TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON TANQ_MOV TO ADMINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON TANQ_MOV TO ELIASI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON TANQ_MOV TO IRISI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON TANQ_MOV TO KEVINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON TANQ_MOV TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON TANQ_REPO TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON TANQ_RIND TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON TANQ_TAB TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON TICKET TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON TICKET_MOV TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON TIPC TO ADMINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON TIPC TO ELIASI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON TIPC TO IRISI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON TIPC TO KEVINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON TIPC TO SYSDBA WITH GRANT OPTION;

ALTER TABLE TLMM ADD CONSTRAINT FK_TLMM_1
  FOREIGN KEY (COD_SUST) REFERENCES SUST (COD_SUST);
ALTER TABLE TLMM ADD CONSTRAINT FK_TLMM_2
  FOREIGN KEY (COD_COMP) REFERENCES COMP1 (COD_COMP);
ALTER TABLE TLMM ADD CONSTRAINT FK_TLMM_3
  FOREIGN KEY (COD_ACTI) REFERENCES ACTI (COD_ACTI);
GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON TLMM TO ADMINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON TLMM TO ELIASI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON TLMM TO IRISI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON TLMM TO KEVINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON TLMM TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON TLVV TO ADMINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON TLVV TO ELIASI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON TLVV TO IRISI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON TLVV TO KEVINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON TLVV TO SYSDBA WITH GRANT OPTION;

CREATE INDEX TRAM_IDX1 ON TRAM (COD_USUA,NIP_TRAM,TAB_TRAM,NDO_TRAM);
GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON TRAM TO ADMINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON TRAM TO ELIASI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON TRAM TO IRISI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON TRAM TO KEVINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON TRAM TO SYSDBA WITH GRANT OPTION;

ALTER TABLE TRAM_MOV ADD CONSTRAINT FK_TRAM_MOV_1
  FOREIGN KEY (NUM_TRAM) REFERENCES TRAM (NUM_TRAM);
GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON TRAM_MOV TO ADMINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON TRAM_MOV TO ELIASI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON TRAM_MOV TO IRISI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON TRAM_MOV TO KEVINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON TRAM_MOV TO SYSDBA WITH GRANT OPTION;

CREATE INDEX TURN_IDX1 ON TURN (COD_VEND);
GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON TURN TO ADMINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON TURN TO ELIASI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON TURN TO IRISI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON TURN TO KEVINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON TURN TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON TURN_DEPO TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON TURN_TARJ TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON USUA TO ADMINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON USUA TO ELIASI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON USUA TO IRISI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON USUA TO KEVINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON USUA TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON VEND TO ADMINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON VEND TO ELIASI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON VEND TO IRISI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON VEND TO KEVINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON VEND TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON VENT TO ADMINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON VENT TO ELIASI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON VENT TO IRISI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON VENT TO KEVINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON VENT TO SYSDBA WITH GRANT OPTION;

ALTER TABLE XBANC ADD CONSTRAINT FK_XBANC_1
  FOREIGN KEY (NUM_LOG) REFERENCES LOG (NUM_LOG);
GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON XBANC TO ADMINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON XBANC TO ELIASI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON XBANC TO IRISI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON XBANC TO KEVINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON XBANC TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON XDCTO TO ADMINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON XDCTO TO ELIASI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON XDCTO TO IRISI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON XDCTO TO KEVINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON XDCTO TO SYSDBA WITH GRANT OPTION;

CREATE INDEX XPGCA_IDX1 ON XPGCA (NUM_PGCA);
CREATE INDEX XPGCA_IDX2 ON XPGCA (FEC_XOGCA);
CREATE INDEX XPGCA_IDX3 ON XPGCA (NUMMCOMP);
CREATE INDEX XPGCA_IDX4 ON XPGCA (USR_XPGCA);
GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON XPGCA TO ADMINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON XPGCA TO ELIASI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON XPGCA TO IRISI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON XPGCA TO KEVINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON XPGCA TO SYSDBA WITH GRANT OPTION;

CREATE INDEX XPGDE_IDX1 ON XPGDE (NUM_PGCA);
CREATE INDEX XPGDE_IDX2 ON XPGDE (NUM_PGDE);
CREATE INDEX XPGDE_IDX3 ON XPGDE (USR_XPGDE);
CREATE INDEX XPGDE_IDX4 ON XPGDE (FEC_XOGDE);
GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON XPGDE TO ADMINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON XPGDE TO ELIASI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON XPGDE TO IRISI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON XPGDE TO KEVINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON XPGDE TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON ZONA TO ADMINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON ZONA TO ELIASI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON ZONA TO IRISI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON ZONA TO KEVINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON ZONA TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON ZONA_DIST TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON CARTERA TO ADMINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON CARTERA TO ELIASI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON CARTERA TO IRISI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON CARTERA TO KEVINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON CARTERA TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON CARTERAA TO ADMINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON CARTERAA TO ELIASI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON CARTERAA TO IRISI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON CARTERAA TO KEVINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON CARTERAA TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON CARTERAEB TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON CARTERAR TO ADMINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON CARTERAR TO ELIASI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON CARTERAR TO IRISI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON CARTERAR TO KEVINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON CARTERAR TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON CHOFER TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON CLIEBODE TO ADMINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON CLIEBODE TO ELIASI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON CLIEBODE TO IRISI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON CLIEBODE TO KEVINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON CLIEBODE TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON CLIEREPE TO ADMINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON CLIEREPE TO ELIASI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON CLIEREPE TO IRISI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON CLIEREPE TO KEVINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON CLIEREPE TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON COMPRASBASEIVA TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON CONCILIA5 TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON DCTOV TO ADMINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON DCTOV TO ELIASI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON DCTOV TO IRISI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON DCTOV TO KEVINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON DCTOV TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON MANGV TO ADMINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON MANGV TO ELIASI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON MANGV TO IRISI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON MANGV TO KEVINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON MANGV TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON MOLI_VISTA TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON STOCK TO ADMINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON STOCK TO ELIASI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON STOCK TO IRISI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON STOCK TO KEVINI;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON STOCK TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON VDESP TO SYSDBA WITH GRANT OPTION;

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE
 ON VENDESP TO SYSDBA WITH GRANT OPTION;

UPDATE RDB$TRIGGERS set
  RDB$DESCRIPTION = 've'
  where RDB$TRIGGER_NAME = 'CON_MOVI_BD0';
