using System;
using System.IO;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.ML;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Reflection;

[assembly: FunctionsStartup(typeof(ML_Baseball_PredictionAzureFunction.Startup))]

namespace ML_Baseball_PredictionAzureFunction
{
    public class Startup : FunctionsStartup
    {
        private readonly string _environment;
        private readonly string _modelPathInducted, _modelPathOnBallot,
            _modelPathInductedFastTree, _modelPathOnBallotFastTree,
            _modelPathInductedLightGbm, _modelPathOnBallotLightGbm;

        public Startup()
        {
            _environment = Environment.GetEnvironmentVariable("AZURE_FUNCTIONS_ENVIRONMENT");

            var binDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var rootDirectory = Path.GetFullPath(Path.Combine(binDirectory, ".."));
            _modelPathInducted = Path.GetFullPath(Path.Combine(rootDirectory, "Models", "InductedToHallOfFame-GeneralizedAdditiveModels.mlnet"));
            _modelPathOnBallot = Path.GetFullPath(Path.Combine(rootDirectory, "Models", "OnHallOfFameBallot-GeneralizedAdditiveModels.mlnet"));
            _modelPathInductedFastTree = Path.GetFullPath(Path.Combine(rootDirectory, "Models", "InductedToHallOfFame-FastTree.mlnet"));
            _modelPathOnBallotFastTree = Path.GetFullPath(Path.Combine(rootDirectory, "Models", "OnHallOfFameBallot-FastTree.mlnet"));
            _modelPathInductedLightGbm = Path.GetFullPath(Path.Combine(rootDirectory, "Models", "InductedToHallOfFame-LightGbm.mlnet"));
            _modelPathOnBallotLightGbm = Path.GetFullPath(Path.Combine(rootDirectory, "Models", "OnHallOfFameBallot-LightGbm.mlnet"));

            //if (_environment == "Development")
            //{
            //    _modelPath = Path.Combine("Models", "InductedToHallOfFame-GeneralizedAdditiveModels.mlnet");
            //}
            //else
            //{
            //    string deploymentPath = @"D:\home\site\wwwroot\";
            //    _modelPath = Path.Combine(deploymentPath, "Models", "InductedToHallOfFame-GeneralizedAdditiveModels.mlnet");
            //}
        }

        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddPredictionEnginePool<MLBBaseballBatter, MLBHOFPrediction>()
                .FromFile(modelName: "InductedToHallOfFame-GeneralizedAdditiveModels", filePath: _modelPathInducted, watchForChanges: true)
                .FromFile(modelName: "OnHallOfFameBallot-GeneralizedAdditiveModels", filePath: _modelPathOnBallot, watchForChanges: true)
                .FromFile(modelName: "InductedToHallOfFame-FastTree", filePath: _modelPathInductedFastTree, watchForChanges: true)
                .FromFile(modelName: "OnHallOfFameBallot-FastTree", filePath: _modelPathOnBallotFastTree, watchForChanges: true)
                .FromFile(modelName: "InductedToHallOfFame-LightGbm", filePath: _modelPathInductedLightGbm, watchForChanges: true)
                .FromFile(modelName: "OnHallOfFameBallot-LightGbm", filePath: _modelPathOnBallotLightGbm, watchForChanges: true);
        }
    }
}
