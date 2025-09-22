# OutdoorPlanner
OutdoorPlanner - это интеллектуальный планировщик мероприятий на открытом воздухе, который помогает организовать события с учетом погодных условий. Приложение автоматически проверяет прогноз погоды, отправляет уведомления и управляет расписанием мероприятий.

Предварительные требования
1) .NET 6.0 SDK
2) PostgreSQL 12+
3) Аккаунт OpenWeatherMap (бесплатный тариф)
4) SMTP сервер для отправки email (Gmail, Yandex, etc.)

В папке bin проекта необходимо создать файл appsettings.json вида:
{
  "ConnectionStrings": {
    "DefaultConnection": "ваша_строка_подключения_к_БД"
  },
  "OpenWeather": {
    "ApiKey": "ваш_api_ключ"
  },
  "EmailSettings": {
    "SmtpServer": "ваш_smtp_сервер",
    "SmtpPort": ваш_smtp_порт,
    "EmailFrom": "почта",
    "EmailPassword": "ваш_пароль_для_внешних_приложений"
  }
}
