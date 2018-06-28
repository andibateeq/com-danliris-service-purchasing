﻿using Com.DanLiris.Service.Purchasing.Lib.Models.DeliveryOrderModel;
using Com.DanLiris.Service.Purchasing.Lib.Utilities;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.DeliveryOrderViewModel;

namespace Com.DanLiris.Service.Purchasing.Lib.AutoMapperProfiles
{
    public class DeliveryOrderProfile : BaseAutoMapperProfile
    {
        public DeliveryOrderProfile()
        {
            CreateMap<DeliveryOrder, DeliveryOrderViewModel>()
                .ForMember(d => d.no, opt => opt.MapFrom(s => s.DONo))
                .ForMember(d => d.supplierDoDate, opt => opt.MapFrom(s => s.DODate))
                .ForMember(d => d.date, opt => opt.MapFrom(s => s.ArrivalDate))

                /*Supplier*/
                .ForPath(d => d.supplier._id, opt => opt.MapFrom(s => s.SupplierId))
                .ForPath(d => d.supplier.code, opt => opt.MapFrom(s => s.SupplierCode))
                .ForPath(d => d.supplier.name, opt => opt.MapFrom(s => s.SupplierName))

                .ForMember(d => d.items, opt => opt.MapFrom(s => s.Items))
                .ReverseMap();

            CreateMap<DeliveryOrderItem, DeliveryOrderItemViewModel>()
                .ForPath(d => d.purchaseOrderExternal._id, opt => opt.MapFrom(s => s.EPOId))
                .ForPath(d => d.purchaseOrderExternal.no, opt => opt.MapFrom(s => s.EPONo))
                .ForMember(d => d.fulfillments, opt => opt.MapFrom(s => s.Details))
                .ReverseMap();

            CreateMap<DeliveryOrderDetail, DeliveryOrderFulFillMentViewModel>()
                .ForPath(d => d.purchaseOrder.purchaseRequest._id, opt => opt.MapFrom(s => s.PRId))
                .ForPath(d => d.purchaseOrder.purchaseRequest.no, opt => opt.MapFrom(s => s.PRNo))
                /*Product*/
                .ForPath(d => d.product._id, opt => opt.MapFrom(s => s.ProductId))
                .ForPath(d => d.product.code, opt => opt.MapFrom(s => s.ProductCode))
                .ForPath(d => d.product.name, opt => opt.MapFrom(s => s.ProductName))
                .ForPath(d => d.remark, opt => opt.MapFrom(s => s.ProductRemark))
                .ForPath(d => d.deliveredQuantity, opt => opt.MapFrom(s => s.DOQuantity))
                .ForPath(d => d.purchaseOrderQuantity, opt => opt.MapFrom(s => s.DealQuantity))
                /*UOM*/
                .ForPath(d => d.purchaseOrderUom._id, opt => opt.MapFrom(s => s.UomId))
                .ForPath(d => d.purchaseOrderUom.unit, opt => opt.MapFrom(s => s.UomUnit))
                .ReverseMap();
        }
    }
}