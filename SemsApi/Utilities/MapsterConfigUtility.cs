using Mapster;
using SemsApi.DTO.EvaluationType;
using SemsApi.DTO.Facility;
using SemsApi.Models;

namespace SemsApi.Utilities
{
    public static class MapsterConfigUtility
    {
      public static void MappingConfig()
        {  
            var config = TypeAdapterConfig.GlobalSettings;

            config.NewConfig<CreateEvaluationTypeDto, EvaluationType>();
            config.NewConfig<EvaluationTypeDto, EvaluationType>();
            config.NewConfig<UpdateEvaluationTypeDto, EvaluationType>()
                .Ignore(e => e.EvaluationTypeId);

            config.NewConfig<CreateFacilityDto, Facility>();
            config.NewConfig<UpdateFacilityDto, Facility>()
                .Ignore(f => f.FacilityId);
            config.NewConfig<FacilityDto, Facility>();
        }
    }
}
