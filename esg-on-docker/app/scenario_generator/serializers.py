from scenario_generator.models import SimulationParameters, TimeSeries
from rest_framework import serializers


class SimulationParametersSerializer(serializers.HyperlinkedModelSerializer):
    class Meta:
        model = SimulationParameters
        fields = ['id', 'name', 'headers', 's0', 'ar', 'mu', 'sigma', 'corr']


class TimeSeriesSerializer(serializers.HyperlinkedModelSerializer):
    class Meta:
        model = TimeSeries
        fields = ['id', 'name', 'headers', 'ts']
