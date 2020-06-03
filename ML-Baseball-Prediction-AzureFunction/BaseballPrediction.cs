using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ML;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace ML_Baseball_PredictionAzureFunction
{
    public class BaseballPrediction
    {
        private readonly PredictionEnginePool<MLBBaseballBatter, MLBHOFPrediction> _predictionEnginePool;

        public BaseballPrediction(PredictionEnginePool<MLBBaseballBatter, MLBHOFPrediction> predictionEnginePool)
        {
            this._predictionEnginePool = predictionEnginePool;
        }

        [FunctionName("MakeBaseballPrediction")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            if (req.Method == "POST")
            {
                // Get parameters
                var modelAlgorithm = req.Query["ModelAlgorithm"];
                var predictionType = req.Query["PredictionType"];

                var requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                var machineLearningModelToUse = string.Empty;

                log.LogInformation($"ModelAlgorithm: {modelAlgorithm} || PREDICTIONTYPE: {predictionType}");
                machineLearningModelToUse = $"{predictionType}-{modelAlgorithm}";

                if (!String.IsNullOrEmpty(requestBody))
                {
                    var baseballBatters =
                    JsonConvert.DeserializeObject<List<MLBBaseballBatter>>(requestBody, new BooleanJsonConverter(typeof(MLBBaseballBatter)));

                    log.LogInformation("PROCESSING: {numberofPredictionsRequested} batter predictions, using {}", baseballBatters.Count, machineLearningModelToUse);

                    //Make Predictions
                    var predictionResults = new List<double>();
                    foreach (var batter in baseballBatters)
                    {
                        if (modelAlgorithm == "Ensemble")
                        {
                            var predictionFastTree = _predictionEnginePool.Predict($"{predictionType}-FastTree", batter);
                            var predictionLightGbm = _predictionEnginePool.Predict($"{predictionType}-LightGbm", batter);
                            var predictionGeneralizedAdditiveModels = _predictionEnginePool.Predict($"{predictionType}-GeneralizedAdditiveModels", batter);

                            // Simple ensemble with equal voting weights
                            var ensemblePrediction = (predictionFastTree.Probability + predictionLightGbm.Probability + predictionGeneralizedAdditiveModels.Probability) / 3;

                            predictionResults.Add(Math.Round(ensemblePrediction, 5, MidpointRounding.AwayFromZero));
                        }
                        else
                        {
                            var prediction = _predictionEnginePool.Predict(machineLearningModelToUse, batter);
                            predictionResults.Add(Math.Round(prediction.Probability, 5, MidpointRounding.AwayFromZero));
                        }
                    }

                    log.LogInformation("PROCESSED:  {} batter predictions, using {}", baseballBatters.Count, machineLearningModelToUse);

                    var results = JsonConvert.SerializeObject(predictionResults);

                    return new OkObjectResult(results);
                }
            }

            if (req.Method == "GET")
            {
                return new OkObjectResult("ML-Baseball Function is operating");
            }

            return new OkObjectResult("Received request, but incorrect Header/Body combination.");
        }
    }
}
