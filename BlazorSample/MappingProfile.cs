using AutoMapper;
using CellarPassAppAdmin.Shared.Models;
using CellarPassAppAdmin.Shared.Models.RequestModel;
using CellarPassAppAdmin.Shared.Models.RequestModel.SA;

namespace CellarPassAppAdmin.Client
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<POSProfileFullModel, POSProfileRequestModel>();
            CreateMap<CPNotificationModel, CPNotificationRequestModel>();
            CreateMap<CPPressReleaseModel, CPPressReleaseRequestModel>();
            CreateMap<CPBlogArticleDetailModel, BlogArticleRequestModel>();
            CreateMap<CPBlogArticleBlockRequestModel, TextBlockRequestModel>();
            CreateMap<CPBlogArticleBlockRequestModel, MemberBlockRequestModel>();
            CreateMap<OrderDetailFullModel, OrderPDFRequestModel>();
            CreateMap<BusinessClubModel, BusinessClubRequestModel>();
            CreateMap<BusinessClubModel, UpdateClubRequestModel>();
            CreateMap<BusinessClubRequestModel, UpdateClubRequestModel>()
                .ForMember(x => x.ClubShippingOptions, opt => opt.Ignore())
                .ForMember(x => x.ClubGiftShipments, opt => opt.Ignore())
                .ForMember(x => x.ClubFrequencies, opt => opt.Ignore())
                .ForMember(x => x.ClubCancelReasons, opt => opt.Ignore())
                .ForMember(x => x.ClubShipInventoryLocation, opt => opt.Ignore())
                .ForMember(x => x.ClubPickupInventoryLocation, opt => opt.Ignore());
            CreateMap<ClubShipmentDetailFullModel, UpdateShipmentRequestModel>();
            CreateMap<ClubShipmentItemModel, ProductItemsDetail>().ReverseMap();
            CreateMap<ClubShipmentDocumentsModel, DocumentsDetail>();
            CreateMap<DiscountDetailModel, BusinessDiscountRequestModel>();
            CreateMap<DiscountGroupDetailModel, BusinessDiscountGroupRequestModel>();
            CreateMap<AddCPNotificationEmailRequestModel, UpdateCPNotificationEmailRequestModel>();
            CreateMap<AddCPNotificationEmailVariableRequestModel, UpdateCPNotificationEmailVariableRequestModel>();
            CreateMap<EmailNotificationDetailModel, AddCPNotificationEmailRequestModel>();
            CreateMap<NotificationEmailVariableModel, AddCPNotificationEmailVariableRequestModel>();
            CreateMap<TicketingPlanModel, TicketingPlanRequestModel>();
        }
    }
}
