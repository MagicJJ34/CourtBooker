# CourtBooker – System rezerwacji kortów tenisowych

CourtBooker to aplikacja internetowa umo¿liwiaj¹ca rezerwacjê kortów tenisowych online. Projekt realizowany jest w ramach pracy in¿ynierskiej i ma na celu usprawnienie procesu rezerwacji oraz zarz¹dzania dostêpnoœci¹ kortów.

## Wykorzystane technologie

- **Backend:** C#, ASP.NET Core MVC (.NET 8)
- **Frontend:** HTML, CSS, JavaScript, Bootstrap 5
- **Baza danych:** PostgreSQL (Supabase)
- **Dostêp do danych:** Entity Framework Core
- **Kalendarz rezerwacji:** FullCalendar

## Funkcjonalnoœci aplikacji

### U¿ytkownik niezalogowany
- Przegl¹danie dostêpnych kortów tenisowych
- Sprawdzanie zajêtoœci terminów w kalendarzu
- Wybór kortu i terminu przed zalogowaniem
- Rejestracja i logowanie do systemu

### U¿ytkownik zalogowany
- Tworzenie rezerwacji na wybrany termin
- Przegl¹danie i anulowanie w³asnych rezerwacji
- Automatyczne blokowanie zajêtych terminów
- Edycja danych konta oraz zmiana has³a

### Administrator
- Zarz¹dzanie kortami tenisowymi
- Zarz¹dzanie u¿ytkownikami i rezerwacjami
- Przegl¹danie statystyk dotycz¹cych rezerwacji
- Generowanie raportów z mo¿liwoœci¹ filtrowania danych
- Eksport raportów do plików CSV

## Bezpieczeñstwo

W aplikacji zastosowano uwierzytelnianie u¿ytkowników i kontrolê dostêpu opart¹ na rolach. Has³a s¹ przechowywane w postaci skrótów kryptograficznych, a mechanizmy walidacji i ograniczenia w bazie danych zapobiegaj¹ tworzeniu nak³adaj¹cych siê rezerwacji.

## Uruchomienie projektu

Do uruchomienia aplikacji wymagane s¹:
- Œrodowisko .NET SDK obs³uguj¹ce .NET 8
- Baza danych PostgreSQL z przygotowan¹ struktur¹ tabel
- Skonfigurowane po³¹czenie z baz¹ danych

Po skonfigurowaniu œrodowiska nale¿y przejœæ do katalogu zawieraj¹cego plik `CourtBooker.csproj` i wykonaæ:

```
dotnet restore
dotnet build
dotnet run
```

Aplikacja bêdzie dostêpna pod adresem lokalnym wskazanym w terminalu.

## Informacje o projekcie

Projekt rozwijany jest jako czêœæ pracy in¿ynierskiej. Obejmuje implementacjê aplikacji internetowej, integracjê z relacyjn¹ baz¹ danych oraz mechanizmy obs³ugi rezerwacji i zarz¹dzania systemem.