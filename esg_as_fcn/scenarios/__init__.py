import logging

import json
import azure.functions as func
import numpy as np
import pandas as pd
from .esg import EconomicScenarioGenerator 


"""
API
$ curl -X POST http://localhost:7071/api/scenarios/ -H 'Content-Type: application/json' -d '{"corr":"[[1.0, 0.032], [0.032, 1.0]]","headers":"['AK_SV', 'AK_WORLD']","s0":"[393.0, 28.5]","ar":"[0.0,1.0]","mu":"[0.2, 0.4]","sigma":"[0.16,0.1]","name":"esg"}'

$ curl -X POST https://holmens.azurewebsites.net/api/scenarios/ -H 'Content-Type: application/json' -d @./parameters.json -o szenarios.json
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

    N,t_steps, partition = 10, 24, 12
    ESG = EconomicScenarioGenerator(s0, ar, mu, sigma, corr)
    S = ESG.get_scenarios(N,t_steps, partition)

    tmp = S.reshape(-1, S.shape[-1])
    df = pd.DataFrame(S.reshape(-1, S.shape[-1]), columns=['S1', 'S2', 'R'])
    df['N'] = np.nan
    df['t'] = np.nan
    df.N = df.index // t_steps
    df.t = df.index % t_steps

    return (
        func.HttpResponse(
            "Please pass a integer n on the query string or in the request body",
            status_code=400,
        )
        if df.empty
        else func.HttpResponse(df.to_csv(index=False))
    )
