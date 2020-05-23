import pytest
from scenario_generator.models import SimulationParameters


@pytest.mark.django_db
def test_esg_model():
    params = SimulationParameters(
        name="esg",
        headers="['SEK_SWAPC_5Y','SEK_SWAPC_15Y']",
        s0="[393.0777, 28.545]",
        ar="[3, 2]",
        mu="[0.221777, 0.418426]",
        sigma="[0.2, 0.4]",
        corr="[[0.048546344666584106, 0.03228643679647287], [0.03228643679647287, 0.10178422944914328]]"
    )
    params.save()
    assert params.name == "esg"
    assert params.headers == "['SEK_SWAPC_5Y','SEK_SWAPC_15Y']"
    assert params.s0 == "[393.0777, 28.545]"
    assert params.created
    assert params.updated
    assert str(params) == params.name
