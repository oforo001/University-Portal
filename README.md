🎓 University-Portal
Uniwersytecki Portal Społecznościowy z Systemem Wydarzeń

 Opis projektu
Projekt zakłada stworzenie internetowego portalu społecznościowego dla studentów i pracowników uczelni, który umożliwia organizację wydarzeń oraz udział w nich. Aplikacja została zbudowana w technologii ASP.NET MVC z wykorzystaniem Entity Framework i bazy danych SQLite.

✅ Funkcjonalności
1. Rejestracja i logowanie
Rejestracja nowych użytkowników z podziałem na role: Student, Pracownik (Administrator).

Logowanie i wylogowanie z konta.

Resetowanie hasła i edycja danych konta.

Zarządzanie kontami użytkowników za pomocą ASP.NET Identity.

2. System wydarzeń uczelnianych
Przeglądanie listy nadchodzących wydarzeń.

Szczegóły wydarzenia: tytuł, opis, data, miejsce, limit uczestników.

Rejestracja na wydarzenia (z uwzględnieniem limitów).

Panel administratora do tworzenia, edycji i usuwania wydarzeń.

Lista zarejestrowanych uczestników dostępna dla organizatora.

🛠3. Panel administratora
Zarządzanie użytkownikami (np. blokowanie kont).

Zarządzanie wydarzeniami i zgłoszonymi wpisami.

Usuwanie lub edycja nieodpowiednich treści.

🚀 Uruchomienie aplikacji lokalnie
Aby uruchomić projekt lokalnie:

Przełącz się na branch main:
git checkout main

Otwórz Package Manager Console w Visual Studio.

Uruchom polecenie:

Update-Database