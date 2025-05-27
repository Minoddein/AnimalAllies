# AnimalAllies
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
<div>
  
[![Build/Restore/Docker/Tests](https://github.com/Minoddein/AnimalAllies/actions/workflows/dotnet.yml/badge.svg?branch=main)](https://github.com/Minoddein/AnimalAllies/actions/workflows/dotnet.yml)
[![Code Style Validation](https://github.com/Minoddein/AnimalAllies/actions/workflows/code-style.yml/badge.svg)](https://github.com/Minoddein/AnimalAllies/actions/workflows/code-style.yml)
[![.NET Unit Tests](https://github.com/Minoddein/AnimalAllies/actions/workflows/unit-testing.yml/badge.svg)](https://github.com/Minoddein/AnimalAllies/actions/workflows/unit-testing.yml)
[![Dependabot Updates](https://github.com/Minoddein/AnimalAllies/actions/workflows/dependabot/dependabot-updates/badge.svg)](https://github.com/Minoddein/AnimalAllies/actions/workflows/dependabot/dependabot-updates)
[![CodeQL (C#, GH Actions)](https://github.com/Minoddein/AnimalAllies/actions/workflows/codeql.yaml/badge.svg)](https://github.com/Minoddein/AnimalAllies/actions/workflows/codeql.yaml)

</div>

[![.NET](https://img.shields.io/badge/.NET-9.0-blue.svg)](https://dotnet.microsoft.com/)
[![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-9.0-orange.svg)](https://dotnet.microsoft.com/apps/aspnet)
[![RabbitMQ](https://img.shields.io/badge/RabbitMQ-8.3.6-green.svg)](https://www.rabbitmq.com/)
[![Entity Framework Core](https://img.shields.io/badge/Entity%20Framework%20Core-9.0.1-purple.svg)](https://docs.microsoft.com/en-us/ef/core/)

AnimalAllies - платформа волонтёрской помощи животным, где оказывается ряд услуг: материальная помощь нуждающимся животным, поиск пропавших питомцев, оставление животного на временное на попечительство в виду отъезда и т.д.

<details><summary><h2>Скриншоты</h2></summary>
Будут добавлены, когда будет готова фронтенд часть платформы
</details>

## Возможности backend`а:
- [x] Аутентификация и авторизация на основе разрешений
- [x] Подтверждение учётной записи по почте через сервис уведомлений 
- [x] Модуль управления животными и управления видами реализован по ddd
- [x] CRUD операции над всеми сущностями: волонтёр, питомец, порода, вид
- [x] Питомцы обладают позицией, которая отображает их очередность на помощь от волонтера. Позиция может быть изменена в зависимости от условий. 
- [x] Возможность обновления профиля пользователя, в качестве обычного клиента и в роли волонтёра
- [x] Мягкое и полное удаление некоторых сущностей
- [x] Хранение файлов (видео и фото) животных или аваторок пользователей в S3 хранилище, которое реализовано в видео отдельного сервиса
- [x] Манипуляции с S3 хранилищем реализованы через AWS SDK S3 и Minio
- [x] Для хранения файлов предусмотрена мультичастичная загрузка 
- [x] В модуле заявок на волонтёрство реализована следующая бизнес-логика по ddd:
  - [x] Подать заявку на волонтёрство, также проверяется наличие запрета на подачу через доменные события
  - [x] Взять заявку на рассмотрение админом (проверка пользователя закрепляется за ним)
  - [x] Одобрить заявку на волонтёрство, тогда через RabbiMQ будет отправлена команда на создание аккаунта волонтёра в модуле авторизации
  - [x] Отправка заявки админом на доработку с указанием проблемных мест
  - [x] Обновление заявки с последующей отправкой на дальнейшее рассмотрение
  - [x] Отклонить заявку на волонтёрство с указанием причины, после чего, через доменные события создастся запрет на подачу новых волонтёрских заявок сроком на неделю
  - [x] Все важные этапы в процессе рассмотрения заявки уведомляются по почте
- [x] В модуле дискуссий реализована следующая бизнес-логика по ddd:
  - [x] Когда админ берёт заявку на рассмотрение в модулей заявок на волонтерство, то в модуле дискуссий через контракты создаётся сущность дискуссии между админом и пользователем для обсуждения вопросов.
  - [x] Реализована функциональность, которая закрывает дискуссию для обоих участников
  - [x] Реализована функциональность, которая позволяет удалять пользователям свои сообщения в дискуссии
  - [x] Реализована функциональность, которая позволяет отправлять сообщений в дискуссии
  - [x] Реализована функциональность, которая позволяет обновлять пользователям свои сообщения в дискуссии
- [x] Реализованы разного вида запросы с пагинацией, фильтрацией и сортировкой. Для повышения производительности был использован CQRS. Для команд - ef core, для запросов - dapper 
- [x] Написаны юнит-тесты для всей бизнес-логики 
- [x] Добавленно кэширование через Redis для повышения производительности запрос и улучшения пользовательского опыта
- [x] Проект переведен на .NET 9
- [x] Внедрен transactional outbox паттерн для обеспечения атомарности в распределенных транзакциях, а также вынесен в nuget пакет
- [x] Добавлены метрики через ElasticSearch, Kibana, Grafana
- [x] Настроен CI/CD
- [ ] Написаны интеграционные тесты
- [x] Покрытие юнит-тестами >= 70%
- [ ] Покрытие интеграционными-тестами >= 70%
- [ ] Реализация полнотекстового поиска на PostgreSql/ElasticSearch
- [x] Реализован файловый сервис
- [x] Реализован сервис уведомлений
- [ ] Реализована OAuth 2, интегрированы следующие сервисы: Google, VK, Yandex 
- [x] Реализован телеграм-бот
- [ ] Реализован сервис событий
- [ ] Реализован платёжный сервис
- [ ] Реализован сервис поиска пропавших животных через ML.NET

## Функционал Файлового сервиса:
- [x] Скачивание файлов через presigned url
- [x] Загрузка файлов через presigned url
- [x] Удаление файлов через presigned url
- [x] Получения мета-данных файлов из MongoDB
- [x] Мультичастичная загрузка для больших файлов
- [x] Проверка консистентности данных через Hangfire (запланированная задача смотри, сопоставляется ли записи из MongoDB наличие файла в S3 Minio)
- [x] Реализован слой Communication для связи с другими сервисами
- [x] Слой Communication и Contracts добавлен в nu-get
- [x] Реализованы функции для одиночной и многофайловой загрузки 

## Функционал Сервиса уведомлений:
- [x] Реализация отправки писем, используя MailKit и протокол SMTP
- [x] Отправление письма пользователю для подтверждения регистрации
- [x] Возможность настройки сервиса уведомлений: получать уведомление на Email, Telegram, Web
- [x] Создание базовых настроек сервиса с включенными уведомлениями на Email по умолчанию при подтверждённом аккаунте
- [x] Связь с бэкендом по RabbitMQ
- [x] Слой Contracts добавлен в nu-get

## Функционал Сервиса инвалидации кэша:
- [x] Инвалидация кэша L1 и L2 Redis
- [x] Инвалидация кэша по тегу
- [x] Инвалидация кэша по ключу

## Функционал Телеграм бота:
- [x] Реализована своя FSM, используя паттерн Command и State
- [x] Кэширование контекст в Redis
- [x] Авторизация в аккаунт
- [x] Уведомление о статусе заявки
- [x] Вспомогательные команды: /help, /info
- [ ] Получение информации о своих животных (для волонтёра)

## Функционал Платежного сервиса:
- [ ] *В разработке*
- [ ] *В разработке*

## Функционал Сервиса событий:
- [ ] *В разработке*
- [ ] *В разработке*

## Функционал CV-сервиса:
- [ ] *В разработке*
- [ ] *В разработке*

## Стек:

Вот список наиболее значимых фреймворков из предоставленного списка в формате markdown:

| Фреймворк | Версия | Источник |
| --- | --- | --- |
| AWSSDK.S3 | 3.7.414.1 | nuget.org |
| Hangfire | 1.8.15 | nuget.org |
| Hangfire.Core | 1.8.17 | nuget.org |
| Hangfire.PostgreSql | 1.20.10 | nuget.org |
| Microsoft.AspNetCore.OpenApi | 9.0.1 | nuget.org |
| Microsoft.Extensions.Configuration | 9.0.1 | nuget.org |
| Microsoft.Extensions.Logging.Abstractions | 9.0.1 | nuget.org |
| Minio | 6.0.4 | nuget.org |
| MongoDB.Bson | 3.1.0 | nuget.org |
| MongoDB.Driver | 3.1.0 | nuget.org |
| MongoDB.Driver.Core | 2.30.0 | nuget.org |
| Serilog.AspNetCore | 9.0.0 | nuget.org |
| Serilog.Enrichers.Environment | 3.0.1 | nuget.org |
| Serilog.Enrichers.Thread | 4.0.0 | nuget.org |
| Serilog.Exceptions | 8.4.0 | nuget.org |
| Serilog.Sinks.Seq | 9.0.0 | nuget.org |
| Swashbuckle.AspNetCore | 7.2.0 | nuget.org |
| Swashbuckle.AspNetCore.Swagger | 7.2.0 | nuget.org |
| Swashbuckle.AspNetCore.SwaggerGen | 7.2.0 | nuget.org |
| Swashbuckle.AspNetCore.SwaggerUI | 7.2.0 | nuget.org |
| Mass Transit.RabbitMQ | 8.3.6 | nuget.org |
| MediatR | 12.4.1 | nuget.org |
| Dapper | 2.1.66 | nuget.org |
| Microsoft.EntityFrameworkCore | 9.0.1 | nuget.org |
| Microsoft.AspNetCore | 9.0.1 | nuget.org |
| Npgsql | 9.0.2 | nuget.org |
| Npgsql.EntityFrameworkCore.PostgreSQL | 9.0.3 | nuget.org |
| Scrutor | 6.0.1 | nuget.org |
| FluentValidation | 11.11.0 | nuget.org |
| FluentValidation.DependencyInjectionExtensions | 11.11.0 | nuget.org |
| FluentAssertions | 8.0.1 | nuget.org |
| xunit | 2.5.3; 2.9.0; 2.9.3 | nuget.org |
| xunit.runner.visualstudio | 2.8.2; 3.0.2 | nuget.org |
| Microsoft.Extensions.Logging | 9.0.1 | nuget.org |
| Moq | 4.20.72 | nuget.org |
| Serilog.AspNetCore | 9.0.0 | nuget.org |

## Установка и запуск

0. Клонируйте репозиторий и перейдите в его папку.

### Посредством Docker

1. Установите Docker.
2. Установите .NET SDK (тот же, что прописан в global.json), а также EF Core. Последний можно добавить командой:

```shell
dotnet tool install --global dotnet-ef
```

3. Настройте файлы appsetting.Docker.json в каждом сервисе, прописав собственные строки
   подключения (они должны совпадать с указанными в [compose.yaml](compose.yaml))

4. В Gitlab аккаунте сгенерируйте Personal Access Token, проставьте нужные разрешения

![Image](https://github.com/user-attachments/assets/5b596f5b-97a4-4af2-87ed-a4068d249681)

Далее скопируйте токен:

![Image](https://github.com/user-attachments/assets/a1ea47e6-ef0a-4b44-a3b9-73123d09893f)

Перейдите в IDE, откройте пакет-менеджер -> источники -> добавьте источник для скачивания nuget-пакетов
Ссылка: https://gitlab.com/api/v4/projects/63188031/packages/nuget/index.json

![Image](https://github.com/user-attachments/assets/13bf2f91-aad1-4903-90d3-f760ca655472)

Заполните форму, пароль - это ваш сгенерированный токен

![Image](https://github.com/user-attachments/assets/c097afc3-2199-4d18-b035-81ebca543666)

6. Добавить источник для скачивания nu-get пакета
   
7. Создайте файлы `.env`  и настройте все описанные там параметры.

8. Запустите сборку и подъём контейнера:

```shell
docker-compose up -d --build
```
   
8. Создайте миграции к базе данных:

Воспользуйтесь заготовленным скриптом:

```shell
.\migrations-add-and-update.cmd
```


Теперь можно использовать бэкенд по адресу http://localhost:8080. Документация к бэкенду доступна в
интерфейсе [Swagger](http://localhost:8080/swagger).

## Конфигурация

`в процессе написания`
