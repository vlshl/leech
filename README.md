# Leech

Компонент Leech системы биржевой торговли отвечает за взаимодействие с торговым терминалом AlorTrade. Торговый терминал  устанавливается в систему Windows и регистрируется как COM-сервер. Компонент Leech взаимодействует с терминалом при помощи механизма COM-объектов.

## Функциональность

- Управление торговым терминалом по расписанию (запуск терминала, соединение терминала с сервером брокера перед началом торговой сессии, отключение терминала от сервера брокера после окончания торговой сессии, завершение работы торгового терминала).

- Получение от торгового терминала информации о котировках  в режиме реального времени, накопление полученных данных в собственной базе данных.

- Получение от терминала информации по торговому счету в реальном времени - совершенные сделки, создание и изменения заявок, изменение состояния портфеля. Сохранение этой информации в собственной базе данных.

- Взаимодействие с сервером Pulxer через механизм WebSocket. Компонент Leech автоматически подключается в серверу Pulxer. Взаимодействие происходит по протоколу LeechPipe. Это бинарный протокол уровня приложения, который работает поверх WebSocket-соединения и обеспечивает многоканальное взаимодействие между сторонами.

- Предоставление информации о текущих котировках (цены последних сделок по финансовым инструментам) в режиме реального времени. Также предоставление информации по событиям торгового счета (заявки, стоп-заявки, сделки, текущее состояние портфеля).

- Получение от компонента Pulxer команд, их выполнение и выдача результата.

- Обслуживание собственной базы данных (удаление устаревшей информации).

- Предоставление исторических данных по котировкам в тиковом формате (информация по каждой сделке в торговой сессии на указанную дату по указанному инструменту).

- Возможность работы биржевых роботов. Компонент Leech является автономной платформой для работы биржевых роботов.

## Реализация

Компонент Leech реализован в виде Windows-сервиса, устанавливается в систему и настраивается на автоматический запуск. Компонент имеет конфигурационный файл. Также возможен вариант запуска компонента в виде CLI-приложения и работа с ним через консоль.
