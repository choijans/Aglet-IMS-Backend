using Aglet_Backend.DTO;
using Aglet_Backend.Models;
using AutoMapper; 

namespace Aglet_Backend.Profiles 
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Supplier, SupplierDto>().ReverseMap();
            CreateMap<Shoe, ShoeDto>().ReverseMap();
            CreateMap<StockTransmission, StockTransmissionDto>().ReverseMap();
            CreateMap<PurchaseRecord, PurchaseRecordDto>().ReverseMap();
        }
    }
}