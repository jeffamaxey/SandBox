import logging

import azure.functions as func
import json
import numpy as np
import pandas as pd
from ..noice import NoiceGenerator

"""
$ curl -X POST https://holmens.azurewebsites.net/api/brown?code=vtesm2s3CpWR3kJ7BGhRfJAauwn5qf%2FeeLmVP5PEuYq2Cz0ea3udVw%3D%3D -H 'Content-Type: application/json' -d '{"corr":"[[1.0, 0.032], [0.032, 1.0]]","n":10,"t":12}'
"""

def main(req: func.HttpRequest) -> func.HttpResponse:
    logging.info('Python HTTP trigger function processed a request.')

    try:
        req_body = req.get_json()
    except ValueError:
        pass
    else:
        corr = np.array(json.loads(req_body.get('corr')))
        N = req_body.get('n')
        T = req_body.get('t')

    # Brownian increments ~ N(0, corr^0.5)
    NG = NoiceGenerator()
    dB = NG.brown_steps(corr, N * T)
    
    # Index with Pandas
    df = pd.DataFrame(dB.T)
    df['N'] = np.nan
    df['t'] = np.nan
    df.N = df.index // T
    df.t = df.index % T

    if not df.empty:
        r = func.HttpResponse(df.to_csv(index=False))
        return func.HttpResponse(df.to_csv(index=False))
    else:
        return func.HttpResponse(
             status_code=400
        )
