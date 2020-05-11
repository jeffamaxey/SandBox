# SandBox
Where it all begins


- mats-site\
Dockerizing Django with Postgres, Gunicorn, and Nginx  
Using https://github.com/testdrivenio/django-on-docker.git

    Development  
    $ docker-compose up -d --build

    Production  
    $ docker-compose -f docker-compose.prod.yml up -d --build  
    $ docker-compose -f docker-compose.prod.yml exec web python manage.py migrate --noinput


- django-tdd-docker\
Test-Driven Development with Django, Django REST Framework, and Docker

