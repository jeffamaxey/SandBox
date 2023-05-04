import pytest
from scenario_generator.models import SimulationParameters


@pytest.mark.django_db
def test_add_simulationparameters(client):
    params = SimulationParameters.objects.all()
    assert len(params) == 0

    resp = client.post(
        "/api/scenario_generator/simulationparameters/",
        {
            "name": "alm18",
            "headers": "['AK_SV', 'AK_WORLD', 'AK_WORLD_LOC']",
            "corr": "[[1.0,0.75247927,0.82241407],[0.7524792753,1.0,0.7562530243],[0.8224140753,0.7562530243,1.0]]",
            "s0": "[224.48,4041.233665,591.7445716]",
            "ar": "[0.0,0.0,0.0]",
            "mu": "[0.0942865035,0.0838593127,0.07242036]",
            "sigma": "[0.1605389858,0.1117554048,0.127642938]"
        },
        content_type="application/json"
    )
    assert resp.status_code == 201
    assert resp.data["name"] == "alm18"

    params = SimulationParameters.objects.all()
    assert len(params) == 1


@pytest.mark.django_db
def test_add_simulationparameters_invalid_json(client):
    params = SimulationParameters.objects.all()
    assert len(params) == 0

    resp = client.post("/api/scenario_generator/simulationparameters/", {}, content_type="application/json")
    assert resp.status_code == 400

    params = SimulationParameters.objects.all()
    assert len(params) == 0


@pytest.mark.django_db
def test_add_simulationparameters_invalid_json_keys(client):
    params = SimulationParameters.objects.all()
    assert len(params) == 0

    resp = client.post(
        "/api/scenario_generator/simulationparameters/",
        {
            "corr": "[[0.048546344666584106, 0.03228643679647287], [0.03228643679647287, 0.10178422944914328]]",
            "headers": "['AK_SV', 'AK_WORLD']",
            "ar": "[0.0,0.0]",
            "mu": "[0.221777, 0.418426]",
            "sigma": "[0.1605389858,0.1117554048]"
        },
        content_type="application/json",
    )
    assert resp.status_code == 400

    params = SimulationParameters.objects.all()
    assert len(params) == 0


@pytest.mark.django_db
def test_get_single_simulationparameters(client, add_simulationparameters):
    params = add_simulationparameters(
        name="alm18",
        headers="['AK_SV', 'AK_WORLD', 'AK_WORLD_LOC']",
        corr="[[1.0,0.7524792753,0.8224140753],[0.7524792753,1.0,0.7562530243],[0.8224140753,0.7562530243,1.0]]",
        s0="[224.48,4041.233665,591.7445716]",
        ar="[0.0,0.0,0.0]",
        mu="[0.0942865035,0.0838593127,0.07242036]",
        sigma="[0.1605389858,0.1117554048,0.127642938]"
    )
    resp = client.get(f"/api/scenario_generator/simulationparameters/{params.id}/")
    assert resp.status_code == 200
    assert resp.data["name"] == "alm18"


def test_get_single_simulationparameters_incorrect_id(client):
    resp = client.get("/api/scenario_generator/simulationparameters/foo/")
    assert resp.status_code == 404


@pytest.mark.django_db
def test_remove_simulationparameters(client, add_simulationparameters):
    params = add_simulationparameters(
        name="alm18",
        headers="['AK_SV', 'AK_WORLD', 'AK_WORLD_LOC']",
        corr="[[1.0,0.7524792753,0.8224140753],[0.7524792753,1.0,0.7562530243],[0.8224140753,0.7562530243,1.0]]",
        s0="[224.48,4041.233665,591.7445716]",
        ar="[0.0,0.0,0.0]",
        mu="[0.0942865035,0.0838593127,0.07242036]",
        sigma="[0.1605389858,0.1117554048,0.127642938]"
    )

    resp = client.get(f"/api/scenario_generator/simulationparameters/{params.id}/")
    assert resp.status_code == 200
    assert resp.data["name"] == "alm18"

    resp_two = client.delete(f"/api/scenario_generator/simulationparameters/{params.id}/")
    assert resp_two.status_code == 204

    resp_three = client.get("/api/scenario_generator/simulationparameters/")
    assert resp_three.status_code == 200
    assert len(resp_three.data) == 0


@pytest.mark.django_db
def test_remove_simulationparameters_incorrect_id(client):
    resp = client.delete("/api/scenario_generator/simulationparameters/99/")
    assert resp.status_code == 404


