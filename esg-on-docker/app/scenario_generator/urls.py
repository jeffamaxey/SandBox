
from django.urls import path
from .views import SimulationParametersList, SimulationParametersDetail, Scenarios


urlpatterns = [
    path("api/scenario_generator/simulationparameters/", SimulationParametersList.as_view()),
    path("api/scenario_generator/simulationparameters/<int:pk>/", SimulationParametersDetail.as_view()),
    path("api/scenario_generator/", Scenarios.as_view()),
]
