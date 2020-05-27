from scenario_generator.serializers import SimulationParametersSerializer


def test_valid_simulationparameters_serializer():
    valid_serializer_data = {
        "name": "esg",
        "headers": "['SEK_SWAPC_5Y','SEK_SWAPC_15Y']",
        "s0": "[393.0777, 28.545]",
        "ar": "[3, 2]",
        "mu": "[0.221777, 0.418426]",
        "sigma": "[0.2, 0.4]",
        "corr": "[[0.048546344666584106, 0.03228643679647287], [0.03228643679647287, 0.10178422944914328]]"
    }
    serializer = SimulationParametersSerializer(data=valid_serializer_data)
    assert serializer.is_valid()
    assert serializer.validated_data == valid_serializer_data
    assert serializer.data == valid_serializer_data
    assert serializer.errors == {}


def test_invalid_corr_simulationparameters_serializer():
    invalid_serializer_data = {
        "name": "esg",
        "headers": "['SEK_SWAPC_5Y','SEK_SWAPC_15Y']",
        "s0": "[393.0777, 28.545]",
        "ar": "[3, 2]",
        "mu": "[0.221777, 0.418426]",
        "sigma": "[0.2, 0.4]",
        "corr": "[[1.0, 1.2], [2.1, 1.0]]"
    }
    serializer = SimulationParametersSerializer(data=invalid_serializer_data)
    assert not serializer.is_valid()
    assert serializer.errors["corr"][0] == 'corr is not symmetric'
