from .models import SimulationParameters
from .serializers import SimulationParametersSerializer
import numpy as np
import json

from django.http import Http404
from rest_framework.views import APIView
from rest_framework.response import Response
from rest_framework import status

"""
API
$ curl -X POST http://127.0.0.1:8000/api/scenario_generator/simulationparameters/ -H 'Content-Type: application/json'
 -d '{"corr":"[[0.048546344666584106, 0.03228643679647287], [0.03228643679647287, 0.10178422944914328]]",
 "headers":"['AK_SV', 'AK_WORLD']","s0":"[393.0777, 28.545]","ar":"[0.0,0.0]","mu":"[0.221777, 0.418426]",
 "sigma":"[0.1605389858,0.1117554048]","name":"esg"}'
$ curl -X POST http://127.0.0.1:8000/scenario_generator/timeseries/ -H 'Content-Type: application/json'
-d @./returns.json
"""


class SimulationParametersList(APIView):
    """
    List all snippets, or create a new snippet.
    """

    def get(self, request, format=None):
        simulationparameters = SimulationParameters.objects.all()
        serializer = SimulationParametersSerializer(simulationparameters, many=True)
        return Response(serializer.data)

    def post(self, request, format=None):
        serializer = SimulationParametersSerializer(data=request.data)
        if serializer.is_valid():
            serializer.save()
            return Response(serializer.data, status=status.HTTP_201_CREATED)
        return Response(serializer.errors, status=status.HTTP_400_BAD_REQUEST)


class SimulationParametersDetail(APIView):
    """
    Retrieve, update or delete a snippet instance.
    """

    def get_object(self, pk):
        try:
            return SimulationParameters.objects.get(pk=pk)
        except SimulationParameters.DoesNotExist:
            raise Http404

    def get(self, request, pk, format=None):
        simulationparameter = self.get_object(pk)
        serializer = SimulationParametersSerializer(simulationparameter)
        return Response(serializer.data)

    def put(self, request, pk, format=None):
        simulationparameter = self.get_object(pk)
        serializer = SimulationParametersSerializer(simulationparameter, data=request.data)
        if serializer.is_valid():
            serializer.save()
            return Response(serializer.data)
        return Response(serializer.errors, status=status.HTTP_400_BAD_REQUEST)

    def delete(self, request, pk, format=None):
        simulationparameter = self.get_object(pk)
        simulationparameter.delete()
        return Response(status=status.HTTP_204_NO_CONTENT)


class Scenarios(APIView):

    def get(self, request, format=None):
        simulationparameters = SimulationParameters.objects.all()
        serializer = SimulationParametersSerializer(simulationparameters, many=True)
        return Response(serializer.data)

    def post(self, request, format=None):
        serializer = SimulationParametersSerializer(data=request.data)
        if serializer.is_valid():

            C = np.array(json.loads(serializer.data["corr"]))
            A = np.zeros((4, 2))
            A[:, 0] = C[0, 0]
            A[:, 1] = C[0, 1]
            alist = A.tolist()
            json_str = json.dumps(alist)

            return Response(json_str, status=status.HTTP_201_CREATED)
        return Response(serializer.errors, status=status.HTTP_400_BAD_REQUEST)
