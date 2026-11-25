using Mapster;
using SemsApi.DTO.EvaluationType;
using SemsApi.Models;

namespace SemsApi.Utilities
{
    public static class MapsterConfigUtility
    {
      public static void MappingConfig()
        {  
            var config = TypeAdapterConfig.GlobalSettings;

            config.NewConfig<CreateEvaluationTypeDto, EvaluationType>();
            config.NewConfig<GetEvaluationTypeDto, EvaluationType>();
            config.NewConfig<UpdateEvaluationTypeDto, EvaluationType>();
        }
    }
}
