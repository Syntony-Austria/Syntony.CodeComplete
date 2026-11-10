# Library: fuer Agenten

## Befehle
- Test: dotnet test --filter Category!=Long
- Vor dem Commit: dotnet build -warnaserror und dotnet format --verify-no-changes

## Grenzen
- Library.Domain kennt keine Technik. Prueft ein Architekturtest.
- Oeffentliche Typen in Library.Api sind ein Vertrag.
  Nicht aendern ohne Rueckfrage.
- Migrationen, Loeschen und alles nach aussen: nur nach Freigabe.

## Tests
- Einen Test nur aendern, wenn der Auftrag das sagt.
- Nie [Ignore], nie eine Grenze senken.
- Fehler: zuerst der rote Test, dann die Korrektur, zwei Commits.
- Erwartete Werte kommen aus der Regel, nicht aus dem Code.

## Bericht
- Sage, was du geprueft hast und wie.
- Was du nicht geprueft hast, heisst "vermutlich".
- Nenne, was fehlt. Nach fuenf Versuchen ohne Fortschritt: aufhoeren und berichten.

## Vorbilder
- Fachregel mit Tests: src/Fees/OverdueFee.cs
- Zugriff auf Dateien: src/Storage/LoanFile.cs
