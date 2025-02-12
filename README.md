# Прод проекта
- Миниприложение в разработке
- Первой прописать команду "docker plugin install grafana/loki-docker-driver:latest"
- Потом уже запускать через docker-compose up -d --build


# Сервисы: 
1. ClientAPI URL: http://localhost:8080/ui-swagger 
2. CourierAPI URL: http://localhost:8081/ui-swagger 
3. RestaurantAPI URL: http://localhost:8082/ui-swagger 
4. PaymentAPI URL: http://localhost:8083/ui-swagger 
5. Frontend URL: http://localhost:4001
6. Grafana URL: http://localhost:3000
7. RabbitMQ URL: http://localhost:15672
8. PgAdmin4 URL: http://localhost:5050

# Дополнительная информация

- На каждом микросервисе реализован редирект из корня "/" к "/ui-swagger"
  
- JWT токены подписаны RS512 с публичным и приватным ключом RSA

- Данные от PgAdmin4<br>
 Логин: qwerty11ert@gmail.com<br>
 Пароль: root<br>

- PostgreSQL добавление сервера в PgAdmin4<br>
 Имя сервера: postgres_db<br>
 Служебная база данных: simbirfood<br>
 Пользователь: practice_user<br>
 Пароль: root<br>

- Данные от RabbitMQ<br>
 Логин: guest<br>
 Пароль: guest<br>

- Данные от Grafana<br>
 Логин: admin<br>
 Пароль: admin<br>

- Все переменные окружения в .env файле, напишите мне в телегу я кину