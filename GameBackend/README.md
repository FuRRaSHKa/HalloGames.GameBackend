# GameBackend

Монорепозиторий независимо запускаемых сервисов на .NET 10.
Общее решение `GameBackend.sln` объединяет проекты для работы в IDE;
каждый сервис можно собирать, запускать и публиковать отдельно.

## Структура

```text
GameBackend.sln
services/
  Auth/
    GameBackend.Auth.csproj
    Program.cs
    appsettings.json
    Properties/launchSettings.json
tests/
  GameBackend.Tests/
```

`GameBackend.Auth` содержит регистрацию и вход по email, учётные данные,
пользователей аутентификации и выпуск токенов. Все текущие контексты БД,
вспомогательные классы и настройки принадлежат этому сервису.
Пространства имён `GameBackend.HalloGames.*` пока сохранены для совместимости.
Существующий тестовый проект также сохраняет своё имя.

## Запуск Auth

Команды выполняются из каталога с `GameBackend.sln`. Требуется .NET 10 SDK.
Для регистрации и входа нужен PostgreSQL с подготовленной схемой.
Миграции пока отсутствуют: запуск приложения не создаёт таблицы автоматически.

Перед запуском передайте настройки через окружение процесса, например в PowerShell:

```powershell
$env:ConnectionStrings__DefaultConnection = 'Host=localhost;Port=5432;Database=gamebackend_auth;Username=auth;Password=<database-password>'
$env:Jwt__Key = '<random-secret-containing-at-least-32-UTF8-bytes>'
dotnet run --project services/Auth/GameBackend.Auth.csproj --launch-profile https
```

Замените значения в угловых скобках реальными секретами. Не сохраняйте их в Git.
В среде развёртывания настройки передаются сервису через секреты и переменные окружения.

HTTPS-профиль использует `https://localhost:7073`, HTTP — `http://localhost:5055`.
Для HTTPS нужен доверенный сертификат разработки (`dotnet dev-certs https --trust`).
В окружении Development Swagger доступен по пути `/swagger`.
Примеры запросов находятся в `services/Auth/GameBackend.Auth.http`.

Текущая подпись JWT использует HS256. Переход на RS256, проверка входящих JWT,
HTTP-методы refresh и logout остаются отдельными доработками.

## Сборка, тесты и публикация

```powershell
dotnet build services/Auth/GameBackend.Auth.csproj
dotnet test tests/GameBackend.Tests/GameBackend.Tests.csproj
dotnet publish services/Auth/GameBackend.Auth.csproj -c Release -o publish/Auth
```

Полная проверка решения:

```powershell
dotnet test GameBackend.sln
```

## Добавление следующего сервиса

Например, игровой API можно создать отдельным приложением:

```powershell
dotnet new webapi -n GameBackend.Game -o services/Game --framework net10.0
dotnet sln GameBackend.sln add services/Game/GameBackend.Game.csproj --solution-folder services
```

У нового сервиса должны быть собственные настройки, порты, подключение к БД
и отдельная команда запуска или публикации. Его тесты добавляются отдельным проектом.
Игровые данные принадлежат Game; пароли и сессии — Auth.
Не добавляйте ссылку на исполняемый проект Auth для доступа к его таблицам.
Взаимодействие сервисов организуется через явные API и контракты токенов.

CI/CD пока не настроен. Для независимых проверок будущий pipeline Auth должен
учитывать изменения в `services/Auth/**`, `tests/GameBackend.Tests/**`
и общих настройках сборки. Аналогично выделяется pipeline каждого нового сервиса.
Изменения общих зависимостей и контрактов требуют проверки всех затронутых сервисов.
