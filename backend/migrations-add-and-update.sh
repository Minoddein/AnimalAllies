# Создание миграций
dotnet ef migrations add init -c AccountsDbContext -p ./backend/src/Accounts/AnimalAllies.Accounts.Infrastructure -s ./backend/src/AnimalAllies.Web
dotnet ef migrations add init -c VolunteerWriteDbContext -p ./backend/src/PetManagement/AnimalAllies.Volunteer.Infrastructure -s ./backend/src/AnimalAllies.Web
dotnet ef migrations add init -c SpeciesWriteDbContext -p ./backend/src/BreedManagement/AnimalAllies.Species.Infrastructure -s ./backend/src/AnimalAllies.Web
dotnet ef migrations add init -c WriteDbContext -p ./backend/src/VolunteerRequests/VolunteerRequests.Infrastructure -s ./backend/src/AnimalAllies.Web
dotnet ef migrations add init -c WriteDbContext -p ./backend/src/Discussion/Discussion.Infrastructure -s ./backend/src/AnimalAllies.Web
dotnet ef migrations add init -c OutboxContext -p ./backend/src/Outbox/Outbox -s ./backend/src/AnimalAllies.Web
dotnet ef migrations add init -c ApplicationDbContext -p ./TelegramBotService/src/TelegramBotService -s ./TelegramBotService/src/TelegramBotService
dotnet ef migrations add init -c ApplicationDbContext -p ./NotificationService/src/NotificationService -s ./NotificationService/src/NotificationService

# Применение миграций
dotnet ef database update -c AccountsDbContext -p ./backend/src/Accounts/AnimalAllies.Accounts.Infrastructure -s ./backend/src/AnimalAllies.Web
dotnet ef database update -c VolunteerWriteDbContext -p ./backend/src/PetManagement/AnimalAllies.Volunteer.Infrastructure -s ./backend/src/AnimalAllies.Web
dotnet ef database update -c SpeciesWriteDbContext -p ./backend/src/BreedManagement/AnimalAllies.Species.Infrastructure -s ./backend/src/AnimalAllies.Web
dotnet ef database update -c WriteDbContext -p ./backend/src/VolunteerRequests/VolunteerRequests.Infrastructure -s ./backend/src/AnimalAllies.Web
dotnet ef database update -c WriteDbContext -p ./backend/src/Discussion/Discussion.Infrastructure -s ./backend/src/AnimalAllies.Web
dotnet ef database update -c OutboxContext -p ./backend/src/Outbox/Outbox -s ./backend/src/AnimalAllies.Web
dotnet ef database update -c ApplicationDbContext -p ./TelegramBotService/src/TelegramBotService -s ./TelegramBotService/src/TelegramBotService
dotnet ef database update -c ApplicationDbContext -p ./NotificationService/src/NotificationService -s ./NotificationService/src/NotificationService
