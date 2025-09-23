# Hangfire bez OWIN - Aplikacja konsolowa z Dashboard Info

## Opis
Ta aplikacja demonstruje, jak uruchomić Hangfire w aplikacji konsolowej .NET Framework 4.8.1 **bez używania OWIN** wraz z prostym serwerem informacyjnym na porcie 5000.

## Wymagania
- .NET Framework 4.8.1
- SQL Server LocalDB (lub dowolna instancja SQL Server)
- Pakiety NuGet Hangfire (już zainstalowane)

## Nowe funkcje ✨

### 🌐 Serwer informacyjny na porcie 5000
- **URL**: http://localhost:5000/
- **Funkcja**: Wyświetla status systemu Hangfire w czasie rzeczywistym
- **Automatyczne odświeżanie**: Co 30 sekund
- **Technologia**: HttpListener (bez ASP.NET Core)

### 📊 Statystyki zadań
- Możliwość podglądu statystyk zadań z poziomu konsoli (opcja 's')
- Liczba wykonanych, nieudanych i przetwarzanych zadań
- Informacje o zadaniach zaplanowanych

## Konfiguracja

### Connection String
Domyślnie aplikacja używa LocalDB:
```
Server=(localdb)\MSSQLLocalDB;Database=HangfireTest;Integrated Security=true;
```

Możesz zmienić connection string w pliku `Program.cs` (linia ~18) na swoją instancję SQL Server.

### Baza danych
Hangfire automatycznie utworzy potrzebne tabele w bazie danych przy pierwszym uruchomieniu.

## Funkcjonalności

### Typy zadań
1. **Proste zadania w tle** - wykonują się natychmiast
2. **Zadania z opóźnieniem** - wykonują się po określonym czasie
3. **Zadania cykliczne** - powtarzają się według harmonogramu
4. **Zadania z ciągiem** - jedno zadanie uruchamia następne
5. **Zadania z retry** - automatycznie ponawiane w przypadku błędu

### Nowe opcje w menu
- **Opcja 8** - Otwiera stronę informacyjną w przeglądarce
- **Opcja 's'** - Pokazuje statystyki zadań Hangfire

### Dashboard Info
- ✅ Status serwera Hangfire
- 🗃️ Informacje o bazie danych
- 📊 Podstawowe statystyki
- 🎮 Instrukcje użytkowania
- ⚠️ Informacje o pełnym Dashboard

## Przykłady użycia

#### Zadanie natychmiastowe
```csharp
BackgroundJob.Enqueue(() => HangfireJobs.SimpleBackgroundJob());
```

#### Zadanie z opóźnieniem
```csharp
BackgroundJob.Schedule(() => HangfireJobs.SimpleBackgroundJob(), TimeSpan.FromMinutes(5));
```

#### Zadanie cykliczne
```csharp
RecurringJob.AddOrUpdate("my-job", () => HangfireJobs.RecurringTask(), Cron.Daily);
```

#### Ciąg zadań
```csharp
var jobId1 = BackgroundJob.Enqueue(() => HangfireJobs.FirstJobInChain());
var jobId2 = BackgroundJob.ContinueJobWith(jobId1, () => HangfireJobs.SecondJobInChain());
```

## Uruchomienie

1. Upewnij się, że masz uruchomiony SQL Server LocalDB
2. Skompiluj projekt
3. Uruchom aplikację
4. Otwórz http://localhost:5000/ w przeglądarce (opcja 8)
5. Używaj opcji 1-7 do testowania różnych typów zadań
6. Sprawdzaj statystyki opcją 's'
7. Naciśnij 'q' aby zakończyć

## Zalety tego podejścia

✅ **Brak OWIN** - prostsze, mniej zależności
✅ **Pełna kontrola** - możesz kontrolować cykl życia serwera Hangfire
✅ **Lekkie** - idealne dla aplikacji konsolowych, usług Windows
✅ **Elastyczne** - łatwo integrować z istniejącym kodem
✅ **Serwer informacyjny** - podstawowy monitoring przez przeglądarkę
✅ **Statystyki w konsoli** - szybki podgląd wydajności

## O Dashboard

### 🚧 Ograniczenia aktualnej wersji
- **Nie ma pełnego Dashboard Hangfire** - wymaga ASP.NET Core/OWIN
- Serwer na porcie 5000 wyświetla tylko **informacje i status**
- Wszystkie zadania działają prawidłowo w tle

### 🎯 Pełny Dashboard
Aby uzyskać pełny Dashboard Hangfire:
1. Użyj aplikacji ASP.NET Core
2. Zainstaluj pakiet `Hangfire.AspNetCore`
3. Dodaj `app.UseHangfireDashboard();`

## Monitorowanie

### W przeglądarce (port 5000)
- Status systemu w czasie rzeczywistym
- Informacje o konfiguracji
- Instrukcje użytkowania
- Automatyczne odświeżanie

### W konsoli
- Wykonanie zadań na żywo
- Statystyki zadań (opcja 's')
- Identyfikatory zadań (Job IDs)

### W bazie danych
Hangfire automatycznie tworzy tabele:
- `HangFire.Job` - lista zadań
- `HangFire.JobQueue` - kolejka zadań
- `HangFire.State` - stany zadań
- itd.

## Uwagi

- Aplikacja automatycznie zamknie serwer Hangfire przy zakończeniu
- Zadania cykliczne będą kontynuowane po restarcie aplikacji
- Serwer informacyjny działa na HttpListener (nie wymaga IIS)
- W przypadku problemów z bazą danych, sprawdź czy LocalDB jest zainstalowany i uruchomiony