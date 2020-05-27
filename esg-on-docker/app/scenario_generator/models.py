from django.contrib.auth.models import AbstractUser
from django.db import models
from .validators import validate_symmetric


class CustomUser(AbstractUser):
    pass


class SimulationParameters(models.Model):
    created = models.DateTimeField(auto_now_add=True)
    updated = models.DateTimeField(auto_now=True)
    name = models.CharField(max_length=50, blank=True, default='null')
    headers = models.CharField(max_length=1000, default='null')
    s0 = models.CharField(max_length=1000)  # Array Initial values
    ar = models.CharField(max_length=1000, default='null')  # Speed of reversion in Vasicek
    mu = models.CharField(max_length=1000)  # Array Mean returns (yearly)
    sigma = models.CharField(max_length=1000)  # Array Mean returns (yearly)
    corr = models.CharField(max_length=10000, validators=[validate_symmetric])  # Array Correlation

    def __str__(self):
        return f"{self.name}"


class TimeSeries(models.Model):
    created = models.DateTimeField(auto_now_add=True)
    updated = models.DateTimeField(auto_now=True)
    name = models.CharField(max_length=50, blank=True, default='null')
    headers = models.CharField(max_length=1000, blank=True, default='null')
    ts = models.CharField(max_length=1000000)  # Array Initial values

    def __str__(self):
        return f"{self.name}"
