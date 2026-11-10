"""Wie oft findet eine binaere Suche den richtigen Commit, wenn der Test unzuverlaessig ist?

Modell: 64 Commits, Fehler ab Commit 41. Auf einem fehlerhaften Stand schlaegt der Test nur mit
Wahrscheinlichkeit p an. Auf einem guten Stand ist er immer gruen. Gemessen: Trefferquote und
mittlere Zahl der Testlaeufe, mit einem Lauf und mit drei Laeufen je Stand. 100.000 Durchgaenge,
fester Startwert. Aufruf: python Measure-FlakyBisect.py
"""
import random

N, FEHLER_AB, DURCHGAENGE = 64, 41, 100_000


def suche(p, wiederholungen, zufall):
    gut, schlecht, laeufe = 0, N, 0
    while schlecht - gut > 1:
        mitte = (gut + schlecht) // 2
        rot = False
        for _ in range(wiederholungen):
            laeufe += 1
            if mitte >= FEHLER_AB and zufall.random() < p:
                rot = True
                break
        if rot:
            schlecht = mitte
        else:
            gut = mitte
    return schlecht, laeufe


zufall = random.Random(20261102)
for p in (1.0, 0.9, 0.7, 0.5):
    for w in (1, 3):
        treffer = laeufe = 0
        for _ in range(DURCHGAENGE):
            gefunden, n = suche(p, w, zufall)
            treffer += gefunden == FEHLER_AB
            laeufe += n
        print(f"p={p:.1f} wiederholungen={w}: Treffer {100 * treffer / DURCHGAENGE:5.1f} %, Laeufe im Mittel {laeufe / DURCHGAENGE:4.1f}")
