# Economic Scenario Generator on Docker

Dockerizing Django REST, Postgres, Gunicorn, and Nginx  
Using
https://testdriven.io/blog/dockerizing-django-with-postgres-gunicorn-and-nginx/
https://testdriven.io/courses/tdd-django/
https://testdriven.io/blog/django-debugging-vs-code/

    Development  
    $ docker-compose up -d --build

    Production  
    $ docker-compose -f docker-compose.prod.yml up -d --build
    $ docker-compose -f docker-compose.prod.yml exec web python manage.py migrate --noinput
    $ docker-compose -f docker-compose.prod.yml exec web python manage.py collectstatic --no-input --clear
