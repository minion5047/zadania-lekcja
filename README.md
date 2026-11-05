# Stoper — Aplikacja Pulpitowa (Zadanie 2.1)

> **Przedmiot:** Programowanie aplikacji desktopowych / Pracownia programowania  
> **Klasa:** 4TC | **Autor:** Szymon D.  
> **Technologia:** C# / .NET 10 / WPF (Windows Presentation Foundation)  

---

## ⏱ O projekcie

Aplikacja desktopowa stopera z precyzyjnym pomiarem czasu z dokładnością do **setnych części sekundy**, zaawansowaną rejestracją międzyczasów (okrążeń) oraz nowoczesnym, ciemnym interfejsem graficznym.

### ✨ Funkcje i możliwości

- **Precyzja pomiaru:** Odświeżanie 60 FPS (~16ms) bazujące na `System.Diagnostics.Stopwatch`.
- **Wyraźne przyciski akcji:**
  - `START` (zielony) – uruchomienie pomiaru.
  - `STOP` (czerwony) – zatrzymanie pomiaru.
  - `OKRĄŻENIE` (niebieski) – rejestracja międzyczasu.
  - `ZERUJ` – reset czasu i historii.
- **Tabela okrążeń:**
  - Numer okrążenia (`#01`, `#02`, ...).
  - Czas bieżącego okrążenia (split time).
  - Czas sumaryczny (total time).
  - Automatyczne wykrywanie i wyróżnianie najszybszego (zielony badge) i najwolniejszego (czerwony badge) okrążenia.
  - Przycisk **Kopiuj** kopiujący sformatowaną listę do schowka systemu Windows.
- **Zawsze na wierzchu (Always on Top):** Checkbox pozwalający przypiąć stoper ponad innymi oknami.
- **Skróty klawiszowe:**
  - <kbd>Spacja</kbd> – Start / Stop
  - <kbd>L</kbd> – Nowe okrążenie (Lap)
  - <kbd>R</kbd> – Zeruj (Reset)

---

## 🚀 Uruchomienie

1. **Dwuklik:** Uruchom plik `Uruchom_Stoper.bat` lub `Stoper.exe`.
2. **Przez wiersz poleceń:**
   ```bash
   dotnet run
   ```
3. **Kompilacja projektu:**
   ```bash
   dotnet build
   ```
