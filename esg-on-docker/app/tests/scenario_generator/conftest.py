import pytest
from scenario_generator.models import SimulationParameters


@pytest.fixture(scope='function')
def add_simulationparameters():
    def _add_simulationparameters(name, headers, corr, s0, ar, mu, sigma):
        params = SimulationParameters.objects.create(
            name=name,
            headers=headers,
            corr=corr,
            s0=s0,
            ar=ar,
            mu=mu,
            sigma=sigma
        )
        return params
    return _add_simulationparameters
