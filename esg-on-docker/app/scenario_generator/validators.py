from django.core.exceptions import ValidationError
import numpy as np
import json


def validate_symmetric(a):
    A = np.array(json.loads(a))
    if not np.allclose(A, A.T, rtol=1e-05, atol=1e-08):
        raise ValidationError('corr is not symmetric')
