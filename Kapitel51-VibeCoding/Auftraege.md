KNAPP (dreimal, je eine frische Sitzung)

Antworte nur mit einem einzigen C#-Codeblock, ohne Tools zu benutzen.
Namensraum Library.Lending.
1. enum MemberKind { Adult, Child, Staff } und
   OverdueFee.For(int daysLate, MemberKind member, bool isReference).
   Ergebnis in Cent. Mahngebuehr einer Bibliothek: 25 Cent pro Tag,
   drei Tage Nachsicht, Kinder zahlen die Haelfte, Nachschlagewerke
   doppelt, Personal nichts, hoechstens 20 Euro.
2. Scans.Read(string root, string name) mit Ergebnis byte[]?.
   Liest den Scan mit diesem Namen aus dem Ordner.

GENAU (dreimal, je eine frische Sitzung)

Dieselben Signaturen. Dazu:
1. Bis einschliesslich 3 Tage kostet nichts. Ab dem 4. Tag zaehlen alle
   Tage. Pro Tag 25 Cent, bei Nachschlagewerken 50 Cent. Die Summe ist
   hoechstens 2000 Cent. Kinder zahlen die Haelfte dieser Summe.
   Personal zahlt nie etwas.
   Beispiele als Tests: For(3, Adult, false) == 0,
   For(4, Adult, false) == 100, For(4, Adult, true) == 200,
   For(4, Child, false) == 50, For(365, Staff, false) == 0,
   For(1000, Adult, false) == 2000, For(int.MaxValue, Adult, false) == 2000,
   For(400, Child, true) == 1000.
2. name kommt ungeprueft aus einer Webanfrage. Es darf keine Datei
   ausserhalb von root gelesen werden, auch nicht in einem Nachbarordner,
   dessen Name mit dem von root beginnt. In jedem unzulaessigen Fall und
   wenn die Datei fehlt, liefert die Methode null.
