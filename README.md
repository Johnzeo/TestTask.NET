# TestTask.NET

Приложение для автоматизации поиска компаний в браузере, записи действий в видеоформат WebM и загрузки видеозаписей на сервер.

## Инструкция по запуску

### 1. Клонирование проекта

```powershell
git clone https://github.com/YOUR_USERNAME/TestTask.git
cd TestTask
```

Замените `YOUR_USERNAME` на имя пользователя GitHub, в аккаунте которого находится репозиторий.

### 2. Восстановление зависимостей

```powershell
dotnet restore
```

### 3. Сборка проекта

```powershell
dotnet build
```

### 4. Установка Chromium

```powershell
pwsh .\TestTask.Recorder\bin\Debug\net10.0\playwright.ps1 install chromium
```

### 5. Создание базы данных

Если `dotnet-ef` ещё не установлен, выполните:

```powershell
dotnet tool install --global dotnet-ef --version 10.0.14
```

Примените миграции для создания базы данных:

```powershell
dotnet ef database update --project .\TestTask.Infrastructure\TestTask.Infrastructure.csproj --startup-project .\TestTask.Api\TestTask.Api.csproj
```

### 6. Запуск API

Откройте первое окно PowerShell:

```powershell
cd TestTask.Api
dotnet run
```

После запуска API будет доступен по адресу:

- API: https://localhost:7130
- Swagger: https://localhost:7130/swagger

Оставьте это окно открытым.

### 7. Запуск Recorder

Откройте второе окно PowerShell:

```powershell
cd TestTask.Recorder
dotnet run -- --server http://localhost:5034
```

Программа автоматически:
- Прочитает список компаний из `input.csv`.
- Откроет браузер Chromium.
- Выполнит поисковые запросы.
- Запишет действия в видеоформат WebM.
- Загрузит видеозаписи на API.
- Откроет загруженное видео для просмотра.

Для автоматического закрытия окна просмотра через 5 секунд используйте:

```powershell
dotnet run -- --server http://localhost:5034 --view-seconds 5
```

### 8. Проверка результата

После завершения обработки видеозаписи будут находиться в следующих папках:

- `TestTask.Recorder/bin/Debug/net10.0/videos/` — локальные записи.
- `TestTask.Api/uploads/` — загруженные на сервер видео.

Информацию о видеозаписях можно проверить в базе данных SQLite.

Для проверки нескольких компаний добавьте новые строки в `input.csv`.

## Формат CSV

Файл `input.csv` уже содержит пример данных.

Формат: UTF-8, разделитель `;`.

```csv
URL;Company Name;Language;Country;Postcode;City;Street;HouseNo
https://duckduckgo.com/;Test Company;English;Netherlands;1012;Amsterdam;Damrak;1
```

## API Endpoints

| Метод | Endpoint | Описание |
|---|---|---|
| POST | `/upload` | Загрузка видео |
| GET | `/get_video_url?company_name=Test%20Company` | Получение URL видео |
| GET | `/media/{fileName}` | Получение и воспроизведение видео |

**Требования:** Windows 10/11, .NET 10 SDK, PowerShell, подключение к интернету и Chromium для Playwright.

**Важно:** API и Recorder запускаются в двух отдельных окнах PowerShell. Сначала необходимо запустить API и дождаться его готовности, затем запускать Recorder.
