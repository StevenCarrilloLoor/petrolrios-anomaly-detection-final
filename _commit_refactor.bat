@echo off
cd /d "C:\Users\steve\Desktop\Proyecto Tesis\petrolrios-anomaly-detection"

git add src/PetrolRios.Detectors/RuleBasedDetector.cs src/PetrolRios.Detectors/InvoiceAnomalyDetector.cs src/PetrolRios.Detectors/CashFraudDetector.cs src/PetrolRios.Detectors/PaymentFraudDetector.cs src/PetrolRios.Detectors/ComplianceViolationDetector.cs src/PetrolRios.Detectors/Rules/CashFraud/CashFraudRules.cs src/PetrolRios.Detectors/Rules/PaymentFraud/PaymentFraudRules.cs src/PetrolRios.Detectors/Rules/ComplianceViolation/ComplianceViolationRules.cs src/PetrolRios.Detectors/DependencyInjection.cs tests/PetrolRios.Detectors.Tests/TestHelpers.cs tests/PetrolRios.Detectors.Tests/CashFraudDetectorTests.cs tests/PetrolRios.Detectors.Tests/PaymentFraudDetectorTests.cs tests/PetrolRios.Detectors.Tests/ComplianceViolationDetectorTests.cs tests/PetrolRios.Detectors.Tests/NuevasReglasDetectorTests.cs > _cr.log 2>&1
echo ADD_EXIT=%ERRORLEVEL% >> _cr.log

git commit -m "Refactorizar los 4 detectores a Strategy por regla (orquestador base)" -m "Cada detector deja de ser una god-class: la logica de cada regla pasa a su propia clase IDetectionRule y los detectores se vuelven orquestadores delgados sobre el nuevo RuleBasedDetector. Agregar una regla nueva = crear una clase y registrarla en DI, sin tocar ningun detector (principio Abierto/Cerrado)." -m "RuleBasedDetector centraliza la orquestacion (filtra reglas por carril, respeta el on/off y el umbral/carril configurados). CashFraud (5 reglas), PaymentFraud (5) y ComplianceViolation (6) extraidos a clases de regla; InvoiceAnomaly migrado al mismo orquestador. 16 reglas registradas en DI." -m "Tests actualizados con helpers Crear*Detector: 206 pruebas en verde (build 0 warnings / 0 errors). Comportamiento identico verificado E2E (Firebird->agente->Hangfire): alertas de los 4 detectores con tipos, scores y descripciones correctos." >> _cr.log 2>&1
echo COMMIT_EXIT=%ERRORLEVEL% >> _cr.log

git log --oneline -6 >> _cr.log 2>&1
echo --- STATUS --- >> _cr.log
git status --short >> _cr.log 2>&1
echo --- DONE --- >> _cr.log
