
from django.urls import path
from .views import SimulationParametersList, SimulationParametersDetail


urlpatterns = [
    path("api/scenario_generator/simulationparameters/", SimulationParametersList.as_view()),
    path("api/scenario_generator/simulationparameters/<int:pk>/", SimulationParametersDetail.as_view()),
]
