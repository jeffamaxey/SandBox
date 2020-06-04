import logging

import json
import azure.functions as func
import numpy as np
import pandas as pd
from .esg import EconomicScenarioGenerator 


"""
API
$ curl -X POST http://127.0.0.1:8000/api/scenario_generator/simulationparameters/ -H 'Content-Type: application/json' -d '{"corr":"[[1.0, 0.032], [0.032, 1.0]]","headers":"['AK_SV', 'AK_WORLD']","s0":"[393.0, 28.5]","ar":"[0.0,1.0]","mu":"[0.2, 0.4]","sigma":"[0.16,0.1]","name":"esg"}'

$ curl -X POST http://127.0.0.1:8000/scenario_generator/timeseries/ -H 'Content-Type: application/json'
-d @./returns.json
"""

def main(req: func.HttpRequest) -> func.HttpResponse:
    logging.info('Python HTTP trigger function processed a request.')

    try:
        req_body = req.get_json()
    except ValueError:
        pass
    else:
        name = req_body.get('name')
        headers = req_body.get('headers')
        corr = np.array(json.loads(req_body.get('corr')))
        s0 = np.array(json.loads(req_body.get('s0')))
        ar = np.array(json.loads(req_body.get('ar')))
        mu = np.array(json.loads(req_body.get('mu')))
        sigma = np.array(json.loads(req_body.get('sigma')))

    N,t_steps, partition = 1, 12, 12
    ESG = EconomicScenarioGenerator(s0, ar, mu, sigma, corr)
    S, R = ESG.get_scenarios(N,t_steps, partition)
    res = np.append(S, R, axis=1)
    df = pd.DataFrame(res[0,:,:].T, columns=['S', 'R'])

    if not df.empty:
        return func.HttpResponse(f"szenarios = {df.to_json()}")
    else:
        return func.HttpResponse(
             "Please pass a integer n on the query string or in the request body",
             status_code=400
        )