@pytest.mark.django_db
def test_update_simulationparameters(client, add_simulationparameters):
    params = add_simulationparameters(
        name="alm18",
        headers="['AK_SV', 'AK_WORLD', 'AK_WORLD_LOC']",
        corr="[[1.0,0.7524792753,0.8224140753],[0.7524792753,1.0,0.7562530243],[0.8224140753,0.7562530243,1.0]]",
        s0="[224.48,4041.233665,591.7445716]",
        ar="[0.0,0.0,0.0]",
        mu="[0.0942865035,0.0838593127,0.07242036]",
        sigma="[0.1605389858,0.1117554048,0.127642938]"
    )

    resp = client.put(
        f"/api/scenario_generator/simulationparameters/{params.id}/",
        {
            "name": "alm18",
            "headers": "['AK_SV', 'AK_WORLD', 'AK_WORLD_LOC']",
            "corr": "[[1.0,0.75247927,0.82241407],[0.7524792753,1.0,0.7562530243],[0.8224140753,0.7562530243,1.0]]",
            "s0": "[224.48,4041.233665,591.7445716]",
            "ar": "[0.0,0.0,0.0]",
            "mu": "[0.0942865035,0.0838593127,0.07242036]",
            "sigma": "[0.1605389858,0.1117554048,0.127642938]"
        },
        content_type="application/json"
    )
    assert resp.status_code == 200
    assert resp.data["name"] == "alm18"
    assert resp.data["ar"] == "[0.0,0.0,0.0]"

    resp_two = client.get(f"/api/scenario_generator/simulationparameters/{params.id}/")
    assert resp_two.status_code == 200
    assert resp_two.data["name"] == "alm18"
    assert resp.data["ar"] == "[0.0,0.0,0.0]"


@pytest.mark.django_db
def test_update_simulationparameters_incorrect_id(client):
    resp = client.put("/api/scenario_generator/simulationparameters/99/")
    assert resp.status_code == 404


@pytest.mark.django_db
def test_update_simulationparameters_invalid_json(client, add_simulationparameters):
    params = add_simulationparameters(
        name="alm18",
        headers="['AK_SV', 'AK_WORLD', 'AK_WORLD_LOC']",
        corr="[[1.0,0.7524792753,0.8224140753],[0.7524792753,1.0,0.7562530243],[0.8224140753,0.7562530243,1.0]]",
        s0="[224.48,4041.233665,591.7445716]",
        ar="[0.0,0.0,0.0]",
        mu="[0.0942865035,0.0838593127,0.07242036]",
        sigma="[0.1605389858,0.1117554048,0.127642938]"
    )
    resp = client.put(
        f"/api/scenario_generator/simulationparameters/{params.id}/",
        {},
        content_type="application/json"
    )
    assert resp.status_code == 400


@pytest.mark.django_db
def test_update_simulationparameters_invalid_json_keys(client, add_simulationparameters):
    params = add_simulationparameters(
        name="alm18",
        headers="['AK_SV', 'AK_WORLD', 'AK_WORLD_LOC']",
        corr="[[1.0,0.7524792753,0.8224140753],[0.7524792753,1.0,0.7562530243],[0.8224140753,0.7562530243,1.0]]",
        s0="[224.48,4041.233665,591.7445716]",
        ar="[0.0,0.0,0.0]",
        mu="[0.0942865035,0.0838593127,0.07242036]",
        sigma="[0.1605389858,0.1117554048,0.127642938]"
    )

    resp = client.put(
        f"/api/scenario_generator/simulationparameters/{params.id}/",
        {
            "name": "alm18",
            "headers": "['AK_SV', 'AK_WORLD', 'AK_WORLD_LOC']",
            "corr": "[[1.0,0.75247927,0.82241407],[0.7524792753,1.0,0.7562530243],[0.8224140753,0.7562530243,1.0]]",
            "s0": "[224.48,4041.233665,591.7445716]",
            "ar": "[0.0,0.0,0.0]",
            "mu": "[0.0942865035,0.0838593127,0.07242036]"
        },
        content_type="application/json",
    )
    assert resp.status_code == 400


@pytest.mark.django_db
def test_generate_scenario(client):

    resp = client.post(
        "/api/scenario_generator/",
        {
            "name": "alm18",
            "headers": "['AK_SV', 'AK_WORLD']",
            "corr": "[[1.0, 0.82],[0.82, 1.0]]",
            "s0": "[22, 59]",
            "ar": "[0.0, 0.0]",
            "mu": "[0.09,0.07]",
            "sigma": "[0.16, 0.12]"
        },
        content_type="application/json"
    )
    assert resp.status_code == 201
    assert resp.data == "[[1.0, 0.82], [1.0, 0.82], [1.0, 0.82], [1.0, 0.82]]"
