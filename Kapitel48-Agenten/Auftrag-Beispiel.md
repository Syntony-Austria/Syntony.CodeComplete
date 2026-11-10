ZIEL
Die Mahngebuehr fuer Nachschlagewerke steigt ab 1. Jaenner 2027 auf das
Dreifache. Massgeblich ist der Tag der Berechnung. Beispiele stehen in
docs/gebuehr-2027.md.

GRENZE
Aendere nur src/Fees/OverdueFee.cs und tests/Fees/OverdueFeeTests.cs.
Bestehende Tests nicht aendern, keine Warnung unterdruecken, kein neues
Paket. Die Zeit kommt ueber TimeProvider. Wenn das nicht reicht: aufhoeren, fragen.

PRUEFUNG
Zuerst die Beispiele als Tests, sie muessen rot sein. Dann die Aenderung.
dotnet test --filter FullyQualifiedName~OverdueFee
dotnet build -warnaserror

BERICHT
Ausgabe beider Befehle. Was du geprueft hast, was vermutet, was offen ist.
