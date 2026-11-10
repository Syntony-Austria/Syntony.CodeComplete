# Syntony Code Complete – Beispielcode

Begleitcode zum Buch *Syntony Code Complete – Professionelle Software mit C#* von Josef Hahnl.
Jedes C#-Listing im Buch stammt aus einem Projekt in diesem Repository, das baut und dessen Tests laufen.

*Companion code for the German book* Syntony Code Complete. *Code, identifiers and comments are in English.*

## Voraussetzungen

- .NET SDK 10 (gebaut mit 10.0.403; `dotnet --version` zeigt deine Fassung)
- Git auf der Kommandozeile
- eine Netzverbindung, wenn NuGet die Pakete beim ersten Bau lädt
- Python 3 nur, wenn du die Messungen mit den `Measure-*.py`-Skripten selbst wiederholen willst

## So fängst du an

```
git clone https://github.com/Syntony-Austria/Syntony.CodeComplete.git
cd Syntony.CodeComplete/Kapitel01
dotnet test
```

Ist alles grün, bist du bereit. Jeder Kapitelordner steht für sich; du brauchst kein Framework von uns.

## Was du wissen solltest

- **`Tangled`** im Dateinamen heißt: absichtlich verhedderter Code, den das Kapitel verbessert.
- **`Probe`** im Namen heißt: Diese Datei oder dieser Ordner soll ein Gate auslösen. Ein roter Build ist dort das richtige Ergebnis.
- Vier Projekte bauen absichtlich nicht für sich allein:
  `Kapitel37-Doku/Probe/BrokenDocs` (Dokumentation mit Fehlern),
  `Kapitel37-Doku/Probe/Undocumented` (fehlende Dokumentation, CS1591),
  `Kapitel41-Betrieb/Lieferkette/Verbraucher` (Verbraucher der Paketmessung) und
  `Kapitel51-VibeCoding` (Vorlage für `Measure-Proben.py`, das die Proben einsetzt).
- Die Root-Dateien `Directory.Build.props`, `Directory.Build.targets`, `Directory.Packages.props`,
  `NuGet.config` und `.editorconfig` sind absichtlich leer oder minimal. Sie schirmen die Kapitel
  gegen Einstellungen aus übergeordneten Ordnern ab, damit ein Klon überall gleich baut.
- Die Dateien unter `Kapitel06/.github` und `Kapitel10/.github` sind Lehrbeispiele. GitHub führt
  sie hier nicht aus, weil sie nicht im Wurzelordner liegen.

## Ordner und Kapitel

| Ordner | Kapitel im Buch |
|---|---|
| `AnhangB` | Anhang B · Beispielkonfiguration |
| `Kapitel01` | 1 · Was Konstruktion ist und warum Komplexität der Hauptgegner ist |
| `Kapitel02` | 2 · Code wird gelesen: für den nächsten Menschen schreiben |
| `Kapitel03` | 3 · Vom Absatz zum Gate: Regeln, die eine Maschine prüft |
| `Kapitel04` | 4 · Entscheidungen vor der ersten Zeile |
| `Kapitel05` | 5 · Git: der Verlauf als Werkzeug |
| `Kapitel06` | 6 · GitHub: gemeinsam am selben Code |
| `Kapitel07` | 7 · .editorconfig und dotnet format |
| `Kapitel08` | 8 · Analyzer: der Compiler als Kollege |
| `Kapitel09` | 9 · Der Build als Regelwerk |
| `Kapitel10` | 10 · Prüfen ohne Erinnern |
| `Kapitel11` | 11 · Namen |
| `Kapitel12` | 12 · Variablen und ihre Lebensdauer |
| `Kapitel13` | 13 · Datentypen: Werte, Records, Nullbarkeit |
| `Kapitel14` | 14 · Routinen |
| `Kapitel15` | 15 · Kontrollfluss und Verschachtelung |
| `Kapitel16` | 16 · Tabellen statt Verzweigungen |
| `Kapitel17` | 17 · Klassen als abstrakte Datentypen |
| `Kapitel18` | 18 · Ressourcen und Besitz |
| `Kapitel19` | 19 · Zwei Spuren: erwartete Fehler und Programmfehler |
| `Kapitel20` | 20 · Die Grenze sichern: Validierung und Guards |
| `Kapitel21` | 21 · Beobachtbarkeit |
| `Kapitel22` | 22 · Tiefe Module und Information Hiding |
| `Kapitel23` | 23 · Schichten und Abhängigkeiten |
| `Kapitel24` | 24 · Die öffentliche API als Vertrag |
| `Kapitel25` | 25 · Konstanten, Standardwerte, Konfiguration |
| `Kapitel26` | 26 · Architektur als ausführbarer Test |
| `Kapitel27` | 27 · Zukunftsfest bauen |
| `Kapitel28` | 28 · Sicherheit als Entwurfsfrage |
| `Kapitel29` | 29 · Tests kodieren Absicht |
| `Kapitel30` | 30 · Testfälle entwerfen |
| `Kapitel31` | 31 · Test zuerst und der Regressionstest |
| `Kapitel32` | 32 · Testbar bauen |
| `Kapitel33` | 33 · Testinfrastruktur und Abdeckung als Ratsche |
| `Kapitel34` | 34 · Async-Disziplin |
| `Kapitel35` | 35 · Threads, Sperren und gemeinsamer Zustand |
| `Kapitel36` | 36 · Performance: erst messen, dann ändern, dann halten |
| `Kapitel37-Doku` | 37 · Dokumentation und Entscheidungen |
| `Kapitel38-Daten` | 38 · Daten, die bleiben: Persistenz und Migration |
| `Kapitel39-Prozesse` | 39 · Fehler zwischen Prozessen |
| `Kapitel40-Ausliefern` | 40 · Ausliefern: vom Commit zum Betrieb |
| `Kapitel41-Betrieb` | 41 · Sicherheit im Betrieb und in der Lieferkette |
| `Kapitel42-Last` | 42 · Unter Last: Kapazität, Ziele und Alarme |
| `Kapitel43-Zeichen` | 43 · Oberfläche, Sprache und Zeichen |
| `Kapitel44-Recht` | 44 · Lizenzen und Datenschutz |
| `Kapitel45-Refactoring` | 45 · Refactoring und Debugging als wissenschaftliche Methode |
| `Kapitel46-Review` | 46 · Code-Review |
| `Kapitel47-Team` | 47 · Im Team: Vereinbarungen, die halten |
| `Kapitel48-Anforderungen` | 48 · Anforderungen, die ein Test prüft |
| `Kapitel49-Projekt` | 49 · Projektführung mit Messwerten: schätzen, planen, steuern |
| `Kapitel50-Agenten` | 50 · Mit KI-Agenten programmieren |
| `Kapitel51-VibeCoding` | 51 · Vibe-Coding: was es leistet und wo es bricht |

Die Kapitel 52 und 53 haben keinen eigenen Beispielcode.

## Lizenz

MIT, siehe [LICENSE](LICENSE). Du darfst den Code frei verwenden, auch in eigenen Projekten.
